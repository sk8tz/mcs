using System;
using System.Globalization;
using System.Text;

using Util = Mono.Globalization.Unicode.NormalizationTableUtil;

namespace Mono.Globalization.Unicode
{
	internal enum NormalizationCheck {
		Yes,
		No,
		Maybe
	}

	internal /*static*/ class Normalization
	{
		public const int NoNfd = 1;
		public const int NoNfkd = 2;
		public const int NoNfc = 4;
		public const int MaybeNfc = 8;
		public const int NoNfkc = 16;
		public const int MaybeNfkc = 32;
		public const int ExpandOnNfd = 64;
		public const int ExpandOnNfc = 128;
		public const int ExpandOnNfkd = 256;
		public const int ExpandOnNfkc = 512;
		public const int FullCompositionExclusion = 1024;

		private delegate NormalizationCheck Checker (char c);

		private static Checker noNfd = new Checker (IsNfd);
		private static Checker noNfc = new Checker (IsNfc);
		private static Checker noNfkd = new Checker (IsNfkd);
		private static Checker noNfkc = new Checker (IsNfkc);

		static int PropIdx (int cp)
		{
			return propIdx [Util.PropIdx (cp)];
		}

		static int MapIdx (int cp)
		{
			return mapIndex [Util.MapIdx (cp)];
		}

		static int GetComposedStringLength (int mapIdx)
		{
			int i = mapIdx;
			while (mappedChars [i] != 0)
				i++;
			return i - mapIdx;
		}

		static byte GetCombiningClass (int c)
		{
			return combiningClass [Util.ToIdx (c)];
		}

		static int GetPrimaryCompositeCharIndex (object chars, int start, int charsLength)
		{
			string s = chars as string;
			StringBuilder sb = chars as StringBuilder;
			char startCh = s != null ? s [start] : sb [start];

			int idx = GetPrimaryCompositeHelperIndex ((int) startCh);
			if (idx == 0)
				return 0;
			while (idx < mappedChars.Length &&
				mappedChars [idx] == startCh) {
				for (int i = 1; ; i++) {
					if (mappedChars [idx + i] == 0)
						// match
						return idx;
					if (start + i < charsLength)
						return 0; // no match
					char curCh = s != null ?
						s [start + i] : sb [start + i];
					if (mappedChars [idx + i] == curCh)
						continue;
					if (mappedChars [idx + i] > curCh)
						return 0; // no match
					// otherwise move idx to next item
					while (mappedChars [i] != 0)
						i++;
					idx = i + 1;
					break;
				}
			}
			// reached to end of entries
			return 0;
		}

		private static string Compose (string source, Checker checker)
		{
			StringBuilder sb = null;
			Decompose (source, ref sb, checker);
			if (sb == null)
				sb = Combine (source, 0, checker);
			else
				Combine (sb, 0, checker);

			return sb != null ? sb.ToString () : source;
		}

		private static StringBuilder Combine (string source, int start, Checker checker)
		{
			for (int i = 0; i < source.Length; i++) {
				if (checker (source [i]) == NormalizationCheck.Yes)
					continue;
				StringBuilder sb = new StringBuilder (source.Length);
				sb.Append (source);
				Combine (sb, 0, checker);
				return sb;
			}
			return null;
		}
		
		private static void Combine (StringBuilder sb, int start, Checker checker)
		{
			for (int i = start; i < sb.Length; i++) {
				switch (checker (sb [i])) {
				case NormalizationCheck.Yes:
					continue;
				case NormalizationCheck.No:
					break;
				case NormalizationCheck.Maybe:
					break;
				}

				// x is starter, or sb[i] is blocked
				int x = i - 1;

				int ch = 0;
				int idx = GetPrimaryComposite (sb, (int) sb [i], sb.Length, x, ref ch);
				if (idx == 0)
					continue;
				sb.Remove (x, GetComposedStringLength (idx));
				sb.Insert (x, (char) ch);
				i--; // apply recursively
			}
		}

		static int GetPrimaryComposite (object o, int cur, int length, int bufferPos, ref int ch)
		{
			if ((propValue [PropIdx (cur)] & FullCompositionExclusion) != 0)
				return 0;
			if (GetCombiningClass (cur) != 0)
				return 0; // not a starter
			int idx = GetPrimaryCompositeCharIndex (o, bufferPos, length);
			if (idx == 0)
				return 0;
			return GetPrimaryCompositeFromMapIndex (idx);
		}

		static bool IsNormalized (string source,
			Checker checker)
		{
			int prevCC = -1;
			for (int i = 0; i < source.Length; i++) {
				int cc = GetCombiningClass (source [i]);
				if (cc != 0 && cc < prevCC)
					return false;
				prevCC = cc;
				switch (checker (source [i])) {
				case NormalizationCheck.Yes:
					break;
				case NormalizationCheck.No:
					return false;
				case NormalizationCheck.Maybe:
					int ch = 0;
					if (GetPrimaryComposite (source,
						source [i], source.Length,
						i, ref ch) != 0)
						return false;
					break;
				}
			}
			return true;
		}

		static string Decompose (string source, Checker checker)
		{
			StringBuilder sb = null;
			Decompose (source, ref sb, checker);
			return sb != null ? sb.ToString () : source;
		}

		static void Decompose (string source,
			ref StringBuilder sb, Checker checker)
		{
			int [] buf = null;
			int start = 0;
			for (int i = 0; i < source.Length; i++)
				if (checker (source [i]) == NormalizationCheck.No)
					DecomposeChar (ref sb, ref buf, source,
						i, ref start);
			if (sb != null)
				sb.Append (source, start, source.Length - start);
			ReorderCanonical (source, ref sb, 1);
		}

		static void ReorderCanonical (string src, ref StringBuilder sb, int start)
		{
			if (sb == null) {
				// check only with src.
				for (int i = 1; i < src.Length; i++) {
					int level = GetCombiningClass (src [i]);
					if (level == 0)
						continue;
					if (GetCombiningClass (src [i - 1]) > level) {
						sb = new StringBuilder (src.Length);
						sb.Append (src, 0, i - 1);
						ReorderCanonical (src, ref sb, i);
						return;
					}
				}
				return;
			}
			// check only with sb
			for (int i = start; i < sb.Length; i++) {
				int level = GetCombiningClass (sb [i]);
				if (level == 0)
					continue;
				if (GetCombiningClass (sb [i - 1]) > level) {
					char c = sb [i - 1];
					sb [i - 1] = sb [i];
					sb [i] = c;
					i--; // apply recursively
				}
			}
		}

		static void DecomposeChar (ref StringBuilder sb,
			ref int [] buf, string s, int i, ref int start)
		{
			if (sb == null)
				sb = new StringBuilder (s.Length + 100);
			sb.Append (s, start, i - start);
			if (buf == null)
				buf = new int [5];
			GetCanonical (s [i], buf, 0);
			for (int x = 0; ; x++) {
				if (buf [x] == 0)
					break;
				if (buf [x] < char.MaxValue)
					sb.Append ((char) buf [x]);
				else { // surrogate
					sb.Append ((char) (buf [x] >> 10 + 0xD800));
					sb.Append ((char) ((buf [x] & 0x0FFF) + 0xDC00));
				}
			}
			start = i + 1;
		}

		public static NormalizationCheck IsNfd (char c)
		{
			return (propValue [PropIdx ((int) c)] & NoNfd) == 0 ?
				NormalizationCheck.Yes : NormalizationCheck.No;
		}

		public static NormalizationCheck IsNfc (char c)
		{
			uint v = propValue [PropIdx ((int) c)];
			return (v & NoNfc) == 0 ?
				(v & MaybeNfc) == 0 ?
				NormalizationCheck.Yes :
				NormalizationCheck.Maybe :
				NormalizationCheck.No;
		}

		public static NormalizationCheck IsNfkd (char c)
		{
			return (propValue [PropIdx ((int) c)] & NoNfkd) == 0 ?
				NormalizationCheck.Yes : NormalizationCheck.No;
		}

		public static NormalizationCheck IsNfkc (char c)
		{
			uint v = propValue [PropIdx ((int) c)];
			return (v & NoNfkc) == 0 ?
				(v & MaybeNfkc) == 0 ?
				NormalizationCheck.Yes :
				NormalizationCheck.Maybe :
				NormalizationCheck.No;
		}

		/* for now we don't use FC_NFKC closure
		public static bool IsMultiForm (char c)
		{
			return (propValue [PropIdx ((int) c)] & 0xF0000000) != 0;
		}

		public static char SingleForm (char c)
		{
			uint v = propValue [PropIdx ((int) c)];
			int idx = (int) ((v & 0x7FFF0000) >> 16);
			return (char) singleNorm [idx];
		}

		public static void MultiForm (char c, char [] buf, int index)
		{
			// FIXME: handle surrogate
			uint v = propValue [PropIdx ((int) c)];
			int midx = (int) ((v & 0x7FFF0000) >> 16);
			buf [index] = (char) multiNorm [midx];
			buf [index + 1] = (char) multiNorm [midx + 1];
			buf [index + 2] = (char) multiNorm [midx + 2];
			buf [index + 3] = (char) multiNorm [midx + 3];
			if (buf [index + 3] != 0)
				buf [index + 4] = (char) 0; // zero termination
		}
		*/

		public static void GetCanonical (int c, int [] buf, int bufIdx)
		{
			for (int i = MapIdx (c); mappedChars [i] != 0; i++)
				buf [bufIdx++] = mappedChars [i];
			buf [bufIdx] = (char) 0;
		}

		public static bool IsNormalized (string source, int type)
		{
			switch (type) {
			default:
				return IsNormalized (source, noNfc);
			case 1:
				return IsNormalized (source, noNfd);
			case 2:
				return IsNormalized (source, noNfkc);
			case 3:
				return IsNormalized (source, noNfkd);
			}
		}

		public static string Normalize (string source, int type)
		{
			switch (type) {
			default:
				return Compose (source, noNfc);
			case 1:
				return Decompose (source, noNfd);
			case 2:
				return Compose (source, noNfkc);
			case 3:
				return Decompose (source, noNfkd);
			}
		}

	// below are autogenerated code.

