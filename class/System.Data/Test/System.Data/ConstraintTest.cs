// ConstraintTest.cs - NUnit Test Cases for testing the abstract class System.Data.Constraint
// The tests use an inherited class (UniqueConstraint) to test the Constraint class.
//
// Franklin Wise <gracenote@earthlink.net>
//
// (C) 2002 Franklin Wise
// 

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
//	public class MyUniqueConstraint: UniqueConstraint {
//		public MyUniqueConstraint(DataColumn col, bool pk): base(col,pk){}
//		string _myval = "";
//		public override string ConstraintName {
//			get{
//				return _myval;
//				return base.ConstraintName;
//			}
//			set{
//				Console.WriteLine("NameSet = " + value);
//				base.ConstraintName = value;
//				_myval = value;
//			}
//		}
//	}

	public class ConstraintTest : TestCase 
	{
		private DataTable _table;
		private Constraint _constraint1;
		private Constraint _constraint2;

		public ConstraintTest() : base ("MonoTests.System.Data.ConstraintTest") {}
		public ConstraintTest(string name) : base(name) {}

		public void PublicSetup(){SetUp();}
		protected override void SetUp() {

			//Setup DataTable
			_table = new DataTable("TestTable");
			_table.Columns.Add("Col1",typeof(int));
			_table.Columns.Add("Col2",typeof(int));

			//Use UniqueConstraint to test Constraint Base Class
			_constraint1 = new UniqueConstraint(_table.Columns[0],false); 
			_constraint2 = new UniqueConstraint(_table.Columns[1],false); 

		}  
		

		protected override void TearDown() {}

		public static ITest Suite {
			get { 
				return new TestSuite(typeof(ConstraintTest)); 
			}
		}

		public void TestSetConstraintNameNullOrEmptyExceptions() {
			bool exceptionCaught = false;
			string name = null;

			_table.Constraints.Add (_constraint1);  

			Console.WriteLine(_constraint1.ConstraintName);

			for (int i = 0; i <= 1; i++) {
				exceptionCaught = false;
				if (0 == i) name = null;
				if (1 == i) name = String.Empty;
	
				try {
				
					//Next line should throw ArgumentException
					//Because ConstraintName can't be set to null
					//or empty while the constraint is part of the
					//collection
					_constraint1.ConstraintName = name; 
				}
				catch (ArgumentException){ 
					exceptionCaught = true;
				}
				catch {
					Assertion.Fail("Wrong exception type thrown.");
				}
				
				Assertion.Assert("Failed to throw exception.",
					true == exceptionCaught);
			}	
		}

		public void TestSetConstraintNameDuplicateException() {
			_constraint1.ConstraintName = "Dog";
			_constraint2.ConstraintName = "Cat";

			_table.Constraints.Add(_constraint1);
			_table.Constraints.Add(_constraint2);

			try {
				//Should throw DuplicateNameException
				_constraint2.ConstraintName = "Dog";
			
				Assertion.Fail("Failed to throw " + 
					" DuplicateNameException exception.");
			}	
			catch (DuplicateNameException) {}
			catch (AssertionFailedError exc) {throw exc;}
			catch {
				Assertion.Fail("Wrong exception type thrown.");
			}
		
		}

		public void TestToString() {
			_constraint1.ConstraintName = "Test";
			Assertion.Assert("ToString is the same as constraint name.", _constraint1.ConstraintName.CompareTo( _constraint1.ToString()) == 0);
			
			_constraint1.ConstraintName = null;
			Assertion.AssertNotNull("ToString should return empty.",_constraint1.ToString());
		}

		public void TestGetExtendedProperties() {
			PropertyCollection col = _constraint1.ExtendedProperties as
				PropertyCollection;

			Assertion.AssertNotNull("ExtendedProperties returned null or didn't " +
				"return the correct type", col);
		}
		
	}
}
