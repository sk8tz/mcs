//
// System.CodeDom.Compiler CodeGenerator class
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.CodeDom;
using System.Reflection;
using System.IO;
using System.Collections;
	
namespace System.CodeDom.Compiler {

	public abstract class CodeGenerator
		: ICodeGenerator
	{
		private IndentedTextWriter output;
		private CodeGeneratorOptions options;
		private CodeTypeMember currentMember;
		private CodeTypeDeclaration currentType;

		//
		// Constructors
		//
		protected CodeGenerator()
		{
		}

		//
		// Properties
		//
		protected CodeTypeMember CurrentMember {
			get {
				return currentMember;
			}
		}
		
		protected string CurrentMemberName {
			get {
				if ( currentType == null )
					return null;
				return currentMember.Name;
			}
		}

		protected string CurrentTypeName {
			get {
				if ( currentType == null )
					return null;
				return currentType.Name;
			}
		}
		
		protected int Indent {
			get {
				return output.Indent;
			}
			set {
				output.Indent = value;
			}
		}
		
		protected bool IsCurrentClass {
			get {
				if ( currentType == null )
					return false;
				return currentType.IsClass;
			}
		}

		protected bool IsCurrentDelegate {
			get {
				return currentType is CodeTypeDelegate;
			}
		}

		protected bool IsCurrentEnum {
			get {
				if ( currentType == null )
					return false;
				return currentType.IsEnum;
			}
		}

		protected bool IsCurrentInterface {
			get {
				if ( currentType == null )
					return false;
				return currentType.IsInterface;
			}
		}

		protected bool IsCurrentStruct {
			get {
				if ( currentType == null )
					return false;
				return currentType.IsStruct;
			}
		}

		protected abstract string NullToken {
			get;
		}
		
		
		protected CodeGeneratorOptions Options {
			get {
				return options;
			}
		}
			
		protected TextWriter Output {
			get {
				return output;
			}
		}

		//
		// Methods
		//
		protected virtual void ContinueOnNewLine( string st )
		{
		}

		/*
		 * Code Generation methods
		 */
		protected abstract void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression e);
		protected abstract void GenerateArrayCreateExpression (CodeArrayCreateExpression e);
		protected abstract void GenerateArrayIndexerExpression( CodeArrayIndexerExpression e );
		protected abstract void GenerateAssignStatement (CodeAssignStatement s);
		protected abstract void GenerateAttachEventStatement (CodeAttachEventStatement s);
		protected abstract void GenerateAttributeDeclarationsStart( CodeAttributeDeclarationCollection attributes );
		protected abstract void GenerateAttributeDeclarationsEnd( CodeAttributeDeclarationCollection attributes );
		protected abstract void GenerateBaseReferenceExpression (CodeBaseReferenceExpression e);

		protected virtual void GenerateBinaryOperatorExpression (CodeBinaryOperatorExpression e)
		{
			GenerateExpression( e.Left );
			
			switch ( e.Operator ) {
			case CodeBinaryOperatorType.Add:
				output.Write( " + " );
				break;
			case CodeBinaryOperatorType.Assign:
				output.Write( " = " );
				break;
			case CodeBinaryOperatorType.BitwiseAnd:
				output.Write( " & " );
				break;
			case CodeBinaryOperatorType.BitwiseOr:
				output.Write( " | " );
				break;
			case CodeBinaryOperatorType.BooleanAnd:
				output.Write( " && " );
				break;
			case CodeBinaryOperatorType.BooleanOr:
				output.Write( " || " );
				break;
			case CodeBinaryOperatorType.Divide:
				output.Write( " / " );
				break;
			case CodeBinaryOperatorType.GreaterThan:
				output.Write( " > " );
				break;
			case CodeBinaryOperatorType.GreaterThanOrEqual:
				output.Write( " >= " );
				break;
			case CodeBinaryOperatorType.IdentityEquality:
				output.Write( " == " );
				break;
			case CodeBinaryOperatorType.IdentityInequality:
				output.Write( " != " );
				break;
			case CodeBinaryOperatorType.LessThan:
				output.Write( " < " );
				break;
			case CodeBinaryOperatorType.LessThanOrEqual:
				output.Write( " <= " );
				break;
			case CodeBinaryOperatorType.Modulus:
				output.Write( " % " );
				break;
			case CodeBinaryOperatorType.Multiply:
				output.Write( " * " );
				break;
			case CodeBinaryOperatorType.Subtract:
				output.Write( " - " );
				break;
			case CodeBinaryOperatorType.ValueEquality:
				output.Write( " == " );
				break;
			}

			GenerateExpression( e.Right );
		}

