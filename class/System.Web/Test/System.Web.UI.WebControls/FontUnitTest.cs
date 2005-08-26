//
// Tests for System.Web.UI.WebControls.FontUnit.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
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

using NUnit.Framework;
using System;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class FontUnitTest {

		[Test]
		public void FontUnitConstructors ()
		{
			FontUnit f1 = new FontUnit (FontSize.Large);
			
			Assert.AreEqual (f1.Type, FontSize.Large, "A1");
			Assert.AreEqual (f1.Unit, Unit.Empty, "A1.1");
			
			// Test the AsUnit values
			f1 = new FontUnit (FontSize.AsUnit);
			Assert.AreEqual (f1.Type, FontSize.AsUnit, "A2");
			Assert.AreEqual (f1.Unit.Type, UnitType.Point, "A3");
			Assert.AreEqual (f1.Unit.Value, 10, "A4");

			f1 = new FontUnit (15);
			Assert.AreEqual (f1.Type, FontSize.AsUnit, "A5");
			Assert.AreEqual (f1.Unit.Type, UnitType.Point, "A6");
			Assert.AreEqual (f1.Unit.Value, 15, "A7");

			// Test the string constructor: null and empty
			f1 = new FontUnit (null);
			Assert.AreEqual (f1.Type, FontSize.NotSet, "A8");
			Assert.AreEqual (f1.Unit.IsEmpty, true, "A9");

			f1 = new FontUnit ("");
			Assert.AreEqual (f1.Type, FontSize.NotSet, "A10");
			Assert.AreEqual (f1.Unit.IsEmpty, true, "A11");

#if NET_2_0
			f1 = new FontUnit (2.5);
			Assert.AreEqual (f1.Type, FontSize.AsUnit, "A12");
			Assert.AreEqual (f1.Unit.Type, UnitType.Point, "A13");
			Assert.AreEqual (f1.Unit.Value, 2.5, "A14");

			f1 = new FontUnit (5.0, UnitType.Percentage);
			Assert.AreEqual (f1.Type, FontSize.AsUnit, "A15");
			Assert.AreEqual (f1.Unit.Type, UnitType.Percentage, "A17");
			Assert.AreEqual (f1.Unit.Value, 5.0, "A18");
#endif
		}

		[Test]
		public void FontUnitConstructors_Pixel ()
		{
			FontUnit f1 = new FontUnit ("10px");
			Assert.AreEqual (FontSize.AsUnit, f1.Type, "A12");
			Assert.AreEqual (UnitType.Pixel, f1.Unit.Type, "A13");
			Assert.AreEqual (10, f1.Unit.Value, "A14");
			Assert.AreEqual ("10px", f1.ToString (), "A15");
		}

		[Test]
		public void FontUnitConstructors_Point ()
		{
			FontUnit f1 = new FontUnit ("12pt");
			Assert.AreEqual (FontSize.AsUnit, f1.Type, "Type");
			Assert.AreEqual (UnitType.Point, f1.Unit.Type, "Unit.Type");
			Assert.AreEqual (12, f1.Unit.Value, "Unit.Value");
			Assert.AreEqual ("12pt", f1.ToString (), "ToString");
		}

		[Test]
		[Category ("NotWorking")] // X* ToString
		public void FontUnitConstructors_Enum ()
		{
			// All the enumeration values
			FontUnit fu = new FontUnit ("Large");
			Assert.AreEqual (FontSize.Large, fu.Type, "Large");
			Assert.IsTrue (fu.Unit.IsEmpty, "Large.IsEmpty");
			Assert.AreEqual ("Large", fu.ToString (), "Large.ToString");

			fu = new FontUnit ("Larger");
			Assert.AreEqual (FontSize.Larger, fu.Type, "Larger");
			Assert.IsTrue (fu.Unit.IsEmpty, "Larger.IsEmpty");
			Assert.AreEqual ("Larger", fu.ToString (), "Larger.ToString");

			fu = new FontUnit ("Medium");
			Assert.AreEqual (FontSize.Medium, fu.Type, "Medium");
			Assert.IsTrue (fu.Unit.IsEmpty, "Medium.IsEmpty");
			Assert.AreEqual ("Medium", fu.ToString (), "Medium.ToString");

			fu = new FontUnit ("Small");
			Assert.AreEqual (FontSize.Small, fu.Type, "Small");
			Assert.IsTrue (fu.Unit.IsEmpty, "Small.IsEmpty");
			Assert.AreEqual ("Small", fu.ToString (), "Small.ToString");

			fu = new FontUnit ("Smaller");
			Assert.AreEqual (FontSize.Smaller, fu.Type, "Smaller");
			Assert.IsTrue (fu.Unit.IsEmpty, "Smaller.IsEmpty");
			Assert.AreEqual ("Smaller", fu.ToString (), "Smaller.ToString");

			fu = new FontUnit ("XLarge");
			Assert.AreEqual (FontSize.XLarge, fu.Type, "XLarge");
			Assert.IsTrue (fu.Unit.IsEmpty, "XLarge.IsEmpty");
			Assert.AreEqual ("X-Large", fu.ToString (), "XLarge.ToString");

			fu = new FontUnit ("XSmall");
			Assert.AreEqual (FontSize.XSmall, fu.Type, "XSmall");
			Assert.IsTrue (fu.Unit.IsEmpty, "XSmall.IsEmpty");
			Assert.AreEqual ("X-Small", fu.ToString (), "XSmall.ToString");

			fu = new FontUnit ("XXLarge");
			Assert.AreEqual (FontSize.XXLarge, fu.Type, "XXLarge");
			Assert.IsTrue (fu.Unit.IsEmpty, "XXLarge.IsEmpty");
			Assert.AreEqual ("XX-Large", fu.ToString (), "XXLarge.ToString");

			fu = new FontUnit ("XXSmall");
			Assert.AreEqual (FontSize.XXSmall, fu.Type, "XXSmall");
			Assert.IsTrue (fu.Unit.IsEmpty, "XXSmall.IsEmpty");
			Assert.AreEqual ("XX-Small", fu.ToString (), "XXSmall.ToString");
		}

		[Test]
		public void UnitEquality ()
		{
			FontUnit u1 = new FontUnit ("1px");
			FontUnit u2 = new FontUnit ("2px");
			FontUnit t1 = new FontUnit ("1px");
			FontUnit c2 = new FontUnit ("2cm");

			Assert.AreEqual (u1 == t1, true, "U1");
			Assert.AreEqual (u1 != u2, true, "U2");
			Assert.AreEqual (u1 == u2, false, "U3");
			Assert.AreEqual (u1 != t1, false, "U4");

			// Test that its comparing the units and value
			Assert.AreEqual (u2 != c2, true, "U5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor ()
		{
			FontUnit a = new FontUnit ((FontSize) (-1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IncorrectConstructor2 ()
		{
			FontUnit a = new FontUnit ((FontSize) (FontSize.XXLarge + 1));
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		[Category ("NotWorking")] // wrong exception
		public void FontUnitConstructors_Enum_AsUnit ()
		{
			new FontUnit ("AsUnit");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		[Category ("NotWorking")] // wrong exception
		public void FontUnitConstructors_Enum_NotSet ()
		{
			new FontUnit ("NotSet");
		}

#if NET_2_0
		class MyFormatProvider : IFormatProvider
		{
			public object GetFormat (Type format_type)
			{
				return Activator.CreateInstance (format_type);
			}
		}

		[Test]
		public void FontUnit_IFormatProviderToString ()
		{
			MyFormatProvider mfp = new MyFormatProvider ();

			FontUnit f1 = new FontUnit (FontSize.Large);
			Assert.AreEqual ("Large", f1.ToString (mfp), "T1");

			f1 = new FontUnit (FontSize.AsUnit);
			Assert.AreEqual ("10pt", f1.ToString (mfp), "T2");

			f1 = new FontUnit (15);
			Assert.AreEqual ("15pt", f1.ToString (mfp), "T3");

			f1 = new FontUnit (null);
			Assert.AreEqual ("", f1.ToString (mfp), "T4");

			f1 = new FontUnit ("");
			Assert.AreEqual ("", f1.ToString (mfp), "T5");

			f1 = new FontUnit (2.5);
			Assert.AreEqual ("2.5pt", f1.ToString (mfp), "T6");

			f1 = new FontUnit (5.0, UnitType.Percentage);
			Assert.AreEqual ("5%", f1.ToString (mfp), "T7");
		}
#endif
	}
}
