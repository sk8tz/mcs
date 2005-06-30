//
// Microsoft.VisualBasic.* Test Cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) Novell
//
using System;
using System.Globalization;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

using NUnit.Framework;

namespace MonoTests.Microsoft.VisualBasic
{
	///
	/// <summary>
	///	Test ICodeGenerator's GenerateCodeFromType, along with a 
	///	minimal set CodeDom components.
	/// </summary>
	///
	[TestFixture]
	public class CodeGeneratorFromTypeTest : CodeGeneratorTestBase
	{
		CodeTypeDeclaration type = null;

		[SetUp]
		public void Init ()
		{
			InitBase ();
			type = new CodeTypeDeclaration ();
		}
		
		protected override void Generate ()
		{
			generator.GenerateCodeFromType (type, writer, options);
			writer.Close ();
		}
		
		[Test]
		public void DefaultTypeTest ()
		{
			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class {0}End Class{0}", writer.NewLine), Code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullTypeTest ()
		{
			type = null;
			Generate ();
		}

		[Test]
		public void SimpleTypeTest ()
		{
			type.Name = "Test1";
			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void AttributesAndTypeTest ()
		{
			type.Name = "Test1";

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			type.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			type.CustomAttributes.Add (attrDec);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<A(),  _{0} B()>  _{0}Public Class Test1{0}End Class{0}", 
				writer.NewLine), Code);
		}

		[Test]
		public void EventMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			evt.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			evt.CustomAttributes.Add (attrDec);

			type.Members.Add (evt);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}    {0}    <A(),  _{0}     B()>  _{0}    "
				+ "Private Event  As System.Void{0}End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void EventMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();
			evt.Name = "OnClick";
			evt.Attributes = MemberAttributes.Public;
			evt.Type = new CodeTypeReference(typeof (int));
			type.Members.Add (evt);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}    {0}    "
				+ "Public Event OnClick As Integer{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void FieldMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberField fld = new CodeMemberField ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			fld.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			fld.CustomAttributes.Add (attrDec);

			type.Members.Add (fld);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}    {0}    <A(),  _{0}     B()>  _{0}    "
				+ "Private  As System.Void{0}End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void FieldMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberField fld = new CodeMemberField ();
			fld.Name = "Name";
			fld.Attributes = MemberAttributes.Public;
			fld.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (fld);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}    {0}    "
				+ "Public Name As Integer{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			property.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			property.CustomAttributes.Add (attrDec);

			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    <A(),  _{0}"
				+ "     B()>  _{0}"
#if NET_2_0
				+ "    Private Property () As System.Void{0}"
#else
				+ "    Private Property  As System.Void{0}"
#endif
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
#if NET_2_0
				+ "    Public Overridable Property Name() As Integer{0}"
#else
				+ "    Public Overridable Property Name As Integer{0}"
#endif
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeGetOnly ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Family;
			property.HasGet = true;
			property.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
#if NET_2_0
				+ "    Protected Overridable ReadOnly Property Name() As Integer{0}"
#else
				+ "    Protected Overridable ReadOnly Property Name As Integer{0}"
#endif
				+ "        Get{0}"
				+ "        End Get{0}"
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeSetOnly ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.FamilyAndAssembly;
			property.HasSet = true;
			property.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
#if NET_2_0
				+ "    Friend WriteOnly Property Name() As Integer{0}"
#else
				+ "    Friend WriteOnly Property Name As Integer{0}"
#endif
				+ "        Set{0}"
				+ "        End Set{0}"
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeGetSet ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Family;
			property.HasGet = true;
			property.HasSet = true;
			property.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
#if NET_2_0
				+ "    Protected Overridable Property Name() As Integer{0}"
#else
				+ "    Protected Overridable Property Name As Integer{0}"
#endif
				+ "        Get{0}"
				+ "        End Get{0}"
				+ "        Set{0}"
				+ "        End Set{0}"
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

#if !NET_2_0
		// A bug in MS.NET 1.x causes MemberAttributes.FamilyOrAssembly to be 
		// generated as Protected
		[Category("NotDotNet")]
#endif
		[Test]
		public void PropertyMembersTypeFamilyOrAssembly ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.FamilyOrAssembly;
			property.Type = new CodeTypeReference (typeof (int));

			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
#if NET_2_0
				+ "    Protected Friend Property Name() As Integer{0}"
#else
				+ "    Protected Friend Property Name As Integer{0}"
#endif
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

#if !NET_2_0
		// A bug in MS.NET 1.x causes MemberAttributes.Assembly to be generated
		// as Protected
		[Category ("NotDotNet")]
#endif
		[Test]
		public void PropertyMembersTypeAssembly ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Assembly;
			property.Type = new CodeTypeReference (typeof (int));

			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
#if NET_2_0
				+ "    Friend Overridable Property Name() As Integer{0}"
#else
				+ "    Friend Property Name As Integer{0}"
