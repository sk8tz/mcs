//
// Mono.CSharp CSharpCodeProvider Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

namespace Mono.CSharp
{
	using System;
	using System.CodeDom;
	using System.CodeDom.Compiler;
	using System.IO;
	using System.Reflection;
	using System.Collections;

	internal class CSharpCodeGenerator
		: CodeGenerator
	{
		//
		// Constructors
		//
		public CSharpCodeGenerator()
		{
		}

		//
		// Properties
		//
		protected override string NullToken {
			get {
				return "null";
			}
		}

		//
		// Methods
		//

		protected override void GenerateArrayCreateExpression( CodeArrayCreateExpression expression )
		{
			//
			// This tries to replicate MS behavior as good as
			// possible.
			//
			// The Code-Array stuff in ms.net seems to be broken
			// anyways, or I'm too stupid to understand it.
			//
			// I'm sick of it. If you try to develop array
			// creations, test them on windows. If it works there
			// but not in mono, drop me a note.  I'd be especially
			// interested in jagged-multidimensional combinations
			// with proper initialization :}
			//

			TextWriter output = Output;

			output.Write( "new " );

			CodeExpressionCollection initializers = expression.Initializers;
			CodeTypeReference createType = expression.CreateType;

			if ( initializers.Count > 0 ) {

				OutputType( createType );
				
				output.WriteLine( "[] {" );
				++Indent;
				OutputExpressionList( initializers, true );
				--Indent;
				output.Write( "}" );

			} else {
				CodeTypeReference arrayType = createType.ArrayElementType;
				while ( arrayType != null ) {
					createType = arrayType;
					arrayType = arrayType.ArrayElementType;
				}

				OutputType( createType );

				output.Write( '[' );

				CodeExpression size = expression.SizeExpression;
				if ( size != null )
					GenerateExpression( size );
				else
					output.Write( expression.Size );

				output.Write( ']' );
			}
		}
		
		protected override void GenerateBaseReferenceExpression( CodeBaseReferenceExpression expression )
		{
			Output.Write( "base" );
		}
		
		protected override void GenerateCastExpression( CodeCastExpression expression )
		{
			TextWriter output = Output;
			output.Write( "((" );
			OutputType( expression.TargetType );
			output.Write( ")(" );
			GenerateExpression( expression.Expression );
			output.Write( "))" );
		}


		protected override void GenerateCompileUnitStart( CodeCompileUnit compileUnit )
		{
			GenerateComment( new CodeComment( "------------------------------------------------------------------------------" ) );
			GenerateComment( new CodeComment( " <autogenerated>" ) );
			GenerateComment( new CodeComment( "     This code was generated by a tool." ) );
			GenerateComment( new CodeComment( "     Mono Runtime Version: " +  System.Environment.Version ) );
			GenerateComment( new CodeComment( "" ) );
			GenerateComment( new CodeComment( "     Changes to this file may cause incorrect behavior and will be lost if " ) );
			GenerateComment( new CodeComment( "     the code is regenerated." ) );
			GenerateComment( new CodeComment( " </autogenerated>" ) );
			GenerateComment( new CodeComment( "------------------------------------------------------------------------------" ) );
			Output.WriteLine();
		}


		protected override void GenerateDelegateCreateExpression( CodeDelegateCreateExpression expression )
		{
			TextWriter output = Output;

			output.Write( "new " );

			OutputType( expression.DelegateType );

			output.Write( '(' );

			CodeExpression targetObject = expression.TargetObject;
			if ( targetObject != null ) {
				GenerateExpression( targetObject );
				Output.Write( '.' );
			}
			output.Write( GetSafeName (expression.MethodName) );

			output.Write( ')' );
		}

		protected override void GenerateFieldReferenceExpression( CodeFieldReferenceExpression expression )
		{
			CodeExpression targetObject = expression.TargetObject;
			if ( targetObject != null ) {
				GenerateExpression( targetObject );
				Output.Write( '.' );
			}
			Output.Write( GetSafeName (expression.FieldName) );
		}
		
		protected override void GenerateArgumentReferenceExpression( CodeArgumentReferenceExpression expression )
		{
			Output.Write( GetSafeName (expression.ParameterName) );
		}

		protected override void GenerateVariableReferenceExpression( CodeVariableReferenceExpression expression )
		{
			Output.Write( GetSafeName (expression.VariableName) );
		}
			
		protected override void GenerateIndexerExpression( CodeIndexerExpression expression )
		{
			TextWriter output = Output;

			GenerateExpression( expression.TargetObject );
			output.Write( '[' );
			OutputExpressionList( expression.Indices );
			output.Write( ']' );
		}
		
		protected override void GenerateArrayIndexerExpression( CodeArrayIndexerExpression expression )
		{
			TextWriter output = Output;

			GenerateExpression( expression.TargetObject );
			output.Write( '[' );
			OutputExpressionList( expression.Indices );
			output.Write( ']' );
		}
		
		protected override void GenerateSnippetExpression( CodeSnippetExpression expression )
		{
			Output.Write( expression.Value );
		}
		
		protected override void GenerateMethodInvokeExpression( CodeMethodInvokeExpression expression )
		{
			TextWriter output = Output;

			GenerateMethodReferenceExpression( expression.Method );

			output.Write( '(' );
			OutputExpressionList( expression.Parameters );
			output.Write( ')' );
		}

		protected override void GenerateMethodReferenceExpression( CodeMethodReferenceExpression expression )
		{
			if (expression.TargetObject != null)
			{
				GenerateExpression( expression.TargetObject );
				Output.Write( '.' );
			};
			Output.Write( GetSafeName (expression.MethodName) );
		}

		protected override void GenerateEventReferenceExpression( CodeEventReferenceExpression expression )
		{
			GenerateExpression( expression.TargetObject );
			Output.Write( '.' );
			Output.Write( GetSafeName (expression.EventName) );
		}

		protected override void GenerateDelegateInvokeExpression( CodeDelegateInvokeExpression expression )
		{
			GenerateExpression( expression.TargetObject );
			Output.Write( '(' );
			OutputExpressionList( expression.Parameters );
			Output.Write( ')' );
		}
		
		protected override void GenerateObjectCreateExpression( CodeObjectCreateExpression expression )
		{
			Output.Write( "new " );
			OutputType( expression.CreateType );
			Output.Write( '(' );
			OutputExpressionList( expression.Parameters );
			Output.Write( ')' );
		}

		protected override void GeneratePropertyReferenceExpression( CodePropertyReferenceExpression expression )
		{
			CodeExpression targetObject = expression.TargetObject;
			if ( targetObject != null ) {
				GenerateExpression( targetObject );
				Output.Write( '.' );
			}
			Output.Write( GetSafeName (expression.PropertyName ) );
		}

		protected override void GeneratePropertySetValueReferenceExpression( CodePropertySetValueReferenceExpression expression )
		{
			Output.Write ( "value" );	
		}

		protected override void GenerateThisReferenceExpression( CodeThisReferenceExpression expression )
		{
			Output.Write( "this" );
		}

		protected override void GenerateExpressionStatement( CodeExpressionStatement statement )
		{
			GenerateExpression( statement.Expression );
			Output.WriteLine( ';' );
		}

		protected override void GenerateIterationStatement( CodeIterationStatement statement )
		{
			TextWriter output = Output;

			output.Write( "for (" );
			GenerateStatement( statement.InitStatement );
			output.Write( "; " );
			GenerateExpression( statement.TestExpression );
			output.Write( "; " );
			GenerateStatement( statement.IncrementStatement );
			output.Write( ") " );
			GenerateStatements( statement.Statements );
		}

		protected override void GenerateThrowExceptionStatement( CodeThrowExceptionStatement statement )
		{
			Output.Write( "throw" );
			if (statement.ToThrow != null) {
				Output.Write (' ');
				GenerateExpression (statement.ToThrow);
			}
			Output.WriteLine(";");
		}

		protected override void GenerateComment( CodeComment comment )
		{
			TextWriter output = Output;
			string[] lines = comment.Text.Split ('\n');
			bool first = true;
			foreach (string line in lines){
				if ( comment.DocComment )
					output.Write( "///" );
				else
					output.Write( "//" );
				if (first) {
					output.Write (' ');
					first = false;
				}
				output.WriteLine( line );
    		}
		}

		protected override void GenerateMethodReturnStatement( CodeMethodReturnStatement statement )
		{
			TextWriter output = Output;

			output.Write( "return " );

			GenerateExpression( statement.Expression );

			output.WriteLine ( ";" );
		}

		protected override void GenerateConditionStatement( CodeConditionStatement statement )
		{
			TextWriter output = Output;
			output.Write( "if (" );

			GenerateExpression( statement.Condition );

			output.WriteLine( ") {" );
			++Indent;
			GenerateStatements( statement.TrueStatements );
			--Indent;

			CodeStatementCollection falses = statement.FalseStatements;
			if ( falses.Count > 0 ) {
				output.Write( '}' );
				if ( Options.ElseOnClosing )
					output.Write( ' ' );
				else
					output.WriteLine();
				output.WriteLine( "else {" );
				++Indent;
				GenerateStatements( falses );
				--Indent;
			}
			output.WriteLine( '}' );
		}

		protected override void GenerateTryCatchFinallyStatement( CodeTryCatchFinallyStatement statement )
		{
			TextWriter output = Output;
			CodeGeneratorOptions options = Options;

			output.WriteLine( "try {" );
			++Indent;
			GenerateStatements( statement.TryStatements );
			--Indent;
			output.Write( '}' );
			
			foreach ( CodeCatchClause clause in statement.CatchClauses ) {
				if ( options.ElseOnClosing )
					output.Write( ' ' );
				else
					output.WriteLine();
				output.Write( "catch (" );
				OutputTypeNamePair( clause.CatchExceptionType, GetSafeName (clause.LocalName) );
				output.WriteLine( ") {" );
				++Indent;
				GenerateStatements( clause.Statements );
				--Indent;
				output.Write( '}' );
			}

			CodeStatementCollection finallies = statement.FinallyStatements;
			if ( finallies.Count > 0 ) {
				if ( options.ElseOnClosing )
					output.Write( ' ' );
				else
					output.WriteLine();
				output.WriteLine( "finally {" );
				++Indent;
				GenerateStatements( finallies );
				--Indent;
				output.WriteLine( '}' );
			}

			output.WriteLine();
		}

		protected override void GenerateAssignStatement( CodeAssignStatement statement )
		{			
			TextWriter output = Output;
			GenerateExpression( statement.Left );
			output.Write( " = " );
			GenerateExpression( statement.Right );
			output.WriteLine( ';' );
		}

		protected override void GenerateAttachEventStatement( CodeAttachEventStatement statement )
		{
			TextWriter output = Output;

			GenerateEventReferenceExpression( statement.Event );
			output.Write( " += " );
			GenerateExpression( statement.Listener );
			output.WriteLine( ';' );
		}

		protected override void GenerateRemoveEventStatement( CodeRemoveEventStatement statement )
		{
			TextWriter output = Output;
			GenerateEventReferenceExpression( statement.Event );
			Output.Write( " -= " );
			GenerateExpression( statement.Listener );
			output.WriteLine( ';' );
		}

		protected override void GenerateGotoStatement( CodeGotoStatement statement )
		{
			TextWriter output = Output;

			output.Write( "goto " );
			output.Write( GetSafeName (statement.Label) );
			output.Write( ";" );
		}
		
		protected override void GenerateLabeledStatement( CodeLabeledStatement statement )
		{
			TextWriter output = Output;

			output.Write( GetSafeName (statement.Label) );
			GenerateStatement( statement.Statement );
		}

		protected override void GenerateVariableDeclarationStatement( CodeVariableDeclarationStatement statement )
		{
			TextWriter output = Output;

			OutputTypeNamePair( statement.Type, GetSafeName (statement.Name) );

			CodeExpression initExpression = statement.InitExpression;
			if ( initExpression != null ) {
				output.Write( " = " );
				GenerateExpression( initExpression );
			}

			output.WriteLine( ';' );
		}

		protected override void GenerateLinePragmaStart (CodeLinePragma linePragma)
		{
			Output.WriteLine ();
			Output.Write ("#line ");
			Output.Write (linePragma.LineNumber);
			Output.Write (" \"");
			Output.Write (linePragma.FileName);
			Output.Write ("\"");
			Output.WriteLine ();
		}

		protected override void GenerateLinePragmaEnd (CodeLinePragma linePragma)
		{
			Output.WriteLine ();
			Output.WriteLine ("#line default");
		}

		[MonoTODO]
		protected override void GenerateEvent( CodeMemberEvent eventRef, CodeTypeDeclaration declaration )
		{
			Output.Write( "<GenerateEvent>" );
		}

		protected override void GenerateField( CodeMemberField field )
		{
			TextWriter output = Output;

			if (field.CustomAttributes.Count > 0)
				OutputAttributeDeclarations( field.CustomAttributes );

			MemberAttributes attributes = field.Attributes;
			OutputMemberAccessModifier( attributes );
			OutputFieldScopeModifier( attributes );

			OutputTypeNamePair( field.Type, GetSafeName (field.Name) );

			CodeExpression initExpression = field.InitExpression;
			if ( initExpression != null ) {
				output.Write( " = " );
				GenerateExpression( initExpression );
			}

			if (IsCurrentEnum)
				output.WriteLine( ',' );
			else
				output.WriteLine( ';' );
		}
		
		protected override void GenerateSnippetMember( CodeSnippetTypeMember member )
		{
			Output.Write (member.Text);
		}
		
		protected override void GenerateEntryPointMethod( CodeEntryPointMethod method, 
								  CodeTypeDeclaration declaration )
		{
			method.Name = "Main";
			GenerateMethod( method, declaration );
		}
		
		protected override void GenerateMethod( CodeMemberMethod method,
							CodeTypeDeclaration declaration )
		{
			TextWriter output = Output;

			if (method.CustomAttributes.Count > 0)
				OutputAttributeDeclarations( method.CustomAttributes );

			if (method.ReturnTypeCustomAttributes.Count > 0)
				OutputAttributeDeclarations( method.ReturnTypeCustomAttributes );

			MemberAttributes attributes = method.Attributes;

			if (method.PrivateImplementationType == null && !declaration.IsInterface)
				OutputMemberAccessModifier( attributes );

			if (!declaration.IsInterface)
				OutputMemberScopeModifier( attributes );

			OutputType( method.ReturnType );

			output.Write( ' ' );

			CodeTypeReference privateType = method.PrivateImplementationType;
			if ( privateType != null ) {
				OutputType( privateType );
				output.Write( '.' );
			}
			output.Write( GetSafeName (method.Name) );

			output.Write( '(' );
			OutputParameters( method.Parameters );
			output.Write( ')' );

			if ( (attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract || declaration.IsInterface)
				output.WriteLine( ';' );
			else {
				output.WriteLine( " {" );
				++Indent;
				GenerateStatements( method.Statements );
				--Indent;
				output.WriteLine( '}' );
			}
		}

		protected override void GenerateProperty( CodeMemberProperty property,
							  CodeTypeDeclaration declaration )
		{
			TextWriter output = Output;

			if (property.CustomAttributes.Count > 0)
				OutputAttributeDeclarations( property.CustomAttributes );

			MemberAttributes attributes = property.Attributes;
			OutputMemberAccessModifier( attributes );
			OutputMemberScopeModifier( attributes );

			if (property.Name == "Item")
			{
				// indexer
				
				OutputTypeNamePair( property.Type, "this");
				output.Write("[");
				OutputParameters(property.Parameters);
				output.Write("]");
			}
			else
			{
				OutputTypeNamePair( property.Type, GetSafeName (property.Name));
			}
			output.WriteLine (" {");
			++Indent;

			if (declaration.IsInterface)
			{
				if (property.HasGet) output.WriteLine("get; ");
				if (property.HasSet) output.WriteLine("set; ");
			}
			else
			{
				if (property.HasGet)
				{
					output.WriteLine ("get {");
					++Indent;

					GenerateStatements (property.GetStatements);

					--Indent;
					output.WriteLine ("}");
				}

				if (property.HasSet)
				{
					output.WriteLine ("set {");
					++Indent;

					GenerateStatements (property.SetStatements);

					--Indent;
					output.WriteLine ("}");
				}
			}

			--Indent;
			output.WriteLine ("}");
		}

		protected override void GenerateConstructor( CodeConstructor constructor,
							     CodeTypeDeclaration declaration )
		{
			OutputMemberAccessModifier (constructor.Attributes);
			Output.Write (GetSafeName (CurrentTypeName) + " (");
			OutputParameters (constructor.Parameters);
			Output.Write (") ");
			if (constructor.ChainedConstructorArgs.Count > 0)
			{
				Output.Write(": this(");
				bool first = true;
				foreach (CodeExpression ex in constructor.ChainedConstructorArgs)
				{
					if (!first)
						Output.Write(", ");
					first = false;
					GenerateExpression(ex);
				}
				
				Output.Write(") ");
			};
 			if (constructor.BaseConstructorArgs.Count > 0)
			{
				Output.Write(": base(");
				bool first = true;
				foreach (CodeExpression ex in constructor.BaseConstructorArgs)
				{
					if (!first)
						Output.Write(", ");
					first = false;
					GenerateExpression(ex);
				}
				
				Output.Write(") ");
			};
			Output.WriteLine ("{");
			Indent++;
			GenerateStatements (constructor.Statements);
			Indent--;
			Output.WriteLine ('}');
		}
		
		protected override void GenerateTypeConstructor( CodeTypeConstructor constructor )
		{
			Output.WriteLine ("static " + GetSafeName (CurrentTypeName) + "() {");
			Indent++;
			GenerateStatements (constructor.Statements);
			Indent--;
			Output.WriteLine ('}');
		}

		protected override void GenerateTypeStart( CodeTypeDeclaration declaration )
		{
			TextWriter output = Output;

			if (declaration.CustomAttributes.Count > 0)
				OutputAttributeDeclarations( declaration.CustomAttributes );

			TypeAttributes attributes = declaration.TypeAttributes;
			OutputTypeAttributes( attributes,
					      declaration.IsStruct,
					      declaration.IsEnum );

			output.Write( GetSafeName (declaration.Name) );
			output.Write( ' ' );
			
			IEnumerator enumerator = declaration.BaseTypes.GetEnumerator();
			if ( enumerator.MoveNext() ) {
				CodeTypeReference type = (CodeTypeReference)enumerator.Current;
			
				output.Write( ": " );
				OutputType( type );
				
				while ( enumerator.MoveNext() ) {
					type = (CodeTypeReference)enumerator.Current;
				
					output.Write( ", " );
					OutputType( type );
				}

				output.Write( ' ' );
			}
			output.WriteLine( "{" );
			++Indent;
		}

		protected override void GenerateTypeEnd( CodeTypeDeclaration declaration )
		{
			--Indent;
			Output.WriteLine( "}" );
		}

		protected override void GenerateNamespaceStart( CodeNamespace ns )
		{
			TextWriter output = Output;
			
			string name = ns.Name;
			if ( name != null && name != "" ) {
				output.Write( "namespace " );
				output.Write( GetSafeName (name) );
				output.WriteLine( " {" );
				++Indent;
			}
		}

		protected override void GenerateNamespaceEnd( CodeNamespace ns )
		{
			string name = ns.Name;
			if ( name != null && name != "" ) {
				--Indent;
				Output.WriteLine( "}" );
			}
		}

		protected override void GenerateNamespaceImport( CodeNamespaceImport import )
		{
			TextWriter output = Output;

			output.Write( "using " );
			output.Write( GetSafeName (import.Namespace) );
			output.WriteLine( ';' );
		}
		
		protected override void GenerateAttributeDeclarationsStart( CodeAttributeDeclarationCollection attributes )
		{
			Output.Write( '[' );
			CodeMemberMethod met = CurrentMember as CodeMemberMethod;
			if (met != null && met.ReturnTypeCustomAttributes == attributes)
				Output.Write ("return: ");
		}
		
		protected override void GenerateAttributeDeclarationsEnd( CodeAttributeDeclarationCollection attributes )
		{
			Output.WriteLine( ']' );
		}

		protected override void OutputType( CodeTypeReference type )
		{
			Output.Write( GetTypeOutput( type ) );
		}

		protected override string QuoteSnippetString( string value )
		{
			// FIXME: this is weird, but works.
			string output = value.Replace ("\\", "\\\\");
			output = output.Replace ("\"", "\\\"");
			output = output.Replace ("\t", "\\t");
			output = output.Replace ("\r", "\\r");
			output = output.Replace ("\n", "\\n");

			return "\"" + output + "\"";
		}

		private void GenerateDeclaration( CodeTypeReference type, string name, CodeExpression initExpression )
		{
			TextWriter output = Output;

			OutputTypeNamePair( type, GetSafeName (name) );

			if ( initExpression != null ) {
				output.Write( " = " );
				GenerateExpression( initExpression );
			}

			output.WriteLine( ';' );
		}
		
		private void GenerateMemberReferenceExpression( CodeExpression targetObject, string memberName )
		{
			if (targetObject != null ) {
				GenerateExpression( targetObject );
				Output.Write( '.' );
			}
			Output.Write( GetSafeName (memberName) );
		}
			
		protected override void GenerateParameterDeclarationExpression (CodeParameterDeclarationExpression e)
		{
			if (e.CustomAttributes != null && e.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (e.CustomAttributes);
			OutputDirection (e.Direction);
			OutputType (e.Type);
			Output.Write (' ');
			Output.Write (GetSafeName (e.Name));
		}

		/* 
		 * ICodeGenerator
		 */

		protected override string CreateEscapedIdentifier (string value)
		{
			return GetSafeName (value);
		}

		protected override string CreateValidIdentifier (string value)
		{
			if (value == null)
				throw new NullReferenceException ();

			if (keywordsTable == null) {
				FillKeywordTable ();
			}

			if (keywordsTable.Contains (value))
				return "_" + value;
			else
				return value;
		}

		protected override string GetTypeOutput( CodeTypeReference type )
		{
			string output;
			CodeTypeReference arrayType;

			arrayType = type.ArrayElementType;
			if ( arrayType != null )
				output = GetTypeOutput( arrayType );
			else { 
				switch ( type.BaseType ) {

				case "System.Decimal":
					output = "decimal";
					break;
				case "System.Double":
					output = "double";
					break;
				case "System.Single":
					output = "float";
					break;
					
				case "System.Byte":
					output = "byte";
					break;
				case "System.SByte":
					output = "sbyte";
					break;
				case "System.Int32":
					output = "int";
					break;
				case "System.UInt32":
					output = "uint";
					break;
				case "System.Int64":
					output = "long";
					break;
				case "System.UInt64":
					output = "ulong";
					break;
				case "System.Int16":
					output = "short";
					break;
				case "System.UInt16":
					output = "ushort";
					break;

				case "System.Boolean":
					output = "bool";
					break;
					
				case "System.Char":
					output = "char";
					break;

				case "System.String":
					output = "string";
					break;
				case "System.Object":
					output = "object";
					break;

				case "System.Void":
					output = "void";
					break;

				default:
					output = type.BaseType;
					break;
				}
			}

			int rank = type.ArrayRank;
			if ( rank > 0 ) {
				output += "[";
				for ( --rank; rank > 0; --rank  )
					output += ",";
				output += "]";
			}

			return output;
		}

		protected override bool IsValidIdentifier( string identifier )
		{
			return true;
		}

		protected override bool Supports( GeneratorSupport supports )
		{
			if ( (supports & GeneratorSupport.Win32Resources) != 0 )
				return false;
			return true;
		}

#if false
		//[MonoTODO]
		public override void ValidateIdentifier( string identifier )
		{
		}
#endif

		string GetSafeName (string id)
		{
			if (keywordsTable == null) {
				FillKeywordTable ();
			}
			if (keywordsTable.Contains (id)) return "@" + id;
			else return id;
		}

		static void FillKeywordTable ()
		{
			keywordsTable = new Hashtable ();
				foreach (string keyword in keywords) keywordsTable.Add (keyword,keyword);
		}

		static Hashtable keywordsTable;
		static string[] keywords = new string[] {
			"abstract","event","new","struct","as","explicit","null","switch","base","extern",
			"object","this","bool","false","operator","throw","break","finally","out","true",
			"byte","fixed","override","try","case","float","params","typeof","catch","for",
			"private","uint","char","foreach","protected","ulong","checked","goto","public",
			"unchecked","class","if","readonly","unsafe","const","implicit","ref","ushort",
			"continue","in","return","using","decimal","int","sbyte","virtual","default",
			"interface","sealed","volatile","delegate","internal","short","void","do","is",
			"sizeof","while","double","lock","stackalloc","else","long","static","enum",
			"namespace","string"
		};
	}
}
