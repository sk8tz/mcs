// MainForm.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Mono.Doc.Core;

namespace Mono.Doc.Gui
{
	public class MainForm : Form
	{
		#region Private Instance Fields

		// project
		private DocProject project;
		private string     projectName;

		// main menu / items
		private MainMenu mainMenu;
		private MenuItem menuFile;
		private MenuItem menuFileNew;
		private MenuItem menuFileOpen;
		private MenuItem menuFileClose;
		private MenuItem menuFileSave;
		private MenuItem menuFileSaveAs;
		private MenuItem menuFileSeparator1;
		private MenuItem menuFileRecent;
		private MenuItem menuFileSeparator2;
		private MenuItem menuFileExit;
		private MenuItem menuEdit;
		private MenuItem menuWindow;
		private MenuItem menuWindowCascade;
		private MenuItem menuWindowTile;
		private MenuItem menuWindowTileHorizontal;
		private MenuItem menuHelp;
		private MenuItem menuHelpAbout;

		// tree context menu / items
		private ContextMenu treeProjectMenu;
		private MenuItem    treeMenuProjectAddAssembly;
		private MenuItem    treeMenuProjectAddDirectory;
		private MenuItem    treeMenuProjectSeparator1;
		private MenuItem    treeMenuProjectOptions;
		private ContextMenu treeShortcutMenu;
		private MenuItem    treeMenuShortcutClear;
		private ContextMenu treeDirectoryMenu;
		private MenuItem    treeMenuDirectoryAdd;
		private ContextMenu treeAssemblyMenu;
		private MenuItem    treeMenuAssemblyAdd;

		// status bar
		private StatusBar status;

		// project tree
		private TreeView tree;
		private TreeNode treeProjectRootNode;
		private TreeNode treeShortcutsNode;
		private TreeNode treeDirectoryNode;
		private TreeNode treeAssemblyNode;

		// splitter
		private Splitter verticalSplitter;

		#endregion // Private Instance Fields

		#region Constructors and Destructors

