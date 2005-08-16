//
// System.Drawing.ColorConverter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Ravindra (rkumar@novell.com)
//
// Copyright (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell, Inc.  http://www.novell.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace System.Drawing
{
	public class ColorConverter : TypeConverter
	{
		static StandardValuesCollection cached;
		static object creatingCached = new object ();

		public ColorConverter () { }

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (InstanceDescriptor))
				return true;

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			string s = value as string;
			if (s == null)
				return base.ConvertFrom (context, culture, value);

			s = s.Trim ();

			if (s.Length == 0) {
				return Color.Empty;
			}

			object named = Color.NamedColors [s];
			if (named != null)
				return (Color) named;

			named = Color.SystemColors [s];
			if (named != null)
				return (Color) named;

			String numSeparator = culture.TextInfo.ListSeparator;

			int A, R, G, B;
			if (s.IndexOf (numSeparator) > 0) { // "A, R, G, B" format
				String [] components = s.Split (numSeparator.ToCharArray ());
				if (components.Length == 3) {
					A = 255;
					R = GetNumber (components [0].Trim ());
					G = GetNumber (components [1].Trim ());
					B = GetNumber (components [2].Trim ());
				}
				else if (components.Length == 4) {
					A = GetNumber (components [0].Trim ());
					R = GetNumber (components [1].Trim ());
					G = GetNumber (components [2].Trim ());
					B = GetNumber (components [3].Trim ());
				}
				else
					throw new ArgumentException (s + " is not a valid color value.");
			} 
			else { // #RRGGBB format
				int i = GetNumber (s.Trim ());
				A = (int) (i >> 24) & 0xFF;
				if (A == 0)
					A = 255;
				R = (i >> 16) & 0xFF;
				G = (i >> 8) & 0xFF;
				B = i & 0xFF;
			}

			Color result = Color.FromArgb (A, R, G, B);
			// Look for a named or system color with those values
			foreach (Color c in Color.NamedColors.Values) {
				if (c == result)
					return c;
			}

			foreach (Color c in Color.SystemColors.Values) {
				if (c == result)
					return c;
			}

			return result;
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if ((destinationType == typeof (string)) && (value is Color)) {
				Color color = (Color) value;

				if (color == Color.Empty) {
					return string.Empty;
				}

				if (color.IsKnownColor) {
					return color.Name;
				}

				if (color.IsNamedColor)
					return color.Name;

				String numSeparator = culture.TextInfo.ListSeparator;

				StringBuilder sb = new StringBuilder ();
				if (color.A != 255) {
					sb.Append (color.A);
					sb.Append (numSeparator);
					sb.Append (" ");
				}
				sb.Append (color.R);
				sb.Append (numSeparator);
				sb.Append (" ");

				sb.Append (color.G);
				sb.Append (numSeparator);
				sb.Append (" ");

				sb.Append (color.B);
				return sb.ToString ();
			}
			
			if (destinationType == typeof (InstanceDescriptor) && value is Color) {
				Color c = (Color)value;
				if (c.IsKnownColor){
					return new InstanceDescriptor (typeof (SystemColors).GetProperty (c.Name), null);
				} else {
					MethodInfo met = typeof(Color).GetMethod ("FromArgb", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) } );
					return new InstanceDescriptor (met, new object[] {c.A, c.R, c.G, c.B });
				}
			}

			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			if (cached != null)
				return cached;

			lock (creatingCached)
			{
				if (cached != null)
					return cached;
			
				ICollection named = (ICollection) Color.NamedColors.Values;
				ICollection system = (ICollection) Color.SystemColors.Values;
				Array colors = Array.CreateInstance (typeof (Color), named.Count + system.Count);
				named.CopyTo (colors, 0);
				system.CopyTo (colors, named.Count);
				Array.Sort (colors, 0, colors.Length, new CompareColors ());
				cached = new StandardValuesCollection (colors);
			}

			return cached;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		private int GetNumber (String str)
		{
			int number;
			try {
				if (str.StartsWith ("#0x") || str.StartsWith ("#0X")) 
					// #0xRRGGBB format. Parse hex string.
					number = Int32.Parse (str.Substring (3), NumberStyles.HexNumber);

				else if (str [0] == '#') 
					// #RRGGBB format. Parse hex string.
					number = Int32.Parse (str.Substring (1), NumberStyles.HexNumber);

				else if (str.StartsWith ("0x") || str.StartsWith ("0X"))
					// 0xRRGGBB format. Parse hex string.
					number = Int32.Parse (str.Substring (2), NumberStyles.HexNumber);

				else    // if (str [0] == '-' || str [0] == '+' || Char.IsDigit (str [0]))
					// [+/-]RRGGBB format. Parse decimal string.
					number = Int32.Parse (str, NumberStyles.Integer);

				return number;
			}
			catch (FormatException e) {
				throw new ArgumentException ("Can not convert this string to color: "+str, e);
			}
		}

		class CompareColors : IComparer
		{
			public int Compare (object x, object y)
			{
				return String.Compare (((Color) x).Name, ((Color) y).Name);
			}
		}
	}
}
