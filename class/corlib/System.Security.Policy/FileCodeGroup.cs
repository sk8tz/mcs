//
// System.Security.Policy.FileCodeGroup
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak, All rights reserved.
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Security.Policy;
using System.Security.Permissions;
using System.Collections;
using System;  // for MonoTODO attribute

namespace System.Security.Policy {

	[Serializable]
	public sealed class FileCodeGroup : CodeGroup {

		private FileIOPermissionAccess m_access;
#if NET_2_0
		private CodeGroupGrantScope _scope = CodeGroupGrantScope.Assembly;
#endif

		public FileCodeGroup (IMembershipCondition membershipCondition,
					FileIOPermissionAccess access) 
			: base(membershipCondition, null)
		{
			// note: FileIOPermissionAccess is a [Flag]
			m_access = access;
		}

		// for PolicyLevel (to avoid validation duplication)
		internal FileCodeGroup (SecurityElement e, PolicyLevel level)
			: base (e, level)
		{
		}

		public override CodeGroup Copy ()
		{
			FileCodeGroup copy = new FileCodeGroup (MembershipCondition, m_access);
			foreach (CodeGroup child in Children) {
				copy.AddChild (child.Copy ());	// deep copy
			}
			return copy;
		}
		
		public override string MergeLogic {
			get { return "Union";}
		}

                [MonoTODO ("no children processing")]
		public override PolicyStatement Resolve (Evidence evidence)
		{
			if (null == evidence)
				throw new ArgumentNullException("evidence");

			if (null == PolicyStatement)
				throw new PolicyException();

			if (!MembershipCondition.Check(evidence))
				return null;

			PolicyStatement pst = this.PolicyStatement.Copy ();
			if (this.Children.Count > 0) {
				foreach (CodeGroup cg in this.Children) {
					PolicyStatement child = cg.Resolve (evidence);
					if (child != null) {
						// TODO union
					}
				}
			}
			return pst;
		}

		public override CodeGroup ResolveMatchingCodeGroups(Evidence evidence)
		{
			if (null == evidence)
				throw new ArgumentNullException("evidence");

			if (!MembershipCondition.Check (evidence))
				return null;

			FileCodeGroup matchRoot = new FileCodeGroup(MembershipCondition, m_access);

			foreach (CodeGroup child in Children)
			{
				CodeGroup childMatchingCodeGroup = child.ResolveMatchingCodeGroups(evidence);
				if (childMatchingCodeGroup != null)
					AddChild(childMatchingCodeGroup);
			}

			return matchRoot;
		}

		public override string AttributeString {
			get { return null; }
		}

		public override string PermissionSetName {
			get { return "Same directory FileIO - " + m_access.ToString(); }
		}

#if NET_2_0
		public CodeGroupGrantScope Scope {
			get { return _scope; }
			set { _scope = value; }
		}
#endif

		public override bool Equals(object o)
		{
			if (!(o is FileCodeGroup))
				return false;

			if (this.m_access != ((FileCodeGroup)o).m_access)
				return false;

			return Equals((CodeGroup)o, false);
		}

		public override int GetHashCode ()
		{
			return m_access.GetHashCode ();
		}

		protected override void ParseXml(SecurityElement e, PolicyLevel level)
		{
			m_access = (FileIOPermissionAccess)Enum.Parse(typeof(FileIOPermissionAccess), e.Attribute("Access"), true);
		}
		
		protected override void CreateXml(SecurityElement element, PolicyLevel level)
		{
			element.AddAttribute("Access", m_access.ToString());
		}
	}
}
