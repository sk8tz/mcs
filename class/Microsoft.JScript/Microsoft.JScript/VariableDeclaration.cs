//
// VariableDeclaration.cs: The AST representation of a VariableDeclaration.
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Text;

namespace Microsoft.JScript.Tmp {

	public class VariableDeclaration : Statement {

		private string id;
		private string type;
		private AST assignExp;

		internal VariableDeclaration ()
		{}


		public string Id {
			get { return id; }
			set { id = value; }
		}

		
		public string Type {
			get { return type; }
			set { type = value; }
		}


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			// FIXME: we must add the string 
			// representation of assignExp, too.
			sb.Append (Id);
			
			return sb.ToString ();
		}
	}
}