		protected abstract void GenerateCastExpression (CodeCastExpression e);
		protected abstract void GenerateComment( CodeComment comment );

		protected virtual void GenerateCommentStatement( CodeCommentStatement statement )
		{
			GenerateComment( statement.Comment );
		}

		protected virtual void GenerateCommentStatements (CodeCommentStatementCollection statements)
		{
			foreach ( CodeCommentStatement comment in statements )
				GenerateCommentStatement( comment );
		}

		protected virtual void GenerateCompileUnit( CodeCompileUnit compileUnit )
		{
			GenerateCompileUnitStart( compileUnit );

			CodeAttributeDeclarationCollection attributes = compileUnit.AssemblyCustomAttributes;
			if ( attributes.Count != 0 ) {
				GenerateAttributeDeclarationsStart( attributes );
				output.Write( "assembly: " );
				OutputAttributeDeclarations( compileUnit.AssemblyCustomAttributes );
				GenerateAttributeDeclarationsEnd( attributes );
			}

			foreach ( CodeNamespace ns in compileUnit.Namespaces )
				GenerateNamespace( ns );

			GenerateCompileUnitEnd( compileUnit );
		}

		protected virtual void GenerateCompileUnitEnd( CodeCompileUnit compileUnit )
		{
			output.WriteLine( "<compileUnitEnd>" );
		}

		protected virtual void GenerateCompileUnitStart( CodeCompileUnit compileUnit )
		{
			output.WriteLine( "<compileUnitStart>" );
		}

		protected abstract void GenerateConditionStatement( CodeConditionStatement s );
		protected abstract void GenerateConstructor (CodeConstructor x, CodeTypeDeclaration d);
		protected abstract void GenerateDelegateCreateExpression( CodeDelegateCreateExpression e );
		protected abstract void GenerateDelegateInvokeExpression( CodeDelegateInvokeExpression e );

		protected virtual void GenerateDirectionExpression( CodeDirectionExpression e )
		{
			OutputDirection( e.Direction );
			output.Write( ' ' );
			GenerateExpression( e.Expression );
		}

		protected abstract void GenerateEntryPointMethod( CodeEntryPointMethod m, CodeTypeDeclaration d );
		protected abstract void GenerateEvent( CodeMemberEvent ev, CodeTypeDeclaration d );
		protected abstract void GenerateEventReferenceExpression( CodeEventReferenceExpression e );

