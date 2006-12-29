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
// Copyright (c) 2006 Alexander Olk
//
// Authors:
//	Alexander Olk (alex.olk@googlemail.com)
//
//

using System;
using System.Drawing;
using System.ComponentModel;
using System.Resources;
using System.IO;
using System.Collections;

namespace System.Windows.Forms {
	[DefaultEvent ("HelpRequest")]
	[DefaultProperty ("SelectedPath")]
	[Designer ("System.Windows.Forms.Design.FolderBrowserDialogDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public sealed class FolderBrowserDialog : CommonDialog
	{
		#region Local Variables
		private string description = "";
		private Environment.SpecialFolder rootFolder = Environment.SpecialFolder.Desktop;
		private string selectedPath = "";
		private bool showNewFolderButton = true;
		
		private Label descriptionLabel;
		private Button cancelButton;
		private Button okButton;
		private FolderBrowserTreeView folderBrowserTreeView;
		private Button newFolderButton;
		private ContextMenu folderBrowserTreeViewContextMenu;
		private MenuItem newFolderMenuItem;
		
		private string old_selectedPath = "";
		
		private readonly string folderbrowserdialog_string = "FolderBrowserDialog";
		private readonly string width_string = "Width";
		private readonly string height_string = "Height";
		private readonly string x_string = "X";
		private readonly string y_string = "Y";
		#endregion	// Local Variables
		
		#region Public Constructors
		public FolderBrowserDialog ()
		{
			Size formConfigSize = Size.Empty;
			Point formConfigLocation = Point.Empty;
			
			object formWidth = MWFConfig.GetValue (folderbrowserdialog_string, width_string);
			
			object formHeight = MWFConfig.GetValue (folderbrowserdialog_string, height_string);
			
			if (formHeight != null && formWidth != null)
				formConfigSize = new Size ((int)formWidth, (int)formHeight);
			
			object formLocationX = MWFConfig.GetValue (folderbrowserdialog_string, x_string);
			object formLocationY = MWFConfig.GetValue (folderbrowserdialog_string, y_string);
			
			if (formLocationX != null && formLocationY != null)
				formConfigLocation = new Point ((int)formLocationX, (int)formLocationY);
			
			newFolderButton = new Button ();
			folderBrowserTreeView = new FolderBrowserTreeView (this);
			okButton = new Button ();
			cancelButton = new Button ();
			descriptionLabel = new Label ();
			folderBrowserTreeViewContextMenu = new ContextMenu ();
			
			form.AcceptButton = okButton;
			form.CancelButton = cancelButton;
			
			form.SuspendLayout ();
			form.Size =  new Size (322, 288);
			form.MinimumSize = new Size (322, 288);
			form.Text = "Search Folder";
			form.SizeGripStyle = SizeGripStyle.Show;

			newFolderMenuItem = new MenuItem("New Folder", new EventHandler (OnClickNewFolderButton));
			folderBrowserTreeViewContextMenu.MenuItems.Add(newFolderMenuItem);
			
			// descriptionLabel
			descriptionLabel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
				| AnchorStyles.Right)));
			descriptionLabel.Location = new Point (17, 14);
			descriptionLabel.Size = new Size (290, 40);
			descriptionLabel.TabIndex = 0;
			descriptionLabel.Text = "";
			
			// folderBrowserTreeView
			folderBrowserTreeView.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
				| AnchorStyles.Left)
				| AnchorStyles.Right)));
			folderBrowserTreeView.ImageIndex = -1;
			folderBrowserTreeView.Location = new Point (20, 61);
			folderBrowserTreeView.SelectedImageIndex = -1;
			folderBrowserTreeView.Size = new Size (278, 153);
			folderBrowserTreeView.TabIndex = 1;
			folderBrowserTreeView.ShowLines = false;
			folderBrowserTreeView.ShowPlusMinus = true;
			folderBrowserTreeView.HotTracking = true;
			folderBrowserTreeView.BorderStyle = BorderStyle.Fixed3D;
			folderBrowserTreeView.ContextMenu = folderBrowserTreeViewContextMenu;
			//folderBrowserTreeView.Indent = 2;
			
			// newFolderButton
			newFolderButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
			newFolderButton.FlatStyle = FlatStyle.System;
			newFolderButton.Location = new Point (14, 230);
			newFolderButton.Size = new Size (125, 23);
			newFolderButton.TabIndex = 2;
			newFolderButton.Text = "New Folder";
			newFolderButton.Enabled = true;
			
			// okButton
			okButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			okButton.FlatStyle = FlatStyle.System;
			okButton.Location = new Point (142, 230);
			okButton.Size = new Size (80, 23);
			okButton.TabIndex = 3;
			okButton.Text = "OK";
			
			// cancelButton
			cancelButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.FlatStyle = FlatStyle.System;
			cancelButton.Location = new Point (226, 230);
			cancelButton.Size = new Size (80, 23);
			cancelButton.TabIndex = 4;
			cancelButton.Text = "Cancel";
			
			form.Controls.Add (cancelButton);
			form.Controls.Add (okButton);
			form.Controls.Add (newFolderButton);
			form.Controls.Add (folderBrowserTreeView);
			form.Controls.Add (descriptionLabel);
			
			form.ResumeLayout (false);
			
			if (formConfigSize != Size.Empty) {
				form.Size = formConfigSize;
			}
			
			if (formConfigLocation != Point.Empty) {
				form.Location = formConfigLocation;
			}
			
			okButton.Click += new EventHandler (OnClickOKButton);
			cancelButton.Click += new EventHandler (OnClickCancelButton);
			newFolderButton.Click += new EventHandler (OnClickNewFolderButton);
			
			RootFolder = rootFolder;
		}
		
		#endregion	// Public Constructors
		
		#region Public Instance Properties
		[Browsable(true)]
		[DefaultValue("")]
		[Localizable(true)]
		public string Description {
			set {
				description = value;
				descriptionLabel.Text = description;
			}
			
			get {
				return description;
			}
		}
		
		[Browsable(true)]
		[DefaultValue(Environment.SpecialFolder.Desktop)]
		[Localizable(false)]
		public Environment.SpecialFolder RootFolder {
			set {
				int v = (int)value;
				
				if (!Enum.IsDefined(typeof(Environment.SpecialFolder), v))
					throw new InvalidEnumArgumentException ();
				
				if (rootFolder != value)
					rootFolder = value;
				
				folderBrowserTreeView.RootFolder = rootFolder;
			}
			
			get {
				return rootFolder;
			}
		}
		
		[Browsable(true)]
		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.SelectedPathEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		public string SelectedPath {
			set {
				if (!Path.IsPathRooted(value))
					return;
				
				selectedPath = value;
				old_selectedPath = value;
				folderBrowserTreeView.SelectedPath = selectedPath;
			}
			
			get {
				return selectedPath;
			}
		}
		
		[Browsable(true)]
		[DefaultValue(true)]
		[Localizable(false)]
		public bool ShowNewFolderButton {
			set {
				if (value != showNewFolderButton) {
					showNewFolderButton = value;
					if (showNewFolderButton)
						newFolderButton.Show ();
					else
						newFolderButton.Hide ();
				}
			}
			
			get {
				return showNewFolderButton;
			}
		}
		#endregion	// Public Instance Properties
		
		#region Public Instance Methods
		public override void Reset ()
		{
			Description = "";
			RootFolder = Environment.SpecialFolder.Desktop;
			selectedPath = "";
			ShowNewFolderButton = true;
		}
		
		protected override bool RunDialog (IntPtr hwndOwner)
		{
			form.Refresh ();
			
			return true;
		}
		#endregion	// Public Instance Methods
		
		#region Internal Methods
		void OnClickOKButton (object sender, EventArgs e)
		{
			WriteConfigValues ();
			
			form.DialogResult = DialogResult.OK;
		}
		
		void OnClickCancelButton (object sender, EventArgs e)
		{
			WriteConfigValues ();
			
			selectedPath = old_selectedPath;
			form.DialogResult = DialogResult.Cancel;
		}
		
		void OnClickNewFolderButton (object sender, EventArgs e)
		{
			folderBrowserTreeView.CreateNewFolder ();
		}
		
		private void WriteConfigValues ()
		{
			MWFConfig.SetValue (folderbrowserdialog_string, width_string, form.Width);
			MWFConfig.SetValue (folderbrowserdialog_string, height_string, form.Height);
			MWFConfig.SetValue (folderbrowserdialog_string, x_string, form.Location.X);
			MWFConfig.SetValue (folderbrowserdialog_string, y_string, form.Location.Y);
		}
		#endregion	// Internal Methods
		
		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler HelpRequest {
			add { base.HelpRequest += value; }
			remove { base.HelpRequest -= value; }
		}
		#endregion
		
		internal class FolderBrowserTreeView : TreeView
		{
			private MWFVFS vfs = new MWFVFS ();
			new private FBTreeNode root_node;
			private FolderBrowserDialog parentDialog;
			private ImageList imageList = new ImageList ();
			private Environment.SpecialFolder rootFolder;
			private bool dont_enable = false;
			
			private int platform = (int) Environment.OSVersion.Platform;
			
			public FolderBrowserTreeView (FolderBrowserDialog parent_dialog)
			{
				parentDialog = parent_dialog;
				ImageList = imageList;
				SetupImageList ();
			}
			
			public Environment.SpecialFolder RootFolder {
				set {
					rootFolder = value;
					
					string root_path = "";
					
					switch (rootFolder) {
						default:
						case Environment.SpecialFolder.Desktop:
							root_node = new FBTreeNode ("Desktop");
							root_node.RealPath = ThemeEngine.Current.Places (UIIcon.PlacesDesktop);
							root_path = MWFVFS.DesktopPrefix;
							break;
						case Environment.SpecialFolder.MyComputer:
							root_node = new FBTreeNode ("My Computer");
							root_path = MWFVFS.MyComputerPrefix;
							break;
						case Environment.SpecialFolder.Personal:
							root_node = new FBTreeNode ("Personal");
							root_path = MWFVFS.PersonalPrefix;
							root_node.RealPath = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
							break;
					}
					
					root_node.Tag = root_path;
					root_node.ImageIndex = NodeImageIndex (root_path);
					
					FillNode (root_node);
					
					root_node.Expand ();
					
					Nodes.Add (root_node);
				}
			}
			
			public string SelectedPath {
				set {
					if (Check_if_path_is_child_of_RootFolder (value)) {
						SetSelectedPath (Path.GetFullPath (value));
					}
				}
			}
			
			private string parent_real_path;
			private bool dont_do_onbeforeexpand = false;
			
			public void CreateNewFolder ()
			{
				FBTreeNode fbnode = SelectedNode as FBTreeNode;
				
				if (fbnode == null || fbnode.Parent == null)
					return;
				
				if (fbnode.RealPath == null)
					return;
				
				string tmp_filename = "New Folder";
				
				if (Directory.Exists (Path.Combine (fbnode.RealPath, tmp_filename))) {
					int i = 1;
					
					if ((platform == 4) || (platform == 128)) {
						tmp_filename = tmp_filename + "-" + i;
					} else {
						tmp_filename = tmp_filename + " (" + i + ")";
					}
					
					while (Directory.Exists (Path.Combine (fbnode.RealPath, tmp_filename))) {
						i++;
						if ((platform == 4) || (platform == 128)) {
							tmp_filename = "New Folder" + "-" + i;
						} else {
							tmp_filename = "New Folder" + " (" + i + ")";
						}
					}
				}
				
				parent_real_path = fbnode.RealPath;
				
				FBTreeNode new_node = new FBTreeNode (tmp_filename);
				new_node.ImageIndex = NodeImageIndex(tmp_filename);
				
				FillNode (fbnode);
				dont_do_onbeforeexpand = true;
				fbnode.Expand ();
				dont_do_onbeforeexpand = false;
				
				fbnode.Nodes.Add (new_node);
				
				LabelEdit = true;
				if (!new_node.IsEditing)
					new_node.BeginEdit();
			}
			
			protected override void OnAfterLabelEdit (NodeLabelEditEventArgs e)
			{
				if (e.Label != null) {
					if (e.Label.Length > 0) {
						FBTreeNode fbnode = e.Node as FBTreeNode;
						
						fbnode.RealPath = Path.Combine(parent_real_path, e.Label);
						
						if (vfs.CreateFolder (fbnode.RealPath)) {
							SelectedNode = e.Node;
						} else {
							SelectedNode = e.Node.Parent;
							e.Node.Parent.Nodes.Remove(e.Node);
						}
						
						e.Node.EndEdit (false);
					} else {
						e.CancelEdit = true;
						e.Node.BeginEdit();
					}
					
					LabelEdit = false;
				}
			}
			
			private void SetSelectedPath (string path)
			{
				BeginUpdate ();
				
				FBTreeNode node = FindPathInNodes (path, Nodes);
				
				if (node == null) {
					Stack stack = new Stack ();
					
					string path_cut = path.Substring (0, path.LastIndexOf (Path.AltDirectorySeparatorChar));
					
					while (node == null && path_cut.Length > 0) {
						node = FindPathInNodes (path_cut, Nodes);
						
						if (node == null) {
							string path_cut_new = path_cut.Substring (0, path_cut.LastIndexOf (Path.AltDirectorySeparatorChar));
							string leftover = path_cut.Replace (path_cut_new, "");
							
							stack.Push (leftover);
							
							path_cut = path_cut_new;
						}
					}
					
					if (stack.Count > 0) {
						FillNode (node);
						node.Expand ();
						
						// walk through the subdirs and fill the nodes
						while (stack.Count > 0) {
							string part_name = stack.Pop () as string;
							
							foreach (TreeNode treeNode in node.Nodes) {
								FBTreeNode fbnode = treeNode as FBTreeNode;
								
								if (path_cut + part_name == fbnode.RealPath) {
									node = fbnode;
									path_cut += part_name;
									
									FillNode (node);
									node.Expand ();
									break;
								}
							}
						}
						
						// finally find the node for the complete path
						foreach (TreeNode treeNode in node.Nodes) {
							FBTreeNode fbnode = treeNode as FBTreeNode;
							
							if (path == fbnode.RealPath) {
								node = fbnode;
								break;
							}
						}
					}
				}
				
				if (node != null) {
					SelectedNode = node;
					node.EnsureVisible ();
				}
				
				EndUpdate ();
			}
			
			private FBTreeNode FindPathInNodes (string path, TreeNodeCollection nodes)
			{
				foreach (TreeNode node in nodes) {
					FBTreeNode fbnode = node as FBTreeNode;
					
					if (fbnode != null && fbnode.RealPath != null) {
						if (fbnode.RealPath == path)
							return fbnode;
					}
					
					return FindPathInNodes (path, node.Nodes);
				}
				
				return null;
			}
			
			private bool Check_if_path_is_child_of_RootFolder (string path)
			{
				string root_path = (string)root_node.RealPath;
				
				if (root_path != null) {
					try {
						if (!Directory.Exists (path))
							return false;
						
						switch (rootFolder) {
							case Environment.SpecialFolder.Desktop:
							case Environment.SpecialFolder.MyComputer:
								return true;
							case Environment.SpecialFolder.Personal:
								if (!path.StartsWith (root_path))
									return false;
								else
									return true;
							default:
								return false;
						}
					} catch {}
				}
				
				return false;
			}
			
			private void FillNode (TreeNode node)
			{
				BeginUpdate ();
				
				node.Nodes.Clear ();
				vfs.ChangeDirectory ((string)node.Tag);
				ArrayList folders = vfs.GetFoldersOnly ();
				
				foreach (FSEntry fsentry in folders) {
					if (fsentry.Name.StartsWith ("."))
						continue;
					
					FBTreeNode child = new FBTreeNode (fsentry.Name);
					child.Tag = fsentry.FullName;
					child.RealPath = fsentry.RealName == null ? fsentry.FullName : fsentry.RealName;
					child.ImageIndex = NodeImageIndex (fsentry.FullName);
					
					vfs.ChangeDirectory (fsentry.FullName);
					ArrayList sub_folders = vfs.GetFoldersOnly ();
					
					foreach (FSEntry fsentry_sub in sub_folders) {
						if (!fsentry_sub.Name.StartsWith (".")) {
							child.Nodes.Add (new TreeNode (String.Empty));
							break;
						}
					}
					
					node.Nodes.Add (child);
				}
				
				EndUpdate ();
			}
			
			private void SetupImageList ()
			{
				imageList.ColorDepth = ColorDepth.Depth32Bit;
				imageList.ImageSize = new Size (16, 16);
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesRecentDocuments, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesDesktop, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesPersonal, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesMyComputer, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesMyNetwork, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.NormalFolder, 16));
				imageList.TransparentColor = Color.Transparent;
			}
			
			private int NodeImageIndex (string path)
			{
				int index = 5;
				
				if (path == MWFVFS.DesktopPrefix)
					index = 1;
				else 
				if (path == MWFVFS.RecentlyUsedPrefix)
					index = 0;
				else
				if (path == MWFVFS.PersonalPrefix)
					index = 2;
				else
				if (path == MWFVFS.MyComputerPrefix)
					index = 3;
				else
				if (path == MWFVFS.MyNetworkPrefix)
					index = 4;
				
				return index;
			}
			
			protected override void OnAfterSelect (TreeViewEventArgs e)
			{
				if (e.Node == null)
					return;
				
				FBTreeNode fbnode = e.Node as FBTreeNode;
				
				if (fbnode.RealPath == null || fbnode.RealPath.IndexOf ("://") != -1) {
					parentDialog.okButton.Enabled = false;
					parentDialog.newFolderButton.Enabled = false;
					parentDialog.newFolderMenuItem.Enabled = false;
					dont_enable = true;
				} else {
					parentDialog.okButton.Enabled = true;
					parentDialog.newFolderButton.Enabled = true;
					parentDialog.newFolderMenuItem.Enabled = true;
					parentDialog.selectedPath = fbnode.RealPath;
					dont_enable = false;
				}
				
				base.OnAfterSelect (e);
			}
			
			protected internal override void OnBeforeExpand (TreeViewCancelEventArgs e)
			{
				if (!dont_do_onbeforeexpand) {
					if (e.Node == root_node)
						return;
					FillNode (e.Node);
				}
				
				base.OnBeforeExpand (e);
			}
			
			protected override void OnMouseUp (MouseEventArgs e)
			{
				if (SelectedNode == null) {
					parentDialog.okButton.Enabled = false;
					parentDialog.newFolderButton.Enabled = false;
					parentDialog.newFolderMenuItem.Enabled = false;
				} else
				if (!dont_enable) {
					parentDialog.okButton.Enabled = true;
					parentDialog.newFolderButton.Enabled = true;
					parentDialog.newFolderMenuItem.Enabled = true;
				}
				
				base.OnMouseUp (e);
			}
		}
		
		internal class FBTreeNode : TreeNode
		{
			private string realPath = null;
			
			public FBTreeNode (string text)
			{
				Text = text;
			}
			
			public string RealPath {
				set {
					realPath = value;
				}
				
				get {
					return realPath;
				}
			}
		}
	}
}
