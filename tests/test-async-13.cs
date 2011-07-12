// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Linq;

struct S
{
	public int Value;
	
	public void SetValue (int value)
	{
		Value = value;
	}
}

enum E
{
	E_1 = 1
}

class Base
{
	protected int field_int;
	protected int field_this;
	protected int property_this_counter;
	
	public event Action Event;
	
	public bool PropertyBool {
		get {
			return true;
		}
	}
	
	public int PropertyInt {
		get {
			return field_int;
		}
		set {
			field_int += value;
		}
	}
	
	protected Base PropertyThis {
		get {
			++property_this_counter;
			return this;
		}
	}
	
	public int this [int arg] {
		get {
			return field_this;
		}
		set {
			field_this += value * arg;
		}
	}
	
	protected int Call (int arg1, int arg2, int arg3)
	{
		if (arg1 != 5)
			return 1;

		if (arg2 != -3)
			return 2;

		if (arg3 != 6)
			return 3;

		return 0;
	}
	
	protected int Call(ref char ch, int arg)
	{
		ch = 'z';
		return arg;
	}
	
	protected int CallNamed (int a, int b)
	{
		return a - b;
	}
	
	protected void CallEvent ()
	{
		Event ();
	}
}

class Tester : Base
{
	async Task<int> AssignTest_1 ()
	{
		field_int = await Task.Factory.StartNew (() => 0);
		return field_int;
	}
	
	async Task<int> BinaryTest_1 ()
	{
		return await Task.Factory.StartNew (() => { Thread.Sleep (10); return 5; }) +
			await Task.Factory.StartNew (() => -3) +
			await Task.Factory.StartNew (() => -2);
	}
	
	async Task<int> BinaryTest_2 ()
	{
		int i = 1;
		var b = await Task.Factory.StartNew (() => { i += 3; return true; }) &&
			await Task.Factory.StartNew (() => { i += 4; return false; }) &&
			await Task.Factory.StartNew (() => { i += 5; return true; });

		return b ? -1 : i == 8 ? 0 : i;
	}
	
	async Task<int> CallTest_1 ()
	{
		return Call (
			await Task.Factory.StartNew (() => { Thread.Sleep (10); return 5; }),
			await Task.Factory.StartNew (() => -3),
			await Task.Factory.StartNew (() => 6));
	}
	
	async Task<bool> CallTest_2 ()
	{
		char ch = 'a';
		var r = Call (
			ref ch, 
			await Task.Factory.StartNew (() => { Thread.Sleep (10); return 5; }));
		
		return ch == 'z' && r == 5;
	}

	async Task<int> CallTest_3 ()
	{
		S s = new S ();
		s.SetValue (await Task.Factory.StartNew (() => 10));
		return s.Value - 10;
	}

	async Task<bool> CallTest_4 ()
	{
		return E.E_1.Equals (await Task.Factory.StartNew (() => E.E_1));
	}

	async Task<int> CallTest_5 ()
	{
		int value = 9;
		return CallNamed (
			b: await Task.Factory.StartNew (() => value++),
			a: value) - 1;
	}

	async Task<int> ConditionalTest_1 ()
	{
		// TODO: problem with Resumable point setup when the expression never emitted
		//bool b = true;
		//return true ? await Task.Factory.StartNew (() => 0) : await Task.Factory.StartNew (() => 1);
		return 0;
	}
	
	async Task<int> ConditionalTest_2 ()
	{
		return PropertyBool ? await Task.Factory.StartNew (() => 0) : await Task.Factory.StartNew (() => 1);
	}
	
	async Task<int> ConditionalTest_3 ()
	{
		int v = 5;
		return v * (await Task.Factory.StartNew (() => true) ? 0 : await Task.Factory.StartNew (() => 1));
	}
	
	async Task<int> ConditionalTest_4 ()
	{
		int v = 5;
		return v * (v == 2 ? 3 : await Task.Factory.StartNew (() => 0));
	}
	
	async Task<int> DelegateInvoke_4 ()
	{
		Func<int, int> d = l => l - 3;
		int value = 1;
		return value + d (await Task.Factory.StartNew (() => 2));
	}
	
	async Task<int> EventInvoke_1 ()
	{
		int value = 0;
		Event += await Task.Factory.StartNew (() => {
			Action a = () => { value = 5; };
			return a;
		});
		
		CallEvent ();
		return value - 5;
	}