#endif
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		/// <summary>
		/// Apparently VB.NET CodeDOM also allows properties that aren't indexers
		/// to have parameters.
		/// </summary>
		[Test]
		public void PropertyParametersTest ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));

			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			property.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (
				typeof (int), "value2");
			param.Direction = FieldDirection.Ref;
			property.Parameters.Add (param);

			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Overridable Property Name(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}"
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyIndexerTest1 ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			// ensure case-insensitive comparison is done on name of property
			property.Name = "iTem";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));

			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			property.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (
				typeof (int), "value2");
			param.Direction = FieldDirection.Ref;
			property.Parameters.Add (param);

			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Overridable Default Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}"
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		/// <summary>
		/// Ensures Default keyword is only output if property is named "Item"
		/// (case-insensitive comparison) AND parameters are defined.
		/// </summary>
		[Test]
		public void PropertyIndexerTest2 ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			// ensure case-insensitive comparison is done on name of property
			property.Name = "iTem";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
#if NET_2_0
				+ "    Public Overridable Property iTem() As Integer{0}"
#else
				+ "    Public Overridable Property iTem As Integer{0}"
#endif
				+ "    End Property{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void MethodMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			method.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			method.CustomAttributes.Add (attrDec);

			type.Members.Add (method);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" 
				+ "    {0}"
				+ "    <A(),  _{0}" 
				+ "     B()>  _{0}"
				+ "    Private Sub (){0}"
				+ "    End Sub{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void MethodMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Something";
			method.Attributes = MemberAttributes.Public;
			method.ReturnType = new CodeTypeReference (typeof (int));

			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof(object), "value1");
			method.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (
				typeof (object), "value2");
			param.Direction = FieldDirection.In;
			method.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (typeof (int), "index");
			param.Direction = FieldDirection.Out;
			method.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (typeof (int), "count");
			param.Direction = FieldDirection.Ref;
			method.Parameters.Add (param);

			type.Members.Add (method);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Overridable Function Something(ByVal value1 As Object, ByVal value2 As Object, ByRef index As Integer, ByRef count As Integer) As Integer{0}"
				+ "    End Function{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void MethodMembersTypeTest3 ()
		{
			type.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Something";
			method.Attributes = MemberAttributes.Public;
			method.ReturnType = new CodeTypeReference (typeof (int));

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value");
			method.Parameters.Add (param);

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			param.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			param.CustomAttributes.Add (attrDec);


			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "index");
			param.Direction = FieldDirection.Out;
			method.Parameters.Add (param);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "C";
			attrDec.Arguments.Add (new CodeAttributeArgument ("A1",
				new CodePrimitiveExpression (false)));
			attrDec.Arguments.Add (new CodeAttributeArgument ("A2",
				new CodePrimitiveExpression (true)));
			param.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "D";
			param.CustomAttributes.Add (attrDec);

			type.Members.Add (method);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Overridable Function Something(<A(), B()> ByVal value As Object, <C(A1:=false, A2:=true), D()> ByRef index As Integer) As Integer{0}"
				+ "    End Function{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void ConstructorAttributesTest ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			ctor.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			ctor.CustomAttributes.Add (attrDec);

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    <A(),  _{0}"
				+ "     B()>  _{0}"
				+ "    Private Sub New(){0}"
				+ "        MyBase.New{0}"
				+ "    End Sub{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void ConstructorParametersTest ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Whatever";
			ctor.Attributes = MemberAttributes.Public;

			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (
				typeof (object), "value2");
			param.Direction = FieldDirection.In;
			ctor.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (typeof (int), "index");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (typeof (int), "count");
			param.Direction = FieldDirection.Ref;
			ctor.Parameters.Add (param);

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Sub New(ByVal value1 As Object, ByVal value2 As Object, ByRef index As Integer, ByRef count As Integer){0}"
				+ "        MyBase.New{0}"
				+ "    End Sub{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void ConstructorParameterAttributesTest ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value");
			ctor.Parameters.Add (param);

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			param.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			param.CustomAttributes.Add (attrDec);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "index");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "C";
			attrDec.Arguments.Add (new CodeAttributeArgument ("A1",
				new CodePrimitiveExpression (false)));
			attrDec.Arguments.Add (new CodeAttributeArgument ("A2",
				new CodePrimitiveExpression (true)));
			param.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "D";
			param.CustomAttributes.Add (attrDec);

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Sub New(<A(), B()> ByVal value As Object, <C(A1:=false, A2:=true), D()> ByRef index As Integer){0}"
				+ "        MyBase.New{0}"
				+ "    End Sub{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void BaseConstructorSingleArg ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "value2");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			// base ctor args
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}"
				+ "        MyBase.New(value1){0}"
				+ "    End Sub{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void BaseConstructorMultipleArgs ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "value2");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			// base ctor args
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value2"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}"
				+ "        MyBase.New(value1, value2){0}"
				+ "    End Sub{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void ChainedConstructorSingleArg ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "value2");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			// chained ctor args
			ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));

			// should be ignored as chained ctor args should take precedence over base 
			// ctor args
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value3"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}"
				+ "        Me.New(value1){0}"
				+ "    End Sub{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}

		[Test]
		public void ChainedConstructorMultipleArgs ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "value2");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			// chained ctor args
			ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));
			ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value2"));

			// should be ignored as chained ctor args should take precedence over base 
			// ctor args
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value3"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}"
				+ "    {0}"
				+ "    Public Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}"
				+ "        Me.New(value1, value2){0}"
				+ "    End Sub{0}"
				+ "End Class{0}", writer.NewLine), Code);
		}
	}
}
