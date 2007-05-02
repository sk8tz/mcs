//
// System.Collections.Generic.RBTree
//
// Authors:
//   Raja R Harinath <rharinath@novell.com>
//

//
// Copyright (C) 2007, Novell, Inc.
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

#define ONE_MEMBER_CACHE

#if NET_2_0
using System;
using System.Collections;

namespace System.Collections.Generic
{
	internal class RBTree : IEnumerable, IEnumerable<RBTree.Node> {
		public interface INodeComparer<T> {
			int Compare (T value, Node node);
		}

		public abstract class Node {
			public Node left, right;
			uint size_black;

			const uint black_mask = 1;
			const int black_shift = 1;
			public bool IsBlack {
				get { return (size_black & black_mask) == black_mask; }
				set { size_black = value ? (size_black | black_mask) : (size_black & ~black_mask); }
			}

			public uint Size {
				get { return size_black >> black_shift; }
				set { size_black = (value << black_shift) | (size_black & black_mask); }
			}

			public uint FixSize ()
			{
				Size = 1;
				if (left != null)
					Size += left.Size;
				if (right != null)
					Size += right.Size;
				return Size;
			}

			public Node ()
			{
				size_black = 2; // Size == 1, IsBlack = false
			}

			public abstract void SwapValue (Node other);

#if TEST
			public int VerifyInvariants ()
			{
				int black_depth_l = 0;
				int black_depth_r = 0;
				uint size = 1;
				bool child_is_red = false;
				if (left != null) {
					black_depth_l = left.VerifyInvariants ();
					size += left.Size;
					child_is_red |= !left.IsBlack;
				}

				if (right != null) {
					black_depth_r = right.VerifyInvariants ();
					size += right.Size;
					child_is_red |= !right.IsBlack;
				}

				if (black_depth_l != black_depth_r)
					throw new SystemException ("Internal error: black depth mismatch");

				if (!IsBlack && child_is_red)
					throw new SystemException ("Internal error: red-red conflict");
				if (Size != size)
					throw new SystemException ("Internal error: metadata error");

				return black_depth_l + (IsBlack ? 1 : 0);
			}

			public abstract void Dump (string indent);
#endif
		}

		Node root;
		object cmp;
		uint version;

#if ONE_MEMBER_CACHE
		[ThreadStatic]
		static List<Node> cached_path;

		static List<Node> alloc_path ()
		{
			if (cached_path == null)
				return new List<Node> ();

			List<Node> path = cached_path;
			cached_path = null;
			return path;
		}

		static void release_path (List<Node> path)
		{
			if (cached_path == null || cached_path.Capacity < path.Capacity) {
				path.Clear ();
				cached_path = path;
			}
		}
#else
		static List<Node> alloc_path ()
		{
			return new List<Node> ();
		}

		static void release_path (List<Node> path)
		{
		}
#endif

		public RBTree (object cmp)
		{
			// cmp is INodeComparer<T> for some T
			this.cmp = cmp;
		}

		public void Clear ()
		{
			root = null;
			++version;
		}

		// if key is already in the tree, return the node associated with it
		// if not, insert new_node into the tree, and return it
		public Node Intern<T> (T key, Node new_node)
		{
			if (root == null) {
				root = new_node;
				root.IsBlack = true;
				++version;
				return root;
			}

			List<Node> path = alloc_path ();
			int in_tree_cmp = find_key (key, path);
			Node retval = in_tree_cmp == 0 ? path [path.Count - 1] : do_insert (in_tree_cmp, new_node, path);
			// no need for a try .. finally, this is only used to mitigate allocations
			release_path (path);
			return retval;
		}

		// returns the just-removed node (or null if the value wasn't in the tree)
		public Node Remove<T> (T key)
		{
			if (root == null)
				return null;

			List<Node> path = alloc_path ();
			int in_tree_cmp = find_key (key, path);
			Node retval = in_tree_cmp == 0 ? do_remove (path) : null;
			// no need for a try .. finally, this is only used to mitigate allocations
			release_path (path);
			return retval;
		}