	async Task<bool> IndexerTest_1 ()
	{
		this[2] = await Task.Factory.StartNew (() => 6);
		return this[2] == 12;
	}

	async Task<bool> IndexerTest_2 ()
	{
		this[await Task.Factory.StartNew (() => 3)] = await Task.Factory.StartNew (() => 6);
		return this[3] == 18;
	}
	
	async Task<int> IndexerTest_3 ()
	{
		int value = 3;
		this[await Task.Factory.StartNew (() => value++)] += await Task.Factory.StartNew (() => 5);
		return this[3] - 15;
	}
	
	async Task<int> IndexerTest_4 ()
	{
		int value = 3;
		PropertyThis[await Task.Factory.StartNew (() => value++)] += await Task.Factory.StartNew (() => -5);
		return PropertyThis[3] + value + 11;
	}
	
	async Task<int> IndexerTest_5 ()
	{
		int value = 3;
		field_this = 6;
		int res = PropertyThis[await Task.Factory.StartNew (() => value++)]++;
		if (res != 6)
			return 1;

		if (PropertyThis[0] != 27)
			return 2;

		return 0;
	}
	
	async Task<bool> NewArrayInitTest_1 ()
	{
		var a = new int[await Task.Factory.StartNew (() => 5)];
		return a.Length == 5;
	}
	
	async Task<bool> NewArrayInitTest_2 ()
	{
		var a = new short[await Task.Factory.StartNew (() => 3), await Task.Factory.StartNew (() => 4)];
		return a.Length == 12;
	}
	
	async Task<int> NewArrayInitTest_3 ()
	{
		var a = new byte[] { await Task.Factory.StartNew (() => (byte)5) };
		return a [0] - 5;
	}
	
	async Task<bool> NewArrayInitTest_4 ()
	{
		var a = new ushort[,] {
			{ await Task.Factory.StartNew (() => (ushort) 5), 50 },
			{ 30, await Task.Factory.StartNew (() => (ushort) 3) }
		};
		
		return a [0, 0] * a [1, 1] == 15;
	}
	
	async Task<int> NewArrayInitTest_5 ()
	{
		var a = new S[] { await Task.Factory.StartNew (() => new S () { Value = 4 }) };
		return a [0].Value - 4;
	}

	async Task<bool> PropertyTest_1 ()
	{
		PropertyInt = await Task.Factory.StartNew (() => 6);
		return PropertyInt == 6;
	}
	
	async Task<int> PropertyTest_2 ()
	{
		PropertyThis.PropertyInt += await Task.Factory.StartNew (() => 6);
		if (property_this_counter != 1)
			return 1;
		
		return PropertyInt - 6;
	}
	
	async Task<bool> StringConcatTest_1 ()
	{
		return (await Task.Factory.StartNew (() => "a") +
			await Task.Factory.StartNew (() => "b") +
			await Task.Factory.StartNew (() => (string) null) == "ab");
	}
	
	static bool RunTest (MethodInfo test)
	{
		Console.Write ("Running test {0, -25}", test.Name);
		try {
			Task t = test.Invoke (new Tester (), null) as Task;
			if (!Task.WaitAll (new[] { t }, 1000)) {
				Console.WriteLine ("FAILED (Timeout)");
				return false;
			}

			var ti = t as Task<int>;
			if (ti != null) {
				if (ti.Result != 0) {
					Console.WriteLine ("FAILED (Result={0})", ti.Result);
					return false;
				}
			} else {
				var tb = t as Task<bool>;
				if (tb != null) {
					if (!tb.Result) {
						Console.WriteLine ("FAILED (Result={0})", tb.Result);
						return false;
					}
				}
			}

			Console.WriteLine ("OK");
			return true;
		} catch (Exception e) {
			Console.WriteLine ("FAILED");
			Console.WriteLine (e.ToString ());
			return false;
		}
	}

	public static int Main ()
	{
		var tests = from test in typeof (Tester).GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
					where test.GetParameters ().Length == 0
					orderby test.Name
					select RunTest (test);

		int failures = tests.Count (a => !a);
		Console.WriteLine (failures + " tests failed");
		return failures;
	}
}
