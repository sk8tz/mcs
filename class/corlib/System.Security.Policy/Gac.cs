//
// System.Security.Policy.Gac
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System;
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
	public sealed class Gac : IIdentityPermissionFactory, IBuiltInEvidence {

		public Gac ()
		{
		}

		public object Copy ()
		{
			return (object) new Gac ();
		}

		public IPermission CreateIdentityPermission (Evidence evidence)
		{
			return new GacIdentityPermission ();
		}

		public override bool Equals (object o)
		{
			if (o == null)
				return false;
			return (o is Gac);
		}

		public override int GetHashCode ()
		{
			return 0; // as documented
		}

		public override string ToString ()
		{
			SecurityElement se = new SecurityElement (GetType ().FullName);
			se.AddAttribute ("version", "1");
			return se.ToString ();
		}

		// IBuiltInEvidence

		int IBuiltInEvidence.GetRequiredSize (bool verbose)
		{
			return (verbose ? 5 : 0);	// as documented
		}

		[MonoTODO]
		int IBuiltInEvidence.InitFromBuffer (char[] buffer, int position)
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.OutputToBuffer (char[] buffer, int position, bool verbose)
		{
			return 0;
		}
	}
}

#endif
