//
// MethodOnTypeBuilderInstTest - NUnit Test Cases for MethodOnTypeBuilderInst
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2008 Gert Driesen
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

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class MethodOnTypeBuilderInstTest
	{
		private static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.MethodOnTypeBuilderInstTest";
		
		private AssemblyBuilder assembly;
		private ModuleBuilder module;
		private MethodBuilder mb_create;
		private MethodBuilder mb_edit;
		private Type typeBarOfT;
		private Type typeBarOfInt32;
		private MethodInfo method_create;
		private MethodInfo method_edit;

		[SetUp]
		public void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;

			assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (
				assemblyName, AssemblyBuilderAccess.RunAndSave,
				Path.GetTempPath ());

			module = assembly.DefineDynamicModule ("module1");

			TypeBuilder tb = module.DefineType ("Bar");
			GenericTypeParameterBuilder [] typeParams = tb.DefineGenericParameters ("T");

			ConstructorBuilder cb = tb.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
			ILGenerator ig = cb.GetILGenerator ();
			ig.Emit (OpCodes.Ret);

			typeBarOfT = tb.MakeGenericType (typeParams [0]);

			mb_create = tb.DefineMethod ("create",
				MethodAttributes.Public | MethodAttributes.Static,
				typeBarOfT, Type.EmptyTypes);
			ig = mb_create.GetILGenerator ();
			ig.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (
				typeBarOfT, cb));
			ig.Emit (OpCodes.Ret);

			mb_edit = tb.DefineMethod ("edit",
				MethodAttributes.Public | MethodAttributes.Static,
				typeBarOfT, Type.EmptyTypes);
			ig = mb_edit.GetILGenerator ();
			ig.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (
				typeBarOfT, cb));
			ig.Emit (OpCodes.Ret);
			mb_edit.SetParameters (mb_edit.DefineGenericParameters ("X"));

			typeBarOfInt32 = tb.MakeGenericType (typeof (int));

			method_create = TypeBuilder.GetMethod (typeBarOfInt32, mb_create);
			method_edit = TypeBuilder.GetMethod (typeBarOfInt32, mb_edit);
		}

		[Test]
		public void Attributes ()
		{
			MethodAttributes attrs;

			attrs = method_create.Attributes;
			Assert.AreEqual (MethodAttributes.PrivateScope |
				MethodAttributes.Public | MethodAttributes.Static,
				attrs, "#1");
			attrs = method_edit.Attributes;
			Assert.AreEqual (MethodAttributes.PrivateScope |
				MethodAttributes.Public | MethodAttributes.Static,
				attrs, "#2");
		}

		[Test]
		public void CallingConvention ()
		{
			CallingConventions conv;

			conv = method_create.CallingConvention;
			Assert.AreEqual (CallingConventions.Standard, conv, "#1");
			conv = method_edit.CallingConvention;
			Assert.AreEqual (CallingConventions.Standard, conv, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void ContainsGenericParameters ()
		{
			try {
				bool genparam = method_create.ContainsGenericParameters;
				Assert.Fail ("#A1:" + genparam);
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				bool genparam = method_edit.ContainsGenericParameters;
				Assert.Fail ("#B1:" + genparam);
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void DeclaringType ()
		{
			Assert.AreSame (typeBarOfInt32, method_create.DeclaringType, "#1");
			Assert.AreSame (typeBarOfInt32, method_edit.DeclaringType, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetBaseDefinition ()
		{
			try {
				method_create.GetBaseDefinition ();
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.GetBaseDefinition ();
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // GetCustomAttributes (Boolean)
		[Category ("NotWorking")]
		public void GetCustomAttributes1 ()
		{
			try {
				method_create.GetCustomAttributes (false);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.GetCustomAttributes (false);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // GetCustomAttributes (Type, Boolean)
		[Category ("NotWorking")]
		public void GetCustomAttributes2 ()
		{
			try {
				method_create.GetCustomAttributes (typeof (FlagsAttribute), false);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.GetCustomAttributes (typeof (FlagsAttribute), false);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void GetGenericArguments ()
		{
			Type [] args;
			
			args = method_create.GetGenericArguments ();
			Assert.IsNull (args, "#A");

			args = method_edit.GetGenericArguments ();
			Assert.IsNotNull (args, "#B1");
			Assert.AreEqual (1, args.Length, "#B2");
			Assert.AreEqual ("X", args [0].Name, "#B3");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetGenericMethodDefinition ()
		{
			MethodInfo method_def;

			method_def = method_create.GetGenericMethodDefinition ();
			Assert.IsNotNull (method_def, "#A1");
			Assert.AreSame (mb_create, method_def, "#A2");

			method_def = method_edit.GetGenericMethodDefinition ();
			Assert.IsNotNull (method_def, "#B1");
			Assert.AreSame (mb_edit, method_def, "#B2");
		}

		[Test]
		public void GetMethodImplementationFlags ()
		{
			MethodImplAttributes flags;
			
			flags = method_create.GetMethodImplementationFlags ();
			Assert.AreEqual (MethodImplAttributes.Managed, flags, "#1");
			flags = method_edit.GetMethodImplementationFlags ();
			Assert.AreEqual (MethodImplAttributes.Managed, flags, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetParameters ()
		{
			try {
				method_create.GetParameters ();
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.GetParameters ();
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Invoke ()
		{
			try {
				method_create.Invoke (null, BindingFlags.Default, null,
					new object [0], CultureInfo.InvariantCulture);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.Invoke (null, BindingFlags.Default, null,
					new object [0], CultureInfo.InvariantCulture);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void IsDefined ()
		{
			try {
				method_create.IsDefined (typeof (FlagsAttribute), false);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.IsDefined (typeof (FlagsAttribute), false);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void IsGenericMethodDefinition ()
		{
			Assert.IsFalse (method_create.IsGenericMethodDefinition, "#1");
			Assert.IsTrue (method_edit.IsGenericMethodDefinition, "#2");
		}

		[Test]
		public void IsGenericMethod ()
		{
			Assert.IsFalse (method_create.IsGenericMethod, "#1");
			Assert.IsTrue (method_edit.IsGenericMethod, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void MakeGenericMethod ()
		{
			try {
				method_create.MakeGenericMethod (typeof (int));
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// create is not a GenericMethodDefinition.
				// MakeGenericMethod may only be called on a
				// method for which MethodBase.IsGenericMethodDefinition
				// is true
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			MethodInfo genEdit = method_edit.MakeGenericMethod (typeof (int));
			Assert.IsFalse (genEdit.ContainsGenericParameters, "#B1");
			Assert.IsTrue (genEdit.IsGenericMethod, "#B2");
			Assert.IsFalse (genEdit.IsGenericMethodDefinition, "#B3");
		}

		[Test]
		public void MemberType ()
		{
			Assert.AreEqual (MemberTypes.Method, method_create.MemberType, "#1");
			Assert.AreEqual (MemberTypes.Method, method_edit.MemberType, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void MethodHandle ()
		{
			try {
				RuntimeMethodHandle handle = method_create.MethodHandle;
				Assert.Fail ("#A1:" + handle);
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				RuntimeMethodHandle handle = method_edit.MethodHandle;
				Assert.Fail ("#B1:" + handle);
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void Module ()
		{
			Assert.AreSame (module, method_create.Module, "#1");
			Assert.AreSame (module, method_edit.Module, "#2");
		}

		[Test]
		public void Name ()
		{
			Assert.AreEqual ("create", method_create.Name, "#1");
			Assert.AreEqual ("edit", method_edit.Name, "#2");
		}

		[Test]
		public void ReflectedType ()
		{
			Assert.AreSame (typeBarOfInt32, method_create.ReflectedType, "#1");
			Assert.AreSame (typeBarOfInt32, method_edit.ReflectedType, "#2");
		}

		[Test]
		public void ReturnParameter ()
		{
			try {
				ParameterInfo ret = method_create.ReturnParameter;
				Assert.Fail ("#A1:" + (ret != null));
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				ParameterInfo ret = method_create.ReturnParameter;
				Assert.Fail ("#B1:" + (ret != null));
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void ReturnType ()
		{
			Type ret;
			
			ret = method_create.ReturnType;
			Assert.AreSame (typeBarOfT, ret, "#1");
			ret = method_edit.ReturnType;
			Assert.AreSame (typeBarOfT, ret, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void ReturnTypeCustomAttributes ()
		{
			try {
				ICustomAttributeProvider attr_prov = method_create.ReturnTypeCustomAttributes;
				Assert.Fail ("#A1:" + (attr_prov != null));
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				ICustomAttributeProvider attr_prov = method_edit.ReturnTypeCustomAttributes;
				Assert.Fail ("#B1:" + (attr_prov != null));
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}
	}
}

#endif
