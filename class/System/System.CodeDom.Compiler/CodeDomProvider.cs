//
// System.CodeDom.Compiler CodeDomProvider Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.ComponentModel;
using System.IO;

namespace System.CodeDom.Compiler
{
	public abstract class CodeDomProvider
		: Component
	{
		//
		// Constructors
		//
		protected CodeDomProvider()
		{
		}

		//
		// Properties
		//
		public virtual string FileExtension {
			get {
				return String.Empty;
			}
		}

		public virtual LanguageOptions LanguageOptions {
			get {
				return LanguageOptions.None;
			}
		}

		//
		// Methods
		//
		public abstract ICodeCompiler CreateCompiler();

		public abstract ICodeGenerator CreateGenerator();
		
		public virtual ICodeGenerator CreateGenerator( string fileName )
		{
			return CreateGenerator();
		}

		public virtual ICodeGenerator CreateGenerator( TextWriter output )
		{
			return CreateGenerator();
		}

		public virtual ICodeParser CreateParser()
		{
			return null;
		}

		[MonoTODO]
		public virtual TypeConverter GetConverter( Type type )
		{
			throw new NotImplementedException();
		}

	}
}
