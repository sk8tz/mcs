// TODO: add tests for Comparer and HashCodeProvider


using System;
using System.Collections;

using NUnit.Framework;



namespace MonoTests.System.Collections {


	/// <summary>Hashtable test.</summary>
	public class HashtableTest : TestCase {
		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite();

				suite.AddTest(new TestSuite(typeof(HashtableTest)));
				suite.AddTest(new TestSuite(typeof(HashtableTest2)));
				return suite;
			}
		}


		protected Hashtable ht;
		private static Random rnd;

		public HashtableTest(String name) : base(name) {}

		protected override void SetUp() {
			ht=new Hashtable();
			rnd=new Random();
		}

		private void SetDefaultData() {
			ht.Clear();
			ht.Add("k1","another");
			ht.Add("k2","yet");
			ht.Add("k3","hashtable");
		}


		public void TestAddRemoveClear() {
			ht.Clear();
			Assert(ht.Count==0);

			SetDefaultData();
			Assert(ht.Count==3);

			bool thrown=false;
			try {
				ht.Add("k2","cool");
			} catch (ArgumentException) {thrown=true;}
			Assert("Must throw ArgumentException!",thrown);

			ht["k2"]="cool";
			Assert(ht.Count==3);
			Assert(ht["k2"].Equals("cool"));

		}

		public void TestCopyTo() {
			SetDefaultData();
			Object[] entries=new Object[ht.Count];
			ht.CopyTo(entries,0);
			Assert("Not an entry.",entries[0] is DictionaryEntry);
		}


		public void TestUnderHeavyLoad() {
			ht.Clear();
			int max=100000;
			String[] cache=new String[max*2];
			int n=0;

			for (int i=0;i<max;i++) {
				int id=rnd.Next()&0xFFFF;
				String key=""+id+"-key-"+id;
				String val="value-"+id;
				if (ht[key]==null) {
					ht[key]=val;
					cache[n]=key;
					cache[n+max]=val;
					n++;
				}
			}

			Assert(ht.Count==n);

			for (int i=0;i<n;i++) {
				String key=cache[i];
				String val=ht[key] as String;
				String err="ht[\""+key+"\"]=\""+val+
				      "\", expected \""+cache[i+max]+"\"";
				Assert(err,val!=null && val.Equals(cache[i+max]));
			}

			int r1=(n/3);
			int r2=r1+(n/5);

			for (int i=r1;i<r2;i++) {
				ht.Remove(cache[i]);
			}


			for (int i=0;i<n;i++) {
				if (i>=r1 && i<r2) {
					Assert(ht[cache[i]]==null);
				} else {
					String key=cache[i];
					String val=ht[key] as String;
					String err="ht[\""+key+"\"]=\""+val+
					      "\", expected \""+cache[i+max]+"\"";
					Assert(err,val!=null && val.Equals(cache[i+max]));
				}
			}

			ICollection keys=ht.Keys;
			int nKeys=0;
			foreach (Object key in keys) {
				Assert((key as String) != null);
				nKeys++;
			}
			Assert(nKeys==ht.Count);


			ICollection vals=ht.Values;
			int nVals=0;
			foreach (Object val in vals) {
				Assert((val as String) != null);
				nVals++;
			}
			Assert(nVals==ht.Count);

		}


	private class  HashtableTest2 : TestCase {

		protected Hashtable ht;
		private static Random rnd;

		public HashtableTest2 (String name) : base(name)
		{
		}

		protected override void SetUp ()
		{
			ht=new Hashtable ();
			rnd=new Random ();
		}

		public static ITest Suite
		{
			get {
				return new TestSuite (typeof(HashtableTest2));
			}
		}

		private void SetDefaultData ()
		{
			ht.Clear ();
			ht.Add ("k1","another");
			ht.Add ("k2","yet");
			ht.Add ("k3","hashtable");
		}


		public void TestAddRemoveClear ()
		{
			ht.Clear ();
			Assert (ht.Count == 0);

			SetDefaultData ();
			Assert (ht.Count == 3);

			bool thrown=false;
			try {
				ht.Add ("k2","cool");
			} catch (ArgumentException) {thrown=true;}
			Assert("Must throw ArgumentException!",thrown);

			ht["k2"]="cool";
			Assert(ht.Count == 3);
			Assert(ht["k2"].Equals("cool"));

		}

		public void TestCopyTo ()
		{
			SetDefaultData ();
			Object[] entries=new Object[ht.Count];
			ht.CopyTo (entries,0);
			Assert("Not an entry.",entries[0] is DictionaryEntry);
		}


		public void TestUnderHeavyLoad ()
		{
			ht.Clear ();

			int max=100000;
			String[] cache=new String[max*2];
			int n=0;

			for (int i=0;i<max;i++) {
				int id=rnd.Next()&0xFFFF;
				String key=""+id+"-key-"+id;
				String val="value-"+id;
				if (ht[key]==null) {
					ht[key]=val;
					cache[n]=key;
					cache[n+max]=val;
					n++;
				}
			}

			Assert(ht.Count==n);

			for (int i=0;i<n;i++) {
				String key=cache[i];
				String val=ht[key] as String;
				String err="ht[\""+key+"\"]=\""+val+
				      "\", expected \""+cache[i+max]+"\"";
				Assert(err,val!=null && val.Equals(cache[i+max]));
			}

			int r1=(n/3);
			int r2=r1+(n/5);

			for (int i=r1;i<r2;i++) {
				ht.Remove(cache[i]);
			}


			for (int i=0;i<n;i++) {
				if (i>=r1 && i<r2) {
					Assert(ht[cache[i]]==null);
				} else {
					String key=cache[i];
					String val=ht[key] as String;
					String err="ht[\""+key+"\"]=\""+val+
					      "\", expected \""+cache[i+max]+"\"";
					Assert(err,val!=null && val.Equals(cache[i+max]));
				}
			}

			ICollection keys=ht.Keys;
			int nKeys=0;
			foreach (Object key in keys) {
				Assert((key as String) != null);
				nKeys++;
			}
			Assert(nKeys==ht.Count);


			ICollection vals=ht.Values;
			int nVals=0;
			foreach (Object val in vals) {
				Assert((val as String) != null);
				nVals++;
			}
			Assert(nVals==ht.Count);

		}

		/// <summary>
		///  Test hashtable with CaseInsensitiveHashCodeProvider
		///  and CaseInsensitive comparer.
		/// </summary>
		public void TestCaseInsensitive ()
		{
			// Not very meaningfull test, just to make
			// sure that hcp is set properly set.
			Hashtable ciHashtable = new Hashtable(11,1.0f,CaseInsensitiveHashCodeProvider.Default,CaseInsensitiveComparer.Default);
			ciHashtable ["key1"] = "value";
			ciHashtable ["key2"] = "VALUE";
			Assert(ciHashtable ["key1"].Equals ("value"));
			Assert(ciHashtable ["key2"].Equals ("VALUE"));

			ciHashtable ["KEY1"] = "new_value";
			Assert(ciHashtable ["key1"].Equals ("new_value"));

		}


		public void TestCopyConstructor ()
		{
			SetDefaultData ();

			Hashtable htCopy = new Hashtable (ht);

			Assert(ht.Count == htCopy.Count);
		}


		public void TestEnumerator ()
		{
			SetDefaultData ();

			IEnumerator e = ht.GetEnumerator ();

			while (e.MoveNext ()) {}

			Assert (!e.MoveNext ());

		}


	}


	}
}
