//
// System.Web.UI.Page.jsf.cs
//
// Authors:
//   Igor Zelmanovich (igorz@mainsoft.com)
//   Konstantin Triger (kostat@mainsoft.com)
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
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
using System.Collections.Generic;
using System.Text;
using javax.faces.el;
using java.lang;
using javax.faces.context;
using javax.faces.component;
using javax.faces.@event;
using javax.servlet;
using System.Collections;
using javax.faces.application;
using javax.faces.render;
using javax.faces;
using System.IO;
using System.Threading;
using System.Collections.Specialized;

namespace System.Web.UI
{
	public partial class Page
	{
		private sealed class AspNetResponseWriter : java.io.Writer
		{
			readonly TextWriter _writer;
			public AspNetResponseWriter (TextWriter writer) {
				_writer = writer;
			}
			public override void close () {
				_writer.Close ();
			}

			public override void flush () {
				_writer.Flush ();
			}

			public override void write (char [] __p1, int __p2, int __p3) {
				_writer.Write (__p1, __p2, __p3);
			}

			public override void write (int __p1) {
				_writer.Write ((char) __p1);
			}

			public override void write (char [] __p1) {
				_writer.Write (__p1);
			}

			public override void write (string __p1) {
				_writer.Write (__p1);
			}

			public override void write (string __p1, int __p2, int __p3) {
				_writer.Write (__p1, __p2, __p3);
			}
		}

		MethodBinding _action;
		MethodBinding _actionListener;
		bool _immediate;
		Pair _state;
		bool [] _validatorsState;

		void EnterThread (FacesContext context) {
			if (_lifeCycle == PageLifeCycle.Unknown)
				ProcessRequest (HttpContext.Current);
			else
				SetContext (HttpContext.Current);
		}

		void ExitThread (FacesContext context) {
			// TODO
			//if (context.getResponseComplete ())
			//    Response.End ();
		}

		public override void encodeBegin (FacesContext context) {
			// do nothing
		}

		public override void encodeChildren (FacesContext context) {
#if DEBUG
			Console.WriteLine ("encodeChildren");
#endif
			EnterThread (context);
			try {
				if (!context.getResponseComplete ()) {
					// ensure lifecycle complete.
					if (!IsLoaded) {
						ProcessLoad ();
						RestoreValidatorsState (_validatorsState);
					}
					if (!IsPrerendered)
						ProcessLoadComplete ();

					RenderPage ();
				}
			}
			catch (Exception ex) {
				if (!(ex is ThreadAbortException))
					ProcessException (ex);
				throw;
			}
			finally {
				ProcessUnload ();
				ExitThread (context);
			}
		}

		public override void encodeEnd (FacesContext context) {
			// do nothing
		}

		public override UIComponent getParent () {
			return null;
		}

		public override void setParent (UIComponent parent) {
			//ignore: parent is root
		}

		// TODO: consider validators state
		public override object processSaveState (FacesContext context) {
#if DEBUG
			Console.WriteLine ("processSaveState");
#endif
			return new object [] { _state, GetValidatorsState () };
		}

		public override void processRestoreState (FacesContext context, object state) {
#if DEBUG
			Console.WriteLine ("processRestoreState");
#endif
			EnterThread (context);
			try {
				_state = (Pair) ((object []) state) [0];
				_validatorsState = (bool []) ((object []) state) [1];
				RestorePageState ();
			}
			catch (Exception ex) {
				HandleException (ex);
				throw;
			}
			finally {
				ExitThread (context);
			}
		}

		public override void processDecodes (FacesContext context) {
#if DEBUG
			Console.WriteLine ("processDecodes");
#endif
			EnterThread (context);
			try {
				ProcessPostData ();

				EventRaiserFacesEvent facesEvent = new EventRaiserFacesEvent (this);
				facesEvent.setPhaseId (PhaseId.INVOKE_APPLICATION);
				context.getViewRoot ().queueEvent (facesEvent);

				base.processDecodes (context);
			}
			catch (Exception ex) {
				HandleException (ex);
				throw;
			}
			finally {
				ExitThread (context);
			}
		}

