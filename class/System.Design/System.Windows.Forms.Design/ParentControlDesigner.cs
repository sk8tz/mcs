//
// System.Windows.Forms.Design.ParentControlDesigner
//
// Authors:
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006 Ivan N. Zlatev

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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Collections;
#if NET_2_0
using System.Windows.Forms.Design.Behavior;
#endif

namespace System.Windows.Forms.Design

{

	public class ParentControlDesigner : ControlDesigner
	{

		public ParentControlDesigner ()
		{
		}


#region Initialization
		// Settings paths taken from the example at:
		// http://msdn2.microsoft.com/en-us/library/system.componentmodel.design.idesigneroptionservice.aspx
		//
		public override void Initialize (IComponent component)
		{
			base.Initialize (component);
			
			this.Control.AllowDrop = true;

			// Initialize the default values of the Design-Time properties.
			//
			_defaultDrawGrid = true;
			_defaultSnapToGrid = true;
			_defaultGridSize = new Size (8, 8);

			// If the parent Control of the designed one has a ParentDesigner then inherit the values
			// from it's designer.
			//
			if (this.Control.Parent != null) {
				ParentControlDesigner parentDesigner = GetParentControlDesignerOf (Control.Parent);
				if (parentDesigner != null) {
					_defaultDrawGrid = (bool) GetValue (parentDesigner.Component, "DrawGrid");
					_defaultSnapToGrid = (bool) GetValue (parentDesigner.Component, "SnapToGrid");
					_defaultGridSize = (Size) GetValue (parentDesigner.Component, "GridSize");
				}
			}
			else {
				// Else retrieve them through the IDesignerOptionService (if available)
				//
				IDesignerOptionService options = GetService (typeof (IDesignerOptionService)) as
																  IDesignerOptionService;
				if (options != null) {
					object value = null;
					value = options.GetOptionValue (@"WindowsFormsDesigner\General", "DrawGrid");
					if (value is bool)
						_defaultDrawGrid = (bool) value;

					value = options.GetOptionValue (@"WindowsFormsDesigner\General", "SnapToGrid");
					if (value is bool)
						_defaultSnapToGrid = (bool) value;

					value = options.GetOptionValue (@"WindowsFormsDesigner\General", "GridSize");
					if (value is Size)
						_defaultGridSize = (Size) value;
				}
			}

			// At the end set whatever we've managed to get
			//
			_drawGrid = _defaultDrawGrid;
			_snapToGrid = _defaultSnapToGrid;
			_gridSize = _defaultGridSize;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				EnableDragDrop (false);
				OnMouseDragEnd (true);
			}
			base.Dispose (disposing);
		}
#endregion


#region IToolboxService Related

		// This is the code that is executed when you drop a tool from the Toolbox in the designer.
		//

		protected static void InvokeCreateTool (ParentControlDesigner toInvoke, ToolboxItem tool)
		{
			if (toInvoke != null)
				toInvoke.CreateTool (tool);
		}

		protected void CreateTool (ToolboxItem tool)
		{
			CreateToolCore (tool, DefaultControlLocation.X, DefaultControlLocation.Y, 0, 0, true, false);
		}

		protected void CreateTool (ToolboxItem tool, Point location)
		{
			CreateToolCore (tool, location.X, location.Y, 0, 0, true, false);
		}

		protected void CreateTool (ToolboxItem tool, Rectangle bounds)
		{
			CreateToolCore (tool, bounds.X, bounds.Y, bounds.Width, bounds.Width, true, true);
		}

		// Creates a component from a ToolboxItem, sets its location and size if available and snaps it's
		// location to the grid.
		//
		protected virtual IComponent[] CreateToolCore (ToolboxItem tool, int x, int y, int width,  int height,
								bool hasLocation, bool hasSize)
		{
			if (tool == null)
				throw new ArgumentNullException ("tool");

			IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
			DesignerTransaction transaction = host.CreateTransaction ("Create components in tool '" + tool.DisplayName + "'");
			IComponent[] components = tool.CreateComponents (host);

			foreach (IComponent component in components)
			{
				Control control = component as Control;
				if (control != null) {
					if (hasLocation)
						base.SetValue (component, "Location", this.SnapPointToGrid (new Point (x, y)));
					else
						base.SetValue (component, "Location", this.SnapPointToGrid (this.DefaultControlLocation));

					if (hasSize)
						base.SetValue (component, "Size", new Size (width, height));

					this.Control.SuspendLayout ();
					this.Control.Controls.Add (control);
					this.Control.SuspendLayout ();
					this.Control.Refresh ();
				}
			}
			ISelectionService selectionServ = this.GetService (typeof (ISelectionService)) as ISelectionService;
			if (selectionServ != null)
				selectionServ.SetSelectedComponents (components, SelectionTypes.Replace);
			transaction.Commit ();
			return components;
		}

#endregion


#region Drag and Drop

