//
// System.Windows.Forms.TreeView
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>

	//
	// </summary>

    public class TreeView : Control {

		private int imageIndex;
		private int selectedImageIndex;
		private TreeNodeCollection nodes;
		private int indent;
		private BorderStyle borderStyle;
		private bool checkBoxes;
		private bool fullRowSelect;
		private bool hideSelection;
		private bool hotTracking;
		private bool showLines;
		private bool showPlusMinus;
		private bool showRootLines;
		private ImageList imageList;
		private bool sorted;
		private TreeNode selectedNode;
		private TreeNode dummyNode;

		const int DefaultIndent = 19;
		
		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public TreeView()
		{
			imageIndex = 0;
			selectedImageIndex = 0;
			SubClassWndProc_ = true;
			borderStyle = BorderStyle.Fixed3D;
			checkBoxes = false;
			fullRowSelect = false;
			hideSelection = true;
			hotTracking = false;
			showLines = true;
			showPlusMinus = true;
			showRootLines = true;
			sorted = false;
			imageIndex = 0;
			imageList = null;
			indent = DefaultIndent;
			selectedNode = null;

			dummyNode = new TreeNode( RootHandle, this );
		}
		
		// --- Public Properties
		
		[MonoTODO]
		public override Color BackColor {
			get { return base.BackColor; }
			set {
				base.BackColor = value;

				if ( IsHandleCreated )
					setBackColor ( );
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage;  }
			set { base.BackgroundImage = value; }
		}

		public BorderStyle BorderStyle {
			get {   return borderStyle; }
			set {
				if ( !Enum.IsDefined ( typeof(BorderStyle), value ) )
					throw new InvalidEnumArgumentException( "BorderStyle",
						(int)value,
						typeof(BorderStyle));
				
				if ( borderStyle != value ) {
					int oldStyle = getBorderStyle ( borderStyle );
					int oldExStyle = getBorderExStyle ( borderStyle );
					borderStyle = value;

					if ( IsHandleCreated ) {
						Win32.UpdateWindowStyle ( Handle, oldStyle, getBorderStyle ( borderStyle ) );
						Win32.UpdateWindowExStyle ( Handle, oldExStyle, getBorderExStyle ( borderStyle ) );
					}
				}
			}
		}
		[MonoTODO]
		public bool CheckBoxes {
			get { return checkBoxes; }
			set {
				checkBoxes = value;
				RecreateHandle ( );
			}
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set {
				base.ForeColor = value;
			
				if ( IsHandleCreated )
					setForeColor ( );
			}
		}

		public bool FullRowSelect {
			get { return fullRowSelect; }
			set {
				if ( fullRowSelect != value ) {
					int oldStyle = fullRowSelect ? (int)TreeViewStyles.TVS_FULLROWSELECT : 0;
					fullRowSelect = value;
						
					if ( IsHandleCreated ) {
						int newStyle = fullRowSelect ? (int)TreeViewStyles.TVS_FULLROWSELECT : 0;
						Win32.UpdateWindowStyle ( Handle, oldStyle, newStyle );
					}
				}
			}
		}

		public bool HideSelection {
			get { return hideSelection; }
			set {
				if ( hideSelection != value ) {
					int oldStyle = hideSelection ? 0 : (int)TreeViewStyles.TVS_SHOWSELALWAYS;
					hideSelection = value;
						
					if ( IsHandleCreated ) {
						int newStyle = hideSelection ? 0 : (int)TreeViewStyles.TVS_SHOWSELALWAYS;
						Win32.UpdateWindowStyle ( Handle, oldStyle, newStyle );
					}
				}
			}
		}

		public bool HotTracking {
			get { return hotTracking; }
			set {
				if ( hotTracking != value ) {
					int oldStyle = hotTracking ? (int)TreeViewStyles.TVS_TRACKSELECT : 0;
					hotTracking = value;
						
					if ( IsHandleCreated ) {
						int newStyle = hotTracking ? (int)TreeViewStyles.TVS_TRACKSELECT : 0;
						Win32.UpdateWindowStyle ( Handle, oldStyle, newStyle );
					}
				}
			}
		}
		[MonoTODO]
		public int ImageIndex {
			get { return imageIndex; }
			set {
				if ( imageIndex != value ) {
					imageIndex = value;
					RecreateHandle ( );
				}
			}
		}
		[MonoTODO]
		public ImageList ImageList {
			get { return imageList; }
			set {
				if ( imageList != value ) {
					imageList = value;
					if ( IsHandleCreated )
						setImageList ( );
				}
			}
		}

		public int Indent {
			get { return indent; }
			set {
				if ( value < 0 )
					throw new ArgumentException ( 
						 string.Format ("'{0}' is not a valid value for 'Indent'.  'Indent' must be greater than or equal to 0.", value), "value");

				if ( value > 32000 )
					throw new ArgumentException ( 
						string.Format ("'{0}' is not a valid value for 'Indent'. 'Indent' must be less than or equal to 32000.", value), "value" );

				if ( indent != value ) {
					indent = value;

					if ( IsHandleCreated )
						setIndent ( );
				}
			}
		}
		[MonoTODO]
		public int ItemHeight {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public bool LabelEdit {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public TreeNodeCollection Nodes {
			get {
				if ( nodes == null )
					nodes = new TreeNodeCollection ( dummyNode );
				return nodes;
			}
		}
		[MonoTODO]
		public string PathSeparator {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public bool Scrollable {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public int SelectedImageIndex {
			get { return selectedImageIndex; }
			set {
				if ( selectedImageIndex != value ) {
					selectedImageIndex = value;
					RecreateHandle ( );
				}
			}
		}
		[MonoTODO]
		public TreeNode SelectedNode {
			get {
				if ( IsHandleCreated ) {
					int hitem = Win32.SendMessage ( Handle, (int) TreeViewMessages.TVM_GETNEXTITEM, (int)TreeViewItemSelFlags.TVGN_CARET, 0 );
					selectedNode = TreeNode.FromHandle ( this, (IntPtr) hitem );
				}
				return selectedNode;
			}
			set {
				selectedNode = value;
				if ( IsHandleCreated )
					selectItem ( selectedNode != null ? selectedNode.Handle : IntPtr.Zero );
			}
		}

		public bool ShowLines {
			get { return showLines;	}
			set {
				if ( showLines != value ) {
					int oldStyle = showLines ? (int)TreeViewStyles.TVS_HASLINES : 0;
					showLines = value;
						
					if ( IsHandleCreated ) {
						int newStyle = showLines ? (int)TreeViewStyles.TVS_HASLINES : 0;
						Win32.UpdateWindowStyle ( Handle, oldStyle, newStyle );
					}
				}
			}
		}

		public bool ShowPlusMinus {
			get { return showPlusMinus; }
			set {
				if ( showPlusMinus != value ) {
					int oldStyle = showPlusMinus ? (int)TreeViewStyles.TVS_HASBUTTONS : 0;
					showPlusMinus = value;
						
					if ( IsHandleCreated ) {
						int newStyle = showPlusMinus ? (int)TreeViewStyles.TVS_HASBUTTONS : 0;
						Win32.UpdateWindowStyle ( Handle, oldStyle, newStyle );
					}
				}
			}
		}

		public bool ShowRootLines {
			get { return showRootLines; }
			set {
				if ( showRootLines != value ) {
					int oldStyle = showRootLines ? (int)TreeViewStyles.TVS_LINESATROOT : 0;
					showRootLines = value;
						
					if ( IsHandleCreated ) {
						int newStyle = showRootLines ? (int)TreeViewStyles.TVS_LINESATROOT : 0;
						Win32.UpdateWindowStyle ( Handle, oldStyle, newStyle );
					}
				}
			}
		}
		[MonoTODO]
		public bool Sorted {
			get { return sorted; }
			set {
				sorted = value;
				if ( IsHandleCreated && sorted )
					sortTree ( );
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text;	 }
			set { base.Text = value; }
		}
		[MonoTODO]
		public TreeNode TopNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int VisibleCount {
			get
			{
				throw new NotImplementedException ();
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void BeginUpdate() 
		{
			//FIXME:
		}
		[MonoTODO]
		public void CollapseAll()
		{
			//FIXME:
		}
		[MonoTODO]
		public void EndUpdate()
		{
			//FIXME:
		}
		[MonoTODO]
		public void ExpandAll()
		{
			//FIXME:
		}
		[MonoTODO]
		public TreeNode GetNodeAt(Point pt)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public TreeNode GetNodeAt(int x, int y)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int GetNodeCount(bool includeSubTrees)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			//FIXME:
			return base.ToString();
		}
		
		// --- Public Events
		
		public event TreeViewEventHandler AfterCheck;
		public event TreeViewEventHandler AfterCollapse;
		public event TreeViewEventHandler AfterExpand;
		public event NodeLabelEditEventHandler AfterLabelEdit;
		public event TreeViewEventHandler AfterSelect;
		public event TreeViewCancelEventHandler BeforeCheck;
		public event TreeViewCancelEventHandler BeforeCollapse;
		public event TreeViewCancelEventHandler BeforeExpand;
		public event NodeLabelEditEventHandler BeforeLabelEdit;
		public event TreeViewCancelEventHandler BeforeSelect;
		public event ItemDragEventHandler ItemDrag;
		//public new event PaintEventHandler Paint;
        
        // --- Protected Properties
        
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = Win32.TREEVIEW_CLASS;
				createParams.Style |= (int) WindowStyles.WS_CHILD ;

				createParams.Style   |= getBorderStyle   ( BorderStyle );
				createParams.ExStyle |= getBorderExStyle ( BorderStyle );

				if ( CheckBoxes )
					createParams.Style |= (int) TreeViewStyles.TVS_CHECKBOXES;

				if ( FullRowSelect )
					createParams.Style |= (int) TreeViewStyles.TVS_FULLROWSELECT;

				if ( ShowLines )
					createParams.Style |= (int) TreeViewStyles.TVS_HASLINES;

				if ( !HideSelection )
					createParams.Style |= (int) TreeViewStyles.TVS_SHOWSELALWAYS;

				if ( HotTracking )
					createParams.Style |= (int) TreeViewStyles.TVS_TRACKSELECT;

				if ( ShowPlusMinus )
					createParams.Style |= (int) TreeViewStyles.TVS_HASBUTTONS;

				if ( ShowRootLines )
					createParams.Style |= (int) TreeViewStyles.TVS_LINESATROOT;

				return createParams;
			}		
		}

		protected override Size DefaultSize {
			get { return new Size(121,97); }
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override void CreateHandle()
		{
			initCommonControlsLibrary ( );
			base.CreateHandle();
		}


		[MonoTODO]
		protected override bool IsInputKey(Keys keyData)
		{
			//FIXME:
			return base.IsInputKey(keyData);
		}
		[MonoTODO]
		protected virtual void OnAfterCheck(TreeViewEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnAfterCollapse(TreeViewEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnAfterExpand(TreeViewEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnAfterLabelEdit(NodeLabelEditEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnAfterSelect(TreeViewEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeCheck(TreeViewCancelEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeCollapse(TreeViewCancelEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			//FIXME:
			base.OnHandleCreated(e);

			makeTree ( );
			
			setImageList ( );
			if ( BackColor != Control.DefaultBackColor )
				setBackColor ( );
			if ( ForeColor != Control.DefaultForeColor )
				setForeColor ( );
			if ( Indent != DefaultIndent )
				setIndent ( );
			if ( selectedNode != null )
				selectItem ( selectedNode.Handle );
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			//FIXME:
			base.OnHandleDestroyed(e);
		}
		[MonoTODO]
		protected virtual void OnItemDrag(ItemDragEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void OnKeyDown(KeyEventArgs e)
		{
			//FIXME:
			base.OnKeyDown(e);
		}
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			//FIXME:
			base.OnKeyPress(e);
		}
		[MonoTODO]
		protected override void OnKeyUp(KeyEventArgs e)
		{
			//FIXME:
            base.OnKeyUp(e);
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			CallControlWndProc( ref m );
		}

		private void initCommonControlsLibrary ( ) {
			if ( !RecreatingHandle ) {
				INITCOMMONCONTROLSEX initEx = new INITCOMMONCONTROLSEX();
				initEx.dwICC = CommonControlInitFlags.ICC_TREEVIEW_CLASSES;
				Win32.InitCommonControlsEx(initEx);
			}
		}

		internal void makeTree ( )
		{
			foreach ( TreeNode node in Nodes )
				node.makeTree ( RootHandle );
		}

		private void setBackColor ( )
		{
			Win32.SendMessage ( Handle , (int)TreeViewMessages.TVM_SETBKCOLOR, 0, Win32.RGB( BackColor ) ) ;
		}

		private void setForeColor ( )
		{
			Win32.SendMessage ( Handle , (int)TreeViewMessages.TVM_SETTEXTCOLOR, 0, Win32.RGB( ForeColor ) ) ;
		}

		private int getBorderStyle ( BorderStyle style )
		{
			if ( style == BorderStyle.FixedSingle )
				return (int) WindowStyles.WS_BORDER;

			return 0;
		}

		private int getBorderExStyle ( BorderStyle style )
		{
			if ( style == BorderStyle.Fixed3D )
				return (int) (int)WindowExStyles.WS_EX_CLIENTEDGE;

			return 0;
		}

		private void setImageList ( )
		{
			int handle = ( ImageList != null ) ? ImageList.Handle.ToInt32 ( ) : 0 ;
			Win32.SendMessage ( Handle , (int)TreeViewMessages.TVM_SETIMAGELIST, (int)TreeViewImageListFlags.TVSIL_NORMAL, handle ) ;
		}

		private void setIndent ( )
		{
			Win32.SendMessage ( Handle , (int)TreeViewMessages.TVM_SETINDENT, Indent, 0 ) ;
		}

		private void selectItem ( IntPtr handle )
		{
			Win32.SendMessage ( Handle , (int)TreeViewMessages.TVM_SELECTITEM, (int)TreeViewItemSelFlags.TVGN_CARET, handle.ToInt32 ( ) ) ;
		}

		internal static IntPtr RootHandle {
			get {
				int rootHandle = 0;
				unchecked {
					rootHandle = (int) TreeViewItemInsertPosition.TVI_ROOT;
				}
				return ( IntPtr ) rootHandle;
			}
		}

		private void sortTree ( )
		{
			int res = Win32.SendMessage ( Handle, (int)TreeViewMessages.TVM_SORTCHILDREN, 0, RootHandle.ToInt32 ( ) );
			foreach ( TreeNode node in Nodes )
				node.sortNode ( );
		}		
	}
}