		public Node Lookup<T> (T key)
		{
			INodeComparer<T> cmp = (INodeComparer<T>) this.cmp;
			Node current = root;
			while (current != null) {
				int c = cmp.Compare (key, current);
				if (c == 0)
					break;
				current = c < 0 ? current.left : current.right;
			}
			return current;
		}

		public int Count {
			get { return root == null ? 0 : (int) root.Size; }
		}

		public Node this [int index] {
			get {
				if (index < 0 || index >= Count)
					throw new IndexOutOfRangeException ("index");

				Node current = root;
				while (current.Size > 1) {
					int left_size = current.left == null ? 0 : (int) current.left.Size;
					if (index == left_size)
						return current;
					if (index < left_size) {
						current = current.left;
					} else {
						index -= left_size + 1;
						current = current.right;
					}
				}

				if (index != 0)
					throw new SystemException ("Internal Error: index calculation");
				return current;
			}
		}

		public NodeEnumerator GetEnumerator ()
		{
			return new NodeEnumerator (this);
		}

		IEnumerator<Node> IEnumerable<Node>.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

#if TEST
		public void VerifyInvariants ()
		{
			if (root != null) {
				if (!root.IsBlack)
					throw new SystemException ("Internal Error: root is not black");
				root.VerifyInvariants ();
			}
		}

		public void Dump ()
		{
			if (root != null)
				root.Dump ("");
		}
#endif

		// Pre-condition: root != null
		int find_key<T> (T key, List<Node> path)
		{
			INodeComparer<T> cmp = (INodeComparer<T>) this.cmp;
			int c = 0;
			Node sibling = null;
			Node current = root;

			if (path != null)
				path.Add (root);

			while (current != null) {
				c = cmp.Compare (key, current);
				if (c == 0)
					return c;

				if (c < 0) {
					sibling = current.right;
					current = current.left;
				} else {
					sibling = current.left;
					current = current.right;
				}

				if (path != null) {
					path.Add (sibling);
					path.Add (current);
				}
			}

			return c;
		}

		Node do_insert (int in_tree_cmp, Node current, List<Node> path)
		{
			path [path.Count - 1] = current;
			Node parent = path [path.Count - 3];

			if (in_tree_cmp < 0)
				parent.left = current;
			else
				parent.right = current;
			for (int i = 0; i < path.Count - 2; i += 2)
				++ path [i].Size;

			if (!parent.IsBlack)
				rebalance_insert (path);

			if (!root.IsBlack)
				throw new SystemException ("Internal error: root is not black");

			++version;
			return current;
		}

		Node do_remove (List<Node> path)
		{
			int curpos = path.Count - 1;

			Node current = path [curpos];
			if (current.left != null) {
				Node pred = right_most (current.left, current.right, path);
				current.SwapValue (pred);
				if (pred.left != null) {
					Node ppred = pred.left;
					path.Add (null); path.Add (ppred);
					pred.SwapValue (ppred);
				}
			} else if (current.right != null) {
				Node succ = current.right;
				path.Add (null); path.Add (succ);
				current.SwapValue (succ);
			}

			curpos = path.Count - 1;
			current = path [curpos];

			if (current.Size != 1)
				throw new SystemException ("Internal Error: red-black violation somewhere");

			// remove it from our data structures
			path [curpos] = null;
			node_reparent (curpos == 0 ? null : path [curpos-2], current, 0, null);

			for (int i = 0; i < path.Count - 2; i += 2)
				-- path [i].Size;

			if (curpos != 0 && current.IsBlack)
				rebalance_delete (path);

			// no need for a try .. finally, this is only used to mitigate allocations/gc
			release_path (path);

			if (root != null && !root.IsBlack)
				throw new SystemException ("Internal Error: root is not black");

			++version;
			return current;
		}

		// Pre-condition: current is red
		void rebalance_insert (List<Node> path)
		{
			int curpos = path.Count - 1;
			do {
				// parent == curpos-2, uncle == curpos-3, grandpa == curpos-4
				if (path [curpos-3] == null || path [curpos-3].IsBlack) {
					rebalance_insert__rotate_final (curpos, path);
					return;
				}

				path [curpos-2].IsBlack = path [curpos-3].IsBlack = true;

				curpos -= 4; // move to the grandpa

				if (curpos == 0) // => current == root
					return;
				path [curpos].IsBlack = false;
			} while (!path [curpos-2].IsBlack);
		}

