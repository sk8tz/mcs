/*

Classes

	CompareInfo : implements convenience overrides.
	CompareInfoImpl : dispatches core methods to Collator

	SortKeyBuffer : stores on-building sortkey binary data.
	CharacterIterator
		iterates character elements.
		Has string, collator, CompareOptions.
	StringIterator
		CharacterIterator implementation that iterates a string.
	SingleCharacterIterator
		CharacterIterator implementation that iterates a char.
		Used in IndexOf(string,char..) and LastIndexOf(string,char..).
	Collator
		supports collation.
		Contains primary purposes: GetSortKey(), Compare(), IndexOf(),
		LastIndexOf(), IsPrefix(), IsSuffix().

		Has CultureInfo, customCjkKeys, reverseAccent,
		contraction mapping data, expansion mapping data.

		... and more.

*/

using System;
using System.IO;
using System.Globalization;


namespace Mono.Globalization.Unicode
{
	// Internal sort key storage that is reused during GetSortKey.
	internal class SortKeyBuffer
	{
		int l1, l2, l3, l4, l5;
		byte [] l1b, l2b, l3b, l4b, l5b;
		int level5LastPos;

		public SortKeyBuffer ()
		{
		}

		public void Reset ()
		{
			l1 = l2 = l3 = l4 = l5 = 0;
			level5LastPos = 0;
		}

		// It is used for CultureInfo.ClearCachedData().
		internal void ClearBuffer ()
		{
			l1b = l2b = l3b = l4b = l5b = null;
		}

		internal void AdjustBufferSize (string s)
		{
			if (l1b == null || l1b.Length < s.Length)
				l1b = new byte [s.Length * 2 + 10];
			if (l2b == null || l2b.Length < s.Length)
				l2b = new byte [s.Length + 10];
			if (l3b == null || l3b.Length < s.Length)
				l3b = new byte [s.Length + 10];
			// This weight is used only in Japanese text.
			// We could expand the initial length as well as
			// primary length (actually x3), but even in ja-JP
			// most of the compared strings won't be Japanese.
			if (l4b == null)
				l5b = new byte [10];
			if (l5b == null)
				l5b = new byte [10];
		}

		// Append non-variable character.
		internal void AppendNormal (byte [] table, int idx1, int idx2, int idx3, int idx4)
		{
			// idx1-4 points to CollationTable indexes
			while (table [idx1] != 0)
				AppendBufferPrimitive (table [idx1++], ref l1b, ref l1);
			while (table [idx2] != 0)
				AppendBufferPrimitive (table [idx2++], ref l2b, ref l2);
			while (table [idx3] != 0)
				AppendBufferPrimitive (table [idx3++], ref l3b, ref l3);
			// level 4 weight is added only for Japanese kana.
			if (idx4 > 0)
				while (table [idx4] != 0)
					AppendBufferPrimitive (table [idx4++],
						ref l4b, ref l4);
		}

		// Append variable-weight character.
		internal void AppendLevel5 (byte [] table, int idx, int currentIndex)
		{
			// offset
			int offsetValue = currentIndex - level5LastPos;
			for (; offsetValue > 8064; offsetValue -= 8064)
				AppendBufferPrimitive (0xFF, ref l5b, ref l5);
			if (offsetValue > 63)
				AppendBufferPrimitive ((byte) (offsetValue - 63 / 4 + 0x80), ref l5b, ref l5);
			AppendBufferPrimitive ((byte) (offsetValue % 63), ref l5b, ref l5);

			level5LastPos = currentIndex;

			// sortkey value
			idx++; // skip the "variable" mark: 01
			while (table [idx] != 0)
				AppendBufferPrimitive (table [idx++], ref l5b, ref l5);
		}

		private void AppendBufferPrimitive (byte value, ref byte [] buf, ref int bidx)
		{
			buf [bidx++] = value;
			if (bidx == buf.Length) {
				byte [] tmp = new byte [bidx * 2];
				Array.Copy (buf, tmp, buf.Length);
				buf = tmp;
			}
		}

		public byte [] GetResultAndReset ()
		{
			byte [] ret = GetResult ();
			Reset ();
			return ret;
		}

