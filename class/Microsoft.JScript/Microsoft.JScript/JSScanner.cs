//
// JSScanner.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.IO;

	public sealed class JSScanner
	{
		internal JScriptLexer Lexer;

		public JSScanner ()
		{}

		public JSScanner (Context sourceContext)
		{
			string filename = sourceContext.Document.Name;
			StreamReader file = new StreamReader (filename);

			Lexer = new JScriptLexer (file);
		}


		public void SetAuthoringMode (bool mode)
		{
			throw new NotImplementedException ();
		}


		public void SetSource (Context sourceContext)
		{
			throw new NotImplementedException ();
		}


		public void GetNextToken ()
		{
			throw new NotImplementedException ();
		}


		public int GetCurrentPosition (bool absolute)
		{
			throw new NotImplementedException ();
		}


		public int GetCurrentLine ()
		{
			throw new NotImplementedException ();
		}


		public int GetStartLinePosition ()
		{
			throw new NotImplementedException ();
		}


		public string GetStringLiteral ()
		{
			throw new NotImplementedException ();
		}


		public string GetSourceCode ()
		{
			throw new NotImplementedException ();
		}


		public bool GetEndOfLine ()
		{
			throw new NotImplementedException ();
		}


		public int SkipMultiLineComment ()
		{
			throw new NotImplementedException ();
		}


		public static bool IsOperator (JSToken token)
		{
			throw new NotImplementedException ();
		}


		public static bool IsKeyboard (JSToken token)
		{
				throw new NotImplementedException ();
		}
	}
}
