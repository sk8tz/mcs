//
// System.String.cs
//
// Authors:
//	  Patrik Torstensson (patrik.torstensson@labs2.com)
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
	public sealed class String : IConvertible, IComparable, ICloneable, IEnumerable {
		private char[] c_str;
		private int length;

		public static readonly String Empty = "";

		// New constructors (to use when we change the internal structure of a string 
		/*
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
		
		*/

		// Constructors (old one's from the old String.cs)
		internal String (int storage) {
			if (storage < 0)
				throw new ArgumentOutOfRangeException ();
			length = storage;
			c_str = new char [storage];
		}
		
		[CLSCompliant(false)]
		unsafe public String (char *value) {
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

		public String (char[] value) {
			int i;

			// FIXME: value.Length includes the terminating null char?
			this.length = value != null ? value.Length : 0;
			this.c_str = new char [this.length + 1];
			for (i = 0; i < this.length; i++)
				this.c_str[i] = value[i];
		}

		[CLSCompliant(false)]
		unsafe public String (sbyte *value) {
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

		public String (char c, int count) {
			int i;

			this.length = count;
			this.c_str = new char [count + 1];
			for (i = 0; i < count; i++)
				this.c_str[i] = c;
		}

		[CLSCompliant(false)]
		unsafe public String (char *value, int startIndex, int length) {
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

		public String (char[] value, int startIndex, int length) {
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
		unsafe public String (sbyte *value, int startIndex, int length) {
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override int GetHashCode();

		public static bool Equals(String str1, String str2) {
			if ((Object) str1 == (Object) str2)
				return true;
	    
			if (null == (Object) str1 || null == (Object) str2)
				return false;
			if (str1.length != str2.length)
				return false;
    
			return Compare(str1, 0, str2, 0, str1.length, false) == 0;
		}

		public static bool operator == (String str1, String str2) {
			return Equals(str1, str2);
		}

		public static bool operator != (String str1, String str2) {
			return !Equals(str1, str2);
		}

		public override bool Equals(Object obj) {
			if (null == obj)
				return false;

			if (!(obj is String))
				return false;

			return Compare (this, (String)obj, false) == 0;
		}

		public bool Equals(String value) {
			if (null == value)
				return false;

			if (length != value.length)
				return false;
			return Compare (this, 0, value, 0, length, false) == 0;
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
			if (null == separator) {
				separator = WhiteChars;
			}

			if (count < 0)
				throw new ArgumentOutOfRangeException ();

			if (count == 0) 
				return new String[1] { ToString() };

			return InternalSplit(separator, count);
		}

		public String Substring (int sindex) {
			if (sindex < 0 || sindex > this.length) {
				Console.WriteLine("Substring({0}) = {2}\n", sindex, this.length, this);
				throw new ArgumentOutOfRangeException();
			}

			string tmp = InternalAllocateStr(this.length - sindex);
			InternalStrcpy(tmp, 0, this, sindex, length - sindex);
			
			return tmp;
		}

		public String Substring (int sindex, int length) {
			if (length < 0 || sindex < 0 || sindex + length > this.length) {
				Console.WriteLine("Substring({0}, {1}) = {2}\n", sindex, length, this.ToString());
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
			return Compare(s1, s2, false);
		}

		public static int Compare(String s1, String s2, bool inCase) {
			if (null == s1) {
				if (null == s2)
					return 0;
				else
					return -1;
			} else if (null == s2)
				return 1;

			return InternalCompare(s1, 0, s2, 0, Math.Max(s1.length, s2.length), inCase);
		}
		
		[MonoTODO()]
		public static int Compare(String s1, String s2, bool inCase, CultureInfo culture) {
			return Compare(s1, s2, inCase);
		}

		public static int Compare(String s1, int i1, String s2, int i2, int length) {
			return Compare(s1, i1, s2, i2, length, false);
		}

		public static int Compare(String s1, int i1, String s2, int i2, int length, bool inCase) {
			if (null == s1) {
				if (null == s2)
					return 0;
				else
					return -1;
			} else if (null == s2)
				return 1;

			if (length < 0 || i1 < 0 || i2 < 0)
				throw new ArgumentOutOfRangeException ();

			if (i1 > s1.length || i2 > s2.length)
				throw new ArgumentOutOfRangeException ();

			if (length == 0)
				return 0;

			return InternalCompare(s1, i1, s2, i2, length, inCase);
		}

		[MonoTODO()]
		public static int Compare(String s1, int i1, String s2, int i2, int length, bool inCase, CultureInfo culture) {
			return Compare(s1, i1, s2, i2, length, inCase);
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
			if (null == s1 || null == s2) {
				if ((Object)s1 == (Object) s2) {
					return 0;
				}

				return (s1 == null) ? -1 : 1;
			}

			return InternalCompare(s1, 0, s2, 0, Math.Max(s1.length, s2.length), false);
		}

		public static int CompareOrdinal(String s1, int i1, String s2, int i2, int length) {
			if (null == s1 || null == s2) {
				if ((Object)s1 == (Object) s2) {
					return 0;
				}

				return (s1 == null) ? -1 : 1;
			}

			if (i1 < 0 || i2 < 0 || length < 0)
				throw new ArgumentOutOfRangeException ();

			if (i1 > s1.length || i2 > s2.length)
				throw new ArgumentOutOfRangeException ();

			return InternalCompare(s1, i1, s2, i2, length, false);
		}

		public bool EndsWith(String value) {
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
			return InternalIndexOfAny(arr, sindex, this.length - sindex);
		}

		public int IndexOfAny(char [] arr, int sindex, int count) {
			return InternalIndexOfAny(arr, sindex, count);
		}

		public int IndexOf(char value) {
			return InternalIndexOf(value, 0, this.length);
		}

		public int IndexOf(String value) {
			return IndexOf(value, 0, this.length);
		}

		public int IndexOf(char value, int sindex) {
			if (sindex < 0 || sindex >= this.length) {
				Console.WriteLine("value={0} sindex={0}\n", value, sindex);
				throw new ArgumentOutOfRangeException();
			}

			return InternalIndexOf(value, sindex, this.length - sindex);
		}

		public int IndexOf(String value, int sindex) {
			return IndexOf(value, sindex, value.length - sindex);
		}

		public int IndexOf(char value, int sindex, int count) {
			if (sindex < 0 || count < 0 || sindex + count > this.length)
				throw new ArgumentOutOfRangeException ();
			
			if (sindex == 0 && this.length == 0)
				return -1;			

			return InternalIndexOf(value, sindex, count);
		}
		
		public int IndexOf(String value, int sindex, int count) {
			if (null == value) 
				throw new ArgumentNullException();

			if (sindex < 0 || count < 0 || sindex + count > this.length)
				throw new ArgumentOutOfRangeException ();
			
			if (sindex == 0 && this.length == 0)
				return -1;			

			return InternalIndexOf(value, sindex, count);
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
			return InternalLastIndexOf(value, this.length - 1, this.length);
		}

		public int LastIndexOf(String value) {
			if (null == value) 
				throw new ArgumentNullException();

			return InternalLastIndexOf(value, this.length - 1, this.length);
		}

		public int LastIndexOf(char value, int sindex){
			return LastIndexOf(value, sindex, sindex + 1);
		}

		public int LastIndexOf(String value, int sindex) {
			return LastIndexOf(value, sindex, sindex + 1);
		}

		public int LastIndexOf(char value, int sindex, int count) {
			if (count < 0 || sindex < 0)
				throw new ArgumentOutOfRangeException ();

			if (count > this.length || sindex > this.length)
				throw new ArgumentOutOfRangeException ();

			if (sindex == 0 && this.length == 0)
				return -1;

			return InternalLastIndexOf(value, sindex, count);
		}

		public int LastIndexOf(String value, int sindex, int count) {
			if (null == value) 
				throw new ArgumentNullException();

			if (sindex < 0 || sindex > this.length)
				throw new ArgumentOutOfRangeException ();

			if (count < 0 || count - sindex <= 0)
				throw new ArgumentOutOfRangeException ();

			if (sindex == 0 && this.length == 0)
				return -1;

			return InternalLastIndexOf(value, sindex, count);
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
			if (this.length < value.length)
				return false;

			return (0 == Compare(this, 0, value, 0 , value.length));
		}
	
    
		public String Replace (char oldChar, char newChar) {
			return InternalReplace(oldChar, newChar);
		}

		public String Replace(String oldValue, String newValue) {
			if (null == oldValue)
				throw new ArgumentNullException();

			return InternalReplace(oldValue, newValue);
		}

		public String Remove(int sindex, int count) {
			if (sindex < 0 || count < 0 || sindex + count > this.length)
				throw new ArgumentOutOfRangeException ();

			return InternalRemove(sindex, count);
		}

		public String ToLower() {
			return InternalToLower();
		}

		public String ToLower(CultureInfo culture) {
			throw new NotImplementedException();
		}

		public String ToUpper() {
			return InternalToUpper();
		}

		public String ToUpper(CultureInfo culture) {
			throw new NotImplementedException();
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
			if (format == null || args == null)
				throw new ArgumentNullException ();
		
			StringBuilder result = new StringBuilder ();

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
				else if (c == '}' && format[ptr] == '}') {
					result.Append (format, start, ptr - start - 1);
					start = ptr ++;
				}
			}

			if (start < format.length)
				result.Append (format.Substring (start));

			return result.ToString ();
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

		public static String Concat(Object obj1, Object obj2) {
			if (null == obj1)
				obj1 = String.Empty;
    
			if (null == obj2)
				obj2 = String.Empty;

			return Concat(obj1.ToString(), obj2.ToString());
		}

		public static String Concat(Object obj1, Object obj2, Object obj3) {
			if (null == obj1)
				obj1 = String.Empty;
    
			if (null == obj2)
				obj1 = String.Empty;
    
			if (null == obj3)
				obj1 = String.Empty;
    
			return Concat(obj1.ToString(), obj2.ToString(), obj3.ToString());
		}

		[CLSCompliant(false)]
		public static String Concat(Object arg0, Object arg1, Object arg2, Object arg3, __arglist)  {
			throw new NotImplementedException();
		}

		public static String Concat(String s1, String s2) {
			if (null == s1) {
				if (null == s2) { return String.Empty; }
				return s2;
			}

			if (null == s2) { return s1; }

			String tmp = InternalAllocateStr(s1.length + s2.length);
            
			InternalStrcpy(tmp, 0, s1);
			InternalStrcpy(tmp, s1.length, s2);
            
			return tmp;
		}

		public static String Concat(String s1, String s2, String s3) {
			if (null == s1 && null == s2 && null == s3) {
				return String.Empty;
			}

			if (null == s1) { s1 = String.Empty; }
			if (null == s2) { s2 = String.Empty; }
			if (null == s3) { s3 = String.Empty; }

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
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string InternalJoin(string separator, string[] value, int sindex, int count);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalInsert(int sindex, String value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalReplace(char oldChar, char newChar);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalReplace(String oldValue, String newValue);
    
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalRemove(int sindex, int count);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void InternalCopyTo(int sindex, char[] dest, int dindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String[] InternalSplit(char[] separator, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalTrim(char[] chars, int typ);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern int InternalIndexOf(char value, int sindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern int InternalIndexOf(string value, int sindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern int InternalIndexOfAny(char [] arr, int sindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern int InternalLastIndexOf(char value, int sindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern int InternalLastIndexOf(String value, int sindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern int InternalLastIndexOfAny(char [] anyOf, int sindex, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalPad(int width, char chr, bool right);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalToLower();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern String InternalToUpper();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static String InternalAllocateStr(int length);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void InternalStrcpy(String dest, int destPos, String src);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void InternalStrcpy(String dest, int destPos, String src, int startPos, int count);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string InternalIntern(string str);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string InternalIsInterned(string str);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int InternalCompare(String s1, int i1, String s2, int i2, int length, bool inCase);
	}
}
