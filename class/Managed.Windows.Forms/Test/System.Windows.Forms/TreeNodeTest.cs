using System;
using NUnit.Framework;
using System.Windows.Forms;
using SystemDrawingNamespace = System.Drawing;

namespace MonoTests.System.Windows.Forms {

	
	[TestFixture]
	public class TreeNodeTest {

		[Test]
		public void EmptyCtorTest ()
		{
			TreeNode tn = new TreeNode ();
			Assert.AreEqual ("", tn.Text, "#1");
			Assert.AreEqual (0, tn.Nodes.Count, "#2");
			Assert.AreEqual (-1, tn.ImageIndex, "#3");
			Assert.AreEqual (-1, tn.SelectedImageIndex, "#4");

			// Set simple properties
			tn.Text = null;
			Assert.AreEqual ("", tn.Text, "#5");
			tn.ImageIndex = 67;
			Assert.AreEqual (67, tn.ImageIndex, "#6");
			tn.SelectedImageIndex = 99;
			Assert.AreEqual (99, tn.SelectedImageIndex, "#7");
		}

		[Test]
		public void CtorTest () {
			TreeNode tn = new TreeNode ("label1");
		
			Assert.AreEqual ("label1", tn.Text);
			Assert.AreEqual (0, tn.Nodes.Count);
			Assert.AreEqual (-1, tn.ImageIndex, "II");
			Assert.AreEqual (-1, tn.SelectedImageIndex, "SI");

			Assert.IsNull (tn.FirstNode);
			Assert.IsNull (tn.LastNode);
			Assert.AreEqual ("", new TreeNode (null).Text);
		}

		[Test]
		public void CtorTest2 ()
		{
			TreeNode tn = new TreeNode ("a1", new TreeNode[] { new TreeNode ("aa1"), new TreeNode ("aa2") } );

			Assert.AreEqual ("a1", tn.Text);
			Assert.AreEqual (-1, tn.ImageIndex, "II");
			Assert.AreEqual (-1, tn.SelectedImageIndex, "SI");

			Assert.AreEqual ("aa1", tn.Nodes [0].Text, "#1");
			Assert.AreEqual ("aa2", tn.Nodes [1].Text, "#2");
			Assert.AreSame (tn.FirstNode, tn.Nodes [0], "#3");
			Assert.AreSame (tn.LastNode, tn.Nodes [1], "#4");
		}