		// For level2-5, 02 is the default and could be cut (implied).
		// 02 02 02 -> 0
		// 02 03 02 -> 2
		// 03 04 05 -> 3
		private int GetOptimizedLength (byte [] data, int len)
		{
			int cur = -1;
			for (int i = 0; i < len; i++)
				if (data [i] != 2)
					cur = i;
			return cur + 1;
		}

		public byte [] GetResult ()
		{
			l2 = GetOptimizedLength (l2b, l2);
			l3 = GetOptimizedLength (l3b, l3);
			l4 = GetOptimizedLength (l4b, l4);
			l5 = GetOptimizedLength (l5b, l5);

			int length = l1 + l2 + l3 + l4 + l5 + 5;

			byte [] ret = new byte [length];
			Array.Copy (l1b, ret, l1);
			ret [l1] = 1; // end-of-level mark
			int cur = l1 + 1;
			if (l2 > 0)
				Array.Copy (l2b, 0, ret, cur, l2);
			cur += l2;
			ret [cur++] = 1; // end-of-level mark
			if (l3 > 0)
				Array.Copy (l3b, 0, ret, cur, l3);
			cur += l3;
			ret [cur++] = 1; // end-of-level mark
			if (l4 > 0)
				Array.Copy (l4b, 0, ret, cur, l4);
			cur += l4;
			ret [cur++] = 1; // end-of-level mark
			if (l5 > 0)
				Array.Copy (l5b, 0, ret, cur, l5);
			cur += l5;
			ret [cur++] = 0; // end-of-data mark
			return ret;
		}
	}

	internal abstract class CharacterIterator
	{
		protected CharacterIterator (CompareOptions opt, Collator coll)
		{
			Options = opt;
			Collator = coll;
		}

		protected readonly CompareOptions Options;
		protected readonly Collator Collator;

		// MinValue: does not have a valid value
		protected char Value;

		// extensions are collected to char[] in both default table
		// and culture-dependent table.
		protected char [] ExtensionTable;
		protected int ExtensionStart; // -1 when no extension
		protected int ExtensionEnd;
		protected int ExtensionCurrent;

		public abstract bool MoveNext ();
		public abstract void Reset ();
	}

	internal class StringIterator : CharacterIterator
	{
		string s;
		int start, end;

		// index in s.
		public int Current;
		// current character element length in s.
		public int Length;

		public StringIterator (string s, int start, int end,
			CompareOptions opt, Collator coll)
			: base (opt, coll)
		{
			this.s = s;
			this.start = start;
			this.end = end;
		}

		public override void Reset ()
		{
			Current = start;
			Length = 0;
			Value = Next = '\0';
		}

		public override bool MoveNext ()
		{
			if (ExtensionStart > 0) {
				if (ExtensionCurrent++ < ExtensionEnd)
					return true;
				ExtensionStart = -1;
			}

			Current += Length;
			if (end <= Current) // actually '<' must not happen.
				return false;
			// mhm, it might be better to have it inside this type.
			Collator.MoveIteratorNext (this);
			return true;
		}

		public override bool MoveBack ()
		{
			if (ExtensionStart > 0) {
				if (ExtensionCurrent++ >= 0)
					return true;
				ExtensionStart = -1;
			}

			// we cannot adjust Current here but should do it
			// inside MoveIteratorBack().
			if (start >= Current) // actually '>' must not happen
				return false;
			// mhm, it might be better to have it inside this type.
			Collator.MoveIteratorBack (this);
			return true;
		}

		public void MoveTo (int i)
		{
			Reset ();
			Current = i;
			MoveNext ();
		}

		internal void SetProp (int len, char c, char exp)
		{
			Length = len;
			Value = c;
			Next = exp;
		}
	}

	internal class SingleCharacterIterator : CharacterIterator
	{
		char c;
		bool done;

		public SingleCharacterIterator (char c, CompareOptions opt, Collator coll)
			: base (opt, coll)
		{
			this.c = c;
		}

		public override void Reset ()
		{
			done = false;
		}

		public override bool MoveNext ()
		{
			if (done)
				return false;
			if (ExtensionStart > 0) {
				if (ExtensionCurrent++ < ExtensionEnd)
					return true;
				ExtensionStart = -1;
			}
			return Collator.MoveIteratorNext (this);
		}

