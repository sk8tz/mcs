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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok		pbartok@novell.com
//
// Based on work by:
//	Aleksey Ryabchuk	ryabchuk@yahoo.com
//	Alexandre Pigolkine	pigolkine@gmx.de
//	Dennis Hayes		dennish@raytek.com
//	Jaak Simm		jaaksimm@firm.ee
//	John Sohn		jsohn@columbus.rr.com
//

// NOT COMPLETE 

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	public class Control : Component, ISynchronizeInvoke, IWin32Window
        {
		#region Local Variables

		// Basic
		internal Rectangle		bounds;			// bounding rectangle for control (client area + decorations)
		internal object			creator_thread;		// thread that created the control
		internal ControlNativeWindow	window;			// object for native window handle
		internal string			name;			// for object naming

		// State
		internal bool			has_focus;		// true if control has focus
		internal bool			is_visible;		// true if control is visible
		internal bool			is_entered;		// is the mouse inside the control?
		internal bool			is_enabled;		// true if control is enabled (usable/not grayed out)
		internal bool			is_selected;		// true if control is selected
		internal bool			is_accessible;		// true if the control is visible to accessibility applications
		internal bool			is_captured;		// tracks if the control has captured the mouse
		internal bool			is_recreating;		// tracks if the handle for the control is being recreated
		internal bool			causes_validation;	// tracks if validation is executed on changes
		internal int			tab_index;		// position in tab order of siblings
		internal bool			tab_stop = true;	// is the control a tab stop?
		internal bool			is_disposed;		// has the window already been disposed?
		internal Size			client_size;		// size of the client area (window excluding decorations)
		internal Rectangle		client_rect;		// rectangle with the client area (window excluding decorations)
		internal ControlStyles		control_style;		// rather win32-specific, style bits for control
		internal ImeMode		ime_mode = ImeMode.Inherit;
		internal bool			layout_pending;		// true if our parent needs to re-layout us
		internal object			control_tag;		// object that contains data about our control
		internal int			mouse_clicks;		// Counter for mouse clicks


		// Visuals
		internal Color			foreground_color;	// foreground color for control
		internal Color			background_color;	// background color for control
		internal Image			background_image;	// background image for control
		internal Font			font;			// font for control
		internal string			text;			// window/title text for control

		// Layout
		internal AnchorStyles		anchor_style;		// anchoring requirements for our control
		internal DockStyle		dock_style;		// docking requirements for our control (supercedes anchoring)
		internal SizeF			size_ratio;		// size ratio of our control to it's parent; required for anchoring
		internal Size			prev_size;		// previous size of the control; required for anchoring

		// to be categorized...
		static internal ArrayList	controls = new ArrayList();		// All of the applications controls, in a flat list
		internal ControlCollection	child_controls;		// our children
		internal Control		parent;			// our parent control
		internal AccessibleObject	accessibility_object;	// object that contains accessibility information about our control
		internal BindingContext		binding_context;	// TODO
		internal RightToLeft		right_to_left;		// drawing direction for control
		internal int			layout_suspended;
		internal bool			double_buffering;
		internal ContextMenu		context_menu;		// Context menu associated with the control

		private Graphics		dc_mem;			// Graphics context for double buffering
		private Bitmap			bmp_mem;		// Bitmap for double buffering control

		#endregion	// Local Variables

		#region Private Classes
		// This helper class allows us to dispatch messages to Control.WndProc
		internal class ControlNativeWindow : NativeWindow {
			private Control control;

			public ControlNativeWindow(Control control) : base() {
				this.control=control;
			}

			static internal Control ControlFromHandle(IntPtr hWnd) {
				ControlNativeWindow	window;

				window = (ControlNativeWindow)window_collection[hWnd];

				return window.control;
			}

			protected override void WndProc(ref Message m) {
				control.WndProc(ref m);
			}
		}
		#endregion
		
		#region Public Classes
		public class ControlAccessibleObject : AccessibleObject {			
			#region ControlAccessibleObject Local Variables
			private Control	owner;
			#endregion	// ControlAccessibleObject Local Variables

			#region ControlAccessibleObject Constructors
			public ControlAccessibleObject(Control ownerControl) {
				this.owner = ownerControl;
			}
			#endregion	// ControlAccessibleObject Constructors

			#region ControlAccessibleObject Public Instance Properties
			public override string DefaultAction {
				get {
					return base.DefaultAction;
				}
			}

			public override string Description {
				get {
					return base.Description;
				}
			}

			public IntPtr Handle {
				get {
					return owner.Handle;
				}

				set {
					// We don't want to let them set it
				}
			}

			public override string Help {
				get {
					return base.Help;
				}
			}

			public override string KeyboardShortcut {
				get {
					return base.KeyboardShortcut;
				}
			}

			public override string Name {
				get {
					return base.Name;
				}

				set {
					base.Name = value;
				}
			}

			public Control Owner {
				get {
					return owner;
				}
			}

			public override AccessibleRole Role {
				get {
					return base.Role;
				}
			}
			#endregion	// ControlAccessibleObject Public Instance Properties

			#region ControlAccessibleObject Public Instance Methods
			public override int GetHelpTopic(out string FileName) {
				return base.GetHelpTopic (out FileName);
			}

			#endregion	// ControlAccessibleObject Public Instance Methods
		}

		public class ControlCollection : IList, ICollection, ICloneable, IEnumerable {
			#region	ControlCollection Local Variables
			internal ArrayList	list;
			internal Control	owner;
			#endregion	// ControlCollection Local Variables

			#region ControlCollection Public Constructor
			public ControlCollection(Control owner) {
				this.owner=owner;
				this.list=new ArrayList();
			}
			#endregion

			#region	ControlCollection Public Instance Properties
			public int Count {
				get {
					return list.Count;
				}
			}

			public bool IsReadOnly {
				get {
					return list.IsReadOnly;
				}
			}

			public virtual Control this[int index] {
				get {
					if (index < 0 || index >= list.Count) {
						throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
					}
					return (Control)list[index];
				}
			}
			#endregion // ControlCollection Public Instance Properties
			
			#region	ControlCollection Private Instance Methods
			public virtual void Add (Control value)
			{
				
				for (int i = 0; i < list.Count; i++) {
					if (list [i] == value) {
						// Do we need to do anything here?
						return;
					}
				}
				list.Add (value);
				value.Parent = owner;
			}
			
			public virtual void AddRange (Control[] controls)
			{
				if (controls == null)
					throw new ArgumentNullException ("controls");

				owner.SuspendLayout ();

				try {
					for (int i = 0; i < controls.Length; i++) 
						Add (controls[i]);
				} finally {
					owner.ResumeLayout ();
				}
			}

			public virtual void Clear ()
			{
				owner.SuspendLayout();
				list.Clear();
				owner.ResumeLayout();
			}

			public virtual bool Contains (Control value)
			{
				return list.Contains (value);
			}

			public void CopyTo (Array array, int index)
			{
				list.CopyTo(array, index);
			}

			public override bool Equals(object other) {
				if (other is ControlCollection && (((ControlCollection)other).owner==this.owner)) {
					return(true);
				} else {
					return(false);
				}
			}

			public int GetChildIndex(Control child) {
				return GetChildIndex(child, false);
			}

			public int GetChildIndex(Control child, bool throwException) {
				int index;

				index=list.IndexOf(child);

				if (index==-1 && throwException) {
					throw new ArgumentException("Not a child control", "child");
				}
				return index;
			}

			public IEnumerator GetEnumerator() {
				return list.GetEnumerator();
			}

			public override int GetHashCode() {
				return base.GetHashCode();
			}

			public int IndexOf(Control control) {
				return list.IndexOf(control);
			}

			public virtual void Remove(Control value) {
				list.Remove(value);
			}

			public void RemoveAt(int index) {
				if (index<0 || index>=list.Count) {
					throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
				}

				list.RemoveAt(index);
			}

			public void SetChildIndex(Control child, int newIndex) {
				int	old_index;

				old_index=list.IndexOf(child);
				if (old_index==-1) {
					throw new ArgumentException("Not a child control", "child");
				}

				if (old_index==newIndex) {
					return;
				}

				RemoveAt(old_index);

				if (newIndex>list.Count) {
					list.Add(child);
				} else {
					list.Insert(newIndex, child);
				}
			}
			#endregion // ControlCollection Private Instance Methods

			#region	ControlCollection Interface Properties
			object IList.this[int index] {
				get {
					if (index<0 || index>=list.Count) {
						throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
					}
					return this[index];
				}

				set {
					if (!(value is Control)) {
						throw new ArgumentException("Object of type Control required", "value");
					}

					list[index]=(Control)value;
				}
			}

			bool IList.IsFixedSize {
				get {
					return false;
				}
			}

			bool IList.IsReadOnly {
				get {
					return list.IsReadOnly;
				}
			}

			bool ICollection.IsSynchronized {
				get {
					return list.IsSynchronized;
				}
			}

			object ICollection.SyncRoot {
				get {
					return list.SyncRoot;
				}
			}
			#endregion // ControlCollection Interface Properties

			#region	ControlCollection Interface Methods
			int IList.Add(object value) {
				if (value == null) {
					throw new ArgumentNullException("value", "Cannot add null controls");
				}

				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}

				return list.Add(value);
			}

			bool IList.Contains(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}

				return this.Contains((Control) value);
			}

			int IList.IndexOf(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control  required", "value");
				}

				return this.IndexOf((Control) value);
			}

			void IList.Insert(int index, object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}
				list.Insert(index, value);
			}

			void IList.Remove(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}
				list.Remove(value);
			}

			void ICollection.CopyTo(Array array, int index) {
				if (list.Count>0) {
					list.CopyTo(array, index);
				}
			}

			Object ICloneable.Clone() {
				ControlCollection clone = new ControlCollection(this.owner);
				clone.list=(ArrayList)list.Clone();		// FIXME: Do we need this?
				return clone;
			}
			#endregion // ControlCollection Interface Methods
		}
		#endregion	// ControlCollection Class
		
		#region Public Constructors
		public Control() {			
			creator_thread = Thread.CurrentThread;

			prev_size = Size.Empty;
			anchor_style = AnchorStyles.Top | AnchorStyles.Left;

			is_visible = true;
			is_captured = false;
			is_disposed = false;
			is_enabled = true;
			is_entered = false;
			layout_pending = false;
			causes_validation = true;
			has_focus = false;
			layout_suspended = 0;		
			double_buffering = true;
			mouse_clicks = 1;

			parent = null;
			background_image = null;
			text = string.Empty;
			name = string.Empty;			

			child_controls = CreateControlsInstance();
			client_size = new Size(DefaultSize.Width, DefaultSize.Height);
			client_rect = new Rectangle(0, 0, DefaultSize.Width, DefaultSize.Height);
			XplatUI.CalculateWindowRect(IntPtr.Zero, ref client_rect, CreateParams.Style, false, out bounds);
			if ((CreateParams.Style & (int)WindowStyles.WS_CHILD) == 0) {
				bounds.X=-1;
				bounds.Y=-1;
			}
		}

		public Control(Control parent, string text) : this() {
			Text=text;
			Parent=parent;
		}

		public Control(Control parent, string text, int left, int top, int width, int height) : this() {
			Parent=parent;
			bounds.X=left;
			bounds.Y=top;
			bounds.Width=width;
			bounds.Height=height;
			SetBoundsCore(left, top, width, height, BoundsSpecified.All);
			Text=text;
		}

		public Control(string text) : this() {
			Text=text;
		}

		public Control(string text, int left, int top, int width, int height) : this() {
			bounds.X=left;
			bounds.Y=top;
			bounds.Width=width;
			bounds.Height=height;
			SetBoundsCore(left, top, width, height, BoundsSpecified.All);
			Text=text;
		}

		protected override void Dispose(bool disposing) {
			is_disposed = true;
			if (dc_mem!=null) {
				dc_mem.Dispose();
				dc_mem=null;
			}

			if (bmp_mem!=null) {
				bmp_mem.Dispose();
				bmp_mem=null;
			}

			DestroyHandle();
			controls.Remove(this);
		}
		#endregion 	// Public Constructors

		#region Internal Properties
		#endregion	// Internal Properties

		#region Private & Internal Methods
		internal static IAsyncResult BeginInvokeInternal (Delegate method, object [] args) {
			AsyncMethodResult result = new AsyncMethodResult ();
			AsyncMethodData data = new AsyncMethodData ();

			data.Method = method;
			data.Args = args;
			data.Result = new WeakReference (result);

			XplatUI.SendAsyncMethod (data);
			return result;
		}

		internal Graphics DeviceContext {
			get { 
				if (dc_mem==null) {
					CreateBuffers(this.Width, this.Height);
				}
				return dc_mem;
			}
		}

		internal Bitmap ImageBuffer {
			get {
				if (bmp_mem==null) {
					CreateBuffers(this.Width, this.Height);
				}
				return bmp_mem;
			}
		}

		internal void CreateBuffers (int width, int height) {
			if (double_buffering == false)
				return;

			if (dc_mem != null)
				dc_mem.Dispose ();
			if (bmp_mem != null)
				bmp_mem.Dispose ();

			if (width < 1) {
				width = 1;
			}

			if (height < 1) {
				height = 1;
			}

			bmp_mem = new Bitmap (width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			dc_mem = Graphics.FromImage (bmp_mem);
		}

		internal void InvalidateBuffers ()
		{
			if (double_buffering == false)
				return;

			if (dc_mem != null)
				dc_mem.Dispose ();
			if (bmp_mem != null)
				bmp_mem.Dispose ();

			dc_mem = null;
			bmp_mem = null;
		}

		internal static void SetChildColor(Control parent) {
			Control	child;

			for (int i=0; i < parent.child_controls.Count; i++) {
				child=parent.child_controls[i];
				if (child.IsHandleCreated) {
					XplatUI.SetWindowBackground(child.window.Handle, child.BackColor);
				}
				if (child.child_controls.Count>0) {
					SetChildColor(child);
				}
			}
				
		}

		private bool Select(Control control) {
			Control	parent;

			if (control == null) {
				return false;
			}

			parent = control.parent;

			if (((control.control_style & ControlStyles.Selectable) !=0)  && (parent != null)) {
				while (parent != null) {
					if (!parent.is_visible || !parent.is_enabled) {
						return false;
					}
				}
			}

			control.is_selected = true;

			return true;
		}


		private Control FindTabStop(Control control, bool forward) {
			if (control == null) {
				return null;
			}

			return null;
		}


		internal virtual void DoDefaultAction() {
			// Only here to be overriden by our actual controls; this is needed by the accessibility class
		}

		internal static int LowOrder (int param) {
			return (param & 0xffff);
		}

		internal static int HighOrder (int param) {
			return (param >> 16);
		}
		
		internal static MouseButtons FromParamToMouseButtons (int param) {		
			MouseButtons buttons = MouseButtons.None;
					
			if ((param & (int) MsgButtons.MK_LBUTTON) != 0)
				buttons |= MouseButtons.Left;
			
			if ((param & (int) MsgButtons.MK_MBUTTON) != 0)
				buttons |= MouseButtons.Middle;
				
			if ((param & (int) MsgButtons.MK_RBUTTON) != 0)
				buttons |= MouseButtons.Right;    	
				
			return buttons;

		}
		#endregion	// Private & Internal Methods

		#region Public Static Properties
		public static Color DefaultBackColor {
			get {
				return ThemeEngine.Current.DefaultControlBackColor;
			}
		}

		public static Font DefaultFont {
			get {
				return ThemeEngine.Current.DefaultFont;
			}
		}

		public static Color DefaultForeColor {
			get {
				return ThemeEngine.Current.DefaultControlForeColor;
			}
		}

		public static Keys ModifierKeys {
			get {
				return XplatUI.State.ModifierKeys;
			}
		}

		public static MouseButtons MouseButtons {
			get {
				return XplatUI.State.MouseButtons;
			}
		}

		public static Point MousePosition {
			get {
				return Cursor.Position;
			}
		}
		#endregion	// Public Static Properties

		#region Public Instance Properties
		public AccessibleObject AccessibilityObject {
			get {
				if (accessibility_object==null) {
					accessibility_object=CreateAccessibilityInstance();
				}
				return accessibility_object;
			}
		}

		public string AccessibleDefaultActionDescription {
			get {
				return AccessibilityObject.default_action;
			}

			set {
				AccessibilityObject.default_action=value;
			}
		}

		public string AccessibleDescription {
			get {
				return AccessibilityObject.description;
			}

			set {
				AccessibilityObject.description=value;
			}
		}

		public string AccessibleName {
			get {
				return AccessibilityObject.Name;
			}

			set {
				AccessibilityObject.Name=value;
			}
		}

		public AccessibleRole AccessibleRole {
			get {
				return AccessibilityObject.role;
			}

			set {
				AccessibilityObject.role=value;
			}
		}

		public virtual bool AllowDrop {
			get {
				return XplatUI.State.DropTarget;
			}

			set {
				XplatUI.State.DropTarget=value;
			}
		}

		public virtual AnchorStyles Anchor {
			get {
				return anchor_style;
			}

			set {
				anchor_style=value;

				if (parent != null) {
					parent.PerformLayout(this, "Parent");
				}
			}
		}

		public virtual Color BackColor {
			get {
				if (background_color.IsEmpty) {
					if (parent!=null) {
						return parent.BackColor;
					}
					return DefaultBackColor;
				}
				return background_color;
			}

			set {
				background_color=value;
				if (this.IsHandleCreated) {
					XplatUI.SetWindowBackground(this.window.Handle, value);
				}
				SetChildColor(this);
				OnBackColorChanged(EventArgs.Empty);
				Refresh();
			}
		}

		public virtual Image BackgroundImage {
			get {
				return background_image;
			}

			set {
				if (background_image!=value) {
					background_image=value;
					OnBackgroundImageChanged(EventArgs.Empty);
				}
			}
		}

		public virtual BindingContext BindingContext {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public int Bottom {
			get {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.Bottom;
				}

				return bounds.Y+bounds.Height;
			}
		}

		public Rectangle Bounds {
			get {
				return this.bounds;
			}

			set {
				SetBoundsCore(value.Left, value.Top, value.Width, value.Height, BoundsSpecified.All);
			}
		}

		public bool CanFocus {
			get {
				if (is_visible && is_enabled && GetStyle(ControlStyles.Selectable)) {
					return true;
				}
				return false;
			}
		}

		public bool CanSelect {
			get {
				Control	parent;

				if (!GetStyle(ControlStyles.Selectable) || this.parent == null) {
					return false;
				}

				parent = this.parent;
				while (parent != null) {
					if (!parent.is_visible || !parent.is_enabled) {
						return false;
					}

					parent = parent.parent;
				}
				return true;
			}
		}

		public bool Capture {
			get {
				return this.is_captured;
			}

			set {
				if (this.IsHandleCreated) {
					if (value && !is_captured) {
						is_captured = true;
						XplatUI.GrabWindow(this.window.Handle, IntPtr.Zero);
					} else if (!value && is_captured) {
						XplatUI.ReleaseWindow(this.window.Handle);
						is_captured = false;
					}
				}
			}
		}

		public bool CausesValidation {
			get {
				return this.causes_validation;
			}

			set {
				if (this.causes_validation != value) {
					causes_validation = value;
					OnCausesValidationChanged(EventArgs.Empty);
				}
			}
		}

		public Rectangle ClientRectangle {
			get {
				client_rect.Width = client_size.Width;
				client_rect.Height = client_size.Height;
				return client_rect;
			}
		}

		public Size ClientSize {
			get {
#if notneeded
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.ClientSize;
				}
#endif

				return client_size;
			}

			set {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					((Form)this).form_parent_window.ClientSize = value;
					return;
				}

				this.SetClientSizeCore(value.Width, value.Height);
			}
		}

		public String CompanyName {
			get {
				return "Mono Project, Novell, Inc.";
			}
		}

		public bool ContainsFocus {
			get {
				if (this.Focused) {
					return true;
				}

				for (int i=0; i < child_controls.Count; i++) {
					if (child_controls[i].Focused) {
						return true;
					}
				}
				return false;
			}
		}
		public virtual ContextMenu ContextMenu {
			get {
				return context_menu;
			}

			set {
				if (context_menu != value) {
					context_menu = value;
					OnContextMenuChanged(EventArgs.Empty);
				}
			}
		}

		public ControlCollection Controls {
			get {
				return this.child_controls;
			}
		}

		public bool Created {
			get {
				if (!this.is_disposed && (this.window.Handle != IntPtr.Zero)) {
					return true;
				}
				return false;
			}
		}

