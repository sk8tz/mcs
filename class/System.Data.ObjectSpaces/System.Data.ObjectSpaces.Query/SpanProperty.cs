//
// System.Data.ObjectSpaces.Query.SpanProperty
//
//
// Author:
//	Richard Thombs (stony@stony.org)
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

using System;
using System.Xml;

namespace System.Data.ObjectSpaces.Query
{
	[MonoTODO()]
	public class SpanProperty
	{
		public SpanProperty parent;

		[MonoTODO()]
		public SpanProperty(string name)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public SpanProperty(string name,SpanPropertyCollection subSpan)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public string ToXmlString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public void WriteXml(XmlWriter xmlw)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public string FullName
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public string Name
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public SpanProperty Owner
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public SpanPropertyCollection SubSpan
		{
			get { throw new NotImplementedException(); }
		}
	}
}

#endif
