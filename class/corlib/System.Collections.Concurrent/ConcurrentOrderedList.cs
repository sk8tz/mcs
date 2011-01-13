// ConcurrentOrderedList.cs
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

#if NET_4_0

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Collections.Concurrent
{
#if INSIDE_MONO_PARALLEL
	public
#endif
	class ConcurrentOrderedList<T>
	{
		class Node
		{
			public T Data;
			public int Key;
			public Node Next;
			public bool Marked;		   

			public Node ()
			{

			}

			public Node (Node wrapped)
			{
				Marked = true;
				Next = wrapped;
			}
		}

		Node head;
		Node tail;

		IEqualityComparer<T> comparer;

		public ConcurrentOrderedList () : this (EqualityComparer<T>.Default)
		{
			
		}

		public ConcurrentOrderedList (IEqualityComparer<T> comparer)
		{
			this.comparer = comparer;

			head = new Node ();
			tail = new Node ();
			head.Next = tail;
		}

		public bool TryAdd (T data)
		{
			Node node = new Node ();
			node.Data = data;
			node.Key = comparer.GetHashCode (data);

			return ListInsert (node);
		}

		public bool TryRemove (T data)
		{
			T dummy;
			return TryRemoveHash (comparer.GetHashCode (data), out dummy);
		}

		public bool TryRemoveHash (int key, out T data)
		{
			return ListDelete (key, out data);
		}

		public bool Contains (T data)
		{
			return ContainsHash (comparer.GetHashCode (data));
		}

		public bool ContainsHash (int key)
		{
			Node node;

			if (!ListFind (key, out node))
				return false;

			return true;

		}

		public bool TryGetFromHash (int key, out T data)
		{
			data = default (T);
			Node node;

			if (!ListFind (key, out node))
				return false;

			data = node.Data;
			return true;
		}

		public IEqualityComparer<T> Comparer {
			get {
				return comparer;
			}
		}

		Node ListSearch (int key, ref Node left)
		{
			Node leftNodeNext = null, rightNode = null;

			do {
				Node t = head;
				Node tNext = t.Next;
				do {
					if (!tNext.Marked) {
						left = t;
						leftNodeNext = tNext;
					}
					t = tNext.Marked ? tNext.Next : tNext;
					if (t == tail)
						break;
					
					tNext = t.Next;
				} while (tNext.Marked || t.Key < key);

				rightNode = t;
				
				if (leftNodeNext == rightNode) {
					if (rightNode != tail && rightNode.Next.Marked)
						continue;
					else 
						return rightNode;
				}
				
				if (Interlocked.CompareExchange (ref left.Next, rightNode, leftNodeNext) == leftNodeNext) {
					if (rightNode != tail && rightNode.Next.Marked)
						continue;
					else
						return rightNode;
				}
			} while (true);
		}

		bool ListDelete (int key, out T data)
		{
			Node rightNode = null, rightNodeNext = null, leftNode = null;
			data = default (T);
			
			do {
				rightNode = ListSearch (key, ref leftNode);
				if (rightNode == tail || rightNode.Key != key)
					return false;

				data = rightNode.Data;
				
				rightNodeNext = rightNode.Next;
				if (!rightNodeNext.Marked)
					if (Interlocked.CompareExchange (ref rightNode.Next, new Node (rightNodeNext), rightNodeNext) == rightNodeNext)
						break;
			} while (true);
			
			if (Interlocked.CompareExchange (ref leftNode.Next, rightNodeNext, rightNode) != rightNodeNext)
				ListSearch (rightNode.Key, ref leftNode);
			
			return true;
		}
		
		bool ListInsert (Node newNode)
		{
			int key = newNode.Key;
			Node rightNode = null, leftNode = null;
			
			do {
				rightNode = ListSearch (key, ref leftNode);
				if (rightNode != tail && rightNode.Key == key)
					return false;
				
				newNode.Next = rightNode;
				if (Interlocked.CompareExchange (ref leftNode.Next, newNode, rightNode) == rightNode)
					return true;
			} while (true);
		}
		
		bool ListFind (int key, out Node data)
		{
			Node rightNode = null, leftNode = null;
			data = null;
			
			data = rightNode = ListSearch (key, ref leftNode);
			
			return rightNode != tail && rightNode.Key == key;
		}
	}
}

#endif