		// Pre-condition: current is black
		void rebalance_delete (List<Node> path)
		{
			int curpos = path.Count - 1;
			do {
				Node sibling = path [curpos-1];
				// current is black => sibling != null
				if (!sibling.IsBlack) {
					// current is black && sibling is red 
					// => both sibling.left and sibling.right are black, and are not null
					curpos = ensure_sibling_black (curpos, path);
					// one of the nephews became the new sibling -- in either case, sibling != null
					sibling = path [curpos-1];
				}

				if ((sibling.left != null && !sibling.left.IsBlack) ||
				    (sibling.right != null && !sibling.right.IsBlack)) {
					rebalance_delete__rotate_final (curpos, path);
					return;
				}

				sibling.IsBlack = false;

				curpos -= 2; // move to the parent

				if (curpos == 0)
					return;
			} while (path [curpos].IsBlack);
			path [curpos].IsBlack = true;
		}

		void rebalance_insert__rotate_final (int curpos, List<Node> path)
		{
			Node current = path [curpos];
			Node parent = path [curpos-2];
			Node grandpa = path [curpos-4];

			uint grandpa_size = grandpa.Size;

			Node new_root;

			bool l1 = parent == grandpa.left;
			bool l2 = current == parent.left;
			if (l1 && l2) {
				grandpa.left = parent.right; parent.right = grandpa;
				new_root = parent;
			} else if (l1 && !l2) {
				grandpa.left = current.right; current.right = grandpa;
				parent.right = current.left; current.left = parent;
				new_root = current;
			} else if (!l1 && l2) {
				grandpa.right = current.left; current.left = grandpa;
				parent.left = current.right; current.right = parent;
				new_root = current;
			} else { // (!l1 && !l2)
				grandpa.right = parent.left; parent.left = grandpa;
				new_root = parent;
			}

			grandpa.FixSize (); grandpa.IsBlack = false;
			if (new_root != parent)
				parent.FixSize (); /* parent is red already, so no need to set it */

			new_root.IsBlack = true;
			node_reparent (curpos == 4 ? null : path [curpos-6], grandpa, grandpa_size, new_root);
		}

		// Pre-condition: sibling is black, and one of sibling.left and sibling.right is red
		void rebalance_delete__rotate_final (int curpos, List<Node> path)
		{
			//Node current = path [curpos];
			Node sibling = path [curpos-1];
			Node parent = path [curpos-2];

			uint parent_size = parent.Size;
			bool parent_was_black = parent.IsBlack;

			Node new_root;
			if (parent.right == sibling) {
				// if far nephew is black
				if (sibling.right == null || sibling.right.IsBlack) {
					// => near nephew is red, move it up
					Node nephew = sibling.left;
					parent.right = nephew.left; nephew.left = parent;
					sibling.left = nephew.right; nephew.right = sibling;
					new_root = nephew;
				} else {
					parent.right = sibling.left; sibling.left = parent;
					sibling.right.IsBlack = true;
					new_root = sibling;
				}
			} else {
				// if far nephew is black
				if (sibling.left == null || sibling.left.IsBlack) {
					// => near nephew is red, move it up
					Node nephew = sibling.right;
					parent.left = nephew.right; nephew.right = parent;
					sibling.right = nephew.left; nephew.left = sibling;
					new_root = nephew;
				} else {
					parent.left = sibling.right; sibling.right = parent;
					sibling.left.IsBlack = true;
					new_root = sibling;
				}
			}

			parent.FixSize (); parent.IsBlack = true;
			if (new_root != sibling)
				sibling.FixSize (); /* sibling is already black, so no need to set it */

			new_root.IsBlack = parent_was_black;
			node_reparent (curpos == 2 ? null : path [curpos-4], parent, parent_size, new_root);
		}