		// If the control is not already parented return true
		//
		public virtual bool CanParent (Control control)
		{
			if (control != null)
				return !control.Contains (this.Control);

			return false;
		}

		public virtual bool CanParent (ControlDesigner designer)
		{
			return CanParent (designer.Control);
		}

		protected override void OnDragDrop (DragEventArgs e)
		{
			IUISelectionService selectionServ = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (selectionServ != null) {
				// once this is fired the parent control (parentcontroldesigner) will start getting dragover events.
				//
				Point location = this.SnapPointToGrid (this.Control.PointToClient (new Point (e.X, e.Y)));
				selectionServ.DragDrop (false, this.Control, location.X, location.Y);
			}
		}

		protected override void OnDragEnter (DragEventArgs e)
		{

			this.Control.Refresh ();
		}

		protected override void OnDragLeave (EventArgs e)
		{
			this.Control.Refresh ();
		}

		protected override void OnDragOver (DragEventArgs e)
		{
			IUISelectionService selectionServ = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (selectionServ != null) {
				// once ControlDesigner.MouseDragBegin is fired this will start getting dragover events.
				//
				Point location = this.SnapPointToGrid (this.Control.PointToClient (new Point (e.X, e.Y)));
				selectionServ.DragOver (this.Control, location.X, location.Y);
			}
			e.Effect = DragDropEffects.Move;
		}
#endregion


#region Properties
		// The default location where a control is placed, when added to the designer
		//
		protected virtual Point DefaultControlLocation {
			get { return new Point (0, 0); }
		}


		protected override bool EnableDragRect {
			get { return true; }
		}
#endregion


#region Design-Time Properties

		private bool _defaultDrawGrid;
		private bool _defaultSnapToGrid;
		private Size _defaultGridSize;
		private bool _drawGrid;
		private bool _snapToGrid;
		private Size _gridSize;

		//This method adds the following design-time browsable properties:
		// "DrawGrid", "SnapToGrid", and "GridSize".
		//
		protected override void PreFilterProperties (IDictionary properties)
		{
			base.PreFilterProperties (properties);

			properties["DrawGrid"] = TypeDescriptor.CreateProperty (typeof (ParentControlDesigner),
							"DrawGrid",
							typeof (bool),
							new Attribute[] {
								BrowsableAttribute.Yes,
								DesignOnlyAttribute.Yes,
								new DescriptionAttribute (
									"Indicates whether or not to draw the positioning grid."),
								CategoryAttribute.Design
							});

			properties["SnapToGrid"] = TypeDescriptor.CreateProperty (typeof (ParentControlDesigner),
							"SnapToGrid",
							typeof (bool),
							new Attribute[] {
								BrowsableAttribute.Yes,
								DesignOnlyAttribute.Yes,
								new DescriptionAttribute (
									"Determines if controls should snap to the positioning grid."),
								CategoryAttribute.Design
							});

			properties["GridSize"] = TypeDescriptor.CreateProperty (typeof (ParentControlDesigner),
							"GridSize",
							typeof (Size),
							new Attribute[] {
								BrowsableAttribute.Yes,
								DesignOnlyAttribute.Yes,
								new DescriptionAttribute (
									"Determines the size of the positioning grid."),
								CategoryAttribute.Design
							});

		}

		
		// Informs all children controls' ParentControlDesigners that the grid properties
		// have changed and passes them
		//
		private void PopulateGridProperties ()
		{
			// Control.Invalidate (true) will redraw the control and it's children
			// this will cause a WM_PAINT message to be send and the ControlDesigenr will raise
			// the OnPaintAdornments, where the grid drawing takes place.
			//
			// Note that this should be called *after* the grid properties have changed :-)
			//
			this.Control.Invalidate (false);

			if (this.Control != null) {
				ParentControlDesigner designer = null;
				foreach (Control control in this.Control.Controls) {
					designer = this.GetParentControlDesignerOf (control);
					if (designer != null)
						designer.OnParentGridPropertiesChanged (this);
				}
			}
		}

