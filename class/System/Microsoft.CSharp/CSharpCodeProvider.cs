//
// Microsoft.CSharp CSharpCodeProvider Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

namespace Microsoft.CSharp
{
	using System;
	using System.CodeDom.Compiler;
	using System.ComponentModel;

	public class CSharpCodeProvider
		: CodeDomProvider
	{
		//
		// Constructors
		//
		CSharpCodeProvider()
		{
		}

		//
		// Properties
		//
		public override string FileExtension {
			get {
				return "cs";
			}
		}

		//
		// Methods
		//
		[MonoTODO]
		public override ICodeCompiler CreateCompiler()
		{
			throw new NotImplementedException();
		}

		public override ICodeGenerator CreateGenerator()
		{
			return new Mono.CSharp.CSharpCodeGenerator();
		}
		
		[MonoTODO]
		public override TypeConverter GetConverter( Type Type )
		{
			throw new NotImplementedException();
		}
	}
}
