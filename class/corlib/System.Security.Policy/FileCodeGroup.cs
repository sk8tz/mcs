// System.Security.Policy.FileCodeGroup
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak, All rights reserved.

using System.Security.Policy;
using System.Security.Permissions;
using System.Collections;
using System;  // for MonoTODO attribute

namespace System.Security.Policy
{
	[Serializable]
	public sealed class FileCodeGroup : CodeGroup
	{
		FileIOPermissionAccess m_access;

		[MonoTODO("Check if membershipCondition is valid")]
		public FileCodeGroup(IMembershipCondition membershipCondition,
					FileIOPermissionAccess access) 
			: base(membershipCondition, null)
		{
			if (!Enum.IsDefined(typeof(FileIOPermissionAccess), access))
				throw new ArgumentException("Value not defined for FileIOPermissionAccess","access");
			
			m_access = access;
		}

		public override CodeGroup Copy()
		{
			FileCodeGroup copy = new FileCodeGroup(MembershipCondition, m_access);
			foreach (CodeGroup child in Children)
			{
				AddChild(child.Copy());
			}

			return copy;
		}
		
		public override string MergeLogic
		{
			get
			{
				return "Union";
			}
		}

		[MonoTODO]
		public override PolicyStatement Resolve(	Evidence evidence)
		{
			if (null == evidence)
				throw new ArgumentNullException("evidence");

			if (null == PolicyStatement)
				throw new PolicyException();

			if (!MembershipCondition.Check(evidence))
				return null;

			IEnumerator hostEnumerator = evidence.GetHostEnumerator();
			while (hostEnumerator.MoveNext())
			{
				// FIXME: not sure what to do here
				//  How do we check the URL and make a PolicyStatement?
			}
			throw new NotImplementedException();
		}

		public override CodeGroup ResolveMatchingCodeGroups(Evidence evidence)
		{
			if (null == evidence)
				throw new ArgumentNullException("evidence");

			if (!MembershipCondition.Check(evidence))
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

		public override string AttributeString
		{
			get
			{
				return null;
			}
		}

		public override string PermissionSetName
		{
			get
			{
				return "Same directory FileIO - " + m_access.ToString();
			}
		}

		public override bool Equals(object o)
		{
			if (!(o is FileCodeGroup))
				return false;

			if (this.m_access != ((FileCodeGroup)o).m_access)
				return false;

			return Equals((CodeGroup)o, false);
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		protected override void ParseXml(SecurityElement e, PolicyLevel level)
		{
			m_access = (FileIOPermissionAccess)Enum.Parse(typeof(FileIOPermissionAccess), e.Attribute("Access"), true);
		}
		
		protected override void CreateXml(SecurityElement element, PolicyLevel level)
		{
			element.AddAttribute("Access", m_access.ToString());
		}
	}  // public abstract class CodeGroup

}  // namespace System.Security.Policy