		// Called by the parent ParentControlDesigner when it is populating the grid-related
		// design-time properties changes
		//
		private void OnParentGridPropertiesChanged (ParentControlDesigner parentDesigner)
		{
			SetValue (this.Component, "DrawGrid", (bool) GetValue (parentDesigner.Component, "DrawGrid"));
			SetValue (this.Component, "SnapToGrid", (bool) GetValue (parentDesigner.Component, "SnapToGrid"));
			SetValue (this.Component, "GridSize", (Size) GetValue (parentDesigner.Component, "GridSize"));

			// Set also the default values to be those, because we should
			// match the parent ParentControlDesigner values.
			// called recursivly, so I will rather go for slower, but no stack-overflowable code
			// 
			_defaultDrawGrid = (bool) GetValue (parentDesigner.Component, "DrawGrid");
			_defaultSnapToGrid = (bool) GetValue (parentDesigner.Component, "SnapToGrid");
			_defaultGridSize = (Size) GetValue (parentDesigner.Component, "GridSize");

			this.PopulateGridProperties ();
		}


		// Retrieves the ParentControlDesigner of the specified control if available,
		// else returns null.
		//
		private ParentControlDesigner GetParentControlDesignerOf (Control control)
		{
			if (control != null) {
				IDesignerHost designerHost = GetService (typeof (IDesignerHost)) as IDesignerHost;
				if (designerHost != null) {
					ParentControlDesigner designer = null;
					designer = designerHost.GetDesigner (this.Control.Parent) as ParentControlDesigner;
					if (designer != null)
						return designer;
				}
			}
			return null;
		}

		protected virtual bool DrawGrid {
			get { return _drawGrid; }
			set {
				_drawGrid = value;

				if (value == false)
					SetValue (this.Component, "SnapToGrid", false);

				PopulateGridProperties ();
			}
		}

		private bool SnapToGrid {
			get { return _snapToGrid; }
			set {
				_snapToGrid = value;
				PopulateGridProperties ();
			}
		}

		protected Size GridSize {
			get { return _gridSize; }
			set {
				_gridSize = value;
				PopulateGridProperties ();
			}
		}

		// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpconshouldpersistresetmethods.asp
		//
		// The ShouldSerializerPROPERTYNAME determines whether a property has changed from
		// the default value and should get serialized.
		//
		// The ResetPROPERTYNAME resets the property to it's default value (used when
		// one right clicks on a property in the property grid and clicks on "Reset".
		//

		private bool ShouldSerializeDrawGrid ()
		{
			return DrawGrid != _defaultDrawGrid;
		}

		private void ResetDrawGrid ()
		{
			this.DrawGrid = _defaultDrawGrid;
		}

		private bool ShouldSerializeSnapToGrid  ()
		{
			return _drawGrid != _defaultDrawGrid;
		}

		private void ResetSnapToGrid ()
		{
			this.SnapToGrid = _defaultSnapToGrid;
		}

		private bool ShouldSerializeGridSize ()
		{
			return GridSize != _defaultGridSize;
		}

		private void ResetGridSize ()
		{
			this.GridSize = _defaultGridSize;
		}
#endregion


#region Design-Time Mouse Drag and Drop

		protected override void OnMouseDragBegin (int x, int y)
		{
			// do not call base here because the behaviour is specific for the ControlDesgner (does IUISelectionService.DragBegin)
			//

			IUISelectionService selectionServ = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (selectionServ != null) {
				// once ControlDesigner.MouseDragBegin is fired this will start getting dragover events.
				//
				Point location = new Point (x, y);
				IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
				if (base.MouseButtonDown == MouseButtons.Middle && host != null && host.RootComponent != this.Control) {
					location = this.Control.Parent.PointToClient (this.Control.PointToScreen (new Point (x, y)));
					// I have to do this, because I get DragOver events fired for the control I am actually dragging
					// 
					this.Control.AllowDrop = false;
					selectionServ.DragBegin ();
				}
				else {
					selectionServ.MouseDragBegin (this.Control, location.X, location.Y);
				}
			}
		}

		protected override void OnMouseDragMove (int x, int y)
		{
			IUISelectionService selectionServ = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (selectionServ != null) {
				Point location = new Point (x, y);
				if (!selectionServ.SelectionInProgress)
					location = this.SnapPointToGrid (new Point (x, y));

				selectionServ.MouseDragMove (location.X, location.Y);
			}
		}

		protected override void OnMouseDragEnd (bool cancel)
		{
			IUISelectionService selectionServ = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (selectionServ != null) {
				if (selectionServ.SelectionInProgress || selectionServ.ResizeInProgress)
					selectionServ.MouseDragEnd (cancel);
			}
		}

#if NET_2_0
		protected override void OnDragComplete (DragEventArgs de)
		{
			base.OnDragComplete (de);
		}
#endif

