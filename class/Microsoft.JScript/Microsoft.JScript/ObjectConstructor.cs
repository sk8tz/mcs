//
// ObjectConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;

namespace Microsoft.JScript {

	public class ObjectConstructor : ScriptFunction {

		public JSObject ConstructObject ()
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public new Object CreateInstance(params Object[] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public Object Invoke(params Object[] args)
		{
			throw new NotImplementedException ();
		}
	}
}