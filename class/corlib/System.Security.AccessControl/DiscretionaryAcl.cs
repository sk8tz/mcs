//
// System.Security.AccessControl.DiscretionaryAcl implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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

using System.Security.Principal;

namespace System.Security.AccessControl
{
	public sealed class DiscretionaryAcl : CommonAcl
	{
		public DiscretionaryAcl (bool isContainer, bool isDS, int capacity)
			: base (isContainer, isDS, capacity)
		{
		}
		
		public DiscretionaryAcl (bool isContainer, bool isDS, RawAcl rawAcl)
			: base (isContainer, isDS, rawAcl)
		{
		}
		
		public DiscretionaryAcl (bool isContainer, bool isDS, byte revision, int capacity)
			: base (isContainer, isDS, revision, capacity)
		{
		}

		public void AddAccess (AccessControlType accessType,
				       SecurityIdentifier sid, int accessMask,
				       InheritanceFlags inheritanceFlags,
				       PropagationFlags propagationFlags)
		{
			throw new NotImplementedException ();
			// CommonAce?
		}
		
		public void AddAccess (AccessControlType accessType,
				       SecurityIdentifier sid, int accessMask,
				       InheritanceFlags inheritanceFlags,
				       PropagationFlags propagationFlags,
				       ObjectAceFlags objectFlags,
				       Guid objectType,
				       Guid inheritedObjectType)
		{
			// ObjectAce?
			throw new NotImplementedException ();
		}
		
		public bool RemoveAccess (AccessControlType accessType,
					  SecurityIdentifier sid,
					  int accessMask,
					  InheritanceFlags inheritanceFlags,
					  PropagationFlags propagationFlags)
		{
			throw new NotImplementedException ();
		}
		
		public bool RemoveAccess (AccessControlType accessType,
					  SecurityIdentifier sid,
					  int accessMask,
					  InheritanceFlags inheritanceFlags,
					  PropagationFlags propagationFlags,
					  ObjectAceFlags objectFlags,
					  Guid objectType,
					  Guid inheritedObjectType)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAccessSpecific (AccessControlType accessType,
						  SecurityIdentifier sid,
						  int accessMask,
						  InheritanceFlags inheritanceFlags,
						  PropagationFlags propagationFlags)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAccessSpecific (AccessControlType accessType,
						  SecurityIdentifier sid,
						  int accessMask,
						  InheritanceFlags inheritanceFlags,
						  PropagationFlags propagationFlags,
						  ObjectAceFlags objectFlags,
						  Guid objectType,
						  Guid inheritedObjectType)
		{
			throw new NotImplementedException ();
		}
		
		public void SetAccess (AccessControlType accessType,
				       SecurityIdentifier sid,
				       int accessMask,
				       InheritanceFlags inheritanceFlags,
				       PropagationFlags propagationFlags)
		{
			throw new NotImplementedException ();
		}
		
		public void SetAccess (AccessControlType accessType,
				       SecurityIdentifier sid,
				       int accessMask,
				       InheritanceFlags inheritanceFlags,
				       PropagationFlags propagationFlags,
				       ObjectAceFlags objectFlags,
				       Guid objectType,
				       Guid inheritedObjectType)
		{
			throw new NotImplementedException ();
		}

		internal override void ApplyCanonicalSortToExplicitAces ()
		{
			int explicitCount = GetCanonicalExplicitAceCount ();
			int explicitDenys = GetCanonicalExplicitDenyAceCount ();

			ApplyCanonicalSortToExplicitAces (0, explicitDenys);
			ApplyCanonicalSortToExplicitAces (explicitDenys, explicitCount - explicitDenys);
		}
		
		internal override bool IsAceMeaningless (GenericAce ace)
		{
			if (base.IsAceMeaningless (ace)) return true;
			
			QualifiedAce qace = ace as QualifiedAce;
			if (null != qace) {
				return !(AceQualifier.AccessAllowed == qace.AceQualifier ||
				         AceQualifier.AccessDenied  == qace.AceQualifier);
			}

			return false;
		}
	}
}

