//
// PropertyReference.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Collections;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class PropertyReference {
	
		OldExpression	parent;
		string		propertyName;
	
		public PropertyReference (OldExpression parent)
		{
			if (parent == null)
				throw new Exception ("Parent Expression needed to find project.");
			this.parent = parent;
		}

		public void ParseSource (string source)
		{
			if (source.Length < 3)
				throw new ArgumentException ("Invalid property.");
			propertyName  = source.Substring (2, source.Length - 3);
		}
		
		public string ConvertToString ()
		{
			if (propertyName != String.Empty) {
				Project p = parent.Project;
				BuildProperty bp;
				bp = p.EvaluatedProperties [propertyName];
				if (bp != null)
					return bp.FinalValue;
				else
					return String.Empty;
			} else
				return String.Empty;
		}
	}
}

#endif