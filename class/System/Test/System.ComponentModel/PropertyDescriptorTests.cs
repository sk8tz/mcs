//
// System.ComponentModel.PropertyDescriptor test cases
//
// Authors:
// 	Chris Toshok (toshok@ximian.com)
//
// (c) 2006 Novell, Inc. (http://www.novell.com/)
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class PropertyDescriptorTests
	{
		class MissingConverterType_test
		{
			public class NestedClass { }

			[TypeConverter ("missing-type-name")]
			public NestedClass Prop {
				get { return null; }
			}

			[TypeConverter ("missing-type-name")]
			public int IntProp {
				get { return 5; }
			}

			[TypeConverter ("missing-type-name")]
			public string StringProp {
				get { return ""; }
			}
		}

		class ReadOnlyProperty_test
		{
			public int Prop {
				get { return 5; }
			}
		}

		class ReadOnlyAttribute_test
		{
			[ReadOnly (true)]
			public int Prop {
				get { return 5; }
				set { }
			}
		}

		class ConflictingReadOnly_test
		{
			[ReadOnly (false)]
			public int Prop {
				get { return 5; }
			}
		}

		class ShouldSerialize_public_test
		{
			public int Prop {
				get { return 5; }
			}

			public bool ShouldSerializeProp()
			{
				return false;
			}
		}

		class ShouldSerialize_protected_test
		{
			public int Prop {
				get { return 5; }
			}

			protected bool ShouldSerializeProp()
			{
				return false;
			}
		}

		class ShouldSerialize_private_test
		{
			public int Prop {
				get { return 5; }
			}

			private bool ShouldSerializeProp()
			{
				return false;
			}
		}

		class ShouldSerializeFalseEffectOnCanReset_test
		{
			public int Prop {
				get { return 5; }
				set { }
			}

			public bool ShouldSerializeProp()
			{
				return false;
			}

			public void ResetProp()
			{
			}
		}

		class NoSerializeOrResetProp_test
		{
			public int Prop {
				get { return 5; }
			}
		}

		class CanReset_public_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
				set { prop = value; }
			}

			public void ResetProp()
			{
				prop = 10;
			}
		}

		class CanReset_protected_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
				set { prop = value; }
			}

			protected void ResetProp()
			{
				prop = 10;
			}
		}

		class CanReset_private_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
				set { prop = value; }
			}

			private void ResetProp()
			{
				prop = 10;
			}
		}

		class CanResetNoSetter_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
			}

			private void ResetProp()
			{
				prop = 10;
			}
		}

		class DisplayName_test
		{
#if NET_2_0
			[DisplayName ("An explicit displayname")]
#endif
			public bool Explicit {
				get { return false; }
			}

			public bool Implicit {
				get { return false; }
			}
		}

		[Test]
		public void MissingTypeConverter ()
		{
			PropertyDescriptor p1 = TypeDescriptor.GetProperties (typeof (MissingConverterType_test))["Prop"];
			PropertyDescriptor p2 = TypeDescriptor.GetProperties (typeof (MissingConverterType_test))["IntProp"];
			PropertyDescriptor p3 = TypeDescriptor.GetProperties (typeof (MissingConverterType_test))["StringProp"];

			Assert.AreEqual (typeof (TypeConverter), p1.Converter.GetType (), "1");
			Assert.AreEqual (typeof (Int32Converter), p2.Converter.GetType (), "2");
			Assert.AreEqual (typeof (StringConverter), p3.Converter.GetType (), "3");
		}

		[Test]
		public void ShouldSerializeTest_public ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_public_test))["Prop"];
			ShouldSerialize_public_test test = new ShouldSerialize_public_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
		}

		[Test]
		public void ShouldSerializeTest_protected ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_protected_test))["Prop"];
			ShouldSerialize_protected_test test = new ShouldSerialize_protected_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
		}

		[Test]
		public void ShouldSerializeTest_private ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_protected_test))["Prop"];
			ShouldSerialize_protected_test test = new ShouldSerialize_protected_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
		}

		[Test]
		public void CanResetTest_public ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanReset_public_test))["Prop"];
			CanReset_public_test test = new CanReset_public_test ();

			Assert.IsTrue (p.CanResetValue (test), "1");
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void CanResetTest_protected ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanReset_protected_test))["Prop"];
			CanReset_protected_test test = new CanReset_protected_test ();

			Assert.IsTrue (p.CanResetValue (test), "1");
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void CanResetTest_private ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanReset_private_test))["Prop"];
			CanReset_private_test test = new CanReset_private_test ();

			Assert.IsTrue (p.CanResetValue (test), "1");
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void CanResetTestNoSetterTest ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanResetNoSetter_test))["Prop"];
			CanResetNoSetter_test test = new CanResetNoSetter_test ();

#if NET_2_0
			Assert.IsFalse (p.CanResetValue (test), "1");
#else
			Assert.IsTrue (p.CanResetValue (test), "1");
#endif
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void NoSerializeOrResetPropTest ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (NoSerializeOrResetProp_test))["Prop"];
			NoSerializeOrResetProp_test test = new NoSerializeOrResetProp_test ();

			Assert.IsFalse (p.CanResetValue (test), "1");
			Assert.IsFalse (p.ShouldSerializeValue (test), "2");
		}

		[Test]
		public void ShouldSerializeFalseEffectOnCanResetTest ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerializeFalseEffectOnCanReset_test))["Prop"];
			ShouldSerializeFalseEffectOnCanReset_test test = new ShouldSerializeFalseEffectOnCanReset_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
			Assert.IsFalse (p.CanResetValue (test), "2");
		}

		[Test]
		public void ReadOnlyPropertyTest ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof (ReadOnlyProperty_test));
			Assert.IsTrue (col["Prop"].IsReadOnly, "1");
		}

		[Test]
		public void ReadOnlyAttributeTest ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof (ReadOnlyAttribute_test));
			Assert.IsTrue (col["Prop"].IsReadOnly, "1");
		}

		[Test]
		public void ReadOnlyConflictingTest ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof (ConflictingReadOnly_test));
			Assert.IsTrue (col["Prop"].IsReadOnly, "1");
		}

		[Test] // bug #80292
		public void DisplayNameTest ()
		{
			PropertyDescriptor p1 = TypeDescriptor.GetProperties (typeof (DisplayName_test)) ["Explicit"];
			PropertyDescriptor p2 = TypeDescriptor.GetProperties (typeof (DisplayName_test)) ["Implicit"];

#if NET_2_0
			Assert.AreEqual ("An explicit displayname", p1.DisplayName, "#1");
#else
			Assert.AreEqual ("Explicit", p1.DisplayName, "#1");
#endif
			Assert.AreEqual ("Implicit", p2.DisplayName, "#2");
		}
	}
}
