//
// System.Windows.Forms.TreeNode
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class TreeNode : MarshalByRefObject, ICloneable {

		TreeNodeCollection children;
		TreeNode           parent;
		string             text;
		IntPtr             handle;
		int                imageIndex;
		int                selectedImageIndex;
		bool               checked_;
		bool               expanded;

		[MonoTODO]
		public TreeNode()
		{
			children = null;
			parent   = null;
			text     = String.Empty;
			handle   = IntPtr.Zero;
			imageIndex = 0;
			selectedImageIndex = 0;
			checked_ = false;
			expanded = false;
		}

		internal TreeNode ( IntPtr handle, TreeView tree ) : this ( )
		{
			this.handle = handle;
			Nodes.TreeView = tree;
		}

		[MonoTODO]
		public TreeNode( string text ) : this ( )
		{
			this.text = text;
		}
		[MonoTODO]
		public TreeNode( string text, TreeNode[] children ) : this ( text )
		{
			Nodes.AddRange ( children );
		}

		[MonoTODO]
		public TreeNode( string text, int imageIndex, int selectedImageIndex ) : this ( text )
		{
			this.imageIndex = imageIndex;
			this.selectedImageIndex = selectedImageIndex;
		}

		[MonoTODO]
		public TreeNode( string text, int imageIndex, int selectedImageIndex, TreeNode[] children ) : this ( text, children )
		{
			this.imageIndex = imageIndex;
			this.selectedImageIndex = selectedImageIndex;
		}

		// --- Public Properties
		
		[MonoTODO]
		public Color BackColor {
			get
			{ 
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}

		public Rectangle Bounds {
			get {
				IntPtr ptr = IntPtr.Zero;
				try {
					TreeView tree = TreeView;
					if ( tree == null )
						throw new NullReferenceException ( );

					ptr = Marshal.AllocHGlobal ( Marshal.SizeOf ( typeof ( RECT) ) );
					Marshal.WriteInt32 ( ptr, Handle.ToInt32 ( ) );
					Win32.SendMessage ( tree.Handle, (int) TreeViewMessages.TVM_GETITEMRECT, 1, ptr.ToInt32 ( ) );
					RECT rect = (RECT) Marshal.PtrToStructure ( ptr, typeof ( RECT ) );
					return new Rectangle ( rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top );
				}
				finally {
					Marshal.FreeHGlobal ( ptr );
				}
			}
		}
		[MonoTODO]
		public bool Checked {
			get {
				TreeView tree = TreeView;
				if ( tree != null && tree.IsHandleCreated && Created ) 
					checked_ = getCheckedState ( tree, Handle );

				return checked_;
			}
			set {
				checked_ = value;

				TreeView tree = TreeView;
				if ( tree != null && tree.IsHandleCreated && Created ) 
					setCheckedState ( tree, Handle, checked_ );
			}
		}

		public TreeNode FirstNode {
			get {
				if ( !Created && Nodes.Count > 0 )
					return Nodes[0];
				else {
					IntPtr hfirst = (IntPtr) Win32.SendMessage ( TreeView.Handle,
								(int) TreeViewMessages.TVM_GETNEXTITEM, 
								(int) TreeViewItemSelFlags.TVGN_CHILD,
								Handle.ToInt32 ( ) );
					return TreeNode.FromHandle ( TreeView, hfirst );
				}
			}
		}
		[MonoTODO]
		public Color ForeColor {
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
		public string FullPath {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public IntPtr Handle {
			get {
				 if ( !Created )
					createNode ( null );
				 return handle; 
			}
		}
		[MonoTODO]
		public int ImageIndex {
			get {
				TreeView tree = TreeView;
				if ( tree != null && tree.ImageIndex != 0 && imageIndex == 0 )
					return tree.ImageIndex;
				return imageIndex;
			}
			set {
				imageIndex = value;
			}
		}

		public int Index {
			get {
				if ( Parent == null )
					return 0;

				if ( Parent.handle == TreeView.RootHandle )
					return TreeView.Nodes.IndexOf ( this );

				return Parent.Nodes.IndexOf ( this );
			}
		}
		[MonoTODO]
		public bool IsEditing {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool IsExpanded {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool IsSelected {
			get {
				if ( TreeView != null ) {
					return false;
				}
				return false;
			}
		}
		[MonoTODO]
		public bool IsVisible {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TreeNode LastNode {
			get {
				return Nodes [ Nodes.Count - 1 ];
			}
		}
		[MonoTODO]
		public TreeNode NextNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TreeNode NextVisibleNode {
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
		public Font NodeFont {
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
				if ( children == null )
					children = new TreeNodeCollection ( this );
				return children;
			}
		}

		public TreeNode Parent {
			get { return parent; }
		}
		[MonoTODO]
		public TreeNode PrevNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TreeNode PrevVisibleNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectedImageIndex {
			get {
				TreeView tree = TreeView;
				if ( tree != null && tree.SelectedImageIndex != 0 && selectedImageIndex == 0 )
					return tree.SelectedImageIndex;

				return selectedImageIndex;
			}
			set {
				selectedImageIndex = value;	
			}
		}
		[MonoTODO]
		public object Tag {
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
		public string Text {
			get { return text; }
			set {
				text = value;
				updateText ( );
			}
		}

		public TreeView TreeView {
			get { 
				if ( Parent != null ) {
					TreeNode parent = Parent;
					while ( parent.Parent != null )
						parent = parent.Parent;
					return parent.Nodes.TreeView;
				}
				else {
					if ( handle == TreeView.RootHandle )
						return Nodes.TreeView;
				}
				return null;
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void BeginEdit()
		{
			//FIXME:
		}
		[MonoTODO]
		public virtual object Clone()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Collapse()
		{
			collapseImpl ( TreeView );
		}
		[MonoTODO]
		public void EndEdit(bool cancel)
		{
			//FIXME:
		}
		[MonoTODO]
		public void EnsureVisible()
		{
			//FIXME:
		}

		public void Expand()
		{
			expandImpl ( TreeView );
		}

		public void ExpandAll()
		{
			expandAllImpl ( TreeView );
		}
		[MonoTODO]
		public static TreeNode FromHandle(TreeView tree, IntPtr handle)
		{
			return FromHandle ( tree.Nodes , handle );
		}
		[MonoTODO]
		public int GetNodeCount(bool includeSubTrees)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Remove()
		{
			if ( Created ) {
				TreeView tree = TreeView;
				if ( tree != null && tree.IsHandleCreated ) {
					int res = Win32.SendMessage ( tree.Handle, (int) TreeViewMessages.TVM_DELETEITEM, 0, handle.ToInt32 ( ) );
					if ( res != 0 ) zeroHandle ( );
				}
			}
		}
		[MonoTODO]
		public void Toggle()
		{
			//FIXME:
		}
		[MonoTODO]
		public override string ToString()
		{
			//FIXME:
			return base.ToString();
		}

		internal void setParent ( TreeNode parent )
		{
			this.parent = parent;
		}
			
		internal void setHandle ( IntPtr hItem )
		{
			this.handle = hItem;
		}

		internal void makeTree ( IntPtr parent, TreeView treeView )
		{
			if ( handle == IntPtr.Zero )
				insertNode ( parent, treeView );
			
			foreach ( TreeNode node in Nodes )
				node.makeTree ( handle, treeView );
		}

		internal void createNode ( TreeView treeView )
		{
			IntPtr parentHandle = IntPtr.Zero;

			if ( Parent != null )
				parentHandle = Parent.Handle;
			else
				parentHandle = TreeView.RootHandle;

			if ( parentHandle != IntPtr.Zero ) {
				insertNode ( parentHandle, treeView );
			}
		}

		internal void insertNode (  IntPtr parent, TreeView treeView )
		{
			TreeView tree = ( treeView != null ) ? treeView : TreeView;
			if ( tree == null )
				return;

			TVINSERTSTRUCT insStruct = tree.insStruct;
			insStruct.hParent = parent;

			unchecked {
				int intPtr = tree.Sorted ? (int)TreeViewItemInsertPosition.TVI_SORT : (int) TreeViewItemInsertPosition.TVI_LAST;
				insStruct.hInsertAfter = (IntPtr) intPtr;
			}

			insStruct.item.mask = (uint) ( TreeViewItemFlags.TVIF_TEXT | TreeViewItemFlags.TVIF_STATE );
			insStruct.item.pszText = Text;
			insStruct.item.cchTextMax = Text.Length;

			if ( expanded ) {
				insStruct.item.state |= (uint) TreeViewItemState.TVIS_EXPANDED;
				insStruct.item.stateMask |= (uint) TreeViewItemState.TVIS_EXPANDED;
			}

			handle = (IntPtr) Win32.SendMessage ( TreeView.Handle , TreeViewMessages.TVM_INSERTITEMA, 0, ref insStruct );
			
			if ( tree.CheckBoxes )
				setCheckedState ( tree, handle, checked_ );
		}

		private static TreeNode FromHandle ( TreeNodeCollection nodes, IntPtr handle )
		{
			foreach ( TreeNode node in nodes ) {
				if ( node.handle == handle )
					return node;
				
				TreeNode cnode = FromHandle ( node.Nodes, handle );
				if ( cnode != null )
					return cnode;
			}
			return null;
		}

		internal void sortNode ( )
		{
			if ( Created ) {
				int res = Win32.SendMessage ( TreeView.Handle, (int)TreeViewMessages.TVM_SORTCHILDREN, 0, Handle.ToInt32 ( ) );
				foreach ( TreeNode node in Nodes )
					node.sortNode ( );
			}
		}

		private void zeroHandle ( )
		{
			handle = IntPtr.Zero;
			foreach ( TreeNode node in Nodes )
				node.zeroHandle ( );
		}

		private bool Created {
			get { return handle != IntPtr.Zero; }
		}

		private void expandImpl ( TreeView tree )
		{
			if ( tree != null && tree.IsHandleCreated && Created )
				Win32.SendMessage ( tree.Handle, (int)TreeViewMessages.TVM_EXPAND, (int) TreeViewItemExpansion.TVE_EXPAND, Handle.ToInt32() );
			else
				expanded = true;
		}

		internal void expandAllImpl ( TreeView tree )
		{
			expandImpl ( tree );
			foreach ( TreeNode node in Nodes )
				node.expandAllImpl ( tree );
		}

		internal void collapseImpl ( TreeView tree )
		{
			if ( tree != null && tree.IsHandleCreated && Created )
				Win32.SendMessage ( tree.Handle, (int)TreeViewMessages.TVM_EXPAND, (int) TreeViewItemExpansion.TVE_COLLAPSE, Handle.ToInt32() );
		}

		internal void collapseAllImpl ( TreeView tree )
		{
			collapseImpl ( tree );
			foreach ( TreeNode node in Nodes )
				node.collapseAllImpl ( tree );
		}
		
		private void setCheckedState ( TreeView tree, IntPtr hitem, bool isChecked )
		{
			TVINSERTSTRUCT insStruct = tree.insStruct;
	
			insStruct.item.mask = (uint) ( TreeViewItemFlags.TVIF_HANDLE | TreeViewItemFlags.TVIF_STATE );
			insStruct.item.hItem = hitem;
			insStruct.item.stateMask = (uint) TreeViewItemState.TVIS_STATEIMAGEMASK;
			insStruct.item.state = (uint) Win32.INDEXTOSTATEIMAGEMASK ( isChecked ? 2 : 1 );

			Win32.SendMessage ( TreeView.Handle , TreeViewMessages.TVM_SETITEMA, 0, ref insStruct.item );
		}

		private bool getCheckedState ( TreeView tree, IntPtr hitem )
		{
			TVINSERTSTRUCT insStruct = tree.insStruct;
	
			insStruct.item.mask = (uint) ( TreeViewItemFlags.TVIF_HANDLE | TreeViewItemFlags.TVIF_STATE );
			insStruct.item.hItem = hitem;
			insStruct.item.stateMask = (uint) TreeViewItemState.TVIS_STATEIMAGEMASK;

			Win32.SendMessage ( TreeView.Handle , TreeViewMessages.TVM_GETITEMA, 0, ref insStruct.item );
			return ((insStruct.item.state >> 12 ) - 1) > 0;
		}

		private void updateText ( )
		{
			TreeView tree = TreeView;
			if ( tree != null && tree.IsHandleCreated && this.Created ) {
				TVINSERTSTRUCT insStruct = tree.insStruct;
				insStruct.item.mask = (uint) ( TreeViewItemFlags.TVIF_HANDLE | TreeViewItemFlags.TVIF_TEXT );
				insStruct.item.hItem = Handle;
				insStruct.item.pszText = Text;
				insStruct.item.cchTextMax = Text.Length;
				Win32.SendMessage ( TreeView.Handle , TreeViewMessages.TVM_SETITEMA, 0, ref insStruct.item );
			}
		}
	}
}