		protected void GenerateExpression( CodeExpression e )
		{
			CodeArgumentReferenceExpression argref = e as CodeArgumentReferenceExpression;
			if ( argref != null ) {
				GenerateArgumentReferenceExpression( argref );
				return;
			}
			CodeArrayCreateExpression mkarray = e as CodeArrayCreateExpression;
			if ( mkarray != null ) {
				GenerateArrayCreateExpression( mkarray );
				return;
			}
			CodeArrayIndexerExpression arrayidx = e as CodeArrayIndexerExpression;
			if ( arrayidx != null ) {
				GenerateArrayIndexerExpression( arrayidx );
				return;
			}
			CodeBaseReferenceExpression baseref = e as CodeBaseReferenceExpression;
			if ( baseref != null ) {
				GenerateBaseReferenceExpression( baseref );
				return;
			}
			CodeBinaryOperatorExpression binary = e as CodeBinaryOperatorExpression;
			if ( binary != null ) {
				GenerateBinaryOperatorExpression( binary );
				return;
			}
			CodeCastExpression cast = e as CodeCastExpression;
			if ( cast != null ) {
				GenerateCastExpression( cast );
				return;
			}
			CodeDelegateCreateExpression mkdel = e as CodeDelegateCreateExpression;
			if ( mkdel != null ) {
				GenerateDelegateCreateExpression( mkdel );
				return;
			}
			CodeDelegateInvokeExpression delinvoke = e as CodeDelegateInvokeExpression;
			if ( delinvoke != null ) {
				GenerateDelegateInvokeExpression( delinvoke );
				return;
			}
			CodeDirectionExpression direction = e as CodeDirectionExpression;
			if ( direction != null ) {
				GenerateDirectionExpression( direction );
				return;
			}
			CodeEventReferenceExpression eventref = e as CodeEventReferenceExpression;
			if ( eventref != null ) {
				GenerateEventReferenceExpression( eventref );
				return;
			}
			CodeFieldReferenceExpression fieldref = e as CodeFieldReferenceExpression;
			if ( fieldref != null ) {
				GenerateFieldReferenceExpression( fieldref );
				return;
			}
			CodeIndexerExpression idx = e as CodeIndexerExpression;
			if ( idx != null ) {
				GenerateIndexerExpression( idx );
				return;
			}
			CodeMethodInvokeExpression methodinv = e as CodeMethodInvokeExpression;
			if ( methodinv != null ) {
				GenerateMethodInvokeExpression( methodinv );
				return;
			}
			CodeMethodReferenceExpression methodref = e as CodeMethodReferenceExpression;
			if ( methodref != null ) {
				GenerateMethodReferenceExpression( methodref );
				return;
			}
			CodeObjectCreateExpression objref = e as CodeObjectCreateExpression;
			if ( objref != null ) {
				GenerateObjectCreateExpression( objref );
				return;
			}
			CodeParameterDeclarationExpression param = e as CodeParameterDeclarationExpression;
			if ( param != null ) {
				GenerateParameterDeclarationExpression( param );
				return;
			}
			CodePrimitiveExpression primitive = e as CodePrimitiveExpression;
			if ( primitive != null ) {
				GeneratePrimitiveExpression( primitive );
				return;
			}
			CodePropertyReferenceExpression propref = e as CodePropertyReferenceExpression;
			if ( propref != null ) {
				GeneratePropertyReferenceExpression( propref );
				return;
			}
			CodePropertySetValueReferenceExpression propset = e as CodePropertySetValueReferenceExpression;
			if ( propset != null ) {
				GeneratePropertySetValueReferenceExpression( propset );
				return;
			}
			CodeSnippetExpression snippet = e as CodeSnippetExpression;
			if ( snippet != null ) {
				GenerateSnippetExpression( snippet );
				return;
			}
			CodeThisReferenceExpression thisref = e as CodeThisReferenceExpression;
			if ( thisref != null ) {
				GenerateThisReferenceExpression( thisref );
				return;
			}
			CodeTypeOfExpression typeOf = e as CodeTypeOfExpression;
			if ( typeOf != null ) {
				GenerateTypeOfExpression( typeOf );
				return;
			}
			CodeTypeReferenceExpression typeref = e as CodeTypeReferenceExpression;
			if ( typeref != null ) {
				GenerateTypeReferenceExpression( typeref );
				return;
			}
			CodeVariableReferenceExpression varref = e as CodeVariableReferenceExpression;
			if ( varref != null ) {
				GenerateVariableReferenceExpression( varref );
				return;
			}
		}

		protected abstract void GenerateExpressionStatement( CodeExpressionStatement statement );
		protected abstract void GenerateField (CodeMemberField f);
		protected abstract void GenerateFieldReferenceExpression (CodeFieldReferenceExpression e);
		protected abstract void GenerateGotoStatement( CodeGotoStatement statement );
		protected abstract void GenerateIndexerExpression (CodeIndexerExpression e);
		protected abstract void GenerateIterationStatement( CodeIterationStatement s );
		protected abstract void GenerateLabeledStatement( CodeLabeledStatement statement );
		protected abstract void GenerateLinePragmaStart (CodeLinePragma p);
		protected abstract void GenerateLinePragmaEnd (CodeLinePragma p);
		protected abstract void GenerateMethod (CodeMemberMethod m, CodeTypeDeclaration d);
		protected abstract void GenerateMethodInvokeExpression (CodeMethodInvokeExpression e);
		protected abstract void GenerateMethodReferenceExpression( CodeMethodReferenceExpression e );
		protected abstract void GenerateMethodReturnStatement (CodeMethodReturnStatement e);

