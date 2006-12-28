//
// System.Windows.Forms.Design.ComponentEditorForm.cs
//
// Authors:
//    Dennis Hayes (dennish@raytek.com)
//    Miguel de Icaza (miguel@novell.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (c) 2006 Novell, Inc.
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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Design {

	public class ControlDesigner : ComponentDesigner
	{
		#region Public Instance Constructors

		public ControlDesigner () : base ()
		{
		}

		#endregion Public Instance Constructors

		#region Static Constructor

		static ControlDesigner ()
		{
			ControlDesigner.InvalidPoint = new Point (int.MinValue, int.MinValue);
		}

		#endregion Static Constructor

		#region Public Instance Methods

		[MonoTODO]
		public virtual bool CanBeParentedTo (IDesigner parentDesigner)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void OnSetComponentDefaults ()
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		[MonoTODO]
		protected void BaseWndProc (ref Message m)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void DefWndProc (ref Message m)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void DisplayError (Exception e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void EnableDragDrop (bool value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool GetHitTest (Point point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void HookChildControls (Control firstChild)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnContextMenu (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnCreateHandle ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDragDrop (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDragEnter (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDragLeave (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDragOver (DragEventArgs de)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnGiveFeedback (GiveFeedbackEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseDragBegin (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseDragEnd (bool cancel)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseDragMove (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseEnter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseHover ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMouseLeave ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnPaintAdornments (PaintEventArgs pe)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnSetCursor ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void UnhookChildControls (Control firstChild)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void WndProc (ref Message m)
		{
			throw new NotImplementedException ();
		}

		#endregion Protected Instance Methods

		#region Override implementation of ComponentDesigner

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		public override void Initialize (IComponent component)
		{
			if (component == null)
				throw new ArgumentNullException ("component");

			designed_control = component as Control;
			
			if (designed_control == null)
				throw new ArgumentException ("component", "Must derive from Control class");
		}

		[MonoTODO]
		public override void InitializeNonDefault ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override ICollection AssociatedComponents {
			get { throw new NotImplementedException (); }
		}

		#endregion Override implementation of ComponentDesigner

		#region Public Instance Properties

		[MonoTODO]
		public virtual AccessibleObject AccessibilityObject {
			get {
				if (accessibilityObj == null)
					accessibilityObj = new ControlDesignerAccessibleObject (this, Control);

				return accessibilityObj;
			}
		}

		[MonoTODO]
		public virtual SelectionRules SelectionRules {
			get { throw new NotImplementedException (); }
		}

		public virtual Control Control {
			get { return designed_control; }
		}

		#endregion Public Instance Properties

		#region Protected Instance Properties

		[MonoTODO]
		protected virtual bool EnableDragRect {
			get { throw new NotImplementedException (); }
		}

		#endregion Protected Instance Properties

		#region Protected Static Fields

		protected static readonly Point InvalidPoint;
		protected AccessibleObject accessibilityObj;

		#endregion Protected Static Fields

		#region Private Instance Fields

		Control designed_control;

		#endregion Private Instance Fields

		[ComVisibleAttribute(true)]
		public class ControlDesignerAccessibleObject : AccessibleObject
		{
			[MonoTODO]
			public ControlDesignerAccessibleObject (ControlDesigner designer, Control control)
			{
				throw new NotImplementedException ();
			}

			#region Override implementation of AccessibleObject

			[MonoTODO]
			public override AccessibleObject GetChild (int index)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override int GetChildCount ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject GetFocused ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject GetSelected ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override AccessibleObject HitTest (int x, int y)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override Rectangle Bounds { 
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override string DefaultAction {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override string Description {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override string Name {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override AccessibleObject Parent {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override AccessibleRole Role {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override AccessibleStates State {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]
			public override string Value {
				get { throw new NotImplementedException (); }
			}

			#endregion Override implementation of AccessibleObject
		}
	}
}
