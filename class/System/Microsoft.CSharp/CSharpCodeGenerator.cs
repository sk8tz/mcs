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
	using System.Globalization;
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

		protected override void GenerateArrayCreateExpression (CodeArrayCreateExpression expression)
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

			output.Write ("new ");

			CodeExpressionCollection initializers = expression.Initializers;
			CodeTypeReference createType = expression.CreateType;

			if (initializers.Count > 0) {

				OutputType (createType);

				if (expression.CreateType.ArrayRank == 0) {
					output.Write ("[]");
				}

				OutputStartBrace ();
				++Indent;
				OutputExpressionList (initializers, true);
				--Indent;
				output.Write ("}");
			} else {
				CodeTypeReference arrayType = createType.ArrayElementType;
				while (arrayType != null) {
					createType = arrayType;
					arrayType = arrayType.ArrayElementType;
				}

				OutputType (createType);

				output.Write ('[');

				CodeExpression size = expression.SizeExpression;
				if (size != null)
					GenerateExpression (size);
				else
					output.Write (expression.Size);

				output.Write(']');
			}
		}
		
		protected override void GenerateBaseReferenceExpression (CodeBaseReferenceExpression expression)
		{
			Output.Write ("base");
		}
		
		protected override void GenerateCastExpression (CodeCastExpression expression)
		{
			TextWriter output = Output;
			output.Write ("((");
			OutputType (expression.TargetType);
			output.Write (")(");
			GenerateExpression (expression.Expression);
			output.Write ("))");
		}


		protected override void GenerateCompileUnitStart (CodeCompileUnit compileUnit)
		{
			GenerateComment (new CodeComment ("------------------------------------------------------------------------------"));
			GenerateComment (new CodeComment (" <autogenerated>"));
			GenerateComment (new CodeComment ("     This code was generated by a tool."));
			GenerateComment (new CodeComment ("     Mono Runtime Version: " +  System.Environment.Version));
			GenerateComment (new CodeComment (""));
			GenerateComment (new CodeComment ("     Changes to this file may cause incorrect behavior and will be lost if "));
			GenerateComment (new CodeComment ("     the code is regenerated."));
			GenerateComment (new CodeComment (" </autogenerated>"));
			GenerateComment (new CodeComment ("------------------------------------------------------------------------------"));
			Output.WriteLine ();
			base.GenerateCompileUnitStart (compileUnit);
		}

		protected override void GenerateCompileUnit (CodeCompileUnit compileUnit)
		{
			GenerateCompileUnitStart (compileUnit);

			if (compileUnit.AssemblyCustomAttributes.Count > 0) {
				OutputAttributes (compileUnit.AssemblyCustomAttributes, 
					"assembly: ", false);
				Output.WriteLine ("");
			}

			foreach (CodeNamespace ns in compileUnit.Namespaces)
				GenerateNamespace (ns);

			GenerateCompileUnitEnd (compileUnit);
		}

		protected override void GenerateDelegateCreateExpression (CodeDelegateCreateExpression expression)
		{
			TextWriter output = Output;

			output.Write ("new ");
			OutputType (expression.DelegateType);
			output.Write ('(');

			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null) {
				GenerateExpression (targetObject);
				Output.Write ('.');
			}
			output.Write (GetSafeName (expression.MethodName));

			output.Write (')');
		}

		protected override void GenerateFieldReferenceExpression (CodeFieldReferenceExpression expression)
		{
			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null) {
				GenerateExpression (targetObject);
				Output.Write ('.');
			}
			Output.Write (GetSafeName (expression.FieldName));
		}
		
		protected override void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression expression)
		{
			Output.Write (GetSafeName (expression.ParameterName));
		}

		protected override void GenerateVariableReferenceExpression (CodeVariableReferenceExpression expression)
		{
			Output.Write (GetSafeName (expression.VariableName));
		}
			
		protected override void GenerateIndexerExpression (CodeIndexerExpression expression)
		{
			TextWriter output = Output;

			GenerateExpression (expression.TargetObject);
			output.Write ('[');
			OutputExpressionList (expression.Indices);
			output.Write (']');
		}
		
		protected override void GenerateArrayIndexerExpression (CodeArrayIndexerExpression expression)
		{
			TextWriter output = Output;

			GenerateExpression (expression.TargetObject);
			output.Write ('[');
			OutputExpressionList (expression.Indices);
			output.Write (']');
		}
		
		protected override void GenerateSnippetExpression (CodeSnippetExpression expression)
		{
			Output.Write (expression.Value);
		}
		
		protected override void GenerateMethodInvokeExpression (CodeMethodInvokeExpression expression)
		{
			TextWriter output = Output;

			GenerateMethodReferenceExpression (expression.Method);

			output.Write ('(');
			OutputExpressionList (expression.Parameters);
			output.Write (')');
		}

		protected override void GenerateMethodReferenceExpression (CodeMethodReferenceExpression expression)
		{
			if (expression.TargetObject != null)
			{
				GenerateExpression (expression.TargetObject);
				Output.Write ('.');
			};
			Output.Write (GetSafeName (expression.MethodName));
#if NET_2_0
			if (expression.TypeArguments.Count > 0)
				Output.Write (GetTypeArguments (expression.TypeArguments));
#endif
		}

		protected override void GenerateEventReferenceExpression (CodeEventReferenceExpression expression)
		{
			GenerateExpression (expression.TargetObject);
			Output.Write ('.');
			Output.Write (GetSafeName (expression.EventName));
		}

		protected override void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression expression)
		{
			GenerateExpression (expression.TargetObject);
			Output.Write ('(');
			OutputExpressionList (expression.Parameters);
			Output.Write (')');
		}
		
		protected override void GenerateObjectCreateExpression (CodeObjectCreateExpression expression)
		{
			Output.Write ("new ");
			OutputType (expression.CreateType);
			Output.Write ('(');
			OutputExpressionList (expression.Parameters);
			Output.Write (')');
		}

		protected override void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression expression)
		{
			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null) {
				GenerateExpression (targetObject);
				Output.Write ('.');
			}
			Output.Write (GetSafeName (expression.PropertyName ));
		}

		protected override void GeneratePropertySetValueReferenceExpression (CodePropertySetValueReferenceExpression expression)
		{
			Output.Write ("value");
		}

		protected override void GenerateThisReferenceExpression (CodeThisReferenceExpression expression)
		{
			Output.Write ("this");
		}

		protected override void GenerateExpressionStatement (CodeExpressionStatement statement)
		{
			GenerateExpression (statement.Expression);
			if (dont_write_semicolon)
				return;
			Output.WriteLine(';');
		}

		protected override void GenerateIterationStatement (CodeIterationStatement statement)
		{
			TextWriter output = Output;

			dont_write_semicolon = true;
			output.Write ("for (");
			GenerateStatement (statement.InitStatement);
			output.Write ("; ");
			GenerateExpression (statement.TestExpression);
			output.Write ("; ");
			GenerateStatement (statement.IncrementStatement);
			output.Write (") ");
			dont_write_semicolon = false;
			OutputStartBrace ();
			++Indent;
			GenerateStatements (statement.Statements);
			--Indent;
			output.WriteLine ('}');
		}

		protected override void GenerateThrowExceptionStatement (CodeThrowExceptionStatement statement)
		{
			Output.Write ("throw");
			if (statement.ToThrow != null) {
				Output.Write (' ');
				GenerateExpression (statement.ToThrow);
			}
			Output.WriteLine (";");
		}

		protected override void GenerateComment (CodeComment comment)
		{
			TextWriter output = Output;

			string commentChars = null;

			if (comment.DocComment) {
				commentChars = "///";
			} else {
				commentChars = "//";
			}

			output.Write (commentChars);
			output.Write (' ');
			string text = comment.Text;

			for (int i = 0; i < text.Length; i++) {
				output.Write (text[i]);
				if (text[i] == '\r') {
					if (i < (text.Length - 1) && text[i + 1] == '\n') {
						continue;
					}
					output.Write (commentChars);
				} else if (text[i] == '\n') {
					output.Write (commentChars);
				}
			}

			output.WriteLine ();
		}

		protected override void GenerateMethodReturnStatement (CodeMethodReturnStatement statement)
		{
			TextWriter output = Output;

			if (statement.Expression != null) {
				output.Write ("return ");
				GenerateExpression (statement.Expression);
				output.WriteLine (";");
			} else {
				output.WriteLine ("return;");
			}
		}

		protected override void GenerateConditionStatement (CodeConditionStatement statement)
		{
			TextWriter output = Output;
			output.Write ("if (");
			GenerateExpression (statement.Condition);
			output.Write (")");
			OutputStartBrace ();

			++Indent;
			GenerateStatements (statement.TrueStatements);
			--Indent;

			CodeStatementCollection falses = statement.FalseStatements;
			if (falses.Count > 0) {
				output.Write ('}');
				if (Options.ElseOnClosing)
					output.Write (' ');
				else
					output.WriteLine ();
				output.WriteLine ("else");
				OutputStartBrace ();
				++Indent;
				GenerateStatements (falses);
				--Indent;
			}
			output.WriteLine ('}');
		}

		protected override void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement statement)
		{
			TextWriter output = Output;
			CodeGeneratorOptions options = Options;

			output.Write ("try");
			OutputStartBrace ();
			++Indent;
			GenerateStatements (statement.TryStatements);
			--Indent;
			
			foreach (CodeCatchClause clause in statement.CatchClauses) {
				output.Write ('}');
				if (options.ElseOnClosing)
					output.Write (' ');
				else
					output.WriteLine ();
				output.Write ("catch (");
				OutputTypeNamePair (clause.CatchExceptionType, GetSafeName(clause.LocalName));
				output.Write (")");
				OutputStartBrace ();
				++Indent;
				GenerateStatements (clause.Statements);
				--Indent;
			}

			CodeStatementCollection finallies = statement.FinallyStatements;
			if (finallies.Count > 0) {
				output.Write ('}');
				if (options.ElseOnClosing)
					output.Write (' ');
				else
					output.WriteLine ();
				output.Write ("finally");
				OutputStartBrace ();
				++Indent;
				GenerateStatements (finallies);
				--Indent;
			}

			output.WriteLine('}');
		}

		protected override void GenerateAssignStatement (CodeAssignStatement statement)
		{
			TextWriter output = Output;
			GenerateExpression (statement.Left);
			output.Write (" = ");
			GenerateExpression (statement.Right);
			if (dont_write_semicolon)
				return;
			output.WriteLine (';');
		}

		protected override void GenerateAttachEventStatement (CodeAttachEventStatement statement)
		{
			TextWriter output = Output;

			GenerateEventReferenceExpression (statement.Event);
			output.Write (" += ");
			GenerateExpression (statement.Listener);
			output.WriteLine (';');
		}

		protected override void GenerateRemoveEventStatement (CodeRemoveEventStatement statement)
		{
			TextWriter output = Output;
			GenerateEventReferenceExpression (statement.Event);
			Output.Write (" -= ");
			GenerateExpression (statement.Listener);
			output.WriteLine (';');
		}

		protected override void GenerateGotoStatement (CodeGotoStatement statement)
		{
			TextWriter output = Output;

			output.Write ("goto ");
			output.Write (GetSafeName (statement.Label));
			output.WriteLine (";");
		}
		
		protected override void GenerateLabeledStatement (CodeLabeledStatement statement)
		{
			Output.Write (String.Concat (GetSafeName (statement.Label), ": "));

			if (statement.Statement != null)
				GenerateStatement (statement.Statement);
		}

		protected override void GenerateVariableDeclarationStatement (CodeVariableDeclarationStatement statement)
		{
			TextWriter output = Output;

			OutputTypeNamePair (statement.Type, GetSafeName (statement.Name));

			CodeExpression initExpression = statement.InitExpression;
			if (initExpression != null) {
				output.Write (" = ");
				GenerateExpression (initExpression);
			}

			output.WriteLine (';');
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

		protected override void GenerateEvent (CodeMemberEvent eventRef, CodeTypeDeclaration declaration)
		{
			if (IsCurrentDelegate || IsCurrentEnum) {
				return;
			}

			OutputAttributes (eventRef.CustomAttributes, null, false);

			if (eventRef.PrivateImplementationType == null) {
				OutputMemberAccessModifier (eventRef.Attributes);
			}

			Output.Write ("event ");

			if (eventRef.PrivateImplementationType != null) {
				OutputTypeNamePair (eventRef.Type,
					eventRef.PrivateImplementationType.BaseType + "." + 
					eventRef.Name);
			} else {
				OutputTypeNamePair (eventRef.Type, GetSafeName (eventRef.Name));
			}
			Output.WriteLine (';');
		}

		protected override void GenerateField (CodeMemberField field)
		{
			if (IsCurrentDelegate || IsCurrentInterface) {
				return;
			}

			TextWriter output = Output;

			OutputAttributes (field.CustomAttributes, null, false);

			if (IsCurrentEnum) {
				Output.Write (GetSafeName (field.Name));
			} else {
				MemberAttributes attributes = field.Attributes;
				OutputMemberAccessModifier (attributes);
				OutputVTableModifier (attributes);
				OutputFieldScopeModifier (attributes);

				OutputTypeNamePair (field.Type, GetSafeName (field.Name));
			}

			CodeExpression initExpression = field.InitExpression;
			if (initExpression != null) {
				output.Write (" = ");
				GenerateExpression (initExpression);
			}

			if (IsCurrentEnum)
				output.WriteLine (',');
			else
				output.WriteLine (';');
		}
		
		protected override void GenerateSnippetMember (CodeSnippetTypeMember member)
		{
			Output.Write (member.Text);
		}
		
		protected override void GenerateEntryPointMethod (CodeEntryPointMethod method, 
								  CodeTypeDeclaration declaration)
		{
#if NET_2_0
			OutputAttributes (method.CustomAttributes, null, false);
#endif

			Output.Write ("public static ");
#if NET_2_0
			OutputType (method.ReturnType);
#else
			Output.Write ("void");
#endif
			Output.Write (" Main()");
			OutputStartBrace ();
			Indent++;
			GenerateStatements (method.Statements);
			Indent--;
			Output.WriteLine ("}");
		}
		
		protected override void GenerateMethod (CodeMemberMethod method,
							CodeTypeDeclaration declaration)
		{
			if (IsCurrentDelegate || IsCurrentEnum) {
				return;
			}

			TextWriter output = Output;

			OutputAttributes (method.CustomAttributes, null, false);

			OutputAttributes (method.ReturnTypeCustomAttributes, 
				"return: ", false);

			MemberAttributes attributes = method.Attributes;

			if (!IsCurrentInterface) {
				if (method.PrivateImplementationType == null) {
					OutputMemberAccessModifier (attributes);
					OutputVTableModifier (attributes);
					OutputMemberScopeModifier (attributes);
				}
			} else {
				OutputVTableModifier (attributes);
			}

			OutputType (method.ReturnType);
			output.Write (' ');

			CodeTypeReference privateType = method.PrivateImplementationType;
			if (privateType != null) {
				output.Write (privateType.BaseType);
				output.Write ('.');
			}
			output.Write (GetSafeName (method.Name));

#if NET_2_0
			GenerateGenericsParameters (method.TypeParameters);
#endif

			output.Write ('(');
			OutputParameters (method.Parameters);
			output.Write (')');

#if NET_2_0
			GenerateGenericsConstraints (method.TypeParameters);
#endif

			if ((attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract || declaration.IsInterface)
				output.WriteLine (';');
			else {
				OutputStartBrace ();
				++Indent;
				GenerateStatements (method.Statements);
				--Indent;
				output.WriteLine ('}');
			}
		}

		protected override void GenerateProperty (CodeMemberProperty property,
							  CodeTypeDeclaration declaration)
		{
			if (IsCurrentDelegate || IsCurrentEnum) {
				return;
			}

			TextWriter output = Output;

			OutputAttributes (property.CustomAttributes, null, false);

			MemberAttributes attributes = property.Attributes;

			if (!IsCurrentInterface) {
				if (property.PrivateImplementationType == null) {
					OutputMemberAccessModifier (attributes);
					OutputVTableModifier (attributes);
					OutputMemberScopeModifier (attributes);
				}
			} else {
				OutputVTableModifier (attributes);
			}

			OutputType (property.Type);
			output.Write (' ');

			if (!IsCurrentInterface && property.PrivateImplementationType != null) {
				output.Write (property.PrivateImplementationType.BaseType);
				output.Write ('.');
			}

			// only consider property indexer if name is Item (case-insensitive 
			// comparison) AND property has parameters
			if (string.Compare(property.Name, "Item", true, CultureInfo.InvariantCulture) == 0 && property.Parameters.Count > 0) {
				output.Write ("this[");
				OutputParameters(property.Parameters);
				output.Write(']');
			} else {
				output.Write (property.Name);
			}
			OutputStartBrace ();
			++Indent;

			if (declaration.IsInterface)
			{
				if (property.HasGet) output.WriteLine("get;");
				if (property.HasSet) output.WriteLine("set;");
			}
			else
			{
				if (property.HasGet)
				{
					output.Write ("get");
					OutputStartBrace ();
					++Indent;

					GenerateStatements (property.GetStatements);

					--Indent;
					output.WriteLine ('}');
				}

				if (property.HasSet)
				{
					output.Write ("set");
					OutputStartBrace ();
					++Indent;

					GenerateStatements (property.SetStatements);

					--Indent;
					output.WriteLine ('}');
				}
			}

			--Indent;
			output.WriteLine ('}');
		}

		protected override void GenerateConstructor (CodeConstructor constructor, CodeTypeDeclaration declaration)
		{
			if (IsCurrentDelegate || IsCurrentEnum || IsCurrentInterface) {
				return;
			}

			OutputAttributes (constructor.CustomAttributes, null, false);

			OutputMemberAccessModifier (constructor.Attributes);
			Output.Write (GetSafeName (CurrentTypeName) + "(");
			OutputParameters (constructor.Parameters);
			Output.Write (")");
			if (constructor.BaseConstructorArgs.Count > 0) {
				Output.WriteLine (" : ");
				Indent += 2;
				Output.Write ("base(");
				OutputExpressionList (constructor.BaseConstructorArgs);
				Output.Write (')');
				Indent -= 2;
			}
			if (constructor.ChainedConstructorArgs.Count > 0) {
				Output.WriteLine (" : ");
				Indent += 2;
				Output.Write("this(");
				OutputExpressionList (constructor.ChainedConstructorArgs);
				Output.Write(')');
				Indent -= 2;
			}
			OutputStartBrace ();
			Indent++;
			GenerateStatements (constructor.Statements);
			Indent--;
			Output.WriteLine ('}');
		}
		
		protected override void GenerateTypeConstructor (CodeTypeConstructor constructor)
		{
			if (IsCurrentDelegate || IsCurrentEnum || IsCurrentInterface) {
				return;
			}

#if NET_2_0
			OutputAttributes (constructor.CustomAttributes, null, false);
#endif

			Output.Write ("static " + GetSafeName (CurrentTypeName) + "()");
			OutputStartBrace ();
			Indent++;
			GenerateStatements (constructor.Statements);
			Indent--;
			Output.WriteLine ('}');
		}

		protected override void GenerateTypeStart(CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;

			OutputAttributes (declaration.CustomAttributes, null, false);

			if (!IsCurrentDelegate) {
				OutputTypeAttributes (declaration);

				output.Write (GetSafeName (declaration.Name));

#if NET_2_0
				GenerateGenericsParameters (declaration.TypeParameters);
#endif

				IEnumerator enumerator = declaration.BaseTypes.GetEnumerator ();
				if (enumerator.MoveNext ()) {
					CodeTypeReference type = (CodeTypeReference) enumerator.Current;

					output.Write (" : ");
					OutputType (type);

					while (enumerator.MoveNext ()) {
						type = (CodeTypeReference) enumerator.Current;

						output.Write (", ");
						OutputType (type);
					}
				}

#if NET_2_0
				GenerateGenericsConstraints (declaration.TypeParameters);
#endif
				OutputStartBrace ();
				++Indent;
			} else {
				if ((declaration.TypeAttributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public) {
					output.Write ("public ");
				}

				CodeTypeDelegate delegateDecl = (CodeTypeDelegate) declaration;
				output.Write ("delegate ");
				OutputType (delegateDecl.ReturnType);
				output.Write (" ");
				output.Write (GetSafeName (declaration.Name));
				output.Write ("(");
				OutputParameters (delegateDecl.Parameters);
				output.WriteLine (");");
			}
		}

		protected override void GenerateTypeEnd (CodeTypeDeclaration declaration)
		{
			if (!IsCurrentDelegate) {
				--Indent;
				Output.WriteLine ("}");
			}
		}

		protected override void GenerateNamespaceStart (CodeNamespace ns)
		{
			TextWriter output = Output;
			
			string name = ns.Name;
			if (name != null && name.Length != 0) {
				output.Write ("namespace ");
				output.Write (GetSafeName (name));
				OutputStartBrace ();
				++Indent;
			}
		}

		protected override void GenerateNamespaceEnd (CodeNamespace ns)
		{
			string name = ns.Name;
			if (name != null && name.Length != 0) {
				--Indent;
				Output.WriteLine ("}");
			}
		}

		protected override void GenerateNamespaceImport (CodeNamespaceImport import)
		{
			TextWriter output = Output;

			output.Write ("using ");
			output.Write (GetSafeName (import.Namespace));
			output.WriteLine (';');
		}
		
		protected override void GenerateAttributeDeclarationsStart (CodeAttributeDeclarationCollection attributes)
		{
			Output.Write ('[');
		}
		
		protected override void GenerateAttributeDeclarationsEnd (CodeAttributeDeclarationCollection attributes)
		{
			Output.Write (']');
		}

		private void OutputStartBrace ()
		{
			if (Options.BracingStyle == "C") {
				Output.WriteLine ("");
				Output.WriteLine ("{");
			} else {
				Output.WriteLine (" {");
			}
		}

		private void OutputAttributes (CodeAttributeDeclarationCollection attributes, string prefix, bool inline)
		{
			foreach (CodeAttributeDeclaration att in attributes) {
				GenerateAttributeDeclarationsStart (attributes);
				if (prefix != null) {
					Output.Write (prefix);
				}
				OutputAttributeDeclaration (att);
				GenerateAttributeDeclarationsEnd (attributes);
				if (inline) {
					Output.Write (" ");
				} else {
					Output.WriteLine ();
				}
			}
		}

		private void OutputAttributeDeclaration (CodeAttributeDeclaration attribute)
		{
			Output.Write (attribute.Name.Replace ('+', '.'));
			Output.Write ('(');
			IEnumerator enumerator = attribute.Arguments.GetEnumerator ();
			if (enumerator.MoveNext ()) {
				CodeAttributeArgument argument = (CodeAttributeArgument) enumerator.Current;
				OutputAttributeArgument (argument);

				while (enumerator.MoveNext ()) {
					Output.Write (", ");
					argument = (CodeAttributeArgument) enumerator.Current;
					OutputAttributeArgument (argument);
				}
			}
			Output.Write (')');
		}

		protected override void OutputType (CodeTypeReference type)
		{
			Output.Write (GetTypeOutput (type));
		}

		private void OutputVTableModifier (MemberAttributes attributes)
		{
			if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New) {
				Output.Write ("new ");
			}
		}

		protected override void OutputFieldScopeModifier (MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.ScopeMask) {
				case MemberAttributes.Static:
					Output.Write ("static ");
					break;
				case MemberAttributes.Const:
					Output.Write ("const ");
					break;
			}
		}

#if NET_2_0

		// Note: this method should in fact be private as in .NET 2.0, the 
		// CSharpCodeGenerator no longer derives from CodeGenerator but we
		// still need to make this change.
		protected override void OutputMemberAccessModifier (MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.AccessMask) {
				case MemberAttributes.Assembly:
				case MemberAttributes.FamilyAndAssembly:
					Output.Write ("internal "); 
					break;
				case MemberAttributes.Family:
					Output.Write ("protected ");
					break;
				case MemberAttributes.FamilyOrAssembly:
					Output.Write ("protected internal ");
					break;
				case MemberAttributes.Private:
					Output.Write ("private ");
					break;
				case MemberAttributes.Public:
					Output.Write ("public ");
					break;
			}
		}

		// Note: this method should in fact be private as in .NET 2.0, the 
		// CSharpCodeGenerator no longer derives from CodeGenerator but we
		// still need to make this change.
		protected override void OutputMemberScopeModifier (MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.ScopeMask) {
				case MemberAttributes.Abstract:
					Output.Write ("abstract ");
					break;
				case MemberAttributes.Final:
					// do nothing
					break;
				case MemberAttributes.Static:
					Output.Write ("static ");
					break;
				case MemberAttributes.Override:
					Output.Write ("override ");
					break;
				default:
					MemberAttributes access = attributes & MemberAttributes.AccessMask;
					if (access == MemberAttributes.Assembly || access == MemberAttributes.Family || access == MemberAttributes.Public) {
						Output.Write ("virtual ");
					}
					break;
			}
		}
