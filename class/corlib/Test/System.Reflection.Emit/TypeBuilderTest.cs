//
// TypeBuilderTest.cs - NUnit Test Cases for the TypeBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO:
//  - implement a mechnanism for easier testing of null argument exceptions
//  - with overloaded methods like DefineNestedType (), check the defaults
//    on the shorter versions.
//  - ToString on enums with the flags attribute set should print all
//    values which match, e.g. 0 == AutoLayou,AnsiClass,NotPublic
//


using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class TypeBuilderTest : Assertion
{	
	private interface AnInterface {
	}

	private AssemblyBuilder assembly;

	private ModuleBuilder module;

	static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.TypeBuilderTest";

	[SetUp]
	protected void SetUp () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = ASSEMBLY_NAME;

		assembly = 
			Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);

		module = assembly.DefineDynamicModule("module1");
	}

	static int typeIndexer = 0;

	// Return a unique type name
	private string genTypeName () {
		return "t" + (typeIndexer ++);
	}

	private string nullName () {
		return String.Format ("{0}", (char)0);
	}

	public void TestAssembly () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		AssertEquals ("Assembly works",
					  tb.Assembly, assembly);
	}

	public void TestAssemblyQualifiedName () {
		TypeBuilder tb = module.DefineType ("A.B.C.D", TypeAttributes.Public);

		AssertEquals ("AssemblyQualifiedName works",
					  tb.AssemblyQualifiedName, "A.B.C.D, " + assembly.GetName ().FullName);
	}

	public void TestAttributes () {
		TypeAttributes attrs = TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
		TypeBuilder tb = module.DefineType (genTypeName (), attrs);

		AssertEquals ("Attributes works",
					  tb.Attributes, attrs);
	}

	public void TestBaseType () {
		TypeAttributes attrs = TypeAttributes.Public;
		TypeBuilder tb = module.DefineType (genTypeName (), attrs);
		AssertEquals ("BaseType defaults to Object",
					  tb.BaseType, typeof (object));

		TypeBuilder tb2 = module.DefineType (genTypeName (), attrs, tb);
		AssertEquals ("BaseType works",
					  tb2.BaseType, tb);

		/* This does not run under mono
		TypeBuilder tb3 = module.DefineType (genTypeName (),
											 TypeAttributes.Interface |
											 TypeAttributes.Abstract);
		AssertEquals ("Interfaces default to no base type",
					  null, tb3.BaseType);
		*/
	}

	public void TestDeclaringType () {
		TypeAttributes attrs = 0;
		TypeBuilder tb = module.DefineType (genTypeName (), attrs);

		AssertEquals ("Has no declaring type",
					  null, tb.DeclaringType);

		attrs = TypeAttributes.NestedPublic;
		TypeBuilder tb2 = tb.DefineNestedType (genTypeName (), attrs);
		TypeBuilder tb3 = tb2.DefineNestedType (genTypeName (), attrs);
		AssertEquals ("DeclaringType works",
					  tb, tb3.DeclaringType.DeclaringType);
	}

	public void TestFullName () {
		string name = genTypeName ();
		TypeAttributes attrs = 0;
		TypeBuilder tb = module.DefineType (name, attrs);
		AssertEquals ("FullName works",
					  name, tb.FullName);

		string name2 = genTypeName ();
		attrs = TypeAttributes.NestedPublic;
		TypeBuilder tb2 = tb.DefineNestedType (name2, attrs);

		string name3 = genTypeName ();
		attrs = TypeAttributes.NestedPublic;
		TypeBuilder tb3 = tb2.DefineNestedType (name3, attrs);

		AssertEquals ("FullName works on nested types",
					  name + "+" + name2 + "+" + name3, tb3.FullName);
	}

	public void TestGUID () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		try {
			Guid g = tb.GUID;
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}

	public void TestHasElementType () {
		// According to the MSDN docs, this member works, but in reality, it
		// returns a NotSupportedException
		TypeBuilder tb = module.DefineType (genTypeName ());
		try {
			bool b = tb.HasElementType;
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}

	public void TestIsAbstract () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsAbstract);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Abstract);
		AssertEquals ("",
					  true, tb2.IsAbstract);
	}

	public void TestIsAnsiClass () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  true, tb.IsAnsiClass);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.UnicodeClass);
		AssertEquals ("",
					  false, tb2.IsAnsiClass);
	}

	public void TestIsArray () {
		// How can a TypeBuilder be an array ?
		string name = genTypeName ();
		TypeBuilder tb = module.DefineType (name);
		AssertEquals ("IsArray works",
					  false, tb.IsArray);
	}

	public void TestIsAutoClass () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsAutoClass);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.AutoClass);
		AssertEquals ("",
					  true, tb2.IsAutoClass);
	}	

	public void TestIsAutoLayout () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("AutoLayout defaults to true",
					  true, tb.IsAutoLayout);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.ExplicitLayout);
		AssertEquals ("",
					  false, tb2.IsAutoLayout);
	}

	public void TestIsByRef () {
		// How can a TypeBuilder be ByRef ?
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("IsByRef works",
					  false, tb.IsByRef);
	}

	public void TestIsClass () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Most types are classes",
					  true, tb.IsClass);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
		AssertEquals ("Interfaces are not classes",
					  false, tb2.IsClass);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ValueType));
		AssertEquals ("value types are not classes",
					  false, tb3.IsClass);

		TypeBuilder tb4 = module.DefineType (genTypeName (), 0, typeof (Enum));
		AssertEquals ("enums are not classes",
					  false, tb4.IsClass);
	}

	public void TestIsCOMObject () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Probably not",
					  false, tb.IsCOMObject);
	}

	public void TestIsContextful () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsContextful);

		TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (ContextBoundObject));
		AssertEquals ("",
					  true, tb2.IsContextful);
	}

	public void TestIsEnum () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsEnum);

		// This returns true under both mono and MS .NET ???
		TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (ValueType));
		AssertEquals ("value types are not necessary enums",
					  false, tb2.IsEnum);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (Enum));
		AssertEquals ("enums are enums",
					  true, tb3.IsEnum);
	}

	public void TestIsExplicitLayout () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("ExplicitLayout defaults to false",
					  false, tb.IsExplicitLayout);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.ExplicitLayout);
		AssertEquals ("",
					  true, tb2.IsExplicitLayout);
	}

	public void TestIsImport () {
		// How can this be true ?
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsImport);
	}

	public void TestIsInterface () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Most types are not interfaces",
					  false, tb.IsInterface);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
		AssertEquals ("Interfaces are interfaces",
					  true, tb2.IsInterface);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ValueType));
		AssertEquals ("value types are not interfaces",
					  false, tb3.IsInterface);
	}

	public void TestIsLayoutSequential () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("SequentialLayout defaults to false",
					  false, tb.IsLayoutSequential);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.SequentialLayout);
		AssertEquals ("",
					  true, tb2.IsLayoutSequential);
	}

	public void TestIsMarshalByRef () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsMarshalByRef);

		TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (MarshalByRefObject));
		AssertEquals ("",
					  true, tb2.IsMarshalByRef);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ContextBoundObject));
		AssertEquals ("",
					  true, tb3.IsMarshalByRef);
	}

	// TODO: Visibility properties

	public void TestIsPointer () {
		// How can this be true?
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsPointer);
	}

	public void TestIsPrimitive () {
		TypeBuilder tb = module.DefineType ("int");
		AssertEquals ("",
					  false, tb.IsPrimitive);
	}

	public void IsSealed () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Sealed defaults to false",
					  false, tb.IsSealed);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Sealed);
		AssertEquals ("IsSealed works",
					  true, tb2.IsSealed);
	}

	public void IsSerializable () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsSerializable);

		tb.SetCustomAttribute (new CustomAttributeBuilder (typeof (SerializableAttribute).GetConstructors (BindingFlags.Public)[0], null));
		AssertEquals ("",
					  true, tb.IsSerializable);
	}

	public void TestIsSpecialName () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("SpecialName defaults to false",
					  false, tb.IsSpecialName);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.SpecialName);
		AssertEquals ("IsSpecialName works",
					  true, tb2.IsSpecialName);
	}

	public void TestIsUnicodeClass () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  false, tb.IsUnicodeClass);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.UnicodeClass);
		AssertEquals ("",
					  true, tb2.IsUnicodeClass);
	}

	public void TestIsValueType () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Most types are not value types",
					  false, tb.IsValueType);

		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
		AssertEquals ("Interfaces are not value types",
					  false, tb2.IsValueType);

		TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ValueType));
		AssertEquals ("value types are value types",
					  true, tb3.IsValueType);

		TypeBuilder tb4 = module.DefineType (genTypeName (), 0, typeof (Enum));
		AssertEquals ("enums are value types",
					  true, tb4.IsValueType);
	}

	public void TestMemberType () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("A type is a type",
					  MemberTypes.TypeInfo, tb.MemberType);
	}

	public void TestModule () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("Module works",
					  module, tb.Module);
	}

	public void TestName () {
		TypeBuilder tb = module.DefineType ("A");
		AssertEquals ("",
					  "A", tb.Name);

		TypeBuilder tb2 = module.DefineType ("A.B.C.D.E");
		AssertEquals ("",
					  "E", tb2.Name);

		TypeBuilder tb3 = tb2.DefineNestedType ("A");
		AssertEquals ("",
					  "A", tb3.Name);

		/* Is .E a valid name ?
		TypeBuilder tb4 = module.DefineType (".E");
		AssertEquals ("",
					  "E", tb4.Name);
		*/
	}

	public void TestNamespace () {
		TypeBuilder tb = module.DefineType ("A");
		AssertEquals ("",
					  "", tb.Namespace);

		TypeBuilder tb2 = module.DefineType ("A.B.C.D.E");
		AssertEquals ("",
					  "A.B.C.D", tb2.Namespace);

		TypeBuilder tb3 = tb2.DefineNestedType ("A");
		AssertEquals ("",
					  "", tb3.Namespace);

		/* Is .E a valid name ?
		TypeBuilder tb4 = module.DefineType (".E");
		AssertEquals ("",
					  "E", tb4.Name);
		*/		
	}

	public void TestPackingSize () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  PackingSize.Unspecified, tb.PackingSize);

		TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (object),
											 PackingSize.Size16, 16);
		AssertEquals ("",
					  PackingSize.Size16, tb2.PackingSize);
	}

	public void TestReflectedType () {
		// It is the same as DeclaringType, but why?
		TypeBuilder tb = module.DefineType (genTypeName ());
		AssertEquals ("",
					  null, tb.ReflectedType);

		TypeBuilder tb2 = tb.DefineNestedType (genTypeName ());
		AssertEquals ("",
					  tb, tb2.ReflectedType);
	}

	public void TestSize () {
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			AssertEquals ("",
						  0, tb.Size);
			tb.CreateType ();
			AssertEquals ("",
						  0, tb.Size);
		}

		{
			TypeBuilder tb = module.DefineType (genTypeName (), 0, typeof (object),
												PackingSize.Size16, 32);
			AssertEquals ("",
						  32, tb.Size);
		}
	}

	public void TestTypeHandle () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		try {
			RuntimeTypeHandle handle = tb.TypeHandle;
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}

	public void TestTypeInitializer () {
		// According to the MSDN docs, this works, but it doesn't
		/* TODO:
		TypeBuilder tb = module.DefineType (genTypeName ());
		try {
			ConstructorInfo cb = tb.TypeInitializer;
			Fail ();
		}
		catch (NotSupportedException) {
		}
		*/
	}

	public void TestTypeToken () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		TypeToken token = tb.TypeToken;
	}

	public void TestUnderlyingSystemType () {
		//
		// For non-enum types, UnderlyingSystemType should return itself.
		// But if I modify the code to do this, I get an exception in mcs.
		// Reason: the enums created during corlib compilation do not seem
		// to be an enum according to IsEnum.
		//
		/*
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			AssertEquals ("For non-enums this equals itself",
						  tb, tb.UnderlyingSystemType);
		}
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
			AssertEquals ("",
						  tb, tb.UnderlyingSystemType);
		}
		{
			TypeBuilder tb = module.DefineType (genTypeName (), 0, typeof (ValueType));
			AssertEquals ("",
						  tb, tb.UnderlyingSystemType);
		}
		*/
		{
			TypeBuilder tb = module.DefineType (genTypeName (), 0, typeof (Enum));
			try {
				Type t = tb.UnderlyingSystemType;
				Fail ();
			}
			catch (InvalidOperationException) {
			}

			tb.DefineField ("val", typeof (int), 0);
			AssertEquals ("",
						  typeof (int), tb.UnderlyingSystemType);
		}
	}

	public void TestAddInterfaceImplementation () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		try {
			tb.AddInterfaceImplementation (null);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		tb.AddInterfaceImplementation (typeof (AnInterface));
		tb.AddInterfaceImplementation (typeof (AnInterface));

		Type t = tb.CreateType ();
		AssertEquals ("Should merge identical interfaces",
					  tb.GetInterfaces ().Length, 1);

		// Can not be called on a created type
		try {
			tb.AddInterfaceImplementation (typeof (AnInterface));
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestCreateType () {
		// TODO: LOTS OF TEST SHOULD GO THERE
		TypeBuilder tb = module.DefineType (genTypeName ());
		tb.CreateType ();

		// Can not be called on a created type
		try {
			tb.CreateType ();
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestDefineConstructor () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		ConstructorBuilder cb = tb.DefineConstructor (0, 0, null);
		cb.GetILGenerator ().Emit (OpCodes.Ret);
		tb.CreateType ();

		// Can not be called on a created type
		try {
			tb.DefineConstructor (0, 0, null);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestDefineDefaultConstructor () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		tb.DefineDefaultConstructor (0);

		tb.CreateType ();

		// Can not be called on a created type, altough the MSDN docs does not mention this
		try {
			tb.DefineDefaultConstructor (0);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestDefineEvent () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Test invalid arguments
		try {
			tb.DefineEvent (null, 0, typeof (int));
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineEvent ("FOO", 0, null);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineEvent ("", 0, typeof (int));
			Fail ();
		}
		catch (ArgumentException) {
		}

		tb.CreateType ();
		// Can not be called on a created type
		try {
			tb.DefineEvent ("BAR", 0, typeof (int));
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestDefineField () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Check invalid arguments
		try {
			tb.DefineField (null, typeof (int), 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineField ("", typeof (int), 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			// Strangely, 'A<NULL>' is accepted...
			string name = String.Format ("{0}", (char)0);
			tb.DefineField (name, typeof (int), 0);
			Fail ("Names with embedded nulls should be rejected");
		}
		catch (ArgumentException) {
		}

		try {
			tb.DefineField ("A", typeof (void), 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		tb.CreateType ();
		// Can not be called on a created type
		try {
			tb.DefineField ("B", typeof (int), 0);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestDefineInitializedData () {
		TypeBuilder tb = module.DefineType (genTypeName ());
		
		// Check invalid arguments
		try {
			tb.DefineInitializedData (null, new byte[1], 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineInitializedData ("FOO", null, 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineInitializedData ("", new byte[1], 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		// The size of the data is less than or equal to zero ???
		try {
			tb.DefineInitializedData ("BAR", new byte[0], 0);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			string name = String.Format ("{0}", (char)0);
			tb.DefineInitializedData (name, new byte[1], 0);
			Fail ("Names with embedded nulls should be rejected");
		}
		catch (ArgumentException) {
		}

		tb.CreateType ();

		// Can not be called on a created type, altough the MSDN docs does not mention this
		try {
			tb.DefineInitializedData ("BAR2", new byte[1], 0);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestDefineMethod () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Check invalid arguments
		try {
			tb.DefineMethod (null, 0, null, null);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineMethod ("", 0, null, null);
			Fail ();
		}
		catch (ArgumentException) {
		}

		// Check non-virtual methods on an interface
		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
		try {
			tb2.DefineMethod ("FOO", MethodAttributes.Abstract, null, null);
			Fail ();
		}
		catch (ArgumentException) {
		}

		tb.CreateType ();
		// Can not be called on a created type
		try {
			tb.DefineMethod ("bar", 0, null, null);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	// TODO: DefineMethodOverride

	public void TestDefineNestedType () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Check invalid arguments
		try {
			tb.DefineNestedType (null);
			Fail ("Should reject null name");
		}
		catch (ArgumentNullException) {
		}

		try {
			tb.DefineNestedType ("");
			Fail ("Should reject empty name");
		}
		catch (ArgumentException) {
		}

		try {
			tb.DefineNestedType (nullName ());
			Fail ("Should reject name with embedded 0s");
		}
		catch (ArgumentException) {
		}

		// If I fix the code so this works then mcs breaks -> how can mcs
		// works under MS .NET in the first place ???
		/*
		try {
			tb.DefineNestedType ("AA", TypeAttributes.Public, null, null);
			Fail ("Nested visibility must be specified.");
		}
		catch (ArgumentException) {
		}
		*/

		try {
			tb.DefineNestedType ("BB", TypeAttributes.NestedPublic, null,
								 new Type[1]);
			Fail ("Should reject empty interface");
		}
		catch (ArgumentException) {
		}

		// I think this should reject non-interfaces, but it does not
		tb.DefineNestedType ("BB", TypeAttributes.NestedPublic, null,
							 new Type[1] { typeof (object) });

		// Normal invocation
		tb.DefineNestedType ("Nest");

		tb.CreateType ();

		// According to the MSDN docs, this cannnot be called after the type
		// is created, but it works.
		tb.DefineNestedType ("Nest2");

		// According to the MSDN docs, a Sealed class can't contain nested 
		// types, but this is not true
		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Sealed);
		tb2.DefineNestedType ("AA");

		// According to the MSDN docs, interfaces can only contain interfaces,
		// but this is not true
		TypeBuilder tb3 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);

		tb3.DefineNestedType ("AA");

		// Check shorter versions
		{
			TypeBuilder nested = tb.DefineNestedType ("N1");

			AssertEquals (nested.Name, "N1");
			AssertEquals (nested.BaseType, typeof (object));
			AssertEquals (nested.Attributes, TypeAttributes.NestedPrivate);
			AssertEquals (nested.GetInterfaces ().Length, 0);
		}

		// TODO:
	}

	public void TestDefinePInvokeMethod () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		tb.DefinePInvokeMethod ("A", "B", "C", 0, 0, null, null, 0, 0);

		// Try invalid parameters
		try {
			tb.DefinePInvokeMethod (null, "B", "C", 0, 0, null, null, 0, 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}
		// etc...

		// Try invalid attributes
		try {
			tb.DefinePInvokeMethod ("A2", "B", "C", MethodAttributes.Abstract, 0, null, null, 0, 0);
		}
		catch (ArgumentException) {
		}

		// Try an interface parent
		TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);

		try {
			tb2.DefinePInvokeMethod ("A", "B", "C", 0, 0, null, null, 0, 0);
		}
		catch (ArgumentException) {
		}
	}

	public void TestDefineProperty () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		// Check null parameter types
		try {
			tb.DefineProperty ("A", 0, null, new Type[1]);
		}
		catch (ArgumentNullException) {
		}
	}

	/* IsDefined actually works under mono */
	/*
	public void TestIsDefined () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		try {
			tb.IsDefined (typeof (int), true);
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}
	*/

	/* FIXME: This does not work under mono
	public void TestGetEvents () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		try {
			tb.GetEvents ();
			Fail ();
		}
		catch (NotSupportedException) {
		}

		try {
			tb.GetEvents (BindingFlags.Public);
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}
	*/

	public void TestGetEvent () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		try {
			tb.GetEvent ("FOO", BindingFlags.Public);
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}

	public void TestGetMember () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		try {
			tb.GetMember ("FOO", MemberTypes.All, BindingFlags.Public);
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}

	public void TestGetMembers () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		try {
			tb.GetMembers (BindingFlags.Public);
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}

	public void TestGetInterface () {
		TypeBuilder tb = module.DefineType (genTypeName ());

		try {
			tb.GetInterface ("FOO", true);
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}
}
}

