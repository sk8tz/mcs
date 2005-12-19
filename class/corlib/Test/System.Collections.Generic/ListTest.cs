//
// MonoTests.System.Collections.Generic.Test.DictionaryTest
//
// Authors:
//      David Waite (mass@akuma.org)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 David Waite (mass@akuma.org)
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Collections.Generic {

	class GenericComparer<T> : IComparer<T> {

		private bool called = false;

		public bool Called {
			get {
				bool result = called;
				called = false;
				return called;
			}
		}

		public int Compare (T x, T y)
		{
			called = true;
			return 0;
		}
	}

	[TestFixture]
	public class ListTest
	{

		int [] _list1_contents;
		List <int> _list1;

		[SetUp]
		public void SetUp ()
		{
			// FIXME arrays currently do not support generic collection
			// interfaces
			_list1_contents = new int [] { 55, 50, 22, 80, 56, 52, 40, 63 };
			// _list1 = new List <int> (_list1_contents);
			
			_list1 = new List <int> (8);
			foreach (int i in _list1_contents)
				_list1.Add (i);
		}

		[Test]  // This was for bug #74980
		public void InsertTest ()
		{
			List <string> test = new List <string> ();
			test.Insert (0, "a");
			test.Insert (0, "b");
			test.Insert (1, "c");

			Assert.AreEqual (3, test.Count);
			Assert.AreEqual ("b", test [0]);
			Assert.AreEqual ("c", test [1]);
			Assert.AreEqual ("a", test [2]);
		}

		[Test]
		public void InsertRangeTest ()
		{
			int count = _list1.Count;
			// FIXME arrays currently do not support generic collection 
			// interfaces
			int [] items = {1, 2, 3};
			// List <int> newRange = new List <int> (items);
			List <int> newRange = new List <int> (3);
			foreach (int i in items)
				   newRange.Add (i);
			_list1.InsertRange (1, newRange);
			Assert.AreEqual (count + 3, _list1.Count);
			Assert.AreEqual (55, _list1 [0]);
			Assert.AreEqual (1, _list1 [1]);
			Assert.AreEqual (2, _list1 [2]);
			Assert.AreEqual (3, _list1 [3]);
			Assert.AreEqual (50, _list1 [4]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void InsertRangeNullTest ()
		{
			IEnumerable <int> n = null;
			_list1.InsertRange (0, n);
		}

		[Test]
		public void IndexOfTest ()
		{
			List <int> l = new List <int> ();

			l.Add (100);
			l.Add (200);

			Assert.AreEqual (1, l.IndexOf (200), "Could not find value");
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IndexOfOutOfRangeTest ()
		{
			List <int> l = new List <int> (4);
			l.IndexOf (0, 0, 4);
		}

		[Test]
		public void GetRangeTest ()
		{
			List <int> r = _list1.GetRange (2, 4);
			Assert.AreEqual (4, r.Count);
			Assert.AreEqual (22, r [0]);
			Assert.AreEqual (80, r [1]);
			Assert.AreEqual (56, r [2]);
			Assert.AreEqual (52, r [3]);
		}

		[Test]
		public void EnumeratorTest ()
		{
			List <int>.Enumerator e = _list1.GetEnumerator ();
			for (int i = 0; i < _list1_contents.Length; i++)
			{
				Assert.IsTrue (e.MoveNext ());
				Assert.AreEqual (_list1_contents [i], e.Current);
			}
			Assert.IsFalse (e.MoveNext ());
		}

		[Test]
		public void ConstructWithSizeTest ()
		{
			List <object> l_1 = new List <object> (1);
			List <object> l_2 = new List <object> (50);
			List <object> l_3 = new List <object> (0);

			Assert.AreEqual (1, l_1.Capacity);
			Assert.AreEqual (50, l_2.Capacity);
			Assert.AreEqual (0, l_3.Capacity);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructWithInvalidSizeTest ()
		{
			List <int> l = new List <int> (-1);
		}

		[Test]
		public void ConstructWithCollectionTest ()
		{
			List <int> l1 = new List <int> (_list1);
			Assert.AreEqual (_list1.Count, l1.Count);
			Assert.AreEqual (l1.Count, l1.Capacity);
			for (int i = 0; i < l1.Count; i++)
				Assert.AreEqual (_list1 [i], l1 [i]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ConstructWithInvalidCollectionTest ()
		{
			List <int> n = null;
			List <int> l1 = new List <int> (n);
		}

		[Test]
		public void AddTest ()
		{
			int count = _list1.Count;
			_list1.Add (-1);
			Assert.AreEqual (count + 1, _list1.Count);
			Assert.AreEqual (-1, _list1 [_list1.Count - 1]);
		}

		[Test]
		public void AddRangeTest ()
		{
			int count = _list1.Count;
			// FIXME arrays currently do not support generic collection
			// interfaces
			int [] range = { -1, -2, -3 };
			List <int> tmp = new List <int> (3);
			foreach (int i in range)
				tmp.Add (i);
			// _list1.AddRange (range);
			_list1.AddRange (tmp);
			
			Assert.AreEqual (count + 3, _list1.Count);
			Assert.AreEqual (-1, _list1 [_list1.Count - 3]);
			Assert.AreEqual (-2, _list1 [_list1.Count - 2]);
			Assert.AreEqual (-3, _list1 [_list1.Count - 1]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void AddNullRangeTest ()
		{
			int [] n = null;
			_list1.AddRange (n);
		}

		[Test]
		public void BinarySearchTest ()
		{
			List <int> l = new List <int> (_list1);
			l.Sort ();
			Assert.AreEqual (0, l.BinarySearch (22));
			Assert.AreEqual (-2, l.BinarySearch (23));
			Assert.AreEqual (- (l.Count + 1), l.BinarySearch (int.MaxValue));
		}

		[Test]
		public void SortTest ()
		{
			List <int> l = new List <int> (_list1);
			l.Sort ();
			Assert.AreEqual (_list1.Count, l.Count);
			Assert.AreEqual (22, l [0]);
			int minimum = 22;
			foreach (int i in l)
			{
				Assert.IsTrue (minimum <= i);
				minimum = i;
			}
		}

		[Test]
		public void ClearTest ()
		{
			int capacity = _list1.Capacity;
			_list1.Clear ();
			Assert.AreEqual (0, _list1.Count);
			Assert.AreEqual (capacity, _list1.Capacity);
		}

		[Test]
		public void ContainsTest ()
		{
			Assert.IsTrue (_list1.Contains (22));
			Assert.IsFalse (_list1.Contains (23));
		}

		private string StringConvert (int i)
		{
			return i.ToString ();
		}
		
		[Test]
		public void ConvertAllTest ()
		{
			List <string> s = _list1.ConvertAll ( (Converter <int, string>)StringConvert);
			Assert.AreEqual (_list1.Count, s.Count);
			Assert.AreEqual ("55", s [0]);
		}

		[Test]
		public void CopyToTest ()
		{
			int [] a = new int [2];
			_list1.CopyTo (1, a, 0, 2);
			Assert.AreEqual (50, a [0]);
			Assert.AreEqual (22, a [1]);

			int [] b = new int [_list1.Count + 1];
			b [_list1.Count] = 555;
			_list1.CopyTo (b);
			Assert.AreEqual (55, b [0]);
			Assert.AreEqual (555, b [_list1.Count]);

			b [0] = 888;
			_list1.CopyTo (b, 1);
			Assert.AreEqual (888, b [0]);
			Assert.AreEqual (55, b [1]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void CopyToNullTest ()
		{
			int [] a = null;
			_list1.CopyTo (0, a, 0, 0);
		}

		static bool FindMultipleOfThree (int i)
		{
			return (i % 3) == 0;
		}

		static bool FindMultipleOfFour (int i)
		{
			return (i % 4) == 0;
		}

		static bool FindMultipleOfTwelve (int i)
		{
			return (i % 12) == 0;
		}

		[Test]
		public void FindTest ()
		{
			int i = _list1.Find (FindMultipleOfThree);
			Assert.AreEqual (63, i);

			i = _list1.Find (FindMultipleOfTwelve);
			Assert.AreEqual (default (int), i);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindNullTest ()
		{
			int i = _list1.Find (null);
		}

		[Test]
		public void FindAllTest ()
		{
			List <int> findings = _list1.FindAll (FindMultipleOfFour);
			Assert.AreEqual (4, findings.Count);
			Assert.AreEqual (80, findings [0]);
			Assert.AreEqual (56, findings [1]);
			Assert.AreEqual (52, findings [2]);
			Assert.AreEqual (40, findings [3]);

			findings = _list1.FindAll (FindMultipleOfTwelve);
			Assert.IsNotNull (findings);
			Assert.AreEqual (0, findings.Count);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindAllNullTest ()
		{
			List <int> findings = _list1.FindAll (null);
		}

		[Test]
		public void FindIndexTest ()
		{
			int i = _list1.FindIndex (FindMultipleOfThree);
			Assert.AreEqual (7, i);

			i = _list1.FindIndex (FindMultipleOfTwelve);
			Assert.AreEqual (-1, i);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindIndexNullTest ()
		{
			int i = _list1.FindIndex (null);
		}

		[Test]
		public void FindLastTest ()
		{
			int i = _list1.FindLast (FindMultipleOfFour);
			Assert.AreEqual (40, i);

			i = _list1.FindLast (FindMultipleOfTwelve);
			Assert.AreEqual (default (int), i);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindLastNullTest ()
		{
			int i = _list1.FindLast (null);
		}

		// FIXME currently generates Invalid IL Code error
		/*
		[Test]
		public void ForEachTest ()
		{
			int i = 0;
			_list1.ForEach (delegate (int j) { i += j; });

			Assert.AreEqual (418, i);
		}
		*/
		[Test]
		public void FindLastIndexTest ()
		{
			int i = _list1.FindLastIndex (FindMultipleOfFour);
			Assert.AreEqual (6, i);

			i = _list1.FindLastIndex (5, FindMultipleOfFour);
			Assert.AreEqual (5, i);

			i = _list1.FindIndex (FindMultipleOfTwelve);
			Assert.AreEqual (-1, i);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void FindLastIndexNullTest ()
		{
			int i = _list1.FindLastIndex (null);
		}

		[Test]
		public void RemoveTest ()
		{
			int count = _list1.Count;
			bool result = _list1.Remove (22);
			Assert.IsTrue (result);
			Assert.AreEqual (count - 1, _list1.Count);

			Assert.AreEqual (-1, _list1.IndexOf (22));

			result = _list1.Remove (0);
			Assert.IsFalse (result);
		}

		[Test]
		public void RemoveAllTest ()
		{
			int count = _list1.Count;
			int removedCount = _list1.RemoveAll (FindMultipleOfFour);
			Assert.AreEqual (4, removedCount);
			Assert.AreEqual (count - 4, _list1.Count);

			removedCount = _list1.RemoveAll (FindMultipleOfTwelve);
			Assert.AreEqual (0, removedCount);
			Assert.AreEqual (count - 4, _list1.Count);
		}

		[Test]
		public void RemoveAtTest ()
		{
			int count = _list1.Count;
			_list1.RemoveAt (0);
			Assert.AreEqual (count - 1, _list1.Count);
			Assert.AreEqual (50, _list1 [0]);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RemoveOutOfRangeTest ()
		{
			_list1.RemoveAt (_list1.Count);
		}

		[Test]
		public void RemoveRangeTest ()
		{
			int count = _list1.Count;
			_list1.RemoveRange (1, 2);
			Assert.AreEqual (count - 2, _list1.Count);
			Assert.AreEqual (55, _list1 [0]);
			Assert.AreEqual (80, _list1 [1]);

			_list1.RemoveRange (0, 0);
			Assert.AreEqual (count - 2, _list1.Count);
		}

		[Test]
	        public void RemoveRangeFromEmptyListTest ()
		{
			List<int> l = new List<int> ();
			l.RemoveRange (0, 0);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void RemoveRangeOutOfRangeTest ()
		{
			_list1.RemoveRange (1, _list1.Count);
		}

		[Test]
		public void ReverseTest ()
		{
			int count = _list1.Count;
			_list1.Reverse ();
			Assert.AreEqual (count, _list1.Count);

			Assert.AreEqual (63, _list1 [0]);
			Assert.AreEqual (55, _list1 [count - 1]);

			_list1.Reverse (0, 2);

			Assert.AreEqual (40, _list1 [0]);
			Assert.AreEqual (63, _list1 [1]);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ReverseOutOfRangeTest ()
		{
			_list1.Reverse (1, _list1.Count);
		}

		[Test]
		public void ToArrayTest ()
		{
			int [] copiedContents = _list1.ToArray ();
			Assert.IsFalse (ReferenceEquals (copiedContents, _list1_contents));

			Assert.AreEqual (_list1.Count, copiedContents.Length);
			Assert.AreEqual (_list1 [0], copiedContents [0]);
		}

		[Test]
		public void TrimExcessTest ()
		{
			List <string> l = new List <string> ();
			l.Add ("foo");

			Assert.IsTrue (l.Count < l.Capacity);
			l.TrimExcess ();
			Assert.AreEqual (l.Count, l.Capacity);
		}

		bool IsPositive (int i)
		{
			return i >= 0;
		}

		[Test]
		public void TrueForAllTest ()
		{
			Assert.IsFalse (_list1.TrueForAll (FindMultipleOfFour));
			Assert.IsTrue (_list1.TrueForAll (IsPositive));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void TrueForAllNullTest ()
		{
			_list1.TrueForAll (null);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CapacityOutOfRangeTest ()
		{
			_list1.Capacity = _list1.Count - 1;
		}

		[Test] // bug 77030
		public void BinarySearch_EmptyList ()
		{
			GenericComparer<int> comparer = new GenericComparer<int> ();
			List<int> l = new List<int> ();
			l.BinarySearch (0, comparer);
			Assert.IsFalse (comparer.Called, "Called");
		}
	}
}
#endif

