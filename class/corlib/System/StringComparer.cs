//
// System.StringComparer
//
// Authors:
//	Marek Safar (marek.safar@seznam.cz)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable, ComVisible(true)]
	public abstract class StringComparer : IComparer, IEqualityComparer
	{
		class StringCultureComparer: StringComparer
		{
			CompareOptions co;
			CompareInfo ci;

			public StringCultureComparer (CultureInfo ci, bool ignore_case)
			{
				this.ci = ci.CompareInfo;
				co = ignore_case ? CompareOptions.IgnoreCase : CompareOptions.None;
			}

			public override int Compare (string x, string y)
			{
				return ci.Compare (x, y, co);
			}

			public override bool Equals (string x, string y)
			{
				return Compare (x, y) == 0;
			}

			[MonoTODO]
			public override int GetHashCode (string s)
			{
				if (s == null)
					throw new ArgumentNullException("s");

				return 0;
				// TODO:
				//return ci.GetHashCode (s);
			}
		}

		static StringComparer invariantCultureIgnoreCase = new StringCultureComparer (CultureInfo.InvariantCulture, true);
		static StringComparer invariantCulture = new StringCultureComparer (CultureInfo.InvariantCulture, false);

		// Constructors
		protected StringComparer ()
		{
		}

		// Properties
		public static StringComparer CurrentCulture {
			get {
				return new StringCultureComparer (CultureInfo.CurrentCulture, false);
			}
		}

		public static StringComparer CurrentCultureIgnoreCase {
			get {
				return new StringCultureComparer (CultureInfo.CurrentCulture, true);
			}
		}

		public static StringComparer InvariantCulture {
			get {
				return invariantCulture;
			}
		}

		public static StringComparer InvariantCultureIgnoreCase {
			get {
				return invariantCultureIgnoreCase;
			}
		}

		// Methods
		public int Compare (object x, object y)
		{
			if (x == y)
				return 0;
			if (x == null)
				return -1;
			if (y == null)
				return 1;

			string s_x = x as string;
			if (s_x != null) {
				string s_y = y as string;
				if (s_y != null)
					return Compare (s_x, s_y);
			}

			IComparable ic = x as IComparable;
			if (ic == null)
				throw new ArgumentException ();

			return ic.CompareTo (y);
		}

		public new bool Equals (object x, object y)
		{
			if (x == y)
				return true;
			if (x == null || y == null)
				return false;

			string s_x = x as string;
			if (s_x != null) {
				string s_y = y as string;
				if (s_y != null)
					return Equals (s_x, s_y);
			}
			return x.Equals (y);
		}

		public int GetHashCode (object o)
		{
			if (o == null)
				throw new ArgumentNullException("o");

			string s = o as string;
			return s == null ? o.GetHashCode (): GetHashCode(s);
		}

		public abstract int Compare (string x, string y);
		public abstract bool Equals (string x, string y);
		public abstract int GetHashCode (string s);
	}
}

#endif