#endif

		private void OutputTypeAttributes (CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;
			TypeAttributes attributes = declaration.TypeAttributes;

			switch (attributes & TypeAttributes.VisibilityMask) {
				case TypeAttributes.Public:
				case TypeAttributes.NestedPublic:
					output.Write ("public ");
					break;
				case TypeAttributes.NestedPrivate:
					output.Write ("private ");
					break;
#if NET_2_0
				case TypeAttributes.NotPublic:
				case TypeAttributes.NestedFamANDAssem:
				case TypeAttributes.NestedAssembly:
					output.Write ("internal ");
					break; 
				case TypeAttributes.NestedFamily:
					output.Write ("protected ");
					break;
				case TypeAttributes.NestedFamORAssem:
					output.Write ("protected internal ");
					break;
#endif
			}

			if (declaration.IsStruct) {
#if NET_2_0
				if (declaration.IsPartial) {
					output.Write ("partial ");
				}
#endif
				output.Write ("struct ");
			} else if (declaration.IsEnum) {
				output.Write ("enum ");
			} else {
				if ((attributes & TypeAttributes.Interface) != 0) {
#if NET_2_0
					if (declaration.IsPartial) {
						output.Write ("partial ");
					}
#endif
					output.Write ("interface ");
				} else {
					if ((attributes & TypeAttributes.Sealed) != 0)
						output.Write ("sealed ");
					if ((attributes & TypeAttributes.Abstract) != 0)
						output.Write ("abstract ");
#if NET_2_0
					if (declaration.IsPartial) {
						output.Write ("partial ");
					}
#endif
					output.Write ("class ");
				}
			}
		}

		[MonoTODO ("Implement missing special characters")]
		protected override string QuoteSnippetString (string value)
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
			OutputAttributes (e.CustomAttributes, null, true);
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
    
		protected override string GetTypeOutput (CodeTypeReference type)
		{
			string typeOutput = null;

			if (type.ArrayElementType != null) {
				typeOutput = GetTypeOutput (type.ArrayElementType);
			} else {
				typeOutput = DetermineTypeOutput (type);
			}

			int rank = type.ArrayRank;
			if (rank > 0) {
				typeOutput += '[';
				for (--rank; rank > 0; --rank) {
					typeOutput += ',';
				}
				typeOutput += ']';
			}
			return typeOutput;
		}

		private string DetermineTypeOutput (CodeTypeReference type)
		{
			string typeOutput = null;
			string baseType = type.BaseType;

			switch (baseType.ToLower (System.Globalization.CultureInfo.InvariantCulture)) {
				case "system.int32":
					typeOutput = "int";
					break;
				case "system.int64":
					typeOutput = "long";
					break;
				case "system.int16":
					typeOutput = "short";
					break;
				case "system.boolean":
					typeOutput = "bool";
					break;
				case "system.char":
					typeOutput = "char";
					break;
				case "system.string":
					typeOutput = "string";
					break;
				case "system.object":
					typeOutput = "object";
					break;
				case "system.void":
					typeOutput = "void";
					break;
#if NET_2_0
				case "system.byte":
					typeOutput = "byte";
					break;
				case "system.sbyte":
					typeOutput = "sbyte";
					break;
				case "system.decimal":
					typeOutput = "decimal";
					break;
				case "system.double":
					typeOutput = "double";
					break;
				case "system.single":
					typeOutput = "float";
					break;
				case "system.uint16":
					typeOutput = "ushort";
					break;
				case "system.uint32":
					typeOutput = "uint";
					break;
				case "system.uint64":
					typeOutput = "ulong";
					break;
#endif
				default:
#if NET_2_0
					StringBuilder sb = new StringBuilder (baseType.Length);
					if (type.Options == CodeTypeReferenceOptions.GlobalReference) {
						sb.Append ("global::");
					}

					int lastProcessedChar = 0;
					for (int i = 0; i < baseType.Length; i++) {
						char currentChar = baseType[i];
						if (currentChar != '+' && currentChar != '.') {
							if (currentChar == '`') {
								sb.Append (CreateEscapedIdentifier (baseType.Substring (
									lastProcessedChar, i - lastProcessedChar)));
								// skip ` character
								i++;
								// determine number of type arguments to output
								int typeArgCount = baseType[i] - '0';
								// output type arguments
								OutputTypeArguments (type.TypeArguments, sb, typeArgCount);
								// skip type argument indicator
								i++;
								// if next character is . or +, then append .
								if ((i < baseType.Length) && ((baseType[i] == '+') || (baseType[i] == '.'))) {
									sb.Append ('.');
									// skip character that we just processed
									i++;
								}
								// save postion of last processed character
								lastProcessedChar = i;
							}
						} else {
							sb.Append (CreateEscapedIdentifier (baseType.Substring (
								lastProcessedChar, i - lastProcessedChar)));
							sb.Append ('.');
							// skip separator
							i++;
							// save postion of last processed character
							lastProcessedChar = i;
						}
					}

					// add characters that have not yet been processed 
					if (lastProcessedChar < baseType.Length) {
						sb.Append (CreateEscapedIdentifier (baseType.Substring (lastProcessedChar)));
					}

					typeOutput = sb.ToString ();
#else
					typeOutput = GetSafeName (baseType);
					typeOutput = typeOutput.Replace ('+', '.');
#endif
					break;
			}
			return typeOutput;
		}

		protected override bool IsValidIdentifier (string identifier)
		{
			if (keywordsTable == null)
				FillKeywordTable ();

			return !keywordsTable.Contains (identifier);
		}

		protected override bool Supports (GeneratorSupport supports)
		{
			return true;
		}

