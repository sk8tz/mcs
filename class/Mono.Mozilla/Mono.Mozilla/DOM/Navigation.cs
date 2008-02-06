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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Navigation: DOMObject, INavigation
	{

		internal nsIWebNavigation navigation;
		
		public Navigation (WebBrowser control, nsIWebNavigation webNav) : base (control)
		{
			this.navigation = webNav;
		}


		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.navigation = null;
				}
			}
			base.Dispose(disposing);
		}		
		#endregion	

		#region INavigation Members

		public bool CanGoBack {
			get {
				if (navigation == null)
					return false;
					
				bool canGoBack;
				navigation.getCanGoBack (out canGoBack);
				return canGoBack;
			}
		}

		public bool CanGoForward {
			get {
				if (navigation == null)
					return false;

				bool canGoForward;
				navigation.getCanGoForward (out canGoForward);
				return canGoForward;
			}
		}

		public bool Back ()
		{
			if (navigation == null)
				return false;

			control.Reset ();
			return navigation.goBack () == 0;
		}

		public bool Forward ()
		{
			if (navigation == null)
				return false;

			control.Reset ();
			return navigation.goForward () == 0;
		}

		public void Home ()
		{
			control.Reset ();
			Base.Home (control);
		}

		public void Reload ()
		{
			Reload (ReloadOption.None);
		}

		public void Reload (ReloadOption option)
		{
			if (navigation == null)
				return;

			control.Reset ();
			if (option == ReloadOption.None)
				navigation.reload ((uint)LoadFlags.None);
			else if (option == ReloadOption.Proxy)
				navigation.reload ((uint) LoadFlags.BypassLocalCache);
			else if (option == ReloadOption.Full)
				navigation.reload ((uint) LoadFlags.BypassProxy);
		}

		public void Stop ()
		{
			if (navigation == null)
				return;

			navigation.stop ((uint)StopOption.All);
		}
		
		public void Go (string url)
		{
			if (navigation == null)
				return;

			control.Reset ();
			navigation.loadURI (url, (uint)LoadFlags.None, null, null, null);
		}

		public void Go (string url, LoadFlags flags) 
		{
			if (navigation == null)
				return;
				
			control.Reset ();
			navigation.loadURI (url, (uint)flags, null, null, null);
		}

		#endregion

		internal Document Document
		{
			get {
				nsIDOMDocument doc;
				this.navigation.getDocument (out doc);
				return new Document (control, doc as nsIDOMHTMLDocument);
			}
		}
		
		public override int GetHashCode () {
			return this.navigation.GetHashCode ();
		}		
	}
}
