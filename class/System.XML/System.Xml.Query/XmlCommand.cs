//
// System.Xml.Query.XmlCommand
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Novell Inc, 2004
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

using System.IO;
using System.Xml.XPath;

namespace System.Xml.Query
{
        public abstract class XmlCommand
        {
		public event QueryEventHandler OnProcessingEvent;

		public abstract void Execute (
			IXPathNavigable contextDocument,
			XmlWriter writer);

		public abstract void Execute (
			IXPathNavigable contextDocument,
			XmlArgumentList args,
			XmlWriter writer);

		public abstract void Execute (
			XmlResolver dataSource,
			XmlArgumentList args,
			XmlWriter writer);

		public abstract void Execute (
			IXPathNavigable contextDocument,
			XmlResolver dataSource,
			XmlArgumentList args,
			XmlWriter writer);

		public abstract void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			XmlArgumentList argList, 
			Stream results);

		public abstract void Execute (
			string contextDocumentUri,
			XmlResolver dataSources,
			XmlArgumentList argList,
			TextWriter results);

		public abstract void Execute (
			string contextDocumentUri,
			XmlResolver dataSources,
			XmlArgumentList argList,
			XmlWriter results);
        }
}

#endif // NET_2_0
