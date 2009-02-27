//
// XmlReaderBodyWriter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class XmlReaderBodyWriter : BodyWriter
	{
		XmlDictionaryReader reader;
		string xml;

		public XmlReaderBodyWriter (string xml)
			: base (true)
		{
			this.xml = xml;
		}

		public XmlReaderBodyWriter (XmlDictionaryReader reader)
			: base (false)
		{
			reader.MoveToContent ();
			this.reader = reader;
		}

		protected override BodyWriter OnCreateBufferedCopy (
			int maxBufferSize)
		{
			if (xml == null)
				throw new NotSupportedException ();
			return new XmlReaderBodyWriter (xml);
		}

		protected override void OnWriteBodyContents (
			XmlDictionaryWriter writer)
		{
			XmlReader r = reader ?? XmlReader.Create (new StringReader (xml));
			r.MoveToContent ();
			writer.WriteNode (r, false);
		}
	}
}
