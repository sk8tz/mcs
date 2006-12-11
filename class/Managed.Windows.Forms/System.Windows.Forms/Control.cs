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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok		pbartok@novell.com
//
// Partially based on work by:
//	Aleksey Ryabchuk	ryabchuk@yahoo.com
//	Alexandre Pigolkine	pigolkine@gmx.de
//	Dennis Hayes		dennish@raytek.com
//	Jaak Simm		jaaksimm@firm.ee
//	John Sohn		jsohn@columbus.rr.com
//

// COMPLETE 

#undef DebugRecreate
#undef DebugFocus

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Windows.Forms
{
	[Designer("System.Windows.Forms.Design.ControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultProperty("Text")]
	[DefaultEvent("Click")]
	[DesignerSerializer("System.Windows.Forms.Design.ControlCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
	[ToolboxItemFilter("System.Windows.Forms")]
	public class Control : Component, ISynchronizeInvoke, IWin32Window
	{
		#region Local Variables

		// Basic
		internal Rectangle		bounds;			// bounding rectangle for control (client area + decorations)
		Rectangle explicit_bounds; // explicitly set bounds
		internal object			creator_thread;		// thread that created the control
		internal ControlNativeWindow	window;			// object for native window handle
		string name; // for object naming

		// State
		bool is_created; // true if OnCreateControl has been sent
		internal bool			has_focus;		// true if control has focus
		internal bool			is_visible;		// true if control is visible
		internal bool			is_entered;		// is the mouse inside the control?
		internal bool			is_enabled;		// true if control is enabled (usable/not grayed out)
		bool is_accessible; // true if the control is visible to accessibility applications
		bool is_captured; // tracks if the control has captured the mouse
		internal bool			is_toplevel;		// tracks if the control is a toplevel window
		bool is_recreating; // tracks if the handle for the control is being recreated
		bool causes_validation; // tracks if validation is executed on changes
		bool is_focusing; // tracks if Focus has been called on the control and has not yet finished
		int tab_index; // position in tab order of siblings
		bool tab_stop; // is the control a tab stop?
		bool is_disposed; // has the window already been disposed?
		Size client_size; // size of the client area (window excluding decorations)
		Rectangle client_rect; // rectangle with the client area (window excluding decorations)
		ControlStyles control_style; // rather win32-specific, style bits for control
		ImeMode ime_mode;
		bool layout_pending; // true if our parent needs to re-layout us
		object control_tag; // object that contains data about our control
		internal int			mouse_clicks;		// Counter for mouse clicks
		Cursor cursor; // Cursor for the window
		internal bool			allow_drop;		// true if the control accepts droping objects on it   
		Region clip_region; // User-specified clip region for the window

		// Visuals
		internal Color			foreground_color;	// foreground color for control
		internal Color			background_color;	// background color for control
		Image background_image; // background image for control
		internal Font			font;			// font for control
		string text; // window/title text for control
		internal BorderStyle		border_style;		// Border style of control

		// Layout
		int layout_suspended;
		internal AnchorStyles		anchor_style;		// anchoring requirements for our control
		internal DockStyle		dock_style;		// docking requirements for our control (supercedes anchoring)
		// Please leave the next 4 as internal until DefaultLayout (2.0) is rewritten
		internal int			dist_left;		// distance to the left border of the parent
		internal int			dist_top; // distance to the top border of the parent
		internal int			dist_right; // distance to the right border of the parent
		internal int			dist_bottom; // distance to the bottom border of the parent

		// to be categorized...
		ControlCollection child_controls; // our children
		Control parent; // our parent control
		AccessibleObject accessibility_object; // object that contains accessibility information about our control
		BindingContext binding_context;
		RightToLeft right_to_left; // drawing direction for control
		ContextMenu context_menu; // Context menu associated with the control

		// double buffering
		Graphics backbuffer_dc;
		object backbuffer;
		Region invalid_region;

		ControlBindingsCollection data_bindings;

#if NET_2_0
		internal bool use_compatible_text_rendering;
		static bool verify_thread_handle;
		Padding padding;
		Size maximum_size;
		Size minimum_size;
		Size preferred_size;
		Padding margin;
		Layout.LayoutEngine layout_engine;
#endif

		#endregion	// Local Variables

		#region Private Classes
		// This helper class allows us to dispatch messages to Control.WndProc
		internal class ControlNativeWindow : NativeWindow {
			private Control owner;

			public ControlNativeWindow(Control control) : base() {
				this.owner=control;
			}


			public Control Owner {
				get {
					return owner;
				}
			}

			static internal Control ControlFromHandle(IntPtr hWnd) {
				ControlNativeWindow	window;

				window = (ControlNativeWindow)window_collection[hWnd];
				if (window != null) {
					return window.owner;
				}

				return null;
			}

			static internal Control ControlFromChildHandle (IntPtr handle) {
				ControlNativeWindow	window;

				Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
				while (hwnd != null) {
					window = (ControlNativeWindow)window_collection[hwnd.Handle];
					if (window != null) {
						return window.owner;
					}
					hwnd = hwnd.Parent;
				}

				return null;
			}

			protected override void WndProc(ref Message m) {
				owner.WndProc(ref m);
			}
		}
		#endregion
		
		#region Public Classes
		[ComVisible(true)]
		public class ControlAccessibleObject : AccessibleObject {
			Control owner;

			#region ControlAccessibleObject Constructors
			public ControlAccessibleObject(Control ownerControl)
				: base (ownerControl)
			{
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

			public override AccessibleObject Parent {
				get {
					return base.Parent;
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

			[MonoTODO("Implement this and tie it into Control.AccessibilityNotifyClients")]
			public void NotifyClients(AccessibleEvents accEvent) {
				throw new NotImplementedException();
			}

			[MonoTODO("Implement this and tie it into Control.AccessibilityNotifyClients")]
			public void NotifyClients(AccessibleEvents accEvent, int childID) {
				throw new NotImplementedException();
			}

			public override string ToString() {
				return "ControlAccessibleObject: Owner = " + owner.ToString() + ", Text: " + owner.text;
			}

			#endregion	// ControlAccessibleObject Public Instance Methods
		}

		[DesignerSerializer("System.Windows.Forms.Design.ControlCollectionCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
		[ListBindable(false)]
		public class ControlCollection : IList, ICollection, ICloneable, IEnumerable {
			#region	ControlCollection Local Variables
			ArrayList list;
			ArrayList impl_list;
			Control[] all_controls;
			Control owner;
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
				if (value == null)
					return;
				
				if (Contains (value)) {
					owner.PerformLayout();
					return;
				}

				if (value.tab_index == -1) {
					int	end;
					int	index;
					int	use;

					use = 0;
					end = owner.child_controls.Count;
					for (int i = 0; i < end; i++) {
						index = owner.child_controls[i].tab_index;
						if (index >= use) {
							use = index + 1;
						}
					}
					value.tab_index = use;
				}

				if (value.parent != null) {
					value.parent.Controls.Remove(value);
				}

				all_controls = null;
				list.Add (value);

				value.ChangeParent(owner);

				value.InitLayout();

				owner.UpdateChildrenZOrder();
				owner.PerformLayout(value, "Parent");
				owner.OnControlAdded(new ControlEventArgs(value));
			}
			
			internal void AddToList (Control c)
			{
				all_controls = null;
				list.Add (c);
			}

			internal virtual void AddImplicit (Control control)
			{
				if (impl_list == null)
					impl_list = new ArrayList ();

				if (AllContains (control))
					return;

				all_controls = null;
				impl_list.Add (control);

				control.ChangeParent (owner);
				control.InitLayout ();
				owner.UpdateChildrenZOrder ();
				owner.PerformLayout (control, "Parent");
				owner.OnControlAdded (new ControlEventArgs (control));
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

			internal virtual void AddRangeImplicit (Control [] controls)
			{
				if (controls == null)
					throw new ArgumentNullException ("controls");

				owner.SuspendLayout ();

				try {
					for (int i = 0; i < controls.Length; i++)
						AddImplicit (controls [i]);
				} finally {
					owner.ResumeLayout ();
				}
			}

			public virtual void Clear ()
			{
				all_controls = null;

				// MS sends remove events in reverse order
				while (list.Count > 0) {
					Remove((Control)list[list.Count - 1]);
				}
			}

			internal virtual void ClearImplicit ()
			{
				if (impl_list == null)
					return;
				all_controls = null;
				impl_list.Clear ();
			}

			public bool Contains (Control value)
			{
				for (int i = list.Count; i > 0; ) {
					i--;
					
					if (list [i] == value) {
						// Do we need to do anything here?
						return true;
					}
				}
				return false;
			}

			internal bool ImplicitContains (Control value)
			{
				if (impl_list == null)
					return false;

				for (int i = impl_list.Count; i > 0; ) {
					i--;
					
					if (impl_list [i] == value) {
						// Do we need to do anything here?
						return true;
					}
				}
				return false;
			}

			internal bool AllContains (Control value)
			{
				return Contains (value) || ImplicitContains (value);
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

			internal IEnumerator GetAllEnumerator ()
			{
				Control [] res = GetAllControls ();
				return res.GetEnumerator ();
			}

			internal Control [] GetAllControls ()
			{
				if (all_controls != null)
					return all_controls;

				if (impl_list == null) {
					all_controls = (Control []) list.ToArray (typeof (Control));
					return all_controls;
				}
				
				all_controls = new Control [list.Count + impl_list.Count];
				impl_list.CopyTo (all_controls);
				list.CopyTo (all_controls, impl_list.Count);

				return all_controls;
			}

			public override int GetHashCode() {
				return base.GetHashCode();
			}

			public int IndexOf(Control control) {
				return list.IndexOf(control);
			}

			public virtual void Remove(Control value) {
				if (value == null)
					return;

				owner.PerformLayout(value, "Parent");
				owner.OnControlRemoved(new ControlEventArgs(value));

				all_controls = null;
				list.Remove(value);

				value.ChangeParent(null);

				owner.UpdateChildrenZOrder();
			}

			internal virtual void RemoveImplicit (Control control)
			{
				if (impl_list != null) {
					all_controls = null;
					owner.PerformLayout (control, "Parent");
					owner.OnControlRemoved (new ControlEventArgs (control));
					impl_list.Remove (control);
				}
				control.ChangeParent (null);
				owner.UpdateChildrenZOrder ();
			}

			public void RemoveAt(int index) {
				if (index < 0 || index >= list.Count) {
					throw new ArgumentOutOfRangeException("index", index, "ControlCollection does not have that many controls");
				}
				Remove ((Control)list[index]);
			}

			public void SetChildIndex(Control child, int newIndex) {
				if (child == null)
					throw new ArgumentNullException ("child");

				int	old_index;

				old_index=list.IndexOf(child);
				if (old_index==-1) {
					throw new ArgumentException("Not a child control", "child");
				}

				if (old_index==newIndex) {
					return;
				}

				all_controls = null;
				list.RemoveAt(old_index);

				if (newIndex>list.Count) {
					list.Add(child);
				} else {
					list.Insert(newIndex, child);
				}
				child.UpdateZOrder();
				owner.PerformLayout();
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

					all_controls = null;
					Control ctrl = (Control) value;
					list[index]= ctrl;

					ctrl.ChangeParent(owner);

					ctrl.InitLayout();

					owner.UpdateChildrenZOrder();
					owner.PerformLayout(ctrl, "Parent");
				}
			}

			bool IList.IsFixedSize {
				get {
					return false;
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
				all_controls = null;
				list.Insert(index, value);
			}

			void IList.Remove(object value) {
				if (!(value is Control)) {
					throw new ArgumentException("Object of type Control required", "value");
				}
				all_controls = null;
				list.Remove(value);
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
		public Control()
		{
			anchor_style = AnchorStyles.Top | AnchorStyles.Left;

			is_created = false;
			is_visible = true;
			is_captured = false;
			is_disposed = false;
			is_enabled = true;
			is_entered = false;
			layout_pending = false;
			is_toplevel = false;
			causes_validation = true;
			has_focus = false;
			layout_suspended = 0;
			mouse_clicks = 1;
			tab_index = -1;
			cursor = null;
			right_to_left = RightToLeft.Inherit;
			border_style = BorderStyle.None;
			background_color = Color.Empty;
			dist_left = 0;
			dist_right = 0;
			dist_top = 0;
			dist_bottom = 0;
			tab_stop = true;
			ime_mode = ImeMode.Inherit;

#if NET_2_0
			use_compatible_text_rendering = Application.use_compatible_text_rendering;
			padding = new Padding(0);
			maximum_size = new Size();
			minimum_size = new Size();
			preferred_size = new Size();
			margin = this.DefaultMargin;
			layout_engine = this.LayoutEngine;
#endif

			control_style = ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | 
					ControlStyles.Selectable | ControlStyles.StandardClick | 
					ControlStyles.StandardDoubleClick;
#if NET_2_0
			control_style |= ControlStyles.UseTextForAccessibility;
#endif

			parent = null;
			background_image = null;
			text = string.Empty;
			name = string.Empty;

			window = new ControlNativeWindow(this);
			child_controls = CreateControlsInstance();
			client_size = new Size(DefaultSize.Width, DefaultSize.Height);
			client_rect = new Rectangle(0, 0, DefaultSize.Width, DefaultSize.Height);
			XplatUI.CalculateWindowRect(ref client_rect, CreateParams.Style, CreateParams.ExStyle, null, out bounds);
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
			SetBounds(left, top, width, height, BoundsSpecified.All);
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
			SetBounds(left, top, width, height, BoundsSpecified.All);
			Text=text;
		}

		private delegate void RemoveDelegate(object c);

		protected override void Dispose(bool disposing) {
			if (!is_disposed && disposing) {
				Capture = false;

				DisposeBackBuffer ();

				if (invalid_region!=null) {
					invalid_region.Dispose();
					invalid_region=null;
				}
				if (this.InvokeRequired) {
					if (Application.MessageLoop) {
						this.BeginInvokeInternal(new MethodInvoker(DestroyHandle), null, true);
					}
				} else {
					DestroyHandle();
				}

				if (parent != null) {
					parent.Controls.Remove(this);
				}

				Control [] children = child_controls.GetAllControls ();
				for (int i=0; i<children.Length; i++) {
					children[i].parent = null;	// Need to set to null or our child will try and remove from ourselves and crash
					children[i].Dispose();
				}
			}
			is_disposed = true;
			is_visible = false;
			base.Dispose(disposing);
		}
		#endregion 	// Public Constructors

		#region Internal Properties
		internal BorderStyle InternalBorderStyle {
			get {
				return border_style;
			}

			set {
				if (!Enum.IsDefined (typeof (BorderStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for BorderStyle", value));

				if (border_style != value) {
					border_style = value;

					if (IsHandleCreated) {
						XplatUI.SetBorderStyle(window.Handle, (FormBorderStyle)border_style);
						Refresh();
					}
				}
			}
		}
		#endregion	// Internal Properties

		#region Private & Internal Methods
		internal IAsyncResult BeginInvokeInternal (Delegate method, object [] args, bool disposing) {
			AsyncMethodResult	result;
			AsyncMethodData		data;

			if (!disposing) {
				Control p = this;
				do {
					if (!p.IsHandleCreated) {
						throw new InvalidOperationException("Cannot call Invoke or InvokeAsync on a control until the window handle is created");
					}
					p = p.parent;
				} while (p != null);
			}

			result = new AsyncMethodResult ();
			data = new AsyncMethodData ();

			data.Handle = window.Handle;
			data.Method = method;
			data.Args = args;
			data.Result = result;

#if NET_2_0
			if (!ExecutionContext.IsFlowSuppressed ()) {
				data.Context = ExecutionContext.Capture ();
			}
#else
#if !MWF_ON_MSRUNTIME
			if (SecurityManager.SecurityEnabled) {
				data.Stack = CompressedStack.GetCompressedStack ();
			}
#endif
#endif

			XplatUI.SendAsyncMethod (data);
			return result;
		}

		
		internal void PointToClient (ref int x, ref int y)
		{
			XplatUI.ScreenToClient (Handle, ref x, ref y);
		}

		internal void PointToScreen (ref int x, ref int y)
		{
			XplatUI.ClientToScreen (Handle, ref x, ref y);
		}

		internal bool IsRecreating {
			get {
				return is_recreating;
			}
		}

		internal Graphics DeviceContext {
			get { return Hwnd.bmp_g; }
		}

		private void InvalidateBackBuffer ()
		{
			if (invalid_region != null)
				invalid_region.Dispose();
			invalid_region = new Region (ClientRectangle);
		}

		private void CreateBackBuffer ()
		{
			if (backbuffer != null)
				return;

			int width = Width;
			int height = Height;

			if (width < 1) width = 1;
			if (height < 1) height = 1;

			XplatUI.CreateOffscreenDrawable (Handle, width, height, out backbuffer, out backbuffer_dc);
			InvalidateBackBuffer ();
		}

		private void DisposeBackBuffer ()
		{
			if (backbuffer == null)
				return;

			XplatUI.DestroyOffscreenDrawable (backbuffer, backbuffer_dc);
			backbuffer = null;
			backbuffer_dc = null;


			if (invalid_region != null)
				invalid_region.Dispose ();
			invalid_region = null;
		}

		internal static void SetChildColor(Control parent) {
			Control	child;

			for (int i=0; i < parent.child_controls.Count; i++) {
				child=parent.child_controls[i];
				if (child.child_controls.Count>0) {
					SetChildColor(child);
				}
			}
		}

		internal bool Select(Control control) {
			IContainerControl	container;

			if (control == null) {
				return false;
			}

			container = GetContainerControl();
			if (container != null) {
				container.ActiveControl = control;
			}
			if (control.IsHandleCreated) {
				XplatUI.SetFocus(control.window.Handle);
			}
			return true;
		}

		internal void SelectChild (Control control)
		{
			if (control.IsHandleCreated)
				XplatUI.SetFocus (control.window.Handle);
		}

		internal virtual void DoDefaultAction() {
			// Only here to be overriden by our actual controls; this is needed by the accessibility class
		}

		internal static int LowOrder (int param) {
			return ((int)(short)(param & 0xffff));
		}

		internal static int HighOrder (int param) {
			return ((int)(short)(param >> 16));
		}

		// This method exists so controls overriding OnPaintBackground can have default background painting done
		internal virtual void PaintControlBackground (PaintEventArgs pevent)
		{
			if (GetStyle(ControlStyles.SupportsTransparentBackColor) && (BackColor.A != 0xff)) {
				if (parent != null) {
					PaintEventArgs	parent_pe;
					GraphicsState	state;

					parent_pe = new PaintEventArgs(pevent.Graphics, new Rectangle(pevent.ClipRectangle.X + Left, pevent.ClipRectangle.Y + Top, pevent.ClipRectangle.Width, pevent.ClipRectangle.Height));

					state = parent_pe.Graphics.Save();
					parent_pe.Graphics.TranslateTransform(-Left, -Top);
					parent.OnPaintBackground(parent_pe);
					parent_pe.Graphics.Restore(state);

					state = parent_pe.Graphics.Save();
					parent_pe.Graphics.TranslateTransform(-Left, -Top);
					parent.OnPaint(parent_pe);
					parent_pe.Graphics.Restore(state);
					parent_pe.SetGraphics(null);
				}
			}

			if ((clip_region != null) && (XplatUI.UserClipWontExposeParent)) {
				if (parent != null) {
					PaintEventArgs	parent_pe;
					Region		region;
					GraphicsState	state;
					Hwnd		hwnd;

					hwnd = Hwnd.ObjectFromHandle(Handle);

					if (hwnd != null) {
						parent_pe = new PaintEventArgs(pevent.Graphics, new Rectangle(pevent.ClipRectangle.X + Left, pevent.ClipRectangle.Y + Top, pevent.ClipRectangle.Width, pevent.ClipRectangle.Height));

						region = new Region ();
						region.MakeEmpty();
						region.Union(ClientRectangle);

						foreach (Rectangle r in hwnd.ClipRectangles) {
							region.Union (r);
						}

						state = parent_pe.Graphics.Save();
						parent_pe.Graphics.Clip = region;

						parent_pe.Graphics.TranslateTransform(-Left, -Top);
						parent.OnPaintBackground(parent_pe);
						parent_pe.Graphics.Restore(state);

						state = parent_pe.Graphics.Save();
						parent_pe.Graphics.Clip = region;

						parent_pe.Graphics.TranslateTransform(-Left, -Top);
						parent.OnPaint(parent_pe);
						parent_pe.Graphics.Restore(state);
						parent_pe.SetGraphics(null);

						region.Intersect(clip_region);
						pevent.Graphics.Clip = region;
					}
				}
			}

			if (background_image == null) {
				pevent.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), new Rectangle(pevent.ClipRectangle.X - 1, pevent.ClipRectangle.Y - 1, pevent.ClipRectangle.Width + 2, pevent.ClipRectangle.Height + 2));
				return;
			}

			DrawBackgroundImage (pevent.Graphics);
		}

		void DrawBackgroundImage (Graphics g)
		{
			using (TextureBrush b = new TextureBrush (background_image, WrapMode.Tile)) {
				g.FillRectangle (b, ClientRectangle);
			}
		}

		internal virtual void DndEnter (DragEventArgs e)
		{
			try {
				OnDragEnter (e);
			} catch { }
		}

		internal virtual void DndOver (DragEventArgs e)
		{
			try {
				OnDragOver (e);
			} catch { }
		}

		internal virtual void DndDrop (DragEventArgs e)
		{
			try {
				OnDragDrop (e);
			} catch (Exception exc) {
				Console.Error.WriteLine ("MWF: Exception while dropping:");
				Console.Error.WriteLine (exc);
			}
		}

		internal virtual void DndLeave (EventArgs e)
		{
			try {
				OnDragLeave (e);
			} catch { }
		}

		internal virtual void DndFeedback(GiveFeedbackEventArgs e)
		{
			try {
				OnGiveFeedback(e);
			} catch { }
		}

		internal virtual void DndContinueDrag(QueryContinueDragEventArgs e)
		{
			try {
				OnQueryContinueDrag(e);
			} catch { }
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

		internal void FireEnter ()
		{
			OnEnter (EventArgs.Empty);
		}

		internal void FireLeave ()
		{
			OnLeave (EventArgs.Empty);
		}

		internal void FireValidating (CancelEventArgs ce)
		{
			OnValidating (ce);
		}

		internal void FireValidated ()
		{
			OnValidated (EventArgs.Empty);
		}

		internal virtual bool ProcessControlMnemonic(char charCode) {
			return ProcessMnemonic(charCode);
		}

		private static Control FindFlatForward(Control container, Control start) {
			Control	found;
			int	index;
			int	end;

			found = null;
			end = container.child_controls.Count;

			if (start != null) {
				index = start.tab_index;
			} else {
				index = -1;
			}

			for (int i = 0, pos = -1; i < end; i++) {
				if (start == container.child_controls[i]) {
					pos = i;
					continue;
				}

				if (found == null) {
					if (container.child_controls[i].tab_index > index || (pos > -1 && pos < i && container.child_controls[i].tab_index == index)) {
						found = container.child_controls[i];
					}
				} else if (found.tab_index > container.child_controls[i].tab_index) {
					if (container.child_controls[i].tab_index > index) {
						found = container.child_controls[i];
					}
				}
			}
			return found;
		}

		private static Control FindControlForward(Control container, Control start) {
			Control found;

			found = null;

			if (start == null) {
				return FindFlatForward(container, start);
			}

			if (start.child_controls != null && start.child_controls.Count > 0 && 
				(start == container || !((start is IContainerControl) &&  start.GetStyle(ControlStyles.ContainerControl)))) {
				return FindControlForward(start, null);
			}
			else {
				while (start != container) {
					found = FindFlatForward(start.parent, start);
					if (found != null) {
						return found;
					}
					start = start.parent;
				}
			}
			return null;
		}

		private static Control FindFlatBackward(Control container, Control start) {
			Control	found;
			int	index;
			int	end;

			found = null;
			end = container.child_controls.Count;

			if (start != null) {
				index = start.tab_index;
			} else {
				// FIXME: Possible speed-up: Keep the highest taborder index in the container
				index = -1;
				for (int i = 0; i < end; i++) {
					if (container.child_controls[i].tab_index > index) {
						index = container.child_controls[i].tab_index;
					}
				}
				index++;
			}

			bool hit = false;
					
			for (int i = end - 1; i >= 0; i--) {
				if (start == container.child_controls[i]) {
					hit = true;
					continue;
				}

				if (found == null || found.tab_index < container.child_controls[i].tab_index) {
					if (container.child_controls[i].tab_index < index || (hit && container.child_controls[i].tab_index == index))
						found = container.child_controls[i];

				}
			}
			return found;
		}

		private static Control FindControlBackward(Control container, Control start) {

			Control found = null;

			if (start == null) {
				found = FindFlatBackward(container, start);
			}
			else if (start != container) {
				if (start.parent != null) {
					found = FindFlatBackward(start.parent, start);

					if (found == null) {
						if (start.parent != container)
							return start.parent;
						return null;
					}
				}
			}
		
			if (found == null || start.parent == null)
				found = start;

			while (found != null && (found == container || (!((found is IContainerControl) && found.GetStyle(ControlStyles.ContainerControl))) &&
				found.child_controls != null && found.child_controls.Count > 0)) {
//				while (ctl.child_controls != null && ctl.child_controls.Count > 0 && 
//					(ctl == this || (!((ctl is IContainerControl) && ctl.GetStyle(ControlStyles.ContainerControl))))) {
				found = FindFlatBackward(found, null);
			}

			return found;

/*
			Control found;

			found = null;

			if (start != null) {
				found = FindFlatBackward(start.parent, start);
				if (found == null) {
					if (start.parent != container) {
						return start.parent;
					}
				}
			}
			if (found == null) {
				found = FindFlatBackward(container, start);
			}

			if (container != start) {
				while ((found != null) && (!found.Contains(start)) && found.child_controls != null && found.child_controls.Count > 0 && !(found is IContainerControl)) {// || found.GetStyle(ControlStyles.ContainerControl))) {
					found = FindControlBackward(found, null);
					 if (found != null) {
						return found;
					}
				}
			}
			return found;
*/			
		}

		internal virtual void HandleClick(int clicks, MouseEventArgs me) {
			if (GetStyle(ControlStyles.StandardClick)) {
				if ((clicks > 1) && GetStyle(ControlStyles.StandardDoubleClick)) {
#if !NET_2_0
					OnDoubleClick(EventArgs.Empty);
				} else {
					OnClick(EventArgs.Empty);
#else
					OnDoubleClick(me);
				} else {
					OnClick(me);
#endif
				}
			}
		}

		private void CheckDataBindings ()
		{
			if (data_bindings == null)
				return;

			BindingContext binding_context = BindingContext;
			foreach (Binding binding in data_bindings) {
				binding.Check (binding_context);
			}
		}

		private void ChangeParent(Control new_parent) {
			bool		pre_enabled;
			bool		pre_visible;
			Font		pre_font;
			Color		pre_fore_color;
			Color		pre_back_color;
			RightToLeft	pre_rtl;

			// These properties are inherited from our parent
			// Get them pre parent-change and then send events
			// if they are changed after we have our new parent
			pre_enabled = Enabled;
			pre_visible = Visible;
			pre_font = Font;
			pre_fore_color = ForeColor;
			pre_back_color = BackColor;
			pre_rtl = RightToLeft;
			// MS doesn't seem to send a CursorChangedEvent

			parent = new_parent;

			if (IsHandleCreated)
				XplatUI.SetParent(Handle,
						  (new_parent == null || !new_parent.IsHandleCreated) ? IntPtr.Zero : new_parent.Handle);

			OnParentChanged(EventArgs.Empty);

			if (pre_enabled != Enabled) {
				OnEnabledChanged(EventArgs.Empty);
			}

			if (pre_visible != Visible) {
				OnVisibleChanged(EventArgs.Empty);
			}

			if (pre_font != Font) {
				OnFontChanged(EventArgs.Empty);
			}

			if (pre_fore_color != ForeColor) {
				OnForeColorChanged(EventArgs.Empty);
			}

			if (pre_back_color != BackColor) {
				OnBackColorChanged(EventArgs.Empty);
			}

			if (pre_rtl != RightToLeft) {
				// MS sneaks a OnCreateControl and OnHandleCreated in here, I guess
				// because when RTL changes they have to recreate the win32 control
				// We don't really need that (until someone runs into compatibility issues)
				OnRightToLeftChanged(EventArgs.Empty);
			}

			if ((new_parent != null) && new_parent.Created && !Created) {
				CreateControl();
			}

			if ((binding_context == null) && Created) {
				OnBindingContextChanged(EventArgs.Empty);
			}
		}

		private void UpdateDistances() {
			if ((parent != null) && (parent.layout_suspended == 0)) {
				dist_left = bounds.X;
				dist_top = bounds.Y;
				dist_right = parent.ClientSize.Width - bounds.X - bounds.Width;
				dist_bottom = parent.ClientSize.Height - bounds.Y - bounds.Height;
			}
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
		
#if NET_2_0
		[MonoTODO]
		public static bool CheckForIllegalCrossThreadCalls 
		{
			get {
				return verify_thread_handle;
			}

			set {
				verify_thread_handle = value;
			}
		}
#endif
		#endregion	// Public Static Properties

		#region Public Instance Properties
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AccessibleObject AccessibilityObject {
			get {
				if (accessibility_object==null) {
					accessibility_object=CreateAccessibilityInstance();
				}
				return accessibility_object;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string AccessibleDefaultActionDescription {
			get {
				return AccessibilityObject.default_action;
			}

			set {
				AccessibilityObject.default_action=value;
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[MWFCategory("Accessibility")]
		public string AccessibleDescription {
			get {
				return AccessibilityObject.description;
			}

			set {
				AccessibilityObject.description=value;
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[MWFCategory("Accessibility")]
		public string AccessibleName {
			get {
				return AccessibilityObject.Name;
			}

			set {
				AccessibilityObject.Name=value;
			}
		}

		[DefaultValue(AccessibleRole.Default)]
		[MWFDescription("Role of the control"), MWFCategory("Accessibility")]
		public AccessibleRole AccessibleRole {
			get {
				return AccessibilityObject.role;
			}

			set {
				AccessibilityObject.role=value;
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Behavior")]
		public virtual bool AllowDrop {
			get {
				return allow_drop;
			}

			set {
				if (allow_drop == value)
					return;
				allow_drop = value;
				if (IsHandleCreated) {
					UpdateStyles();
					XplatUI.SetAllowDrop (Handle, value);
				}
			}
		}

		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue(AnchorStyles.Top | AnchorStyles.Left)]
		[MWFCategory("Layout")]
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

#if NET_2_0
		// XXX: Implement me!
		bool auto_size;

		public virtual bool AutoSize {
			get {
				//Console.Error.WriteLine("Unimplemented: Control::get_AutoSize()");
				return auto_size;
			}
			set {
				Console.Error.WriteLine("Unimplemented: Control::set_AutoSize(bool)");
				auto_size = value;
			}
		}

		public virtual Size MaximumSize {
			get {
				return maximum_size;
			}
			set {
				maximum_size = value;
			}
		}

		public virtual Size MinimumSize {
			get {
				return minimum_size;
			}
			set {
				minimum_size = value;
			}
		}
#endif // NET_2_0

		[DispId(-501)]
		[MWFCategory("Appearance")]
		public virtual Color BackColor {
			get {
				if (background_color.IsEmpty) {
					if (parent!=null) {
						Color pcolor = parent.BackColor;
						if (pcolor.A == 0xff || GetStyle(ControlStyles.SupportsTransparentBackColor))
							return pcolor;
					}
					return DefaultBackColor;
				}
				return background_color;
			}

			set {
				if (!value.IsEmpty && (value.A != 0xff) && !GetStyle(ControlStyles.SupportsTransparentBackColor)) {
					throw new ArgumentException("Transparent background colors are not supported on this control");
				}

				if (background_color != value) {
					background_color=value;
					SetChildColor(this);
					OnBackColorChanged(EventArgs.Empty);
					Invalidate();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[MWFCategory("Appearance")]
		public virtual Image BackgroundImage {
			get {
				return background_image;
			}

			set {
				if (background_image!=value) {
					background_image=value;
					OnBackgroundImageChanged(EventArgs.Empty);
					Invalidate ();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual BindingContext BindingContext {
			get {
				if (binding_context != null)
					return binding_context;
				if (Parent == null)
					return null;
				binding_context = Parent.BindingContext;
				return binding_context;
			}
			set {
				if (binding_context != value) {
					binding_context = value;
					OnBindingContextChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Bottom {
			get {
				return bounds.Y+bounds.Height;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle Bounds {
			get {
				return this.bounds;
			}

			set {
				SetBounds(value.Left, value.Top, value.Width, value.Height, BoundsSpecified.All);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanFocus {
			get {
				if (IsHandleCreated && Visible && Enabled) {
					return true;
				}
				return false;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanSelect {
			get {
				Control	parent;

				if (!GetStyle(ControlStyles.Selectable)) {
					return false;
				}

				parent = this;
				while (parent != null) {
					if (!parent.is_visible || !parent.is_enabled) {
						return false;
					}

					parent = parent.parent;
				}
				return true;
			}
		}

		internal virtual bool InternalCapture {
			get {
				return Capture;
			}

			set {
				Capture = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Capture {
			get {
				return this.is_captured;
			}

			set {
				if (this.IsHandleCreated && value != is_captured) {
					if (value) {
						is_captured = true;
						XplatUI.GrabWindow(this.window.Handle, IntPtr.Zero);
					} else {
						XplatUI.UngrabWindow(this.window.Handle);
						is_captured = false;
					}
				}
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Focus")]
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle ClientRectangle {
			get {
				client_rect.Width = client_size.Width;
				client_rect.Height = client_size.Height;
				return client_rect;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
				this.SetClientSizeCore(value.Width, value.Height);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DescriptionAttribute("ControlCompanyNameDescr")]
		public String CompanyName {
			get {
				return "Mono Project, Novell, Inc.";
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ContainsFocus {
			get {
				IntPtr focused_window;

				focused_window = XplatUI.GetFocus();
				if (IsHandleCreated) {
					if (focused_window == Handle) {
						return true;
					}

					for (int i=0; i < child_controls.Count; i++) {
						if (child_controls[i].ContainsFocus) {
							return true;
						}
					}
				}
				return false;
			}
		}

		[DefaultValue(null)]
		[MWFCategory("Behavior")]
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

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlCollection Controls {
			get {
				return this.child_controls;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Created {
			get {
				return (!is_disposed && is_created);
			}
		}

		[AmbientValue(null)]
		[MWFCategory("Appearance")]
		public virtual Cursor Cursor {
			get {
				if (cursor != null) {
					return cursor;
				}

				if (parent != null) {
					return parent.Cursor;
				}

				return Cursors.Default;
			}

			set {
				if (cursor != value) {
					Point pt;

					cursor = value;
					
					if (IsHandleCreated) {
						pt = Cursor.Position;

						if (bounds.Contains(pt) || Capture) {
							if (GetChildAtPoint(pt) == null) {
								if (cursor != null) {
									XplatUI.SetCursor(window.Handle, cursor.handle);
								} else {
									if (parent != null) {
										XplatUI.SetCursor(window.Handle, parent.Cursor.handle);
									} else {
										XplatUI.SetCursor(window.Handle, Cursors.Default.handle);
									}
								}
							}
						}
					}

					OnCursorChanged(EventArgs.Empty);
				}
			}
		}


		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[ParenthesizePropertyName(true)]
		[RefreshProperties(RefreshProperties.All)]
		[MWFCategory("Data")]
		public ControlBindingsCollection DataBindings {
			get {
				if (data_bindings == null)
					data_bindings = new ControlBindingsCollection (this);
				return data_bindings;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual Rectangle DisplayRectangle {
			get {
				return ClientRectangle;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Disposing {
			get {
				return is_disposed;
			}
		}

		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue(DockStyle.None)]
		[MWFCategory("Layout")]
		public virtual DockStyle Dock {
			get {
				return dock_style;
			}

			set {
				if (dock_style == value) {
					return;
				}

				if (!Enum.IsDefined (typeof (DockStyle), value)) {
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (DockStyle));
				}

				dock_style = value;

				if (dock_style == DockStyle.None) {
					if (explicit_bounds == Rectangle.Empty)
						Bounds = new Rectangle (new Point (0, 0), DefaultSize);
					else
						Bounds = explicit_bounds;
				}

				if (parent != null) {
					parent.PerformLayout(this, "Parent");
				}

				OnDockChanged(EventArgs.Empty);
			}
		}

		[DispId(-514)]
		[Localizable(true)]
		[MWFCategory("Behavior")]
		public bool Enabled {
			get {
				if (!is_enabled) {
					return false;
				}

				if (parent != null) {
					return parent.Enabled;
				}

				return true;
			}

			set {

				bool old_value = is_enabled;

				is_enabled = value;
				if (old_value != value && !value && this.has_focus)
					SelectNextControl(this, true, true, true, true);

				OnEnabledChanged (EventArgs.Empty);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool Focused {
			get {
				return this.has_focus;
			}
		}

		[DispId(-512)]
		[AmbientValue(null)]
		[Localizable(true)]
		[MWFCategory("Appearance")]
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

			[param:MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(Font))]
			set {
				if (font != null && font.Equals (value)) {
					return;
				}

				font = value;
				Invalidate();
				OnFontChanged (EventArgs.Empty);
			}
		}

		[DispId(-513)]
		[MWFCategory("Appearance")]
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
					Invalidate();
					OnForeColorChanged(EventArgs.Empty);
				}
			}
		}

		[DispId(-515)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IntPtr Handle {							// IWin32Window
			get {
#if NET_2_0
				if (verify_thread_handle) {
					if (this.InvokeRequired) {
						throw new InvalidOperationException("Cross-thread access of handle detected. Handle access only valid on thread that created the control");
					}
				}
#endif
				if (!IsHandleCreated) {
					CreateHandle();
				}
				return window.Handle;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasChildren {
			get {
				if (this.child_controls.Count>0) {
					return true;
				}
				return false;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Height {
			get {
				return this.bounds.Height;
			}

			set {
				SetBounds(bounds.X, bounds.Y, bounds.Width, value, BoundsSpecified.Height);
			}
		}

		[AmbientValue(ImeMode.Inherit)]
		[Localizable(true)]
		[MWFCategory("Behavior")]
		public ImeMode ImeMode {
			get {
				if (ime_mode == ImeMode.Inherit) {
					if (parent != null)
						return parent.ImeMode;
					else
						return ImeMode.NoControl; // default value
				}
				return ime_mode;
			}

			set {
				if (ime_mode != value) {
					ime_mode = value;

					OnImeModeChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool InvokeRequired {						// ISynchronizeInvoke
			get {
				if (creator_thread != null && creator_thread!=Thread.CurrentThread) {
					return true;
				}
				return false;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsAccessible {
			get {
				return is_accessible;
			}

			set {
				is_accessible = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDisposed {
			get {
				return this.is_disposed;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsHandleCreated {
			get {
				if ((window != null) && (window.Handle != IntPtr.Zero)) {
					Hwnd hwnd = Hwnd.ObjectFromHandle (window.Handle);
					if (hwnd != null && !hwnd.zombie)
						return true;
				}

				return false;
			}
		}

#if NET_2_0
		public virtual Layout.LayoutEngine LayoutEngine {
			get { return new Layout.DefaultLayout (); }
		} 
#endif

		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Left {
			get {
				return this.bounds.X;
			}

			set {
				SetBounds(value, bounds.Y, bounds.Width, bounds.Height, BoundsSpecified.X);
			}
		}

		[Localizable(true)]
		[MWFCategory("Layout")]
		public Point Location {
			get {
				return new Point(bounds.X, bounds.Y);
			}

			set {
				SetBounds(value.X, value.Y, bounds.Width, bounds.Height, BoundsSpecified.Location);
			}
		}

#if NET_2_0
		[Localizable (true)]
		public Padding Margin {
			get { return this.margin; }
			set { this.margin = value; }
		}
#endif

		[Browsable(false)]
		public string Name {
			get {
				return name;
			}

			set {
				name = value;
			}
		}

#if NET_2_0
		[Localizable(true)]
		public Padding Padding {
			get {
				return padding;
			}

			set {
				padding = value;
			}
		}
#endif

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Control Parent {
			get {
				return this.parent;
			}

			set {
				if (value == this) {
					throw new ArgumentException("A circular control reference has been made. A control cannot be owned or parented to itself.");
				}

				if (parent!=value) {
					if (value==null) {
						parent.Controls.Remove(this);
						parent = null;
						return;
					}

					value.Controls.Add(this);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ProductName {
			get {
				Type t = typeof (AssemblyProductAttribute);
				Assembly assembly = GetType().Module.Assembly;
				object [] attrs = assembly.GetCustomAttributes (t, false);
				AssemblyProductAttribute a = null;
				// On MS we get a NullRefException if product attribute is not
				// set. 
				if (attrs != null && attrs.Length > 0)
					a = (AssemblyProductAttribute) attrs [0];
				return a.Product;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ProductVersion {
			get {
				Type t = typeof (AssemblyVersionAttribute);
				Assembly assembly = GetType().Module.Assembly;
				object [] attrs = assembly.GetCustomAttributes (t, false);
				if (attrs == null || attrs.Length < 1)
					return "1.0.0.0";
				return ((AssemblyVersionAttribute)attrs [0]).Version;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool RecreatingHandle {
			get {
				return is_recreating;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Region Region {
			get {
				return clip_region;
			}

			set {
				if (IsHandleCreated) {
					XplatUI.SetClipRegion(Handle, value);
				}
				clip_region = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Right {
			get {
				return this.bounds.X+this.bounds.Width;
			}
		}

		[AmbientValue(RightToLeft.Inherit)]
		[Localizable(true)]
		[MWFCategory("Appearance")]
		public virtual RightToLeft RightToLeft {
			get {
				if (right_to_left == RightToLeft.Inherit) {
					if (parent != null)
						return parent.RightToLeft;
					else
						return RightToLeft.No; // default value
				}
				return right_to_left;
			}

			set {
				if (value != right_to_left) {
					right_to_left = value;
					OnRightToLeftChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override ISite Site {
			get {
				return base.Site;
			}

			set {
				base.Site = value;

				if (value != null) {
					AmbientProperties ap = (AmbientProperties) value.GetService (typeof (AmbientProperties));
					if (ap != null) {
						BackColor = ap.BackColor;
						ForeColor = ap.ForeColor;
						Cursor = ap.Cursor;
						Font = ap.Font;
					}
				}
			}
		}

		[Localizable(true)]
		[MWFCategory("Layout")]
		public Size Size {
			get {
				return new Size(Width, Height);
			}

			set {
				SetBounds(bounds.X, bounds.Y, value.Width, value.Height, BoundsSpecified.Size);
			}
		}

		[Localizable(true)]
		[MergableProperty(false)]
		[MWFCategory("Behavior")]
		public int TabIndex {
			get {
				if (tab_index != -1) {
					return tab_index;
				}
				return 0;
			}

			set {
				if (tab_index != value) {
					tab_index = value;
					OnTabIndexChanged(EventArgs.Empty);
				}
			}
		}

		[DispId(-516)]
		[DefaultValue(true)]
		[MWFCategory("Behavior")]
		public bool TabStop {
			get {
				return tab_stop;
			}

			set {
				if (tab_stop != value) {
					tab_stop = value;
					OnTabStopChanged(EventArgs.Empty);
				}
			}
		}

		[Localizable(false)]
		[Bindable(true)]
		[TypeConverter(typeof(StringConverter))]
		[DefaultValue(null)]
		[MWFCategory("Data")]
		public object Tag {
			get {
				return control_tag;
			}

			set {
				control_tag = value;
			}
		}

		[DispId(-517)]
		[Localizable(true)]
		[BindableAttribute(true)]
		[MWFCategory("Appearance")]
		public virtual string Text {
			get {
				// Our implementation ignores ControlStyles.CacheText - we always cache
				return this.text;
			}

			set {
				if (value == null) {
					value = String.Empty;
				}

				if (text!=value) {
					text=value;
					if (IsHandleCreated) {
						/* we need to call .SetWindowStyle here instead of just .Text
						   because the presence/absence of Text (== "" or not) can cause
						   other window style things to appear/disappear */
						XplatUI.SetWindowStyle(window.Handle, CreateParams);
						XplatUI.Text(Handle, text);
					}
					OnTextChanged (EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Top {
			get {
				return this.bounds.Y;
			}

			set {
				SetBounds(bounds.X, value, bounds.Width, bounds.Height, BoundsSpecified.Y);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Control TopLevelControl {
			get {
				Control	p = this;

				while (p.parent != null) {
					p = p.parent;
				}

				return p is Form ? p : null;
			}
		}

		[Localizable(true)]
		[MWFCategory("Behavior")]
		public bool Visible {
			get {
				if (!is_visible) {
					return false;
				} else if (parent != null) {
					return parent.Visible;
				}

				return true;
			}

			set {
				SetVisibleCore(value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Width {
			get {
				return this.bounds.Width;
			}

			set {
				SetBounds(bounds.X, bounds.Y, value, bounds.Height, BoundsSpecified.Width);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IWindowTarget WindowTarget {
			get {
				return null;
			}

			set {
				;	// MS Internal
			}
		}
		#endregion	// Public Instance Properties

		#region	Protected Instance Properties
		protected virtual CreateParams CreateParams {
			get {
				CreateParams create_params = new CreateParams();

				try {
					create_params.Caption = Text;
				}
				catch {
					create_params.Caption = text;
				}

				try {
					create_params.X = Left;
				}
				catch {
					create_params.X = this.bounds.X;
				}

				try {
					create_params.Y = Top;
				}
				catch {
					create_params.Y = this.bounds.Y;
				}

				try {
					create_params.Width = Width;
				}
				catch {
					create_params.Width = this.bounds.Width;
				}

				try {
					create_params.Height = Height;
				}
				catch {
					create_params.Height = this.bounds.Height;
				}


				create_params.ClassName = XplatUI.DefaultClassName;
				create_params.ClassStyle = 0;
				create_params.ExStyle = 0;
				create_params.Param = 0;

				if (allow_drop) {
					create_params.ExStyle |= (int)WindowExStyles.WS_EX_ACCEPTFILES;
				}

				if ((parent!=null) && (parent.IsHandleCreated)) {
					create_params.Parent = parent.Handle;
				}

				create_params.Style = (int)WindowStyles.WS_CHILD | (int)WindowStyles.WS_CLIPCHILDREN | (int)WindowStyles.WS_CLIPSIBLINGS;

				if (is_visible) {
					create_params.Style |= (int)WindowStyles.WS_VISIBLE;
				}

				if (!is_enabled) {
					create_params.Style |= (int)WindowStyles.WS_DISABLED;
				}

				switch (border_style) {
				case BorderStyle.FixedSingle:
					create_params.Style |= (int) WindowStyles.WS_BORDER;
					break;
				case BorderStyle.Fixed3D:
					create_params.ExStyle |= (int) WindowExStyles.WS_EX_CLIENTEDGE;
					break;
				}

				return create_params;
			}
		}

		protected virtual ImeMode DefaultImeMode {
			get {
				return ImeMode.Inherit;
			}
		}

#if NET_2_0
		protected virtual Padding DefaultMargin {
			get { return new Padding (3); }
		}
#endif

		protected virtual Size DefaultSize {
			get {
				return new Size(0, 0);
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
		}

		protected bool ResizeRedraw {
			get {
				return GetStyle(ControlStyles.ResizeRedraw);
			}

			set {
				SetStyle(ControlStyles.ResizeRedraw, value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected virtual bool ShowFocusCues {
			get {
				return true;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected bool ShowKeyboardCues {
			get {
				return true;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Static Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Control FromChildHandle(IntPtr handle) {
			return Control.ControlNativeWindow.ControlFromChildHandle (handle);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Control FromHandle(IntPtr handle) {
			return Control.ControlNativeWindow.ControlFromHandle(handle);
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
		[EditorBrowsable(EditorBrowsableState.Advanced)]
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
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginInvoke(Delegate method) {
			object [] prms = null;
			if (method is EventHandler)
				prms = new object [] { this, EventArgs.Empty };
			return BeginInvokeInternal(method, prms, false);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginInvoke (Delegate method, object[] args) {
			return BeginInvokeInternal (method, args, false);
		}

		public void BringToFront() {
			if (parent != null) {
				parent.child_controls.SetChildIndex(this, 0);
				parent.Refresh();
			} else {
				XplatUI.SetZOrder(Handle, IntPtr.Zero, false, false);
			}
		}

		public bool Contains(Control ctl) {
			while (ctl != null) {
				ctl = ctl.parent;
				if (ctl == this) {
					return true;
				}
			}
			return false;
		}

		public void CreateControl() {
			if (is_disposed) {
				throw new ObjectDisposedException(GetType().FullName.ToString());
			}
			if (is_created) {
				return;
			}

			if (!IsHandleCreated) {
				CreateHandle();
			}

			if (!is_created) {
				is_created = true;
			}

			Control [] controls = child_controls.GetAllControls ();
			for (int i=0; i<controls.Length; i++) {
				controls [i].CreateControl ();
			}

			UpdateChildrenZOrder();

			if (binding_context == null) {	// seem to be sent whenever it's null?
				OnBindingContextChanged(EventArgs.Empty);
			}

			OnCreateControl();
		}

		public Graphics CreateGraphics() {
			if (!IsHandleCreated) {
				this.CreateHandle();
			}
			return Graphics.FromHwnd(this.window.Handle);
		}

		public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects) {
			return XplatUI.StartDrag(this.window.Handle, data, allowedEffects);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public object EndInvoke (IAsyncResult async_result) {
			AsyncMethodResult result = (AsyncMethodResult) async_result;
			return result.EndInvoke ();
		}

		public Form FindForm() {
			Control	c;

			c = this;
			while (c != null) {
				if (c is Form) {
					return (Form)c;
				}
				c = c.Parent;
			}
			return null;
		}

		public bool Focus() {
			if (CanFocus && IsHandleCreated && !has_focus && !is_focusing) {
				is_focusing = true;
				Select(this);
				is_focusing = false;
			}
			return has_focus;
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
				if ((current is IContainerControl) && ((current.control_style & ControlStyles.ContainerControl)!=0)) {
					return (IContainerControl)current;
				}
				current = current.parent;
			}
			return null;
		}

		public Control GetNextControl(Control ctl, bool forward) {

			if (!this.Contains(ctl)) {
				ctl = this;
			}

			if (forward) {
				ctl = FindControlForward(this, ctl);
			}
			else {
				ctl = FindControlBackward(this, ctl);
			}

			if (ctl != this) {
				return ctl;
			}
			return null;
		}

#if NET_2_0
		public virtual Size GetPreferredSize (Size proposedSize) {
			return preferred_size;
		}
#endif

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
			if (!IsHandleCreated || !Visible || rc.Width == 0 || rc.Height == 0) {
				return;
			}

			NotifyInvalidate(rc);

			XplatUI.Invalidate(Handle, rc, false);

			if (invalidateChildren) {
				Control [] controls = child_controls.GetAllControls ();
				for (int i=0; i<controls.Length; i++)
					controls [i].Invalidate ();
			}
			OnInvalidated(new InvalidateEventArgs(rc));
		}

		public void Invalidate(System.Drawing.Region region) {
			Invalidate(region, false);
		}

		public void Invalidate(System.Drawing.Region region, bool invalidateChildren) {
			RectangleF bounds = region.GetBounds (CreateGraphics ());
			Invalidate (new Rectangle ((int) bounds.X, (int) bounds.Y, (int) bounds.Width, (int) bounds.Height),
					invalidateChildren);
		}

		public object Invoke (Delegate method) {
			object [] prms = null;
			if (method is EventHandler)
				prms = new object [] { this, EventArgs.Empty };

			return Invoke(method, prms);
		}

		public object Invoke (Delegate method, object[] args) {
			if (!this.InvokeRequired) {
				return method.DynamicInvoke(args);
			}

			IAsyncResult result = BeginInvoke (method, args);
			return EndInvoke(result);
		}

		internal object InvokeInternal (Delegate method, bool disposing) {
			return InvokeInternal(method, null, disposing);
		}

		internal object InvokeInternal (Delegate method, object[] args, bool disposing) {
			if (!this.InvokeRequired) {
				return method.DynamicInvoke(args);
			}

			IAsyncResult result = BeginInvokeInternal (method, args, disposing);
			return EndInvoke(result);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void PerformLayout() {
			PerformLayout(null, null);
		}

#if !NET_2_0
		private void SetImplicitBounds (int x, int y, int width, int height)
		{
			Rectangle saved_bounds = explicit_bounds;
			SetBounds (x, y, width, height);
			explicit_bounds = saved_bounds;
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void PerformLayout(Control affectedControl, string affectedProperty) {
			LayoutEventArgs levent = new LayoutEventArgs(affectedControl, affectedProperty);

			if (layout_suspended > 0) {
				layout_pending = true;
				return;
			}

			layout_pending = false;

			// Prevent us from getting messed up
			layout_suspended++;

			// Perform all Dock and Anchor calculations
			try {

#if NET_2_0
			this.layout_engine.Layout(this, levent);
#else		
				// This has been moved to Layout/DefaultLayout.cs for 2.0, please duplicate any changes/fixes there.
				Control		child;
				AnchorStyles	anchor;
				Rectangle	space;

				space = DisplayRectangle;

				// Deal with docking; go through in reverse, MS docs say that lowest Z-order is closest to edge
				Control [] controls = child_controls.GetAllControls ();
				for (int i = controls.Length - 1; i >= 0; i--) {
					child = controls [i];

					if (!child.Visible) {
						continue;
					}

					switch (child.Dock) {
						case DockStyle.None: {
							// Do nothing
							break;
						}

						case DockStyle.Left: {
							child.SetImplicitBounds(space.Left, space.Y, child.Width, space.Height);
							space.X+=child.Width;
							space.Width-=child.Width;
							break;
						}

						case DockStyle.Top: {
							child.SetImplicitBounds(space.Left, space.Y, space.Width, child.Height);
							space.Y+=child.Height;
							space.Height-=child.Height;
							break;
						}
					
						case DockStyle.Right: {
							child.SetImplicitBounds(space.Right-child.Width, space.Y, child.Width, space.Height);
							space.Width-=child.Width;
							break;
						}

						case DockStyle.Bottom: {
							child.SetImplicitBounds(space.Left, space.Bottom-child.Height, space.Width, child.Height);
							space.Height-=child.Height;
							break;
						}
					}
				}

				for (int i = controls.Length - 1; i >= 0; i--) {
					child=controls[i];

					//if (child.Visible && (child.Dock == DockStyle.Fill)) {
					if (child.Dock == DockStyle.Fill) {
						child.SetImplicitBounds(space.Left, space.Top, space.Width, space.Height);
					}
				}

				space = DisplayRectangle;

				for (int i=0; i < controls.Length; i++) {
					int left;
					int top;
					int width;
					int height;

					child = controls[i];

					// If the control is docked we don't need to do anything
					if (child.Dock != DockStyle.None) {
						continue;
					}

					anchor = child.Anchor;

					left = child.Left;
					top = child.Top;
					width = child.Width;
					height = child.Height;

					if ((anchor & AnchorStyles.Left) !=0 ) {
						if ((anchor & AnchorStyles.Right) != 0) {
							width = space.Width - child.dist_right - left;
						} else {
							; // Left anchored only, nothing to be done
						}
					} else if ((anchor & AnchorStyles.Right) != 0) {
						left = space.Width - child.dist_right - width;
					} else {
						// left+=diff_width/2 will introduce rounding errors (diff_width removed from svn after r51780)
						// This calculates from scratch every time:
						left = child.dist_left + (space.Width - (child.dist_left + width + child.dist_right)) / 2;
					}

					if ((anchor & AnchorStyles.Top) !=0 ) {
						if ((anchor & AnchorStyles.Bottom) != 0) {
							height = space.Height - child.dist_bottom - top;
						} else {
							; // Top anchored only, nothing to be done
						}
					} else if ((anchor & AnchorStyles.Bottom) != 0) {
						top = space.Height - child.dist_bottom - height;
					} else {
						// top += diff_height/2 will introduce rounding errors (diff_height removed from after r51780)
						// This calculates from scratch every time:
						top = child.dist_top + (space.Height - (child.dist_top + height + child.dist_bottom)) / 2;
					}
					
					// Sanity
					if (width < 0) {
						width=0;
					}

					if (height < 0) {
						height=0;
					}

					child.SetImplicitBounds(left, top, width, height);
				}
#endif

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
			return InternalPreProcessMessage (ref msg);
		}

		internal virtual bool InternalPreProcessMessage (ref Message msg) {
			Keys key_data;

			if ((msg.Msg == (int)Msg.WM_KEYDOWN) || (msg.Msg == (int)Msg.WM_SYSKEYDOWN)) {
				key_data = (Keys)msg.WParam.ToInt32() | XplatUI.State.ModifierKeys;

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
			} else if (msg.Msg == (int)Msg.WM_SYSCHAR) {
				return ProcessDialogChar((char)msg.WParam);
			}
			return false;
		}

		public Rectangle RectangleToClient(Rectangle r) {
			return new Rectangle(PointToClient(r.Location), r.Size);
		}

		public Rectangle RectangleToScreen(Rectangle r) {
			return new Rectangle(PointToScreen(r.Location), r.Size);
		}

		public virtual void Refresh() {			
			if (IsHandleCreated == true) {
				Invalidate();
				XplatUI.UpdateWindow(window.Handle);

				Control [] controls = child_controls.GetAllControls ();
				for (int i=0; i < controls.Length; i++) {
					controls[i].Refresh();
				}
				
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetBackColor() {
			BackColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBindings() {
			data_bindings.Clear();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetCursor() {
			Cursor = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetFont() {
			font = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetForeColor() {
			foreground_color = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImeMode() {
			ime_mode = DefaultImeMode;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void ResetRightToLeft() {
			right_to_left = RightToLeft.Inherit;
		}

		public virtual void ResetText() {
			text = String.Empty;
		}

		public void ResumeLayout() {
			ResumeLayout (true);
		}

		public void ResumeLayout(bool performLayout) {
			if (layout_suspended > 0) {
				layout_suspended--;
			}

			if (layout_suspended == 0) {
				Control [] controls = child_controls.GetAllControls ();
				for (int i=0; i<controls.Length; i++) {
					controls [i].UpdateDistances ();
				}

				if (performLayout && layout_pending) {
					PerformLayout();
				}
			}
		}

		public void Scale(float ratio) {
			ScaleCore(ratio, ratio);
		}

		public void Scale(float dx, float dy) {
			ScaleCore(dx, dy);
		}

#if NET_2_0
		public void Scale(SizeF factor) {
			ScaleCore(factor.Width, factor.Height);
		}
#endif

		public void Select() {
			Select(false, false);
		}

#if DebugFocus
		private void printTree(Control c, string t) {
			foreach(Control i in c.child_controls) {
				Console.WriteLine("{2}{0}.TabIndex={1}", i, i.tab_index, t);
				printTree(i, t+"\t");
			}
		}
#endif
		public bool SelectNextControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap) {
			Control c;

#if DebugFocus
			Console.WriteLine("{0}", this.FindForm());
			printTree(this, "\t");
#endif

			if (!this.Contains(ctl) || (!nested && (ctl.parent != this))) {
				ctl = null;
			}
			c = ctl;
			do {
				c = GetNextControl(c, forward);
				if (c == null) {
					if (wrap) {
						wrap = false;
						continue;
					}
					break;
				}

				if (c.CanSelect && ((c.parent == this) || nested) && (c.tab_stop || !tabStopOnly)) {
					c.Select (true, true);
					return true;
				}
			} while (c != ctl); // If we wrap back to ourselves we stop

			return false;
		}

		public void SendToBack() {
			if (parent != null) {
				parent.child_controls.SetChildIndex(this, parent.child_controls.Count);
			}
		}

		public void SetBounds(int x, int y, int width, int height) {
			SetBounds(x, y, width, height, BoundsSpecified.All);
		}

		public void SetBounds(int x, int y, int width, int height, BoundsSpecified specified) {
			if ((specified & BoundsSpecified.X) != BoundsSpecified.X) {
				x = Left;
			}

			if ((specified & BoundsSpecified.Y) != BoundsSpecified.Y) {
				y = Top;
			}

			if ((specified & BoundsSpecified.Width) != BoundsSpecified.Width) {
				width = Width;
			}

			if ((specified & BoundsSpecified.Height) != BoundsSpecified.Height) {
				height = Height;
			}

			SetBoundsCore(x, y, width, height, specified);
			if (parent != null) {
				parent.PerformLayout(this, "Bounds");
			}
		}

		public void Show() {
			if (!is_created) {
				this.CreateControl();
			}

			this.Visible=true;
		}

		public void SuspendLayout() {
			layout_suspended++;
		}

		public void Update() {
			if (IsHandleCreated) {
				XplatUI.UpdateWindow(window.Handle);
			}
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MonoTODO("Implement this and tie it into Control.ControlAccessibleObject.NotifyClients")]
		protected void AccessibilityNotifyClients(AccessibleEvents accEvent, int childID) {
			throw new NotImplementedException();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual AccessibleObject CreateAccessibilityInstance() {
			return new Control.ControlAccessibleObject(this);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual ControlCollection CreateControlsInstance() {
			return new ControlCollection(this);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void CreateHandle() {
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().FullName.ToString());
			}

			if (IsHandleCreated && !is_recreating) {
				return;
			}

			window.CreateHandle(CreateParams);

			if (window.Handle != IntPtr.Zero) {
				creator_thread = Thread.CurrentThread;

				XplatUI.EnableWindow(window.Handle, is_enabled);
				XplatUI.SetVisible(window.Handle, is_visible, true);

				if (clip_region != null) {
					XplatUI.SetClipRegion(Handle, clip_region);
				}

				// Set our handle with our parent
				if ((parent != null) && (parent.IsHandleCreated)) {
					XplatUI.SetParent(window.Handle, parent.Handle);
				}

				// Set our handle as parent for our children
				Control [] children;

				children = child_controls.GetAllControls ();
				for (int i = 0; i < children.Length; i++ ) {
					if (!children[i].RecreatingHandle)
						XplatUI.SetParent(children[i].Handle, window.Handle); 
				}

				UpdateStyles();
				XplatUI.SetAllowDrop (Handle, allow_drop);

				// Find out where the window manager placed us
				if ((CreateParams.Style & (int)WindowStyles.WS_CHILD) != 0) {
					XplatUI.SetBorderStyle(window.Handle, (FormBorderStyle)border_style);
				}
				UpdateBounds();

				OnHandleCreated(EventArgs.Empty);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void DefWndProc(ref Message m) {
			window.DefWndProc(ref m);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void DestroyHandle() {
			if (IsHandleCreated) {
				if (window != null) {
					window.DestroyHandle();
				}
			}
		}

		protected internal bool GetStyle(ControlStyles flag) {
			return (control_style & flag) != 0;
		}

		protected bool GetTopLevel() {
			return is_toplevel;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void InitLayout() {
			UpdateDistances();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void InvokeGotFocus(Control toInvoke, EventArgs e) {
			toInvoke.OnGotFocus(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void InvokeLostFocus(Control toInvoke, EventArgs e) {
			toInvoke.OnLostFocus(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void InvokeOnClick(Control toInvoke, EventArgs e) {
			toInvoke.OnClick(e);
		}

		protected void InvokePaint(Control toInvoke, PaintEventArgs e) {
			toInvoke.OnPaint(e);
		}

		protected void InvokePaintBackground(Control toInvoke, PaintEventArgs e) {
			toInvoke.OnPaintBackground(e);
		}

		protected virtual bool IsInputChar (char charCode) {
			return true;
		}

		protected virtual bool IsInputKey (Keys keyData) {
			// Doc says this one calls IsInputChar; not sure what to do with that
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void NotifyInvalidate(Rectangle invalidatedArea) {
			// override me?
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

			switch (msg.Msg) {
				case (int)Msg.WM_SYSKEYDOWN:
				case (int)Msg.WM_KEYDOWN: {
					key_event = new KeyEventArgs ((Keys)msg.WParam.ToInt32 ());
					OnKeyDown (key_event);
					return key_event.Handled;
				}

				case (int)Msg.WM_SYSKEYUP:
				case (int)Msg.WM_KEYUP: {
					key_event = new KeyEventArgs ((Keys)msg.WParam.ToInt32 ());
					OnKeyUp (key_event);
					return key_event.Handled;
				}

				case (int)Msg.WM_SYSCHAR:
				case (int)Msg.WM_CHAR: {
					KeyPressEventArgs	key_press_event;

					key_press_event = new KeyPressEventArgs((char)msg.WParam);
					OnKeyPress(key_press_event);
#if NET_2_0
					msg.WParam = (IntPtr)key_press_event.KeyChar;
#endif
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseDragEvent(object key, DragEventArgs e) {
			// MS Internal
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseKeyEvent(object key, KeyEventArgs e) {
			// MS Internal
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseMouseEvent(object key, MouseEventArgs e) {
			// MS Internal
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaisePaintEvent(object key, PaintEventArgs e) {
			// MS Internal
		}

		private void SetIsRecreating ()
		{
			is_recreating=true;

			foreach (Control c in Controls.GetAllControls()) {
				c.SetIsRecreating ();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RecreateHandle() {
#if DebugRecreate
			Console.WriteLine("Recreating control {0}", XplatUI.Window(window.Handle));
#endif

			SetIsRecreating ();

			if (IsHandleCreated) {
#if DebugRecreate
				Console.WriteLine(" + handle is created, destroying it.");
#endif
				DestroyHandle();
				// WM_DESTROY will CreateHandle for us
			} else {
#if DebugRecreate
				Console.WriteLine(" + handle is not created, creating it.");
#endif
				if (!is_created) {
					CreateControl();
				} else {
					CreateHandle();
				}

				is_recreating = false;
#if DebugRecreate
				Console.WriteLine (" + new handle = {0:X}", Handle.ToInt32());
#endif
			}

		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void ResetMouseEventArgs() {
			// MS Internal
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected ContentAlignment RtlTranslateContent(ContentAlignment align) {
			return RtlTranslateAlignment(align);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected HorizontalAlignment RtlTranslateHorizontal(HorizontalAlignment align) {
			return RtlTranslateAlignment(align);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected LeftRightAlignment RtlTranslateLeftRight(LeftRightAlignment align) {
			return RtlTranslateAlignment(align);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void ScaleCore(float dx, float dy) {
			Point	location;
			Size	size;

			SuspendLayout();

			location = new Point((int)(Left * dx), (int)(Top * dy));
			size = this.ClientSize;

			if (!GetStyle(ControlStyles.FixedWidth)) {
				size.Width = (int)(size.Width * dx);
			}

			if (!GetStyle(ControlStyles.FixedHeight)) {
				size.Height = (int)(size.Height * dy);
			}

			SetBounds(location.X, location.Y, size.Width, size.Height, BoundsSpecified.All);

			/* Now scale our children */
			Control [] controls = child_controls.GetAllControls ();
			for (int i=0; i < controls.Length; i++) {
				controls[i].Scale(dx, dy);
			}

			ResumeLayout();
		}

		protected virtual void Select(bool directed, bool forward) {
			IContainerControl	container;
			
			container = GetContainerControl();
			if (container != null)
				container.ActiveControl = this;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// SetBoundsCore updates the Win32 control itself. UpdateBounds updates the controls variables and fires events, I'm guessing - pdb
			if (IsHandleCreated) {
				XplatUI.SetWindowPos(Handle, x, y, width, height);

				// Win32 automatically changes negative width/height to 0.
				// The control has already been sent a WM_WINDOWPOSCHANGED message and it has the correct
				// data, but it'll be overwritten when we call UpdateBounds unless we get the updated
				// size.
				if (width < 0 || height < 0) {
					int cw, ch, ix, iy;
					XplatUI.GetWindowPos(Handle, this is Form, out ix, out iy, out width, out height, out cw, out ch);
				}
			}

			UpdateBounds(x, y, width, height);

			UpdateDistances();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void SetClientSizeCore(int x, int y) {
			// Calculate the actual window size from the client size (it usually stays the same or grows)
			Rectangle	ClientRect;
			Rectangle	WindowRect;
			CreateParams	cp;

			ClientRect = new Rectangle(0, 0, x, y);
			cp = this.CreateParams;

			if (XplatUI.CalculateWindowRect(ref ClientRect, cp.Style, cp.ExStyle, null, out WindowRect)==false) {
				return;
			}

			SetBounds(bounds.X, bounds.Y, WindowRect.Width, WindowRect.Height, BoundsSpecified.Size);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected internal void SetStyle(ControlStyles flag, bool value) {
			if (value) {
				control_style |= flag;
			} else {
				control_style &= ~flag;
			}
		}

		protected void SetTopLevel(bool value) {
			if ((GetTopLevel() != value) && (parent != null)) {
				throw new Exception();
			}

			if (this is Form) {
				if (value == true) {
					if (!Visible) {
						Visible = true;
					}
				} else {
					if (Visible) {
						Visible = false;
					}
				}
			}
			is_toplevel = value;
		}

		protected virtual void SetVisibleCore(bool value) {
			if (value!=is_visible) {
				if (value && (window.Handle == IntPtr.Zero) || !is_created) {
					CreateControl();
				}

				is_visible=value;

				if (IsHandleCreated) {
					XplatUI.SetVisible(Handle, value, true);
					// Explicitly move Toplevel windows to where we want them;
					// apparently moving unmapped toplevel windows doesn't work
					if (is_visible && (this is Form)) {
						XplatUI.SetWindowPos(window.Handle, bounds.X, bounds.Y, bounds.Width, bounds.Height);
					}
				}

				OnVisibleChanged(EventArgs.Empty);

				if (value == false && parent != null && Focused) {
					Control	container;

					// Need to start at parent, GetContainerControl might return ourselves if we're a container
					container = (Control)parent.GetContainerControl();
					if (container != null) {
						container.SelectNextControl(this, true, true, true, true);
					}
				}

				if (parent != null) {
					parent.PerformLayout(this, "visible");
				} else {
					PerformLayout(this, "visible");
				}
			}
		}
	
		[EditorBrowsable(EditorBrowsableState.Advanced)]
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

			XplatUI.GetWindowPos(this.Handle, this is Form, out x, out y, out width, out height, out client_width, out client_height);

			UpdateBounds(x, y, width, height, client_width, client_height);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateBounds(int x, int y, int width, int height) {
			CreateParams	cp;
			Rectangle	rect;

			// Calculate client rectangle
			rect = new Rectangle(0, 0, 0, 0);
			cp = CreateParams;

			XplatUI.CalculateWindowRect(ref rect, cp.Style, cp.ExStyle, cp.menu, out rect);
			UpdateBounds(x, y, width, height, width - (rect.Right - rect.Left), height - (rect.Bottom - rect.Top));
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateBounds(int x, int y, int width, int height, int clientWidth, int clientHeight) {
			// UpdateBounds only seems to set our sizes and fire events but not update the GUI window to match
			bool	moved	= false;
			bool	resized	= false;

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

			// Assume explicit bounds set. SetImplicitBounds will restore old bounds
			explicit_bounds = bounds;

			client_size.Width=clientWidth;
			client_size.Height=clientHeight;

			if (moved) {
				OnLocationChanged(EventArgs.Empty);
			}

			if (resized) {
				OnSizeChanged(EventArgs.Empty);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateStyles() {
			if (!IsHandleCreated) {
				return;
			}

			XplatUI.SetWindowStyle(window.Handle, CreateParams);
			OnStyleChanged(EventArgs.Empty);
		}

		private void UpdateZOrderOfChild(Control child) {
			if (IsHandleCreated && child.IsHandleCreated && (child.parent == this)) {
				int	index;

				index = child_controls.IndexOf(child);

				if (index > 0) {
					XplatUI.SetZOrder(child.Handle, child_controls[index - 1].Handle, false, false);
				} else {
					XplatUI.SetZOrder(child.Handle, IntPtr.Zero, true, false);
				}
			}
		}

		private void UpdateChildrenZOrder() {
			Control [] controls;

			if (!IsHandleCreated) {
				return;
			}

			controls = child_controls.GetAllControls ();
			for (int i = 1; i < controls.Length; i++ ) {
				XplatUI.SetZOrder(controls[i].Handle, controls[i-1].Handle, false, false);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void UpdateZOrder() {
			if (parent != null) {
				parent.UpdateZOrderOfChild(this);
			}
		}

		protected virtual void WndProc(ref Message m) {
#if debug
			Console.WriteLine("Control {0} received message {1}", window.Handle == IntPtr.Zero ? this.Text : XplatUI.Window(window.Handle), m.ToString ());
#endif
			if ((this.control_style & ControlStyles.EnableNotifyMessage) != 0) {
				OnNotifyMessage(m);
			}

			switch((Msg)m.Msg) {
			case Msg.WM_DESTROY: {
				OnHandleDestroyed(EventArgs.Empty);
#if DebugRecreate
				IntPtr handle = window.Handle;
#endif
				window.InvalidateHandle();

				if (is_recreating) {
#if DebugRecreate
					Console.WriteLine ("Creating handle for {0:X}", handle.ToInt32());
#endif
					CreateHandle();
#if DebugRecreate
					Console.WriteLine (" + new handle = {0:X}", Handle.ToInt32());
#endif
					is_recreating = false;
				}
				return;
			}

			case Msg.WM_WINDOWPOSCHANGED: {
				if (Visible) {
					Rectangle save_bounds = explicit_bounds;
					UpdateBounds();
					explicit_bounds = save_bounds;
					if (GetStyle(ControlStyles.ResizeRedraw)) {
						Invalidate();
					}
				}
				return;
			}

			// Nice description of what should happen when handling WM_PAINT
			// can be found here: http://pluralsight.com/wiki/default.aspx/Craig/FlickerFreeControlDrawing.html
			// and here http://msdn.microsoft.com/msdnmag/issues/06/03/WindowsFormsPerformance/
			case Msg.WM_PAINT: {
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart(Handle, true);

				if (paint_event == null) {
					return;
				}

				if (invalid_region != null && !invalid_region.IsVisible (paint_event.ClipRectangle)) {

					// Just blit the previous image
					XplatUI.BlitFromOffscreen (Handle, paint_event.Graphics, backbuffer, backbuffer_dc, paint_event.ClipRectangle);
					XplatUI.PaintEventEnd (Handle, true);
					return;
				}

				Graphics dc = null;
				Graphics back_dc = null;
				object back = null;
				if (ThemeEngine.Current.DoubleBufferingSupported) {
					if ((control_style & ControlStyles.DoubleBuffer) != 0) {
						CreateBackBuffer ();
						back = backbuffer;
						back_dc = backbuffer_dc;
						dc = paint_event.SetGraphics (back_dc);
					}
				}

				if (!GetStyle(ControlStyles.Opaque)) {
					OnPaintBackground(paint_event);
				}

				// Button-derived controls choose to ignore their Opaque style, give them a chance to draw their background anyways
				OnPaintBackgroundInternal(paint_event);

				OnPaintInternal(paint_event);
				if (!paint_event.Handled) {
					OnPaint(paint_event);
				}

				if (ThemeEngine.Current.DoubleBufferingSupported)
					if ((control_style & ControlStyles.DoubleBuffer) != 0) {
						XplatUI.BlitFromOffscreen (Handle, dc, back, back_dc, paint_event.ClipRectangle);
						paint_event.SetGraphics (dc);
						invalid_region.Exclude (paint_event.ClipRectangle);

						if (back != backbuffer)
							XplatUI.DestroyOffscreenDrawable (back, back_dc);
					}

				XplatUI.PaintEventEnd(Handle, true);

				return;
			}
					
			case Msg.WM_ERASEBKGND: {
				// The DefWndProc will never have to handle this, we always paint the background in managed code
				// In theory this code would look at ControlStyles.AllPaintingInWmPaint and and call OnPaintBackground
				// here but it just makes things more complicated...
				m.Result = (IntPtr)1;
				return;
			}

			case Msg.WM_LBUTTONUP: {
				MouseEventArgs me;

				me = new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Left, 
							 mouse_clicks, 
							 LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							 0);

				HandleClick(mouse_clicks, me);
				OnMouseUp (me);

				if (InternalCapture) {
					InternalCapture = false;
				}

				if (mouse_clicks > 1) {
					mouse_clicks = 1;
				}
				return;
			}
					
			case Msg.WM_LBUTTONDOWN: {
				if (CanSelect) {
					Select (true, true);
				}
				InternalCapture = true;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				
				return;
			}

			case Msg.WM_LBUTTONDBLCLK: {
				InternalCapture = true;
				mouse_clicks++;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_MBUTTONUP: {
				MouseEventArgs me;

				me = new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Middle, 
							 mouse_clicks, 
							 LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							 0);

				HandleClick(mouse_clicks, me);
				OnMouseUp (me);
				if (InternalCapture) {
					InternalCapture = false;
				}
				if (mouse_clicks > 1) {
					mouse_clicks = 1;
				}
				return;
			}
					
			case Msg.WM_MBUTTONDOWN: {					
				InternalCapture = true;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_MBUTTONDBLCLK: {
				InternalCapture = true;
				mouse_clicks++;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_RBUTTONUP: {
				MouseEventArgs	me;
				Point		pt;

				pt = new Point(LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()));
				pt = PointToScreen(pt);

				XplatUI.SendMessage(m.HWnd, Msg.WM_CONTEXTMENU, m.HWnd, (IntPtr)(pt.X + (pt.Y << 16)));

				me = new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()) | MouseButtons.Right, 
							 mouse_clicks, 
							 LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
							 0);

				HandleClick(mouse_clicks, me);
				OnMouseUp (me);

				if (InternalCapture) {
					InternalCapture = false;
				}

				if (mouse_clicks > 1) {
					mouse_clicks = 1;
				}
				return;
			}
					
			case Msg.WM_RBUTTONDOWN: {					
				InternalCapture = true;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_RBUTTONDBLCLK: {
				InternalCapture = true;
				mouse_clicks++;
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								 mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								 0));
				return;
			}

			case Msg.WM_CONTEXTMENU: {
				if (context_menu != null) {
					Point	pt;

					pt = new Point(LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()));
					context_menu.Show(this, PointToClient(pt));
					return;
				}

				DefWndProc(ref m);
				return;
			}

			case Msg.WM_MOUSEWHEEL: {				
				DefWndProc(ref m);
				OnMouseWheel (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								  mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								  HighOrder(m.WParam.ToInt32())));
				return;
			}


			case Msg.WM_MOUSEMOVE: {					
				OnMouseMove  (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
								  mouse_clicks, 
								  LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
								  0));
				return;
			}

			case Msg.WM_MOUSE_ENTER: {
				if (is_entered) {
					return;
				}
				is_entered = true;
				OnMouseEnter(EventArgs.Empty);
				return;
			}

			case Msg.WM_MOUSE_LEAVE: {
				is_entered=false;
				OnMouseLeave(EventArgs.Empty);
				return;
			}

			case Msg.WM_MOUSEHOVER:	{
				OnMouseHover(EventArgs.Empty);
				return;
			}

			case Msg.WM_SYSKEYUP: {
				if (ProcessKeyMessage(ref m)) {
					m.Result = IntPtr.Zero;
					return;
				}

				if ((m.WParam.ToInt32() & (int)Keys.KeyCode) == (int)Keys.Menu) {
					Form	form;

					form = FindForm();
					if (form != null && form.ActiveMenu != null) {
						form.ActiveMenu.ProcessCmdKey(ref m, (Keys)m.WParam.ToInt32());
					}
				}

				DefWndProc (ref m);
				return;
			}

			case Msg.WM_SYSKEYDOWN:
			case Msg.WM_KEYDOWN:
			case Msg.WM_KEYUP:
			case Msg.WM_SYSCHAR:
			case Msg.WM_CHAR: {
				if (ProcessKeyMessage(ref m)) {
					m.Result = IntPtr.Zero;
					return;
				}
				DefWndProc (ref m);
				return;
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
				m.Result = (IntPtr)1;
				return;
			}

			case Msg.WM_KILLFOCUS: {
				this.has_focus = false;
				OnLostFocusInternal (EventArgs.Empty);
				return;
			}

			case Msg.WM_SETFOCUS: {
				if (!has_focus) {
					this.has_focus = true;
					OnGotFocusInternal (EventArgs.Empty);
				}
				return;
			}
					
			case Msg.WM_SYSCOLORCHANGE: {
				ThemeEngine.Current.ResetDefaults();
				OnSystemColorsChanged(EventArgs.Empty);
				return;
			}

			case Msg.WM_SETCURSOR: {
				if ((cursor == null) || ((HitTest)(m.LParam.ToInt32() & 0xffff) != HitTest.HTCLIENT)) {
					DefWndProc(ref m);
					return;
				}

				XplatUI.SetCursor(window.Handle, cursor.handle);
				m.Result = (IntPtr)1;

				return;
			}

			default:
				DefWndProc(ref m);
				return;
			}
		}
		#endregion	// Public Instance Methods

		#region OnXXX methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnBackColorChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [BackColorChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackColorChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnBackgroundImageChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [BackgroundImageChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBackgroundImageChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnBindingContextChanged(EventArgs e) {
			CheckDataBindings ();
			EventHandler eh = (EventHandler)(Events [BindingContextChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentBindingContextChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnCausesValidationChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [CausesValidationChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnChangeUICues(UICuesEventArgs e) {
			UICuesEventHandler eh = (UICuesEventHandler)(Events [ChangeUICuesEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnClick(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnContextMenuChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ContextMenuChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnControlAdded(ControlEventArgs e) {
			ControlEventHandler eh = (ControlEventHandler)(Events [ControlAddedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnControlRemoved(ControlEventArgs e) {
			ControlEventHandler eh = (ControlEventHandler)(Events [ControlRemovedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnCreateControl() {
			// Override me!
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnCursorChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [CursorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDockChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DockChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDoubleClick(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragDrop(DragEventArgs drgevent) {
			DragEventHandler eh = (DragEventHandler)(Events [DragDropEvent]);
			if (eh != null)
				eh (this, drgevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragEnter(DragEventArgs drgevent) {
			DragEventHandler eh = (DragEventHandler)(Events [DragEnterEvent]);
			if (eh != null)
				eh (this, drgevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragLeave(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DragLeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDragOver(DragEventArgs drgevent) {
			DragEventHandler eh = (DragEventHandler)(Events [DragOverEvent]);
			if (eh != null)
				eh (this, drgevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnEnabledChanged(EventArgs e) {
			if (IsHandleCreated) {
				if (this is Form) {
					if (((Form)this).context == null) {
						XplatUI.EnableWindow(window.Handle, Enabled);
					}
				} else {
					XplatUI.EnableWindow(window.Handle, Enabled);
				}
				Refresh();
			}

			EventHandler eh = (EventHandler)(Events [EnabledChangedEvent]);
			if (eh != null)
				eh (this, e);

			for (int i=0; i<child_controls.Count; i++) {
				child_controls[i].OnParentEnabledChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnEnter(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [EnterEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnFontChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [FontChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentFontChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnForeColorChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ForeColorChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentForeColorChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnGiveFeedback(GiveFeedbackEventArgs gfbevent) {
			GiveFeedbackEventHandler eh = (GiveFeedbackEventHandler)(Events [GiveFeedbackEvent]);
			if (eh != null)
				eh (this, gfbevent);
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnGotFocus(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [GotFocusEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnHandleCreated(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [HandleCreatedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnHandleDestroyed(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [HandleDestroyedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnHelpRequested(HelpEventArgs hevent) {
			HelpEventHandler eh = (HelpEventHandler)(Events [HelpRequestedEvent]);
			if (eh != null)
				eh (this, hevent);
		}

		protected virtual void OnImeModeChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ImeModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnInvalidated(InvalidateEventArgs e) {
			if (ThemeEngine.Current.DoubleBufferingSupported)
				if ((control_style & ControlStyles.DoubleBuffer) != 0) {
					// should this block be here?  seems like it
					// would be more at home in
					// NotifyInvalidated..
					if (e.InvalidRect == ClientRectangle) {
						InvalidateBackBuffer ();
					}
					else {
						// we need this Inflate call here so
						// that the border of the rectangle is
						// considered Visible (the
						// invalid_region.IsVisible call) in
						// the WM_PAINT handling below.
						Rectangle r = Rectangle.Inflate(e.InvalidRect, 1,1);
						if (invalid_region == null)
							invalid_region = new Region (r);
						else
							invalid_region.Union (r);
					}
				}

			InvalidateEventHandler eh = (InvalidateEventHandler)(Events [InvalidatedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnKeyDown(KeyEventArgs e) {
			KeyEventHandler eh = (KeyEventHandler)(Events [KeyDownEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnKeyPress(KeyPressEventArgs e) {
			KeyPressEventHandler eh = (KeyPressEventHandler)(Events [KeyPressEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnKeyUp(KeyEventArgs e) {
			KeyEventHandler eh = (KeyEventHandler)(Events [KeyUpEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLayout(LayoutEventArgs levent) {
			LayoutEventHandler eh = (LayoutEventHandler)(Events [LayoutEvent]);
			if (eh != null)
				eh (this, levent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLeave(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [LeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLocationChanged(EventArgs e) {
			OnMove(e);
			EventHandler eh = (EventHandler)(Events [LocationChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLostFocus(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [LostFocusEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseDown(MouseEventArgs e) {
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseDownEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseEnter(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MouseEnterEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseHover(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MouseHoverEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseLeave(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MouseLeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseMove(MouseEventArgs e) {
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseMoveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseUp(MouseEventArgs e) {
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseUpEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMouseWheel(MouseEventArgs e) {
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseWheelEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMove(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MoveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnNotifyMessage(Message m) {
			// Override me!
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnPaint(PaintEventArgs e) {
			PaintEventHandler eh = (PaintEventHandler)(Events [PaintEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal virtual void OnPaintBackgroundInternal(PaintEventArgs e) {
			// Override me
		}

		internal virtual void OnPaintInternal(PaintEventArgs e) {
			// Override me
		}

		internal virtual void OnGotFocusInternal (EventArgs e)
		{
			OnGotFocus (e);
		}

		internal virtual void OnLostFocusInternal (EventArgs e)
		{
			OnLostFocus (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnPaintBackground(PaintEventArgs pevent) {
			PaintControlBackground (pevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentBackColorChanged(EventArgs e) {
			if (background_color.IsEmpty && background_image==null) {
				Invalidate();
				OnBackColorChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentBackgroundImageChanged(EventArgs e) {
			if (background_color.IsEmpty && background_image==null) {
				Invalidate();
				OnBackgroundImageChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentBindingContextChanged(EventArgs e) {
			if (binding_context==null) {
				binding_context=Parent.binding_context;
				OnBindingContextChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ParentChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentEnabledChanged(EventArgs e) {
			if (is_enabled) {
				OnEnabledChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentFontChanged(EventArgs e) {
			if (font==null) {
				Invalidate();
				OnFontChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentForeColorChanged(EventArgs e) {
			if (foreground_color.IsEmpty) {
				Invalidate();
				OnForeColorChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentRightToLeftChanged(EventArgs e) {
			if (right_to_left==RightToLeft.Inherit) {
				Invalidate();
				OnRightToLeftChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnParentVisibleChanged(EventArgs e) {
			if (is_visible) {
				OnVisibleChanged(e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e) {
			QueryContinueDragEventHandler eh = (QueryContinueDragEventHandler)(Events [QueryContinueDragEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnResize(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ResizeEvent]);
			if (eh != null)
				eh (this, e);

			PerformLayout(this, "bounds");

			if (parent != null) {
				parent.PerformLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [RightToLeftChangedEvent]);
			if (eh != null)
				eh (this, e);
			for (int i=0; i<child_controls.Count; i++) child_controls[i].OnParentRightToLeftChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnSizeChanged(EventArgs e) {
			DisposeBackBuffer ();
			OnResize(e);
			EventHandler eh = (EventHandler)(Events [SizeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnStyleChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [StyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnSystemColorsChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [SystemColorsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnTabIndexChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [TabIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnTabStopChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [TabStopChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnTextChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [TextChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnValidated(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ValidatedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnValidating(System.ComponentModel.CancelEventArgs e) {
			CancelEventHandler eh = (CancelEventHandler)(Events [ValidatingEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnVisibleChanged(EventArgs e) {
			if ((parent != null) && !Created && Visible) {
				if (!is_disposed) {
					CreateControl();
					PerformLayout();
				}
			}

			EventHandler eh = (EventHandler)(Events [VisibleChangedEvent]);
			if (eh != null)
				eh (this, e);

			// We need to tell our kids
			for (int i=0; i<child_controls.Count; i++) {
				if (child_controls[i].Visible) {
					child_controls[i].OnParentVisibleChanged(e);
				}
			}
		}
		#endregion	// OnXXX methods

		#region Events
		static object BackColorChangedEvent = new object ();
		static object BackgroundImageChangedEvent = new object ();
		static object BindingContextChangedEvent = new object ();
		static object CausesValidationChangedEvent = new object ();
		static object ChangeUICuesEvent = new object ();
		static object ClickEvent = new object ();
		static object ContextMenuChangedEvent = new object ();
		static object ControlAddedEvent = new object ();
		static object ControlRemovedEvent = new object ();
		static object CursorChangedEvent = new object ();
		static object DockChangedEvent = new object ();
		static object DoubleClickEvent = new object ();
		static object DragDropEvent = new object ();
		static object DragEnterEvent = new object ();
		static object DragLeaveEvent = new object ();
		static object DragOverEvent = new object ();
		static object EnabledChangedEvent = new object ();
		static object EnterEvent = new object ();
		static object FontChangedEvent = new object ();
		static object ForeColorChangedEvent = new object ();
		static object GiveFeedbackEvent = new object ();
		static object GotFocusEvent = new object ();
		static object HandleCreatedEvent = new object ();
		static object HandleDestroyedEvent = new object ();
		static object HelpRequestedEvent = new object ();
		static object ImeModeChangedEvent = new object ();
		static object InvalidatedEvent = new object ();
		static object KeyDownEvent = new object ();
		static object KeyPressEvent = new object ();
		static object KeyUpEvent = new object ();
		static object LayoutEvent = new object ();
		static object LeaveEvent = new object ();
		static object LocationChangedEvent = new object ();
		static object LostFocusEvent = new object ();
		static object MouseDownEvent = new object ();
		static object MouseEnterEvent = new object ();
		static object MouseHoverEvent = new object ();
		static object MouseLeaveEvent = new object ();
		static object MouseMoveEvent = new object ();
		static object MouseUpEvent = new object ();
		static object MouseWheelEvent = new object ();
		static object MoveEvent = new object ();
		static object PaintEvent = new object ();
		static object ParentChangedEvent = new object ();
		static object QueryAccessibilityHelpEvent = new object ();
		static object QueryContinueDragEvent = new object ();
		static object ResizeEvent = new object ();
		static object RightToLeftChangedEvent = new object ();
		static object SizeChangedEvent = new object ();
		static object StyleChangedEvent = new object ();
		static object SystemColorsChangedEvent = new object ();
		static object TabIndexChangedEvent = new object ();
		static object TabStopChangedEvent = new object ();
		static object TextChangedEvent = new object ();
		static object ValidatedEvent = new object ();
		static object ValidatingEvent = new object ();
		static object VisibleChangedEvent = new object ();

		public event EventHandler BackColorChanged {
			add { Events.AddHandler (BackColorChangedEvent, value); }
			remove { Events.RemoveHandler (BackColorChangedEvent, value); }
		}

		public event EventHandler BackgroundImageChanged {
			add { Events.AddHandler (BackgroundImageChangedEvent, value); }
			remove { Events.RemoveHandler (BackgroundImageChangedEvent, value); }
		}

		public event EventHandler BindingContextChanged {
			add { Events.AddHandler (BindingContextChangedEvent, value); }
			remove { Events.RemoveHandler (BindingContextChangedEvent, value); }
		}

		public event EventHandler CausesValidationChanged {
			add { Events.AddHandler (CausesValidationChangedEvent, value); }
			remove { Events.RemoveHandler (CausesValidationChangedEvent, value); }
		}

		public event UICuesEventHandler ChangeUICues {
			add { Events.AddHandler (ChangeUICuesEvent, value); }
			remove { Events.RemoveHandler (ChangeUICuesEvent, value); }
		}

		public event EventHandler Click {
			add { Events.AddHandler (ClickEvent, value); }
			remove { Events.RemoveHandler (ClickEvent, value); }
		}

		public event EventHandler ContextMenuChanged {
			add { Events.AddHandler (ContextMenuChangedEvent, value); }
			remove { Events.RemoveHandler (ContextMenuChangedEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event ControlEventHandler ControlAdded {
			add { Events.AddHandler (ControlAddedEvent, value); }
			remove { Events.RemoveHandler (ControlAddedEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event ControlEventHandler ControlRemoved {
			add { Events.AddHandler (ControlRemovedEvent, value); }
			remove { Events.RemoveHandler (ControlRemovedEvent, value); }
		}

		[MWFDescription("Fired when the cursor for the control has been changed"), MWFCategory("PropertyChanged")]
		public event EventHandler CursorChanged {
			add { Events.AddHandler (CursorChangedEvent, value); }
			remove { Events.RemoveHandler (CursorChangedEvent, value); }
		}
		public event EventHandler DockChanged {
			add { Events.AddHandler (DockChangedEvent, value); }
			remove { Events.RemoveHandler (DockChangedEvent, value); }
		}

		public event EventHandler DoubleClick {
			add { Events.AddHandler (DoubleClickEvent, value); }
			remove { Events.RemoveHandler (DoubleClickEvent, value); }
		}

		public event DragEventHandler DragDrop {
			add { Events.AddHandler (DragDropEvent, value); }
			remove { Events.RemoveHandler (DragDropEvent, value); }
		}

		public event DragEventHandler DragEnter {
			add { Events.AddHandler (DragEnterEvent, value); }
			remove { Events.RemoveHandler (DragEnterEvent, value); }
		}

		public event EventHandler DragLeave {
			add { Events.AddHandler (DragLeaveEvent, value); }
			remove { Events.RemoveHandler (DragLeaveEvent, value); }
		}

		public event DragEventHandler DragOver {
			add { Events.AddHandler (DragOverEvent, value); }
			remove { Events.RemoveHandler (DragOverEvent, value); }
		}

		public event EventHandler EnabledChanged {
			add { Events.AddHandler (EnabledChangedEvent, value); }
			remove { Events.RemoveHandler (EnabledChangedEvent, value); }
		}

		public event EventHandler Enter {
			add { Events.AddHandler (EnterEvent, value); }
			remove { Events.RemoveHandler (EnterEvent, value); }
		}

		public event EventHandler FontChanged {
			add { Events.AddHandler (FontChangedEvent, value); }
			remove { Events.RemoveHandler (FontChangedEvent, value); }
		}

		public event EventHandler ForeColorChanged {
			add { Events.AddHandler (ForeColorChangedEvent, value); }
			remove { Events.RemoveHandler (ForeColorChangedEvent, value); }
		}

		public event GiveFeedbackEventHandler GiveFeedback {
			add { Events.AddHandler (GiveFeedbackEvent, value); }
			remove { Events.RemoveHandler (GiveFeedbackEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler GotFocus {
			add { Events.AddHandler (GotFocusEvent, value); }
			remove { Events.RemoveHandler (GotFocusEvent, value); }
		}


		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler HandleCreated {
			add { Events.AddHandler (HandleCreatedEvent, value); }
			remove { Events.RemoveHandler (HandleCreatedEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler HandleDestroyed {
			add { Events.AddHandler (HandleDestroyedEvent, value); }
			remove { Events.RemoveHandler (HandleDestroyedEvent, value); }
		}

		public event HelpEventHandler HelpRequested {
			add { Events.AddHandler (HelpRequestedEvent, value); }
			remove { Events.RemoveHandler (HelpRequestedEvent, value); }
		}

		public event EventHandler ImeModeChanged {
			add { Events.AddHandler (ImeModeChangedEvent, value); }
			remove { Events.RemoveHandler (ImeModeChangedEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event InvalidateEventHandler Invalidated {
			add { Events.AddHandler (InvalidatedEvent, value); }
			remove { Events.RemoveHandler (InvalidatedEvent, value); }
		}

		public event KeyEventHandler KeyDown {
			add { Events.AddHandler (KeyDownEvent, value); }
			remove { Events.RemoveHandler (KeyDownEvent, value); }
		}

		public event KeyPressEventHandler KeyPress {
			add { Events.AddHandler (KeyPressEvent, value); }
			remove { Events.RemoveHandler (KeyPressEvent, value); }
		}

		public event KeyEventHandler KeyUp {
			add { Events.AddHandler (KeyUpEvent, value); }
			remove { Events.RemoveHandler (KeyUpEvent, value); }
		}

		public event LayoutEventHandler Layout {
			add { Events.AddHandler (LayoutEvent, value); }
			remove { Events.RemoveHandler (LayoutEvent, value); }
		}

		public event EventHandler Leave {
			add { Events.AddHandler (LeaveEvent, value); }
			remove { Events.RemoveHandler (LeaveEvent, value); }
		}

		public event EventHandler LocationChanged {
			add { Events.AddHandler (LocationChangedEvent, value); }
			remove { Events.RemoveHandler (LocationChangedEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event EventHandler LostFocus {
			add { Events.AddHandler (LostFocusEvent, value); }
			remove { Events.RemoveHandler (LostFocusEvent, value); }
		}

		public event MouseEventHandler MouseDown {
			add { Events.AddHandler (MouseDownEvent, value); }
			remove { Events.RemoveHandler (MouseDownEvent, value); }
		}

		public event EventHandler MouseEnter {
			add { Events.AddHandler (MouseEnterEvent, value); }
			remove { Events.RemoveHandler (MouseEnterEvent, value); }
		}

		public event EventHandler MouseHover {
			add { Events.AddHandler (MouseHoverEvent, value); }
			remove { Events.RemoveHandler (MouseHoverEvent, value); }
		}

		public event EventHandler MouseLeave {
			add { Events.AddHandler (MouseLeaveEvent, value); }
			remove { Events.RemoveHandler (MouseLeaveEvent, value); }
		}

		public event MouseEventHandler MouseMove {
			add { Events.AddHandler (MouseMoveEvent, value); }
			remove { Events.RemoveHandler (MouseMoveEvent, value); }
		}

		public event MouseEventHandler MouseUp {
			add { Events.AddHandler (MouseUpEvent, value); }
			remove { Events.RemoveHandler (MouseUpEvent, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public event MouseEventHandler MouseWheel {
			add { Events.AddHandler (MouseWheelEvent, value); }
			remove { Events.RemoveHandler (MouseWheelEvent, value); }
		}

		public event EventHandler Move {
			add { Events.AddHandler (MoveEvent, value); }
			remove { Events.RemoveHandler (MoveEvent, value); }
		}

		public event PaintEventHandler Paint {
			add { Events.AddHandler (PaintEvent, value); }
			remove { Events.RemoveHandler (PaintEvent, value); }
		}

		public event EventHandler ParentChanged {
			add { Events.AddHandler (ParentChangedEvent, value); }
			remove { Events.RemoveHandler (ParentChangedEvent, value); }
		}

		public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp {
			add { Events.AddHandler (QueryAccessibilityHelpEvent, value); }
			remove { Events.RemoveHandler (QueryAccessibilityHelpEvent, value); }
		}

		public event QueryContinueDragEventHandler QueryContinueDrag {
			add { Events.AddHandler (QueryContinueDragEvent, value); }
			remove { Events.RemoveHandler (QueryContinueDragEvent, value); }
		}

		public event EventHandler Resize {
			add { Events.AddHandler (ResizeEvent, value); }
			remove { Events.RemoveHandler (ResizeEvent, value); }
		}

		public event EventHandler RightToLeftChanged {
			add { Events.AddHandler (RightToLeftChangedEvent, value); }
			remove { Events.RemoveHandler (RightToLeftChangedEvent, value); }
		}

		public event EventHandler SizeChanged {
			add { Events.AddHandler (SizeChangedEvent, value); }
			remove { Events.RemoveHandler (SizeChangedEvent, value); }
		}

		public event EventHandler StyleChanged {
			add { Events.AddHandler (StyleChangedEvent, value); }
			remove { Events.RemoveHandler (StyleChangedEvent, value); }
		}

		public event EventHandler SystemColorsChanged {
			add { Events.AddHandler (SystemColorsChangedEvent, value); }
			remove { Events.RemoveHandler (SystemColorsChangedEvent, value); }
		}

		public event EventHandler TabIndexChanged {
			add { Events.AddHandler (TabIndexChangedEvent, value); }
			remove { Events.RemoveHandler (TabIndexChangedEvent, value); }
		}

		public event EventHandler TabStopChanged {
			add { Events.AddHandler (TabStopChangedEvent, value); }
			remove { Events.RemoveHandler (TabStopChangedEvent, value); }
		}

		public event EventHandler TextChanged {
			add { Events.AddHandler (TextChangedEvent, value); }
			remove { Events.RemoveHandler (TextChangedEvent, value); }
		}

		public event EventHandler Validated {
			add { Events.AddHandler (ValidatedEvent, value); }
			remove { Events.RemoveHandler (ValidatedEvent, value); }
		}

		public event CancelEventHandler Validating {
			add { Events.AddHandler (ValidatingEvent, value); }
			remove { Events.RemoveHandler (ValidatingEvent, value); }
		}

		public event EventHandler VisibleChanged {
			add { Events.AddHandler (VisibleChangedEvent, value); }
			remove { Events.RemoveHandler (VisibleChangedEvent, value); }
		}

		#endregion	// Events
	}
}
