//
// System.Collections.DebuggableAttribute.cs
//
// Author:
//   Nick Drochak II (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

namespace System.Diagnostics 
{

	[Serializable]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class ConditionalAttribute : System.Attribute 
	{

		private string myCondition;

		// Public Instance Constructors
		public ConditionalAttribute(string conditionString) 
		{
			myCondition = conditionString;
		}
		
		// Public Instance Properties
		public string ConditionString { get { return myCondition; } 
		}
	}
}
