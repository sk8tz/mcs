//
// System.CodeDom CodeNamespace Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
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
	public class CodeNamespace
		: CodeObject
	{
		private CodeCommentStatementCollection comments;
		private CodeNamespaceImportCollection imports;
		private CodeTypeDeclarationCollection types;
		private string name;

		//
		// Constructors
		//
		public CodeNamespace()
		{
			name = String.Empty;
		}

		public CodeNamespace(string name)
		{
			this.name = name;
		}

		//
		// Properties
		//
		public CodeCommentStatementCollection Comments {
			get {
				if ( comments == null ) {
					comments = new CodeCommentStatementCollection();
					if ( PopulateComments != null )
						PopulateComments( this, EventArgs.Empty );
				}
				return comments;
			}
		}

		public CodeNamespaceImportCollection Imports {
			get {
				if ( imports == null ) {
					imports = new CodeNamespaceImportCollection();
					if ( PopulateImports != null )
						PopulateImports( this, EventArgs.Empty );
				}
				return imports;
			}
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public CodeTypeDeclarationCollection Types {
			get {
				if ( types == null ) {
					types = new CodeTypeDeclarationCollection();
					if ( PopulateTypes != null )
						PopulateTypes( this, EventArgs.Empty );
				}
				return types;
			}
		}

		//
		// Events
		//
		public event EventHandler PopulateComments;
		
		public event EventHandler PopulateImports;
		
		public event EventHandler PopulateTypes;
	}
}
