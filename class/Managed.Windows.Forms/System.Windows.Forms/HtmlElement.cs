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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita	<avidigal@novell.com>

#if NET_2_0

using System;
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	[MonoTODO ("Needs Implementation")]
	public sealed class HtmlElement
	{
		private IElement element;
		internal HtmlElement (IElement element)
		{
			this.element = element;
		}

		#region Properties
		public HtmlElementCollection All
		{
			get {
				return new HtmlElementCollection (this.element.All);
			}
		}

		public string InnerHtml
		{
			get { return this.element.InnerHTML; }
			set { throw new NotImplementedException (); }
		}

		public string InnerText
		{
			get { return this.element.InnerText; }
			set { this.element.InnerText = value; }
		}

		public string Id
		{
			get { return element.GetAttribute ("id"); }
		}

		public string Name
		{
			get { return element.GetAttribute ("name"); }
		}

		public HtmlElement FirstChild
		{
			get { return new HtmlElement ((IElement)element.FirstChild); }
		}
		#endregion

	}
}

#endif