// monodoc.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;
	using System.IO;
	using System.Text;
	using System.Reflection;
	using System.Collections;
	using Mono.Document.Library;

	[DeclareQtSignal ("Load (String)")]
	public class MonoDoc : QMainWindow {

		private DocEdit docedit;
		private ListView listview;
		private QMenuBar menubar;
		private QPopupMenu filemenu;
		private QPopupMenu settingsmenu;
		private QPopupMenu aboutmenu;
		private Options options;

		public static int Main (String[] args)
		{
			QApplication app = new QApplication (args);
			MonoDoc monodoc = new MonoDoc ();
			monodoc.Show ();
			app.SetMainWidget (monodoc);
			return app.Exec ();
		}

		public MonoDoc () : base (null)
		{
			filemenu = new QPopupMenu (null, "filemenu");
			filemenu.InsertItem ("&Open", this, SLOT ("OpenFile ()"));
			filemenu.InsertItem ("&Quit", qApp, SLOT ("Quit ()"));

			settingsmenu = new QPopupMenu (null, "settingsmenu");
			settingsmenu.InsertItem ("&Configure MonoDoc...", this, SLOT ("Options ()"));

			aboutmenu = new QPopupMenu (null, "helpmenu");
			aboutmenu.InsertItem ("&About MonoDoc", this, SLOT ("AboutMonoDoc ()"));

			menubar = new QMenuBar (this, "");
			menubar.InsertItem ("&File", filemenu);
			menubar.InsertItem ("&Settings", settingsmenu);
			menubar.InsertItem ("&About", aboutmenu);

			QSplitter split = new QSplitter (this);
			listview = new ListView (split);
			docedit = new DocEdit (split);
			split.SetOpaqueResize (true);
			split.SetResizeMode (listview, QSplitter.ResizeMode.FollowSizeHint);
			split.SetResizeMode (docedit, QSplitter.ResizeMode.FollowSizeHint);

			Connect (this, SIGNAL ("Load (String)"), listview, SLOT ("LoadFile (String)"));
			Connect (	listview, SIGNAL ("selectionChanged (QListViewItem)"),
						docedit, SLOT ("ListViewChanged (QListViewItem)"));

			options = new Options (this);
			Connect (options, SIGNAL ("OkClicked ()"), this, SLOT ("WriteInit ()"));

			SetCentralWidget (split);
			Configure ();
		}

		public void OpenFile ()
		{
			string filename = QFileDialog.GetOpenFileName (Global.InitDir, "*.xml", this, "open", "Open Master File", "*.xml", true);

			if (filename != null) {
				Global.LastOpened = filename;
				Global.InitDir = GetDir (filename);
				WriteInit ();
				Emit ("Load (String)", filename);
			}
		}

		public void AboutMonoDoc ()
		{
			QMessageBox.Information (this, "About MonoDoc", "MonoDoc - The Mono Documentation Editor");
		}

		private void Configure ()
		{
			SetGeometry (50, 500, 800, 600);
			SetCaption ("MonoDoc -  The Mono Documentation Editor");
			SetIcon (Global.IMono);

			if (!Directory.Exists (Global.MonoDocDir))
				Directory.CreateDirectory (Global.MonoDocDir);

			if (File.Exists (Global.MonoDocDir+"/monodoc")) {
				StreamReader st = File.OpenText (Global.MonoDocDir+"/monodoc");
				Global.InitDir = st.ReadLine ();
				Global.LastOpened = st.ReadLine ();
				if (Global.LastOpened != null) {
					options.IsChecked (true);
					Emit ("Load (String)", Global.LastOpened);
				}
			}
		}

		public void WriteInit ()
		{
			StreamWriter st = File.CreateText (Global.MonoDocDir+"/monodoc");
			st.WriteLine (Global.InitDir);
			if (options.OpenPrevious)
				st.WriteLine (Global.LastOpened);
			st.Flush ();
		}
		
		public string GetDir (string filename)
		{
			StringBuilder builder = new StringBuilder ();
			string [] s = filename.Split ('/');
			for (int i = 0; i < s.Length - 1; i++) {
				builder.Append (s[i]+"/");
			}
			return builder.ToString ().TrimEnd ('/');
		}

		public void Options ()
		{
			options.Show ();
		}

		private class ListView : QListView {

			QListViewItem _namespace;
			Progress progress;

			public ListView (QWidget parent) : base (parent)
			{
				SetRootIsDecorated (true);
				AddColumn ("Namespace");
				SetSorting (-1);
				progress = new Progress (this);
            }

			public void LoadFile (string filename)
			{
				if(File.Exists (filename)) {
					progress.Show ();
					DocParser parser = new DocParser (filename);
					progress.SetTotalSteps (parser.DocTypes.Count - 1);
					progress.SetLabelText ("Loading Master.xml file");
					SetUpdatesEnabled (false);
					int i = 0;
					foreach(DocType type in parser.DocTypes) {
						type.FileRoot = Global.InitDir;
						type.Language = "en";
						progress.SetProgress (i++);
						ListItem typeitem = new ListItem (GetNamespaceItem (type.Namespace), type.Name, type);
						ProcessMember (type.Dtors, typeitem);
						ProcessMember (type.Events, typeitem);
						ProcessMember (type.Fields, typeitem);
						ProcessMember (type.Properties, typeitem);
						ProcessMember (type.Methods, typeitem);
						ProcessMember (type.Ctors, typeitem);
					}
					SetUpdatesEnabled (true);
            		Repaint ();
				}
			}

			public void ProcessMember (ArrayList array, QListViewItem parent)
			{
				foreach (DocMember member in array) {
					if (parent != null && member != null) {
						new ListItem (parent, member.FullName, member);
					}
				}
			}

			public QListViewItem GetNamespaceItem (string name)
			{
				_namespace = FindItem (name, 0);
				if (_namespace == null) {
					SetUpdatesEnabled (true);
					_namespace = new ListItem (this, name);
            		Repaint ();
					SetUpdatesEnabled (false);
				}
				return _namespace;
			}
		}

		private class DocEdit : QWidgetStack {

			public DocEdit (QWidget parent) : base (parent)
			{
				SetMargin (10);
			}

			public void ListViewChanged (QListViewItem item)
			{
				ListItem listItem = item as ListItem;
				
				if (listItem.IsNamespace)
					return;
				if (listItem != null && !listItem.IsBuilt)
					AddWidget (listItem.BuildEditForm () as QWidget);

				IEditForm edit = VisibleWidget () as IEditForm;
				if (edit != null)
					edit.Flush ();

				listItem.EditForm.Sync ();
				RaiseWidget (listItem.EditForm as QWidget);
			}
		}

		private class ListItem : QListViewItem {

			DocType type;
			DocMember member;
			public bool IsBuilt, IsNamespace = false;
			public IEditForm EditForm;

			public ListItem (QListView parent, string text) : base (parent, text)
			{
				IsNamespace = true;
				SetPixmap (0, Global.IName);
			}

			public ListItem (QListViewItem parent, string text, DocMember member) : base (parent, text)
			{
				this.member = member;
				if (member.IsCtor || member.IsMethod || member.IsDtor)
					SetPixmap (0, Global.IMethod);
				else if (member.IsField)
					SetPixmap (0, Global.IField);
				else if (member.IsProperty)
					SetPixmap (0, Global.IProperty);
				else if (member.IsEvent)
					SetPixmap (0, Global.IEvent);

			}

			public ListItem (QListViewItem parent, string text, DocType type) : base (parent, text)
			{
				this.type = type;
				if (type.IsClass)
					SetPixmap (0, Global.IClass);
				else if (type.IsStructure)
					SetPixmap (0, Global.IStructure);
				else if (type.IsInterface)
					SetPixmap (0, Global.IInterface);
				else if (type.IsDelegate)
					SetPixmap (0, Global.IDelegate);
				else if (type.IsEnum)
					SetPixmap (0, Global.IEnum);
			}

			public IEditForm BuildEditForm ()
			{
				if (type != null)
					EditForm = new TypeEdit (type);
				else if (member != null)
					EditForm = new ParamEdit (member);
				IsBuilt = true;
				return EditForm;
			}
		}

		[DeclareQtSignal ("Sync ()")]
		[DeclareQtSignal ("Flush ()")]
		private class TypeEdit : QVGroupBox, IEditForm {

			DocType document;

			public TypeEdit (DocType document) : base (document.Name)
			{
				this.document = document;
				SetInsideMargin (20);
				
				SummaryForm sum = new SummaryForm (document, this);
				Connect (this, SIGNAL ("Sync ()"), sum, SLOT ("OnSync ()"));
				Connect (this, SIGNAL ("Flush ()"), sum, SLOT ("OnFlush ()"));

				RemarksForm rem = new RemarksForm (document, this);
				Connect (this, SIGNAL ("Sync ()"), rem, SLOT ("OnSync ()"));
				Connect (this, SIGNAL ("Flush ()"), rem, SLOT ("OnFlush ()"));
			}

			public void Sync ()
			{
				if (!File.Exists (document.FilePath))
					return;
				DocParser.Parse (document);
				Emit ("Sync ()", null);
				//Console.WriteLine ("Found doc for: "+document.Name);
			}

			public void Flush ()
			{
				Emit ("Flush ()", null);
				DocArchiver.Archive (document);
				//Console.WriteLine ("Wrote doc for:"+document.Name);
			}
		}

		private class SummaryForm : QVBox {
			QLineEdit edit;
			DocType document;
			public SummaryForm (DocType document, QWidget parent) : base (parent)
			{
				new QLabel (Global.Summary, this);
				edit = new QLineEdit (this);
				this.document = document;
			}
			public void OnSync ()
			{
				edit.SetText (document.Summary);
			}
			public void OnFlush ()
			{
				document.Summary = edit.Text ();
			}
		}

		private class RemarksForm : QVBox {
			QTextEdit edit;
			DocType document;
			public RemarksForm (DocType document, QWidget parent) : base (parent)
			{
				new QLabel (Global.Remarks, this);
				edit = new QTextEdit (this);
				this.document = document;
			}
			public void OnSync ()
			{
				edit.SetText (document.Remarks);
			}
			public void OnFlush ()
			{
				document.Remarks = edit.Text ();
			}
		}

		private class ParamEdit : QVGroupBox, IEditForm {

			DocMember member;

			public ParamEdit (DocMember member) : base (member.FullName)
			{
				this.member = member;
				foreach (DocParam param in member.Params)
				{
					QHBox hbox = new QHBox (this);
					QLabel label = new QLabel (hbox);
					label.SetText (param.Name);
					QLineEdit edit = new QLineEdit (hbox);
				}
			}

			public void Sync ()
			{
				Console.WriteLine (member.FullName);
			}

			public void Flush ()
			{
				Console.WriteLine ("Flush IO");
			}
		}

		private class Progress : QProgressDialog {

			QPushButton pb;

			public Progress (QWidget parent) : base (parent, "", true)
			{
				SetLabelText ("Parsing Master.xml file");
				SetTotalSteps (500);
				pb = new QPushButton (null);
				SetCancelButton (pb);
				pb.Hide ();
			}

			new public void Show ()
			{
				ForceShow ();
				SetProgress (1);
			}
		}

		[DeclareQtSignal ("OkClicked ()")]
		private class Options : QDialog {

			QCheckBox openPrev;

			public Options (QWidget parent) : base (parent, "options", true)
			{
				SetCaption ("Configure MonoDoc");
				SetIcon (Global.IMono);
				openPrev = new QCheckBox (this);
				openPrev.SetText ("Open previous Master.xml file upon startup");
				QPushButton ok = new QPushButton ("OK", this);
				QPushButton cancel = new QPushButton ("Cancel", this);

				Connect (ok, QtSupport.SIGNAL("clicked()"), this, QtSupport.SLOT("OkClicked()"));
				Connect (cancel, QtSupport.SIGNAL("clicked()"), this, SLOT("CancelClicked()"));

				QVBoxLayout dialogLayout = new QVBoxLayout (this);

				QHBoxLayout mainLayout = new QHBoxLayout (dialogLayout);
				mainLayout.AddWidget (openPrev);
			
				QHBoxLayout actionLayout = new QHBoxLayout (dialogLayout);
				actionLayout.AddWidget (ok);
				actionLayout.AddWidget (cancel);
			}

			public void CancelClicked ()
			{
				Reject ();
			}

			public void OkClicked ()
			{
				Emit ("OkClicked ()", null);
				Accept ();
			}
		
			public bool OpenPrevious
			{
				get { return openPrev.IsOn (); }
			}
			
			public void IsChecked (bool value)
			{
				openPrev.SetChecked (value);
			}
		}
	}
	
	public interface IEditForm {
			void Sync ();
			void Flush ();
	}
}
