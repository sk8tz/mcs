//
// Mono.CSharp CSharpCodeProvider Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2002 Ximian, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Mono.CSharp
{
	using System;
	using System.CodeDom;
	using System.CodeDom.Compiler;
	using System.IO;
	using System.Reflection;
	using System.Collections;
	using System.Text;

	internal class CSharpCodeGenerator
		: CodeGenerator
	{
            
		// It is used for beautiful "for" syntax
		bool dont_write_semicolon;
            
		//
		// Constructors
		//
		public CSharpCodeGenerator()
		{
			dont_write_semicolon = false;
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

				if (expression.CreateType.ArrayRank == 0) {
					output.Write( "[]" );
				}
				
				output.WriteLine( " {" );
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
			base.GenerateCompileUnitStart (compileUnit);
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
#if NET_2_0
			if (expression.TypeArguments.Count > 0)
				Output.Write (GetTypeArguments (expression.TypeArguments));
#endif
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
			if (dont_write_semicolon)
				return;
			Output.WriteLine( ';' );
		}

		protected override void GenerateIterationStatement( CodeIterationStatement statement )
		{
			TextWriter output = Output;
                    
			dont_write_semicolon = true;
			output.Write( "for (" );
			GenerateStatement( statement.InitStatement );
			output.Write( "; " );
			GenerateExpression( statement.TestExpression );
			output.Write( "; " );
			GenerateStatement( statement.IncrementStatement );
			output.Write( ") " );
			dont_write_semicolon = false;
			output.WriteLine ('{');
			++Indent;
			GenerateStatements( statement.Statements );
			--Indent;
			output.WriteLine ('}');
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

			if (statement.Expression != null) {
				output.Write ( "return " );
				GenerateExpression (statement.Expression);
				output.WriteLine ( ";" );
			} else {
				output.WriteLine ("return;");
			}
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
			if (dont_write_semicolon)
				return;
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

			output.Write( String.Concat (GetSafeName (statement.Label), ": "));

			if (statement.Statement != null)
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

		protected override void GenerateEvent( CodeMemberEvent eventRef, CodeTypeDeclaration declaration )
		{
			if (eventRef.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (eventRef.CustomAttributes);

			OutputMemberAccessModifier (eventRef.Attributes);
			OutputMemberScopeModifier (eventRef.Attributes | MemberAttributes.Final); // Don't output "virtual"
			Output.Write ("event ");
			OutputTypeNamePair (eventRef.Type, GetSafeName (eventRef.Name));
			Output.WriteLine (';');
		}

		protected override void GenerateField( CodeMemberField field )
		{
			TextWriter output = Output;

			if (field.CustomAttributes.Count > 0)
				OutputAttributeDeclarations( field.CustomAttributes );

			if (IsCurrentEnum)
				Output.Write(GetSafeName (field.Name));
			else {
				MemberAttributes attributes = field.Attributes;
				OutputMemberAccessModifier( attributes );
				OutputFieldScopeModifier( attributes );

				OutputTypeNamePair( field.Type, GetSafeName (field.Name) );
			}

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

#if NET_2_0
			GenerateGenericsParameters (method.TypeParameters);
#endif

			output.Write( '(' );
			OutputParameters( method.Parameters );
			output.Write( ')' );

#if NET_2_0
			GenerateGenericsConstraints (method.TypeParameters);
#endif

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
			if (constructor.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (constructor.CustomAttributes);

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
			CodeTypeDelegate del = declaration as CodeTypeDelegate;

			if (declaration.CustomAttributes.Count > 0)
				OutputAttributeDeclarations( declaration.CustomAttributes );

			TypeAttributes attributes = declaration.TypeAttributes;
			OutputTypeAttributes( attributes,
					      declaration.IsStruct,
					      declaration.IsEnum );

			if (del != null) {
				if (del.ReturnType != null)
					OutputType (del.ReturnType);
				else
					Output.Write ("void");
				output.Write(' ');
			}

			output.Write( GetSafeName (declaration.Name) );

#if NET_2_0
			GenerateGenericsParameters (declaration.TypeParameters);
#endif
			
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
			}

			if (del != null)
				output.Write ( " (" );
			else {
#if NET_2_0
				GenerateGenericsConstraints (declaration.TypeParameters);
#endif
				output.WriteLine ( " {" );
			}
			++Indent;
		}

		protected override void GenerateTypeEnd( CodeTypeDeclaration declaration )
		{
			--Indent;
			if (declaration is CodeTypeDelegate)
				Output.WriteLine (");");
			else
				Output.WriteLine ("}");
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

		[MonoTODO ("Implement missing special characters")]
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

		protected override void GenerateParameterDeclarationExpression (CodeParameterDeclarationExpression e)
		{
			if (e.CustomAttributes != null && e.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (e.CustomAttributes);
			OutputDirection (e.Direction);
			OutputType (e.Type);
			Output.Write (' ');
			Output.Write (GetSafeName (e.Name));
		}

		protected override void GenerateTypeOfExpression (CodeTypeOfExpression e)
		{
			Output.Write ("typeof(");
			OutputType (e.Type);
			Output.Write (")");
		}

		/* 
		 * ICodeGenerator
		 */

		protected override string CreateEscapedIdentifier (string value)
		{
			if (value == null)
				throw new NullReferenceException ("Argument identifier is null.");
			return GetSafeName (value);
		}

		protected override string CreateValidIdentifier (string value)
		{
			if (value == null)
				throw new NullReferenceException ();

			if (keywordsTable == null)
				FillKeywordTable ();

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

				switch ( type.BaseType.ToLower (System.Globalization.CultureInfo.InvariantCulture)) {

				case "system.decimal":
					output = "decimal";
					break;
				case "system.double":
					output = "double";
					break;
				case "system.single":
					output = "float";
					break;
					
				case "system.byte":
					output = "byte";
					break;
				case "system.sbyte":
					output = "sbyte";
					break;
				case "system.int32":
					output = "int";
					break;
				case "system.uint32":
					output = "uint";
					break;
				case "system.int64":
					output = "long";
					break;
				case "system.uint64":
					output = "ulong";
					break;
				case "system.int16":
					output = "short";
					break;
				case "system.uint16":
					output = "ushort";
					break;

				case "system.boolean":
					output = "bool";
					break;
					
				case "system.char":
					output = "char";
					break;

				case "system.string":
					output = "string";
					break;
				case "system.object":
					output = "object";
					break;

				case "system.void":
					output = "void";
					break;

				default:
					output = GetSafeName (type.BaseType);
					break;
				}
			}
			
#if NET_2_0
			if (type.Options == CodeTypeReferenceOptions.GlobalReference)
				output = String.Concat ("global::", output);

			if (type.TypeArguments.Count > 0)
				output += GetTypeArguments (type.TypeArguments);
#endif
			int rank = type.ArrayRank;
			if ( rank > 0 ) {
				output += "[";
				for ( --rank; rank > 0; --rank  )
					output += ",";
				output += "]";
			}

			return output.Replace ('+', '.');
		}

		protected override bool IsValidIdentifier ( string identifier )
		{
			if (keywordsTable == null)
				FillKeywordTable ();

			return !keywordsTable.Contains (identifier);
		}

		protected override bool Supports( GeneratorSupport supports )
		{
			return true;
		}

#if NET_2_0
		protected override void GenerateDirectives( CodeDirectiveCollection directives )
		{
			foreach (CodeDirective d in directives) {
				if (d is CodeChecksumPragma) {
					GenerateCodeChecksumPragma ((CodeChecksumPragma)d);
					continue;
				}
				if (d is CodeRegionDirective) {
					GenerateCodeRegionDirective ((CodeRegionDirective)d);
					continue;
				}
				throw new NotImplementedException ("Unknown CodeDirective");
			}
		}

		void GenerateCodeChecksumPragma (CodeChecksumPragma pragma)
		{
			Output.Write ("#pragma checksum \"");
			Output.Write (pragma.FileName);
			Output.Write ("\" \"");
			Output.Write (pragma.ChecksumAlgorithmId.ToString ("B"));
			Output.Write ("\" \"");
			foreach (byte b in pragma.ChecksumData) {
				Output.Write (b.ToString ("X2"));
			}
			Output.WriteLine ("\"");
		}

		void GenerateCodeRegionDirective (CodeRegionDirective region)
		{
			switch (region.RegionMode) {
				case CodeRegionMode.Start:
					Output.Write ("#region ");
					Output.WriteLine (region.RegionText);
					return;
				case CodeRegionMode.End:
					Output.WriteLine ("#endregion");
					return;
			}
		}

		void GenerateGenericsParameters (CodeTypeParameterCollection parameters)
		{
			int count = parameters.Count;
			if (count == 0)
				return;

			Output.Write ('<');
			for (int i = 0; i < count - 1; ++i) {
				Output.Write (parameters [i].Name);
				Output.Write (", ");
			}
			Output.Write (parameters [count - 1].Name);
			Output.Write ('>');
		}

		void GenerateGenericsConstraints (CodeTypeParameterCollection parameters)
		{
			int count = parameters.Count;
			if (count == 0)
				return;

			++Indent;
			foreach (CodeTypeParameter p in parameters) {
				if (p.Constraints.Count == 0)
					continue;
				Output.WriteLine ();
				Output.Write ("where ");
				Output.Write (p.Name);
				Output.Write (" : ");

				bool is_first = true;
				foreach (CodeTypeReference r in p.Constraints) {
					if (is_first)
						is_first = false;
					else
						Output.Write (", ");
					OutputType (r);
				}
				if (p.HasConstructorConstraint) {
					if (!is_first)
						Output.Write (", ");
					Output.Write ("new ()");
				}

			}
			--Indent;
		}

		string GetTypeArguments (CodeTypeReferenceCollection collection)
		{
			StringBuilder sb = new StringBuilder (" <");
			foreach (CodeTypeReference r in collection) {
				sb.Append (GetTypeOutput (r));
				sb.Append (", ");
			}
			sb.Length--;
			sb [sb.Length - 1] = '>';
			return sb.ToString ();
		}

		internal override void OutputExtraTypeAttribute (CodeTypeDeclaration type)
		{
			if (type.IsPartial)
				Output.Write ("partial ");
		}
#endif


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
			"this","false","operator","throw","break","finally","out","true",
			"fixed","override","try","case","params","typeof","catch","for",
			"private","foreach","protected","checked","goto","public",
			"unchecked","class","if","readonly","unsafe","const","implicit","ref",
			"continue","in","return","using","virtual","default",
			"interface","sealed","volatile","delegate","internal","do","is",
			"sizeof","while","lock","stackalloc","else","static","enum",
			"namespace",
			"object","bool","byte","float","uint","char","ulong","ushort",
			"decimal","int","sbyte","short","double","long","string","void",
#if NET_2_0
			"partial", "yield", "where"
#endif
		};
	}
}
