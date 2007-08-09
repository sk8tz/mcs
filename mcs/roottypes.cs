//
// roottypes.cs: keeps a tree representation of the generated code
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

namespace Mono.CSharp
{
	// <summary>
	//   We store here all the toplevel types that we have parsed,
	//   this is the root of all information we have parsed.
	// </summary>
	public sealed class RootTypes : TypeContainer
	{
		// TODO: It'd be so nice to have generics
		Hashtable anonymous_types;

		public RootTypes ()
			: base (null, null, MemberName.Null, null, Kind.Root)
		{
			types = new ArrayList ();
			anonymous_types = new Hashtable ();
		}

		public void AddAnonymousType (AnonymousTypeClass type)
		{
			ArrayList existing = (ArrayList)anonymous_types [type.Parameters.Count];
			if (existing == null) {
				existing = new ArrayList ();
				anonymous_types.Add (type.Parameters.Count, existing);
			}
			existing.Add (type);
		}

		public override bool IsClsComplianceRequired ()
		{
			return true;
		}

		public AnonymousTypeClass GetAnonymousType (ArrayList parameters)
		{
			ArrayList candidates = (ArrayList)anonymous_types [parameters.Count];
			if (candidates == null)
				return null;

			int i;
			foreach (AnonymousTypeClass at in candidates) {
				for (i = 0; i < parameters.Count; ++i) {
					if (!parameters [i].Equals (at.Parameters [i]))
						break;
				}

				if (i == candidates.Count)
					return at;
			}

			return null;
		}

		public override bool GetClsCompliantAttributeValue ()
		{
			return CodeGen.Assembly.IsClsCompliant;
		}

		public override string GetSignatureForError ()
		{
			return "";
		}

		protected override bool AddMemberType (DeclSpace ds)
		{
			if (!AddToContainer (ds, ds.Name))
				return false;
			ds.NamespaceEntry.NS.AddDeclSpace (ds.Basename, ds);
			return true;
		}

		public override TypeContainer AddPartial (TypeContainer nextPart)
		{
			return AddPartial (nextPart, nextPart.Name);
		}
	}

	public class RootDeclSpace : DeclSpace {
		public RootDeclSpace (NamespaceEntry ns)
			: base (ns, null, MemberName.Null, null)
		{
			PartialContainer = RootContext.ToplevelTypes;
		}

		public override AttributeTargets AttributeTargets {
			get { throw new InternalErrorException ("should not be called"); }
		}

		public override string DocCommentHeader {
			get { throw new InternalErrorException ("should not be called"); }
		}

		public override bool Define ()
		{
			throw new InternalErrorException ("should not be called");
		}

		public override TypeBuilder DefineType ()
		{
			throw new InternalErrorException ("should not be called");
		}

		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf, MemberFilter filter, object criteria)
		{
			throw new InternalErrorException ("should not be called");
		}

		public override MemberCache MemberCache {
			get { return PartialContainer.MemberCache; }
		}

		public override bool GetClsCompliantAttributeValue ()
		{
			return PartialContainer.GetClsCompliantAttributeValue ();
		}

		public override bool IsClsComplianceRequired ()
		{
			return PartialContainer.IsClsComplianceRequired ();
		}
	}
}
