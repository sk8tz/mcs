using System;
using System.Collections;

public class X
{
	public static int Main ()
	{
		// This is a compilation-only test.
		return 0;
	}

	// All code paths throw an exception, no need to set out parameters.
	public static void test1 (out float f)
	{
		throw new NotSupportedException ();
	}

	// The while loop breaks but does not return, so this is ok.
	public static void test2 (int a, out float f)
	{
		while (a > 0) {
			if (a == 5)
				continue;

			Console.WriteLine (a);
		}

		f = 8.53F;
	}

	// a has been assigned in all code paths which do not return.
	public static void test3 (long[] b, int c)
	{
		ICollection a;
		if (b == null)
			throw new ArgumentException ();
		else
			a = (ICollection) b;

		Console.WriteLine (a);
	}

	// Forward goto, it's ok to set f after the target label.
	public static int test4 (int b, out float f)
	{
		long a;

		Console.WriteLine ("Hello World");

		a = 5;

		goto World;

	World:
		Console.WriteLine (a);

		f = 8.53F;

		return 0;
	}

	// try { ... } catch { ... } finally { ... } block
	static public int test5 (out float f, long d)
	{
                int a;
		long b = 8;

		try {
			f = 8.53F;

			if (d == 500)
				return 9;

			a = 5;
		} catch (NotSupportedException e) {
			a = 9;
		} catch (Exception e) {
			return 9;
		} finally {
			f = 9.234F;
		}

		return a;
        }

	// Passing out parameter to method invocation
	static public int test6 (out float f)
	{
		return test5 (out f, 50);
	}

	// Loop-variable of foreach() and for() loop.
	static public long test7 (int[] a, int stop)
	{
		long b = 0;
		foreach (int i in a)
			b += i;

		for (int i = 1; i < stop; i++)
			b *= i;

		return b;
	}

	// Initializing locals in initialize or test of for block
	static public long test8 (int stop)
	{
		int i;
		long b;
		for (i = 1; (b = stop) > 3; i++) {
			stop--;
			b += i;
		}
		return b;
	}

	// Initializing locals in test of while block
	static public long test9 (int stop)
	{
		long b;
		while ((b = stop) > 3) {
			stop--;
			b += stop;
		}
		return b;
	}

	// Return in subblock
	public static void test10 (int a, out float f)
	{
		if (a == 5) {
			f = 8.53F;
			return;
		}

		f = 9.0F;
	}

	// Switch block
	public static long test11 (int a)
	{
		long b;

		switch (a) {
		case 5:
			b = 1;
			break;

		case 9:
			b = 3;
			break;

		default:
			return 9;
		}

		return b;
	}

	// Try block which rethrows exception.
	public static void test12 (out float f)
	{
		try {
			f = 9.0F;
		} catch {
			throw new NotSupportedException ();
		}
	}

	// Return in subblock.
	public static void test13 (int a, out float f)
	{
		do {
			if (a == 8) {
				f = 8.5F;
				return;
			}
		} while (false);

		f = 1.3F;
		return;
	}

	// Switch block with goto case / goto default.
	public static long test14 (int a, out float f)
	{
		long b;

		switch (a) {
		case 1:
			goto case 2;

		case 2:
			f = 9.53F;
			return 9;

		case 3:
			goto default;

		default:
			b = 10;
			break;
		}

		f = 10.0F;

		return b;
	}

	// Forward goto, it's ok to set f before the jump.
	public static int test15 (int b, out float f)
	{
		long a;

		Console.WriteLine ("Hello World");

		a = 5;
		f = 8.53F;

		goto World;

	World:
		Console.WriteLine (a);

		return 0;
	}

	// `continue' breaks unless we're a loop block.
	public static void test16 ()
	{
                int value;

                for (int i = 0; i < 5; ++i) {
                        if (i == 0) {
                                continue;
                        } else if (i == 1) {
                                value = 2;
                        } else {
                                value = 0;
                        }
                        if (value > 0)
                                return;
                }
	}

	// `continue' in a nested if.
	public static void test17 ()
	{
		 int value;
		 long charCount = 9;
		 long testit = 5;

		 while (charCount > 0) {
			 --charCount;

			 if (testit == 8) {
				 if (testit == 9)
					 throw new Exception ();

				 continue;
			 } else {
				 value = 0;
			 }

			 Console.WriteLine (value);
		 }
	}

	// `out' parameter assigned after conditional exception.
	static void test18 (int a, out int f)
	{
		try {
			if (a == 5)
				throw new Exception ();

			f = 9;
		} catch (IndexOutOfRangeException) {
			throw new FormatException ();
		}
	}
}
