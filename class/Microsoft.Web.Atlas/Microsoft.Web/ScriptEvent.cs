//
// Microsoft.Web.ScriptEvent
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

namespace Microsoft.Web
{
	public class ScriptEvent
	{
		IScriptObject owner;
		string name;
		bool supportsActions;
		string handler;
		ActionCollection actions;

		public ScriptEvent (IScriptObject owner, string name, bool supportsActions)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");
			if (name == null)
				throw new ArgumentNullException ("name");

			this.owner = owner;
			this.name = name;
			this.supportsActions = supportsActions;
			this.handler = "";
		}

		public ActionCollection Actions {
			get {
				if (actions == null)
					actions = new ActionCollection(owner, !supportsActions);

				return actions;
			}
		}

		public string Handler {
			get { return handler; }
			set { handler = (value == null ? "" : value); }
		}

		public string Name {
			get { return name; }
		}

		public bool SupportsActions {
			get { return supportsActions; }
		}

		public void RenderActions (ScriptTextWriter writer)
		{
			if (Actions.Count == 0)
				return;

			writer.WriteStartElement (Name);

			foreach (Action a in Actions) {
				a.RenderAction (writer);
			}

			writer.WriteEndElement ();
		}

		public void RenderHandlers (ScriptTextWriter writer)
		{
			writer.WriteAttributeString (Name, Handler);
		}
	}
}

#endif
