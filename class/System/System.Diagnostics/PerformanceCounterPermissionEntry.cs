//
// System.Diagnostics.PerformanceCounterPermissionEntry.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Security.Permissions;

namespace System.Diagnostics {

	[Serializable]
	public class PerformanceCounterPermissionEntry {

#if NET_2_0
		private const PerformanceCounterPermissionAccess All = (PerformanceCounterPermissionAccess) 0x07;
#endif
		private PerformanceCounterPermissionAccess permissionAccess;
		private string machineName;
		private string categoryName;

		public PerformanceCounterPermissionEntry (PerformanceCounterPermissionAccess permissionAccess,
			string machineName, string categoryName)
		{
#if NET_2_0
			if (machineName == null)
				throw new ArgumentNullException ("machineName");
			if ((permissionAccess | All) != All)
				throw new ArgumentException ("permissionAccess");
#endif
			ResourcePermissionBase.ValidateMachineName (machineName);
			if (categoryName == null)
				throw new ArgumentNullException ("categoryName");

			this.permissionAccess = permissionAccess;
			this.machineName = machineName;
			this.categoryName = categoryName;
		}

		public string CategoryName {
			get { return categoryName; }
		}

		public string MachineName {
			get { return machineName; }
		}

		public PerformanceCounterPermissionAccess PermissionAccess {
			get { return permissionAccess; }
		}

		internal ResourcePermissionBaseEntry CreateResourcePermissionBaseEntry ()
		{
			return new ResourcePermissionBaseEntry ((int) permissionAccess, new string[] { machineName, categoryName });
		} 
	}
}

