//
// System.Web.UI.AttributeCollection.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com
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
using System.Globalization;

namespace System.Web.UI {

	public sealed class AttributeCollection
	{
		private StateBag bag;
		private CssStyleCollection styleCollection;
		
		public AttributeCollection (StateBag bag)
		{
			this.bag = bag;
		}

		public int Count {
			get { return bag.Count; }
		}

		public CssStyleCollection CssStyle {
			get {
				if (styleCollection == null)
					styleCollection = new CssStyleCollection (bag);
				return styleCollection;
			}
		}

		public string this [string key] {
			get { return bag [key] as string; }

			set {
				if (0 == String.Compare (key, "style", true, CultureInfo.InvariantCulture)) {
					CssStyle.Clear();
					CssStyle.FillStyle (value);
					key = "style";	// Needs to be always lowercase
				}
				bag.Add (key, value);
			}
		}

		public ICollection Keys {
			get { return bag.Keys; }
		}

		public void Add (string key, string value)
		{
			if (0 == String.Compare (key, "style", true, CultureInfo.InvariantCulture)) {
				CssStyle.Clear();
				CssStyle.FillStyle (value);
				key = "style";	// Needs to be always lowercase
			}
			bag.Add (key, value);
		}

		public void AddAttributes (HtmlTextWriter writer)
		{
			foreach (string key in bag.Keys) {
				string value = bag [key] as string;
				writer.AddAttribute (key, value);
			}
		}

		public void Clear ()
		{
			bag.Clear ();
		}

		public void Remove (string key)
		{
			bag.Remove (key);
		}

		public void Render (HtmlTextWriter writer)
		{
			foreach (string key in bag.Keys) {
				string value = bag [key] as string;
				if (value != null)
					writer.WriteAttribute (key, value, true);
			}
		}
	}
}