		internal override void OnMouseUp ()
		{
			base.OnMouseUp ();
			if (!this.Control.AllowDrop) { // check MouseDragBegin for the reason of having this
				this.Control.AllowDrop = true;
			}
		}

		internal override void OnMouseMove (int x, int y)
		{
			IUISelectionService uiSelection = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (uiSelection != null)
				uiSelection.SetCursor (x, y);

			base.OnMouseMove (x, y);
		}

		// Align the point to the grid
		//
		private Point SnapPointToGrid (Point location)
		{
			Rectangle gridSurface = this.Control.Bounds;
			Size gridSize = (Size)GetValue (this.Component, "GridSize");

			if ((bool)GetValue (this.Component, "SnapToGrid")) {
				int x = location.X + (gridSize.Width - (location.X % gridSize.Width));
				if (x > gridSurface.Width)
					x = gridSurface.Width - gridSize.Width;

				location.X = x;

				int y = location.Y + (gridSize.Height - (location.Y % gridSize.Height));
				if (y > gridSurface.Height)
					y = gridSurface.Height - gridSize.Height;

				location.Y = y;
			}
			return location;
		}
		
#endregion


		#region WndProc and Misc Message Handlers

		protected override void OnSetCursor ()
		{
			if (this.Control != null) {
				IToolboxService tbService = GetService (typeof (IToolboxService)) as IToolboxService;
				if (tbService != null)
					tbService.SetCursor ();
				else
					base.OnSetCursor ();
			}
		}

		// Draws the design-time grid if DrawGrid == true
		//
		protected override void OnPaintAdornments (PaintEventArgs pe)
		{
			base.OnPaintAdornments (pe);

			bool drawGrid;
			Size gridSize;

			// in case WM_PAINT is received before the IDesignerFilter is invoked to add
			// those properties.
			try {
				drawGrid = (bool)GetValue (this.Component, "DrawGrid");
			} catch {
				drawGrid = this.DrawGrid;
			}
			try {
				gridSize = (Size)GetValue (this.Component, "GridSize");
			} catch {
				gridSize = this.GridSize;
			}

			if (drawGrid) {
				GraphicsState state = pe.Graphics.Save ();
				pe.Graphics.TranslateTransform (this.Control.ClientRectangle.X,
								this.Control.ClientRectangle.Y);
				ControlPaint.DrawGrid (pe.Graphics, this.Control.ClientRectangle, gridSize, this.Control.BackColor);
				pe.Graphics.Restore (state);
			}

			IUISelectionService selection = this.GetService (typeof (IUISelectionService)) as IUISelectionService;
			if (selection != null)
				selection.PaintAdornments (this.Control, pe.Graphics);
		}

#endregion


		protected Control GetControl (object component)
		{
			IComponent comp = component as IComponent;

			if (comp != null && comp.Site != null) {
				IDesignerHost host = comp.Site.GetService (typeof (IDesignerHost)) as IDesignerHost;
				if (host != null) {
					ControlDesigner designer = host.GetDesigner (comp) as ControlDesigner;
					if (designer != null)
						return designer.Control;
				}
			}
			return null;
		}

#region NET_2_0 Stubs
#if NET_2_0
		[MonoTODO]
		protected virtual bool AllowControlLasso {
			get { return false; }
		}

		[MonoTODO]
		protected virtual bool AllowGenericDragBox {
			get { return false; }
		}

		[MonoTODO]
		protected internal virtual bool AllowSetChildIndexOnDrop {
			get { return false; }
		}

		[MonoTODO]
		public override IList SnapLines {
			get { return new object [0]; }
		}

		[MonoTODO]
		protected ToolboxItem MouseDragTool {
			get { return null; }
		}

		[MonoTODO]
		public override void InitializeNewComponent (IDictionary defaultValues)
		{
			base.InitializeNewComponent (defaultValues);
		}

		[MonoTODO]
		protected void AddPaddingSnapLines (ref ArrayList snapLines)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual Control GetParentForComponent (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override ControlBodyGlyph GetControlGlyph (GlyphSelectionType selectionType)
		{
			return base.GetControlGlyph (selectionType);
		}

		[MonoTODO]
		public override GlyphCollection GetGlyphs (GlyphSelectionType selectionType)
		{
			return base.GetGlyphs (selectionType);
		}

		[MonoTODO]
		protected Rectangle GetUpdatedRect (Rectangle originalRect, Rectangle dragRect, bool updateSize)
		{
			throw new NotImplementedException ();
		}
#endif
#endregion

	}
}
