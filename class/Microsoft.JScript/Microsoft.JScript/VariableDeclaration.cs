//
// VariableDeclaration.cs: The AST representation of a VariableDeclaration.
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.JScript {
	
	public abstract class Decl : AST {
		internal FieldInfo field_info;
		internal LocalBuilder local_builder;
	}
	
	public class VariableDeclaration : Decl {

		internal string id;
		internal Type type;
		internal string type_annot;
		internal AST val;

		internal VariableDeclaration (AST parent, string id, string t, AST init)
		{
			this.parent = parent;
			this.id = id;

			if (t == null)
				this.type = typeof (System.Object);
			else {
				this.type_annot = t;
				// FIXME: resolve the type annotations
				this.type = typeof (System.Object);
			}
			this.val = init;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (id);
			sb.Append (":" + type_annot);
			sb.Append (" = ");

			if (val != null)
				sb.Append (val.ToString ());

			return sb.ToString ();
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (parent == null) {				
				FieldBuilder field_builder;
				TypeBuilder type  = ec.type_builder;
				
				field_builder = type.DefineField (id, this.type, FieldAttributes.Public | FieldAttributes.Static);
				field_info = field_builder;

				if (val != null) {
					val.Emit (ec);
					ig.Emit (OpCodes.Stsfld, field_builder);
				}
			} else {
				local_builder = ig.DeclareLocal (type);
				
				if (val != null) {
					val.Emit (ec);
					ig.Emit (OpCodes.Stloc, local_builder);
				}
			}
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (val != null)
				r = val.Resolve (context);
			context.Enter (id, this);
			return r;
		}
	}
}
