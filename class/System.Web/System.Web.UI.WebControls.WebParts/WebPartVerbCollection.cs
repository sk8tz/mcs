﻿//
// System.Web.UI.WebControls.WebParts.WebPartVerbCollection.cs
//
// Authors:
//      Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Collections;

namespace System.Web.UI.WebControls.WebParts
{
	public class WebPartVerbCollection : ReadOnlyCollectionBase
	{
		public WebPartVerbCollection ()
		{			
		}

		public WebPartVerbCollection (ICollection verbs)
		{
			if (verbs == null)
				throw new ArgumentNullException ("Parameter verbs cannot be null");
			InnerList.AddRange (verbs);
		}

		public WebPartVerbCollection (WebPartVerbCollection existingVerbs, 
								ICollection verbs)
		{
			if (existingVerbs == null)
				throw new ArgumentNullException ("Parameter existingVerbs cannot be null");
			if (verbs == null)
				throw new ArgumentNullException ("Parameter verbs cannot be null");
			InnerList.AddRange (existingVerbs.InnerList);
			InnerList.AddRange (verbs);
		}

		public static readonly WebPartVerbCollection Empty = new WebPartVerbCollection ();

		public bool Contains (WebPartVerb value)
		{
			return InnerList.Contains (value);
		}

		[MonoTODO]
		public void CopyTo (WebPartVerb [] array, int index)
		{ 
			throw new NotImplementedException();
		}

		public int IndexOf(WebPartVerb value)
		{
			return (InnerList.IndexOf(value));
		}

		public WebPartVerb this [int index ] {
			get { return (WebPartVerb)InnerList[index]; }
		}

	}
}
#endif