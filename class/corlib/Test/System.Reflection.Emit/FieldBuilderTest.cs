//
// FieldBuilderTest.cs - NUnit Test Cases for the FieldBuilder class
//
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Novell, Inc.  http://www.novell.com

using System;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class FieldBuilderTest
	{
		private static int typeIndexer = 0;
		private TypeBuilder _tb;
		private ModuleBuilder module;

		[SetUp]
		protected void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "MonoTests.System.Reflection.Emit.FieldBuilderTest";

			AssemblyBuilder assembly = Thread.GetDomain ().DefineDynamicAssembly (
				assemblyName, AssemblyBuilderAccess.Run);

			module = assembly.DefineDynamicModule ("module1");
			_tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		}

		[Test]
		public void TestFieldProperties ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			Assert.AreEqual (FieldAttributes.Public, field.Attributes);
			Assert.AreEqual (_tb, field.DeclaringType);
			Assert.AreEqual (typeof(string), field.FieldType);
			Assert.AreEqual ("name", field.Name);
			Assert.AreEqual (_tb, field.ReflectedType);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestFieldHandleIncomplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			RuntimeFieldHandle handle = field.FieldHandle;
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestFieldHandleComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();
			RuntimeFieldHandle handle = field.FieldHandle;
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestGetCustomAttributesIncomplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			field.GetCustomAttributes (false);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestGetCustomAttributesComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();
			field.GetCustomAttributes (false);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestGetCustomAttributesOfTypeIncomplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			field.GetCustomAttributes (typeof(ObsoleteAttribute), false);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestGetCustomAttributesOfTypeComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();
			field.GetCustomAttributes (typeof(ObsoleteAttribute), false);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestGetValueIncomplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			field.GetValue (_tb);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestGetValueComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();
			field.GetValue (_tb);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestIsDefinedIncomplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			field.IsDefined (typeof(ObsoleteAttribute), true);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestIsDefinedComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();
			field.IsDefined (typeof(ObsoleteAttribute), true);
		}

		[Test]
		public void TestSetConstantIncomplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			field.SetConstant ("default");
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestSetConstantComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();
			field.SetConstant ("default");
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestSetCustomAttributeCaBuilderComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();

			ConstructorInfo guidCtor = typeof(GuidAttribute).GetConstructor (
				new Type[] {
				typeof(string)
			});
			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (guidCtor,
				new object[] {
				Guid.NewGuid ().ToString ("D")
			}, new FieldInfo[0], new object[0]);

			field.SetCustomAttribute (caBuilder);
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestSetCustomAttributeCtorComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();

			ConstructorInfo guidCtor = typeof(GuidAttribute).GetConstructor (
				new Type[] {
				typeof(string)
			});

			field.SetCustomAttribute (guidCtor, new byte[] { 01,00,01,00,00 });
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestSetMarshalComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();
			field.SetMarshal (UnmanagedMarshal.DefineSafeArray (UnmanagedType.BStr));
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestSetOffsetComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();
			field.SetOffset (1);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestSetValueComplete ()
		{
			FieldBuilder field = _tb.DefineField ("name",
				typeof(string), FieldAttributes.Public);
			_tb.CreateType ();
			field.SetValue ((object) 1, 1, BindingFlags.Public, null,
				CultureInfo.InvariantCulture);
		}

		// Return a unique type name
		private string genTypeName ()
		{
			return "class" + (typeIndexer++);
		}
	}
}
