// MulticastDelegate.cs - NUnit Test Cases for MulticastDelegates (C# delegates)
//
// Daniel Stodden (stodden@in.tum.de)
//
// (C) Daniel Stodden
// 

// these are the standard namespaces you will need.  You may need to add more
// depending on your tests.
using NUnit.Framework;
using System;

// all test namespaces start with "MonoTests."  Append the Namespace that
// contains the class you are testing, e.g. MonoTests.System.Collections
namespace MonoTests.System
{

// the class name should end with "Test" and start with the name of the class
// you are testing, e.g. CollectionBaseTest
public class MulticastDelegateTest : TestCase {
	
	// there should be two constructors for your class.  The first one
	// (without parameters) should set the name to something unique.
	// Of course the name of the method is the same as the name of the
	// class
	public MulticastDelegateTest() : base ("System.MulticastDelegate") {}
	public MulticastDelegateTest(string name) : base(name) {}

	// this method is run before each Test* method is called. You can put
	// variable initialization, etc. here that is common to each test.
	// Just leave the method empty if you don't need to use it.
	protected override void SetUp() {}

	// this method is run after each Test* method is called. You can put
	// clean-up code, etc. here.  Whatever needs to be done after each test.
	// Just leave the method empty if you don't need to use it.
	protected override void TearDown() {}

	// this property is required.  You need change the parameter for
	// typeof() below to be your class.
	public static ITest Suite {
		get {
			return new TestSuite(typeof(MulticastDelegateTest));
		}
	}

	private delegate char MyDelegate( ref string s );

	private char MethodA( ref string s ) 
	{
		s += "a";
		return 'a';
	}

	private char MethodB( ref string s )
	{
		s += "b";
		return 'b';
	}

	private char MethodC( ref string s )
	{
		s += "c";
		return 'c';
	}

	private char MethodD( ref string s )
	{
		s += "d";
		return 'd';
	}

	public void TestEquals()
	{
		MyDelegate dela = new MyDelegate( MethodA );
		MyDelegate delb = new MyDelegate( MethodB );
		MyDelegate delc = new MyDelegate( MethodC );

		AssertEquals( "#A01", false, dela == delb );
		
		MyDelegate del1, del2;

		del1 = dela + delb;
		del2 = delb + delc;
		AssertEquals( "#A02", false, del1 == del2 );
		
		del1 += delc;
		del2 = dela + del2;
		AssertEquals( "#A03", true, del1 == del2 );
	}

	public void TestCombineRemove()
	{
		MyDelegate dela = new MyDelegate( MethodA );
		MyDelegate delb = new MyDelegate( MethodB );
		MyDelegate delc = new MyDelegate( MethodC );
		MyDelegate deld = new MyDelegate( MethodD );

		string val;
		char res;

		// test combine
		MyDelegate del1, del2;
		del1 = dela + delb + delb + delc + delb + delb + deld;
		val = "";
		res = del1( ref val );
		// FIXME: this is bug #28306 - delegate order is reversed
		//   http://bugzilla.ximian.com/show_bug.cgi?id=28306
		// AssertEquals( "#A01", "abbcbbd", val );
		// AssertEquals( "#A02", 'd', res );

		// test remove
		del2 = del1 - ( delb + delb );
		val = "";
		res = del2( ref val );
		// FIXME: this is bug #28306 - delegate order is reversed
		//   http://bugzilla.ximian.com/show_bug.cgi?id=28306
		//AssertEquals( "#A03", "abbcd", val );
		//AssertEquals( "#A04", 'd', res );

		// we did not affect del1, did we?
		val = "";
		res = del1( ref val );
		// FIXME: this is bug #28306 - delegate order is reversed
		//   http://bugzilla.ximian.com/show_bug.cgi?id=28306
		//AssertEquals( "#A05", "abbcbbd", val );
	}
}
}
