//
// Microsoft.Web.BehaviorCollection
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
using System.Collections.ObjectModel;

namespace Microsoft.Web
{
	public class BehaviorCollection : Collection<Behavior>, IScriptObject
	{
		public BehaviorCollection (IScriptObject owner)
		{
			throw new NotImplementedException ();
		}

		public BehaviorCollection (IScriptObject owner, bool readOnly)
		{
			throw new NotImplementedException ();
		}

		protected void ClearItems ()
		{
			throw new NotImplementedException ();
		}

		protected void InsertItem (int index, Behavior item)
		{
			throw new NotImplementedException ();
		}

		protected void RemoveItem (int index)
		{
			throw new NotImplementedException ();
		}

		protected void SetItem (int index, Behavior item)
		{
			throw new NotImplementedException ();
		}

		public string ID {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public IScriptObject Owner {
			get {
				throw new NotImplementedException ();
			}
		}

		ScriptTypeDescriptor IScriptObject.GetTypeDescriptor ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
