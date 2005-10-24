//
// System.CodeDom CodeMethodInvokeExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001 Ximian, Inc.
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
	public class CodeMethodReferenceExpression
		: CodeExpression 
	{
		private string methodName;
		private CodeExpression targetObject;
#if NET_2_0
		CodeTypeReferenceCollection typeArguments;
#endif
		
		//
		// Constructors
		//
		public CodeMethodReferenceExpression ()
		{
			methodName = String.Empty;
		}

		public CodeMethodReferenceExpression (CodeExpression targetObject, 
						      string methodName)
		{
			this.targetObject = targetObject;
			this.methodName = methodName;
		}

#if NET_2_0
		public CodeMethodReferenceExpression (CodeExpression targetObject, 
			string methodName, params CodeTypeReference[] typeParameters) :
			this (targetObject, methodName)
		{
			if (typeParameters != null && typeParameters.Length > 0)
				TypeArguments.AddRange (typeParameters);
		}
#endif

		//
		// Properties
		//
		public string MethodName {
			get {
				return methodName;
			}
			set {
				methodName = value;
			}
		}

		public CodeExpression TargetObject {
			get {
				return targetObject;
			}
			set {
				targetObject = value;
			}
		}

#if NET_2_0
		[ComVisible (false)]
		public CodeTypeReferenceCollection TypeArguments {
			get {
				if (typeArguments == null)
					typeArguments = new CodeTypeReferenceCollection ();
				return typeArguments;
			}
		}
#endif
	}
}