		protected virtual void GenerateNamespace (CodeNamespace ns)
		{
			foreach ( CodeCommentStatement statement in ns.Comments )
				GenerateCommentStatement( statement );

			GenerateNamespaceStart( ns );

			foreach ( CodeNamespaceImport import in ns.Imports )
				GenerateNamespaceImport( import );

			output.WriteLine();

			foreach ( CodeTypeDeclaration type in ns.Types )
				GenerateCodeFromType( type, output, options );

			GenerateNamespaceEnd( ns );
		}

		protected abstract void GenerateNamespaceStart (CodeNamespace ns);
		protected abstract void GenerateNamespaceEnd (CodeNamespace ns);
		protected abstract void GenerateNamespaceImport (CodeNamespaceImport i);
		protected abstract void GenerateObjectCreateExpression (CodeObjectCreateExpression e);

		protected virtual void GenerateParameterDeclarationExpression (CodeParameterDeclarationExpression e)
		{
			OutputAttributeDeclarations( e.CustomAttributes );
			OutputDirection( e.Direction );
			OutputType( e.Type );
			output.Write( ' ' );
			output.Write( e.Name );
		}

		protected virtual void GeneratePrimitiveExpression (CodePrimitiveExpression e)
		{
			output.Write( e.Value );
		}

		protected abstract void GenerateProperty (CodeMemberProperty p, CodeTypeDeclaration d);
		protected abstract void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression e);
		protected abstract void GeneratePropertySetValueReferenceExpression( CodePropertySetValueReferenceExpression e );
		protected abstract void GenerateRemoveEventStatement( CodeRemoveEventStatement statement );
		protected abstract void GenerateSnippetExpression( CodeSnippetExpression e );
		protected abstract void GenerateSnippetMember( CodeSnippetTypeMember m );
		protected virtual void GenerateSnippetStatement( CodeSnippetStatement s )
		{
			output.Write( s.Value );
		}

		protected void GenerateStatement( CodeStatement s )
		{
			CodeAssignStatement assign = s as CodeAssignStatement;
			if ( assign != null ) {
				GenerateAssignStatement( assign );
				return;
			}
			CodeAttachEventStatement attach = s as CodeAttachEventStatement;
			if ( attach != null ) {
				GenerateAttachEventStatement( attach );
				return;
			}
			CodeCommentStatement comment = s as CodeCommentStatement;
			if ( comment != null ) {
				GenerateCommentStatement( comment );
				return;
			}
			CodeConditionStatement condition = s as CodeConditionStatement;
			if ( condition != null ) {
				GenerateConditionStatement( condition );
				return;
			}
			CodeExpressionStatement expression = s as CodeExpressionStatement;
			if ( expression != null ) {
				GenerateExpressionStatement( expression );
				return;
			}
			CodeGotoStatement gotostmt = s as CodeGotoStatement;
			if ( gotostmt != null ) {
				GenerateGotoStatement( gotostmt );
				return;
			}
			CodeIterationStatement iteration = s as CodeIterationStatement;
			if ( iteration != null ) {
				GenerateIterationStatement( iteration );
				return;
			}
			CodeLabeledStatement label = s as CodeLabeledStatement;
			if ( label != null ) {
				GenerateLabeledStatement( label );
				return;
			}
			CodeMethodReturnStatement returnstmt = s as CodeMethodReturnStatement;
			if ( returnstmt != null ) {
				GenerateMethodReturnStatement( returnstmt );
				return;
			}
			CodeRemoveEventStatement remove = s as CodeRemoveEventStatement;
			if ( remove != null ) {
				GenerateRemoveEventStatement( remove );
				return;
			}
			CodeSnippetStatement snippet = s as CodeSnippetStatement;
			if ( snippet != null ) {
				GenerateSnippetStatement( snippet );
				return;
			}
			CodeThrowExceptionStatement exception = s as CodeThrowExceptionStatement;
			if ( exception != null ) {
				GenerateThrowExceptionStatement( exception );
				return;
			}
			CodeTryCatchFinallyStatement trycatch = s as CodeTryCatchFinallyStatement;
			if ( trycatch != null ) {
				GenerateTryCatchFinallyStatement( trycatch );
				return;
			}
			CodeVariableDeclarationStatement declaration = s as CodeVariableDeclarationStatement;
			if ( declaration != null ) {
				GenerateVariableDeclarationStatement( declaration );
				return;
			}
		}

		protected void GenerateStatements( CodeStatementCollection c )
		{
			foreach ( CodeStatement statement in c )
				GenerateStatement( statement );
		}

		protected abstract void GenerateThisReferenceExpression (CodeThisReferenceExpression e);
		protected abstract void GenerateThrowExceptionStatement (CodeThrowExceptionStatement s);
		protected abstract void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement s);
		protected abstract void GenerateTypeEnd( CodeTypeDeclaration declaration );
		protected abstract void GenerateTypeConstructor( CodeTypeConstructor constructor );

		protected virtual void GenerateTypeOfExpression (CodeTypeOfExpression e)
		{
			output.Write( "typeof(" );
			OutputType( e.Type );
			output.Write( ")" );
		}

		protected virtual void GenerateTypeReferenceExpression (CodeTypeReferenceExpression e)
		{
			OutputType( e.Type );
		}

		protected abstract void GenerateTypeStart( CodeTypeDeclaration declaration );
		protected abstract void GenerateVariableDeclarationStatement (CodeVariableDeclarationStatement e);
		protected abstract void GenerateVariableReferenceExpression( CodeVariableReferenceExpression e );

		//
		// Other members
		//
		
		/*
		 * Output Methods
		 */
		protected virtual void OutputAttributeArgument( CodeAttributeArgument argument )
		{
			string name = argument.Name;
			if ( name != null ) {
				output.Write( name );
				output.Write( '=' );
			}
			GenerateExpression( argument.Value );
		}

		private void OutputAttributeDeclaration( CodeAttributeDeclaration attribute )
		{
			output.Write( attribute.Name );
			output.Write( '(' );
			IEnumerator enumerator = attribute.Arguments.GetEnumerator();
			if ( enumerator.MoveNext() ) {
				CodeAttributeArgument argument = (CodeAttributeArgument)enumerator.Current;
				OutputAttributeArgument( argument );
				
				while ( enumerator.MoveNext() ) {
					output.Write( ',' );
					argument = (CodeAttributeArgument)enumerator.Current;
					OutputAttributeArgument( argument );
				}
			}
			output.Write( ')' );
		}

		protected virtual void OutputAttributeDeclarations( CodeAttributeDeclarationCollection attributes )
		{
			GenerateAttributeDeclarationsStart( attributes );
			
			IEnumerator enumerator = attributes.GetEnumerator();
			if ( enumerator.MoveNext() ) {
				CodeAttributeDeclaration attribute = (CodeAttributeDeclaration)enumerator.Current;

				OutputAttributeDeclaration( attribute );
				
				while ( enumerator.MoveNext() ) {
					attribute = (CodeAttributeDeclaration)enumerator.Current;

					output.WriteLine( ',' );
					OutputAttributeDeclaration( attribute );
				}
			}

			GenerateAttributeDeclarationsEnd( attributes );
		}

		protected virtual void OutputDirection( FieldDirection direction )
		{
			switch ( direction ) {
			case FieldDirection.In:
				output.Write( "in " );
				break;
			case FieldDirection.Out:
				output.Write( "out " );
				break;
			case FieldDirection.Ref:
				output.Write( "ref " );
				break;
			}
		}

		protected virtual void OutputExpressionList( CodeExpressionCollection expressions )
		{
			OutputExpressionList( expressions, false );
		}

		protected virtual void OutputExpressionList( CodeExpressionCollection expressions,
							     bool newLineBetweenItems )
		{
			IEnumerator enumerator = expressions.GetEnumerator();
			if ( enumerator.MoveNext() ) {
				CodeExpression expression = (CodeExpression)enumerator.Current;

				GenerateExpression( expression );
				
				while ( enumerator.MoveNext() ) {
					expression = (CodeExpression)enumerator.Current;
					
					output.Write( ',' );
					if ( newLineBetweenItems )
						output.WriteLine();
					else
						output.Write( ' ' );
					
					GenerateExpression( expression );
				}
			}
		}

		protected virtual void OutputFieldScopeModifier( MemberAttributes attributes )
		{
			if ( (attributes & MemberAttributes.VTableMask) == MemberAttributes.New )
				output.Write( "new " );

			switch ( attributes & MemberAttributes.ScopeMask ) {
			case MemberAttributes.Static:
				output.Write( "static " );
				break;
			case MemberAttributes.Const:
				output.Write( "const " );
				break;
			}
		}

		protected virtual void OutputIdentifier( string ident )
		{
		}

		protected virtual void OutputMemberAccessModifier( MemberAttributes attributes )
		{
			switch ( attributes & MemberAttributes.AccessMask ) {
			case MemberAttributes.Assembly:
				output.Write( "internal " );
				break;
			case MemberAttributes.FamilyAndAssembly:
				output.Write( "/* FamAndAssem */ internal " ); 
				break;
			case MemberAttributes.Family:
				output.Write( "protected " );
				break;
			case MemberAttributes.FamilyOrAssembly:
				output.Write( "protected internal " );
				break;
			case MemberAttributes.Private:
				output.Write( "private " );
				break;
			case MemberAttributes.Public:
				output.Write( "public " );
				break;
			}
		}

		protected virtual void OutputMemberScopeModifier( MemberAttributes attributes )
		{
			if ( (attributes & MemberAttributes.VTableMask) == MemberAttributes.New )
				output.Write( "new " );

			switch ( attributes & MemberAttributes.ScopeMask ) {
			case MemberAttributes.Abstract:
				output.Write( "abstract " );
				break;
			case MemberAttributes.Final:
				output.Write( "sealed " );
				break;
			case MemberAttributes.Static:
				output.Write( "static " );
				break;
			case MemberAttributes.Override:
				output.Write( "override " );
				break;
			default:
				//
				// FUNNY! if the scope value is
				// rubbish (0 or >Const), and access
				// is public or protected, make it
				// "virtual".
				//
				// i'm not sure whether this is 100%
				// correct, but it seems to be MS
				// behavior. 
				//
				MemberAttributes access = attributes & MemberAttributes.AccessMask;
				if ( access == MemberAttributes.Public || 
				     access == MemberAttributes.Family )
					output.Write( "virtual " );
				break;
			}
		}
				
		protected virtual void OutputOperator( CodeBinaryOperatorType op )
		{
		}

		protected virtual void OutputParameters( CodeParameterDeclarationExpressionCollection parameters )
		{
		}

		protected abstract void OutputType( CodeTypeReference t );

		protected virtual void OutputTypeAttributes( TypeAttributes attributes,
							     bool isStruct,
							     bool isEnum )
		{
			switch ( attributes & TypeAttributes.VisibilityMask ) {
			case TypeAttributes.NotPublic:
				// private by default
				break; 

			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				output.Write( "public " );
				break;

			case TypeAttributes.NestedPrivate:
				output.Write( "private " );
				break;
			}

			if ( isStruct )

				output.Write( "struct " );

			else if ( isEnum )

				output.Write( "enum " );

			else {
				if ( (attributes & TypeAttributes.Interface) != 0 ) 

					output.Write( "interface " );

				else {

					if ( (attributes & TypeAttributes.Sealed) != 0 )
						output.Write( "sealed " );

					if ( (attributes & TypeAttributes.Abstract) != 0 )
						output.Write( "abstract " );
					
					output.Write( "class " );
				}
			}
		}
		
		protected virtual void OutputTypeNamePair( CodeTypeReference type,
							   string name )
		{
			OutputType( type );
			output.Write( ' ' );
			output.Write( name );
		}

		protected abstract string QuoteSnippetString( string value );

		/*
		 * ICodeGenerator
		 */
		protected abstract string CreateEscapedIdentifier( string value );
		string ICodeGenerator.CreateEscapedIdentifier( string value )
		{
			return CreateEscapedIdentifier( value );
		}

		protected abstract string CreateValidIdentifier( string value );
		string ICodeGenerator.CreateValidIdentifier( string value )
		{
			return CreateValidIdentifier( value );
		}

		private void InitOutput( TextWriter output, CodeGeneratorOptions options )
		{
			if ( options == null )
				options = new CodeGeneratorOptions();
				
			this.output = new IndentedTextWriter( output, options.IndentString );
			this.options = options;
		}

		public virtual void GenerateCodeFromCompileUnit( CodeCompileUnit compileUnit,
								 TextWriter output,
								 CodeGeneratorOptions options )
		{
			InitOutput( output, options );
			GenerateCompileUnit( compileUnit );
		}

		[MonoTODO]
		public virtual void GenerateCodeFromExpression( CodeExpression expression,
								TextWriter output,
								CodeGeneratorOptions options )
		{
			InitOutput( output, options );
		}

		public virtual void GenerateCodeFromNamespace( CodeNamespace ns,
							       TextWriter output, 
							       CodeGeneratorOptions options )
		{
			InitOutput( output, options );
			GenerateNamespace( ns );
		}

		public virtual void GenerateCodeFromStatement( CodeStatement statement,
							       TextWriter output, 
							       CodeGeneratorOptions options )
		{
			InitOutput( output, options );
		}

		public virtual void GenerateCodeFromType( CodeTypeDeclaration type,
							  TextWriter output,
							  CodeGeneratorOptions options )
		{
			CodeTypeDeclaration prevType = this.currentType;
			this.currentType = type;

			InitOutput( output, options );

			GenerateTypeStart( type );

			foreach ( CodeTypeMember member in type.Members ) {

				CodeTypeMember prevMember = this.currentMember;
				this.currentMember = member;

				if ( options.BlankLinesBetweenMembers )
					output.WriteLine();

				CodeMemberEvent eventm = member as CodeMemberEvent;
				if ( eventm != null ) {
					GenerateEvent( eventm, type );
					continue;
				}
				CodeMemberField field = member as CodeMemberField;
				if ( field != null ) {
					GenerateField( field );
					continue;
				}
				CodeEntryPointMethod epmethod = member as CodeEntryPointMethod;
				if ( epmethod != null ) {
					GenerateEntryPointMethod( epmethod, type );
					continue;
				}
				CodeMemberMethod method = member as CodeMemberMethod;
				if ( method != null ) {
					GenerateMethod( method, type );
					continue;
				}
				CodeMemberProperty property = member as CodeMemberProperty;
				if ( property != null ) {
					GenerateProperty( property, type );
					continue;
				}
				CodeSnippetTypeMember snippet = member as CodeSnippetTypeMember;
				if ( snippet != null ) {
					GenerateSnippetMember( snippet );
					continue;
				}
				CodeTypeDeclaration subtype = member as CodeTypeDeclaration;
				if ( subtype != null ) {
					GenerateCodeFromType( subtype, output, options );
					continue;
				}
				
				this.currentMember = prevMember;
			}
				
			GenerateTypeEnd( type );
			this.currentType = prevType;
		}

		protected abstract string GetTypeOutput( CodeTypeReference type );

		string ICodeGenerator.GetTypeOutput( CodeTypeReference type )
		{
			return GetTypeOutput( type );
		}

		protected abstract bool IsValidIdentifier( string value );

		bool ICodeGenerator.IsValidIdentifier( string value )
		{
			return IsValidIdentifier( value );
		}

		protected abstract bool Supports( GeneratorSupport supports );

		bool ICodeGenerator.Supports( GeneratorSupport value )
		{
			return Supports( value );
		}

		[MonoTODO]
		protected virtual void ValidateIdentifier( string value )
		{
			throw new NotImplementedException();
		}

		void ICodeGenerator.ValidateIdentifier( string value )
		{
			ValidateIdentifier( value );
		}
	}
}
