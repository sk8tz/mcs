// TypeTest.cs - NUnit Test Cases for the System.Type class
//
// Authors:
// 	Zoltan Varga (vargaz@freemail.hu)
//  Patrik Torstensson
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Collections;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

class NoNamespaceClass {
}

namespace MonoTests.System
{
	class Super : ICloneable
	{
		public virtual object Clone ()
		{
			return null;
		}
	}

	class Duper: Super
	{
	}

	interface IFace1
	{
		void foo ();
	}

	interface IFace2 : IFace1
	{
		void bar ();
	}

	interface IFace3 : IFace2
	{
	}

	enum TheEnum
	{
		A,
		B,
		C
	};

	abstract class Base
	{
		public int level;

		public abstract int this [byte i] {
			get;
		}

		public abstract int this [int i] {
			get;
		}

		public abstract void TestVoid ();
		public abstract void TestInt (int i);
	}

	class DeriveVTable : Base
	{
		public override int this [byte i] {
			get { return 1; }
		}

		public override int this [int i] {
			get { return 1; }
		}

		public override void TestVoid ()
		{
			level = 1;
		}

		public override void TestInt (int i)
		{
			level = 1;
		}
	}

	class NewVTable : DeriveVTable
	{
		public new int this [byte i] {
			get { return 2; }
		}

		public new int this [int i] {
			get { return 2; }
		}

		public new void TestVoid ()
		{
			level = 2;
		}

		public new void TestInt (int i)
		{
			level = 2;
		}

		public void Overload ()
		{
		}

		public void Overload (int i)
		{
		}

		public NewVTable (out int i)
		{
			i = 0;
		}

		public void byref_method (out int i)
		{
			i = 0;
		}
	}

	class Base1
	{
		public virtual int Foo {
			get { return 1; }
			set { }
		}
	}

	class Derived1 : Base1
	{
		public override int Foo {
			set { }
		}
	}

#if NET_2_0
	public class Foo<T>
	{
		public T Whatever;
	
		public T Test {
			get { throw new NotImplementedException (); }
		}

		public T Execute (T a)
		{
			return a;
		}
	}

	public interface IBar<T>
	{
	}

	public class Baz<T> : IBar<T>
	{
	}
#endif

	[TestFixture]
	public class TypeTest
	{
		private AssemblyBuilder assembly;
		private ModuleBuilder module;
		const string ASSEMBLY_NAME = "MonoTests.System.TypeTest";
		static int typeIndexer = 0;

		[SetUp]
		public void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;
			assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (
					assemblyName, AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());
			module = assembly.DefineDynamicModule ("module1");
		}

		private string genTypeName ()
		{
			return "t" + (typeIndexer++);
		}

		private void ByrefMethod (ref int i, ref Derived1 j, ref Base1 k)
		{
		}
#if NET_2_0
		private void GenericMethod<Q> (Q q)
		{
		}
#endif
		[Test]
		public void TestIsAssignableFrom ()
		{
			// Simple tests for inheritance
			Assert.AreEqual (typeof (Super).IsAssignableFrom (typeof (Duper)) , true, "#01");
			Assert.AreEqual (typeof (Duper).IsAssignableFrom (typeof (Duper)), true, "#02");
			Assert.AreEqual (typeof (Object).IsAssignableFrom (typeof (Duper)), true, "#03");
			Assert.AreEqual (typeof (ICloneable).IsAssignableFrom (typeof (Duper)), true, "#04");

			// Tests for arrays
			Assert.AreEqual (typeof (Super[]).IsAssignableFrom (typeof (Duper[])), true, "#05");
			Assert.AreEqual (typeof (Duper[]).IsAssignableFrom (typeof (Super[])), false, "#06");
			Assert.AreEqual (typeof (Object[]).IsAssignableFrom (typeof (Duper[])), true, "#07");
			Assert.AreEqual (typeof (ICloneable[]).IsAssignableFrom (typeof (Duper[])), true, "#08");

			// Tests for multiple dimensional arrays
			Assert.AreEqual (typeof (Super[][]).IsAssignableFrom (typeof (Duper[][])), true, "#09");
			Assert.AreEqual (typeof (Duper[][]).IsAssignableFrom (typeof (Super[][])), false, "#10");
			Assert.AreEqual (typeof (Object[][]).IsAssignableFrom (typeof (Duper[][])), true, "#11");
			Assert.AreEqual (typeof (ICloneable[][]).IsAssignableFrom (typeof (Duper[][])), true, "#12");

			// Tests for vectors<->one dimensional arrays */
#if TARGET_JVM // Lower bounds arrays are not supported for TARGET_JVM.
			Array arr1 = Array.CreateInstance (typeof (int), new int[] {1});
			Assert.AreEqual (typeof (int[]).IsAssignableFrom (arr1.GetType ()), true, "#13");
#else
			Array arr1 = Array.CreateInstance (typeof (int), new int[] {1}, new int[] {0});
			Array arr2 = Array.CreateInstance (typeof (int), new int[] {1}, new int[] {10});

			Assert.AreEqual (typeof (int[]).IsAssignableFrom (arr1.GetType ()), true, "#13");
			Assert.AreEqual (typeof (int[]).IsAssignableFrom (arr2.GetType ()), false, "#14");
#endif // TARGET_JVM

			// Test that arrays of enums can be cast to their base types
			Assert.AreEqual (typeof (int[]).IsAssignableFrom (typeof (TypeCode[])), true, "#15");

			// Test that arrays of valuetypes can't be cast to arrays of
			// references
			Assert.AreEqual (typeof (object[]).IsAssignableFrom (typeof (TypeCode[])), false, "#16");
			Assert.AreEqual (typeof (ValueType[]).IsAssignableFrom (typeof (TypeCode[])), false, "#17");
			Assert.AreEqual (typeof (Enum[]).IsAssignableFrom (typeof (TypeCode[])), false, "#18");

			// Test that arrays of enums can't be cast to arrays of references
			Assert.AreEqual (typeof (object[]).IsAssignableFrom (typeof (TheEnum[])), false, "#19");
			Assert.AreEqual (typeof (ValueType[]).IsAssignableFrom (typeof (TheEnum[])), false, "#20");
			Assert.AreEqual (typeof (Enum[]).IsAssignableFrom (typeof (TheEnum[])), false, "#21");

			// Check that ValueType and Enum are recognized as reference types
			Assert.AreEqual (typeof (object).IsAssignableFrom (typeof (ValueType)), true, "#22");
			Assert.AreEqual (typeof (object).IsAssignableFrom (typeof (Enum)), true, "#23");
			Assert.AreEqual (typeof (ValueType).IsAssignableFrom (typeof (Enum)), true, "#24");

			Assert.AreEqual (typeof (object[]).IsAssignableFrom (typeof (ValueType[])), true, "#25");
			Assert.AreEqual (typeof (ValueType[]).IsAssignableFrom (typeof (ValueType[])), true, "#26");
			Assert.AreEqual (typeof (Enum[]).IsAssignableFrom (typeof (ValueType[])), false, "#27");

			Assert.AreEqual (typeof (object[]).IsAssignableFrom (typeof (Enum[])), true, "#28");
			Assert.AreEqual (typeof (ValueType[]).IsAssignableFrom (typeof (Enum[])), true, "#29");
			Assert.AreEqual (typeof (Enum[]).IsAssignableFrom (typeof (Enum[])), true, "#30");

			// Tests for byref types
			MethodInfo mi = typeof (TypeTest).GetMethod ("ByrefMethod", BindingFlags.Instance|BindingFlags.NonPublic);
			Assert.IsTrue (mi.GetParameters ()[2].ParameterType.IsAssignableFrom (mi.GetParameters ()[1].ParameterType));
			Assert.IsTrue (mi.GetParameters ()[1].ParameterType.IsAssignableFrom (mi.GetParameters ()[1].ParameterType));

			// Tests for type parameters
#if NET_2_0
			mi = typeof (TypeTest).GetMethod ("GenericMethod", BindingFlags.Instance|BindingFlags.NonPublic);
			Assert.IsTrue (mi.GetParameters ()[0].ParameterType.IsAssignableFrom (mi.GetParameters ()[0].ParameterType));
			Assert.IsFalse (mi.GetParameters ()[0].ParameterType.IsAssignableFrom (typeof (int)));
#endif
		}

