//
// StrongNameIdentityPermission.cs: Strong Name Identity Permission
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class StrongNameIdentityPermission : CodeAccessPermission, IBuiltInPermission {
	
		private StrongNamePublicKeyBlob publickey;
		private string name;
		private Version version;
	
		public StrongNameIdentityPermission (PermissionState state) 
		{
			if (state == PermissionState.Unrestricted)
				throw new ArgumentException ("state");
			name = String.Empty;
			version = new Version (0, 0);
		}
	
		public StrongNameIdentityPermission (StrongNamePublicKeyBlob blob, string name, Version version) 
		{
			if (blob == null)
				throw new ArgumentNullException ("blob");
			if (name == null)
				throw new ArgumentNullException ("name");
			if (version == null)
				throw new ArgumentNullException ("version");
	
			publickey = blob;
			this.name = name;
			this.version = version;
		}
	
		~StrongNameIdentityPermission () 
		{
		}
	
		public string Name { 
			get { return name; }
			set { name = value; }
		}
	
		public StrongNamePublicKeyBlob PublicKey { 
			get { return publickey; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				publickey = value;
			}
		}
	
		public Version Version { 
			get { return version; }
			set { version = value; }
		}
	
		public override IPermission Copy () 
		{
			return new StrongNameIdentityPermission (publickey, name, version);
		}
	
		[MonoTODO]
		public override void FromXml (SecurityElement e) 
		{
			if (e == null)
				throw new ArgumentNullException ("e");
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public override IPermission Intersect (IPermission target) 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public override SecurityElement ToXml () 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public override IPermission Union (IPermission target) 
		{
			throw new NotImplementedException ();
		}
	
		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 11;
		}
	} 
}
