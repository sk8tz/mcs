//
// System.Windows.Forms.TreeView
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>

	//
	// </summary>

    public class TreeView : Control {

		private int imageIndex;
		private int selectedImageIndex;

		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public TreeView()
		{
			imageIndex = 0;
			selectedImageIndex = 0;
		}
		
		// --- Public Properties
		
		[MonoTODO]
		public override Color BackColor {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Image BackgroundImage {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public BorderStyle BorderStyle {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool CheckBoxes {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool FullRowSelect {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool HideSelection {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool HotTracking {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int ImageIndex {
			get
			{
				return imageIndex;
			}
			set
			{
				imageIndex = value;
			}
		}
		[MonoTODO]
		public ImageList ImageList {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int Indent {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TreeNodeCollection Nodes {
			get
			{
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectedImageIndex {
			get
			{
				return selectedImageIndex;
			}
			set
			{
				selectedImageIndex = value;
			}
		}
		[MonoTODO]
		public TreeNode SelectedNode {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ShowLines {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ShowPlusMinus {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ShowRootLines {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Sorted {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		public override string Text {
			//FIXME just to get it to run
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
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
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void CollapseAll()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void EndUpdate()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ExpandAll()
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}
		
		// --- Public Events
		
		[MonoTODO]
		public event TreeViewEventHandler AfterCheck;
		[MonoTODO]
		public event TreeViewEventHandler AfterCollapse;
		[MonoTODO]
		public event TreeViewEventHandler AfterExpand;
		[MonoTODO]
		public event NodeLabelEditEventHandler AfterLabelEdit;
		[MonoTODO]
		public event TreeViewEventHandler AfterSelect;
		[MonoTODO]
		public event TreeViewCancelEventHandler BeforeCheck;
		[MonoTODO]
		public event TreeViewCancelEventHandler BeforeCollapse;
		[MonoTODO]
		public event TreeViewCancelEventHandler BeforeExpand;
		[MonoTODO]
		public event NodeLabelEditEventHandler BeforeLabelEdit;
		[MonoTODO]
		public event TreeViewCancelEventHandler BeforeSelect;
		[MonoTODO]
		public event ItemDragEventHandler ItemDrag;
		//public new event PaintEventHandler Paint;
        
        // --- Protected Properties
        
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "TREEVIEW";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				//			createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
				window.CreateHandle (createParams);
				return createParams;
			}		
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get
			{
				throw new NotImplementedException ();
			}
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override void CreateHandle()
		{
			//FIXME: just to get it to run
			base.CreateHandle();
		}

		//inherited
		//protected override void Dispose(bool disposing)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnAfterCheck(TreeViewEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnAfterCollapse(TreeViewEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnAfterExpand(TreeViewEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnAfterLabelEdit(NodeLabelEditEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnAfterSelect(TreeViewEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnBeforeCheck(TreeViewCancelEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnBeforeCollapse(TreeViewCancelEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnItemDrag(ItemDragEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnKeyDown(KeyEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnKeyUp(KeyEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}
	}
}
