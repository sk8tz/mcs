// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

namespace TopNS
{
	class Foo
	{
		public /// incorrect
		void FooBar (string foo)
		{
		}
	}

}
