//
// Microsoft.Web.UI.DataSourceDropTarget
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
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

#if NET_2_0

using System;

namespace Microsoft.Web.UI
{
	public class DataSourceDropTarget : Behavior
	{
		public DataSourceDropTarget ()
		{
		}

		protected override void AddAttributesToElement (ScriptTextWriter writer)
		{
			base.AddAttributesToElement (writer);
		}

		protected override void InitializeTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
		{
			base.InitializeTypeDescriptor (typeDescriptor);

			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("acceptedDataTypes", ScriptType.Array, false, "AcceptedDataTypes"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("append", ScriptType.Boolean, false, "Append"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("target", ScriptType.String, false, "Target"));
			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("property", ScriptType.String, false, "Property"));
		}

		string acceptedDataTypes = null;
		public string AcceptedDataTypes {
			get {
				return acceptedDataTypes;
			}
			set {
				acceptedDataTypes = value;
			}
		}

		bool append = true;
		public bool Append {
			get {
				return append;
			}
			set {
				append = value;
			}
		}

		string property = "data";
		public string Property {
			get {
				return property;
			}
			set {
				property = value;
			}
		}

		string target = null;
		public string Target {
			get {
				return target;
			}
			set {
				target = value;
			}
		}

		public override string TagName {
			get {
				return "dataSourceDropTarget";
			}
		}
	}
}

#endif
