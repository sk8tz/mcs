//
// System.Security.Permissions.FileDialogPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

//
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

using System;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class FileDialogPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private FileDialogPermissionAccess _access;

		// Constructors

		public FileDialogPermission (PermissionState state)
		{
			switch (state) {
				case PermissionState.None:
					_access = FileDialogPermissionAccess.None;
					break;
				case PermissionState.Unrestricted:
					_access = FileDialogPermissionAccess.OpenSave;
					break;
				default:
					throw new ArgumentException ("Invalid PermissionState", "state");
			}
		}

		public FileDialogPermission (FileDialogPermissionAccess access)
		{
			Access = access;
		}

		// Properties

		public FileDialogPermissionAccess Access { 
			get { return _access; }
			set { 
				switch (value) {
					case FileDialogPermissionAccess.None:
					case FileDialogPermissionAccess.Open:
					case FileDialogPermissionAccess.Save:
					case FileDialogPermissionAccess.OpenSave:
						_access = value;
						break;
					default:
						throw new ArgumentException ("Invalid FileDialogPermissionAccess", "access");
				}
			}
		}

		// Methods

		public override IPermission Copy () 
		{
			return new FileDialogPermission (_access);
		}

		public override void FromXml (SecurityElement esd) 
		{
			if (esd == null)
				throw new ArgumentNullException ("esd");
			if (esd.Tag != "IPermission")
				throw new ArgumentException ("not IPermission");
			if (!(esd.Attributes ["class"] as string).StartsWith ("System.Security.Permissions.FileDialogPermission"))
				throw new ArgumentException ("not FileDialogPermission");
			if ((esd.Attributes ["version"] as string) != "1")
				throw new ArgumentException ("wrong version");

			switch (esd.Attributes ["Access"] as string) {
				case null:
					if ((esd.Attributes ["Unrestricted"] as string) == "true") {
						_access = FileDialogPermissionAccess.OpenSave;
					}
					else
						_access = FileDialogPermissionAccess.None;
					break;
				case "Open":
					_access = FileDialogPermissionAccess.Open;
					break;
				case "Save":
					_access = FileDialogPermissionAccess.Save;
					break;
			}
		}

		public override IPermission Intersect (IPermission target) 
		{
			if (target == null)
				return null;
			if (! (target is FileDialogPermission))
				throw new ArgumentException ("wrong type");

			FileDialogPermission o = (FileDialogPermission) target;
			if (IsUnrestricted ())
				return o.Copy ();
			if (o.IsUnrestricted ())
				return Copy ();

			FileDialogPermission ep = new FileDialogPermission (PermissionState.None);
			// note: there are no more OpenSave cases (as they're Unrestricted)
			if ((_access == FileDialogPermissionAccess.Open) && (o.Access == FileDialogPermissionAccess.Open))
				ep.Access = FileDialogPermissionAccess.Open;
			if ((_access == FileDialogPermissionAccess.Save) && (o.Access == FileDialogPermissionAccess.Save))
				ep.Access = FileDialogPermissionAccess.Save;
			return ((ep.Access == FileDialogPermissionAccess.None) ? null : ep);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			if (target == null)
				return false;

			if (! (target is FileDialogPermission))
				throw new ArgumentException ("wrong type");

			FileDialogPermission o = (FileDialogPermission) target;
			if (IsUnrestricted ())
				return o.IsUnrestricted ();
			else if (o.IsUnrestricted ())
				return true;

			return ((_access | o.Access) == _access);
		}

		public bool IsUnrestricted () 
		{
			return (_access == FileDialogPermissionAccess.OpenSave);
		}

#if !NET_2_0
		// Same results as base class - so why is it overrided ?
		public override string ToString () 
		{
			return base.ToString ();
		}
#endif

		public override SecurityElement ToXml () 
		{
			SecurityElement se = Element (this, 1);
			switch (_access) {
				case FileDialogPermissionAccess.Open:
					se.AddAttribute ("Access", "Open");
					break;
				case FileDialogPermissionAccess.Save:
					se.AddAttribute ("Access", "Save");
					break;
				case FileDialogPermissionAccess.OpenSave:
					se.AddAttribute ("Unrestricted", "true");
					break;
			}
			return se;
		}

		public override IPermission Union (IPermission target)
		{
			if (target == null)
				return Copy ();
			if (! (target is FileDialogPermission))
				throw new ArgumentException ("wrong type");

			FileDialogPermission o = (FileDialogPermission) target;
			if (IsUnrestricted () || o.IsUnrestricted ())
				return new FileDialogPermission (PermissionState.Unrestricted);

			FileDialogPermission ep = (FileDialogPermission) Copy ();
			ep.Access = _access | o.Access;
			return ep;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 1;
		}
	}
}
