//
// System.String.cs
//
// Authors:
//	 Patrik Torstensson
//   Jeffrey Stedfast (fejj@ximian.com)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
	[Serializable]
	public sealed class String : IConvertible, IComparable, ICloneable, IEnumerable {
		[NonSerialized]	private int length;
		[NonSerialized]	private char start_char;

		private const int COMPARE_CASE = 0;
		private const int COMPARE_INCASE = 1;
		private const int COMPARE_ORDINAL = 2;

		public static readonly String Empty = "";

		public static unsafe bool Equals (string str1, string str2)
		{
			if ((str1 as object) == (str2 as object))
				return true;
	    
			if (null == str1 || null == str2)
				return false;

			int len = str1.length;
			
			if (len != str2.length)
				return false;

			if (len == 0)
				return true;
			
			fixed (char * s1 = &str1.start_char, s2 = &str2.start_char) {
				// it must be one char, because 0 len is done above
				if (len < 2)
					return *s1 == *s2;
				
				// check by twos
				int * sint1 = (int *) s1, sint2 = (int *) s2;
				int n2 = len >> 1;
				do {
					if (*sint1++ != *sint2++)
						return false;
				} while (--n2 != 0);
				
				// nothing left
				if ((len & 1) == 0)
					return true;
				
				// check the last one
				return *(char *) sint1 == *(char *) sint2;
			}
		}

		public static bool operator == (String str1, String str2) {
			return Equals(str1, str2);
		}

		public static bool operator != (String str1, String str2) {
			return !Equals(str1, str2);
		}

		public override bool Equals(Object obj) {
			return Equals (this, obj as String);
		}

		public bool Equals(String value) {
			return Equals (this, value);
		}

		[IndexerName("Chars")]
		public extern char this[int index] {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public Object Clone() {
			return this;
		}

		public TypeCode GetTypeCode () {
			return TypeCode.String;
		}

		public void CopyTo(int sindex, char[] dest, int dindex, int count) {
			// LAMESPEC: should I null-terminate?
			
			if (dest == null)
				throw new ArgumentNullException();

			if (sindex < 0 || dindex < 0 || count < 0)
				throw new ArgumentOutOfRangeException (); 

			if (sindex + count > Length)
				throw new ArgumentOutOfRangeException ();

			if (dindex + count > dest.Length)
				throw new ArgumentOutOfRangeException ();

			InternalCopyTo(sindex, dest, dindex, count);
		}

		public char[] ToCharArray() {
			return ToCharArray(0, length);
		}

		public char[] ToCharArray(int sindex, int length) {
			if (sindex < 0 || length < 0 || sindex + length > this.length)
				throw new ArgumentOutOfRangeException (); 

			char [] tmp = new char[length];

			InternalCopyTo(sindex, tmp, 0, length);

			return tmp;
		}
		
		public String [] Split(params char [] separator) {
			return Split(separator, Int32.MaxValue);
		}

		public String[] Split(char[] separator, int count) {
			if (null == separator || separator.Length == 0) {
				separator = WhiteChars;
			}

			if (count < 0)
				throw new ArgumentOutOfRangeException ();

			if (count == 0) 
				return new String[0];

			if (count == 1) 
				return new String[1] { ToString() };

			return InternalSplit(separator, count);
		}

		public String Substring (int sindex) {
			if (sindex < 0 || sindex > this.length) {
				throw new ArgumentOutOfRangeException();
			}

			string tmp = InternalAllocateStr(this.length - sindex);
			InternalStrcpy(tmp, 0, this, sindex, length - sindex);
			
			return tmp;
		}

		public String Substring (int sindex, int length) {
			if (length < 0 || sindex < 0 || sindex + length > this.length) {
				throw new ArgumentOutOfRangeException();
			}

			if (length == 0)
				return String.Empty;

			string tmp = InternalAllocateStr(length);
			InternalStrcpy(tmp, 0, this, sindex, length);

			return tmp;
		}	

		private static readonly char[] WhiteChars = {  (char) 0x9, (char) 0xA, (char) 0xB, (char) 0xC, (char) 0xD, (char) 0x20, (char) 0xA0, (char) 0x2000, (char) 0x2001, (char) 0x2002, (char) 0x2003, (char) 0x2004, (char) 0x2005,
																	  (char) 0x2006, (char) 0x2007, (char) 0x2008, (char) 0x2009, (char) 0x200A, (char) 0x200B, (char) 0x3000, (char) 0xFEFF };

		public String Trim(params char[] chars) {
			if (null == chars || chars.Length == 0)
				chars = WhiteChars;

			return InternalTrim(chars, 0);
		}

		public String TrimStart(params char[] chars) {
			if (null == chars || chars.Length == 0)
				chars = WhiteChars;

			return InternalTrim(chars, 1);
		}

		public String TrimEnd(params char[] chars) {
			if (null == chars || chars.Length == 0)
				chars = WhiteChars;

			return InternalTrim(chars, 2);
		}

		public static int Compare(String s1, String s2) {
			return(Compare(s1, s2, false,
				       CultureInfo.CurrentCulture));
		}

		public static int Compare(String s1, String s2, bool inCase) {
			return(Compare (s1, s2, inCase,
					CultureInfo.CurrentCulture));
		}
		
		public static int Compare(String s1, String s2, bool inCase,
					  CultureInfo culture) {
			if (culture == null) {
				throw new ArgumentNullException ("culture");
			}
			
			if (s1 == null) {
				if (s2 == null) {
					return(0);
				} else {
					return(-1);
				}
			} else if (s2 == null) {
				return(1);
			}

			CompareOptions compopts;
			
			if(inCase) {
				compopts=CompareOptions.IgnoreCase;
			} else {
				compopts=CompareOptions.None;
			}
			
			return(culture.CompareInfo.Compare (s1, s2, compopts));
		}

		public static int Compare(String s1, int i1, String s2, int i2,
					  int length) {
			return(Compare(s1, i1, s2, i2, length, false,
				       CultureInfo.CurrentCulture));
		}

		public static int Compare(String s1, int i1, String s2, int i2,
					  int length, bool inCase) {
			return(Compare (s1, i1, s2, i2, length, inCase,
					CultureInfo.CurrentCulture));
		}
		
		public static int Compare(String s1, int i1, String s2, int i2,
					  int length, bool inCase,
					  CultureInfo culture) {
			if(culture==null) {
				throw new ArgumentNullException ("culture");
			}

			if((i1 > s1.Length) ||
			   (i2 > s2.Length) ||
			   (i1 < 0) || (i2 < 0) || (length < 0)) {
				throw new ArgumentOutOfRangeException ();
			}

			if (length==0) {
				return(0);
			}
			
			if (s1 == null) {
				if (s2 == null) {
					return(0);
				} else {
					return(-1);
				}
			} else if (s2 == null) {
				return(1);
			}
			
			CompareOptions compopts;
			
			if(inCase) {
				compopts=CompareOptions.IgnoreCase;
			} else {
				compopts=CompareOptions.None;
			}
			
			/* Need to cap the requested length to the
			 * length of the string, because
			 * CompareInfo.Compare will insist that length
			 * <= (string.Length - offset)
			 */
			int len1=length;
			int len2=length;
			
			if(length > (s1.Length - i1)) {
				len1=s1.Length - i1;
			}

			if(length > (s2.Length - i2)) {
				len2=s2.Length - i2;
			}

			return(culture.CompareInfo.Compare(s1, i1, len1,
							   s2, i2, len2,
							   compopts));
		}

		public int CompareTo(Object value) {
			if (null == value)
				return 1;
            
			if (!(value is String))
				throw new ArgumentException();

			return String.Compare(this, (String) value, false);
		}

		public int CompareTo(String str) {
			if (null == str)
				return 1;

			return Compare(this, str, false);
		}

		public static int CompareOrdinal(String s1, String s2) {
			if (s1 == null) {
				if (s2 == null) {
					return(0);
				} else {
					return(-1);
				}
			} else if (s2 == null) {
				return(1);
			}

			/* Invariant, because that is cheaper to
			 * instantiate (and chances are it already has
			 * been.)
			 */
			return(CultureInfo.InvariantCulture.CompareInfo.Compare (s1, s2, CompareOptions.Ordinal));
		}

		public static int CompareOrdinal(String s1, int i1, String s2,
						 int i2, int length)
		{
			if ((i1 > s1.Length) ||
			    (i2 > s2.Length) ||
			    (i1 < 0) || (i2 < 0) || (length < 0)) {
				throw new ArgumentOutOfRangeException ();
			}

			if (s1 == null) {
				if (s2 == null) {
					return(0);
				} else {
					return(-1);
				}
			} else if (s2 == null) {
				return(1);
			}

			/* Need to cap the requested length to the
			 * length of the string, because
			 * CompareInfo.Compare will insist that length
			 * <= (string.Length - offset)
			 */
			int len1=length;
			int len2=length;
			
			if(length > (s1.Length - i1)) {
				len1=s1.Length - i1;
			}

			if(length > (s2.Length - i2)) {
				len2=s2.Length - i2;
			}

			return(CultureInfo.InvariantCulture.CompareInfo.Compare(s1, i1, len1, s2, i2, len2, CompareOptions.Ordinal));
		}

		public bool EndsWith(String value) {
			if (null == value)
				throw new ArgumentNullException();

			if (value == String.Empty) {
				return(true);
			}
			
			if (value.length > this.length) {
				return false;
			}

			return (0 == Compare(this, length - value.length, value, 0, value.length));
		}
	
		public int IndexOfAny(char [] arr) {
			if (null == arr)
				throw new ArgumentNullException();

			return InternalIndexOfAny(arr, 0, this.length);
		}

		public int IndexOfAny(char [] arr, int sindex) {
			if (null == arr)
				throw new ArgumentNullException();
			if (sindex < 0 || sindex >= this.length)
				throw new ArgumentOutOfRangeException();

			return InternalIndexOfAny(arr, sindex, this.length - sindex);
		}

		public int IndexOfAny(char [] arr, int sindex, int count) {
			if (null == arr)
				throw new ArgumentNullException();
			if (sindex < 0 || count < 0 || sindex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			return InternalIndexOfAny(arr, sindex, count);
		}

		public int IndexOf(char value) {
			return(IndexOf(value, 0, this.length));
		}

		public int IndexOf(String value) {
			return(IndexOf(value, 0, this.length));
		}

		public int IndexOf(char value, int sindex) {
			return(IndexOf(value, sindex, this.length - sindex));
		}

		public int IndexOf(String value, int sindex) {
			return(IndexOf(value, sindex, this.length - sindex));
		}

		/* This method is culture-insensitive */
		public int IndexOf(char value, int sindex, int count) {
			if (sindex < 0 || count < 0 ||
			    sindex + count > this.length) {
				throw new ArgumentOutOfRangeException ();
			}

			if ((sindex == 0 && this.length == 0) ||
			    (sindex == this.length) ||
			    (count == 0)) {
				return(-1);
			}
			
			for(int pos=sindex; pos < sindex + count; pos++) {
				if(this[pos]==value) {
					return(pos);
				}
			}

			return(-1);
		}
		
		/* But this one is culture-sensitive */
		public int IndexOf(String value, int sindex, int count) {
			if (value == null) {
				throw new ArgumentNullException();
			}

			if (sindex < 0 || count < 0 ||
			    sindex + count > this.length) {
				throw new ArgumentOutOfRangeException ();
			}

			if (value.length == 0) {
				return sindex;
			}
			
			if (sindex == 0 && this.length == 0) {
				return -1;
			}

			if (count == 0) {
				return(-1);
			}
			
			return(CultureInfo.CurrentCulture.CompareInfo.IndexOf (this, value, sindex, count));
		}

		public int LastIndexOfAny(char [] arr) {
			if (null == arr) 
				throw new ArgumentNullException();

			return InternalLastIndexOfAny(arr, this.length - 1, this.length);
		}

		public int LastIndexOfAny(char [] arr, int sindex) {
			if (null == arr) 
				throw new ArgumentNullException();

			if (sindex < 0 || sindex > this.length)
				throw new ArgumentOutOfRangeException();

			if (this.length == 0)
				return -1;

			return InternalLastIndexOfAny(arr, sindex, sindex + 1);
		}

		public int LastIndexOfAny(char [] arr, int sindex, int count) {
			if (null == arr) 
				throw new ArgumentNullException();

			if (sindex < 0 || count < 0 || sindex > this.length || sindex - count < -1)
				throw new ArgumentOutOfRangeException();

			if (this.length == 0)
				return -1;

			return InternalLastIndexOfAny(arr, sindex, count);
		}

		public int LastIndexOf(char value) {
			if(this.length==0) {
				return(-1);
			} else {
				return(LastIndexOf(value, this.length - 1,
						   this.length));
			}
			
		}

		public int LastIndexOf(String value) {
			if(this.length==0) {
				/* This overload does additional checking */
				return(LastIndexOf(value, 0, 0));
			} else {
				return(LastIndexOf(value, this.length - 1,
						   this.length));
			}
		}

		public int LastIndexOf(char value, int sindex){
			return(LastIndexOf(value, sindex, sindex + 1));
		}

		public int LastIndexOf(String value, int sindex) {
			return(LastIndexOf(value, sindex, sindex + 1));
		}

		/* This method is culture-insensitive */
		public int LastIndexOf(char value, int sindex, int count) {
			if (sindex == 0 && this.length == 0) {
				return -1;
			}

			if (sindex < 0 || count < 0) {
				throw new ArgumentOutOfRangeException ();
			}

			if (sindex >= this.length || sindex - count + 1 < 0) {
				throw new ArgumentOutOfRangeException ();
			}
			
			for(int pos=sindex; pos > sindex - count; pos--) {
				if(this[pos]==value) {
					return(pos);
				}
			}

			return(-1);
		}

		/* But this one is culture-sensitive */
		public int LastIndexOf(String value, int sindex, int count) {
			if (null == value) {
				throw new ArgumentNullException();
			}

			if (value == String.Empty) {
				return(0);
			}

			if (sindex == 0 && this.length == 0) {
				return -1;
			}

			// This check is needed to match undocumented MS behaviour
			if (this.length == 0 && value.length > 0) {
				return(-1);
			}

			if (value.length > sindex) {
				return -1;
			}

			if (count == 0) {
				return(-1);
			}

			if (sindex < 0 || sindex > this.length) {
				throw new ArgumentOutOfRangeException ();
			}

			if (count < 0 || sindex - count + 1 < 0) {
				throw new ArgumentOutOfRangeException ();
			}
			
			return(CultureInfo.CurrentCulture.CompareInfo.LastIndexOf (this, value, sindex, count));
		}

		public String PadLeft(int width) {
			return PadLeft(width, ' ');
		}

		public String PadLeft(int width, char chr) {
			if (width < 0)
				throw new ArgumentException();

			if (width < this.length)
				return String.Copy(this);

			return InternalPad(width, chr, false);
		}

		public String PadRight(int width) {
			return PadRight(width, ' ');
		}

		public String PadRight(int width, char chr) {
			if (width < 0)
				throw new ArgumentException();

			if (width < this.length)
				return String.Copy(this);

			return InternalPad(width, chr, true);
		}

		public bool StartsWith(String value) {
			if (value == null) {
				throw new ArgumentNullException("value");
			}
			
			if (value == String.Empty) {
				return(true);
			}

			if (this.length < value.length) {
				return(false);
			}

			return (0 == Compare(this, 0, value, 0 , value.length));
		}
	
    
		/* This method is culture insensitive */
		public String Replace (char oldChar, char newChar) {
			return(InternalReplace(oldChar, newChar));
		}

		/* This method is culture sensitive */
		public String Replace(String oldValue, String newValue) {
			if(oldValue==null) {
				throw new ArgumentNullException ("oldValue");
			}
			if(oldValue==String.Empty) {
				throw new ArgumentException ("oldValue is the empty string.");
			}

			if(this==String.Empty) {
				return(this);
			}

			if(newValue==null) {
				newValue=String.Empty;
			}
			
			return(InternalReplace (oldValue, newValue, CultureInfo.CurrentCulture.CompareInfo));
		}

		public String Remove(int sindex, int count) {
			if (sindex < 0 || count < 0 || sindex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			return InternalRemove(sindex, count);
		}

		public String ToLower() {
			return(InternalToLower(CultureInfo.CurrentCulture));
		}

		public String ToLower(CultureInfo culture) {
			return(InternalToLower(culture));
		}

		public String ToUpper() {
			return(InternalToUpper(CultureInfo.CurrentCulture));
		}

		public String ToUpper(CultureInfo culture) {
			return(InternalToUpper(culture));
		}

		public override String ToString() {
			return this;
		}

		public String ToString(IFormatProvider provider) {
			return this;
		}

		public String Trim() {
			return Trim(null);
		}

		public static String Format(String format, Object arg0) {
			return Format(null, format, new Object[] {arg0});
		}

		public static String Format(String format, Object arg0, Object arg1) {
			return Format(null, format, new Object[] {arg0, arg1});
		}

		public static String Format(String format, Object arg0, Object arg1, Object arg2) {
			return Format(null, format, new Object[] {arg0, arg1, arg2});
		}

		public static string Format (string format, params object[] args) {
			return Format (null, format, args);
		}
	
		public static string Format (IFormatProvider provider, string format, params object[] args) {
			StringBuilder b = new StringBuilder ();
			FormatHelper (b, provider, format, args);
			return b.ToString ();
		}
		
		internal static void FormatHelper (StringBuilder result, IFormatProvider provider, string format, params object[] args) {
			if (format == null || args == null)
				throw new ArgumentNullException ();
			
			int ptr = 0;
			int start = ptr;
			while (ptr < format.length) {
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

					if (width > str.length) {
						string pad = new String (' ', width - str.length);

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
				else if (c == '}' && ptr < format.length && format[ptr] == '}') {
					result.Append (format, start, ptr - start - 1);
					start = ptr ++;
				}
				else if (c == '}') {
					throw new FormatException ("Input string was not in a correct format.");
				}
			}

			if (start < format.length)
				result.Append (format.Substring (start));
		}

		public static String Copy (String str) {
			if (str == null)
				throw new ArgumentNullException ();

			int length = str.length;

			String tmp = InternalAllocateStr(length);
			InternalStrcpy(tmp, 0, str);
			return tmp;
		}

		public static String Concat(Object obj) {
			if (null == obj)
				return String.Empty;

			return obj.ToString();
		}

		public static String Concat(Object obj1, Object obj2)
		{
			string s1, s2;

			s1 = (obj1 != null) ? obj1.ToString () : null;
			s2 = (obj2 != null) ? obj2.ToString () : null;
			
			if (s1 == null) {
				if (s2 == null)
					return String.Empty;
				else
					return s2;
			} else if (s2 == null)
				return s1;

			String tmp = InternalAllocateStr (s1.Length + s2.Length);
			InternalStrcpy (tmp, 0, s1);
			InternalStrcpy (tmp, s1.length, s2);

			return tmp;
		}

		public static String Concat(Object obj1, Object obj2, Object obj3)
		{
			string s1, s2, s3;
			if (obj1 == null)
				s1 = String.Empty;
			else
				s1 = obj1.ToString ();
    
			if (obj2 == null)
				s2 = String.Empty;
			else
				s2 = obj2.ToString ();
    
			if (obj3 == null)
				s3 = String.Empty;
			else
				s3 = obj3.ToString ();
    
			return Concat (s1, s2, s3);
		}

		public static String Concat (Object obj1, Object obj2, Object obj3, Object obj4)
		{
			string s1, s2, s3, s4;

			if (obj1 == null)
				s1 = String.Empty;
			else
				s1 = obj1.ToString ();
    
			if (obj2 == null)
				s2 = String.Empty;
			else
				s2 = obj2.ToString ();
    
			if (obj3 == null)
				s3 = String.Empty;
			else
				s3 = obj3.ToString ();

			if (obj4 == null)
				s4 = String.Empty;
			else
				s4 = obj4.ToString ();
			
			return Concat (s1, s2, s3, s4);
			
		}

		public static String Concat(String s1, String s2)
		{
			if (s1 == null) {
				if (s2 == null)
					return String.Empty;
				return s2;
			}

			if (s2 == null)
				return s1; 

			String tmp = InternalAllocateStr(s1.length + s2.length);
            
			InternalStrcpy(tmp, 0, s1);
			InternalStrcpy(tmp, s1.length, s2);
            
			return tmp;
		}

		public static String Concat(String s1, String s2, String s3)
		{
			if (s1 == null){
				if (s2 == null){
					if (s3 == null)
						return String.Empty;
					return s3;
				} else {
					if (s3 == null)
						return s2;
				}
				s1 = String.Empty;
			} else {
				if (s2 == null){
					if (s3 == null)
						return s1;
					else
						s2 = String.Empty;
				} else {
					if (s3 == null)
						s3 = String.Empty;
				}
			}

			//return InternalConcat (s1, s2, s3);
			String tmp = InternalAllocateStr(s1.length + s2.length + s3.length);

			InternalStrcpy(tmp, 0, s1);
			InternalStrcpy(tmp, s1.length, s2);
			InternalStrcpy(tmp, s1.length + s2.length, s3);

			return tmp;
		}

		public static String Concat(String s1, String s2, String s3, String s4) {
			if (null == s1 && null == s2 && null == s3 && null == s4) {
				return String.Empty;
			}

			if (null == s1) { s1 = String.Empty; }
			if (null == s2) { s2 = String.Empty; }
			if (null == s3) { s3 = String.Empty; }
			if (null == s4) { s4 = String.Empty; }

			String tmp = InternalAllocateStr(s1.length + s2.length + s3.length + s4.length);

			InternalStrcpy(tmp, 0, s1);
			InternalStrcpy(tmp, s1.length, s2);
			InternalStrcpy(tmp, s1.length + s2.length, s3);
			InternalStrcpy(tmp, s1.length + s2.length + s3.length, s4);

			return tmp;
		}

		public static String Concat(params Object[] args) {
			string [] strings;
			int len, i, currentpos;

			if (null == args)
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

			currentpos = 0;

			String tmp = InternalAllocateStr(len);
			for (i = 0; i < strings.Length; i++) {
				InternalStrcpy(tmp, currentpos, strings[i]);
				currentpos += strings[i].length;
			}

			return tmp;
		}

		public static String Concat(params String[] values) {
			int len, i, currentpos;

			if (values == null)
				throw new ArgumentNullException ();

			len = 0;
			foreach (string value in values)
				len += value != null ? value.length : 0;

			if (len == 0)
				return String.Empty;

			currentpos = 0;

			String tmp = InternalAllocateStr(len);
			for (i = 0; i < values.Length; i++) {
				if (values[i] == null)
					continue;

				InternalStrcpy(tmp, currentpos, values[i]);
				currentpos += values[i].length;
			}	
	
			return tmp;
		}

		public String Insert(int sindex, String value) {
			if (null == value)
				throw new ArgumentNullException();

			if (sindex < 0 || sindex > this.length)
				throw new ArgumentOutOfRangeException();
	
			return InternalInsert(sindex, value);
		}


		public static string Intern (string str) {
			if (null == str)
				throw new ArgumentNullException ();

			return InternalIntern(str);
		}

		public static string IsInterned (string str) {
			if (null == str)
				throw new ArgumentNullException();

			return InternalIsInterned(str);
		}
	
		public static string Join (string separator, string [] value) {
			if (value == null)
				throw new ArgumentNullException ();

			return Join(separator, value, 0, value.Length);
		}

		public static string Join(string separator, string[] value, int sindex, int count) {
			if (value == null)
				throw new ArgumentNullException ();

			if (sindex + count > value.Length)
				throw new ArgumentOutOfRangeException ();

			if (sindex == value.Length)
				return String.Empty;

			return InternalJoin(separator, value, sindex, count);
		}

		bool IConvertible.ToBoolean (IFormatProvider provider) {
			return Convert.ToBoolean (this);
		}
		
		byte IConvertible.ToByte (IFormatProvider provider) {
			return Convert.ToByte (this);
		}
		
		char IConvertible.ToChar (IFormatProvider provider) {
			return Convert.ToChar (this);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider) {
			return Convert.ToDateTime (this);
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider) {
			return Convert.ToDecimal (this);
		}

		double IConvertible.ToDouble (IFormatProvider provider) {
			return Convert.ToDouble (this);
		}

		short IConvertible.ToInt16 (IFormatProvider provider) {
			return Convert.ToInt16 (this);
		}

		int IConvertible.ToInt32 (IFormatProvider provider) {
			return Convert.ToInt32 (this);
		}

		long IConvertible.ToInt64 (IFormatProvider provider) {
			return Convert.ToInt64 (this);
		}
	
		[CLSCompliant(false)]
		sbyte IConvertible.ToSByte (IFormatProvider provider) {
			return Convert.ToSByte (this);
		}

		float IConvertible.ToSingle (IFormatProvider provider) {
			return Convert.ToSingle (this);
		}
		string IConvertible.ToString (IFormatProvider format) {
			return this;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider) {
			return Convert.ToType (this, conversionType,  provider);
		}

		[CLSCompliant(false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider) {
			return Convert.ToUInt16 (this);
		}

		[CLSCompliant(false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider) {
			return Convert.ToUInt32 (this);
		}

		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider) {
			return Convert.ToUInt64 (this);
		}

		TypeCode IConvertible.GetTypeCode () {
			return TypeCode.String;
		}

		public int Length {
			get {
				return length;
			}
		}

		public CharEnumerator GetEnumerator () {
			return new CharEnumerator (this);
		}
		
		IEnumerator IEnumerable.GetEnumerator () {
			return new CharEnumerator (this);
		}

		private static void ParseFormatSpecifier (string str, ref int ptr, out int n, out int width, out bool left_align, out string format) {
			// parses format specifier of form:
			//   N,[\ +[-]M][:F]}
			//
			// where:

			try {
				// N = argument number (non-negative integer)
			
				n = ParseDecimal (str, ref ptr);
				if (n < 0)
					throw new FormatException ("Input string was not in a correct format.");
				
				// M = width (non-negative integer)

				if (str[ptr] == ',') {
					// White space between ',' and number or sign.
					int start = ++ptr;
					while (Char.IsWhiteSpace (str [ptr]))
						++ptr;

					format = str.Substring (start, ptr - start);

					left_align = (str [ptr] == '-');
					if (left_align)
						++ ptr;

					width = ParseDecimal (str, ref ptr);
					if (width < 0)
						throw new FormatException ("Input string was not in a correct format.");
				}
				else {
					width = 0;
					left_align = false;
					format = "";
				}

				// F = argument format (string)

				if (str[ptr] == ':') {
					int start = ++ ptr;
					while (str[ptr] != '}')
						++ ptr;

					format += str.Substring (start, ptr - start);
				}
				else
					format = null;

				if (str[ptr ++] != '}')
					throw new FormatException ("Input string was not in a correct format.");
			}
			catch (IndexOutOfRangeException) {
				throw new FormatException ("Input string was not in a correct format.");
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

		internal unsafe void InternalSetChar(int idx, char val) 
		{
			if ((uint) idx >= (uint) Length)
				throw new ArgumentOutOfRangeException("idx");

			fixed (char * pStr = &start_char) 
			{
				pStr [idx] = val;
			}
		}

		internal unsafe void InternalSetLength(int newLength) 
		{
			if (newLength > length)
				throw new ArgumentOutOfRangeException("newLength > length");
			
			length = newLength;
			
			// zero terminate, we can pass string objects directly via pinvoke
			fixed (char * pStr = &start_char) {
				pStr [length] = '\0';
			}
		}

		[CLSCompliant(false), MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe public extern String(char *value);

		[CLSCompliant(false), MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe public extern String(char *value, int sindex, int length);
    
		[CLSCompliant(false), MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe public extern String(sbyte *value);

		[CLSCompliant(false), MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe public extern String(sbyte *value, int sindex, int length);

		[CLSCompliant(false), MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe public extern String(sbyte *value, int sindex, int length, Encoding enc);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern String(char [] val, int sindex, int length);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern String(char [] val);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern String(char c, int count);
	
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override int GetHashCode();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string InternalJoin(string separator, string[] value, int sindex, int count);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalInsert(int sindex, String value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalReplace(char oldChar, char newChar);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalReplace(String oldValue, string newValue, CompareInfo comp);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalRemove(int sindex, int count);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void InternalCopyTo(int sindex, char[] dest, int dindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String[] InternalSplit(char[] separator, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalTrim(char[] chars, int typ);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern int InternalIndexOfAny(char [] arr, int sindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern int InternalLastIndexOfAny(char [] anyOf, int sindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalPad(int width, char chr, bool right);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalToLower(CultureInfo culture);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalToUpper(CultureInfo culture);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static String InternalAllocateStr(int length);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static void InternalStrcpy(String dest, int destPos, String src);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static void InternalStrcpy(String dest, int destPos, String src, int startPos, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string InternalIntern(string str);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string InternalIsInterned(string str);
	}
}
