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
//	Peter Bartok	pbartok@novell.com
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Windows.Forms {
	[DesignerCategory("Form")]
	[DesignTimeVisible(false)]
	[Designer("System.Windows.Forms.Design.FormDocumentDesigner, " + Consts.AssemblySystem_Design, typeof(IRootDesigner))]
	[DefaultEvent("Load")]
	[ToolboxItem(false)]
	public class Form : ContainerControl {
		#region Local Variables
		internal static Form		active_form;
		internal bool			closing;
		FormBorderStyle			form_border_style;
		private bool		        autoscale;
		private Size			clientsize_set;
		private Size		        autoscale_base_size;
		private bool			allow_transparency;
		private static Icon		default_icon;
		internal bool			is_modal;
		internal FormWindowState	window_state;
		private bool			control_box;
		private bool			minimize_box;
		private bool			maximize_box;
		private bool			help_button;
		private bool			show_in_taskbar;
		private bool			topmost;
		private IButtonControl		accept_button;
		private IButtonControl		cancel_button;
		private DialogResult		dialog_result;
		private FormStartPosition	start_position;
		private Form			owner;
		private Form.ControlCollection	owned_forms;
		private MdiClient		mdi_container;
		private InternalWindowManager	window_manager;
		private Form			mdi_parent;
		private bool			key_preview;
		private MainMenu		menu;
		private	Icon			icon;
		private Size			maximum_size;
		private Size			minimum_size;
		private SizeGripStyle		size_grip_style;
		private Rectangle		maximized_bounds;
		private Rectangle		default_maximized_bounds;
		private double			opacity;
		internal ApplicationContext	context;
		Color				transparency_key;
		internal MenuTracker		active_tracker;

		#endregion	// Local Variables

		#region Private & Internal Methods
		static Form ()
		{
			default_icon = (Icon)Locale.GetResource("mono.ico");
		}
		#endregion	// Private & Internal Methods

		#region Public Classes
		public new class ControlCollection : Control.ControlCollection {
			Form	form_owner;

			public ControlCollection(Form owner) : base(owner) {
				this.form_owner = owner;
			}

			public override void Add(Control value) {
				if (Contains (value))
					return;
				AddToList (value);
				((Form)value).owner=(Form)owner;
			}

			public override void Remove(Control value) {
				((Form)value).owner = null;
				base.Remove (value);
			}
		}
		#endregion	// Public Classes

		#region Public Constructor & Destructor
		public Form ()
		{
			SizeF current_scale = GetAutoScaleSize (DeviceContext, Font);

			autoscale = true;
			autoscale_base_size = new Size ((int)current_scale.Width, (int) current_scale.Height);
			allow_transparency = false;
			closing = false;
			is_modal = false;
			dialog_result = DialogResult.None;
			start_position = FormStartPosition.WindowsDefaultLocation;
			form_border_style = FormBorderStyle.Sizable;
			window_state = FormWindowState.Normal;
			key_preview = false;
			opacity = 1D;
			menu = null;
			icon = default_icon;
			minimum_size = Size.Empty;
			maximum_size = Size.Empty;
			clientsize_set = Size.Empty;
			control_box = true;
			minimize_box = true;
			maximize_box = true;
			help_button = false;
			show_in_taskbar = true;
			ime_mode = ImeMode.NoControl;
			is_visible = false;
			is_toplevel = true;
			size_grip_style = SizeGripStyle.Auto;
			maximized_bounds = Rectangle.Empty;
			default_maximized_bounds = Rectangle.Empty;
			owned_forms = new Form.ControlCollection(this);
			transparency_key = Color.Empty;
		}
		#endregion	// Public Constructor & Destructor

		#region Public Static Properties

		public static Form ActiveForm {
			get {
				Control	active;

				active = FromHandle(XplatUI.GetActive());

				if (active != null) {
					if ( !(active is Form)) {
						Control	parent;

						parent = active.Parent;
						while (parent != null) {
							if (parent is Form) {
								return (Form)parent;
							}
							parent = parent.Parent;
						}
					} else {
						return (Form)active;
					}
				}
				return null;
			}
		}

		#endregion	// Public Static Properties

		#region Public Instance Properties
		[DefaultValue(null)]
		public IButtonControl AcceptButton {
			get {
				return accept_button;
			}

			set {
				accept_button = value;
				CheckAcceptButton();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowTransparency {
			get {
				return allow_transparency;
			}

			set {
				if (value == allow_transparency) {
					return;
				}

				if (XplatUI.SupportsTransparency()) {
					allow_transparency = value;

					if (value) {
						if (IsHandleCreated) {
							XplatUI.SetWindowTransparency(Handle, Opacity, TransparencyKey);
						}
					} else {
						UpdateStyles(); // Remove the WS_EX_LAYERED style
					}
				}
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Layout")]
		public bool AutoScale {
			get {
				return autoscale;
			}

			set {
				autoscale = value;
			}
		}

		[Localizable(true)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public virtual Size AutoScaleBaseSize {
			get {
				return autoscale_base_size;
			}

			set {
				autoscale_base_size = value;
			}
		}

		[Localizable(true)]
		public override bool AutoScroll {
			get {
				return base.AutoScroll;
			}
			set {
				base.AutoScroll = value;
			}
		}

		public override Color BackColor {
			get {
				return base.BackColor;
			}
			set {
				base.BackColor = value;
			}
		}

		[DefaultValue(null)]
		public IButtonControl CancelButton {
			get {
				return cancel_button;
			}

			set {
				cancel_button = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Localizable(true)]
		public Size ClientSize {
			get {
				return base.ClientSize;
			}

			set {
				base.ClientSize = value;
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Window Style")]
		public bool ControlBox {
			get {
				return control_box;
			}

			set {
				if (control_box != value) {
					control_box = value;
					UpdateStyles();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle DesktopBounds {
			get {
				return new Rectangle(Location, Size);
			}

			set {
				Bounds = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Point DesktopLocation {
			get {
				return Location;
			}

			set {
				Location = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DialogResult DialogResult {
			get {
				return dialog_result;
			}

			set {
				dialog_result = value;
				if (is_modal) {
					closing = true;
				}
			}
		}

		[DefaultValue(FormBorderStyle.Sizable)]
		[DispId(-504)]
		[MWFCategory("Appearance")]
		public FormBorderStyle FormBorderStyle {
			get {
				return form_border_style;
			}
			set {
				form_border_style = value;

				if (window_manager == null) {
					if (IsHandleCreated) {
						XplatUI.SetBorderStyle(window.Handle, form_border_style);
					}

					if (value == FormBorderStyle.FixedToolWindow ||
							value == FormBorderStyle.SizableToolWindow)
						window_manager = new InternalWindowManager (this);
				} else {
					window_manager.UpdateBorderStyle (value);
				}

				UpdateStyles();
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Window Style")]
		public bool HelpButton {
			get {
				return help_button;
			}

			set {
				if (help_button != value) {
					help_button = value;
					UpdateStyles();
				}
			}
		}

		[Localizable(true)]
		[AmbientValue(null)]
		[MWFCategory("Window Style")]
		public Icon Icon {
			get {
				return icon;
			}

			set {
				if (icon != value) {
					icon = value;

					if (IsHandleCreated) {
						XplatUI.SetIcon(Handle, icon == null ? default_icon : icon);
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsMdiChild {
			get {
				return mdi_parent != null;
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Window Style")]
		public bool IsMdiContainer {
			get {
				return mdi_container != null;
			}

			set {
				if (value && mdi_container == null) {
					mdi_container = new MdiClient (this);
					Controls.Add(mdi_container);
				} else if (!value && mdi_container != null) {
					Controls.Remove(mdi_container);
					mdi_container = null;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form ActiveMdiChild {
			get {
				if (!IsMdiContainer)
					return null;
				return (Form) mdi_container.ActiveMdiChild;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool IsRestrictedWindow {
			get {
				return false;
			}
		}

		[DefaultValue(false)]
		public bool KeyPreview {
			get {
				return key_preview;
			}

			set {
				key_preview = value;
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Window Style")]
		public bool MaximizeBox {
			get {
				return maximize_box;
			}
			set {
				if (maximize_box != value) {
					maximize_box = value;
					if (IsHandleCreated) {
						RecreateHandle();
					}
					UpdateStyles();
				}
			}
		}

		[DefaultValue("{Width=0, Height=0}")]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFCategory("Layout")]
		public Size MaximumSize {
			get {
				return maximum_size;
			}

			set {
				if (maximum_size != value) {
					maximum_size = value;
					OnMaximumSizeChanged(EventArgs.Empty);
					if (IsHandleCreated) {
						XplatUI.SetWindowMinMax(Handle, maximized_bounds, minimum_size, maximum_size);
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form[] MdiChildren {
			get {
				if (mdi_container != null) {
					Form[] form_list;

					form_list = new Form[mdi_container.Controls.Count];
					for (int i = 0; i < mdi_container.Controls.Count; i++) {
						form_list[i] = (Form)mdi_container.Controls[i];
					}
					return form_list;
				} else {
					return new Form[0];
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form MdiParent {
			get {
				return mdi_parent;
			}

			set {
				SuspendLayout ();

				// TopLevel = true;

				if (!value.IsMdiContainer)
					throw new ArgumentException ();

				if (mdi_parent != null) {
					mdi_parent.MdiContainer.Controls.Remove (this);
				}

				mdi_parent = value;
				if (mdi_parent != null) {
					window_manager = new MdiWindowManager (this,
							mdi_parent.MdiContainer);
					mdi_parent.MdiContainer.Controls.Add (this);
					UpdateStyles ();
				}

				ResumeLayout ();
			}
		}

		internal MenuTracker ActiveTracker {
			get { return active_tracker; }
			set {
				if (value == active_tracker)
					return;

				Capture = value != null;
				active_tracker = value;
			}
		}

		internal MdiClient MdiContainer {
			get { return mdi_container; }
		}

		internal InternalWindowManager WindowManager {
			get { return window_manager; }
		}

		[DefaultValue(null)]
		[MWFCategory("Window Style")]
		public MainMenu Menu {
			get {
				return menu;
			}

			set {
				if (menu != value) {
					menu = value;

					if (menu != null && !IsMdiChild) {
						menu.SetForm (this);

						if (IsHandleCreated) {
							XplatUI.SetMenu (window.Handle, menu);
						}

						if (clientsize_set != Size.Empty) {
							SetClientSizeCore(clientsize_set.Width, clientsize_set.Height);
						} else {
							UpdateBounds (bounds.X, bounds.Y, bounds.Width, bounds.Height, ClientSize.Width, ClientSize.Height - 
								ThemeEngine.Current.CalcMenuBarSize (DeviceContext, menu, ClientSize.Width));
						}
					} else
						UpdateBounds ();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public MainMenu MergedMenu {
			get {
				if (!IsMdiChild || window_manager == null)
					return null;
				return ((MdiWindowManager) window_manager).MergedMenu;
			}
		}

		// This is the menu in display and being used because of merging this can
		// be different then the menu that is actually assosciated with the form
		internal MainMenu ActiveMenu {
			get {
				if (IsMdiChild)
					return null;

				if (IsMdiContainer && mdi_container.Controls.Count > 0 &&
						((Form) mdi_container.Controls [0]).WindowState == FormWindowState.Maximized) {
					MdiWindowManager wm = (MdiWindowManager) ((Form) mdi_container.Controls [0]).WindowManager;
					return wm.MaximizedMenu;
				}

				Form amc = ActiveMdiChild;
				if (amc == null || amc.Menu == null)
					return menu;
				return amc.MergedMenu;
			}
		}

		internal MdiWindowManager ActiveMaximizedMdiChild {
			get {
				Form child = ActiveMdiChild;
				if (child == null)
					return null;
				if (child.WindowManager == null || child.window_state != FormWindowState.Maximized)
					return null;
				return (MdiWindowManager) child.WindowManager;
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Window Style")]
		public bool MinimizeBox {
			get {
				return minimize_box;
			}
			set {
				if (minimize_box != value) {
					minimize_box = value;
					if (IsHandleCreated) {
						RecreateHandle();
					}
					UpdateStyles();
				}
			}
		}

		[DefaultValue("{Width=0, Height=0}")]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFCategory("Layout")]
		public Size MinimumSize {
			get {
				return minimum_size;
			}

			set {
				if (minimum_size != value) {
					minimum_size = value;
					OnMinimumSizeChanged(EventArgs.Empty);
					if (IsHandleCreated) {
						XplatUI.SetWindowMinMax(Handle, maximized_bounds, minimum_size, maximum_size);
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Modal  {
			get {
				return is_modal;
			}
		}

		[DefaultValue(1D)]
		[TypeConverter(typeof(OpacityConverter))]
		[MWFCategory("Window Style")]
		public double Opacity {
			get {
				return opacity;
			}

			set {
				opacity = value;

				AllowTransparency = true;

				if (IsHandleCreated) {
					UpdateStyles();
					XplatUI.SetWindowTransparency(Handle, opacity, TransparencyKey);
				}
			}
		}
			

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form[] OwnedForms {
			get {
				Form[] form_list;

				form_list = new Form[owned_forms.Count];

				for (int i=0; i<owned_forms.Count; i++) {
					form_list[i] = (Form)owned_forms[i];
				}

				return form_list;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form Owner {
			get {
				return owner;
			}

			set {
				if (owner != value) {
					if (owner != null) {
						owner.RemoveOwnedForm(this);
					}
					owner = value;
					owner.AddOwnedForm(this);
					if (IsHandleCreated) {
						if (owner != null && owner.IsHandleCreated) {
							XplatUI.SetTopmost(this.window.Handle, owner.window.Handle, true);
						} else {
							XplatUI.SetTopmost(this.window.Handle, IntPtr.Zero, false);
						}
					}
				}
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Window Style")]
		public bool ShowInTaskbar {
			get {
				return show_in_taskbar;
			}
			set {
				if (show_in_taskbar != value) {
					show_in_taskbar = value;
					if (IsHandleCreated) {
						RecreateHandle();
					}
					UpdateStyles();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Localizable(false)]
		public Size Size {
			get {
				return base.Size;
			}

			set {
				base.Size = value;
			}
		}

		[MonoTODO("Trigger something when GripStyle is set")]
		[DefaultValue(SizeGripStyle.Auto)]
		[MWFCategory("Window Style")]
		public SizeGripStyle SizeGripStyle {
			get {
				return size_grip_style;
			}

			set {
				size_grip_style = value;
			}
		}

		[DefaultValue(FormStartPosition.WindowsDefaultLocation)]
		[Localizable(true)]
		[MWFCategory("Layout")]
		public FormStartPosition StartPosition {
			get {
				return start_position;
			}

			set {
				if (start_position == FormStartPosition.WindowsDefaultLocation) {		// Only do this if it's not set yet
					start_position = value;
					if (IsHandleCreated) {
						switch(start_position) {
							case FormStartPosition.CenterParent: {
								CenterToParent();
								break;
							}

							case FormStartPosition.CenterScreen: {
								CenterToScreen();
								break;
							}

							case FormStartPosition.Manual: {
								Left = CreateParams.X;
								Top = CreateParams.Y;
								break;
							}

							default: {
								break;
							}
						}
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int TabIndex {
			get {
				return base.TabIndex;
			}

			set {
				base.TabIndex = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool TopLevel {
			get {
				return GetTopLevel();
			}

			set {
				if (!value && IsMdiContainer)
					throw new ArgumentException ("MDI Container forms must be top level.");
				SetTopLevel(value);
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Window Style")]
		public bool TopMost {
			get {
				return topmost;
			}

			set {
				if (topmost != value) {
					topmost = value;
					if (IsHandleCreated)
						XplatUI.SetTopmost(window.Handle, owner != null ? owner.window.Handle : IntPtr.Zero, value);
				}
			}
		}

		[MWFCategory("Window Style")]
		public Color TransparencyKey {
			get {
				return transparency_key;
			}

			set {
				transparency_key = value;

				AllowTransparency = true;
				UpdateStyles();
				XplatUI.SetWindowTransparency(Handle, Opacity, transparency_key);
			}
		}

		[DefaultValue(FormWindowState.Normal)]
		[MWFCategory("Layout")]
		public FormWindowState WindowState {
			get {
				if (IsHandleCreated) {

					if (window_manager != null)
						return window_manager.GetWindowState ();

					try {
						window_state = XplatUI.GetWindowState(Handle);
					}

					catch(NotSupportedException) {
					}
				}

				return window_state;
			}

			set {
				FormWindowState old_state = window_state;
				window_state = value;
				if (IsHandleCreated) {

					if (window_manager != null) {
						window_manager.SetWindowState (old_state, value);
						return;
					}

					try {
						XplatUI.SetWindowState(Handle, value);
					}

					catch(NotSupportedException) {
					}
				}
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = new CreateParams ();

				cp.Caption = Text;
				cp.ClassName = XplatUI.DefaultClassName;
				cp.ClassStyle = 0;
				cp.Style = 0;
				cp.ExStyle = 0;
				cp.Param = 0;
				cp.Parent = IntPtr.Zero;
				cp.menu = ActiveMenu;

				if (start_position == FormStartPosition.WindowsDefaultLocation && !IsMdiChild) {
					cp.X = unchecked((int)0x80000000);
					cp.Y = unchecked((int)0x80000000);
				} else {
					cp.X = Left;
					cp.Y = Top;
				}
				cp.Width = Width;
				cp.Height = Height;

				cp.Style = (int)(WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS);

				if (IsMdiChild) {
					cp.Style |= (int)(WindowStyles.WS_CHILD | WindowStyles.WS_CAPTION);
					cp.Parent = Parent.Handle;

					cp.ExStyle |= (int) (WindowExStyles.WS_EX_WINDOWEDGE | WindowExStyles.WS_EX_MDICHILD);

					switch (FormBorderStyle) {
					case FormBorderStyle.None:
						break;
					case FormBorderStyle.FixedToolWindow:
					case FormBorderStyle.SizableToolWindow:
						cp.ExStyle |= (int) WindowExStyles.WS_EX_TOOLWINDOW;
						goto default;
					default:
						cp.Style |= (int) WindowStyles.WS_OVERLAPPEDWINDOW;
						break;
					}
					
				} else {
					switch (FormBorderStyle) {
						case FormBorderStyle.Fixed3D: {
							cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
							cp.ExStyle |= (int)WindowExStyles.WS_EX_CLIENTEDGE; 
							break;
						}

						case FormBorderStyle.FixedDialog: {
							cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
							cp.ExStyle |= (int)(WindowExStyles.WS_EX_DLGMODALFRAME);
							break;
						}

						case FormBorderStyle.FixedSingle: {
							cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
							break;
						}

						case FormBorderStyle.FixedToolWindow: { 
							cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
							cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW);
							break;
						}

						case FormBorderStyle.Sizable: {
							cp.Style |= (int)(WindowStyles.WS_BORDER | WindowStyles.WS_THICKFRAME | WindowStyles.WS_CAPTION); 
							break;
						}

						case FormBorderStyle.SizableToolWindow: {
							cp.Style |= (int)(WindowStyles.WS_BORDER | WindowStyles.WS_THICKFRAME | WindowStyles.WS_CAPTION);
							cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW);
							break;
						}

						case FormBorderStyle.None: {
							break;
						}
					}
				}

				switch(window_state) {
					case FormWindowState.Maximized: {
						cp.Style |= (int)WindowStyles.WS_MAXIMIZE;
						break;
					}

					case FormWindowState.Minimized: {
						cp.Style |= (int)WindowStyles.WS_MINIMIZE;
						break;
					}
				}

				if (TopMost) {
					cp.ExStyle |= (int) WindowExStyles.WS_EX_TOPMOST;
				}

				if (ShowInTaskbar) {
					cp.ExStyle |= (int)WindowExStyles.WS_EX_APPWINDOW;
				}

				if (MaximizeBox) {
					cp.Style |= (int)WindowStyles.WS_MAXIMIZEBOX;
				}

				if (MinimizeBox) {
					cp.Style |= (int)WindowStyles.WS_MINIMIZEBOX;
				}

				if (ControlBox) {
					cp.Style |= (int)WindowStyles.WS_SYSMENU;
				}

				if (HelpButton && !MaximizeBox && !MinimizeBox) {
					cp.ExStyle |= (int)WindowExStyles.WS_EX_CONTEXTHELP;
				}
				
				if (Visible)
					cp.Style |= (int)WindowStyles.WS_VISIBLE;

				if (Opacity < 1.0 || TransparencyKey != Color.Empty) {
					cp.ExStyle |= (int)WindowExStyles.WS_EX_LAYERED;
				}

				if (!is_enabled && context == null) {
					cp.Style |= (int)(WindowStyles.WS_DISABLED);
				}

				return cp;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {
				return ImeMode.NoControl;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size (300, 300);
			}
		}

		protected Rectangle MaximizedBounds {
			get {
				if (maximized_bounds != Rectangle.Empty) {
					return maximized_bounds;
				}
				return default_maximized_bounds;
			}

			set {
				maximized_bounds = value;
				OnMaximizedBoundsChanged(EventArgs.Empty);
				if (IsHandleCreated) {
					XplatUI.SetWindowMinMax(Handle, maximized_bounds, minimum_size, maximum_size);
				}
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Static Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static SizeF GetAutoScaleSize (Font font)
		{
			return XplatUI.GetAutoScaleSize(font);
		}

		#endregion	// Public Static Methods

		#region Public Instance Methods
		internal SizeF GetAutoScaleSize (Graphics g, Font font)
		{
			//
			// The following constants come from the dotnet mailing list
			// discussion: http://discuss.develop.com/archives/wa.exe?A2=ind0203A&L=DOTNET&P=R3655
			//
			// The magic number is "Its almost the length
			// of the string with a smattering added in
			// for compat with earlier code".
			//
	
			string magic_string = "The quick brown fox jumped over the lazy dog.";
			double magic_number = 44.549996948242189;
			float width = (float) (g.MeasureString (magic_string, font).Width / magic_number);
			
			return new SizeF (width, font.Height);
		}
						 
		public void Activate() {
			Form	active;

			// The docs say activate only activates if our app is already active
			active = ActiveForm;
			if ((active != null) && (this != active)) {
				XplatUI.Activate(window.Handle);
			}
		}

		public void AddOwnedForm(Form ownedForm) {
			owned_forms.Add(ownedForm);
		}

		public void Close () {
			if (!IsDisposed) {
				XplatUI.SendMessage(this.Handle, Msg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
			}
		}

		public void LayoutMdi(MdiLayout value) {
			if (mdi_container != null) {
				mdi_container.LayoutMdi(value);
			}
		}

		public void RemoveOwnedForm(Form ownedForm) {
			owned_forms.Remove(ownedForm);
		}

		public void SetDesktopBounds(int x, int y, int width, int height) {
			DesktopBounds = new Rectangle(x, y, width, height);
		}

		public void SetDesktopLocation(int x, int y) {
			DesktopLocation = new Point(x, y);
		}

		public DialogResult ShowDialog() {
			return ShowDialog(null);
		}

		public DialogResult ShowDialog(IWin32Window ownerWin32) {
			Form		previous;

			owner = null;

			if (ownerWin32 != null) {
				Control c = Control.FromHandle (ownerWin32.Handle);
				if (c != null)
					owner = c.TopLevelControl as Form;
			}

			if (owner == this) {
				throw new InvalidOperationException("The 'ownerWin32' cannot be the form being shown.");
			}

			if (is_modal) {
				throw new InvalidOperationException("The form is already displayed as a modal dialog.");
			}

			if (Visible) {
				throw new InvalidOperationException("Already visible forms cannot be displayed as a modal dialog. Set the Visible property to 'false' prior to calling Form.ShowDialog.");
			}

			if (!Enabled) {
				throw new InvalidOperationException("Cannot display a disabled form as modal dialog.");
			}

			if (TopLevelControl != this) {
				throw new InvalidOperationException("Can only display TopLevel forms as modal dialog.");
			}

			#if broken
			// Can't do this, will screw us in the modal loop
			form_parent_window.Parent = this.owner;
			#endif

			previous = Form.ActiveForm;

			if (!IsHandleCreated) {
				CreateControl();
			}

			Application.RunLoop(true, new ApplicationContext(this));

			if (previous != null) {
				// Cannot use Activate(), it has a check for the current active window...
				XplatUI.Activate(previous.window.Handle);
			}

			return DialogResult;
		}

		public override string ToString() {
			return GetType().FullName.ToString() + ", Text: " + Text;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected void ActivateMdiChild(Form form) {
			if (!IsMdiContainer)
				return;
			mdi_container.ActivateChild (form);
			OnMdiChildActivate(EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void AdjustFormScrollbars(bool displayScrollbars) {
			base.AdjustFormScrollbars (displayScrollbars);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void ApplyAutoScaling()
		{
			SizeF current_size_f = GetAutoScaleSize (DeviceContext, Font);
			Size current_size = new Size ((int) current_size_f.Width, (int) current_size_f.Height);
			float	dx;
			float	dy;

			if (current_size == autoscale_base_size)
				return;

			if (Environment.GetEnvironmentVariable ("MONO_MWF_SCALING") == "disable"){
				Console.WriteLine ("Not scaling");
				return;
			}
			
			//
			// I tried applying the Fudge height factor from:
			// http://blogs.msdn.com/mharsh/archive/2004/01/25/62621.aspx
			// but it makes things larger without looking better.
			//
			if (current_size_f.Width != AutoScaleBaseSize.Width) {
				dx = current_size_f.Width / AutoScaleBaseSize.Width + 0.08f;
			} else {
				dx = 1;
			}

			if (current_size_f.Height != AutoScaleBaseSize.Height) {
				dy = current_size_f.Height / AutoScaleBaseSize.Height + 0.08f;
			} else {
				dy = 1;
			}

			Scale (dx, dy);
			
			AutoScaleBaseSize = current_size;
		}

		protected void CenterToParent() {
			Control	ctl;
			int	w;
			int	h;

			if (Width > 0) {
				w = Width;
			} else {
				w = DefaultSize.Width;
			}

			if (Height > 0) {
				h = Height;
			} else {
				h = DefaultSize.Height;
			}

			ctl = null;
			if (parent != null) {
				ctl = parent;
			} else if (owner != null) {
				ctl = owner;
			}

			if (owner != null) {
				this.Location = new Point(ctl.Left + ctl.Width / 2 - w /2, ctl.Top + ctl.Height / 2 - h / 2);
			}
		}

		protected void CenterToScreen() {
			Size	DisplaySize;
			int	w;
			int	h;

			if (Width > 0) {
				w = Width;
			} else {
				w = DefaultSize.Width;
			}

			if (Height > 0) {
				h = Height;
			} else {
				h = DefaultSize.Height;
			}

			XplatUI.GetDisplaySize(out DisplaySize);
			this.Location = new Point(DisplaySize.Width / 2 - w / 2, DisplaySize.Height / 2 - h / 2);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override Control.ControlCollection CreateControlsInstance() {
			return base.CreateControlsInstance ();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void CreateHandle() {
			base.CreateHandle ();

			if (XplatUI.SupportsTransparency()) {
				if (allow_transparency) {
					XplatUI.SetWindowTransparency(Handle, Opacity, TransparencyKey);
				}
			}

			XplatUI.SetWindowMinMax(window.Handle, maximized_bounds, minimum_size, maximum_size);
			if (icon != null) {
				XplatUI.SetIcon(window.Handle, icon);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void DefWndProc(ref Message m) {
			base.DefWndProc (ref m);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose (disposing);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnActivated(EventArgs e) {
			if (Activated != null) {
				Activated(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnClosed(EventArgs e) {
			if (Closed != null) {
				Closed(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnClosing(System.ComponentModel.CancelEventArgs e) {
			if (Closing != null) {
				Closing(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnCreateControl() {
			base.OnCreateControl ();
			if (this.ActiveControl == null) {
				bool visible;

				// This visible hack is to work around CanSelect always being false if one of the parents
				// is not visible; and we by default create Form invisible...
				visible = this.is_visible;
				this.is_visible = true;

				if (SelectNextControl(this, true, true, true, true) == false) {
					Select(this);
				}

				this.is_visible = visible;
			} else {
				Select(ActiveControl);
			}

			if (menu != null) {
				XplatUI.SetMenu(window.Handle, menu);
			}

			OnLoad(EventArgs.Empty);

			// Send initial location
			OnLocationChanged(EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDeactivate(EventArgs e) {
			if (Deactivate != null) {
				Deactivate(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnHandleCreated(EventArgs e) {
			XplatUI.SetBorderStyle(window.Handle, form_border_style);
			base.OnHandleCreated (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnHandleDestroyed(EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnInputLanguageChanged(InputLanguageChangedEventArgs e) {
			if (InputLanguageChanged!=null) {
				InputLanguageChanged(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnInputLanguageChanging(InputLanguageChangingEventArgs e) {
			if (InputLanguageChanging!=null) {
				InputLanguageChanging(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLoad(EventArgs e) {
			if (AutoScale){
				ApplyAutoScaling ();
				AutoScale = false;
			}

			if (Load != null) {
				Load(this, e);
			}

			if (!IsMdiChild) {
				switch (StartPosition) {
					case FormStartPosition.CenterScreen:
						this.CenterToScreen();
						break;
					case FormStartPosition.CenterParent:
						this.CenterToParent ();
						break;
					case FormStartPosition.Manual: 
						Left = CreateParams.X;
						Top = CreateParams.Y;
						break;
				}
			} else {
				MdiParent.MdiContainer.LayoutMdi (MdiLayout.Cascade);
				
			}

		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMaximizedBoundsChanged(EventArgs e) {
			if (MaximizedBoundsChanged != null) {
				MaximizedBoundsChanged(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMaximumSizeChanged(EventArgs e) {
			if (MaximumSizeChanged != null) {
				MaximumSizeChanged(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMdiChildActivate(EventArgs e) {
			if (MdiChildActivate != null) {
				MdiChildActivate(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMenuComplete(EventArgs e) {
			if (MenuComplete != null) {
				MenuComplete(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMenuStart(EventArgs e) {
			if (MenuStart != null) {
				MenuStart(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMinimumSizeChanged(EventArgs e) {
			if (MinimumSizeChanged != null) {
				MinimumSizeChanged(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnPaint (PaintEventArgs pevent) {
			base.OnPaint (pevent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnStyleChanged(EventArgs e) {
			base.OnStyleChanged (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnTextChanged(EventArgs e) {
			base.OnTextChanged (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged (e);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if (base.ProcessCmdKey (ref msg, keyData)) {
				return true;
			}

			// Give our menu a shot
			if (ActiveMenu != null) {
				return ActiveMenu.ProcessCmdKey(ref msg, keyData);
			}

			return false;
		}

		// LAMESPEC - Not documented that Form overrides ProcessDialogChar; class-status showed
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override bool ProcessDialogChar(char charCode) {
			return base.ProcessDialogChar (charCode);
		}

		protected override bool ProcessDialogKey(Keys keyData) {
			if ((keyData & Keys.Modifiers) == 0) {
				if (keyData == Keys.Enter && accept_button != null) {
					accept_button.PerformClick();
					return true;
				} else if (keyData == Keys.Escape && cancel_button != null) {
					cancel_button.PerformClick();
					return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override bool ProcessKeyPreview(ref Message msg) {
			if (key_preview) {
				if (ProcessKeyEventArgs(ref msg)) {
					return true;
				}
			}
			return base.ProcessKeyPreview (ref msg);
		}

		protected override bool ProcessTabKey(bool forward) {
			return SelectNextControl(ActiveControl, forward, true, true, true);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void ScaleCore(float dx, float dy) {
			// We can't scale max or min windows
			if (WindowState == FormWindowState.Normal) {
				// We cannot call base since base also adjusts X/Y, but
				// a form is toplevel and doesn't move
				Size	size;

				SuspendLayout();

				size = ClientSize;
				if (!GetStyle(ControlStyles.FixedWidth)) {
					size.Width = (int)(size.Width * dx);
				}

				if (!GetStyle(ControlStyles.FixedHeight)) {
					size.Height = (int)(size.Height * dy);
				}

				ClientSize = size;

				/* Now scale our children */
				Control [] controls = child_controls.GetAllControls ();
				for (int i=0; i < controls.Length; i++) {
					controls[i].Scale(dx, dy);
				}
				
				ResumeLayout();
			}
		}

		protected override void Select(bool directed, bool forward) {
			Form	parent;

			if (directed) {
				base.SelectNextControl(null, forward, true, true, true);
			}

			parent = this.ParentForm;
			if (parent != null) {
				parent.ActiveControl = this;
			}

			Activate();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			base.SetBoundsCore (x, y, width, height, specified);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void SetClientSizeCore(int x, int y) {
			if ((minimum_size.Width != 0) && (x < minimum_size.Width)) {
				x = minimum_size.Width;
			} else if ((maximum_size.Width != 0) && (x > maximum_size.Width)) {
				x = maximum_size.Width;
			}

			if ((minimum_size.Height != 0) && (y < minimum_size.Height)) {
				y = minimum_size.Height;
			} else if ((maximum_size.Height != 0) && (y > maximum_size.Height)) {
				y = maximum_size.Height;
			}

			Rectangle ClientRect = new Rectangle(0, 0, x, y);
			Rectangle WindowRect;
			CreateParams cp = this.CreateParams;

			clientsize_set = new Size(x, y);

			if (!IsMdiChild) {
				if (XplatUI.CalculateWindowRect(ref ClientRect, cp.Style, cp.ExStyle, cp.menu, out WindowRect)) {
					SetBoundsCore(bounds.X, bounds.Y, WindowRect.Width, WindowRect.Height, BoundsSpecified.Size);
				}
			} else {
				if (XplatUI.CalculateWindowRect(ref ClientRect, cp.Style, cp.ExStyle, cp.menu, out WindowRect)) {
					SetBoundsCore(bounds.X, bounds.Y, WindowRect.Width, WindowRect.Height, BoundsSpecified.Size);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void SetVisibleCore(bool value) {
			base.SetVisibleCore (value);
		}

		protected override void UpdateDefaultButton() {
			base.UpdateDefaultButton ();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void WndProc(ref Message m) {

			if (window_manager != null && window_manager.HandleMessage (ref m)) {
				return;
			}

			bool native_enabled = XplatUI.IsEnabled (Handle);

			switch((Msg)m.Msg) {
				case Msg.WM_DESTROY: {
					base.WndProc(ref m);
					if (!is_recreating) {
						this.closing = true;
					}
					return;
				}

				case Msg.WM_CLOSE_INTERNAL: {
					DestroyHandle();
					break;
				}

				case Msg.WM_CLOSE: {
					if (!is_modal) {
						CancelEventArgs args = new CancelEventArgs ();

						OnClosing (args);
						if (!args.Cancel) {
							OnClosed (EventArgs.Empty);
							base.Dispose();
						}
						return;
					} else {
						closing = true;
					}
					return;
				}

				case Msg.WM_WINDOWPOSCHANGED: {
					if (WindowState != FormWindowState.Minimized) {
						base.WndProc(ref m);
					}
					return;
				}

				case Msg.WM_ACTIVATE: {
					if (m.WParam != (IntPtr)WindowActiveFlags.WA_INACTIVE) {
						OnActivated(EventArgs.Empty);
					} else {
						OnDeactivate(EventArgs.Empty);
					}
					return;
				}

				case Msg.WM_KILLFOCUS: {
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_SETFOCUS: {
					if (ActiveControl != null && ActiveControl != this) {
						ActiveControl.Focus();
						return;	// FIXME - do we need to run base.WndProc, even though we just changed focus?
					}
					base.WndProc(ref m);
					return;
				}

				// Menu drawing
				case Msg.WM_NCLBUTTONDOWN: {
					if (native_enabled && ActiveMenu != null) {
						ActiveMenu.OnMouseDown(this, new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 0));
					}

					if (ActiveMaximizedMdiChild != null) {
						ActiveMaximizedMdiChild.HandleMenuMouseDown (ActiveMenu,
								LowOrder ((int) m.LParam.ToInt32 ()),
								HighOrder ((int) m.LParam.ToInt32 ()));
					}
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_NCMOUSEMOVE: {
					if (native_enabled && ActiveMenu != null) {
						ActiveMenu.OnMouseMove(this, new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 0));
					}
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_NCPAINT: {
					if (ActiveMenu != null) {
						PaintEventArgs	pe;
						Point		pnt;

						pe = XplatUI.PaintEventStart(Handle, false);
						pnt = XplatUI.GetMenuOrigin(window.Handle);

						ActiveMenu.Draw (pe, new Rectangle (pnt.X, pnt.Y, ClientSize.Width, 0));

						if (ActiveMaximizedMdiChild != null) {
							ActiveMaximizedMdiChild.DrawMaximizedButtons (pe, ActiveMenu);
						}

						XplatUI.PaintEventEnd(Handle, false);
					}

					base.WndProc(ref m);
					return;
				}

				case Msg.WM_NCCALCSIZE: {
					XplatUIWin32.NCCALCSIZE_PARAMS	ncp;

					if ((ActiveMenu != null) && (m.WParam == (IntPtr)1)) {
						ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(XplatUIWin32.NCCALCSIZE_PARAMS));

						// Adjust for menu
						ncp.rgrc1.top += ThemeEngine.Current.CalcMenuBarSize (DeviceContext, ActiveMenu, ncp.rgrc1.right - ncp.rgrc1.left);
						Marshal.StructureToPtr(ncp, m.LParam, true);
					}
					DefWndProc(ref m);
					break;
				}

				case Msg.WM_MOUSEMOVE: {
					if (native_enabled && active_tracker != null) {
						MouseEventArgs args;

						args = new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
							mouse_clicks,  LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()),  0);
						active_tracker.OnMotion(new MouseEventArgs (args.Button, args.Clicks, Control.MousePosition.X, Control.MousePosition.Y, args.Delta));
						break;
					}
					base.WndProc(ref m);
					break;
				}

				case Msg.WM_LBUTTONDOWN:
				case Msg.WM_MBUTTONDOWN:
				case Msg.WM_RBUTTONDOWN: {					
					if (native_enabled && active_tracker != null) {
						MouseEventArgs args;

						args = new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
							mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 0);
						active_tracker.OnClick(new MouseEventArgs (args.Button, args.Clicks, Control.MousePosition.X, Control.MousePosition.Y, args.Delta));
						return;
					}
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_LBUTTONUP:
				case Msg.WM_MBUTTONUP:
				case Msg.WM_RBUTTONUP: {
					if (native_enabled && active_tracker != null) {
						MouseEventArgs args;

						args = new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
							mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 0);
						active_tracker.OnMouseUp(new MouseEventArgs (args.Button, args.Clicks, Control.MousePosition.X, Control.MousePosition.Y, args.Delta));
						mouse_clicks = 1;
						return;
					}
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_GETMINMAXINFO: {
					MINMAXINFO	mmi;

					if (m.LParam != IntPtr.Zero) {
						mmi = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));

						default_maximized_bounds = new Rectangle(mmi.ptMaxPosition.x, mmi.ptMaxPosition.y, mmi.ptMaxSize.x, mmi.ptMaxSize.y);
						if (maximized_bounds != Rectangle.Empty) {
							mmi.ptMaxPosition.x = maximized_bounds.Left;
							mmi.ptMaxPosition.y = maximized_bounds.Top;
							mmi.ptMaxSize.x = maximized_bounds.Width;
							mmi.ptMaxSize.y = maximized_bounds.Height;
						}

						if (minimum_size != Size.Empty) {
							mmi.ptMinTrackSize.x = minimum_size.Width;
							mmi.ptMinTrackSize.y = minimum_size.Height;
						}

						if (minimum_size != Size.Empty) {
							mmi.ptMaxTrackSize.x = maximum_size.Width;
							mmi.ptMaxTrackSize.y = maximum_size.Height;
						}
						Marshal.StructureToPtr(mmi, m.LParam, false);
					}
					break;
				}

				default: {
					base.WndProc (ref m);
					break;
				}
			}
		}
		#endregion	// Protected Instance Methods

		internal void RemoveWindowManager ()
		{
			window_manager = null;
		}
		
		internal override void CheckAcceptButton()
		{
			if (accept_button != null) {
				Button a_button = accept_button as Button;
				
				if (ActiveControl == a_button)
					return;
				
				if (ActiveControl is Button) {
					a_button.paint_as_acceptbutton = false;
					a_button.Redraw();
					return;
				} else {
					a_button.paint_as_acceptbutton = true;
					a_button.Redraw();
				}
			}
		}
		
		#region Events
		public event EventHandler Activated;
		public event EventHandler Closed;
		public event CancelEventHandler Closing;
		public event EventHandler Deactivate;
		public event InputLanguageChangedEventHandler InputLanguageChanged;
		public event InputLanguageChangingEventHandler InputLanguageChanging;
		public event EventHandler Load;
		public event EventHandler MaximizedBoundsChanged;
		public event EventHandler MaximumSizeChanged;
		public event EventHandler MdiChildActivate;
		public event EventHandler MenuComplete;
		public event EventHandler MenuStart;
		public event EventHandler MinimumSizeChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}
		#endregion	// Events
	}
}