		[Test]
		public void TestIsSubclassOf ()
		{
			Assert.IsTrue (typeof (ICloneable).IsSubclassOf (typeof (object)), "#01");

			// Tests for byref types
			Type paramType = typeof (TypeTest).GetMethod ("ByrefMethod", BindingFlags.Instance|BindingFlags.NonPublic).GetParameters () [0].ParameterType;
			Assert.IsTrue (!paramType.IsSubclassOf (typeof (ValueType)), "#02");
			//Assert.IsTrue (paramType.IsSubclassOf (typeof (Object)), "#03");
			Assert.IsTrue (!paramType.IsSubclassOf (paramType), "#04");
		}

		[Test]
		public void TestGetMethodImpl ()
		{
			// Test binding of new slot methods (using no types)
			Assert.AreEqual (typeof (Base), typeof (Base).GetMethod("TestVoid").DeclaringType, "#01");
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetMethod ("TestVoid").DeclaringType, "#02");

			// Test binding of new slot methods (using types)
			Assert.AreEqual (typeof (Base), typeof (Base).GetMethod ("TestInt", new Type[] { typeof (int) }).DeclaringType, "#03");
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetMethod ("TestInt", new Type[] { typeof (int) }).DeclaringType, "#04");

			// Test overload resolution
			Assert.AreEqual (0, typeof (NewVTable).GetMethod ("Overload", new Type[0]).GetParameters ().Length, "#05");

			// Test byref parameters
			Assert.AreEqual (null, typeof (NewVTable).GetMethod ("byref_method", new Type[] { typeof (int) }), "#06");
			Type byrefInt = typeof (NewVTable).GetMethod ("byref_method").GetParameters ()[0].ParameterType;
			Assert.IsNotNull (typeof (NewVTable).GetMethod ("byref_method", new Type[] { byrefInt }), "#07");
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void TestGetPropertyImpl ()
		{
			// Test getting property that is exact
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetProperty ("Item", new Type[1] { typeof (Int32) }).DeclaringType, "#01");

			// Test getting property that is not exact
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetProperty ("Item", new Type[1] { typeof (Int16) }).DeclaringType, "#02");

			// Test overriding of properties when only the set accessor is overriden
			Assert.AreEqual (1, typeof (Derived1).GetProperties ().Length, "#03");
		}

#if !TARGET_JVM // StructLayout not supported for TARGET_JVM
		[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 64)]
		public class Class1
		{
		}

		[StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode)]
		public class Class2
		{
		}

#if NET_2_0
		[Test]
		public void StructLayoutAttribute ()
		{
			StructLayoutAttribute attr1 = typeof (TypeTest).StructLayoutAttribute;
			Assert.AreEqual (LayoutKind.Auto, attr1.Value);

			StructLayoutAttribute attr2 = typeof (Class1).StructLayoutAttribute;
			Assert.AreEqual (LayoutKind.Explicit, attr2.Value);
			Assert.AreEqual (4, attr2.Pack);
			Assert.AreEqual (64, attr2.Size);

			StructLayoutAttribute attr3 = typeof (Class2).StructLayoutAttribute;
			Assert.AreEqual (LayoutKind.Explicit, attr3.Value);
			Assert.AreEqual (CharSet.Unicode, attr3.CharSet);
		}
#endif
#endif // TARGET_JVM

		[Test]
		public void Namespace ()
		{
			Assert.AreEqual (null, typeof (NoNamespaceClass).Namespace);
		}

		public static void Reflected (ref int a)
		{
		}

		[Test]
		public void Name ()
		{
			Assert.AreEqual ("Int32&", typeof (TypeTest).GetMethod ("Reflected").GetParameters () [0].ParameterType.Name);
		}

		[Test]
		public void GetInterfaces ()
		{
			Type[] t = typeof (Duper).GetInterfaces ();
			Assert.AreEqual (1, t.Length);
			Assert.AreEqual (typeof (ICloneable), t[0]);

			Type[] t2 = typeof (IFace3).GetInterfaces ();
			Assert.AreEqual (2, t2.Length);
		}

		public int AField;

		[Test]
		public void GetFieldIgnoreCase ()
		{
			Assert.IsNotNull (typeof (TypeTest).GetField ("afield", BindingFlags.Instance|BindingFlags.Public|BindingFlags.IgnoreCase));
		}

