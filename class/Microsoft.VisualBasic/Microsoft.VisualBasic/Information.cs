//
// Information.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Chris J Breisch
//     2003 Tipic, Inc. (http://www.tipic.com)
//     2004 Rafael Teixeira
//

using System;

namespace Microsoft.VisualBasic 
{
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class Information {
		// Declarations
		// Constructors
		// Properties
		// Methods
		[MonoTODO]
		public static Microsoft.VisualBasic.ErrObject Err () { 
			return Microsoft.VisualBasic.CompilerServices.ProjectData.Err;
		}
		[MonoTODO]
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
		public static System.Int32 Erl () { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean IsArray (System.Object VarName) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean IsDate (System.Object Expression) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean IsDBNull (System.Object Expression) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean IsNothing (System.Object Expression) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean IsError (System.Object Expression) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean IsReference (System.Object Expression) { throw new NotImplementedException (); }

		public static System.Boolean IsNumeric (System.Object Expression) 
		{ 
			if (Expression == null || Expression is DateTime)
				return false;

			if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal ||
				Expression is Single || Expression is Double)
				return true;

			try {
				if (Expression is string)
					Double.Parse(Expression as string);
				else
					Double.Parse(Expression.ToString());
				return true;
			} catch {} // just dismiss errors but return false

			return false;
		}

		[MonoTODO]
		public static System.Int32 LBound (System.Array Array, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] System.Int32 Rank) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 UBound (System.Array Array, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] System.Int32 Rank) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String TypeName (System.Object VarName) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String SystemTypeName (System.String VbName) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String VbTypeName (System.String UrtName) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 QBColor (System.Int32 Color) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 RGB (System.Int32 Red, System.Int32 Green, System.Int32 Blue) { throw new NotImplementedException (); }
		[MonoTODO]
		public static Microsoft.VisualBasic.VariantType VarType (System.Object VarName) { throw new NotImplementedException (); }
		// Events
	};
}
