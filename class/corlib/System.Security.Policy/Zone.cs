//
// System.Security.Policy.Zone
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
using System.Security;
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
	public sealed class Zone : IIdentityPermissionFactory, IBuiltInEvidence	{
		SecurityZone zone;
		
		public Zone (SecurityZone zone)
		{
			if (!Enum.IsDefined (typeof (SecurityZone), zone))
				throw new ArgumentException ("invalid zone");

			this.zone = zone;
		}

		public object Copy ()
		{
			return new Zone (zone);
		}

		public IPermission CreateIdentityPermission (Evidence evidence)
		{
			return new ZoneIdentityPermission (zone);
		}

		[MonoTODO("This depends on zone configuration in IE")]
		public static Zone CreateFromUrl (string url)
		{
			if (url == null)
				throw new ArgumentNullException ("url");
			// while waiting for our own tool...
			if (url.ToUpper ().StartsWith ("FILE://"))
				return new Zone (SecurityZone.MyComputer);
			else
				return new Zone (SecurityZone.Untrusted);
		}

		public override bool Equals (object o)
		{
			if (!(o is Zone))
				return false;

			return (((Zone) o).zone == zone);
		}

		public override int GetHashCode ()
		{
			return (int) zone;
		}

		public override string ToString ()
		{
			SecurityElement se = new SecurityElement (GetType ().FullName);
			se.AddAttribute ("version", "1");
			se.AddChild (new SecurityElement ("Zone", zone.ToString ()));

			return se.ToString ();
		}

		int IBuiltInEvidence.GetRequiredSize (bool verbose)
		{
			return 3;
		}

		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) {
			int new_zone = (int) buffer [position++];
			new_zone += buffer [position++];
			return position;
		}

		int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose)
		{
			buffer [position++] = '\x0003';
			buffer [position++] = (char) (((int) zone) >> 16);
			buffer [position++] = (char) (((int) zone) & 0x0FFFF);
			return position;
		}

		public SecurityZone SecurityZone
		{
			get { return zone; }
		}
	}
}