#if NET_2_0
		public int Count {
			internal get {
				return 0;
			}

			set {
			}
		}

		[Test]
		public void GetPropertyAccessorModifiers ()
		{
			Assert.IsNotNull (typeof (TypeTest).GetProperty ("Count", BindingFlags.Instance | BindingFlags.Public));
			Assert.IsNull (typeof (TypeTest).GetProperty ("Count", BindingFlags.Instance | BindingFlags.NonPublic));
		}
#endif

		[Test]
		public void IsAbstract ()
		{
			Assert.IsFalse (typeof (string).IsAbstract, "#1");
			Assert.IsTrue (typeof (ICloneable).IsAbstract, "#2");
			Assert.IsTrue (typeof (ValueType).IsAbstract, "#3");
			Assert.IsTrue (typeof (Enum).IsAbstract, "#4");
			Assert.IsFalse (typeof (TimeSpan).IsAbstract, "#5");
			Assert.IsTrue (typeof (TextReader).IsAbstract, "#6");

#if NET_2_0
			// LAMESPEC:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=286308
			Type [] typeArgs = typeof (List<>).GetGenericArguments ();
			Assert.IsFalse (typeArgs [0].IsAbstract, "#7");
#endif
		}

		[Test]
		public void IsCOMObject ()
		{
			Type type = typeof (string);
			Assert.IsFalse (type.IsCOMObject, "#1");

			TypeBuilder tb = module.DefineType (genTypeName ());
			type = tb.CreateType ();
			Assert.IsFalse (type.IsCOMObject, "#2");
		}

		[Test]
		public void IsImport ()
		{
			Type type = typeof (string);
			Assert.IsFalse (type.IsImport, "#1");

			TypeBuilder tb = module.DefineType (genTypeName ());
			type = tb.CreateType ();
			Assert.IsFalse (type.IsImport, "#2");

			tb = module.DefineType (genTypeName (), TypeAttributes.Import |
				TypeAttributes.Interface | TypeAttributes.Abstract);
			type = tb.CreateType ();
			Assert.IsTrue (type.IsImport, "#3");
		}

		[Test]
		public void IsInterface ()
		{
			Assert.IsFalse (typeof (string).IsInterface, "#1");
			Assert.IsTrue (typeof (ICloneable).IsInterface, "#2");
		}

		[Test]
		public void IsPrimitive () {
			Assert.IsTrue (typeof (IntPtr).IsPrimitive, "#1");
			Assert.IsTrue (typeof (int).IsPrimitive, "#2");
			Assert.IsFalse (typeof (string).IsPrimitive, "#2");
		}

		[Test]
		public void IsValueType ()
		{
			Assert.IsTrue (typeof (int).IsValueType, "#1");
			Assert.IsFalse (typeof (Enum).IsValueType, "#2");
			Assert.IsFalse (typeof (ValueType).IsValueType, "#3");
			Assert.IsTrue (typeof (AttributeTargets).IsValueType, "#4");
			Assert.IsFalse (typeof (string).IsValueType, "#5");
			Assert.IsTrue (typeof (TimeSpan).IsValueType, "#6");
		}

		[Test]
		[Category("NotDotNet")]
		// Depends on the GAC working, which it doesn't durring make distcheck.
		[Category ("NotWorking")]
		public void GetTypeWithWhitespace ()
		{
			Assert.IsNotNull (Type.GetType
						   (@"System.Configuration.NameValueSectionHandler,
			System,
Version=1.0.5000.0,
Culture=neutral
,
PublicKeyToken=b77a5c561934e089"));
		}
		
		[Test]
		public void ExerciseFilterName ()
		{
			MemberInfo[] mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterName, "*");
			Assert.AreEqual (4, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterName, "Test*");
			Assert.AreEqual (2, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterName, "TestVoid");
			Assert.AreEqual (1, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterName, "NonExistingMethod");
			Assert.AreEqual (0, mi.Length);
		}
		
		[Test]
		public void ExerciseFilterNameIgnoreCase ()
		{
			MemberInfo[] mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterNameIgnoreCase, "*");
			Assert.AreEqual (4, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterNameIgnoreCase, "test*");
			Assert.AreEqual (2, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterNameIgnoreCase, "TESTVOID");
			Assert.AreEqual (1, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterNameIgnoreCase, "NonExistingMethod");
			Assert.AreEqual (0, mi.Length);
		}

		public class ByRef0
		{
			public int field;
			public int property {
				get { return 0; }
			}
			public ByRef0 (int i) {}
			public void f (int i) {}
		}

		[Test]
		public void ByrefTypes ()
		{
			Type t = Type.GetType ("MonoTests.System.TypeTest+ByRef0&");
			Assert.IsNotNull (t);
			Assert.IsTrue (t.IsByRef);
			Assert.AreEqual (0, t.GetMethods (BindingFlags.Public | BindingFlags.Instance).Length);
			Assert.AreEqual (0, t.GetConstructors (BindingFlags.Public | BindingFlags.Instance).Length);
			Assert.AreEqual (0, t.GetEvents (BindingFlags.Public | BindingFlags.Instance).Length);
			Assert.AreEqual (0, t.GetProperties (BindingFlags.Public | BindingFlags.Instance).Length);

			Assert.IsNull (t.GetMethod ("f"));
			Assert.IsNull (t.GetField ("field"));
			Assert.IsNull (t.GetProperty ("property"));
		}
		
		[Test]
		public void TestAssemblyQualifiedName ()
		{
			Type t = Type.GetType ("System.Byte[]&");
			Assert.IsTrue (t.AssemblyQualifiedName.StartsWith ("System.Byte[]&"));
			
			t = Type.GetType ("System.Byte*&");
			Assert.IsTrue (t.AssemblyQualifiedName.StartsWith ("System.Byte*&"));
			
			t = Type.GetType ("System.Byte&");
			Assert.IsTrue (t.AssemblyQualifiedName.StartsWith ("System.Byte&"));
		}

		struct B
		{
			int value;
		}

		[Test]
		public void CreateValueTypeNoCtor ()
		{
			typeof(B).InvokeMember ("", BindingFlags.CreateInstance, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void CreateValueTypeNoCtorArgs ()
		{
			typeof(B).InvokeMember ("", BindingFlags.CreateInstance, null, null, new object [] { 1 });
		}

		static string bug336841 (string param1, params string [] param2)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("#A:");
			sb.Append (param1);
			sb.Append ("|");
			for (int i = 0; i < param2.Length; i++) {
				if (i > 0)
					sb.Append (",");
				sb.Append (param2 [i]);
			}
			return sb.ToString ();
		}

		static string bug336841 (string param1)
		{
			return "#B:" + param1;
		}

		static string bug336841 (params string [] param1)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("#C:");
			for (int i = 0; i < param1.Length; i++) {
				if (i > 0)
					sb.Append (";");
				sb.Append (param1 [i]);
			}
			return sb.ToString ();
		}

		[Test] // bug #336841
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=306797
		public void InvokeMember_VarArgs ()
		{
			BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.Public |
				BindingFlags.NonPublic | BindingFlags.OptionalParamBinding |
				BindingFlags.Static | BindingFlags.FlattenHierarchy |
				BindingFlags.Instance;

			Type type = typeof (TypeTest);
			string result = (string) type.InvokeMember ("bug336841",
				flags, null, null, new object [] { "1" });
			Assert.IsNotNull (result, "#A1");
			Assert.AreEqual ("#B:1", result, "#A2");

			result = (string) type.InvokeMember ("bug336841", flags,
				null, null, new object [] { "1", "2", "3", "4" });
			Assert.IsNotNull (result, "#B1");
			Assert.AreEqual ("#A:1|2,3,4", result, "#B2");
		}

		class X
		{
			public static int Value;
		}

		class Y : X
		{
		}

		[Test]
		public void InvokeMemberGetSetField ()
		{
			typeof (X).InvokeMember ("Value", BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy|BindingFlags.SetField, null, null, new object [] { 5 });

			Assert.AreEqual (5, X.Value);
			Assert.AreEqual (5, typeof (X).InvokeMember ("Value", BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy|BindingFlags.GetField, null, null, new object [0]));
			Assert.AreEqual (5, Y.Value);
			Assert.AreEqual (5, typeof (Y).InvokeMember ("Value", BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy|BindingFlags.GetField, null, null, new object [0]));
		}

		class Z
		{
			public Z (IComparable value)
			{
			}
		}
	
		[Test]
		public void InvokeMemberMatchPrimitiveTypeWithInterface ()
		{
			object[] invokeargs = {1};
			typeof (Z).InvokeMember( "", 
											BindingFlags.DeclaredOnly |
											BindingFlags.Public |
											BindingFlags.NonPublic |
											BindingFlags.Instance |
											BindingFlags.CreateInstance,
											null, null, invokeargs 
											);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvokeMember_NoOperation ()
		{
			typeof (TypeTest).InvokeMember ("Run", BindingFlags.Public|BindingFlags.Static, null, null, new object [0]);
		}

        public static void Run ()
        {
        }

		class TakesInt
		{
			private int i;

			public TakesInt (int x)
			{
				i = x;
			}

			public int Integer {
				get { return i; }
			}
		}

		class TakesObject
		{
			public TakesObject (object x) {}
		}

		[Test] // bug #75241
		public void GetConstructorNullInTypes ()
		{
			// This ends up calling type.GetConstructor ()
			Activator.CreateInstance (typeof (TakesInt), new object [] { null });
			Activator.CreateInstance (typeof (TakesObject), new object [] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetConstructorNullInTypes_Bug71300 ()
		{
			typeof (TakesInt).GetConstructor (new Type[1] { null });
			// so null in types isn't valid for GetConstructor!
		}

		[Test]
		public void GetConstructor_TakeInt_Object ()
		{
			Assert.IsNull (typeof (TakesInt).GetConstructor (new Type[1] { typeof (object) }));
		}

		[Test]
		public void GetCustomAttributes_All ()
		{
			object [] attrs = typeof (A).GetCustomAttributes (false);
			Assert.AreEqual (2, attrs.Length, "#A1");
			Assert.IsTrue (HasAttribute (attrs, typeof (FooAttribute)), "#A2");
			Assert.IsTrue (HasAttribute (attrs, typeof (VolatileModifier)), "#A3");

			attrs = typeof (BA).GetCustomAttributes (false);
			Assert.AreEqual (1, attrs.Length, "#B1");
			Assert.AreEqual (typeof (BarAttribute), attrs [0].GetType (), "#B2");

			attrs = typeof (BA).GetCustomAttributes (true);
			Assert.AreEqual (2, attrs.Length, "#C1");
			Assert.IsTrue (HasAttribute (attrs, typeof (BarAttribute)), "#C2");
			Assert.IsTrue (HasAttribute (attrs, typeof (VolatileModifier)), "#C3");

			attrs = typeof (CA).GetCustomAttributes (false);
			Assert.AreEqual (0, attrs.Length, "#D");

			attrs = typeof (CA).GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#E1");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#E2");
		}

		static bool HasAttribute (object [] attrs, Type attributeType)
		{
			foreach (object attr in attrs)
				if (attr.GetType () == attributeType)
					return true;
			return false;
		}

		[Test]
		public void GetCustomAttributes_Type ()
		{
			object [] attrs = null;

			attrs = typeof (A).GetCustomAttributes (
				typeof (VolatileModifier), false);
			Assert.AreEqual (1, attrs.Length, "#A1");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#A2");
			attrs = typeof (A).GetCustomAttributes (
				typeof (VolatileModifier), true);
			Assert.AreEqual (1, attrs.Length, "#A3");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#A4");

			attrs = typeof (A).GetCustomAttributes (
				typeof (NemerleAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#B1");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#B2");
			attrs = typeof (A).GetCustomAttributes (
				typeof (NemerleAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#B3");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#B4");

			attrs = typeof (A).GetCustomAttributes (
				typeof (FooAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#C1");
			Assert.AreEqual (typeof (FooAttribute), attrs [0].GetType (), "#C2");
			attrs = typeof (A).GetCustomAttributes (
				typeof (FooAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#C3");
			Assert.AreEqual (typeof (FooAttribute), attrs [0].GetType (), "#C4");

			attrs = typeof (BA).GetCustomAttributes (
				typeof (VolatileModifier), false);
			Assert.AreEqual (0, attrs.Length, "#D1");
			attrs = typeof (BA).GetCustomAttributes (
				typeof (VolatileModifier), true);
			Assert.AreEqual (1, attrs.Length, "#D2");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#D3");

			attrs = typeof (BA).GetCustomAttributes (
				typeof (NemerleAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#E1");
			attrs = typeof (BA).GetCustomAttributes (
				typeof (NemerleAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#E2");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#E3");

			attrs = typeof (BA).GetCustomAttributes (
				typeof (FooAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#F1");
			Assert.AreEqual (typeof (BarAttribute), attrs [0].GetType (), "#F2");
			attrs = typeof (BA).GetCustomAttributes (
				typeof (FooAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#F3");
			Assert.AreEqual (typeof (BarAttribute), attrs [0].GetType (), "#F4");

			attrs = typeof (bug82431A1).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#G1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#G2");
			attrs = typeof (bug82431A1).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#G3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#G4");

			attrs = typeof (bug82431A1).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#H1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#H2");
			attrs = typeof (bug82431A1).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#H3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#H4");

			attrs = typeof (bug82431A2).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#I1");
			attrs = typeof (bug82431A2).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#I2");

			attrs = typeof (bug82431A2).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#J1");
			attrs = typeof (bug82431A2).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#J2");

			attrs = typeof (bug82431A3).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (2, attrs.Length, "#K1");
			Assert.IsTrue (HasAttribute (attrs, typeof (InheritAttribute)), "#K2");
			Assert.IsTrue (HasAttribute (attrs, typeof (NotInheritAttribute)), "#K3");
			attrs = typeof (bug82431A3).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (2, attrs.Length, "#K4");
			Assert.IsTrue (HasAttribute (attrs, typeof (InheritAttribute)), "#K5");
			Assert.IsTrue (HasAttribute (attrs, typeof (NotInheritAttribute)), "#K6");

			attrs = typeof (bug82431A3).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#L1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#L2");
			attrs = typeof (bug82431A3).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#L3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#L4");

			attrs = typeof (bug82431B1).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#M1");
			Assert.AreEqual (typeof (InheritAttribute), attrs [0].GetType (), "#M2");
			attrs = typeof (bug82431B1).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#M3");
			Assert.AreEqual (typeof (InheritAttribute), attrs [0].GetType (), "#M4");

			attrs = typeof (bug82431B1).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#N1");
			attrs = typeof (bug82431B1).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#N2");

			attrs = typeof (bug82431B2).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#O1");
			attrs = typeof (bug82431B2).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#O2");
			Assert.AreEqual (typeof (InheritAttribute), attrs [0].GetType (), "#O3");

			attrs = typeof (bug82431B2).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#P1");
			attrs = typeof (bug82431B2).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#P2");

			attrs = typeof (bug82431B3).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#Q1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#Q2");
			attrs = typeof (bug82431B3).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (2, attrs.Length, "#Q3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#Q4");
			Assert.AreEqual (typeof (InheritAttribute), attrs [1].GetType (), "#Q5");

			attrs = typeof (bug82431B3).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#R1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#R2");
			attrs = typeof (bug82431B3).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#R3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#R4");

			attrs = typeof (bug82431B4).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#S1");
			attrs = typeof (bug82431B4).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#S2");
			Assert.AreEqual (typeof (InheritAttribute), attrs [0].GetType (), "#S3");

			attrs = typeof (bug82431B4).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#T1");
			attrs = typeof (bug82431B4).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#T2");

			attrs = typeof (A).GetCustomAttributes (
				typeof (string), false);
			Assert.AreEqual (0, attrs.Length, "#U1");
			attrs = typeof (A).GetCustomAttributes (
				typeof (string), true);
			Assert.AreEqual (0, attrs.Length, "#U2");
		}

		[Test] // bug #76150
		public void IsDefined ()
		{
			Assert.IsTrue (typeof (A).IsDefined (typeof (NemerleAttribute), false), "#A1");
			Assert.IsTrue (typeof (A).IsDefined (typeof (VolatileModifier), false), "#A2");
			Assert.IsTrue (typeof (A).IsDefined (typeof (FooAttribute), false), "#A3");
			Assert.IsFalse (typeof (A).IsDefined (typeof (BarAttribute), false), "#A4");

			Assert.IsFalse (typeof (BA).IsDefined (typeof (NemerleAttribute), false), "#B1");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (VolatileModifier), false), "#B2");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (FooAttribute), false), "#B3");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (BarAttribute), false), "#B4");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (string), false), "#B5");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (int), false), "#B6");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (NemerleAttribute), true), "#B7");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (VolatileModifier), true), "#B8");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (FooAttribute), true), "#B9");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (BarAttribute), true), "#B10");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (string), true), "#B11");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (int), true), "#B12");
		}

		[Test]
		public void IsDefined_AttributeType_Null ()
		{
			try {
				typeof (BA).IsDefined ((Type) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("attributeType", ex.ParamName, "#6");
			}
		}

		[Test] // bug #82431
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void IsDefined_Inherited ()
		{
			Assert.IsFalse (typeof (CA).IsDefined (typeof (NemerleAttribute), false), "#C1");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (VolatileModifier), false), "#C2");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (FooAttribute), false), "#C3");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (BarAttribute), false), "#C4");
			Assert.IsTrue (typeof (CA).IsDefined (typeof (NemerleAttribute), true), "#C5");
			Assert.IsTrue (typeof (CA).IsDefined (typeof (VolatileModifier), true), "#C6");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (FooAttribute), true), "#C7");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (BarAttribute), true), "#C8");

			Assert.IsFalse (typeof (BBA).IsDefined (typeof (NemerleAttribute), false), "#D1");
			Assert.IsFalse (typeof (BBA).IsDefined (typeof (VolatileModifier), false), "#D2");
			Assert.IsFalse (typeof (BBA).IsDefined (typeof (FooAttribute), false), "#D3");
			Assert.IsFalse (typeof (BBA).IsDefined (typeof (BarAttribute), false), "#D4");
			Assert.IsTrue (typeof (BBA).IsDefined (typeof (NemerleAttribute), true), "#D5");
			Assert.IsTrue (typeof (BBA).IsDefined (typeof (VolatileModifier), true), "#D6");
