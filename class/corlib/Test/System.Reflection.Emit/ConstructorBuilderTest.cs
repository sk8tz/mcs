//
// ConstructorBuilderTest.cs - NUnit Test Cases for the ConstructorBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com

// TODO:
//  - implement 'Signature' (what the hell it does???) and test it
//  - implement Equals and test it

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class ConstructorBuilderTest : Assertion
{	
    private TypeBuilder genClass;

	private ModuleBuilder module;

	private static int typeIndexer = 0;

	[SetUp]
	protected void SetUp () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.Emit.ConstructorBuilderTest";

		AssemblyBuilder assembly 
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);

		module = assembly.DefineDynamicModule("module1");
		
		genClass = module.DefineType(genTypeName (), 
									 TypeAttributes.Public);
	}

	// Return a unique type name
	private string genTypeName () {
		return "class" + (typeIndexer ++);
	}

	public void TestAttributes () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);

		Assert ("Attributes works", 
				(cb.Attributes & MethodAttributes.Public) != 0);
		Assert ("Attributes works", 
				(cb.Attributes & MethodAttributes.SpecialName) != 0);
	}

	public void TestCallingConvention () {
		/* This does not work under MS.NET
		ConstructorBuilder cb3 = genClass.DefineConstructor (
			0, CallingConventions.VarArgs, new Type [0]);
		AssertEquals ("CallingConvetion works",
					  CallingConventions.VarArgs | CallingConventions.HasThis,
					  cb3.CallingConvention);
		*/

		ConstructorBuilder cb4 = genClass.DefineConstructor (
			 MethodAttributes.Static, CallingConventions.Standard, new Type [0]);
		AssertEquals ("Static implies !HasThis",
					  cb4.CallingConvention,
					  CallingConventions.Standard);
	}

	public void TestDeclaringType () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type[0]);

		AssertEquals ("DeclaringType works",
					  cb.DeclaringType, genClass);
	}

	public void TestInitLocals () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type[0]);

		AssertEquals ("InitLocals defaults to true", cb.InitLocals, true);
		cb.InitLocals = false;
		AssertEquals ("InitLocals is settable", cb.InitLocals, false);
	}
	
	[Test]
	public void TestMethodHandle () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [0]);

		RuntimeMethodHandle handle = cb.MethodHandle;
	}

	public void TestName () {
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		AssertEquals ("Name works", ".ctor", cb.Name);

		ConstructorBuilder cb2 = genClass.DefineConstructor (MethodAttributes.Static, 0, new Type [0]);
		AssertEquals ("Static constructors have the right name", ".cctor", cb2.Name);
	}

	public void TestReflectedType () {
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		AssertEquals ("ReflectedType works", 
					  genClass, cb.ReflectedType);
	}

	public void TestReturnType () {
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		AssertEquals ("ReturnType works", 
					  null, cb.ReturnType);
	}

	public void TestDefineParameter () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [2] { typeof(int), typeof(int) });

		// index out of range
		try {
			cb.DefineParameter (0, 0, "param1");
			Fail ();
		} catch (ArgumentOutOfRangeException) {
		}
		try {
			cb.DefineParameter (3, 0, "param1");
			Fail ();
		} catch (ArgumentOutOfRangeException) {
		}

		// Normal usage
		cb.DefineParameter (1, 0, "param1");
		cb.DefineParameter (1, 0, "param1");
		cb.DefineParameter (2, 0, null);

		// Can not be called on a created type
		cb.GetILGenerator ().Emit (OpCodes.Ret);
		tb.CreateType ();
		try {
			cb.DefineParameter (1, 0, "param1");
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestGetCustomAttributes () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [1] {typeof(int)});

		try {
			cb.GetCustomAttributes (true);
			Fail ();
		} catch (NotSupportedException) {
		}

		try {
			cb.GetCustomAttributes (null, true);
			Fail ();
		} catch (NotSupportedException) {
		}
	}

	public void TestMethodImplementationFlags () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [0]);

		AssertEquals ("MethodImplementationFlags defaults to Managed+IL",
					  cb.GetMethodImplementationFlags (),
					  MethodImplAttributes.Managed | MethodImplAttributes.IL);

		cb.SetImplementationFlags (MethodImplAttributes.OPTIL);

		AssertEquals ("SetImplementationFlags works",
					  cb.GetMethodImplementationFlags (),
					  MethodImplAttributes.OPTIL);

		// Can not be called on a created type
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb2 = tb.DefineConstructor (
			 0, 0, new Type [0]);

		cb2.GetILGenerator ().Emit (OpCodes.Ret);
		cb2.SetImplementationFlags (MethodImplAttributes.Managed);
		tb.CreateType ();
		try {
			cb2.SetImplementationFlags (MethodImplAttributes.OPTIL);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestGetModule () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [0]);

		AssertEquals ("GetModule works",
					  module, cb.GetModule ());
	}

	public void TestGetParameters () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [1] {typeof(int)});
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		// Can't be called before CreateType ()
		/* This does not work under mono
		try {
			cb.GetParameters ();
			Fail ();
		} catch (InvalidOperationException) {
		}
		*/

		tb.CreateType ();

		/* This does not work under MS.NET !
		cb.GetParameters ();
		*/
	}

	public void TestGetToken () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [1] {typeof(void)});

		cb.GetToken ();
	}

	public void TestInvoke () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, 
			new Type [1] {typeof(int)});

		try {
			cb.Invoke (null, new object [1] { 42 });
			Fail ();
		} catch (NotSupportedException) {
		}

		try {
			cb.Invoke (null, 0, null, new object [1] { 42 }, null);
			Fail ();
		} catch (NotSupportedException) {
		}
	}

	public void TestIsDefined () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, 
			new Type [1] {typeof(int)});

		try {
			cb.IsDefined (null, true);
			Fail ();
		} catch (NotSupportedException) {
		}
	}

	public void TestSetCustomAttribute () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, 
			new Type [1] {typeof(int)});
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		// Null argument
		try {
			cb.SetCustomAttribute (null);
			Fail ();
		} catch (ArgumentNullException) {
		}

		byte[] custAttrData = { 1, 0, 0, 0, 0};
		Type attrType = Type.GetType
			("System.Reflection.AssemblyKeyNameAttribute");
		Type[] paramTypes = new Type[1];
		paramTypes[0] = typeof(String);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor(paramTypes);

		cb.SetCustomAttribute (ctorInfo, custAttrData);

		// Null arguments again
		try {
			cb.SetCustomAttribute (null, new byte[2]);
			Fail ();
		} catch (ArgumentNullException) {
		}

		try {
			cb.SetCustomAttribute (ctorInfo, null);
			Fail ();
		} catch (ArgumentNullException) {
		}
	}

	// Same as in MethodBuilderTest
	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestAddDeclarativeSecurityAlreadyCreated () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);
		ILGenerator ilgen = cb.GetILGenerator ();
		ilgen.Emit (OpCodes.Ret);
		genClass.CreateType ();

		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		cb.AddDeclarativeSecurity (SecurityAction.Demand, set);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestAddDeclarativeSecurityNullPermissionSet () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);
		cb.AddDeclarativeSecurity (SecurityAction.Demand, null);
	}

	[Test]
	public void TestAddDeclarativeSecurityInvalidAction () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);

		SecurityAction[] actions = new SecurityAction [] { 
			SecurityAction.RequestMinimum,
			SecurityAction.RequestOptional,
			SecurityAction.RequestRefuse };
		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);

		foreach (SecurityAction action in actions) {
			try {
				cb.AddDeclarativeSecurity (action, set);
				Fail ();
			}
			catch (ArgumentException) {
			}
		}
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestAddDeclarativeSecurityDuplicateAction () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);
		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		cb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		cb.AddDeclarativeSecurity (SecurityAction.Demand, set);
	}
}
}