		public MainForm(string projectFile)
		{
			this.project = new DocProject();
			UpdateTitle();

			this.SuspendLayout();

			// this
			this.AutoScaleBaseSize         = new Size(5, 13);
			this.IsMdiContainer            = true;
			this.Name                      = "MainForm";

			// set initial size to 75% of the current screen
			// TODO: this should only happen if we have no size prefs
			// HACK: not sure how best to determine the current screen for multihead users
			Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
			int       x        = (int) (workArea.Width * 0.75);
			int       y        = (int) (workArea.Height * 0.75);
			this.ClientSize    = new Size(x, y);

			// won't completely remove flicker, it but helps
			this.SetStyle(ControlStyles.DoubleBuffer, true);

			// main menu / items
			this.mainMenu                  = new MainMenu();
			this.menuFile                  = new MenuItem();
			this.menuFileNew               = new MenuItem();
			this.menuFileOpen              = new MenuItem();
			this.menuFileClose             = new MenuItem();
			this.menuFileSave              = new MenuItem();
			this.menuFileSaveAs            = new MenuItem();
			this.menuFileSeparator1        = new MenuItem();
			this.menuFileRecent            = new MenuItem();
			this.menuFileSeparator2        = new MenuItem();
			this.menuFileExit              = new MenuItem();
			this.menuEdit                  = new MenuItem();
			this.menuWindow                = new MenuItem();
			this.menuWindowCascade         = new MenuItem();
			this.menuWindowTile            = new MenuItem();
			this.menuWindowTileHorizontal  = new MenuItem();
			this.menuHelp                  = new MenuItem();
			this.menuHelpAbout             = new MenuItem();

			InitializeMainMenu();

			// status bar
			this.status                    = new StatusBar();
			this.status.Text               = "Ready.";

			// project tree
			this.tree                      = new TreeView();
			this.treeAssemblyNode          = new TreeNode();
			this.treeDirectoryNode         = new TreeNode();
			this.treeProjectRootNode       = new TreeNode();
			this.treeShortcutsNode         = new TreeNode();

			InitializeTree();

			// vertical splitter
			// TODO: figure out how to store location in prefs
			this.verticalSplitter          = new Splitter();
			this.verticalSplitter.Name     = "verticalSplitter";
			this.verticalSplitter.TabStop  = false;

			// add components and layout
			this.Menu = this.mainMenu;
			this.Controls.AddRange(new Control[] {
				this.verticalSplitter,
				this.tree,
				this.status
			});

			this.ResumeLayout(false);

			// project tree context menus
			this.treeProjectMenu             = new ContextMenu();
			this.treeMenuProjectAddAssembly  = new MenuItem();
			this.treeMenuProjectAddDirectory = new MenuItem();
			this.treeMenuProjectSeparator1   = new MenuItem();
			this.treeMenuProjectOptions      = new MenuItem();

			this.treeShortcutMenu            = new ContextMenu();
			this.treeMenuShortcutClear       = new MenuItem();

			this.treeDirectoryMenu           = new ContextMenu();
			this.treeMenuDirectoryAdd        = new MenuItem();

			this.treeAssemblyMenu            = new ContextMenu();
			this.treeMenuAssemblyAdd         = new MenuItem();

			InitializeTreeContextMenu();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		#endregion // Constructors and Destructors

		#region Private Instance Methods

		private void DisplayCorrectTreeMenu(Point showAt)
		{
			ContextMenu displayMenu = null;

			if (this.treeProjectRootNode == tree.SelectedNode)
			{
				displayMenu = this.treeProjectMenu;
			}
			else if (this.treeShortcutsNode == tree.SelectedNode)
			{
				displayMenu = this.treeShortcutMenu;
			}
			else if (this.treeDirectoryNode == tree.SelectedNode)
			{
				displayMenu = this.treeDirectoryMenu;
			}
			else if (this.treeAssemblyNode == tree.SelectedNode)
			{
				displayMenu = this.treeAssemblyMenu;
			}

			if (displayMenu != null)
			{
				displayMenu.Show(this.tree, showAt);
			}
		}

		private void Clear()
		{
			project.Clear();
		}

		private void CloseProject()
		{
			if (project.IsModified)
			{
				// TODO: i18n
				DialogResult r = MessageBox.Show(
					"Save changes to " + projectName + "?",
					"Save Modified Project",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1
					);

				switch (r)
				{
					case DialogResult.Yes:
						SaveOrSaveAsProject();
						Clear();
						UpdateTitle();
						InitializeTree();
						break;
					case DialogResult.No:
						Clear();
						UpdateTitle();
						InitializeTree();
						break;
				}
			}
			else
			{
				Clear();
				UpdateTitle();
				InitializeTree();
			}
		}

		private void UpdateTitle()
		{
			string title = GuiResources.GetString("Form.Main.Title");

			if (project.IsNewProject)
			{
				projectName = DocProject.UntitledProjectName;
			}
			else
			{
				projectName = Path.GetFileName(project.FilePath);
				projectName = projectName.Substring(0, projectName.LastIndexOf('.'));
			}

			this.Text = projectName + (project.IsModified ? "*" : "") + " - " + title;
		}

		private void SaveOrSaveAsProject()
		{
			if (project.IsNewProject)
			{
				SaveAsProject();
			}
			else
			{
				SaveProject();
			}
		}

		private void SaveAsProject()
		{
			SaveFileDialog save = new SaveFileDialog();

			if (project.IsNewProject)
			{
				save.FileName =
					"." + 
					Path.DirectorySeparatorChar +
					DocProject.UntitledProjectName +
					".mdproj"; // TODO: abstract constant
			}
			else
			{
				save.FileName = project.FilePath;
			}

			save.Filter = "Monodoc Project Files (*.mdproj)|*.mdproj|All Files (*.*)|*.*"; // TODO: abstract constrant
			save.RestoreDirectory = true;

			if (save.ShowDialog() == DialogResult.OK)
			{
				project.FilePath = save.FileName;
				SaveProject();
				InitializeTree();
			}
		}

		private void SaveProject()
		{
			try
			{
				project.Save();
				UpdateTitle();
			}
			catch (MonodocException mde)
			{
				// TODO: error handling
				MessageBox.Show("MonodocException during project save: " + mde.Message);
			}
			catch (Exception e)
			{
				// TODO: better error handling
				MessageBox.Show("OTHER exception during project open: " + e.Message);
			}
		}

		private void OpenProject(string fileName)
		{
			try
			{
				project.Load(fileName);
				UpdateTitle();
				InitializeTree();
			}
			catch (MonodocException mde)
			{
				// TODO: better error handling
				MessageBox.Show("MonodocException during project open: " + mde.Message);
			}
			catch (Exception e)
			{
				// TODO: better error handling
				MessageBox.Show("OTHER exception during project open: " + e.Message + "\n" + e.StackTrace);
			}
		}

		#endregion // Private Instance Methods

		#region Tree Init

		private void InitializeTree()
		{
			
			// tree
			this.tree.Dock                 = DockStyle.Left;
			this.tree.ImageList            = AssemblyTreeImages.List;
			this.tree.ImageIndex           = 0;
			this.tree.SelectedImageIndex   = 0;
			this.tree.Name                 = "tree";
			this.tree.TabIndex             = 1;
			tree.AfterSelect              += new TreeViewEventHandler(this.tree_AfterSelect);
			tree.MouseUp                  += new MouseEventHandler(this.tree_MouseUp);

			// treeAssemblyNode
			this.treeAssemblyNode.Text               = "Assemblies"; // TODO: i18n
			this.treeAssemblyNode.ImageIndex         = AssemblyTreeImages.AssemblyClosed;
			this.treeAssemblyNode.SelectedImageIndex = AssemblyTreeImages.AssemblyClosed;
			this.treeAssemblyNode.Tag                = "ASSEMBLIES"; // TODO: abstract constant
			
			// treeDirectoryNode
			this.treeDirectoryNode.Text               = "Source Directories"; // TODO: i18n
			this.treeDirectoryNode.ImageIndex         = AssemblyTreeImages.Shortcuts; // TODO: folder image
			this.treeDirectoryNode.SelectedImageIndex = AssemblyTreeImages.Shortcuts;
			this.treeDirectoryNode.Tag                = "DIRECTORIES"; // TODO: abstract constant

			// treeProjectRootNode
			this.treeProjectRootNode.Text               = projectName + " Project"; // TODO: i18n
			this.treeProjectRootNode.ImageIndex         = AssemblyTreeImages.Shortcuts; // TODO: project image
			this.treeProjectRootNode.SelectedImageIndex = AssemblyTreeImages.Shortcuts;
			this.treeProjectRootNode.Tag                = "PROJECT"; // TODO: abstract constant

			// treeShortcutsNode
			this.treeShortcutsNode.Text               = "Shortcuts"; // TODO: i18n
			this.treeShortcutsNode.ImageIndex         = AssemblyTreeImages.Shortcuts;
			this.treeShortcutsNode.SelectedImageIndex = AssemblyTreeImages.Shortcuts;
			this.treeShortcutsNode.Tag                = "SHORTCUTS"; // TODO: abstract constant

			tree.BeginUpdate();

			tree.Nodes.Clear();

			// ugh.  appears necessary to effectively rebuild the tree.
			TreeNode[] nodesToRemove = new TreeNode[] {
														  this.treeAssemblyNode,
														  this.treeDirectoryNode,
														  this.treeProjectRootNode,
														  this.treeShortcutsNode
													  };

			foreach (TreeNode n in nodesToRemove)
			{
				n.Nodes.Clear();

				if (n.Parent != null)
				{
					n.Remove();
				}
			}

			tree.Nodes.Add(this.treeProjectRootNode);

			this.treeProjectRootNode.Nodes.AddRange(
				new TreeNode[] {
								   this.treeShortcutsNode,
								   this.treeDirectoryNode,
								   this.treeAssemblyNode
							   });

			// project xml directories
			foreach (string xmlDir in project.XmlDirectories)
			{
				TreeNode dirNode           = new TreeNode(xmlDir);
				dirNode.ImageIndex         = AssemblyTreeImages.Shortcuts; // TODO: folder image
				dirNode.SelectedImageIndex = AssemblyTreeImages.Shortcuts;
				dirNode.Tag                = "DIRECTORY:" + xmlDir;

				this.treeDirectoryNode.Nodes.Add(dirNode);
			}

			// project assemblies
			try
			{
				foreach (string assemblyFile in project.AssemblyFiles)
				{
					Assembly assem = AssemblyLoader.Load(assemblyFile);
					AssemblyTreeLoader.LoadNode(this.treeAssemblyNode, assem);
				}
			} 
			catch (ApplicationException ae)
			{
				// TODO: better error handling
				MessageBox.Show(ae.Message, "Error Loading Assembly");
			}

			this.treeProjectRootNode.Expand();
			tree.EndUpdate();
		}

		#endregion // Tree Init
		
		#region Tree Events

		private void tree_AfterSelect(object sender, TreeViewEventArgs args)
		{
			this.status.Text = (string) args.Node.Tag;
		}

		private void tree_MouseUp(object sender, MouseEventArgs args)
		{
			if (args.Button == MouseButtons.Right)
			{
				tree.SelectedNode = tree.GetNodeAt(args.X, args.Y);

				if (tree.SelectedNode != null)
				{
					Point menuLoc = this.PointToClient(tree.PointToScreen(new Point(args.X, args.Y)));
					this.status.Text = "display tree menu at " + menuLoc.ToString();
					DisplayCorrectTreeMenu(menuLoc);
				}
			}
		}

		#endregion // Tree Events

		#region Tree Context Menu Init

		private void InitializeTreeContextMenu()
		{
			// treeProjectMenu
			this.treeProjectMenu.MenuItems.AddRange(
				new MenuItem[] {
								   this.treeMenuProjectAddAssembly,
								   this.treeMenuProjectAddDirectory,
								   this.treeMenuProjectSeparator1,
								   this.treeMenuProjectOptions
							   });

			this.treeMenuProjectAddAssembly.Index   = 0;
			this.treeMenuProjectAddAssembly.Text    = "Add Assembly..."; // TODO: i18n
			this.treeMenuProjectAddAssembly.Click  +=
				new EventHandler(this.treeMenuProjectAddAssembly_Click);

			this.treeMenuProjectAddDirectory.Index  = 1;
			this.treeMenuProjectAddDirectory.Text   = "Add Directory..."; // TODO: i18n
			this.treeMenuProjectAddDirectory.Click +=
				new EventHandler(this.treeMenuProjectAddDirectory_Click);

			this.treeMenuProjectSeparator1.Index    = 2;
			this.treeMenuProjectSeparator1.Text     = "-";

			this.treeMenuProjectOptions.Index       = 3;
			this.treeMenuProjectOptions.Text        = "Options..."; // TODO: i18n
			this.treeMenuProjectOptions.Click      +=
				new EventHandler(this.treeMenuProjectOptions_Click);

			// treeShortcutMenu
			this.treeShortcutMenu.MenuItems.AddRange(
				new MenuItem[] {
								   this.treeMenuShortcutClear
							   });

			this.treeMenuShortcutClear.Index  = 0;
			this.treeMenuShortcutClear.Text   = "Clear Shortcuts"; // TODO: i18n
			this.treeMenuShortcutClear.Click += new EventHandler(this.treeMenuShortcutClear_Click);

			// treeDirectoryMenu
			this.treeDirectoryMenu.MenuItems.AddRange(
				new MenuItem[] {
								   this.treeMenuDirectoryAdd
							   });

			this.treeMenuDirectoryAdd.Index  = 0;
			this.treeMenuDirectoryAdd.Text   = "Add Directory..."; // TODO: i18n
			this.treeMenuDirectoryAdd.Click += new EventHandler(this.treeMenuDirectoryAdd_Click);

			// treeAssemblyMenu
			this.treeAssemblyMenu.MenuItems.AddRange(
				new MenuItem[] {
								   this.treeMenuAssemblyAdd
							   });

			this.treeMenuAssemblyAdd.Index  = 0;
			this.treeMenuAssemblyAdd.Text   = "Add Assembly..."; // TODO: i18n
			this.treeMenuAssemblyAdd.Click += new EventHandler(this.treeMenuAssemblyAdd_Click);
		}

		#endregion // Tree Context Menu Init

		#region Tree Context Menu Events

		private void treeMenuProjectAddAssembly_Click(object sender, EventArgs args)
		{
			MessageBox.Show("TODO: add assembly to project");
		}

		private void treeMenuProjectAddDirectory_Click(object sender, EventArgs args)
		{
			MessageBox.Show("TODO: add source directory to project");
		}

		private void treeMenuProjectOptions_Click(object sender, EventArgs args)
		{
			Form options = new ProjectOptionsForm(this.project);
			options.ShowDialog();
			options.Dispose();
		}

		private void treeMenuShortcutClear_Click(object sender, EventArgs args)
		{
			treeShortcutsNode.Nodes.Clear();
		}

		private void treeMenuDirectoryAdd_Click(object sender, EventArgs args)
		{
			this.treeMenuProjectAddDirectory_Click(sender, args);
		}

		private void treeMenuAssemblyAdd_Click(object sender, EventArgs args)
		{
			this.treeMenuProjectAddAssembly_Click(sender, args);
		}

		#endregion // Tree Context Menu Events

		#region Main Menu Init

		private void InitializeMainMenu()
		{
			// main
			this.mainMenu.MenuItems.AddRange(
				new MenuItem[] {
								   this.menuFile,
								   this.menuEdit,
								   this.menuWindow,
								   this.menuHelp
							   });

			// File
			this.menuFile.Index = 0;
			this.menuFile.Text  = "File"; // TODO: i18n
			
			this.menuFile.MenuItems.AddRange(
				new MenuItem[] {
								   this.menuFileNew,
								   this.menuFileOpen,
								   this.menuFileClose,
								   this.menuFileSave,
								   this.menuFileSaveAs,
								   this.menuFileSeparator1,
								   this.menuFileRecent,
								   this.menuFileSeparator2,
								   this.menuFileExit
							   });

			// File|New Project
			this.menuFileNew.Index = 0;
			this.menuFileNew.Text  = "New Project"; // TODO: i18n

			// File|Open Project
			this.menuFileOpen.Index  = 1;
			this.menuFileOpen.Text   = "Open Project..."; // TODO: i18n
			this.menuFileOpen.Click += new EventHandler(this.menuFileOpen_Click);


			// File|Save Project
			this.menuFileSave.Index  = 2;
			this.menuFileSave.Text   = "Save Project"; // TODO: i18n
			this.menuFileSave.Click += new EventHandler(this.menuFileSave_Click);

			// File|Save Project As
			this.menuFileSaveAs.Index  = 3;
			this.menuFileSaveAs.Text   = "Save Project As..."; // TODO: i18n
			this.menuFileSaveAs.Click += new EventHandler(this.menuFileSaveAs_Click);

			// File|Separator1
			this.menuFileSeparator1.Index = 4;
			this.menuFileSeparator1.Text  = "-";

			// File|Recent Projects
			this.menuFileRecent.Index = 5;
			this.menuFileRecent.Text  = "Recent Projects"; // TODO: i18n

			// File|Separator2
			this.menuFileSeparator2.Index = 6;
			this.menuFileSeparator2.Text  = "-";

			// File|Close Project
			this.menuFileClose.Index  = 7;
			this.menuFileClose.Text   = "Close Project"; // TODO: i18n
			this.menuFileClose.Click += new EventHandler(this.menuFileClose_Click);

			// File|Exit
			this.menuFileExit.Index  = 8;
			this.menuFileExit.Text   = "Exit"; // TODO: i18n
			this.menuFileExit.Click += new EventHandler(this.menuFileExit_Click);

			// Edit
			this.menuEdit.Index = 1;
			this.menuEdit.Text  = "Edit"; // TODO: i18n

			// Window
			this.menuWindow.Index   = 2;
			this.menuWindow.Text    = "Window"; // TODO: i18n
			this.menuWindow.MdiList = true;

			this.menuWindow.MenuItems.AddRange(
				new MenuItem[] {
								   this.menuWindowCascade,
								   this.menuWindowTile,
								   this.menuWindowTileHorizontal
							   });

			// Window|Cascade
			this.menuWindowCascade.Index  = 0;
			this.menuWindowCascade.Text   = "Cascade"; // TODO: i18n
			this.menuWindowCascade.Click += new EventHandler(this.menuWindowCascade_Click);

			// Window|Tile
			this.menuWindowTile.Index  = 1;
			this.menuWindowTile.Text   = "Tile"; // TODO: i18n
			this.menuWindowTile.Click += new EventHandler(this.menuWindowTile_Click);
			
			// Window|Tile Horizontal
			this.menuWindowTileHorizontal.Index  = 2;
			this.menuWindowTileHorizontal.Text   = "Tile Horizontal"; // TODO: i18n
			this.menuWindowTileHorizontal.Click += new EventHandler(this.menuWindowTileHorizontal_Click);

			// Help
			this.menuHelp.Index   = 3;
			this.menuHelp.Text    = "Help"; // TODO: i18n

			this.menuHelp.MenuItems.AddRange(
				new MenuItem[] {
								   this.menuHelpAbout
							   });

			// Help|About
			this.menuHelpAbout.Index  = 0;
			this.menuHelpAbout.Text   = "About..."; // TODO: i18n
			this.menuHelpAbout.Click += new EventHandler(this.menuHelpAbout_Click);
		}

		#endregion // Main Menu Init
		
		#region Main Menu Events

		private void menuFileOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();
			open.Filter         = "Monodoc Project Files (*.mdproj)|*.mdproj| All Files (*.*)|*.*";

			if (open.ShowDialog() == DialogResult.OK)
			{
				OpenProject(open.FileName);
			}
		}

		private void menuFileClose_Click(object sender, EventArgs e)
		{
			CloseProject();
		}

		private void menuFileSave_Click(object sender, EventArgs e)
		{
			if (project.IsNewProject || project.IsModified)
			{
				SaveOrSaveAsProject();
			}
		}

		private void menuFileSaveAs_Click(object sender, EventArgs e)
		{
			SaveAsProject();
		}

		private void menuFileExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void menuWindowCascade_Click(object sender, EventArgs e)
		{
			this.LayoutMdi(MdiLayout.Cascade);
		}

		private void menuWindowTile_Click(object sender, EventArgs e)
		{
			this.LayoutMdi(MdiLayout.TileVertical);
		}

		private void menuWindowTileHorizontal_Click(object sender, EventArgs e)
		{
			this.LayoutMdi(MdiLayout.TileHorizontal);
		}

		private void menuHelpAbout_Click(object sender, EventArgs e)
		{
			Form aboutForm = new AboutForm();
			aboutForm.ShowDialog();
		}

		#endregion // Main Menu Events
	}
}