		[Test]
		public void CtorTest3 ()
		{
			TreeNode tn = new TreeNode ("a", 5, 9);

			Assert.AreEqual ("a", tn.Text);
			Assert.IsNotNull (tn.Nodes);
			Assert.AreEqual (5, tn.ImageIndex);
			Assert.AreEqual (9, tn.SelectedImageIndex);
			Assert.AreEqual ("", new TreeNode (null, 0, 0).Text);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void CtorException1 ()
		{
			new TreeNode ("", 1, 1, null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void CtorException2 () {
			new TreeNode ("tt", null);
		}

		[Test]
		public void Traverse ()
		{
			TreeNode tn_1 = new TreeNode ("1");
			TreeNode tn_2 = new TreeNode ("2");
			TreeNode tn_3 = new TreeNode ("3");
			TreeNode tn = new TreeNode ("lev1");
			tn.Nodes.Add (tn_1);
			Assert.AreSame (tn, tn_1.Parent, "#1");
			Assert.IsNull (tn_1.NextNode, "#2");
			Assert.AreEqual (0, tn_1.Parent.Index, "#3");
			tn.Nodes.Add (tn_2);
			Assert.IsNull (tn_1.NextNode.NextNode, "#33");
			tn.Nodes.Add (tn_3);
			Assert.AreEqual (2, tn_3.Index, "#4");

			Assert.AreEqual (3, tn.Nodes.Count, "#5");
			Assert.AreSame (tn_2, tn_2.NextNode.PrevNode, "#6");
			Assert.IsNull (tn_1.PrevNode, "#7");
		}

		[Test]
		public void ExpandCollapseLeafTest ()
		{
			// Leaf nodes should keep its expanded state
			TreeNode tn_1 = new TreeNode ();
			Assert.IsFalse (tn_1.IsExpanded, "#1");

			tn_1.Expand ();
			Assert.IsTrue (tn_1.IsExpanded, "#2");
		}

		[Test]
		public void FullPathException ()
		{
			try {
				string s = new TreeNode ("").FullPath;
				Assert.Fail ("#1");
				// Prevent CS0219, will never write anything
				// due to previous statement throwing an
				// exception
				Console.WriteLine (s);
#if NET_2_0
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
#else
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
#endif
		}

		[Test]
		public void FullPathTest ()
		{
			TreeNode tn_1 = new TreeNode ("A");
			TreeNode tn_2 = new TreeNode ("B");
			tn_2.Nodes.Add (tn_1);

			TreeView tv = new TreeView ();
			tv.Nodes.Add (tn_1);
			tv.Nodes [0].Nodes.Add (tn_2);

			Assert.AreEqual ("A", tn_1.FullPath, "#1");
			Assert.AreEqual ("A", tv.Nodes[0].FullPath, "#2");
			Assert.AreEqual (@"A\B", tn_2.FullPath, "#3");
			tv.PathSeparator = "_separator_";
			Assert.AreEqual ("A_separator_B", tn_2.FullPath, "#4");
		}

		[Test]
		public void CloneTest ()
		{
			TreeNode orig = new TreeNode ("text", 2, 3, new TreeNode [] { new TreeNode ("child", 22, 33) });
			orig.Tag = FlatStyle.Flat;
			orig.Checked = true;
			orig.BackColor = SystemDrawingNamespace.Color.AliceBlue;
			orig.ForeColor = SystemDrawingNamespace.Color.Beige;

			TreeNode clone = (TreeNode)orig.Clone ();
			Assert.AreEqual ("text", clone.Text, "#1");
			Assert.AreEqual (2, clone.ImageIndex, "#2");
			Assert.AreEqual (3, clone.SelectedImageIndex, "#3");
			Assert.AreEqual (1, clone.Nodes.Count, "#4");
			Assert.AreEqual (FlatStyle.Flat, clone.Tag, "#5");
			Assert.IsTrue (clone.Checked, "#6");
			Assert.AreEqual ("child", clone.Nodes [0].Text, "#10");
			Assert.AreEqual (22, clone.Nodes [0].ImageIndex, "#11");
			Assert.AreEqual (SystemDrawingNamespace.Color.AliceBlue, clone.BackColor, "#12");
			Assert.AreEqual (SystemDrawingNamespace.Color.Beige, clone.ForeColor, "#13");
		}

		[Test]
		public void SingleNodeIndexTest ()
		{
			TreeNode tn_1 = new TreeNode ("A");
			Assert.AreEqual (0, tn_1.Index, "#1");
			TreeView tv = new TreeView ();
			tv.Nodes.Add (tn_1);
			Assert.AreEqual (0, tn_1.Index, "#2");
		}

		[Test]
		public void EndEditTest ()
		{
			TreeNode node1 = new TreeNode ("A");
			TreeNode node2 = new TreeNode ("B");

			Form f = new Form ();
			TreeView tv = new TreeView ();
			tv.LabelEdit = true;
			tv.Parent = f;
			tv.Nodes.Add (node1);
			tv.Nodes.Add (node2);

			f.Show ();

			// EndEdit called on a different node
			node1.BeginEdit ();
			Assert.AreEqual (true, node1.IsEditing, "#1");
			node2.EndEdit (false);
			Assert.AreEqual (false, node1.IsEditing, "#2");

			node1.BeginEdit ();
			Assert.AreEqual (true, node1.IsEditing, "#3");
			node2.EndEdit (true);
			Assert.AreEqual (false, node1.IsEditing, "#4");

			f.Dispose ();
		}
		
#if NET_2_0
		[Test]
		public void PropertyName ()
		{
			TreeNode tn = new TreeNode ();
			
			Assert.AreEqual (string.Empty, tn.Name, "A1");
			
			tn.Name = "Monkey";
			Assert.AreEqual ("Monkey", tn.Name, "A2");
	
			tn.Name = null;
			Assert.AreEqual (string.Empty, tn.Name, "A3");
		}
#endif
	}
}
