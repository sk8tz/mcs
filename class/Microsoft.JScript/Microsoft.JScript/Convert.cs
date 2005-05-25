//
// Convert.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) 2005, Novell Inc, (http://novell.com)
//

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
using System.Diagnostics;
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript {

	public sealed class Convert {

		public static bool IsBadIndex (AST ast)
		{
			throw new NotImplementedException ();
		}


		public static double CheckIfDoubleIsInteger (double d)
		{
			if (d == Math.Round (d))
				return d;
			throw new NotImplementedException ();
		}


		public static Single CheckIfSingleIsInteger (Single s)
		{
			if (s == Math.Round (s))
				return s;
			throw new NotImplementedException ();
		}


		public static object Coerce (object value, object type)
		{
			throw new NotImplementedException ();
		}


		public static object CoerceT (object value, Type t, bool explicitOK)
		{
			throw new NotImplementedException ();
		}


		public static object Coerce2 (object value, TypeCode target, 
					      bool truncationPermitted)
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static void ThrowTypeMismatch (object val)
		{
			throw new NotImplementedException ();
		}

		public static bool ToBoolean (double d)
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static bool ToBoolean (object value)
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static bool ToBoolean (object value, bool explicitConversion)
		{
			return false;
		}


		public static object ToForInObject (object value, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public static int ToInt32 (object value)
		{
			throw new NotImplementedException ();
		}


		public static double ToNumber (object value)
		{
			throw new NotImplementedException ();
		}


		public static double ToNumber (string str)
		{
			throw new NotImplementedException ();
		}


		public static object ToNativeArray (object value, RuntimeTypeHandle handle)
		{
			throw new NotImplementedException ();
		}


		public static object ToObject (object value, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public static object ToObject2 (object value, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		internal static string ToString (object obj)
		{
			return Convert.ToString (obj, true);
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static string ToString (object value, bool explicitOK)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.DBNull:
				return "null";

			case TypeCode.String:
			case TypeCode.Double:
				return ic.ToString (null);

			case TypeCode.Object:
				if (value is ArrayObject)
					return ArrayPrototype.toString (value);
				else if (value is BooleanObject)
					return BooleanPrototype.toString (value);
				else if (value is DateObject)
					return DatePrototype.toString (value);
				else if (value is ErrorObject)
					return ErrorPrototype.toString (value);
				else if (value is FunctionObject)
					return FunctionPrototype.toString (value);
				else if (value is NumberObject)
					return NumberPrototype.toString (value, 10);
				else if (value is ObjectPrototype)
					return ObjectPrototype.toString (value);
				else if (value is RegExpObject)
					return RegExpPrototype.toString (value);
				else if (value is StringObject)
					return StringPrototype.toString (value);
				Console.WriteLine ("value.GetType = {0}", value.GetType ());
				throw new NotImplementedException ();
			default:
				Console.WriteLine ("tc = {0}", tc);
				throw new NotImplementedException ();
			}
		}


		public static string ToString (bool b)
		{
			return b ? "true" : "false";
		}


		public static string ToString (double d)
		{	
			IConvertible ic = d as IConvertible;
			return ic.ToString (null);
		}

		//
		// Utility methods
		//
		internal static TypeCode GetTypeCode (object obj, IConvertible ic)
		{
			if (obj == null)
				return TypeCode.Empty;
			else if (ic == null)
				return TypeCode.Object;
			else 
				return  ic.GetTypeCode ();
		}
	}
}
