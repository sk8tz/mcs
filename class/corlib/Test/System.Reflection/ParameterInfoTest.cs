//
// ParameterInfoTest - NUnit Test Cases for the ParameterInfo class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System;
using System.Threading;
using System.Reflection;
#if !TARGET_JVM
using System.Reflection.Emit;
#endif // TARGET_JVM
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
	public class Marshal1 : ICustomMarshaler
	{
		public static ICustomMarshaler GetInstance (string s)
		{
			return new Marshal1 ();
		}

		public void CleanUpManagedData (object managedObj)
		{
		}

		public void CleanUpNativeData (IntPtr pNativeData)
		{
		}

		public int GetNativeDataSize ()
		{
			return 4;
		}

		public IntPtr MarshalManagedToNative (object managedObj)
		{
			return IntPtr.Zero;
	 	}

		public object MarshalNativeToManaged (IntPtr pNativeData)
		{
			return null;
		}
}

	[TestFixture]
	public class ParameterInfoTest
	{
		[Test]
		public void IsDefined_AttributeType_Null ()
		{
			MethodInfo mi = typeof (object).GetMethod ("Equals", 
				new Type [1] { typeof (object) });
			ParameterInfo pi = mi.GetParameters () [0];

			try {
				pi.IsDefined ((Type) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("attributeType", ex.ParamName, "#6");
			}
		}


#if NET_2_0
		public enum ParamEnum {
			None = 0,
			Foo = 1,
			Bar = 2
		};

		public static void paramMethod (int i, [In] int j, [Out] int k, [Optional] int l, [In,Out] int m, [DefaultParameterValue (ParamEnum.Foo)] ParamEnum n) {
		}
#if !TARGET_JVM // No support for extern methods in TARGET_JVM
		[DllImport ("foo")]
		public extern static void marshalAsMethod (
			[MarshalAs(UnmanagedType.Bool)]int p0, 
			[MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr)] string [] p1,
			[MarshalAs( UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (Marshal1), MarshalCookie = "5")] object p2);
#endif
		[Test]
		public void DefaultValueEnum () {
			ParameterInfo[] info = typeof (ParameterInfoTest).GetMethod ("paramMethod").GetParameters ();

			Assert.AreEqual (typeof (ParamEnum), info [5].DefaultValue.GetType (), "#1");
			Assert.AreEqual (ParamEnum.Foo, info [5].DefaultValue, "#2");
		}

		[Test]
		public void PseudoCustomAttributes () {
			ParameterInfo[] info = typeof (ParameterInfoTest).GetMethod ("paramMethod").GetParameters ();
			Assert.AreEqual (0, info[0].GetCustomAttributes (true).Length, "#A1");
			Assert.AreEqual (1, info[1].GetCustomAttributes (typeof (InAttribute), true).Length, "#A2");
			Assert.AreEqual (1, info[2].GetCustomAttributes (typeof (OutAttribute), true).Length, "#A3");
			Assert.AreEqual (1, info[3].GetCustomAttributes (typeof (OptionalAttribute), true).Length, "#A4");
			Assert.AreEqual (2, info[4].GetCustomAttributes (true).Length, "#A5");

#if !TARGET_JVM // No support for extern methods in TARGET_JVM
			ParameterInfo[] pi = typeof (ParameterInfoTest).GetMethod ("marshalAsMethod").GetParameters ();
			MarshalAsAttribute attr;

			attr = (MarshalAsAttribute)(pi [0].GetCustomAttributes (true) [0]);
			Assert.AreEqual (UnmanagedType.Bool, attr.Value, "#B");

			attr = (MarshalAsAttribute)(pi [1].GetCustomAttributes (true) [0]);
			Assert.AreEqual (UnmanagedType.LPArray, attr.Value, "#C1");
			Assert.AreEqual (UnmanagedType.LPStr, attr.ArraySubType, "#C2");

			attr = (MarshalAsAttribute)(pi [2].GetCustomAttributes (true) [0]);
			Assert.AreEqual (UnmanagedType.CustomMarshaler, attr.Value, "#D1");
			Assert.AreEqual ("5", attr.MarshalCookie, "#D2");
			Assert.AreEqual (typeof (Marshal1), Type.GetType (attr.MarshalType), "#D3");
#endif
		}
#endif
	}
}