		public override void processValidators (FacesContext context) {
#if DEBUG
			Console.WriteLine ("processValidators");
#endif
			EnterThread (context);
			try {
				base.processValidators (context);
			}
			catch (Exception ex) {
				HandleException (ex);
				throw;
			}
			finally {
				ExitThread (context);
			}
		}

		public override void processUpdates (FacesContext context) {
#if DEBUG
			Console.WriteLine ("processUpdates");
#endif
			EnterThread (context);
			try {
				base.processUpdates (context);
			}
			catch (Exception ex) {
				HandleException (ex);
				throw;
			}
			finally {
				ExitThread (context);
			}
		}

		public override void broadcast (FacesEvent e) {
#if DEBUG
			Console.WriteLine ("broadcast");
#endif
			if (!(e is EventRaiserFacesEvent))
				throw new NotSupportedException ("FacesEvent of class " + e.GetType ().Name + " not supported by Page");

			FacesContext context = getFacesContext ();
			try {
				ProcessRaiseChangedEvents ();
				ProcessRaisePostBackEvents ();
				ProcessLoadComplete ();
			}
			catch (Exception ex) {
				HandleException (ex);
				throw;
			}
			finally {
				ExitThread (context);
			}
		}

		void HandleException (Exception ex) {
			Console.WriteLine (ex.ToString ());
			try {
				if (!(ex is ThreadAbortException))
					ProcessException (ex);
			}
			finally {
				ProcessUnload ();
			}
		}

		bool [] GetValidatorsState () {
			if (is_validated && Validators.Count > 0) {
				bool [] validatorsState = new bool [Validators.Count];
				bool isValid = true;
				for (int i = 0; i < Validators.Count; i++) {
					IValidator val = Validators [i];
					if (!val.IsValid)
						isValid = false;
					else
						validatorsState [i] = true;
				}
				return validatorsState;
			}
			return null;
		}

		void RestoreValidatorsState (bool [] validatorsState) {
			if (validatorsState == null)
				return;

			is_validated = true;
			for (int i = 0; i < Math.Min (validatorsState.Length, Validators.Count); i++) {
				IValidator val = Validators [i];
				val.IsValid = validatorsState [i];
			}
		}

		ResponseWriter SetupResponseWriter (TextWriter httpWriter) { //TODO
			FacesContext facesContext = getFacesContext ();

			ResponseWriter oldWriter = facesContext.getResponseWriter ();
			RenderKitFactory renderFactory = (RenderKitFactory) FactoryFinder.getFactory (FactoryFinder.RENDER_KIT_FACTORY);
			RenderKit renderKit = renderFactory.getRenderKit (facesContext,
															 facesContext.getViewRoot ().getRenderKitId ());

			ServletResponse response = (ServletResponse) facesContext.getExternalContext ().getResponse ();

			ResponseWriter writer = renderKit.createResponseWriter (new AspNetResponseWriter (httpWriter),
													 response.getContentType (), //TODO: is this the correct content type?
													 response.getCharacterEncoding ());
			facesContext.setResponseWriter (writer);

			return oldWriter;
		}

		sealed class FacesPageStatePersister : PageStatePersister
		{
			public FacesPageStatePersister (Page page)
				: base (page) {
			}

			public override void Load () {
				if (Page._state != null) {
					ViewState = Page._state.First;
					ControlState = Page._state.Second;
				}
			}

			public override void Save () {
				Page._state = new Pair (ViewState, ControlState);
			}
		}

		sealed class EventRaiserFacesEvent : FacesEvent
		{
			public EventRaiserFacesEvent (Page page)
				: base (page) {
			}

			public override bool isAppropriateListener (FacesListener __p1) {
				throw new NotSupportedException ();
			}

			public override void processListener (FacesListener __p1) {
				throw new NotSupportedException ();
			}
		}
	}
}
