// -*- Mode: C++; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.String.cs
//
// Authors:
//   Jeffrey Stedfast (fejj@ximian.com)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

// FIXME: from what I gather from msdn, when a function is to return an empty string
//        we should be returning this.Empty - some methods do this and others don't.

// FIXME: I didn't realise until later that `string' has a .Length method and so
//        I am missing some proper bounds-checking in some methods. Find these
//        instances and throw the ArgumentOutOfBoundsException at the programmer.
//        I like pelting programmers with ArgumentOutOfBoundsException's :-)

// FIXME: The ToLower(), ToUpper(), and Compare(..., bool ignoreCase) methods
//        need to be made unicode aware.

// FIXME: when you have a char carr[], does carr.Length include the terminating null char?

using System;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {

	//[DefaultMemberName("Chars")]
	[Serializable]
	public sealed class String : IComparable, ICloneable, IConvertible, IEnumerable {
		public static readonly string Empty = "";
		private char[] c_str;
		private int length;

		// Constructors

		internal String (int storage)
		{
			if (storage < 0)
				throw new ArgumentOutOfRangeException ();
			length = storage;
			c_str = new char [storage];
		}
		
		[CLSCompliant(false)]
		unsafe public String (char *value)
		{
			int i;

			// FIXME: can I do value.Length here?
			if (value == null) {
				this.length = 0;
			} else {
				for (i = 0; *(value + i) != '\0'; i++);
				this.length = i;
			}

			this.c_str = new char [this.length + 1];
			for (i = 0; i < this.length; i++)
				this.c_str[i] = *(value + i);
		}

		public String (char[] value)
		{
			int i;

			// FIXME: value.Length includes the terminating null char?
			this.length = value != null ? strlen (value): 0;
			this.c_str = new char [this.length + 1];
			for (i = 0; i < this.length; i++)
				this.c_str[i] = value[i];
		}

		[CLSCompliant(false)]
		unsafe public String (sbyte *value)
		{
			// FIXME: consider unicode?
			int i;

			// FIXME: can I do value.Length here? */
			if (value == null) {
				this.length = 0;
			} else {
				for (i = 0; *(value + i) != '\0'; i++);
				this.length = i;
			}

			this.c_str = new char [this.length + 1];
			for (i = 0; i < this.length; i++)
				this.c_str[i] = (char) *(value + i);
		}

		public String (char c, int count)
		{
			int i;

			this.length = count;
			this.c_str = new char [count + 1];
			for (i = 0; i < count; i++)
				this.c_str[i] = c;
		}

		[CLSCompliant(false)]
		unsafe public String (char *value, int startIndex, int length)
		{
			int i;

			if (value == null && startIndex != 0 && length != 0)
				throw new ArgumentNullException ();

			if (startIndex < 0 || length < 0)
				throw new ArgumentOutOfRangeException ();

			this.length = length;
			this.c_str = new char [length + 1];
			for (i = 0; i < length; i++)
				this.c_str[i] = *(value + startIndex + i);
		}

		public String (char[] value, int startIndex, int length)
		{
			int i;

			if (value == null && startIndex != 0 && length != 0)
				throw new ArgumentNullException ();

			if (startIndex < 0 || length < 0)
				throw new ArgumentOutOfRangeException ();

			this.length = length;
			this.c_str = new char [length + 1];
			for (i = 0; i < length; i++)
				this.c_str[i] = value[startIndex + i];
		}

		[CLSCompliant(false)]
		unsafe public String (sbyte *value, int startIndex, int length)
		{
			// FIXME: consider unicode?
			int i;

			if (value == null && startIndex != 0 && length != 0)
				throw new ArgumentNullException ();

			if (startIndex < 0 || length < 0)
				throw new ArgumentOutOfRangeException ();

			this.length = length;
			this.c_str = new char [length + 1];
			for (i = 0; i < length; i++)
				this.c_str[i] = (char) *(value + startIndex + i);
		}

		[CLSCompliant(false)][MonoTODO]
		unsafe public String (sbyte *value, int startIndex, int length, Encoding enc)
		{
			// FIXME: implement me
		}

		~String ()
		{
			// FIXME: is there anything we need to do here?
			/*base.Finalize ();*/
		}

		// Properties
		public int Length {
			get {
				return this.length;
			}
		}

		[IndexerName("Chars")]
		public char this [int index] {
			get {
				if (index >= this.length)
					throw new ArgumentOutOfRangeException ();

				return this.c_str[index];
			}
		}

		// Private helper methods
		private static int strlen (char[] str)
		{
			// FIXME: if str.Length includes terminating null char, then return (str.Length - 1)
			return str.Length;
		}

		[MonoTODO]
		private static char tolowerordinal (char c)
		{
			// FIXME: implement me
			return c;
		}

		private static bool is_lwsp (char c)
		{
			/* this comes from the msdn docs for String.Trim() */
			if ((c >= '\x9' && c <= '\xD') || c == '\x20' || c == '\xA0' ||
			    (c >= '\x2000' && c <= '\x200B') || c == '\x3000' || c == '\xFEFF')
				return true;
			else
				return false;
		}

		private static int BoyerMoore (char[] haystack, string needle, int startIndex, int count)
		{
			/* (hopefully) Unicode-safe Boyer-Moore implementation */
			int[] skiptable = new int[65536];  /* our unicode-safe skip-table */
			int h, n, he, ne, hc, nc, i;

			if (haystack == null || needle == null)
				throw new ArgumentNullException ();

			/* if the search buffer is shorter than the pattern buffer, we can't match */
			if (count < needle.length)
				return -1;

			/* return an instant match if the pattern is 0-length */
			if (needle.length == 0)
				return startIndex;

			/* set a pointer at the end of each string */
			ne = needle.length - 1;      /* position of char before '\0' */
			he = startIndex + count;     /* position of last valid char */

			/* init the skip table with the pattern length */
			nc = needle.length;
			for (i = 0; i < 65536; i++)
				skiptable[i] = nc;

			/* set the skip value for the chars that *do* appear in the
			 * pattern buffer (needle) to the distance from the index to
			 * the end of the pattern buffer. */
			for (nc = 0; nc < ne; nc++)
				skiptable[(int) needle[nc]] = ne - nc;

			h = startIndex;
			while (count >= needle.length) {
				hc = h + needle.length - 1;  /* set the haystack compare pointer */
				nc = ne;                     /* set the needle compare pointer */

				/* work our way backwards until they don't match */
				for (i = 0; nc > 0; nc--, hc--, i++)
					if (needle[nc] != haystack[hc])
						break;

				if (needle[nc] != haystack[hc]) {
					n = skiptable[(int) haystack[hc]] - i;
					h += n;
					count -= n;
				} else
					return h;
			}

			return -1;
		}

		// Methods
		[MonoTODO]
		public object Clone ()
		{
			// FIXME: implement me
			return null;
		}

		const int StringCompareModeDirect = 0;
		const int StringCompareModeCaseInsensitive = 1;
		const int StringCompareModeOrdinal = 2;

		internal static int _CompareGetLength (string strA, string strB)
		{
			if ((strA == null) || (strB == null))
					return 0;
				else
				return Math.Max (strA.Length, strB.Length);
		}

		internal static int _CompareChar (char chrA, char chrB, CultureInfo culture,
						  int mode)
		{
			int result = 0;

			switch (mode) {
			case StringCompareModeDirect:
				// FIXME: We should do a culture based comparision here,
				//        but for the moment let's do it by hand.
				//        In the microsoft runtime, uppercase letters
				//        sort after lowercase letters in the default
				//        culture.
				if (Char.IsUpper (chrA) && Char.IsLower (chrB))
				return 1;
				else if (Char.IsLower (chrA) && Char.IsUpper (chrB))
				return -1;
				result = (int) (chrA - chrB);
				break;
			case StringCompareModeCaseInsensitive:
				result = (int) (Char.ToLower (chrA) - Char.ToLower (chrB));
				break;
			case StringCompareModeOrdinal:
				result = (int) (tolowerordinal (chrA) - tolowerordinal (chrB));
				break;
			}

			if (result == 0)
					return 0;
			else if (result < 0)
					return -1;
			else
				return 1;
		}

		[MonoTODO]
		internal static int _Compare (string strA, int indexA, string strB, int indexB,
					      int length, CultureInfo culture,
					      int mode)

		{
			int i;

			/* When will the hurting stop!?!? */
			if (strA == null) {
				if (strB == null)
					return 0;
				else
					return -1;
			} else if (strB == null)
				return 1;

			if (length < 0 || indexA < 0 || indexB < 0)
				throw new ArgumentOutOfRangeException ();

			if (indexA > strA.Length || indexB > strB.Length)
				throw new ArgumentOutOfRangeException ();

			// FIXME: Implement culture
			if (culture != null)
				throw new NotImplementedException ();

			for (i = 0; i < length - 1; i++) {
				if ((indexA+i >= strA.Length) || (indexB+i >= strB.Length))
					break;

				if (_CompareChar (strA[indexA+i], strB[indexB+i], culture, mode) != 0)
					break;
		}

			if (indexA+i >= strA.Length) {
				if (indexB+i >= strB.Length)
					return 0;
				else
					return -1;
			} else if (indexB+i >= strB.Length)
				return 1;

			return _CompareChar (strA[indexA+i], strB[indexB+i], culture, mode);
		}


		public static int Compare (string strA, string strB)
		{
			return Compare (strA, strB, false);
		}

		public static int Compare (string strA, string strB, bool ignoreCase)
		{
			return Compare (strA, strB, ignoreCase, null);
			}

		public static int Compare (string strA, string strB, bool ignoreCase, CultureInfo culture)
		{
			return Compare (strA, 0, strB, 0,
					_CompareGetLength (strA, strB),
					ignoreCase, culture);
		}

		public static int Compare (string strA, int indexA, string strB, int indexB, int length)
		{
			return  Compare (strA, indexA, strB, indexB, length, false);
		}

		public static int Compare (string strA, int indexA, string strB, int indexB,
					   int length, bool ignoreCase)
		{
			return Compare (strA, indexA, strB, indexB, length, ignoreCase, null);
		}

		public static int Compare (string strA, int indexA, string strB, int indexB,
					   int length, bool ignoreCase, CultureInfo culture)
		{
			int mode;

			mode = ignoreCase ? StringCompareModeCaseInsensitive :
				StringCompareModeDirect;

			return _Compare (strA, indexA, strB, indexB, length, culture, mode);
		}

		public static int CompareOrdinal (string strA, string strB)
		{
			return CompareOrdinal (strA, 0, strB, 0, _CompareGetLength (strA, strB));
			}

		public static int CompareOrdinal (string strA, int indexA, string strB, int indexB,
						  int length)
		{
			return _Compare (strA, indexA, strB, indexB, length, null,
					 StringCompareModeOrdinal);
		}

		public int CompareTo (object obj)
		{
			return Compare (this, obj == null ? null : obj.ToString ());
		}

		public int CompareTo (string str)
		{
			return Compare (this, str);
		}

		public static string Concat (object arg)
		{
			return arg != null ? arg.ToString () : String.Empty;
		}

		public static string Concat (params object[] args)
		{
			string[] strings;
			char[] str;
			int len, i;

			if (args == null)
				throw new ArgumentNullException ();

			strings = new string [args.Length];
			len = 0;
			i = 0;
			foreach (object arg in args) {
				/* use Empty for each null argument */
				if (arg == null)
					strings[i] = String.Empty;
				else
					strings[i] = arg.ToString ();
				len += strings[i].length;
				i++;
			}

			if (len == 0)
				return String.Empty;

			String res = new String (len);
			str = res.c_str;
			i = 0;
			for (int j = 0; j < strings.Length; j++)
				for (int k = 0; k < strings[j].length; k++)
					str[i++] = strings[j].c_str[k];

			return res;
		}

		public static string Concat (params string[] values)
		{
			int len, i;
			char[] str;

			if (values == null)
				throw new ArgumentNullException ();

			len = 0;
			foreach (string value in values)
				len += value != null ? value.Length : 0;

			if (len == 0)
				return String.Empty;

			String res = new String (len);
			str = res.c_str;
			i = 0;
			foreach (string value in values) {
				if (value == null)
					continue;

				for (int j = 0; j < value.length; j++)
					str[i++] = value.c_str[j];
			}

			return res;
		}

		public static string Concat (object arg0, object arg1)
		{
			string str0 = arg0 != null ? arg0.ToString () : String.Empty;
			string str1 = arg1 != null ? arg1.ToString () : String.Empty;

			return Concat (str0, str1);
		}

		public static string Concat (string str0, string str1)
		{
			char[] concat;
			int i, j, len;

			if (str0 == null)
				str0 = String.Empty;
			if (str1 == null)
				str1 = String.Empty;

			len = str0.length + str1.length;
			if (len == 0)
				return String.Empty;

			String res = new String (len);

			concat = res.c_str;
			for (i = 0; i < str0.length; i++)
				concat[i] = str0.c_str[i];
			for (j = 0 ; j < str1.length; j++)
				concat[i + j] = str1.c_str[j];

			return res;
		}

		public static string Concat (object arg0, object arg1, object arg2)
		{
			string str0 = arg0 != null ? arg0.ToString () : String.Empty;
			string str1 = arg1 != null ? arg1.ToString () : String.Empty;
			string str2 = arg2 != null ? arg2.ToString () : String.Empty;

			return Concat (str0, str1, str2);
		}

		public static string Concat (string str0, string str1, string str2)
		{
			char[] concat;
			int i, j, k, len;

			if (str0 == null)
				str0 = String.Empty;
			if (str1 == null)
				str1 = String.Empty;
			if (str2 == null)
				str2 = String.Empty;

			len = str0.length + str1.length + str2.length;
			if (len == 0)
				return String.Empty;

			String res = new String (len);

			concat = res.c_str;
			for (i = 0; i < str0.length; i++)
				concat[i] = str0.c_str[i];
			for (j = 0; j < str1.length; j++)
				concat[i + j] = str1.c_str[j];
			for (k = 0; k < str2.length; k++)
				concat[i + j + k] = str2.c_str[k];

			return res;
		}

		public static string Concat (string str0, string str1, string str2, string str3)
		{
			char[] concat;
			int i, j, k, l, len;

			if (str0 == null)
				str0 = String.Empty;
			if (str1 == null)
				str1 = String.Empty;
			if (str2 == null)
				str2 = String.Empty;
			if (str3 == null)
				str3 = String.Empty;

			len = str0.length + str1.length + str2.length + str3.length;
			if (len == 0)
				return String.Empty;
			String res = new String (len);

			concat = res.c_str;
			for (i = 0; i < str0.length; i++)
				concat[i] = str0.c_str[i];
			for (j = 0; j < str1.length; j++)
				concat[i + j] = str1.c_str[j];
			for (k = 0; k < str2.length; k++)
				concat[i + j + k] = str2.c_str[k];
			for (l = 0; l < str3.length; l++)
				concat[i + j + k + l] = str3.c_str[l];

			return res;
		}

		public static string Copy (string str)
		{
			// FIXME: how do I *copy* a string if I can only have 1 of each?
			if (str == null)
				throw new ArgumentNullException ();

			return str;
		}

		public void CopyTo (int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			// LAMESPEC: should I null-terminate?
			int i;

			if (destination == null)
				throw new ArgumentNullException ();

			if (sourceIndex < 0 || destinationIndex < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			if (sourceIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			if (destinationIndex + count > destination.Length)
				throw new ArgumentOutOfRangeException ();

			for (i = 0; i < count; i++)
				destination[destinationIndex + i] = this.c_str[sourceIndex + i];
		}

		public bool EndsWith (string value)
		{
			bool endswith = true;
			int start, i;

			if (value == null)
				throw new ArgumentNullException ();

			start = this.length - value.length;
			if (start < 0)
				return false;

			for (i = start; i < this.length && endswith; i++)
				endswith = this.c_str[i] == value.c_str[i - start];

			return endswith;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is String))
				return false;

			return this == (String) obj;
		}

		public bool Equals (string value)
		{
			return this == value;
		}

		public static bool Equals (string a, string b)
		{
			return a == b;
		}

		public static string Format (string format, object arg0) {
			return Format (null, format, new object[] { arg0 });
		}

		public static string Format (string format, object arg0, object arg1) {
			return Format (null, format, new object[] { arg0, arg1 });
		}

		public static string Format (string format, object arg0, object arg1, object arg2) {
			return Format (null, format, new object[] { arg0, arg1, arg2 });
		}

		public static string Format (string format, params object[] args) {
			return Format (null, format, args);
		}

		public static string Format (IFormatProvider provider, string format, params object[] args) {
			if (format == null || args == null)
				throw new ArgumentNullException ();
		
			StringBuilder result = new StringBuilder ();

			int ptr = 0;
			int start = ptr;
			while (ptr < format.Length) {
				char c = format[ptr ++];

				if (c == '{') {
					result.Append (format, start, ptr - start - 1);

					// check for escaped open bracket

					if (format[ptr] == '{') {
						start = ptr ++;
						continue;
					}

					// parse specifier
				
					int n, width;
					bool left_align;
					string arg_format;

					ParseFormatSpecifier (format, ref ptr, out n, out width, out left_align, out arg_format);
					if (n >= args.Length)
						throw new FormatException ("Index (zero based) must be greater than or equal to zero and less than the size of the argument list.");

					// format argument

					object arg = args[n];

					string str;
					if (arg == null)
						str = "";
					else if (arg is IFormattable)
						str = ((IFormattable)arg).ToString (arg_format, provider);
					else
						str = arg.ToString ();

					// pad formatted string and append to result

					if (width > str.Length) {
						string pad = new String (' ', width - str.Length);

						if (left_align) {
							result.Append (str);
							result.Append (pad);
						}
						else {
							result.Append (pad);
							result.Append (str);
						}
					}
					else
						result.Append (str);

					start = ptr;
				}
				else if (c == '}' && format[ptr] == '}') {
					result.Append (format, start, ptr - start - 1);
					start = ptr ++;
				}
			}

			if (start < format.Length)
				result.Append (format.Substring (start));

			return result.ToString ();
		}

		public CharEnumerator GetEnumerator ()
		{
			return new CharEnumerator (this);
		}
		
                IEnumerator IEnumerable.GetEnumerator ()
		{
			return new CharEnumerator (this);
		}

		public override int GetHashCode ()
		{
			int h = 0;
			int i;
			for (i = 0; i < length; ++i)
				h = (h << 5) - h + c_str [i];
			return h;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.String;
		}

		public int IndexOf (char value)
		{
			return IndexOf (value, 0, this.length);
		}

		public int IndexOf (string value)
		{
			return IndexOf (value, 0, this.length);
		}

		public int IndexOf (char value, int startIndex)
		{
			return IndexOf (value, startIndex, this.length - startIndex);
		}

		public int IndexOf (string value, int startIndex)
		{
			return IndexOf (value, startIndex, this.length - startIndex);
		}

		public int IndexOf (char value, int startIndex, int count)
		{
			int i;

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			for (i = startIndex; i - startIndex < count; i++)
				if (this.c_str[i] == value)
					return i;

			return -1;
		}

		public int IndexOf (string value, int startIndex, int count)
		{
			if (value == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

#if XXX
			return BoyerMoore (this.c_str, value, startIndex, count);
#endif
			int i;
			for (i = startIndex; i - startIndex + value.length <= count; ) {
				if (this.c_str[i] == value.c_str [0]) {
					bool equal = true;
					int j, nexti = 0;

					for (j = 1; equal && j < value.length; j++) {
						equal = this.c_str[i + j] == value.c_str [j];
						if (this.c_str [i + j] == value.c_str [0] && nexti == 0)
							nexti = i + j;
					}

					if (equal)
						return i;

					if (nexti != 0)
						i = nexti;
					else
						i += j;
				} else
					i++;
			}

			return -1;
		}

		public int IndexOfAny (char[] values)
		{
			return IndexOfAny (values, 0, this.length);
		}

		public int IndexOfAny (char[] values, int startIndex)
		{
			return IndexOfAny (values, startIndex, this.length - startIndex);
		}

		public int IndexOfAny (char[] values, int startIndex, int count)
		{
			if (values == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			for (int i = startIndex; i < startIndex + count; i++) {
				for (int j = 0; j < strlen (values); j++) {
					if (this.c_str[i] == values[j])
						return i;
				}
			}

			return -1;
		}

		public string Insert (int startIndex, string value)
		{
			char[] str;
			int i, j;

			if (value == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			String res = new String (value.length + this.length);

			str = res.c_str;
			for (i = 0; i < startIndex; i++)
				str[i] = this.c_str[i];
			for (j = 0; j < value.length; j++)
				str[i + j] = value.c_str[j];
			for ( ; i < this.length; i++)
				str[i + j] = this.c_str[i];

			return res;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static string _Intern (string str);

		public static string Intern (string str)
		{
			if (str == null)
				throw new ArgumentNullException ();

			return _Intern (str);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static string _IsInterned (string str);

		public static string IsInterned (string str)
		{
			if (str == null)
				throw new ArgumentNullException ();

			return _IsInterned (str);
		}

		public static string Join (string separator, string[] value)
		{
			if (value == null)
				throw new ArgumentNullException ();

			return Join (separator, value, 0, value.Length);
		}

		public static string Join (string separator, string[] value, int startIndex, int count)
		{
			// LAMESPEC: msdn doesn't specify what happens when separator is null
			int len, i, j, used;
			char[] str;

			if (separator == null || value == null)
				throw new ArgumentNullException ();

			if (startIndex + count > value.Length)
				throw new ArgumentOutOfRangeException ();

			len = 0;
			for (i = startIndex, used = 0; used < count; i++, used++) {
				if (i != startIndex)
					len += separator.length;

				len += value[i].length;
			}

			// We have no elements to join?
			if (i == startIndex)
				return String.Empty;

			String res = new String (len);

			str = res.c_str;
			for (i = 0; i < value[startIndex].length; i++)
				str[i] = value[startIndex][i];

			used = 1;
			for (j = startIndex + 1; used < count; j++, used++) {
				int k;

				for (k = 0; k < separator.length; k++)
					str[i++] = separator.c_str[k];
				for (k = 0; k < value[j].length; k++)
					str[i++] = value[j].c_str[k];
			}

			return res;
		}

		public int LastIndexOf (char value)
		{
			int i = this.length;
			if (i == 0)
				return -1;
			--i;
			for (; i >= 0; i--) {
				if (this.c_str[i] == value)
					return i;
			}

			return -1;
		}

		public int LastIndexOf (string value)
		{
			if (value == null)
				throw new ArgumentNullException ();
			if (value.length == 0)
				return 0;
			if (this.length == 0)
				return -1;
				
			return LastIndexOf (value, this.length - 1, this.length);
		}

		public int LastIndexOf (char value, int startIndex)
		{
			if (startIndex < 0 || startIndex >= this.length)
				throw new ArgumentOutOfRangeException ();

			for (int i = startIndex; i >= 0; i--) {
				if (this.c_str[i] == value)
					return i;
			}

			return -1;
		}

		public int LastIndexOf (string value, int startIndex)
		{
			return LastIndexOf (value, startIndex, startIndex + 1);
		}

		public int LastIndexOf (char value, int startIndex, int count)
		{
			if (startIndex < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			if (startIndex >= this.length || startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ();

			for (int i = startIndex; i > startIndex - count; i--) {
				if (this.c_str[i] == value)
					return i;
			}

			return -1;
		}

		public int LastIndexOf (string value, int startIndex, int count)
		{
			// startIndex points to the end of value, ie. we're searching backwards.
			int i, len;

			if (value == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			if (count < 0 || startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ();

			if (value.length > startIndex)
				return -1;

			if (value == String.Empty)
				return startIndex;

			if (startIndex == this.length)
				startIndex--;

			// FIXME: use a reversed-unicode-safe-Boyer-Moore?
			len = value.length - 1;
			for (i = startIndex; i > startIndex - count; i--) {

				if (this.c_str[i] == value.c_str[len]) {
					bool equal = true;
					int j;

					for (j = 0; equal && j < len; j++)
						equal = this.c_str[i - j] == value.c_str[len - j];

					if (equal)
						return i - j;
				}
			}

			return -1;
		}

		public int LastIndexOfAny (char[] values)
		{
			return LastIndexOfAny (values, this.length - 1, this.length);
		}

		public int LastIndexOfAny (char[] values, int startIndex)
		{
			return LastIndexOfAny (values, startIndex, startIndex + 1);
		}

		public int LastIndexOfAny (char[] values, int startIndex, int count)
		{
			int i;

			if (values == null)
				throw new ArgumentNullException ();

			if (startIndex < 0 || count < 0 || startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ();

			for (i = startIndex; i > startIndex - count; i--) {
				for (int j = 0; j < strlen (values); j++) {
					if (this.c_str[i] == values[j])
						return i;
				}
			}

			return -1;
		}

		public string PadLeft (int totalWidth)
		{
			return PadLeft (totalWidth, ' ');
		}

		public string PadLeft (int totalWidth, char padChar)
		{
			char[] str;
			int i, j;

			if (totalWidth < 0)
				throw new ArgumentException ();

			str = new char [totalWidth > this.length ? totalWidth : this.length];
			for (i = 0; i < totalWidth - this.length; i++)
				str[i] = padChar;

			for (j = 0; j < this.length; i++, j++)
				str[i] = this.c_str[j];

			return new String (str);
		}

		public string PadRight (int totalWidth)
		{
			return PadRight (totalWidth, ' ');
		}

		public string PadRight (int totalWidth, char padChar)
		{
			char[] str;
			int i;

			if (totalWidth < 0)
				throw new ArgumentException ();

			str = new char [totalWidth > this.length ? totalWidth : this.length];
			for (i = 0; i < this.length; i++)
				str[i] = this.c_str[i];

			for ( ; i < str.Length; i++)
				str[i] = padChar;

			return new String (str);
		}

		public string Remove (int startIndex, int count)
		{
			char[] str;
			int i, j, len;

			if (startIndex < 0 || count < 0 || startIndex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			len = this.length - count;
			if (len == 0)
				return String.Empty;
			
			String res = new String (len);
			str = res.c_str;
			for (i = 0; i < startIndex; i++)
				str[i] = this.c_str[i];
			for (j = i + count; j < this.length; j++)
				str[i++] = this.c_str[j];

			return res;
		}

		public string Replace (char oldChar, char newChar)
		{
			char[] str;
			int i;

			String res = new String (length);
			str = res.c_str;
			for (i = 0; i < this.length; i++) {
				if (this.c_str[i] == oldChar)
					str[i] = newChar;
				else
					str[i] = this.c_str[i];
			}

			return res;
		}

		public string Replace (string oldValue, string newValue)
		{
			// If newValue is null, oldValue is removed.
			int index, len, i, j, newlen;
			string thestring;
			char[] str;

			if (oldValue == null)
				throw new ArgumentNullException ();

			thestring = Substring (0, this.length);
			index = 0;

			// Runs until all occurences of oldValue have been replaced.
			while (true) {
				// Use IndexOf in case I later rewrite it to use Boyer-Moore
				index = thestring.IndexOf (oldValue, index);

				if (index == -1)
					return thestring;

				newlen = (newValue == null) ? 0 : newValue.length;
				len = thestring.length - oldValue.length + newlen;

				if (len == 0)
					return String.Empty;

				String res = new String (len);
				str = res.c_str;
				for (i = 0; i < index; i++)
					str[i] = thestring.c_str[i];
				for (j = 0; j < newlen; j++)
					str[i++] = newValue[j];
				for (j = index + oldValue.length; j < thestring.length; j++)
					str[i++] = thestring.c_str[j];

				// Increment index, we're already done replacing until this index.
				thestring = res;
				index += newlen;
			}
		}

		private int splitme (char[] separators, int startIndex)
		{
			/* this is basically a customized IndexOfAny() for the Split() methods */
			for (int i = startIndex; i < this.length; i++) {
				if (separators != null) {
					foreach (char sep in separators) {
						if (this.c_str[i] == sep)
							return i - startIndex;
					}
				} else if (is_lwsp (this.c_str[i])) {
					return i - startIndex;
				}
			}

			return -1;
		}

		public string[] Split (params char[] separator)
		{
			/**
			 * split:
			 * @separator: delimiting chars or null to split on whtspc
			 *
			 * Returns: 1. An array consisting of a single
			 * element (@this) if none of the delimiting
			 * chars appear in @this. 2. An array of
			 * substrings which are delimited by one of
			 * the separator chars. 3. An array of
			 * substrings separated by whitespace if
			 * @separator is null. The Empty string should
			 * be returned wherever 2 delimiting chars are
			 * adjacent.
			 **/
			// FIXME: would using a Queue be better?
			string[] strings;
			ArrayList list;
			int index, len;

			list = new ArrayList ();
			for (index = 0, len = 0; index < this.length; index += len + 1) {
				len = splitme (separator, index);
				len = len > -1 ? len : this.length - index;
				if (len == 0) {
					list.Add (String.Empty);
				} else {
					char[] str;
					int i;

					str = new char [len];
					for (i = 0; i < len; i++)
						str[i] = this.c_str[index + i];

					list.Add (new String (str));
				}
			}

			strings = new string [list.Count];
			if (list.Count == 1) {
				/* special case for an array holding @this */
				strings[0] = this;
			} else {
				for (index = 0; index < list.Count; index++)
					strings[index] = (string) list[index];
			}

			return strings;
		}

		public string[] Split (char[] separator, int maxCount)
		{
			// FIXME: would using Queue be better than ArrayList?
			string[] strings;
			ArrayList list;
			int index, len, used;

			if (maxCount == 0)
				return new string[0];
			else if (maxCount < 0)
				throw new ArgumentOutOfRangeException ();

			used = 0;
			list = new ArrayList ();
			for (index = 0, len = 0; index < this.length && used < maxCount; index += len + 1) {
				len = splitme (separator, index);
				len = len > -1 ? len : this.length - index;
				if (len == 0) {
					list.Add (String.Empty);
				} else {
					char[] str;
					int i;

					str = new char [len];
					for (i = 0; i < len; i++)
						str[i] = this.c_str[index + i];

					list.Add (new String (str));
				}
				used++;
			}

			/* fit the remaining chunk of the @this into it's own element */
			if (index <= this.length)
			{
				char[] str;
				int i;

				str = new char [this.length - index];
				for (i = index; i < this.length; i++)
					str[i - index] = this.c_str[i];

				// maxCount cannot be zero if we reach this point and this means that
				// index can't be zero either.
				if (used == maxCount)
					list[used-1] += this.c_str[index-1] + new String (str);
				else
				list.Add (new String (str));
			}

			strings = new string [list.Count];
			if (list.Count == 1) {
				/* special case for an array holding @this */
				strings[0] = this;
			} else {
				for (index = 0; index < list.Count; index++)
					strings[index] = (string) list[index];
			}

			return strings;
		}

		public bool StartsWith (string value)
		{
			bool startswith = true;
			int i;

			if (value == null)
				throw new ArgumentNullException ();

			if (value.length > this.length)
				return false;

			for (i = 0; i < value.length && startswith; i++)
				startswith = startswith && value.c_str[i] == this.c_str[i];

			return startswith;
		}

		public string Substring (int startIndex)
		{
			char[] str;
			int i, len;

			if (startIndex < 0 || startIndex > this.length)
				throw new ArgumentOutOfRangeException ();

			len = this.length - startIndex;
			if (len == 0)
				return String.Empty;
			String res = new String (len);
			str = res.c_str;
			for (i = startIndex; i < this.length; i++)
				str[i - startIndex] = this.c_str[i];

			return res;
		}

		public string Substring (int startIndex, int length)
		{
			char[] str;
			int i;

			if (startIndex < 0 || length < 0 || startIndex + length > this.length)
				throw new ArgumentOutOfRangeException ();

			if (length == 0)
				return String.Empty;
			
			String res = new String (length);
			str = res.c_str;
			for (i = startIndex; i < startIndex + length; i++)
				str[i - startIndex] = this.c_str[i];

			return res;
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return Convert.ToBoolean (this);
		}
		
		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return Convert.ToByte (this);
		}
		
		char IConvertible.ToChar (IFormatProvider provider)
		{
			return Convert.ToChar (this);
		}

		public char[] ToCharArray ()
		{
			return ToCharArray (0, this.length);
		}

		public char[] ToCharArray (int startIndex, int length)
		{
			char[] chars;
			int i;

			if (startIndex < 0 || length < 0 || startIndex + length > this.length)
				throw new ArgumentOutOfRangeException ();

			chars = new char [length];
			for (i = startIndex; i < length; i++)
				chars[i - startIndex] = this.c_str[i];

			return chars;
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return Convert.ToDateTime (this);
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return Convert.ToDecimal (this);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return Convert.ToDouble (this);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return Convert.ToInt16 (this);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return Convert.ToInt32 (this);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return Convert.ToInt64 (this);
		}

		public string ToLower ()
		{
			char[] str;
			int i;

			String res = new String (length);
			str = res.c_str;
			for (i = 0; i < this.length; i++)
				str[i] = Char.ToLower (this.c_str[i]);

			return res;
		}

		[MonoTODO]
		public string ToLower (CultureInfo culture)
		{
			// FIXME: implement me
			throw new NotImplementedException ();

		}

		[CLSCompliant(false)]
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return Convert.ToSByte (this);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return Convert.ToSingle (this);
		}

		public override string ToString ()
		{
			return this;
		}

		string IConvertible.ToString (IFormatProvider format)
		{
			return this;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return Convert.ToType (this, conversionType,  provider);
		}

		[CLSCompliant(false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return Convert.ToUInt16 (this);
		}

		[CLSCompliant(false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return Convert.ToUInt32 (this);
		}

		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return Convert.ToUInt64 (this);
		}

		public string ToUpper ()
		{
			char[] str;
			int i;

			String res = new String (length);
			str = res.c_str;
			for (i = 0; i < this.length; i++)
				str[i] = Char.ToUpper (this.c_str[i]);

			return res;
		}

		[MonoTODO]
		public string ToUpper (CultureInfo culture)
		{
			// FIXME: implement me
			throw new NotImplementedException ();
		}

		public string Trim ()
		{
			return Trim (null);
		}

		public string Trim (params char[] trimChars)
		{
			int begin, end;
			bool matches = false;

			for (begin = 0; begin < this.length; begin++) {
				if (trimChars != null) {
					matches = false;
					foreach (char c in trimChars) {
						matches = this.c_str[begin] == c;
						if (matches)
							break;
					}
					if (matches)
						continue;
				} else {
					matches = is_lwsp (this.c_str[begin]);
					if (matches)
						continue;
				}
				break;
			}

			for (end = this.length - 1; end > begin; end--) {
				if (trimChars != null) {
					matches = false;
					foreach (char c in trimChars) {
						matches = this.c_str[end] == c;
						if (matches)
							break;
					}
					if (matches)
						continue;
				} else {
					matches = is_lwsp (this.c_str[end]);
					if (matches)
						continue;
				}
				break;
			}
			end++;

			if (begin >= end)
				return String.Empty;

			return Substring (begin, end - begin);
		}

		public string TrimEnd (params char[] trimChars)
		{
			bool matches = true;
			int end;

			for (end = this.length - 1; matches && end > 0; end--) {

				if (trimChars != null) {
					matches = false;
					foreach (char c in trimChars) {
						matches = this.c_str[end] == c;
						if (matches)
							break;
					}
				} else {
					matches = is_lwsp (this.c_str[end]);
				}

				if (!matches)
					return Substring (0, end+1);
			}

			if (end == 0)
				return String.Empty;

			return Substring (0, end);
		}

		public string TrimStart (params char[] trimChars)
		{
			bool matches = true;
			int begin;

			for (begin = 0; matches && begin < this.length; begin++) {
				if (trimChars != null) {
					matches = false;
					foreach (char c in trimChars) {
						matches = this.c_str[begin] == c;
						if (matches)
							break;
					}
				} else {
					matches = is_lwsp (this.c_str[begin]);
				}

				if (!matches)
					return Substring (begin, this.length - begin);
			}

				return String.Empty;
		}

		// Operators
		public static bool operator ==(string a, string b)
		{
			if ((object)a == null) {
				if ((object)b == null)
					return true;
				return false;
			}
			if ((object)b == null)
				return false;
	
			if (a.length != b.length)
				return false;

			int l = a.length;
			for (int i = 0; i < l; i++)
				if (a.c_str[i] != b.c_str[i])
					return false;

			return true;
		}

		public static bool operator !=(string a, string b)
		{
			return !(a == b);
		}

		// private

		private static void ParseFormatSpecifier (string str, ref int ptr, out int n, out int width, out bool left_align, out string format) {
			// parses format specifier of form:
			//   N,[[-]M][:F]}
			//
			// where:

			try {
				// N = argument number (non-negative integer)
			
				n = ParseDecimal (str, ref ptr);
				if (n < 0)
					throw new FormatException ("Input string was not in correct format.");
				
				// M = width (non-negative integer)

				if (str[ptr] == ',') {
					left_align = (str[++ ptr] == '-');
					if (left_align)
						++ ptr;

					width = ParseDecimal (str, ref ptr);
					if (width < 0)
						throw new FormatException ("Input string was not in correct format.");
				}
				else {
					width = 0;
					left_align = false;
				}

				// F = argument format (string)

				if (str[ptr] == ':') {
					int start = ++ ptr;
					while (str[ptr] != '}')
						++ ptr;

					format = str.Substring (start, ptr - start);
				}
				else
					format = null;

				if (str[ptr ++] != '}')
					throw new FormatException ("Input string was not in correct format.");
			}
			catch (IndexOutOfRangeException) {
				throw new FormatException ("Input string was not in correct format.");
			}
		}

		private static int ParseDecimal (string str, ref int ptr) {
			int p = ptr;
			int n = 0;
			while (true) {
				char c = str[p];
				if (c < '0' || '9' < c)
					break;

				n = n * 10 + c - '0';
				++ p;
			}

			if (p == ptr)
				return -1;
			
			ptr = p;
			return n;
		}
	}
}