		// Pre-condition: sibling is red (=> parent, sibling.left and sibling.right are black)
		int ensure_sibling_black (int curpos, List<Node> path)
		{
			Node current = path [curpos];
			Node sibling = path [curpos-1];
			Node parent = path [curpos-2];

			bool current_on_left;
			uint parent_size = parent.Size;

			if (parent.right == sibling) {
				parent.right = sibling.left; sibling.left = parent;
				current_on_left = true;
			} else {
				parent.left = sibling.right; sibling.right = parent;
				current_on_left = false;
			}

			parent.FixSize (); parent.IsBlack = false;

			sibling.IsBlack = true;
			node_reparent (curpos == 2 ? null : path [curpos-4], parent, parent_size, sibling);

			// accomodate the rotation
			if (curpos+1 == path.Count) {
				path.Add (null);
				path.Add (null);
			}

			path [curpos-2] = sibling;
			path [curpos-1] = current_on_left ? sibling.right : sibling.left;
			path [curpos] = parent;
			path [curpos+1] = current_on_left ? parent.right : parent.left;
			path [curpos+2] = current;

			return curpos + 2;
		}

		void node_reparent (Node orig_parent, Node orig, uint orig_size, Node updated)
		{
			if (updated != null && updated.FixSize () != orig_size)
				throw new SystemException ("Internal error: rotation");

			if (orig == root)
				root = updated;
			else if (orig == orig_parent.left)
				orig_parent.left = updated;
			else if (orig == orig_parent.right)
				orig_parent.right = updated;
			else
				throw new SystemException ("Internal error: path error");
		}

		// Pre-condition: current != null
		static Node right_most (Node current, Node sibling, List<Node> path)
		{
			for (;;) {
				path.Add (sibling);
				path.Add (current);
				if (current.right == null)
					return current;
				sibling = current.left;
				current = current.right;
			}
		}

		public struct NodeEnumerator : IEnumerator, IEnumerator<Node> {
			RBTree tree;
			uint version;

			Stack<Node> pennants;

			internal NodeEnumerator (RBTree tree)
			{
				this.tree = tree;
				version = tree.version;
				pennants = null;
			}

			public void Reset ()
			{
				if (tree == null)
					throw new ObjectDisposedException ("enumerator");
				pennants = null;
			}

			public Node Current {
				get {
					check_version ();
					return pennants.Peek ();
				}
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public bool MoveNext ()
			{
				check_version ();

				Node next;
				if (pennants == null) {
					if (tree.root == null)
						return false;
					pennants = new Stack<Node> ();
					next = tree.root;
				} else {
					if (pennants.Count == 0)
						return false;
					Node current = pennants.Pop ();
					next = current.right;
				}
				for (; next != null; next = next.left)
					pennants.Push (next);

				return pennants.Count != 0;
			}

			public void Dispose ()
			{
				tree = null;
				pennants = null;
			}

			void check_version ()
			{
				if (tree == null)
					throw new ObjectDisposedException ("enumerator");
				if (version != tree.version)
					throw new InvalidOperationException ("tree modified");
			}
		}
	}
}

#if TEST
namespace Mono.ValidationTest {
	using System.Collections.Generic;

	internal class TreeSet<T> : IEnumerable<T>, IEnumerable
	{
		public class Node : RBTree.Node {
			public T value;

			public Node (T v)
			{
				value = v;
			}

			public override void SwapValue (RBTree.Node other)
			{
				Node o = (Node) other;
				T v = value;
				value = o.value;
				o.value = v;
			}

			public override void Dump (string indent)
			{
				Console.WriteLine ("{0}{1} {2}({3})", indent, value, IsBlack ? "*" : "", Size);
				if (left != null)
					left.Dump (indent + "  /");
				if (right != null)
					right.Dump (indent + "  \\");
			}
		}

		public class NodeComparer : RBTree.INodeComparer<T> {
			IComparer<T> cmp;

			public int Compare (T value, RBTree.Node node)
			{
				return cmp.Compare (value, ((Node) node).value);
			}

			private NodeComparer (IComparer<T> cmp)
			{
				this.cmp = cmp;
			}
			static NodeComparer Default = new NodeComparer (Comparer<T>.Default);
			public static NodeComparer GetComparer (IComparer<T> cmp)
			{
				if (cmp == null || cmp == Comparer<T>.Default)
					return Default;
				return new NodeComparer (cmp);
			}
		}

