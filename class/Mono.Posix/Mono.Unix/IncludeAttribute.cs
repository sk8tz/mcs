//
// IncludeAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) Novell, Inc.  
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

namespace Mono.Unix {

	[AttributeUsage (AttributeTargets.Assembly)]
	internal class IncludeAttribute : Attribute {
		string [] includes;
		string [] defines;
		
		public IncludeAttribute (string [] includes)
		{
			this.includes = includes;
		}

		public IncludeAttribute (string [] includes, string [] defines)
		{
			this.includes = includes;
			this.defines = defines;
		}

		public string [] Includes {
			get {
				if (includes == null)
					return new string [0];
				return includes;
			}
		}

		public string [] Defines {
			get {
				if (defines == null)
					return new string [0];
				return defines;
			}
		}
		
	}
}
