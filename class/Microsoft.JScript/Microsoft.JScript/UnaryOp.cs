//
// UnaryOp.cs:
//
// Author:
//	 Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript {

	public abstract class UnaryOp : Exp {

		internal AST operand;
		internal JSToken oper;
	}
}
