//
// System.Data.Odbc.OdbcPermissionAttribute
//
// Authors:
//	Umadevi S (sumadevi@novell.com)
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

using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Odbc {

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | 
			AttributeTargets.Struct | AttributeTargets.Constructor |
			AttributeTargets.Method, AllowMultiple=true,
			Inherited=false)]
	[Serializable]
	public sealed class OdbcPermissionAttribute : DBDataPermissionAttribute {

		#region Constructors 

		public OdbcPermissionAttribute (SecurityAction action) 
			: base (action)
		{
		}

		#endregion

		#region Properties
		#endregion

		#region Methods

		public override IPermission CreatePermission () 
		{
			if (base.Unrestricted) {
				return new OdbcPermission (PermissionState.Unrestricted);
			}

			OdbcPermission p = new OdbcPermission (PermissionState.None, this.AllowBlankPassword);
			p.Add (this.ConnectionString, this.KeyRestrictions, this.KeyRestrictionBehavior);
			return p;
		}

		#endregion
	}
}