#if notdef
		public virtual Cursor Cursor {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public ControlBidingsCollection DataBindings {
			get {
				throw new NotImplementedException();
			}
		}
#endif
		public virtual Rectangle DisplayRectangle {
			get {
				return ClientRectangle;
			}
		}

		public bool Disposing {
			get {
				return is_disposed;
			}
		}

		public virtual DockStyle Dock {
			get {
				return dock_style;
			}

			set {
				if (dock_style == value) {
					return;
				}

				dock_style = value;

				if (parent != null) {
					parent.PerformLayout(this, "Parent");
				}

				OnDockChanged(EventArgs.Empty);
			}
		}

		public bool Enabled {
			get {
				return is_enabled;
			}

			set {
				if (is_enabled == value) {
					return;
				}

				is_enabled = value;
				Refresh();
				OnEnabledChanged (EventArgs.Empty);				
			}
		}

		public virtual bool Focused {
			get {
				return this.has_focus;
			}
		}

		public virtual Font Font {
			get {
				if (font != null) {
					return font;
				}

				if (Parent != null && Parent.Font != null) {
					return Parent.Font;
				}

				return DefaultFont;
			}

			set {
				if (font == value) {
					return;
				}

				font = value;	
				Refresh();
				OnFontChanged (EventArgs.Empty);				
			}
		}

		public virtual Color ForeColor {
			get {
				if (foreground_color.IsEmpty) {
					if (parent!=null) {
						return parent.ForeColor;
					}
					return DefaultForeColor;
				}
				return foreground_color;
			}

			set {
				if (foreground_color != value) {
					foreground_color=value;
					Refresh();
					OnForeColorChanged(EventArgs.Empty);
				}
			}
		}

		public IntPtr Handle {							// IWin32Window
			get {
				if (!IsHandleCreated) {
					CreateHandle();
				}
				return window.Handle;
			}
		}

		public bool HasChildren {
			get {
				if (this.child_controls.Count>0) {
					return true;
				}
				return false;
			}
		}

		public int Height {
			get {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.Height;
				}
				return this.bounds.Height;
			}

			set {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					((Form)this).form_parent_window.Height = value;
					return;
				}

				SetBoundsCore(bounds.X, bounds.Y, bounds.Width, value, BoundsSpecified.Height);
			}
		}

		public ImeMode ImeMode {
			get {
				return ime_mode;
			}

			set {
				ime_mode = value;
			}
		}

		public bool InvokeRequired {						// ISynchronizeInvoke
			get {
				if (creator_thread!=Thread.CurrentThread) {
					return true;
				}
				return false;
			}
		}

		public bool IsAccessible {
			get {
				return is_accessible;
			}

			set {
				is_accessible = value;
			}
		}

		public bool IsDisposed {
			get {
				return this.is_disposed;
			}
		}

		public bool IsHandleCreated {
			get {
				if ((window!=null) && (window.Handle!=IntPtr.Zero)) {
					return true;
				}

				return false;
			}
		}

		public int Left {
			get {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.Left;
				}

				return this.bounds.X;
			}

			set {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					((Form)this).form_parent_window.Left = value;
					return;
				}

				SetBoundsCore(value, bounds.Y, bounds.Width, bounds.Height, BoundsSpecified.X);
			}
		}

		public Point Location {
			get {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.Location;
				}
				return new Point(bounds.X, bounds.Y);
			}

			set {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					((Form)this).form_parent_window.Location = value;
					return;
				}

				SetBoundsCore(value.X, value.Y, bounds.Width, bounds.Height, BoundsSpecified.Location);
			}
		}

		public string Name {
			get {
				return this.name;
			}

			set {
				this.name=value;
			}
		}

		public Control Parent {
			get {
				return this.parent;
			}

			set {
				if (value == this) {
					throw new ArgumentException("A circular control reference has been made. A control cannot be owned or parented to itself.");
				}

				if (parent!=value) {
					if (parent!=null) {
						parent.Controls.Remove(this);
					}

					parent=value;

					if (!parent.Controls.Contains(this)) {
						parent.Controls.Add(this);
					}

					XplatUI.SetParent(Handle, value.Handle);

					InitLayout();
				}
			}
		}

		public string ProductName {
			get {
				return "Novell Mono .NET Framework";
			}
		}

		public string ProductVersion {
			get {
				return "1.1.4322.573";
			}
		}

		public bool RecreatingHandle {
			get {
				return is_recreating;
			}
		}

		public Region Region {
			get {
				return new Region(this.bounds);
			}

			set {
				Graphics	g;
				RectangleF	r;

				g = this.CreateGraphics();
				r = value.GetBounds(g);

				SetBounds((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);

				g.Dispose();
			}
		}

		public int Right {
			get {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.Right;
				}

				return this.bounds.X+this.bounds.Width;
			}
		}

		public virtual RightToLeft RightToLeft {
			get {
				return right_to_left;
			}

			set {
				if (value != right_to_left) {
					right_to_left = value;
					OnRightToLeftChanged(EventArgs.Empty);
				}
			}
		}

		public override ISite Site {
			get {
				return base.Site;
			}

			set {
				base.Site = value;
			}
		}

		public Size Size {
			get {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.Size;
				}
				return new Size(Width, Height);
			}

			set {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					((Form)this).form_parent_window.Size = value;
					return;
				}
				SetBoundsCore(bounds.X, bounds.Y, value.Width, value.Height, BoundsSpecified.Size);
			}
		}

		public int TabIndex {
			get {
				return tab_index;
			}

			set {
				tab_index = value;
			}
		}

		public bool TabStop {
			get {
				return tab_stop;
			}

			set {
				tab_stop = value;
			}
		}

		public object Tag {
			get {
				return control_tag;
			}

			set {
				control_tag = value;
			}
		}

		public virtual string Text {
			get {
				return this.text;
			}

			set {
				if (value == null) {
					value = String.Empty;
				}

				if (text!=value) {
					text=value;
					XplatUI.Text(Handle, text);
					// FIXME: Do we need a Refresh() here?
					OnTextChanged (EventArgs.Empty);
				}
			}
		}

		public int Top {
			get {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.Top;
				}
				return this.bounds.Y;
			}

			set {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					((Form)this).form_parent_window.Top = value;
					return;
				}

				SetBoundsCore(bounds.X, value, bounds.Width, bounds.Height, BoundsSpecified.Y);
			}
		}

		public Control TopLevelControl {
			get {
				Control	p = this;

				while (p.parent != null) {
					p = p.parent;
				}

				return p;
			}
		}

		public bool Visible {
			get {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.Visible;
				}
				if (!is_visible) {
					return false;
				}

				return true;
			}

			set {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					((Form)this).form_parent_window.Visible = value;
					return;
				}

				SetVisibleCore(value);
			}
		}

		public int Width {
			get {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					return ((Form)this).form_parent_window.Width;
				}
				return this.bounds.Width;
			}

			set {
				if ((this is Form) && (((Form)this).form_parent_window != null)) {
					((Form)this).form_parent_window.Width = value;
					return;
				}

				SetBoundsCore(bounds.X, bounds.Y, value, bounds.Height, BoundsSpecified.Width);
			}
		}

		public IWindowTarget WindowTarget {
			get {
				return null;
			}

			set {
				;
			}
		}
		#endregion	// Public Instance Properties

		#region	Protected Instance Properties
		protected virtual CreateParams CreateParams {
			get {
				CreateParams create_params = new CreateParams();

				create_params.Caption = Text;
				create_params.X = Left;
				create_params.Y = Top;
				create_params.Width = Width;
				create_params.Height = Height;

				create_params.ClassName = XplatUI.DefaultClassName;
				create_params.ClassStyle = 0;
				create_params.ExStyle = 0;
				create_params.Param = 0;

				if (parent!=null) {
					create_params.Parent = parent.Handle;
				}

				create_params.Style = (int)WindowStyles.WS_CHILD | (int)WindowStyles.WS_CLIPCHILDREN | (int)WindowStyles.WS_CLIPSIBLINGS;

				if (is_visible) {
					create_params.Style |= (int)WindowStyles.WS_VISIBLE;
				}

				return create_params;
			}
		}

		protected virtual ImeMode DefaultImeMode {
			get {
				return ImeMode.Inherit;
			}
		}

		protected virtual Size DefaultSize {
			get {
				return new Size(100, 23);
			}
		}

		protected int FontHeight {
			get {
				return Font.Height;
			}

			set {
				;; // Nothing to do
			}
		}

		protected bool RenderRightToLeft {
			get {
				return (this.right_to_left == RightToLeft.Yes);
			}

			set {
				;; // Nothing to do?
			}
		}

		protected bool ResizeRedraw {
			get {
				return GetStyle(ControlStyles.ResizeRedraw);
			}

			set {
				SetStyle(ControlStyles.ResizeRedraw, value);
			}
		}

		protected virtual bool ShowFocusCues {
			get {
				return true;
			}
		}

		protected bool ShowKeyboardCues {
			get {
				return true;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Static Methods
		public static Control FromChildHandle(IntPtr handle) {
			IEnumerator control = Control.controls.GetEnumerator();

			while (control.MoveNext()) {
				if (((Control)control.Current).window.Handle == handle) {
					// Found it
					if (((Control)control.Current).Parent != null) {
						return ((Control)control.Current).Parent;
					}
				}
			}
			return null;
		}

		public static Control FromHandle(IntPtr handle) {
			IEnumerator control = Control.controls.GetEnumerator();

			while (control.MoveNext()) {
				if (((Control)control.Current).window.Handle == handle) {
					// Found it
					return ((Control)control.Current);
				}
			}
			return null;
		}

		public static bool IsMnemonic(char charCode, string text) {
			int amp;			

			amp = text.IndexOf('&');

			if (amp != -1) {
				if (amp + 1 < text.Length) {
					if (text[amp + 1] != '&') {
						if (Char.ToUpper(charCode) == Char.ToUpper(text.ToCharArray(amp + 1, 1)[0])) {
							return true;
						}	
					}
				}
			}
			return false;
		}
		#endregion

		#region Protected Static Methods
		protected static bool ReflectMessage(IntPtr hWnd, ref Message m) {
			Control	c;

			c = Control.FromHandle(hWnd);

			if (c != null) {
				c.WndProc(ref m);
				return true;
			}
			return false;
		}
		#endregion

		#region	Public Instance Methods
		public IAsyncResult BeginInvoke(Delegate method) {
			return BeginInvokeInternal(method, null);
		}

		public IAsyncResult BeginInvoke (Delegate method, object[] args) {
			return BeginInvokeInternal (method, args);
		}

		public void BringToFront() {
			if ((parent != null) && (parent.child_controls[0]!=this)) {
				if (parent.child_controls.Contains(this)) {
					parent.child_controls.SetChildIndex(this, 0);
				}
			}

			XplatUI.SetZOrder(this.window.Handle, IntPtr.Zero, true, false);

			if (parent != null) {
				parent.Refresh();
			}
		}

		public bool Contains(Control ctl) {
			Control current;

			current=ctl;
			while (current!=null) {
				if (current==ctl) {
					return true;
				}
				current=current.parent;
			}
			return false;
		}

		public void CreateControl() {

			if (!IsHandleCreated)
				CreateHandle();

			for (int i=0; i<child_controls.Count; i++) {
				child_controls[i].CreateControl();
			}
			OnCreateControl();
		}

		public Graphics CreateGraphics() {
			if (!IsHandleCreated) {
				this.CreateHandle();
			}
			return Graphics.FromHwnd(this.window.Handle);
		}

		[MonoTODO("Come up with cross platform drag-drop driver interface")]
		public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects) {
			return DragDropEffects.None;
		}

		public object EndInvoke (IAsyncResult async_result) {
			AsyncMethodResult result = (AsyncMethodResult) async_result;
			return result.EndInvoke ();
		}


		public Control GetChildAtPoint(Point pt) {
			// Microsoft's version of this function doesn't seem to work, so I can't check
			// if we only consider children or also grandchildren, etc.
			// I'm gonna say 'children only'
			for (int i=0; i<child_controls.Count; i++) {
				if (child_controls[i].Bounds.Contains(pt)) {
					return child_controls[i];
				}
			}
			return null;
		}

		public IContainerControl GetContainerControl() {
			Control	current = this;

			while (current!=null) {
				if ((current.control_style & ControlStyles.ContainerControl)!=0) {
					return (IContainerControl)current;
				}
				current = current.parent;
			}
			return null;
		}

		public void Hide() {
			this.Visible = false;
		}

		public void Invalidate() {
			Invalidate(ClientRectangle, false);
		}

		public void Invalidate(bool invalidateChildren) {
			Invalidate(ClientRectangle, invalidateChildren);
		}

		public void Invalidate(System.Drawing.Rectangle rc) {
			Invalidate(rc, false);
		}

		public void Invalidate(System.Drawing.Rectangle rc, bool invalidateChildren) {
			if (!IsHandleCreated || !Visible) {
				return;
			}

			XplatUI.Invalidate(Handle, rc, !GetStyle (ControlStyles.AllPaintingInWmPaint));

			if (invalidateChildren) {
				for (int i=0; i<child_controls.Count; i++) child_controls[i].Invalidate();
			}
		}

		public void Invalidate(System.Drawing.Region region) {
			Invalidate(region, false);
		}

		[MonoTODO("Figure out if GetRegionScans is usable")]
		public void Invalidate(System.Drawing.Region region, bool invalidateChildren) {
			throw new NotImplementedException();

			// FIXME - should use the GetRegionScans function of the region to invalidate each area
			//if (invalidateChildren) {
			//	for (int i=0; i<child_controls.Count; i++) child_controls[i].Invalidate();
			//}
		}

		public object Invoke (Delegate method) {
			return Invoke(method, null);
		}

		public object Invoke (Delegate method, object[] args) {
			IAsyncResult result = BeginInvoke (method, args);
			return EndInvoke(result);
		}

		public void PerformLayout() {
			PerformLayout(null, null);
		}

		public void PerformLayout(Control affectedControl, string affectedProperty) {
			LayoutEventArgs levent = new LayoutEventArgs(affectedControl, affectedProperty);

			if (layout_suspended>0) {
				layout_pending = true;
				return;
			}

			layout_pending = false;

			// Prevent us from getting messed up
			layout_suspended++;

			// Perform all Dock and Anchor calculations
			try {
				Control		child;
				AnchorStyles	anchor;
				Rectangle	space;
				int		diff_width;
				int		diff_height;

				space=this.DisplayRectangle;
				if (prev_size != Size.Empty) {
					diff_width = space.Width - prev_size.Width;
					diff_height = space.Height - prev_size.Height;
				} else {
					diff_width = 0;
					diff_height = 0;
				}

				// Deal with docking
				for (int i=0; i < child_controls.Count; i++) {
					child=child_controls[i];
					switch (child.Dock) {
					case DockStyle.None: {
						// Do nothing
						break;
					}

					case DockStyle.Left: {
						child.SetBounds(space.Left, space.Y, child.Width, space.Height);
						space.X+=child.Width;
						space.Width-=child.Width;
						break;
					}

					case DockStyle.Top: {
						child.SetBounds(space.Left, space.Y, space.Width, child.Height);
						space.Y+=child.Height;
						space.Height-=child.Height;
						break;
					}
				
					case DockStyle.Right: {
						child.SetBounds(space.Right-child.Width, space.Y, child.Width, space.Height);
						space.Width-=child.Width;
						break;
					}

					case DockStyle.Bottom: {
						child.SetBounds(space.Left, space.Bottom-child.Height, space.Width, child.Height);
						space.Height-=child.Height;
						break;
					}

					case DockStyle.Fill: {
						child.SetBounds(space.Left, space.Top, space.Width, space.Height);
						space.Width=0;
						space.Height=0;
						break;
					}
					}
				}

				space=this.DisplayRectangle;

				// Deal with anchoring
				for (int i=0; i < child_controls.Count; i++) {
					int left;
					int top;
					int width;
					int height;

					child=child_controls[i];
					anchor=child.Anchor;

					left=child.Left;
					top=child.Top;
					width=child.Width;
					height=child.Height;

					// If the control is docked we don't need to do anything
					if (child.Dock != DockStyle.None) {
						continue;
					}

					if ((anchor & AnchorStyles.Left) !=0 ) {
						if ((anchor & AnchorStyles.Right) != 0) {
							// Anchoring to left and right
							width=width+diff_width;
						} else {
							; // nothing to do
						}
					} else if ((anchor & AnchorStyles.Right) != 0) {
						left+=diff_width;
					} else {
						left+=diff_width/2;
					}

					if ((anchor & AnchorStyles.Top) !=0 ) {
						if ((anchor & AnchorStyles.Bottom) != 0) {
							height+=diff_height;
						} else {
							; // nothing to do
						}
					} else if ((anchor & AnchorStyles.Bottom) != 0) {
						top+=diff_height;
					} else {
						top+=diff_height/2;
					}

					// Sanity
					if (width < 0) {
						width=0;
					}

					if (height < 0) {
						height=0;
					}

					child.SetBounds(left, top, width, height);
				}

				// Let everyone know
				OnLayout(levent);
			}

				// Need to make sure we decremend layout_suspended
			finally {
				layout_suspended--;
			}
		}

		public Point PointToClient (Point p) {
			int x = p.X;
			int y = p.Y;

			XplatUI.ScreenToClient (Handle, ref x, ref y);

			return new Point (x, y);
		}

		public Point PointToScreen(Point p) {
			int x = p.X;
			int y = p.Y;

			XplatUI.ClientToScreen(Handle, ref x, ref y);

			return new Point(x, y);
		}

		public virtual bool PreProcessMessage(ref Message msg) {
			Keys key_data;

			if (msg.Msg == (int)Msg.WM_KEYDOWN) {
				key_data = (Keys)msg.WParam.ToInt32();
				if (!ProcessCmdKey(ref msg, key_data)) {
					if (IsInputKey(key_data)) {
						return false;
					}

					return ProcessDialogKey(key_data);
				}

				return true;
			} else if (msg.Msg == (int)Msg.WM_CHAR) {
				if (IsInputChar((char)msg.WParam)) {
					return false;
				}

				return ProcessDialogChar((char)msg.WParam);
			}

			return false;
		}

		public virtual void Refresh() {			
			if (IsHandleCreated == true) {
				XplatUI.RefreshWindow(window.Handle);
			}
		}

		public void ResumeLayout() {
			ResumeLayout (true);
		}

		public void ResumeLayout(bool performLayout) {
			layout_suspended--;
			
			if (layout_suspended > 0) {
				return;
			}

			if (performLayout || layout_pending) {
				PerformLayout();
			}
		}

		public void Select() {
			Select(false, false);
		}

		[MonoTODO("Finish")]
		public bool SelectNextControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap) {
			return false;
		}

		public void SendToBack() {
			if ((parent != null) && (parent.child_controls[parent.child_controls.Count-1]!=this)) {
				if (parent.child_controls.Contains(this)) {
					parent.child_controls.SetChildIndex(this, parent.child_controls.Count);
				}
			}

			XplatUI.SetZOrder(this.window.Handle, IntPtr.Zero, false, true);
			if (parent != null) {
				parent.Refresh();
			}
		}

		public void SetBounds(int x, int y, int width, int height) {
			SetBoundsCore(x, y, width, height, BoundsSpecified.All);
		}

		public void SetBounds(int x, int y, int width, int height, BoundsSpecified specified) {
			SetBoundsCore(x, y, width, height, specified);
		}

		public void Show() {
			if (!IsHandleCreated) {
				this.CreateHandle();
			}

			this.Visible=true;
		}

		public void SuspendLayout() {
			layout_suspended++;
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual AccessibleObject CreateAccessibilityInstance() {
			return new AccessibleObject(this);
		}

		protected virtual ControlCollection CreateControlsInstance() {
			return new ControlCollection(this);
		}

		protected virtual void CreateHandle() {
			if (IsDisposed) {
				throw new ObjectDisposedException(Name);
			}

			if (IsHandleCreated) {
				return;
			}

			if (window==null) {
				window = new ControlNativeWindow(this);
				window.CreateHandle(CreateParams);
			}

			if (window.Handle!=IntPtr.Zero) {
				if (!controls.Contains(window.Handle)) {
					controls.Add(this);
				}

				creator_thread = Thread.CurrentThread;

				XplatUI.SetWindowBackground(window.Handle, this.BackColor);

				OnHandleCreated(EventArgs.Empty);
			}
		}

		protected virtual void DefWndProc(ref Message m) {
			window.DefWndProc(ref m);
		}

		protected virtual void DestroyHandle() {
			if (IsHandleCreated) {
				if (Handle != IntPtr.Zero) {
					controls.Remove(Handle);
				}

				if (window != null) {
					window.DestroyHandle();
				}
			}
		}

		protected bool GetStyle(ControlStyles flag) {
			return (control_style & flag) != 0;
		}

		protected virtual void InitLayout() {
			if (parent != null) {
				parent.PerformLayout(this, "parent");
			}
		}

		protected virtual bool IsInputChar (char charCode) {
			if (parent != null) {
				return parent.IsInputChar(charCode);
			}

			return true;
		}

		protected virtual bool IsInputKey (Keys keyData) {
			// Doc says this one calls IsInputChar; not sure what to do with that
			return false;
		}

		protected virtual bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if ((context_menu != null) && context_menu.ProcessCmdKey(ref msg, keyData)) {
				return true;
			}

			if (parent != null) {
				return parent.ProcessCmdKey(ref msg, keyData);
			}

			return false;
		}

		protected virtual bool ProcessDialogChar(char charCode) {
			if (parent != null) {
				return parent.ProcessDialogChar (charCode);
			}

			return false;
		}

		protected virtual bool ProcessDialogKey (Keys keyData) {
			if (parent != null) {
				return parent.ProcessDialogKey (keyData);
			}

			return false;
		}

		protected virtual bool ProcessKeyEventArgs (ref Message msg)
		{
			KeyEventArgs		key_event;

			PreProcessMessage(ref msg);

			switch (msg.Msg) {
				case (int)Msg.WM_KEYDOWN: {
					key_event = new KeyEventArgs ((Keys)msg.WParam.ToInt32 ());
					OnKeyDown (key_event);
					return key_event.Handled;
				}
				case (int)Msg.WM_KEYUP: {
					key_event = new KeyEventArgs ((Keys)msg.WParam.ToInt32 ());
					OnKeyUp (key_event);
					return key_event.Handled;
				}

				case (int)Msg.WM_CHAR: {
					KeyPressEventArgs	key_press_event;

					key_press_event = new KeyPressEventArgs((char)msg.WParam);
					OnKeyPress(key_press_event);
					return key_press_event.Handled;
				}

				default: {
					break;
				}
			}

			return false;
		}

		protected internal virtual bool ProcessKeyMessage(ref Message msg) {
			if (parent != null) {
				if (parent.ProcessKeyPreview(ref msg)) {
					return true;
				}
			}

			return ProcessKeyEventArgs(ref msg);
		}

		protected virtual bool ProcessKeyPreview(ref Message msg) {
			if (parent != null) {
				return parent.ProcessKeyPreview(ref msg);
			}

			return false;
		}

		protected virtual bool ProcessMnemonic(char charCode) {
			// override me
			return false;
		}

		protected void RaiseDragEvent(object key, DragEventArgs e) {
			// MS Internal
		}

		protected void RaiseKeyEvent(object key, KeyEventArgs e) {
			// MS Internal
		}

		protected void RaiseMouseEvent(object key, MouseEventArgs e) {
			// MS Internal
		}

		protected void RaisePaintEvent(object key, PaintEventArgs e) {
			// MS Internal
		}

		protected void RecreateHandle() {
			IEnumerator child = child_controls.GetEnumerator();

			is_recreating=true;

			if (IsHandleCreated) {
				DestroyHandle();
				CreateHandle();

				// FIXME ZOrder?

				while (child.MoveNext()) {
					((Control)child.Current).RecreateHandle();
				}
			} else {
				CreateHandle();
			}

			is_recreating = false;
		}

		protected void ResetMouseEventArgs() {
			// MS Internal
		}

		protected ContentAlignment RtlTranslateAlignment(ContentAlignment align) {
			if (right_to_left == RightToLeft.No) {
				return align;
			}

			switch (align) {
				case ContentAlignment.TopLeft: {
					return ContentAlignment.TopRight;
				}

				case ContentAlignment.TopRight: {
					return ContentAlignment.TopLeft;
				}

				case ContentAlignment.MiddleLeft: {
					return ContentAlignment.MiddleRight;
				}

				case ContentAlignment.MiddleRight: {
					return ContentAlignment.MiddleLeft;
				}

				case ContentAlignment.BottomLeft: {
					return ContentAlignment.BottomRight;
				}

				case ContentAlignment.BottomRight: {
					return ContentAlignment.BottomLeft;
				}

				default: {
					// if it's center it doesn't change
					return align;
				}
			}
		}

		protected HorizontalAlignment RtlTranslateAlignment(HorizontalAlignment align) {
			if ((right_to_left == RightToLeft.No) || (align == HorizontalAlignment.Center)) {
				return align;
			}

			if (align == HorizontalAlignment.Left) {
				return HorizontalAlignment.Right;
			}

			// align must be HorizontalAlignment.Right
			return HorizontalAlignment.Left;
		}

		protected LeftRightAlignment RtlTranslateAlignment(LeftRightAlignment align) {
			if (right_to_left == RightToLeft.No) {
				return align;
			}

			if (align == LeftRightAlignment.Left) {
				return LeftRightAlignment.Right;
			}

			// align must be LeftRightAlignment.Right;
			return LeftRightAlignment.Left;
		}

		protected ContentAlignment RtlTranslateContent(ContentAlignment align) {
			return RtlTranslateAlignment(align);
		}

		protected HorizontalAlignment RtlTranslateHorizontal(HorizontalAlignment align) {
			return RtlTranslateAlignment(align);
		}

		protected LeftRightAlignment RtlTranslateLeftRight(LeftRightAlignment align) {
			return RtlTranslateAlignment(align);
		}

		protected virtual void ScaleCore(float dx, float dy) {
			throw new NotImplementedException();
		}

		protected virtual void Select(bool directed, bool forward) {
			int	index;
			bool	result;

			if (!directed) {
				// Select this control
				Select(this);
			}

			if (parent == null) {
				return;
			}

			index = parent.child_controls.IndexOf(this);
			result = false;

			

			do {
				if (forward) {
					if ((index+1) < parent.child_controls.Count) {
						index++;
					} else {
						index = 0;
					}
				} else {
					if (index>0) {
						index++;
					} else {
						index = parent.child_controls.Count-1;
					}
				}
				result = Select(parent.child_controls[index]);
			} while (!result && parent.child_controls[index] != this);
		}

		protected virtual void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// SetBoundsCore updates the Win32 control itself. UpdateBounds updates the controls variables and fires events, I'm guessing - pdb
			if ((specified & BoundsSpecified.X) != BoundsSpecified.X) {
				x = Left;
			}

			if ((specified & BoundsSpecified.Y) != BoundsSpecified.Y) {
				y = Top;
			}

			if ((specified & BoundsSpecified.Width)!= BoundsSpecified.Width) {
				width = Width;
			}

			if ((specified & BoundsSpecified.Height) != BoundsSpecified.Height) {
				height = Height;
			}

			if (IsHandleCreated) {
				XplatUI.SetWindowPos(Handle, x, y, width, height);
			}
			UpdateBounds(x, y, width, height);
		}

		protected virtual void SetClientSizeCore(int x, int y) {
			// Calculate the actual window size from the client size (it usually stays the same or grows)
			Rectangle	ClientRect;
			Rectangle	WindowRect;
			CreateParams	cp;

			ClientRect = new Rectangle(0, 0, x, y);
			cp = this.CreateParams;

			if (XplatUI.CalculateWindowRect(Handle, ref ClientRect, cp.Style, false, out WindowRect)==false) {
				return;
			}

			this.client_size = new Size(x, y);
			SetBoundsCore(bounds.X, bounds.Y, WindowRect.Width, WindowRect.Height, BoundsSpecified.Size);
		}

		protected void SetStyle(ControlStyles flag, bool value) {
			if (value) {
				control_style |= flag;
			} else {
				control_style &= ~flag;
			}
		}

		protected virtual void SetVisibleCore(bool value) {
			if (value!=is_visible) {
				is_visible=value;
				XplatUI.SetVisible(Handle, value);
				OnVisibleChanged(EventArgs.Empty);
				if (!is_visible) {
					if (dc_mem != null) {
						dc_mem.Dispose();
						dc_mem = null;
					}

					if (bmp_mem != null) {
						bmp_mem.Dispose();
						bmp_mem = null;
					}
				} else {
					this.CreateBuffers(bounds.Width, bounds.Height);
				}

				// FIXME - deal with focus

				if (parent != null) {
					parent.PerformLayout(this, "visible");
				} else {
					PerformLayout(this, "visible");
				}
			}
		}
	
		protected void UpdateBounds() {
			int	x;
			int	y;
			int	width;
			int	height;
			int	client_width;
			int	client_height;

			if (!IsHandleCreated) {
				CreateHandle();
			}

			XplatUI.GetWindowPos(this.Handle, out x, out y, out width, out height, out client_width, out client_height);
			UpdateBounds(x, y, width, height, client_width, client_height);
		}

		protected void UpdateBounds(int x, int y, int width, int height) {
			// UpdateBounds only seems to set our sizes and fire events but not update the GUI window to match
			bool	moved	= false;
			bool	resized	= false;

			int	client_x_diff = this.bounds.Width-this.client_size.Width;
			int	client_y_diff = this.bounds.Height-this.client_size.Height;

			// Needed to generate required notifications
			if ((this.bounds.X!=x) || (this.bounds.Y!=y)) {
				moved=true;
			}

			if ((this.Bounds.Width!=width) || (this.Bounds.Height!=height)) {
				resized=true;
			}

			bounds.X=x;
			bounds.Y=y;
			bounds.Width=width;
			bounds.Height=height;

			// Update client rectangle as well
			if (this.layout_suspended==0) {
				prev_size.Width=client_size.Width;
				prev_size.Height=client_size.Height;
			}

			client_size.Width=width-client_x_diff;
			client_size.Height=height-client_y_diff;

			if (moved) {
				OnLocationChanged(EventArgs.Empty);
			}

			if (resized) {
				OnSizeChanged(EventArgs.Empty);
			}
		}

		protected void UpdateBounds(int x, int y, int width, int height, int clientWidth, int clientHeight) {
			UpdateBounds(x, y, width, height);

			this.client_size.Width=clientWidth;
			this.client_size.Height=clientHeight;
		}

		[MonoTODO]
		protected virtual void WndProc(ref Message m) {
			EventArgs	e = new EventArgs();

#if debug
			Console.WriteLine("Received message {0}", m);
#endif

			switch((Msg)m.Msg) {
			case Msg.WM_WINDOWPOSCHANGED: {
				if (Visible) {
					UpdateBounds();
					if (GetStyle(ControlStyles.ResizeRedraw)) {
						Invalidate();
					}
				}
				break;
			}

			case Msg.WM_PAINT: {				
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart(Handle);
				OnPaint(paint_event);
				XplatUI.PaintEventEnd(Handle);
				DefWndProc(ref m);	
				break;
			}
				
			case Msg.WM_ERASEBKGND: {
				if (GetStyle (ControlStyles.UserPaint)) {
					if (!GetStyle(ControlStyles.AllPaintingInWmPaint)) {
						PaintEventArgs eraseEventArgs = new PaintEventArgs (Graphics.FromHdc (m.WParam), new Rectangle (new Point (0,0),Size));
						OnPaintBackground (eraseEventArgs);
					}
					m.Result = (IntPtr)1;
				} else {
					m.Result = IntPtr.Zero;
					DefWndProc (ref m);	
				}    					
    					
				break;
			}

			case Msg.WM_LBUTTONUP: {
				OnMouseUp (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Left, 
					mouse_clicks, 
					LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
				if (mouse_clicks > 1) {
					mouse_clicks = 1;
				}
				break;
			}
				
			case Msg.WM_LBUTTONDOWN: {					
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
					mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
					
				break;
			}

			case Msg.WM_LBUTTONDBLCLK: {
				mouse_clicks++;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
					mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
				break;
			}

			case Msg.WM_MBUTTONUP: {
				OnMouseUp (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Middle, 
					mouse_clicks, 
					LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
				if (mouse_clicks > 1) {
					mouse_clicks = 1;
				}
				break;
			}
				
			case Msg.WM_MBUTTONDOWN: {					
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
					mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
					
				break;
			}

			case Msg.WM_MBUTTONDBLCLK: {
				mouse_clicks++;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
					mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
				break;
			}

			case Msg.WM_RBUTTONUP: {
				OnMouseUp (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Right, 
					mouse_clicks, 
					LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
				if (mouse_clicks > 1) {
					mouse_clicks = 1;
				}
				break;
			}
				
			case Msg.WM_RBUTTONDOWN: {					
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
					mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
					
				break;
			}

			case Msg.WM_RBUTTONDBLCLK: {
				mouse_clicks++;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
					mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
				break;
			}

			case Msg.WM_MOUSEWHEEL: {				

				OnMouseWheel (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
					mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					HighOrder(m.WParam.ToInt32())));
				break;
			}

				
			case Msg.WM_MOUSEMOVE: {					
				OnMouseMove  (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
					mouse_clicks, 
					LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
					0));
				break;
			}

			case Msg.WM_MOUSE_ENTER: {
				if (is_entered) {
					return;
				}
				is_entered = true;
				OnMouseEnter(EventArgs.Empty);
				break;
			}

			case Msg.WM_MOUSE_LEAVE: {
				is_entered=false;
				OnMouseLeave(EventArgs.Empty);
				break;
			}

			case Msg.WM_MOUSEHOVER:	{
				OnMouseHover(EventArgs.Empty);
				break;
			}
			
			case Msg.WM_KEYDOWN: {
				if (!ProcessKeyMessage(ref m)) {
					DefWndProc (ref m);
				}

				break;					
			}

			case Msg.WM_KEYUP: {
				if (!ProcessKeyMessage(ref m)) {
					DefWndProc (ref m);
				}

				break;					
			}		

			case Msg.WM_CHAR: {
				if (!ProcessKeyMessage(ref m)) {
					DefWndProc (ref m);
				}

				break;					
			}

			case Msg.WM_HELP: {
				Point	mouse_pos;
				if (m.LParam != IntPtr.Zero) {
					HELPINFO	hi;

					hi = new HELPINFO();

					hi = (HELPINFO) Marshal.PtrToStructure (m.LParam, typeof (HELPINFO));
					mouse_pos = new Point(hi.MousePos.x, hi.MousePos.y);
				} else {
					mouse_pos = Control.MousePosition;
				}
				OnHelpRequested(new HelpEventArgs(mouse_pos));

				break;
			}
				

#if notyet				
				case Msg.WM_WINDOWPOSCHANGED:	throw new NotImplementedException();	break;
				case Msg.WM_SYSCOLORCHANGE:	throw new NotImplementedException();	break;
				
#endif

			default:
				DefWndProc(ref m);	
				break;
			}
			
			
			
		}
		#endregion	// Public Instance Methods

		#region OnXXX methods
		protected virtual void OnBackColorChanged(EventArgs e) {
			if (BackColorChanged!=null) BackColorChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackColorChanged(e);
		}

		protected virtual void OnBackgroundImageChanged(EventArgs e) {
			if (BackgroundImageChanged!=null) BackgroundImageChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackgroundImageChanged(e);
		}

		protected virtual void OnBindingContextChanged(EventArgs e) {
			if (BindingContextChanged!=null) BindingContextChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBindingContextChanged(e);
		}

		protected virtual void OnCausesValidationChanged(EventArgs e) {
			if (CausesValidationChanged!=null) CausesValidationChanged(this, e);
		}

		protected virtual void OnChangeUICues(UICuesEventArgs e) {
			if (CausesValidationChanged!=null) CausesValidationChanged(this, e);
		}

		protected virtual void OnClick(EventArgs e) {
			if (Click!=null) Click(this, e);
		}

		protected virtual void OnContextMenuChanged(EventArgs e) {
			if (ContextMenuChanged!=null) ContextMenuChanged(this, e);
		}

		protected virtual void OnControlAdded(ControlEventArgs e) {
			if (ControlAdded!=null) ControlAdded(this, e);
		}

		protected virtual void OnControlRemoved(ControlEventArgs e) {
			if (ControlRemoved!=null) ControlRemoved(this, e);
		}

		protected virtual void OnCreateControl() {
			// Override me!
		}

		protected virtual void OnCursorChanged(EventArgs e) {
			if (CursorChanged!=null) CursorChanged(this, e);
		}

		protected virtual void OnDockChanged(EventArgs e) {
			if (DockChanged!=null) DockChanged(this, e);
		}

		protected virtual void OnDoubleClick(EventArgs e) {
			if (DoubleClick!=null) DoubleClick(this, e);
		}

		protected virtual void OnDragDrop(DragEventArgs drgevent) {
			if (DragDrop!=null) DragDrop(this, drgevent);
		}

		protected virtual void OnDragEnter(DragEventArgs drgevent) {
			if (DragEnter!=null) DragEnter(this, drgevent);
		}

		protected virtual void OnDragLeave(EventArgs e) {
			if (DragLeave!=null) DragLeave(this, e);
		}

		protected virtual void OnDragOver(DragEventArgs drgevent) {
			if (DragOver!=null) DragOver(this, drgevent);
		}

		protected virtual void OnEnabledChanged(EventArgs e) {
			if (EnabledChanged!=null) EnabledChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentEnabledChanged(e);
		}

		protected virtual void OnEnter(EventArgs e) {
			if (Enter!=null) Enter(this, e);
		}

		protected virtual void OnFontChanged(EventArgs e) {
			if (FontChanged!=null) FontChanged(this, e);
		}

		protected virtual void OnForeColorChanged(EventArgs e) {
			if (ForeColorChanged!=null) ForeColorChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentForeColorChanged(e);
		}

		protected virtual void OnGiveFeedback(GiveFeedbackEventArgs gfbevent) {
			if (GiveFeedback!=null) GiveFeedback(this, gfbevent);
		}
		
		protected virtual void OnGotFocus(EventArgs e) {
			if (GotFocus!=null) GotFocus(this, e);
		}

		protected virtual void OnHandleCreated(EventArgs e) {
			if (HandleCreated!=null) HandleCreated(this, e);
		}

		protected virtual void OnHandleDestroyed(EventArgs e) {
			if (HandleDestroyed!=null) HandleDestroyed(this, e);
		}

		protected virtual void OnHelpRequested(HelpEventArgs hevent) {
			if (HelpRequested!=null) HelpRequested(this, hevent);
		}

		protected virtual void OnImeModeChanged(EventArgs e) {
			if (ImeModeChanged!=null) ImeModeChanged(this, e);
		}

		protected virtual void OnInvalidated(InvalidateEventArgs e) {
			if (Invalidated!=null) Invalidated(this, e);
		}

		protected virtual void OnKeyDown(KeyEventArgs e) {			
			if (KeyDown!=null) KeyDown(this, e);
		}

		protected virtual void OnKeyPress(KeyPressEventArgs e) {
			if (KeyPress!=null) KeyPress(this, e);
		}

		protected virtual void OnKeyUp(KeyEventArgs e) {
			if (KeyUp!=null) KeyUp(this, e);
		}

		protected virtual void OnLayout(LayoutEventArgs levent) {
			if (Layout!=null) Layout(this, levent);
		}

		protected virtual void OnLeave(EventArgs e) {
			if (Leave!=null) Leave(this, e);
		}

		protected virtual void OnLocationChanged(EventArgs e) {
			if (LocationChanged!=null) LocationChanged(this, e);
		}

		protected virtual void OnLostFocus(EventArgs e) {
			if (LostFocus!=null) LostFocus(this, e);
		}

		protected virtual void OnMouseDown(MouseEventArgs e) {
			if (MouseDown!=null) MouseDown(this, e);
		}

		protected virtual void OnMouseEnter(EventArgs e) {
			if (MouseEnter!=null) MouseEnter(this, e);
		}

		protected virtual void OnMouseHover(EventArgs e) {
			if (MouseHover!=null) MouseHover(this, e);
		}

		protected virtual void OnMouseLeave(EventArgs e) {
			if (MouseLeave!=null) MouseLeave(this, e);
		}

		protected virtual void OnMouseMove(MouseEventArgs e) {			
			if (MouseMove!=null) MouseMove(this, e);
		}

		protected virtual void OnMouseUp(MouseEventArgs e) {
			if (MouseUp!=null) MouseUp(this, e);
		}

		protected virtual void OnMouseWheel(MouseEventArgs e) {
			if (MouseWheel!=null) MouseWheel(this, e);
		}

		protected virtual void OnMove(EventArgs e) {
			if (Move!=null) Move(this, e);
		}

		protected virtual void OnNotifyMessage(Message m) {
			// Override me!
		}

		protected virtual void OnPaint(PaintEventArgs e) {
			if (Paint!=null) Paint(this, e);
		}

		protected virtual void OnPaintBackground(PaintEventArgs pevent) {
			// Override me!
		}

		protected virtual void OnParentBackColorChanged(EventArgs e) {
			if (background_color.IsEmpty && background_image==null) {
				Invalidate();
				OnBackColorChanged(e);
			}
		}

		protected virtual void OnParentBackgroundImageChanged(EventArgs e) {
			if (background_color.IsEmpty && background_image==null) {
				Invalidate();
				OnBackgroundImageChanged(e);
			}
		}

		protected virtual void OnParentBindingContextChanged(EventArgs e) {
			if (binding_context==null) {
				binding_context=Parent.binding_context;
				OnBindingContextChanged(e);
			}
		}

		protected virtual void OnParentChanged(EventArgs e) {
			if (ParentChanged!=null) ParentChanged(this, e);
		}

		protected virtual void OnParentEnabledChanged(EventArgs e) {
			if ((is_enabled && !Parent.is_enabled) || (!is_enabled && Parent.is_enabled)) {
				is_enabled=false;
				Invalidate();
				EnabledChanged(this, e);
			}
		}

		protected virtual void OnParentFontChanged(EventArgs e) {
			if (font==null) {
				Invalidate();
				OnFontChanged(e);
			}
		}

		protected virtual void OnParentForeColorChanged(EventArgs e) {
			if (foreground_color.IsEmpty) {
				Invalidate();
				OnForeColorChanged(e);
			}
		}

		protected virtual void OnParentRightToLeftChanged(EventArgs e) {
			if (right_to_left==RightToLeft.Inherit) {
				Invalidate();
				OnRightToLeftChanged(e);
			}
		}

		protected virtual void OnParentVisibleChanged(EventArgs e) {
			if (is_visible!=Parent.is_visible) {
				is_visible=false;
				Invalidate();
				OnVisibleChanged(e);
			}
		}

		protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e) {
			if (QueryContinueDrag!=null) QueryContinueDrag(this, e);
		}

		protected virtual void OnResize(EventArgs e) {
			if (Resize!=null) Resize(this, e);

			PerformLayout(this, "bounds");

			if (parent != null) {
				parent.PerformLayout();
			}
		}

		protected virtual void OnRightToLeftChanged(EventArgs e) {
			if (RightToLeftChanged!=null) RightToLeftChanged(this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentRightToLeftChanged(e);
		}

		protected virtual void OnSizeChanged(EventArgs e) {
			InvalidateBuffers ();
			OnResize(e);
			if (SizeChanged!=null) SizeChanged(this, e);
		}

		protected virtual void OnStyleChanged(EventArgs e) {
			if (StyleChanged!=null) StyleChanged(this, e);
		}

		protected virtual void OnSystemColorsChanged(EventArgs e) {
			if (SystemColorsChanged!=null) SystemColorsChanged(this, e);
		}

		protected virtual void OnTabIndexChanged(EventArgs e) {
			if (TabIndexChanged!=null) TabIndexChanged(this, e);
		}

		protected virtual void OnTabStopChanged(EventArgs e) {
			if (TabStopChanged!=null) TabStopChanged(this, e);
		}

		protected virtual void OnTextChanged(EventArgs e) {
			if (TextChanged!=null) TextChanged(this, e);
		}

		protected virtual void OnValidated(EventArgs e) {
			if (Validated!=null) Validated(this, e);
		}

		protected virtual void OnValidating(System.ComponentModel.CancelEventArgs e) {
			if (Validating!=null) Validating(this, e);
		}

		protected virtual void OnVisibleChanged(EventArgs e) {
			if (!is_visible) {
				if (dc_mem!=null) {
					dc_mem.Dispose ();
					bmp_mem=null;
				}

				if (bmp_mem!=null) {
					bmp_mem.Dispose();
					bmp_mem=null;
				}
			} else {
				if (!is_disposed) {
					if (!this.IsHandleCreated) {
						this.CreateHandle();
					}
					PerformLayout();
				}
			}
			
			if (VisibleChanged!=null) VisibleChanged(this, e);

			// We need to tell our kids
			for (int i=0; i<child_controls.Count; i++) {
				child_controls[i].OnParentVisibleChanged(e);
			}
		}
		#endregion	// OnXXX methods

		#region Events
		public event EventHandler		BackColorChanged;
		public event EventHandler		BackgroundImageChanged;
		public event EventHandler		BindingContextChanged;
		public event EventHandler		CausesValidationChanged;
		public event UICuesEventHandler		ChangeUICues;
		public event EventHandler		Click;
		public event EventHandler		ContextMenuChanged;
		public event ControlEventHandler	ControlAdded;
		public event ControlEventHandler	ControlRemoved;
		public event EventHandler		CursorChanged;
		public event EventHandler		DockChanged;
		public event EventHandler		DoubleClick;
		public event DragEventHandler		DragDrop;
		public event DragEventHandler		DragEnter;
		public event EventHandler		DragLeave;
		public event DragEventHandler		DragOver;
		public event EventHandler		EnabledChanged;
		public event EventHandler		Enter;
		public event EventHandler		FontChanged;
		public event EventHandler		ForeColorChanged;
		public event GiveFeedbackEventHandler	GiveFeedback;
		public event EventHandler		GotFocus;
		public event EventHandler		HandleCreated;
		public event EventHandler		HandleDestroyed;
		public event HelpEventHandler		HelpRequested;
		public event EventHandler		ImeModeChanged;
		public event InvalidateEventHandler	Invalidated;
		public event KeyEventHandler		KeyDown;
		public event KeyPressEventHandler	KeyPress;
		public event KeyEventHandler		KeyUp;
		public event LayoutEventHandler		Layout;
		public event EventHandler		Leave;
		public event EventHandler		LocationChanged;
		public event EventHandler		LostFocus;
		public event MouseEventHandler		MouseDown;
		public event EventHandler		MouseEnter;
		public event EventHandler		MouseHover;
		public event EventHandler		MouseLeave;
		public event MouseEventHandler		MouseMove;
		public event MouseEventHandler		MouseUp;
		public event MouseEventHandler		MouseWheel;
		public event EventHandler		Move;
		public event PaintEventHandler		Paint;
		public event EventHandler		ParentChanged;
		public event QueryAccessibilityHelpEventHandler	QueryAccessibilityHelp;
		public event QueryContinueDragEventHandler	QueryContinueDrag;
		public event EventHandler		Resize;
		public event EventHandler		RightToLeftChanged;
		public event EventHandler		SizeChanged;
		public event EventHandler		StyleChanged;
		public event EventHandler		SystemColorsChanged;
		public event EventHandler		TabIndexChanged;
		public event EventHandler		TabStopChanged;
		public event EventHandler		TextChanged;
		public event EventHandler		Validated;
		public event CancelEventHandler		Validating;
		public event EventHandler		VisibleChanged;
		#endregion	// Events
	}
}