#if NET_2_0
			Assert.IsTrue (typeof (BBA).IsDefined (typeof (FooAttribute), true), "#D7");
			Assert.IsTrue (typeof (BBA).IsDefined (typeof (BarAttribute), true), "#D8");
#else
			Assert.IsFalse (typeof (BBA).IsDefined (typeof (FooAttribute), true), "#D7");
			Assert.IsFalse (typeof (BBA).IsDefined (typeof (BarAttribute), true), "#D8");
#endif

			Assert.IsTrue (typeof (bug82431A1).IsDefined (typeof (InheritAttribute), false), "#E1");
			Assert.IsTrue (typeof (bug82431A1).IsDefined (typeof (NotInheritAttribute), false), "#E2");
			Assert.IsTrue (typeof (bug82431A1).IsDefined (typeof (InheritAttribute), true), "#E3");
			Assert.IsTrue (typeof (bug82431A1).IsDefined (typeof (NotInheritAttribute), true), "#E4");

			Assert.IsFalse (typeof (bug82431A2).IsDefined (typeof (InheritAttribute), false), "#F1");
			Assert.IsFalse (typeof (bug82431A2).IsDefined (typeof (NotInheritAttribute), false), "#F2");
#if NET_2_0
			Assert.IsFalse (typeof (bug82431A2).IsDefined (typeof (InheritAttribute), true), "#F3");