		public override bool MoveBack ()
		{
			if (done)
				return false;
			if (ExtensionStart > 0) {
				if (ExtensionCurrent-- >= 0)
					return true;
				ExtensionStart = -1;
			}
			if (ExtensionStart > 0) {
				if (Extension

			return Collator.MoveIteratorBack (this);
		}
	}

	internal class Collator
	{
		static readonly Collator invariant;

		static Collator ()
		{
			invariant = new Collator (CultureInfo.InvariantCulture);
		}

		public static char [] DefaultContractions {
			get { return invariant.contractions; }
		}

		CultureInfo culture;

		//
		// Data layout:
		//	[c1][c2]...[0][i][0]
		// where c1 to c3 represent one contraction, and i represents 
		// corresponding index to (custom) sortkey table.
		//
		char [] contractions;

		//
		// Data layout:
		//	[x][c1][c2]...[0]
		// where x represents the target character and ... mhm, maybe
		// I had better aggregate them into current "contractions" field
		char [] expansions;
		//

		// this reference to byte[] could be altered depending on
		// the culture, namely ja, ko, zh-*.
		byte [] customCjkKeys;

		// True if the culture expects so-called French accent order.
		bool reverseAccent;

		// reused
		SortKeyBuffer buf = new SortKeyBuffer ();

		public Collator (CultureInfo ci)
		{
			culture = ci;
			StreamReader sr = new StreamReader (blahResourceStream);
			fill_charprops_and_sortkey_tailorings (ci);
		}

		public void ClearInternalBuffer ()
		{
			buf.ClearBuffer ();
		}

		// returns the level:
		//	0: no difference
		//	1: primary difference
		//	2: diacritical difference
		//	3: case difference
		//	4: kanatype difference
		//	5: variable weight difference
		public int CompareChar (CharacterIterator i1, CharacterIterator i2)
		{
		}

		// Get character element length of the argument character in s at cur.
		internal void MoveIteratorNext (CharacterIterator i)
		{
			// check contractions
			int ret = -1;
			char [] target = null;
			if (contractions != null) {
				ret = CheckContraction (contractions, i.Text, i.Current);
				if (ret >= 0)
					target = contractions;
			}
			if (ret < 0) {
				ret = CheckContraction (DefaultContractions, i.Text, i.Current);
				if (ret >= 0)
					target = DefaultContractions;
			}
			if (ret >= 0) {
				int len = 0;
				while (target [ret++] != 0)
					len++;
				i.SetProp (len, target [ret + len + 2], '\x0');
				return;
			}
			// check expansions

			i.SerProp (1, i.Text [i.Current], char.MinValue);
		}

		// returns -1 if no matching contraction.
		// Other than that, returns contraction index.
		private int CheckContraction (char [] contractions, string s, int cur)
		{
			for (int i = 0; i < contractions.Length;) {
				int x = cur;
				int cstart = i;
				if (contractions [i] == s [x]) {
					while (contractions [++i] != 0)
						if (++x < s.Length && s [x] != contractions [i])
							break;
					if (contractions [i] == 0)
						return cstart;
				}
				while (contractions [i] != 0)
					i++;
				i += 3; // [0][i][0] (see contractions layout)
			}
			return -1;
		}

		//
		// Below are normalization matters:
		// - IgnoreCase -> use culture dependent ToLower()
		// - IgnoreKanaType -> ToKanaTypeInsensitive().
		// - IgnoreNonSpace -> IsIgnorableNonSpacing().
		// - IgnoreSymbols -> IsIgnorableSymbol()
		// - IgnoreWidth -> ToWidthInsensitive()
		//
		// It is independently considered, maybe as a tailored
		// collation element table.
		// - StringSort
		//
		// Even with CompareOptions.None both of Japanese katakana
		// with voice mark "included" or "separate" are regarded as
		// equal, while half-width katakana is distinguished unless
		// IgnoreWidth is specified.
		// So maybe canonical normalization is done.
		//
		[Obsolete]
		internal static bool IsIgnorable (int i, CompareOptions opt, CultureInfo ci)
		{
			if (MSCompatUnicodeTable.IsIgnorable (i))
				return true;
			if ((opt & CompareOptions.IgnoreWidth) != 0)
				i = MSCompatUnicodeTable.ToWidthInsensitive (i);
			if ((opt & CompareOptions.IgnoreKanaType) != 0)
				i = MSCompatUnicodeTable.ToKanaTypeInsensitive (i);
			if ((opt & CompareOptions.IgnoreCase) != 0 && i <= char.MaxValue)
				i = ci.TextInfo.ToLower ((char) i);
			if ((opt & CompareOptions.IgnoreSymbols) != 0
				&& MSCompatUnicodeTable.IsIgnorableSymbol (i))
				return true;
			if ((opt & CompareOptions.IgnoreNonSpace) != 0
				&& MSCompatUnicodeTable.IsIgnorableNonSpacing (i))
				return true;
			return false;
		}

		#region IndexOf() and LastIndexOf()
		/*
<!--
		Create character iterator for both of the arguments (target 
		could be either a string or a char).
		Iterate the searchee until both of the head of the iterators
		match.
		Once they match, save the location of the searchee, and iterate
		through the end of the target. If there is a difference, then
		searchee.MoveTo(location+1) and target.Reset().
		If the target came to the end, then return the saved location.
-->

		From the beginning of the string to the end, run IsPrefix()
		and if it was true then return the_location.
		If no match, return -1.
		*/
		public int IndexOf (string s, char target, int start, int length, CompareOptions opt)
		{
			return IndexOf (new StringIterator (s, start, length), new SingleCharacterIterator (target), opt);
		}

		public int IndexOf (string s, string target, int start, int length, CompareOptions opt)
		{
			return IndexOf (new StringIterator (s, start, length, opt), new StringIterator (target, opt));
		}

		private int IndexOf (StringIterator src,
			CharacterIterator target)
		{
			while (src.MoveNext ()) {
				target.Reset ();
				target.MoveNext ();
				if (IsPrefix (src, target))
					return src.Current;
			}
			return -1;
		}

		// From the end of the string to the start, run IsPrefix()
		// and if it was true then return "length - the_location".
		// If no match, return -1.
		public int LastIndexOf (string s, char target, int start, int length, CompareOptions opt)
		{
			StringIterator src = new StringIterator (s, start, length, opt);
			src.MoveTo (s.Length - 1);
			return LastIndexOf (src, new SingleCharacterIterator (target));
		}

		public int LastIndexOf (string s, string target, int start, int length, CompareOptions opt)
		{
			StringIterator src = new StringIterator (s, start, length. opt);
			src.MoveTo (s.Length - target.Length);
			return IndexOf (src, new StringIterator (target, opt));
		}

		private int LastIndexOf (StringIterator src,
			CharacterIterator target, ref bool testSuffix)
		{
			int r = IndexOf (src, target);
			if (r >= 0) { // further results might be found
				while (src.MoveNext ()) {
					int i = IndexOf (src, target);
					if (i < 0)
						break;
					r = i;
				};
				return r;
			} else { // if not found then search up to the top.
				for (int i = src.Length - 1; i >= 0; i--) {
					src.MoveTo (i);
					src.MoveNext ();
					target.Reset ();
					target.MoveNext ();
					if (IsPrefix (src, target, ref testSuffix))
						return src.Current;
				}
				return -1;
			}
		}

		#endregion

		#region IsPrefix(), IsSuffix()
		//
		// Create character iterator for both of the argument strings.
		// Compare them until the end of the target. If there was a 
		// difference, then return false.
		//
		public bool IsPrefix (string src, string target, CompareOptions opt)
		{
			bool dummy = false;
			return IsPrefix (
				new StringIterator (src, 0, src.Length, opt),
				new StringIterator (target, 0, target.Length, opt), dummy);
		}

		private bool IsPrefix (StringIterator src,
			CharacterIterator target, ref bool testSuffix)
		{
			int i = src.Current;
			try {
				do {
					if (CompareChar (src, target) != 0) {
						return false;
					}
					if (!target.MoveNext ()) {
						if (testSuffix)
							testSuffix = !src.MoveNext ();
						return true;
					}
				} while (src.MoveNext ());
				return false;
			} finally {
				src.MoveTo (i);
			}
		}

		/*
		<!--
		Create the sortkey for the target.
		Create a character iterator for searchee.
		Move to the point where it holds the same offset as the length
		of the target.
		Compute the sortkey for the iterator and compare primary length.
		While the searchee key is shorter, then move the offset back and
		try again.
		compare the sortkeys, from the end, level by level. Their
		length could be different (because the searchee key could be
		longer in this algorithm), but it is OK.
		If ALL levels matches from the end, then return true.
		Otherwise return false.
		-->
		
		I think it could be implemented more easily and safely,
		using LastIndexOf().
		*/
		public bool IsSuffix (string src, string target, CompareOptions opt)
		{
			StringIterator si = new StringIterator (src, 0, src.Length, opt, this);
			StringIterator ti = new StringIterator (target, 0, target.Length, opt, this);

			// FIXME: this is not effective at all.
//			si.MoveTo (src.Length - target.Length);
			bool suffixTest = true;
			int last = LastIndexOf (si, ti, ref suffixTest);
			return (last >= 0) && suffixTest;
		}
		#endregion

		#region Compare()
/*
		We initially hold "ActualOption" as initial CompareOptions value.
		It is changed during the comparison.

		Start codepoint comparison from the beginning. When different,
		move back to the "safe" character, and do "actual comparison"
		which takes ActualOption into consideration.

		If there is level 1 difference, it is returned.

		If there is level 2 difference, then add flags 'f' to 
		ActualOption and store the result, where 'f' is
		1) CompareOptions.Ignore* for non-French mode, or
		2) CompareOptions.Ignore*- IgnoreNonSpace for French mode.
		(Thus for French mode it still checks the "latest" diacritical
		differences).

		If there is level 3 difference, then add Ignore* except for
		IgnoreNonSpace to ActualOption and store the result.

		If there is level 4 difference, then all IgnoreKanaType to
		ActualOption and store the result.

		The stored result is returned when there was no primary
		difference.
*/
		[Obsolete ("a bit old code")]
		public int Compare (string s1, int start1, int len1,
			string s2, int start2, int len2, CompareOptions opt)
		{
			/*
			if (s1.Length < start1 + len1 ||
				s2.Length < start2 + len2)
				throw new ArgumentOutOfRangeException ("start and length must not be out of range of the string.");

			int min = len1 < len2 ? len1 : len2;
			int start = 0;

			while (true) {
				int stop = -1;
				for (int i = start; i < min; i++) {
					if (s1 [start1 + i] != s2 [start2 + i]) {
						stop = i;
						break;
					}
				}
				if (stop == -1)
					break;

				// go back to where NFD-normalization should start.
				for (int i = stop; i >= start1; i--) {
					if (!CharUnicodeInfo.IsUnsafe (s1 [i])) {
						start1 = i;
						break;
					}
				}
				for (int i = stop; i >= start2; i--) {
					if (!CharUnicodeInfo.IsUnsafe (s2 [i])) {
						start2 = i;
						break;
					}
				}

				CompareResult ret = ComparePrimary (s1, start1, len1, s2, start2, len2);
				if (ret.Result == 0 && Strength >= 2)
					ret = CompareSecondary (s1, start1, len1, s2, start2, len2);
				if (ret.Result == 0 && Strength >= 3)
					ret = CompareThirtiary (s1, start1, len1, s2, start2, len2);
				if (ret.Result == 0 && Strength >= 4)
					ret = CompareQuaternary (s1, start1, len1, s2, start2, len2);
				if (ret.Result != 0)
					return ret.Result;
				start1 += ret.Advance1;
				start2 += ret.Advance2;
			}

			if (len1 == len2)
				return 0;
			return len2 > len1 ? 1 : -1;
			*/
		}
		#endregion

		#region GetSortKey()
		//
		// Create character iterator.
		// Do below until the end of the iterator:
		//	- Move next character.
		//	- Get corresponding sortkey indexes with options.
		// Collect the results and return as a byte [] array.
		//
		public byte [] GetSortKey (string s, CompareOptions opt)
		{
			StringIterator iter = new StringIterator (s, 0, s.Length, opt, this);
			buf.AdjustBufferSize (s);
			while (iter.MoveNext ())
				GetSortKeyForChar (iter, buf);
			return buf.GetResultAndReset ();
		}

		private void GetSortKeyForChar (
			StringIterator iter, SortKeyBuffer buf)
		{
			// TODO: fill it.
		}
		#endregion
	}
}
