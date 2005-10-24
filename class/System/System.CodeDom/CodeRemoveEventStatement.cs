//
// System.CodeDom CodeRemoveEventStatement Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
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

using System.Runtime.InteropServices;

namespace System.CodeDom 
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeRemoveEventStatement
		: CodeStatement 
	{
		private CodeEventReferenceExpression eventRef;
		private CodeExpression listener;

		//
		// Constructors
		//
		public CodeRemoveEventStatement ()
		{
			eventRef = new CodeEventReferenceExpression (null, String.Empty);
		}

		public CodeRemoveEventStatement (CodeEventReferenceExpression eventRef,
						 CodeExpression listener)
		{
			this.eventRef = eventRef;
			this.listener = listener;
		}

		public CodeRemoveEventStatement (CodeExpression targetObject,
						 string eventName,
						 CodeExpression listener)
		{
			this.eventRef = new CodeEventReferenceExpression( targetObject,
									  eventName );
			this.listener = listener;
		}

		//
		// Properties
		//
		public CodeEventReferenceExpression Event {
			get {
				return eventRef;
			}
			set {
				eventRef = value;
			}
		}

		public CodeExpression Listener {
			get {
				return listener; 
			}
			set {
				listener = value;
			}
		}
	}
}

