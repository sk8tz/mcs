using System.Windows;

namespace Xaml.TestVocab.Console {
	public class ConsoleReader : DependencyObject, IConsoleAction {
		private string variable;
		private ConsoleWriter prompt;

		public string Variable {
			get { return variable; }
			set { variable = value; }
		}

		public ConsoleWriter Prompt {
			get { return prompt; }
			set { prompt = value; }
		}
		
		public void Run() {
			prompt.Run();
			string s = System.Console.ReadLine();
			ConsoleVars.Set(variable, s);
		}

		public override bool Equals(object o)
		{
			ConsoleReader reader = (ConsoleReader)o;
			return (reader.variable == variable) && (reader.prompt == prompt);
		}
		public override int GetHashCode()
		{
			return variable.GetHashCode() + prompt.GetHashCode();
		}
	}
}