#else
			Assert.IsTrue (typeof (bug82431A2).IsDefined (typeof (InheritAttribute), true), "#F3");
#endif
			Assert.IsFalse (typeof (bug82431A2).IsDefined (typeof (NotInheritAttribute), true), "#F4");

			Assert.IsTrue (typeof (bug82431A3).IsDefined (typeof (InheritAttribute), false), "#G1");
			Assert.IsTrue (typeof (bug82431A3).IsDefined (typeof (NotInheritAttribute), false), "#G2");
			Assert.IsTrue (typeof (bug82431A3).IsDefined (typeof (InheritAttribute), true), "#G3");
			Assert.IsTrue (typeof (bug82431A3).IsDefined (typeof (NotInheritAttribute), true), "#G4");

			Assert.IsTrue (typeof (bug82431B1).IsDefined (typeof (InheritAttribute), false), "#H1");
			Assert.IsFalse (typeof (bug82431B1).IsDefined (typeof (NotInheritAttribute), false), "#H2");
			Assert.IsTrue (typeof (bug82431B1).IsDefined (typeof (InheritAttribute), true), "#H3");
			Assert.IsFalse (typeof (bug82431B1).IsDefined (typeof (NotInheritAttribute), true), "#H4");

			Assert.IsFalse (typeof (bug82431B2).IsDefined (typeof (InheritAttribute), false), "#I1");
			Assert.IsFalse (typeof (bug82431B2).IsDefined (typeof (NotInheritAttribute), false), "#I2");
			Assert.IsTrue (typeof (bug82431B2).IsDefined (typeof (InheritAttribute), true), "#I3");
			Assert.IsFalse (typeof (bug82431B2).IsDefined (typeof (NotInheritAttribute), true), "#I4");

			Assert.IsTrue (typeof (bug82431B3).IsDefined (typeof (InheritAttribute), false), "#J1");
			Assert.IsTrue (typeof (bug82431B3).IsDefined (typeof (NotInheritAttribute), false), "#J2");
			Assert.IsTrue (typeof (bug82431B3).IsDefined (typeof (InheritAttribute), true), "#J3");
			Assert.IsTrue (typeof (bug82431B3).IsDefined (typeof (NotInheritAttribute), true), "#J4");

			Assert.IsFalse (typeof (bug82431B4).IsDefined (typeof (InheritAttribute), false), "#K2");
			Assert.IsFalse (typeof (bug82431B4).IsDefined (typeof (NotInheritAttribute), false), "#K2");
			Assert.IsTrue (typeof (bug82431B4).IsDefined (typeof (InheritAttribute), true), "#K3");
			Assert.IsFalse (typeof (bug82431B4).IsDefined (typeof (NotInheritAttribute), true), "#K4");
		}

		[Test]
		public void GetTypeCode ()
		{
			Assert.AreEqual (TypeCode.Boolean, Type.GetTypeCode (typeof (bool)), "#1");
			Assert.AreEqual (TypeCode.Byte, Type.GetTypeCode (typeof (byte)), "#2");
			Assert.AreEqual (TypeCode.Char, Type.GetTypeCode (typeof (char)), "#3");
			Assert.AreEqual (TypeCode.DateTime, Type.GetTypeCode (typeof (DateTime)), "#4");
			Assert.AreEqual (TypeCode.DBNull, Type.GetTypeCode (typeof (DBNull)), "#5");
			Assert.AreEqual (TypeCode.Decimal, Type.GetTypeCode (typeof (decimal)), "#6");
			Assert.AreEqual (TypeCode.Double, Type.GetTypeCode (typeof (double)), "#7");
			Assert.AreEqual (TypeCode.Empty, Type.GetTypeCode (null), "#8");
			Assert.AreEqual (TypeCode.Int16, Type.GetTypeCode (typeof (short)), "#9");
			Assert.AreEqual (TypeCode.Int32, Type.GetTypeCode (typeof (int)), "#10");
			Assert.AreEqual (TypeCode.Int64, Type.GetTypeCode (typeof (long)), "#11");
			Assert.AreEqual (TypeCode.Object, Type.GetTypeCode (typeof (TakesInt)), "#12");
			Assert.AreEqual (TypeCode.SByte, Type.GetTypeCode (typeof (sbyte)), "#13");
			Assert.AreEqual (TypeCode.Single, Type.GetTypeCode (typeof (float)), "#14");
			Assert.AreEqual (TypeCode.String, Type.GetTypeCode (typeof (string)), "#15");
			Assert.AreEqual (TypeCode.UInt16, Type.GetTypeCode (typeof (ushort)), "#16");
			Assert.AreEqual (TypeCode.UInt32, Type.GetTypeCode (typeof (uint)), "#17");
			Assert.AreEqual (TypeCode.UInt64, Type.GetTypeCode (typeof (ulong)), "#18");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetConstructor1a_Bug71300 ()
		{
			typeof (BindingFlags).GetConstructor (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetConstructor1b_Bug71300 ()
		{
			typeof (BindingFlags).GetConstructor (new Type[1] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetConstructor4_Bug71300 ()
		{
			typeof (BindingFlags).GetConstructor (BindingFlags.Default, null, new Type[1] { null }, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetConstructor5_Bug71300 ()
		{
			typeof (BindingFlags).GetConstructor (BindingFlags.Default, null, CallingConventions.Any, new Type[1] { null }, null);
		}

		[Test]
		public void GetMethod_Bug77367 ()
		{
			MethodInfo i = typeof (Bug77367).GetMethod ("Run", Type.EmptyTypes);
			Assert.IsNull (i);
		}

#if !TARGET_JVM // Reflection.Emit is not supported for TARGET_JVM
		[Test]
		public void EqualsUnderlyingType ()
		{
			AssemblyBuilderAccess access = AssemblyBuilderAccess.RunAndSave;
			TypeAttributes attribs = TypeAttributes.Public;

			AssemblyName name = new AssemblyName ();
			name.Name = "enumtest";
			AssemblyBuilder assembly = 
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, access);

			ModuleBuilder module = assembly.DefineDynamicModule 
				("m", "enumtest.dll");
			EnumBuilder e = module.DefineEnum ("E", attribs, typeof (int));

			Assert.IsTrue (typeof (int).Equals (e));
		}
#endif // TARGET_JVM

		[Test]
		public void Equals_Type_Null ()
		{
			Assert.IsFalse (typeof (int).Equals ((Type) null), "#1");
			Assert.IsFalse (typeof (int).Equals ((object) null), "#2");
		}

		[Test]
		public void GetElementType_Bug63841 ()
		{
			Assert.IsNull (typeof (TheEnum).GetElementType (), "#1");
		}

#if NET_2_0
		[Test]
		public void FullNameGenerics ()
		{
			Type fooType = typeof (Foo<>);
			FieldInfo [] fields = fooType.GetFields ();

			Assert.AreEqual (1, fields.Length, "#0");

			Assert.IsNotNull (fooType.FullName, "#1");
			Assert.IsNotNull (fooType.AssemblyQualifiedName, "#1a");

			FieldInfo field = fooType.GetField ("Whatever");
			Assert.IsNotNull (field, "#2");
			Assert.AreEqual (field, fields [0], "#2a");
			Assert.IsNull (field.FieldType.FullName, "#3");
			Assert.IsNull (field.FieldType.AssemblyQualifiedName, "#3a");
			Assert.IsNotNull (field.FieldType.ToString (), "#4");

			PropertyInfo prop = fooType.GetProperty ("Test");
			Assert.IsNotNull (prop, "#5");
			Assert.IsNull (prop.PropertyType.FullName, "#6");
			Assert.IsNull (prop.PropertyType.AssemblyQualifiedName, "#6a");
			Assert.IsNotNull (prop.PropertyType.ToString (), "#7");

			MethodInfo method = fooType.GetMethod("Execute");
			Assert.IsNotNull (method, "#8");
			Assert.IsNull (method.ReturnType.FullName, "#9");
			Assert.IsNull (method.ReturnType.AssemblyQualifiedName, "#9a");
			Assert.IsNotNull (method.ReturnType.ToString (), "#10");

			ParameterInfo[] parameters = method.GetParameters();
			Assert.AreEqual (1, parameters.Length, "#11");
			Assert.IsNull (parameters[0].ParameterType.FullName, "#12");
			Assert.IsNull (parameters[0].ParameterType.AssemblyQualifiedName, "#12a");
			Assert.IsNotNull (parameters[0].ParameterType.ToString (), "#13");
		}

		[Test]
		public void TypeParameterIsNotGeneric ()
		{
			Type fooType = typeof (Foo<>);
			Type type_param = fooType.GetGenericArguments () [0];
			Assert.IsTrue (type_param.IsGenericParameter);
			Assert.IsFalse (type_param.IsGenericType);
			Assert.IsFalse (type_param.IsGenericTypeDefinition);

			// LAMESPEC: MSDN claims that this should be false, but .NET v2.0.50727 says it's true
			// http://msdn2.microsoft.com/en-us/library/system.type.isgenerictype.aspx
			Assert.IsTrue (type_param.ContainsGenericParameters);
		}

		[Test]
		public void IsAssignable ()
		{
			Type foo_type = typeof (Foo<>);
			Type foo_int_type = typeof (Foo<int>);
			Assert.IsFalse (foo_type.IsAssignableFrom (foo_int_type), "Foo<int> -!-> Foo<>");
			Assert.IsFalse (foo_int_type.IsAssignableFrom (foo_type), "Foo<> -!-> Foo<int>");

			Type ibar_short_type = typeof (IBar<short>);
			Type ibar_int_type = typeof (IBar<int>);
			Type baz_short_type = typeof (Baz<short>);
			Type baz_int_type = typeof (Baz<int>);

			Assert.IsTrue (ibar_int_type.IsAssignableFrom (baz_int_type), "Baz<int> -> IBar<int>");
			Assert.IsTrue (ibar_short_type.IsAssignableFrom (baz_short_type), "Baz<short> -> IBar<short>");

			Assert.IsFalse (ibar_int_type.IsAssignableFrom (baz_short_type), "Baz<short> -!-> IBar<int>");
			Assert.IsFalse (ibar_short_type.IsAssignableFrom (baz_int_type), "Baz<int> -!-> IBar<short>");

			// Nullable tests
			Assert.IsTrue (typeof (Nullable<int>).IsAssignableFrom (typeof (int)));
			Assert.IsFalse (typeof (int).IsAssignableFrom (typeof (Nullable<int>)));
			Assert.IsTrue (typeof (Nullable<FooStruct>).IsAssignableFrom (typeof (FooStruct)));
		}

		[Test]
		public void IsInstanceOf ()
		{
			Assert.IsTrue (typeof (Nullable<int>).IsInstanceOfType (5));
		}

		[Test]
		public void ByrefType ()
		{
			Type foo_type = typeof (Foo<>);
			Type type_param = foo_type.GetGenericArguments () [0];
			Type byref_type_param = type_param.MakeByRefType ();
			Assert.IsFalse (byref_type_param.IsGenericParameter);
			Assert.IsNull (byref_type_param.DeclaringType);
		}

		[Test]
		[Category ("NotWorking")] // BindingFlags.SetField throws since args.Length != 1, even though we have SetProperty
		public void Bug79023 ()
		{
			ArrayList list = new ArrayList();
			list.Add("foo");

			// The next line used to throw because we had SetProperty
			list.GetType().InvokeMember("Item",
						    BindingFlags.SetField|BindingFlags.SetProperty|
						    BindingFlags.Instance|BindingFlags.Public,
						    null, list, new object[] { 0, "bar" });
			Assert.AreEqual ("bar", list[0]);
		}
		
		[ComVisible (true)]
		public class ComFoo<T> {
		}

		[Test]
		public void GetCustomAttributesGenericInstance ()
		{
			Assert.AreEqual (1, typeof (ComFoo<int>).GetCustomAttributes (typeof (ComVisibleAttribute), true).Length);
		}

		interface ByRef1<T> { void f (ref T t); }
		interface ByRef2 { void f<T> (ref T t); }

		interface ByRef3<T> where T:struct { void f (ref T? t); }
		interface ByRef4 { void f<T> (ref T? t) where T:struct; }

		void CheckGenericByRef (Type t)
		{
			string name = t.Name;
			t = t.GetMethod ("f").GetParameters () [0].ParameterType;

			Assert.IsFalse (t.IsGenericType, name);
			Assert.IsFalse (t.IsGenericTypeDefinition, name);
			Assert.IsFalse (t.IsGenericParameter, name);
		}

		[Test]
		public void GenericByRef ()
		{
			CheckGenericByRef (typeof (ByRef1<>));
			CheckGenericByRef (typeof (ByRef2));
			CheckGenericByRef (typeof (ByRef3<>));
			CheckGenericByRef (typeof (ByRef4));
		}

		public class Bug80242<T> {
			public interface IFoo { }
			public class Bar : IFoo { }
			public class Baz : Bar { }
		}

		[Test]
		public void TestNestedTypes ()
		{
			Type t = typeof (Bug80242<object>);
			Assert.IsFalse (t.IsGenericTypeDefinition);
			foreach (Type u in t.GetNestedTypes ()) {
				Assert.IsTrue (u.IsGenericTypeDefinition, "{0} isn't a generic definition", u);
				Assert.AreEqual (u, u.GetGenericArguments () [0].DeclaringType);
			}
		}

		[Test] // bug #82211
		public void GetMembers_GenericArgument ()
		{
			Type argType = typeof (ComFoo<>).GetGenericArguments () [0];
			MemberInfo [] members = argType.GetMembers ();
			Assert.IsNotNull (members, "#1");
			Assert.AreEqual (4, members.Length, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ReflectionOnlyGetTypeNullTypeName ()
		{
			Type.ReflectionOnlyGetType (null, false, false);
		}

		[Test]
		public void ReflectionOnlyGetTypeDoNotThrow ()
		{
			Assert.IsNull (Type.ReflectionOnlyGetType ("a, nonexistent.dll", false, false));
		}

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void ReflectionOnlyGetTypeThrow ()
		{
			Type.ReflectionOnlyGetType ("a, nonexistent.dll", true, false);
		}

		[Test]
		public void ReflectionOnlyGetType ()
		{
			Type t = Type.ReflectionOnlyGetType (typeof (int).AssemblyQualifiedName.ToString (), true, true);
			Assert.AreEqual ("System.Int32", t.FullName);
		}
#endif

		public class NemerleAttribute : Attribute
		{
		}

		public class VolatileModifier : NemerleAttribute
		{
		}

		[VolatileModifier]
		[FooAttribute]
		class A
		{
		}

		[AttributeUsage (AttributeTargets.Class, Inherited=false)]
		public class FooAttribute : Attribute
		{
		}

		public class BarAttribute : FooAttribute
		{
		}

		[BarAttribute]
		class BA : A
		{
		}

		class BBA : BA
		{
		}

		class CA : A
		{
		}

		[AttributeUsage (AttributeTargets.Class, Inherited=true)]
		public class InheritAttribute : Attribute
		{
		}

		[AttributeUsage (AttributeTargets.Class, Inherited=false)]
		public class NotInheritAttribute : InheritAttribute
		{
		}

		[NotInheritAttribute]
		public class bug82431A1
		{
		}

		public class bug82431A2 : bug82431A1
		{
		}

		[NotInheritAttribute]
		[InheritAttribute]
		public class bug82431A3 : bug82431A1
		{
		}

		[InheritAttribute]
		public class bug82431B1
		{
		}

		public class bug82431B2 : bug82431B1
		{
		}

		[NotInheritAttribute]
		public class bug82431B3 : bug82431B2
		{
		}

		public class bug82431B4 : bug82431B3
		{
		}

		struct FooStruct
		{
		}

		public class Bug77367
		{
			public void Run (bool b)
			{
			}
		}
	}
}
