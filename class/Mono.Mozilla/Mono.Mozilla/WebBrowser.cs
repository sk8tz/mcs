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

#undef debug

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla
{
	/// <summary>
	/// Summary description for WebBrowser.
	/// </summary>
	public class WebBrowser : Component, IWebBrowser, ICallback
	{
		private bool loaded;
		private DOM.Document document;
		internal DOM.Navigation navigation;
		internal Platform platform;
		internal Platform enginePlatform;

		private string statusText;
		
		public WebBrowser (Platform platform)
		{
			this.platform = platform;
			loaded = Base.Init (this, platform);
		}

		public bool Load (IntPtr handle, int width, int height)
		{
			Base.Bind (this, handle, width, height);
			return loaded;
		}

		public void Shutdown ()
		{
			Base.Shutdown (this);
		}
		
		internal void Reset ()
		{
			this.document = null;	
		}

		public IWindow Window {
			get {
				if (Navigation != null) {
					nsIWebBrowserFocus webBrowserFocus = (nsIWebBrowserFocus) (navigation.navigation);
					nsIDOMWindow window;
					webBrowserFocus.getFocusedWindow (out window);
					return new DOM.Window (this, window) as IWindow;
				}
				return null;
			}
		}

		public IDocument Document {
			get {
				if (Navigation != null && document == null) {
					document = navigation.Document;
				}
				return document as IDocument;
			}
		}

		public INavigation Navigation {
			get {
				if (navigation == null) {
					
					nsIWebNavigation webNav = Base.GetWebNavigation (this);
					navigation = new DOM.Navigation (this, webNav);
				}
				return navigation as INavigation;
			}
		}
		
		public string StatusText {
			get { return statusText; }
		}

		#region Layout
		public void FocusIn (FocusOption focus)
		{
			Base.Focus (this, focus);
		}
		public void FocusOut ()
		{
			Base.Blur (this);
		}

		public void Activate ()
		{
			Base.Activate (this);
		}
		public void Deactivate ()
		{
			Base.Deactivate (this);
		}

		public void Resize (int width, int height)
		{
			Base.Resize (this, width, height);
		}
		#endregion

		#region Events
		static object KeyDownEvent = new object ();
		public event EventHandler KeyDown
		{
			add { Events.AddHandler (KeyDownEvent, value); }
			remove { Events.RemoveHandler (KeyDownEvent, value); }
		}

		static object KeyPressEvent = new object ();
		public event EventHandler KeyPress
		{
			add { Events.AddHandler (KeyPressEvent, value); }
			remove { Events.RemoveHandler (KeyPressEvent, value); }
		}
		static object KeyUpEvent = new object ();
		public event EventHandler KeyUp
		{
			add { Events.AddHandler (KeyUpEvent, value); }
			remove { Events.RemoveHandler (KeyUpEvent, value); }
		}
		static object MouseClickEvent = new object ();
		public event EventHandler MouseClick
		{
			add { Events.AddHandler (MouseClickEvent, value); }
			remove { Events.RemoveHandler (MouseClickEvent, value); }
		}
		static object MouseDoubleClickEvent = new object ();
		public event EventHandler MouseDoubleClick
		{
			add { Events.AddHandler (MouseDoubleClickEvent, value); }
			remove { Events.RemoveHandler (MouseDoubleClickEvent, value); }
		}
		static object MouseDownEvent = new object ();
		public event EventHandler MouseDown
		{
			add { Events.AddHandler (MouseDownEvent, value); }
			remove { Events.RemoveHandler (MouseDownEvent, value); }
		}
		static object MouseEnterEvent = new object ();
		public event EventHandler MouseEnter
		{
			add { Events.AddHandler (MouseEnterEvent, value); }
			remove { Events.RemoveHandler (MouseEnterEvent, value); }
		}
		static object MouseLeaveEvent = new object ();
		public event EventHandler MouseLeave
		{
			add { Events.AddHandler (MouseLeaveEvent, value); }
			remove { Events.RemoveHandler (MouseLeaveEvent, value); }
		}
		static object MouseMoveEvent = new object ();
		public event EventHandler MouseMove
		{
			add { Events.AddHandler (MouseMoveEvent, value); }
			remove { Events.RemoveHandler (MouseMoveEvent, value); }
		}
		static object MouseUpEvent = new object ();
		public event EventHandler MouseUp
		{
			add { Events.AddHandler (MouseUpEvent, value); }
			remove { Events.RemoveHandler (MouseUpEvent, value); }
		}

		static object FocusEvent = new object ();
		public event EventHandler Focus
		{
			add { Events.AddHandler (FocusEvent, value); }
			remove { Events.RemoveHandler (FocusEvent, value); }
		}

		static object BlurEvent = new object ();
		public event EventHandler Blur
		{
			add { Events.AddHandler (BlurEvent, value); }
			remove { Events.RemoveHandler (BlurEvent, value); }
		}

		static object CreateNewWindowEvent = new object ();
		public event CreateNewWindowEventHandler CreateNewWindow
		{
			add { Events.AddHandler (CreateNewWindowEvent, value); }
			remove { Events.RemoveHandler (CreateNewWindowEvent, value); }
		}

		static object AlertEvent = new object ();
		public event AlertEventHandler Alert
		{
			add { Events.AddHandler (AlertEvent, value); }
			remove { Events.RemoveHandler (AlertEvent, value); }
		}


		static object TransferringEvent = new object ();
		public event EventHandler Transferring
		{
			add { Events.AddHandler (TransferringEvent, value); }
			remove { Events.RemoveHandler (TransferringEvent, value); }
		}

		static object DocumentCompletedEvent = new object ();
		public event EventHandler DocumentCompleted
		{
			add { Events.AddHandler (DocumentCompletedEvent, value); }
			remove { Events.RemoveHandler (DocumentCompletedEvent, value); }
		}

		static object CompletedEvent = new object ();
		public event EventHandler Completed
		{
			add { Events.AddHandler (CompletedEvent, value); }
			remove { Events.RemoveHandler (CompletedEvent, value); }
		}

		static object LoadEvent = new object ();
		public event EventHandler Loaded
		{
			add { Events.AddHandler (LoadEvent, value); }
			remove { Events.RemoveHandler (LoadEvent, value); }
		}

		static object UnloadEvent = new object ();
		public event EventHandler Unloaded
		{
			add { Events.AddHandler (UnloadEvent, value); }
			remove { Events.RemoveHandler (UnloadEvent, value); }
		}
		#endregion


		#region ICallback

		public void OnWidgetLoaded ()
		{
			//			loaded = true;
		}

		public void OnJSStatus ()
		{
			// TODO:  Add WebBrowser.OnJSStatus implementation
		}

		public void OnLinkStatus ()
		{
			// TODO:  Add WebBrowser.OnLinkStatus implementation
		}

		public void OnDestroyBrowser ()
		{
			// TODO:  Add WebBrowser.OnDestroyBrowser implementation
		}

		public void OnClientSizeTo (Int32 width, Int32 height)
		{
			// TODO:  Add WebBrowser.OnClientSizeTo implementation
		}

		public void OnFocusNext ()
		{
			// TODO:  Add WebBrowser.OnFocusNext implementation
		}

		public void OnFocusPrev ()
		{
			// TODO:  Add WebBrowser.OnFocusPrev implementation
		}

		public void OnTitleChanged ()
		{
			// TODO:  Add WebBrowser.OnTitleChanged implementation
		}

		public void OnShowTooltipWindow (string tiptext, Int32 x, Int32 y)
		{
			// TODO:  Add WebBrowser.OnShowTooltipWindow implementation
		}

		public void OnHideTooltipWindow ()
		{
			// TODO:  Add WebBrowser.OnHideTooltipWindow implementation
		}

		public void OnStateNetStart ()
		{
			// TODO:  Add WebBrowser.OnStateNetStart implementation
		}

		public void OnStateNetStop ()
		{
			// TODO:  Add WebBrowser.OnStateNetStop implementation
		}

		public void OnStateSpecial (UInt32 stateFlags, Int32 status)
		{
			// TODO:  Add WebBrowser.OnStateSpecial implementation
		}

		public void OnStateChange (Int32 status, UInt32 state)
		{
#if debug
			System.Text.StringBuilder s = new System.Text.StringBuilder ();
			if ((state & (uint) StateFlags.Start) != 0) {
				s.Append ("Start\t");
			}
			if ((state & (uint) StateFlags.Redirecting) != 0) {
				s.Append ("Redirecting\t");
			}
			if ((state & (uint) StateFlags.Transferring) != 0) {
				s.Append ("Transferring\t");
			}
			if ((state & (uint) StateFlags.Negotiating) != 0) {
				s.Append ("Negotiating\t");
			}
			if ((state & (uint) StateFlags.Stop) != 0) {
				s.Append ("Stop\t");
			}
			if ((state & (uint) StateFlags.IsRequest) != 0) {
				s.Append ("Request\t");
			}
			if ((state & (uint) StateFlags.IsDocument) != 0) {
				s.Append ("Document\t");
			}
			if ((state & (uint) StateFlags.IsNetwork) != 0) {
				s.Append ("Network\t");
			}
			if ((state & (uint) StateFlags.IsWindow) != 0) {
				s.Append ("Window\t");
			}
			Console.Error.WriteLine (s.ToString ());
#endif
			if ((state & (uint) StateFlags.Transferring) != 0 && 
				(state & (uint) StateFlags.IsRequest) != 0 &&
				(state & (uint) StateFlags.IsDocument) != 0
				)
			{
			    EventHandler eh = (EventHandler) (Events[TransferringEvent]);
			    if (eh != null) {
			        EventArgs e = new EventArgs ();
			        eh (this, e);
			    }
			} else if ((state & (uint) StateFlags.Stop) != 0 && 
				(state & (uint) StateFlags.IsDocument) != 0
				)
			{
			    EventHandler eh = (EventHandler) (Events[DocumentCompletedEvent]);
			    if (eh != null) {
			        EventArgs e = new EventArgs ();
			        eh (this, e);
			    }
			} else if ((state & (uint) StateFlags.Stop) != 0 && 
				(state & (uint) StateFlags.IsNetwork) != 0 &&
				(state & (uint) StateFlags.IsWindow) != 0
				)
			{
			    EventHandler eh = (EventHandler) (Events[CompletedEvent]);
			    if (eh != null) {
			        EventArgs e = new EventArgs ();
			        eh (this, e);
			    }
			}  
#if debug
			Console.Error.WriteLine ("{0} completed", s.ToString ());
#endif
		}

		public void OnProgress (Int32 currentTotalProgress, Int32 maxTotalProgress)
		{
			// TODO:  Add WebBrowser.OnProgress implementation
		}

		public void OnProgressAll (string URI, Int32 currentTotalProgress, Int32 maxTotalProgress)
		{
			// TODO:  Add WebBrowser.OnProgressAll implementation
		}

		public void OnLocationChanged (string uri)
		{
			/*
			EventHandler eh = (EventHandler) (Events[NavigatedEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
			}
			*/
		}

		public void OnStatusChange (string message, Int32 status)
		{
			statusText = message;
		}

		public void OnSecurityChange (UInt32 state)
		{
			// TODO:  Add WebBrowser.OnSecurityChange implementation
		}

		public void OnVisibility (bool val)
		{
			// TODO:  Add WebBrowser.OnVisibility implementation
		}

		public bool OnClientDomKeyDown (KeyInfo keyInfo, ModifierKeys modifiers)
		{
#if debug
			Console.Error.WriteLine ("OnClientDomKeyDown");
#endif
			EventHandler eh = (EventHandler) (Events[KeyDownEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientDomKeyUp (KeyInfo keyInfo, ModifierKeys modifiers)
		{
#if debug
			Console.Error.WriteLine ("OnClientDomKeyUp");
#endif
			EventHandler eh = (EventHandler) (Events[KeyUpEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientDomKeyPress (KeyInfo keyInfo, ModifierKeys modifiers)
		{
#if debug
			Console.Error.WriteLine ("OnClientDomKeyPress");
#endif
			EventHandler eh = (EventHandler) (Events[KeyPressEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseDown (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
#if debug
			Console.Error.WriteLine ("OnClientMouseDown");
#endif
			EventHandler eh = (EventHandler) (Events[MouseDownEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseUp (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
#if debug
			Console.Error.WriteLine ("OnClientMouseUp");
#endif
			EventHandler eh = (EventHandler) (Events[MouseUpEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseClick (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
#if debug
			Console.Error.WriteLine ("OnClientMouseClick");
#endif
			EventHandler eh = (EventHandler) (Events[MouseClickEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseDoubleClick (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
#if debug
			Console.Error.WriteLine ("OnClientMouseDoubleClick");
#endif
			EventHandler eh = (EventHandler) (Events[MouseDoubleClickEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseOver (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
#if debug
			Console.Error.WriteLine ("OnClientMouseOver");
#endif
			EventHandler eh = (EventHandler) (Events[MouseEnterEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseOut (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
#if debug
			Console.Error.WriteLine ("OnClientMouseOut");
#endif
			EventHandler eh = (EventHandler) (Events[MouseLeaveEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientActivate ()
		{
			// TODO:  Add WebBrowser.OnClientActivate implementation
			return false;
		}

		public bool OnClientFocus ()
		{
			EventHandler eh = (EventHandler) (Events[FocusEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
			}
			return false;
		}

		public bool OnClientBlur ()
		{
			EventHandler eh = (EventHandler) (Events[BlurEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
			}
			return false;
		}

		public bool OnBeforeURIOpen (string URL)
		{
			// TODO:  Add WebBrowser.OnBeforeURIOpen implementation
			return false;
		}

		public bool OnCreateNewWindow ()
		{
			bool ret = false;
			CreateNewWindowEventHandler eh = (CreateNewWindowEventHandler) (Events[CreateNewWindowEvent]);
			if (eh != null) {
				CreateNewWindowEventArgs e = new CreateNewWindowEventArgs (false);
				ret = eh (this, e);
			}
			return ret;
		}

		public void OnAlert (IntPtr title, IntPtr text)
		{
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.Alert;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				eh (this, e);
			}
		}

		public bool OnAlertCheck (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState)
		{
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.AlertCheck;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnConfirm (IntPtr title, IntPtr text)
		{
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.Confirm;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnConfirmCheck (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState)
		{
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.ConfirmCheck;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				chkState = e.CheckState;
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnConfirmEx (IntPtr title, IntPtr text, DialogButtonFlags flags,
								IntPtr title0, IntPtr title1, IntPtr title2,
								IntPtr chkMsg, ref bool chkState, out Int32 retVal)
		{
			retVal = -1;

			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.ConfirmEx;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				chkState = e.CheckState;
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnPrompt (IntPtr title, IntPtr text, ref IntPtr retVal)
		{
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.Prompt;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (retVal != IntPtr.Zero)
					e.Text2 = Marshal.PtrToStringUni (retVal);
				eh (this, e);
				retVal = Marshal.StringToHGlobalUni (e.StringReturn);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnPromptUsernameAndPassword (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState, out IntPtr username, out IntPtr password)
		{
			username = IntPtr.Zero;
			password = IntPtr.Zero;
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.PromptUsernamePassword;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnPromptPassword (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState, out IntPtr password)
		{
			password = IntPtr.Zero;
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.PromptPassword;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnSelect (IntPtr title, IntPtr text, uint count, IntPtr list, out int retVal)
		{
			retVal = 0;
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.Select;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public void OnLoad ()
		{
			((DOM.Window)Window).OnLoad ();
		}

		public void OnUnload ()
		{		
			((DOM.Window)Window).OnUnload ();
		}

		public void OnGeneric (IntPtr type)
		{
			string t = Marshal.PtrToStringUni (type);
#if debug
			Console.Error.WriteLine ("Generic:{0}", t);
#endif

		}
		#endregion
	}
}
