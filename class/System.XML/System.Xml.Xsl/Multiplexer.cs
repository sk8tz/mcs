// System.Xml.Xsl.XslTransform
//
// Authors:
//	Tim Coleman <tim@timcoleman.com>
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Copyright 2002 Tim Coleman
// (c) 2003 Ximian Inc. (http://www.ximian.com)
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
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Policy;
using System.Xml.XPath;

namespace System.Xml.Xsl {
	public sealed class XslTransform {

		XmlResolver xmlResolver = new XmlUrlResolver ();
		XslTransformImpl impl;

		#region Constructors
		public XslTransform ()
		{
			if (Environment.GetEnvironmentVariable ("MONO_UNMANAGED_XSLT") == null)
				impl = new ManagedXslTransform ();
			else
				impl = new UnmanagedXslTransform ();
		}
		#endregion
		
		[MonoTODO ("Security check.")]
#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public XmlResolver XmlResolver {
			set {
				 xmlResolver = value;
			}
		}
		
		#region Transform
#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public XmlReader Transform (IXPathNavigable input, XsltArgumentList args)
		{
			return Transform (input.CreateNavigator (), args, xmlResolver);
		}

#if NET_1_1
		public XmlReader Transform (IXPathNavigable input, XsltArgumentList args, XmlResolver resolver)
#else
		XmlReader Transform (IXPathNavigable input, XsltArgumentList args, XmlResolver resolver)
#endif
		{
			return Transform (input.CreateNavigator (), args, resolver);
		}

#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public XmlReader Transform (XPathNavigator input, XsltArgumentList args)
		{
			return Transform (input, args, xmlResolver);
		}
#if NET_1_1
		public XmlReader Transform (XPathNavigator input, XsltArgumentList args, XmlResolver resolver)
#else
		XmlReader Transform (XPathNavigator input, XsltArgumentList args, XmlResolver resolver)
#endif
		{
			// todo: is this right?
			MemoryStream stream = new MemoryStream ();
			Transform (input, args, new XmlTextWriter (stream, null), resolver);
			stream.Position = 0;
			return new XmlTextReader (stream);
		}

#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public void Transform (IXPathNavigable input, XsltArgumentList args, TextWriter output)
		{
			Transform (input.CreateNavigator (), args, output, xmlResolver);
		}
#if NET_1_1
		public void Transform (IXPathNavigable input, XsltArgumentList args, TextWriter output, XmlResolver resolver)
#else
		void Transform (IXPathNavigable input, XsltArgumentList args, TextWriter output, XmlResolver resolver)
#endif
		{
			Transform (input.CreateNavigator (), args, output, resolver);
		}
		
#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public void Transform (IXPathNavigable input, XsltArgumentList args, Stream output)
		{
			Transform (input.CreateNavigator (), args, output, xmlResolver);
		}
#if NET_1_1
		public void Transform (IXPathNavigable input, XsltArgumentList args, Stream output, XmlResolver resolver)
#else
		void Transform (IXPathNavigable input, XsltArgumentList args, Stream output, XmlResolver resolver)
#endif
		{
			Transform (input.CreateNavigator (), args, output, resolver);
		}
		
#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public void Transform (IXPathNavigable input, XsltArgumentList args, XmlWriter output)
		{
			Transform (input.CreateNavigator (), args, output, xmlResolver);
		}
#if NET_1_1
		public void Transform (IXPathNavigable input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
#else
		void Transform (IXPathNavigable input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
#endif
		{
			Transform (input.CreateNavigator (), args, output, resolver);
		}

#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output)
		{
			impl.Transform (input, args, output, xmlResolver);
		}
#if NET_1_1
		public void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
#else
		void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
#endif
		{
			impl.Transform (input, args, output, resolver);
		}

#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public void Transform (XPathNavigator input, XsltArgumentList args, Stream output)
		{
			impl.Transform (input, args, output, xmlResolver);		
		}
#if NET_1_1
		public void Transform (XPathNavigator input, XsltArgumentList args, Stream output, XmlResolver resolver)
#else
		void Transform (XPathNavigator input, XsltArgumentList args, Stream output, XmlResolver resolver)
#endif
		{
			impl.Transform (input, args, output, resolver);
		}

#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output)
		{
			impl.Transform (input, args, output, xmlResolver);
		}
#if NET_1_1
		public void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output, XmlResolver resolver)
#else
		void Transform(XPathNavigator input, XsltArgumentList args, TextWriter output, XmlResolver resolver)
#endif
		{
			impl.Transform (input, args, output, resolver);
		}
		
#if NET_1_1
		[Obsolete ("You should pass XmlResolver to Transform() method", false)]
#endif
		public void Transform (string inputfile, string outputfile)
		{ 
			impl.Transform (inputfile, outputfile, xmlResolver);
		}

#if NET_1_1
		public void Transform (string inputfile, string outputfile, XmlResolver resolver)
#else
		void Transform (string inputfile, string outputfile, XmlResolver resolver)
#endif
		{
			impl.Transform (inputfile, outputfile, resolver);
		}
		#endregion

		#region Load
		public void Load (string url)
		{
			Load (url, null);
		}
		
		public void Load (string url, XmlResolver resolver)
		{
			impl.Load (url, resolver);
		}

#if NET_1_1
		[Obsolete("You should pass evidence.", false)]
#endif
		public void Load (XmlReader stylesheet)
		{
			Load (stylesheet, null, null);
		}

#if NET_1_1
		[Obsolete("You should pass evidence.", false)]
#endif
		public void Load (XmlReader stylesheet, XmlResolver resolver)
		{
			Load (stylesheet, resolver, null);
		}

#if NET_1_1
		[Obsolete("You should pass evidence.", false)]
#endif
		public void Load (XPathNavigator stylesheet)
		{
			Load (stylesheet, null, null);
		}

#if NET_1_1
		[Obsolete("You should pass evidence.", false)]
#endif
		public void Load (XPathNavigator stylesheet, XmlResolver resolver)
		{
			Load (stylesheet, resolver, null);
		}
		
#if NET_1_1
		[Obsolete("You should pass evidence.", false)]
#endif
		public void Load (IXPathNavigable stylesheet)
		{
			Load (stylesheet.CreateNavigator(), null);
		}

#if NET_1_1
		[Obsolete("You should pass evidence.", false)]
#endif
		public void Load (IXPathNavigable stylesheet, XmlResolver resolver)
		{
			Load (stylesheet.CreateNavigator(), resolver);
		}

		// Introduced in .NET 1.1
#if NET_1_1
		public void Load (IXPathNavigable stylesheet, XmlResolver resolver, Evidence evidence)
#else
		internal void Load (IXPathNavigable stylesheet, XmlResolver resolver, Evidence evidence)
#endif
		{
			impl.Load (stylesheet.CreateNavigator(), resolver, evidence);
		}

#if NET_1_1
		public void Load (XPathNavigator stylesheet, XmlResolver resolver, Evidence evidence)
#else
		internal void Load (XPathNavigator stylesheet, XmlResolver resolver, Evidence evidence)
#endif
		{
			impl.Load (stylesheet, resolver, evidence);
		}

#if NET_1_1
		public void Load (XmlReader stylesheet, XmlResolver resolver, Evidence evidence)
#else
		internal void Load (XmlReader stylesheet, XmlResolver resolver, Evidence evidence)
#endif
		{
			impl.Load (stylesheet, resolver, null);
		}
		#endregion
	}
}