#if NET_2_0
		protected override void GenerateDirectives (CodeDirectiveCollection directives)
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
			if (pragma.ChecksumData != null) {
				foreach (byte b in pragma.ChecksumData) {
					Output.Write (b.ToString ("X2"));
				}
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

		private void OutputTypeArguments (CodeTypeReferenceCollection typeArguments, StringBuilder sb, int count)
		{
			if (count == 0) {
				return;
			}

			sb.Append ('<');

			// write first type argument
			sb.Append (GetTypeOutput (typeArguments[0]));
			// subsequent type argument are prefixed by ', ' separator
			for (int i = 1; i < count; i++) {
				sb.Append (", ");
				sb.Append (GetTypeOutput (typeArguments[i]));
			}

			sb.Append ('>');
		}
#endif

#if false
		//[MonoTODO]
		public override void ValidateIdentifier (string identifier)
		{
		}
#endif

		private string GetSafeName (string id)
		{
			if (keywordsTable == null) {
				FillKeywordTable ();
			}
			if (keywordsTable.Contains (id)) {
				return "@" + id;
			} else {
				return id;
			}
		}

		static void FillKeywordTable ()
		{
			keywordsTable = new Hashtable ();
			foreach (string keyword in keywords) {
				keywordsTable.Add (keyword, keyword);
			}
		}

		private static Hashtable keywordsTable;
		private static string[] keywords = new string[] {
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
