﻿//
// ExtenderControl.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
//
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
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace System.Web.UI
{
	[DefaultProperty ("TargetControlID")]
	[ParseChildren (true)]
	[NonVisualControl]
	[PersistChildren (false)]
	public abstract class ExtenderControl : Control, IExtenderControl
	{
		protected ExtenderControl () { }

		[DefaultValue ("")]
		[IDReferenceProperty]
		[Category ("Behavior")]
		public string TargetControlID {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool Visible {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		protected abstract IEnumerable<ScriptDescriptor> GetScriptDescriptors (Control targetControl);

		protected abstract IEnumerable<ScriptReference> GetScriptReferences ();

		protected override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);
		}

		protected override void Render (HtmlTextWriter writer) {
		}

		#region IExtenderControl Members

		IEnumerable<ScriptDescriptor> IExtenderControl.GetScriptDescriptors (Control targetControl) {
			return GetScriptDescriptors (targetControl);
		}

		IEnumerable<ScriptReference> IExtenderControl.GetScriptReferences () {
			return GetScriptReferences ();
		}

		#endregion
	}
}
