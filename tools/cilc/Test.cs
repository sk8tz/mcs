namespace Demo
{
	using System;

	public class Counter
	{
		int counter;

		public void Increment ()
		{
			counter++;
			Console.WriteLine ("Instance method invoked: Value incremented, making it " + counter);
		}

		public void AddNumber (int num)
		{
			counter += num;
			Console.WriteLine ("Instance method with an argument invoked: " + num + " added to value, making it " + counter);
		}
	}

	public class Test
	{
		string title;
		int counter;

		public static void StaticMethod ()
		{
			Console.WriteLine ("Static method invoked");
		}

		public Test ()
		{
			title = "";
			counter = 0;

			Console.WriteLine ("Class constructor invoked: Value initialised, making it " + counter);
		}

		public void Increment ()
		{
			counter++;
			Console.WriteLine ("Instance method invoked: Value incremented, making it " + counter);
		}

		public void AddNumber (int num)
		{
			counter += num;
			Console.WriteLine ("Instance method with an argument invoked: " + num + " added to value, making it " + counter);
		}

		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		public void Echo (string arg1string)
		{
			Console.WriteLine ("string: " + arg1string);
		}

		public void Method4 (string arg1string, int arg2int)
		{
			Console.WriteLine (arg1string + arg2int.ToString ());
		}

		public void GTypeGTypeGType ()
		{
			Console.WriteLine ("c# method with an unusual name invoked");
		}
	}
}
