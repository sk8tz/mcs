//
// Tests for System.Web.UI.MinimizableAttributeTypeConvert.cs 
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Reflection;
using NUnit.Framework;

#if NET_2_0
namespace MonoTests.System.Web.UI {

	[TestFixture]
	public class MinimizableAttributeTypeConverterTest {
		private TypeConverter GetTypeConverter ()
		{
			Type t = typeof (HtmlTableCell);
			PropertyInfo[] props;
			PropertyInfo pi = null;
			TypeConverter tc;

			props = t.GetProperties();

			foreach (PropertyInfo p in props) {
				if (p.Name == "NoWrap") {
					pi = p;
					break;
				}
			}

			object[] attrs = pi.GetCustomAttributes (typeof (TypeConverterAttribute), false);
			TypeConverterAttribute tca = (TypeConverterAttribute)attrs[0];

			Type tct = Type.GetType (tca.ConverterTypeName);
			tc = (TypeConverter)Activator.CreateInstance (tct);

			return tc;
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		public void CanConvertFrom ()
		{
			TypeConverter tc = GetTypeConverter ();

			Assert.IsFalse (tc.CanConvertFrom (typeof (bool)), "A1");
			Assert.IsTrue (tc.CanConvertFrom (typeof (string)), "A2");
			Assert.IsTrue (tc.CanConvertFrom (typeof (InstanceDescriptor)), "A3");
		}

		[Test]
		public void CanConvertTo ()
		{
			TypeConverter tc = GetTypeConverter ();

			Assert.IsFalse (tc.CanConvertTo (typeof (bool)), "A1");
			Assert.IsTrue (tc.CanConvertTo (typeof (string)), "A2");
			Assert.IsFalse (tc.CanConvertTo (typeof (InstanceDescriptor)), "A3");
		}

		[Test]
		public void ConvertFrom ()
		{
			TypeConverter tc = GetTypeConverter ();

			Assert.AreEqual ("hi", tc.ConvertTo ("hi", typeof (string)), "A1");
			Assert.AreEqual ("", tc.ConvertTo ("", typeof (string)), "A2");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromNull ()
		{
			TypeConverter tc = GetTypeConverter ();

			tc.ConvertFrom (null);
		}


		[Test]
		public void ConvertTo ()
		{
			TypeConverter tc = GetTypeConverter ();

			Assert.AreEqual ("hi", tc.ConvertTo ("hi", typeof (string)), "A1");
			Assert.AreEqual ("", tc.ConvertTo ("", typeof (string)), "A2");
			Assert.AreEqual ("", tc.ConvertTo (null, typeof (string)), "A3");
			Assert.AreEqual ("False", tc.ConvertTo (false, typeof (string)), "A4");
			Assert.AreEqual ("True", tc.ConvertTo (true, typeof (string)), "A5");
		}
	}
}
#endif
