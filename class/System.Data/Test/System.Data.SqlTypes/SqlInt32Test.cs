// SqlInt32Test.cs - NUnit Test Cases for System.Data.SqlTypes.SqlInt32
//
// Tim Coleman (tim@timcoleman.com)
//
// (C) Tim Coleman
// 

using NUnit.Framework;
using System;
using System.Data;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{
	public class SqlInt32Test : TestCase 
	{
		
		public SqlInt32Test() : base ("System.Data.SqlTypes.SqlInt32") {}
		public SqlInt32Test(string name) : base(name) {}

		protected override void SetUp() {}

		protected override void TearDown() {}

		public static ITest Suite {
			get { 
				return new TestSuite(typeof(SqlInt32)); 
			}
		}

		public void TestCreate ()  
		{
			SqlInt32 foo = new SqlInt32 (5);
			AssertEquals ("Test explicit cast to int", (int)foo, 5);
		}

		public void TestAdd () 
		{
			int a = 5;
			int b = 7;
			
			SqlInt32 x;
			SqlInt32 y;
			SqlInt32 z;

			x = new SqlInt32 (a);
			y = new SqlInt32 (b);
			z = x + y;
			AssertEquals ("Addition operator does not work correctly", z.Value, a + b);
			z = SqlInt32.Add (x, y);
			AssertEquals ("Addition function does not work correctly", z.Value, a + b);
		}

		public void TestBitwiseAnd () 
		{
			int a = 5;
			int b = 7;
						
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x & y;
			AssertEquals ("Bitwise And operator does not work correctly", z.Value, a & b);
			z = SqlInt32.BitwiseAnd (x, y);
			AssertEquals ("Bitwise And function does not work correctly", z.Value, a & b);
		}

		public void TestBitwiseOr () 
		{
			int a = 5;
			int b = 7;
			
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x | y;
			AssertEquals ("Bitwise Or operator does not work correctly", z.Value, a | b);
			z = SqlInt32.BitwiseOr (x, y);
			AssertEquals ("Bitwise Or function does not work correctly", z.Value, a | b);
		}

		public void TestDivide () 
		{
			int a = 5;
			int b = 7;
			
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x / y;
			AssertEquals ("Division operator does not work correctly", z.Value, a / b);
			z = SqlInt32.Divide (x, y);
			AssertEquals ("Division function does not work correctly", z.Value, a / b);
		}
		
		public void TestEquals ()
		{
			SqlInt32 x;
			SqlInt32 y;
			
			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			AssertEquals ("Equality operator didn't return Null when one was Null.", x == y, SqlBoolean.Null);
			AssertEquals ("Equality function didn't return Null when one was Null.", SqlInt32.Equals (x, y), SqlBoolean.Null);

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			AssertEquals ("Equality operator didn't return Null when both were Null.", x == y, SqlBoolean.Null);
			AssertEquals ("Equality function didn't return Null when both were Null.", SqlInt32.Equals (x, y), SqlBoolean.Null);

			// Case 3: both are equal
			x = new SqlInt32 (5);
			y = new SqlInt32 (5);
			AssertEquals ("Equality operator didn't return true when they were equal.", x == y, SqlBoolean.True);
			AssertEquals ("Equality function didn't return true when they were equal.", SqlInt32.Equals (x, y), SqlBoolean.True);

			// Case 4: inequality
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			AssertEquals ("Equality operator didn't return false when they were not equal.", x == y, SqlBoolean.False);
			AssertEquals ("Equality function didn't return false when they were not equal.", SqlInt32.Equals (x, y), SqlBoolean.False);
		}

		public void TestGreaterThan ()
		{
			SqlInt32 x;
			SqlInt32 y;
			
			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			AssertEquals ("Greater Than operator didn't return Null when one was Null.", x > y, SqlBoolean.Null);
			AssertEquals ("Greater Than function didn't return Null when one was Null.", SqlInt32.GreaterThan (x, y), SqlBoolean.Null);

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			AssertEquals ("Greater Than operator didn't return Null when both were Null.", x > y, SqlBoolean.Null);
			AssertEquals ("Greater Than function didn't return Null when both were Null.", SqlInt32.GreaterThan (x, y), SqlBoolean.Null);

			// Case 3: x > y
			x = new SqlInt32 (5);
			y = new SqlInt32 (4);
			AssertEquals ("Greater than operator didn't return true when x > y.", x > y, SqlBoolean.True);
			AssertEquals ("Greater than function didn't return true when x > y.", SqlInt32.GreaterThan (x,y), SqlBoolean.True);

			// Case 4: x < y
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			AssertEquals ("Greater than operator didn't return false when x < y.", x > y, SqlBoolean.False);
			AssertEquals ("Greater than function didn't return false when x < y.", SqlInt32.GreaterThan (x,y), SqlBoolean.False);
		}

		public void TestGreaterThanOrEqual ()
		{
			SqlInt32 x;
			SqlInt32 y;
			
			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			AssertEquals ("Greater Than Or Equal operator didn't return Null when one was Null.", x >= y, SqlBoolean.Null);
			AssertEquals ("Greater Than Or Equal function didn't return Null when one was Null.", SqlInt32.GreaterThanOrEqual (x, y), SqlBoolean.Null);

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			AssertEquals ("Greater Than Or Equal operator didn't return Null when both were Null.", x >= y, SqlBoolean.Null);
			AssertEquals ("Greater Than Or Equal function didn't return Null when both were Null.", SqlInt32.GreaterThanOrEqual (x, y), SqlBoolean.Null);

			// Case 3: x > y
			x = new SqlInt32 (5);
			y = new SqlInt32 (4);
			AssertEquals ("Greater than or equal operator didn't return true when x > y.", x >= y, SqlBoolean.True);
			AssertEquals ("Greater than or equal function didn't return true when x > y.", SqlInt32.GreaterThanOrEqual (x,y), SqlBoolean.True);

			// Case 4: x < y
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			AssertEquals ("Greater than or equal operator didn't return false when x < y.", x >= y, SqlBoolean.False);
			AssertEquals ("Greater than or equal function didn't return false when x < y.", SqlInt32.GreaterThanOrEqual (x,y), SqlBoolean.False);

			// Case 5: x == y
			x = new SqlInt32 (5);
			y = new SqlInt32 (5);
			AssertEquals ("Greater than or equal operator didn't return true when x == y.", x >= y, SqlBoolean.True);
			AssertEquals ("Greater than or equal function didn't return true when x == y.", SqlInt32.GreaterThanOrEqual (x,y), SqlBoolean.True);
		}

		public void TestLessThan ()
		{
			SqlInt32 x;
			SqlInt32 y;
			
			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			AssertEquals ("Less Than operator didn't return Null when one was Null.", x < y, SqlBoolean.Null);
			AssertEquals ("Less Than function didn't return Null when one was Null.", SqlInt32.LessThan (x, y), SqlBoolean.Null);

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			AssertEquals ("Less Than operator didn't return Null when both were Null.", x < y, SqlBoolean.Null);
			AssertEquals ("Less Than function didn't return Null when both were Null.", SqlInt32.LessThan (x, y), SqlBoolean.Null);

			// Case 3: x > y
			x = new SqlInt32 (5);
			y = new SqlInt32 (4);
			AssertEquals ("Less than operator didn't return false when x > y.", x < y, SqlBoolean.False);
			AssertEquals ("Less than function didn't return false when x > y.", SqlInt32.LessThan (x,y), SqlBoolean.False);

			// Case 4: x < y
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			AssertEquals ("Less than operator didn't return true when x < y.", x < y, SqlBoolean.True);
			AssertEquals ("Less than function didn't return true when x < y.", SqlInt32.LessThan (x,y), SqlBoolean.True);
		}

		public void TestLessThanOrEqual ()
		{
			SqlInt32 x;
			SqlInt32 y;
			
			// Case 1: either is SqlInt32.Null
			x = SqlInt32.Null;
			y = new SqlInt32 (5);
			AssertEquals ("Less Than Or Equal operator didn't return Null when one was Null.", x <= y, SqlBoolean.Null);
			AssertEquals ("Less Than Or Equal function didn't return Null when one was Null.", SqlInt32.LessThanOrEqual (x, y), SqlBoolean.Null);

			// Case 2: both are SqlInt32.Null
			y = SqlInt32.Null;
			AssertEquals ("Less Than Or Equal operator didn't return Null when both were Null.", x <= y, SqlBoolean.Null);
			AssertEquals ("Less Than Or Equal function didn't return Null when both were Null.", SqlInt32.LessThanOrEqual (x, y), SqlBoolean.Null);

			// Case 3: x > y
			x = new SqlInt32 (5);
			y = new SqlInt32 (4);
			AssertEquals ("Less than or equal operator didn't return false when x > y.", x <= y, SqlBoolean.False);
			AssertEquals ("Less than or equal function didn't return false when x > y.", SqlInt32.LessThanOrEqual (x,y), SqlBoolean.False);

			// Case 4: x < y
			x = new SqlInt32 (5);
			y = new SqlInt32 (6);
			AssertEquals ("Less than or equal operator didn't return true when x < y.", x <= y, SqlBoolean.True);
			AssertEquals ("Less than or equal function didn't return true when x < y.", SqlInt32.LessThanOrEqual (x,y), SqlBoolean.True);

			// Case 5: x == y
			x = new SqlInt32 (5);
			y = new SqlInt32 (5);
			AssertEquals ("Less than or equal operator didn't return true when x == y.", x <= y, SqlBoolean.True);
			AssertEquals ("Less than or equal function didn't return true when x == y.", SqlInt32.LessThanOrEqual (x,y), SqlBoolean.True);
		}

		public void TestMod () 
		{
			int a = 5;
			int b = 7;
			
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x % y;
			AssertEquals ("Modulus operator does not work correctly", z.Value, a % b);
			z = SqlInt32.Mod (x, y);
			AssertEquals ("Modulus function does not work correctly", z.Value, a % b);
		}

		public void TestMultiply () 
		{
			int a = 5;
			int b = 7;
			
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x * y;
			AssertEquals ("Multiplication operator does not work correctly", z.Value, a * b);
			z = SqlInt32.Multiply (x, y);
			AssertEquals ("Multiplication function does not work correctly", z.Value, a * b);
		}

		public void TestNotEquals () 
		{
			SqlInt32 x;
			SqlInt32 y;

			x = new SqlInt32 (5);
			y = SqlInt32.Null;

			AssertEquals ("Not Equals operator does not return null when one or both of the parameters is Null.", x != y, SqlBoolean.Null);
			AssertEquals ("Not Equals function does not return null when one or both of the parameters is Null.", SqlInt32.NotEquals (x, y), SqlBoolean.Null);

			y = new SqlInt32 (5);
			AssertEquals ("Not Equals operator does not return false when x == y.", x != y, SqlBoolean.False);
			AssertEquals ("Not Equals function does not return false when x == y.", SqlInt32.NotEquals (x, y), SqlBoolean.False);

			y = new SqlInt32 (6);
			AssertEquals ("Not Equals operator does not return true when x != y.", x != y, SqlBoolean.True);
			AssertEquals ("Not Equals function does not return true when x != y.", SqlInt32.NotEquals (x, y), SqlBoolean.True);
		}
	
		public void TestOnesComplement () 
		{
			int a = 5;
			
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 z = ~x;
			AssertEquals ("Ones Complement operator does not work correctly", z.Value, ~a);
			z = SqlInt32.OnesComplement (x);
			AssertEquals ("Ones Complement function does not work correctly", z.Value, ~a);
		}
		
		public void TestIsNullProperty ()
		{
			SqlInt32 n = SqlInt32.Null;
			Assert ("Null is not defined correctly", n.IsNull);
		}
	
		public void TestSubtract () 
		{
			int a = 7;
			int b = 5;
			
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x - y;
			AssertEquals ("Subtraction operator does not work correctly", z.Value, a - b);
			z = SqlInt32.Subtract (x, y);
			AssertEquals ("Subtraction function does not work correctly", z.Value, a - b);
		}

		public void TestConversionMethods ()
		{
			SqlInt32 x;

			// Case 1: SqlInt32.Null -> SqlBoolean == SqlBoolean.Null
			x = SqlInt32.Null;
			AssertEquals ("SqlInt32.Null -> SqlBoolean does not yield SqlBoolean.Null.", x.ToSqlBoolean (), SqlBoolean.Null );

			// Case 2: SqlInt32.Zero -> SqlBoolean == False
			x = SqlInt32.Zero;
			AssertEquals ("SqlInt32.Zero -> SqlBoolean does not yield SqlBoolean.False.", x.ToSqlBoolean (), SqlBoolean.False );
		
			// Case 3: SqlInt32(nonzero) -> SqlBoolean == True
			x = new SqlInt32 (27);
			AssertEquals ("SqlInt32 (27) -> SqlBoolean does not yield SqlBoolean.True.", x.ToSqlBoolean (), SqlBoolean.True );
		
			// Case 4: SqlInt32.Null -> SqlByte == SqlByte.Null
			x = SqlInt32.Null;
			AssertEquals ("SqlInt32.Null -> SqlByte does not yield SqlByte.Null.", x.ToSqlByte (), SqlByte.Null );

			// Case 5: Test non-null conversion to SqlByte
			x = new SqlInt32 (27);
			AssertEquals ("SqlInt32 (27) -> SqlByte does not yield a value of 27", x.ToSqlByte ().Value, (byte)27);

			// Case 6: SqlInt32.Null -> SqlDecimal == SqlDecimal.Null
			x = SqlInt32.Null;
			AssertEquals ("SqlInt32.Null -> SqlDecimal does not yield SqlDecimal.Null.", x.ToSqlDecimal (), SqlDecimal.Null );

			// Case 7: Test non-null conversion to SqlDecimal
			x = new SqlInt32 (27);
			AssertEquals ("SqlInt32 (27) -> SqlDecimal does not yield a value of 27", x.ToSqlDecimal ().Value, (decimal)27);

			// Case 8: SqlInt32.Null -> SqlDouble == SqlDouble.Null
			x = SqlInt32.Null;
			AssertEquals ("SqlInt32.Null -> SqlDouble does not yield SqlDouble.Null.", x.ToSqlDouble (), SqlDouble.Null );

			// Case 9: Test non-null conversion to SqlDouble
			x = new SqlInt32 (27);
			AssertEquals ("SqlInt32 (27) -> SqlDouble does not yield a value of 27", x.ToSqlDouble ().Value, (double)27);

			// Case 10: SqlInt32.Null -> SqlInt16 == SqlInt16.Null
			x = SqlInt32.Null;
			AssertEquals ("SqlInt32.Null -> SqlInt16 does not yield SqlInt16.Null.", x.ToSqlInt16 (), SqlInt16.Null );

			// Case 11: Test non-null conversion to SqlInt16
			x = new SqlInt32 (27);
			AssertEquals ("SqlInt32 (27) -> SqlInt16 does not yield a value of 27", x.ToSqlInt16 ().Value, (short)27);

			// Case 12: SqlInt32.Null -> SqlInt64 == SqlInt64.Null
			x = SqlInt32.Null;
			AssertEquals ("SqlInt32.Null -> SqlInt64 does not yield SqlInt64.Null.", x.ToSqlInt64 (), SqlInt64.Null );

			// Case 13: Test non-null conversion to SqlInt64
			x = new SqlInt32 (27);
			AssertEquals ("SqlInt32 (27) -> SqlInt64 does not yield a value of 27", x.ToSqlInt64 ().Value, (long)27);

			// Case 14: SqlInt32.Null -> SqlMoney == SqlMoney.Null
			x = SqlInt32.Null;
			AssertEquals ("SqlInt32.Null -> SqlMoney does not yield SqlMoney.Null.", x.ToSqlMoney (), SqlMoney.Null );

			// Case 15: Test non-null conversion to SqlMoney
			x = new SqlInt32 (27);
			AssertEquals ("SqlInt32 (27) -> SqlMoney does not yield a value of 27", x.ToSqlMoney ().Value, (decimal)27);

			// Case 16: SqlInt32.Null -> SqlSingle == SqlSingle.Null
			x = SqlInt32.Null;
			AssertEquals ("SqlInt32.Null -> SqlSingle does not yield SqlSingle.Null.", x.ToSqlSingle (), SqlSingle.Null );

			// Case 17: Test non-null conversion to SqlSingle
			x = new SqlInt32 (27);
			AssertEquals ("SqlInt32 (27) -> SqlSingle does not yield a value of 27", x.ToSqlSingle ().Value, (float)27);
		}
	
		public void TestXor () 
		{
			int a = 5;
			int b = 7;
			
			SqlInt32 x = new SqlInt32 (a);
			SqlInt32 y = new SqlInt32 (b);
			SqlInt32 z = x ^ y;
			AssertEquals ("Xor operator does not work correctly", z.Value, a ^ b);
			z = SqlInt32.Xor (x, y);
			AssertEquals ("Xor function does not work correctly", z.Value, a ^ b);
		}
			
	}
}