		public struct Enumerator : IDisposable, IEnumerator, IEnumerator<T> {
			RBTree.NodeEnumerator host;

			internal Enumerator (TreeSet<T> tree)
			{
				host = new RBTree.NodeEnumerator (tree.tree);
			}

			void IEnumerator.Reset ()
			{
				host.Reset ();
			}

			public T Current {
				get { return ((Node) host.Current).value; }
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public bool MoveNext ()
			{
				return host.MoveNext ();
			}

			public void Dispose ()
			{
				host.Dispose ();
			}
		}

		RBTree tree;

		public TreeSet () : this (null)
		{
		}

		public TreeSet (IComparer<T> cmp)
		{
			tree = new RBTree (NodeComparer.GetComparer (cmp));
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}

		// returns true if the value was inserted, false if the value already existed in the tree
		public bool Insert (T value)
		{
			RBTree.Node n = new Node (value);
			return tree.Intern (value, n) == n;
		}

		// returns true if the value was removed, false if the value didn't exist in the tree
		public bool Remove (T value)
		{
			return tree.Remove (value) != null;
		}

		public bool Contains (T value)
		{
			return tree.Lookup (value) != null;
		}

		public T this [int index] {
			get { return ((Node) tree [index]).value; }
		}

		public int Count {
			get { return (int) tree.Count; }
		}

		public void VerifyInvariants ()
		{
			tree.VerifyInvariants ();
		}

		public void Dump ()
		{
			tree.Dump ();
		}
	}
	
	class Test {
		static void Main (string [] args)
		{
			Random r = new Random ();
			Dictionary<int, int> d = new Dictionary<int, int> ();
			TreeSet<int> t = new TreeSet<int> ();
			int iters = args.Length == 0 ? 100000 : Int32.Parse (args [0]);

			for (int i = 0; i < iters; ++i) {
				if ((i % 100) == 0)
					t.VerifyInvariants ();

				int n = r.Next ();
				if (d.ContainsKey (n))
					continue;
				d [n] = n;

				try {
					if (t.Contains (n))
						throw new Exception ("tree says it has a number it shouldn't");
					if (!t.Insert (n))
						throw new Exception ("tree says it has a number it shouldn't");
				} catch {
					Console.Error.WriteLine ("Exception while inserting {0} in iteration {1}", n, i);
					throw;
				}
			}
			t.VerifyInvariants ();
			if (d.Count != t.Count)
				throw new Exception ("tree count is wrong?");

			Console.WriteLine ("Tree has {0} elements", t.Count);

			foreach (int n in d.Keys)
				if (!t.Contains (n))
					throw new Exception ("tree says it doesn't have a number it should");

			Dictionary<int, int> d1 = new Dictionary<int, int> (d);

			int prev = -1;
			foreach (int n in t) {
				if (n < prev)
					throw new Exception ("iteration out of order");
				if (!d1.Remove (n))
					throw new Exception ("tree has a number it shouldn't");
				prev = n;
			}

			if (d1.Count != 0)
				throw new Exception ("tree has numbers it shouldn't");

			for (int i = 0; i < iters; ++i) {
				int n = r.Next ();
				if (!d.ContainsKey (n)) {
					if (t.Contains (n))
						throw new Exception ("tree says it doesn't have a number it should");
				} else if (!t.Contains (n)) {
					throw new Exception ("tree says it has a number it shouldn't");
				}
			}

			int j = 0;
			foreach (int n in d.Keys) {
				if ((j++ % 100) == 0)
					t.VerifyInvariants ();
				try {
					if (!t.Remove (n))
						throw new Exception ("tree says it doesn't have a number it should");

				} catch {
					Console.Error.WriteLine ("While trying to remove {0} from tree of size {1}", n, t.Count);
					t.Dump ();
					t.VerifyInvariants ();
					throw;
				}
				if (t.Contains (n))
					throw new Exception ("tree says it has a number it shouldn't");
			}
			t.VerifyInvariants ();

			if (t.Count != 0)
				throw new Exception ("tree claims to have elements");
		}
	}
}
#endif

#endif
