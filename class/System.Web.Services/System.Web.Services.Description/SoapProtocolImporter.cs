// 
// System.Web.Services.Description.SoapProtocolImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.CodeDom;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Services.Configuration;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;
using System.Collections;

namespace System.Web.Services.Description {
	public sealed class SoapProtocolImporter : ProtocolImporter {

		#region Fields

		SoapBinding soapBinding;
		SoapCodeExporter soapExporter;
		SoapSchemaImporter soapImporter;
		XmlCodeExporter xmlExporter;
		XmlSchemaImporter xmlImporter;
		CodeIdentifiers memberIds;
		ArrayList extensionImporters;
		Hashtable headerVariables;
		
		#endregion // Fields

		#region Constructors

		public SoapProtocolImporter ()
		{
			extensionImporters = ExtensionManager.BuildExtensionImporters ();
		}
		
		void SetBinding (SoapBinding soapBinding)
		{
			this.soapBinding = soapBinding;
		}
		
		#endregion // Constructors

		#region Properties

		public override string ProtocolName {
			get { return "Soap"; }
		}

		public SoapBinding SoapBinding {
			get { return soapBinding; }
		}

		public SoapCodeExporter SoapExporter {
			get { return soapExporter; }
		}

		public SoapSchemaImporter SoapImporter {
			get { return soapImporter; }
		}

		public XmlCodeExporter XmlExporter {
			get { return xmlExporter; }
		}

		public XmlSchemaImporter XmlImporter {
			get { return xmlImporter; }
		}

		#endregion // Properties

		#region Methods

		protected override CodeTypeDeclaration BeginClass ()
		{
			soapBinding = (SoapBinding) Binding.Extensions.Find (typeof(SoapBinding));
			if (soapBinding.Style != SoapBindingStyle.Document) throw new Exception ("Binding style not supported");
			
			CodeTypeDeclaration codeClass = new CodeTypeDeclaration (ClassName);
			
			string location = null;			
			SoapAddressBinding sab = (SoapAddressBinding) Port.Extensions.Find (typeof(SoapAddressBinding));
			if (sab != null) location = sab.Location;
			string url = GetServiceUrl (location); 

			CodeTypeReference ctr = new CodeTypeReference ("System.Web.Services.Protocols.SoapHttpClientProtocol");
			codeClass.BaseTypes.Add (ctr);
			
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Web.Services.WebServiceBinding");
			att.Arguments.Add (GetArg ("Name", Port.Name));
			att.Arguments.Add (GetArg ("Namespace", Port.Binding.Namespace));
			AddCustomAttribute (codeClass, att, true);
	
			CodeConstructor cc = new CodeConstructor ();
			cc.Attributes = MemberAttributes.Public;
			CodeExpression ce = new CodeFieldReferenceExpression (new CodeThisReferenceExpression(), "Url");
			CodeAssignStatement cas = new CodeAssignStatement (ce, new CodePrimitiveExpression (url));
			cc.Statements.Add (cas);
			codeClass.Members.Add (cc);
			
			memberIds = new CodeIdentifiers ();
			headerVariables = new Hashtable ();
			return codeClass;
		}

		protected override void BeginNamespace ()
		{
			xmlImporter = new XmlSchemaImporter (Schemas);
			soapImporter = new SoapSchemaImporter (Schemas);
			xmlExporter = new XmlCodeExporter (CodeNamespace, null);
		}

		protected override void EndClass ()
		{
			SoapTransportImporter transportImporter = SoapTransportImporter.FindTransportImporter (soapBinding.Transport);
			if (transportImporter == null) throw new Exception ("Transport '" + soapBinding.Transport + "' not supported");
			transportImporter.ImportContext = this;
			transportImporter.ImportClass ();			
		}

		protected override void EndNamespace ()
		{
		}

		protected override bool IsBindingSupported ()
		{
			return Binding.Extensions.Find (typeof(SoapBinding)) != null;
		}

		[MonoTODO]
		protected override bool IsOperationFlowSupported (OperationFlow flow)
		{
			throw new NotImplementedException ();
		}

		protected override CodeMemberMethod GenerateMethod ()
		{
			try
			{
				SoapOperationBinding soapOper = OperationBinding.Extensions.Find (typeof (SoapOperationBinding)) as SoapOperationBinding;
				if (soapOper == null) throw new Exception ("Soap operation binding not found");
				if (soapOper.Style != SoapBindingStyle.Document) throw new Exception ("Operation binding style not supported");

				SoapBodyBinding isbb = OperationBinding.Input.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
				if (isbb == null) throw new Exception ("Soap body binding not found");
				
				SoapBodyBinding osbb = OperationBinding.Output.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
				if (osbb == null) throw new Exception ("Soap body binding not found");
				
				XmlMembersMapping inputMembers = ImportMembersMapping (InputMessage, isbb, soapOper);
				if (inputMembers == null) throw new Exception ("Input message not declared");

				XmlMembersMapping outputMembers = ImportMembersMapping (OutputMessage, osbb, soapOper);
				if (outputMembers == null) throw new Exception ("Output message not declared");
				
				CodeMemberMethod met = GenerateMethod (memberIds, soapOper, isbb, inputMembers, outputMembers);
				
				xmlExporter.ExportMembersMapping (inputMembers);
				xmlExporter.ExportMembersMapping (outputMembers);

				foreach (SoapExtensionImporter eximporter in extensionImporters)
				{
					eximporter.ImportContext = this;
					eximporter.ImportMethod (met.CustomAttributes);
				}
				
				return met;
			}
			catch (Exception ex)
			{
				UnsupportedOperationBindingWarning (ex.Message);
				return null;
			}
		}
		
		XmlMembersMapping ImportMembersMapping (Message msg, SoapBodyBinding sbb, SoapOperationBinding soapOper)
		{
			XmlQualifiedName elem = null;
			if (msg.Parts.Count == 1 && msg.Parts[0].Name == "parameters")
			{
				// Wrapped parameter style
				
				MessagePart part = msg.Parts[0];
				if (sbb.Use == SoapBindingUse.Encoded)
				{
					SoapSchemaMember ssm = new SoapSchemaMember ();
					ssm.MemberName = part.Name;
					ssm.MemberType = part.Type;
					return soapImporter.ImportMembersMapping (Operation.Name, part.Type.Namespace, ssm);
				}
				else
					return xmlImporter.ImportMembersMapping (part.Element);				
			}
			else
			{
				if (sbb.Use == SoapBindingUse.Encoded)
				{
					SoapSchemaMember[] mems = new SoapSchemaMember [msg.Parts.Count];
					for (int n=0; n<mems.Length; n++)
					{
						SoapSchemaMember mem = new SoapSchemaMember();
						mem.MemberName = msg.Parts[n].Name;
						mem.MemberType = msg.Parts[n].Type;
						mems[n] = mem;
					}
					return soapImporter.ImportMembersMapping (Operation.Name, "", mems);
				}
				else
				{
					XmlQualifiedName[] pnames = new XmlQualifiedName [msg.Parts.Count];
					for (int n=0; n<pnames.Length; n++)
						pnames[n] = msg.Parts[n].Element;
					return xmlImporter.ImportMembersMapping (pnames);
				}
			}
		}
		
		CodeMemberMethod GenerateMethod (CodeIdentifiers memberIds, SoapOperationBinding soapOper, SoapBodyBinding bodyBinding, XmlMembersMapping inputMembers, XmlMembersMapping outputMembers)
		{
			CodeIdentifiers pids = new CodeIdentifiers ();
			CodeMemberMethod method = new CodeMemberMethod ();
			CodeMemberMethod methodBegin = new CodeMemberMethod ();
			CodeMemberMethod methodEnd = new CodeMemberMethod ();
			method.Attributes = MemberAttributes.Public;
			methodBegin.Attributes = MemberAttributes.Public;
			methodEnd.Attributes = MemberAttributes.Public;
			
			// Find unique names for temporary variables
			
			for (int n=0; n<inputMembers.Count; n++)
				pids.AddUnique (inputMembers[n].MemberName, inputMembers[n]);

			for (int n=0; n<outputMembers.Count; n++)
				pids.AddUnique (outputMembers[n].MemberName, outputMembers[n]);
				
			string varAsyncResult = pids.AddUnique ("asyncResult","asyncResult");
			string varResults = pids.AddUnique ("results","results");
			string varCallback = pids.AddUnique ("callback","callback");
			string varAsyncState = pids.AddUnique ("asyncState","asyncState");

			string messageName = memberIds.AddUnique(CodeIdentifier.MakeValid(Operation.Name),method);

			method.Name = Operation.Name;
			methodBegin.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("Begin" + memberIds.MakeRightCase(Operation.Name)),method);
			methodEnd.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("End" + memberIds.MakeRightCase(Operation.Name)),method);

			method.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.Parameters.Add (new CodeParameterDeclarationExpression (typeof (IAsyncResult),varAsyncResult));

			CodeExpression[] paramArray = new CodeExpression [inputMembers.Count];
			CodeParameterDeclarationExpression[] outParams = new CodeParameterDeclarationExpression [outputMembers.Count];

			for (int n=0; n<inputMembers.Count; n++)
			{
				CodeParameterDeclarationExpression param = GenerateParameter (inputMembers[n], FieldDirection.In);
				method.Parameters.Add (param);
				GenerateMemberAttributes (inputMembers, inputMembers[n], param);
				methodBegin.Parameters.Add (GenerateParameter (inputMembers[n], FieldDirection.In));
				paramArray [n] = new CodeVariableReferenceExpression (param.Name);
			}

			for (int n=0; n<outputMembers.Count; n++)
			{
				CodeParameterDeclarationExpression cpd = GenerateParameter (outputMembers[n], FieldDirection.Out);
				outParams [n] = cpd;
				
				bool found = false;
				foreach (CodeParameterDeclarationExpression ip in method.Parameters)
				{
					if (ip.Name == cpd.Name && ip.Type.BaseType == cpd.Type.BaseType) {
						ip.Direction = FieldDirection.Ref;
						methodEnd.Parameters.Add (GenerateParameter (outputMembers[n], FieldDirection.Out));
						found = true;
						break;
					}
				}
				
				if (found) continue;

				if ((outputMembers [n].ElementName == Operation.Name + "Result") || (inputMembers.Count==0 && outputMembers.Count==1)) {
					method.ReturnType = cpd.Type;
					methodEnd.ReturnType = cpd.Type;
					GenerateReturnAttributes (outputMembers, outputMembers[n], method);
					outParams [n] = null;
					continue;
				}
				
				method.Parameters.Add (cpd);
				GenerateMemberAttributes (outputMembers, outputMembers[n], cpd);
				methodEnd.Parameters.Add (GenerateParameter (outputMembers[n], FieldDirection.Out));
			}

			methodBegin.Parameters.Add (new CodeParameterDeclarationExpression (typeof (AsyncCallback),varCallback));
			methodBegin.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object),varAsyncState));
			methodBegin.ReturnType = new CodeTypeReference (typeof(IAsyncResult));

			// Array of input parameters
			
			CodeArrayCreateExpression methodParams;
			if (paramArray.Length > 0)
				methodParams = new CodeArrayCreateExpression (typeof(object), paramArray);
			else
				methodParams = new CodeArrayCreateExpression (typeof(object), 0);

			// Assignment of output parameters
			
			CodeStatementCollection outAssign = new CodeStatementCollection ();
			CodeVariableReferenceExpression arrVar = new CodeVariableReferenceExpression (varResults);
			for (int n=0; n<outParams.Length; n++)
			{
				CodeExpression index = new CodePrimitiveExpression (n);
				if (outParams[n] == null)
				{
					CodeExpression res = new CodeCastExpression (method.ReturnType, new CodeArrayIndexerExpression (arrVar, index));
					outAssign.Add (new CodeMethodReturnStatement (res));
				}
				else
				{
					CodeExpression res = new CodeCastExpression (outParams[n].Type, new CodeArrayIndexerExpression (arrVar, index));
					CodeExpression var = new CodeVariableReferenceExpression (outParams[n].Name);
					outAssign.Insert (0, new CodeAssignStatement (var, res));
				}
			}
			
			// Invoke call
			
			CodeThisReferenceExpression ethis = new CodeThisReferenceExpression();
			CodePrimitiveExpression varMsgName = new CodePrimitiveExpression (messageName);
			CodeMethodInvokeExpression inv;

			inv = new CodeMethodInvokeExpression (ethis, "Invoke", varMsgName, methodParams);
			CodeVariableDeclarationStatement dec = new CodeVariableDeclarationStatement (typeof(object[]), varResults, inv);
			method.Statements.Add (dec);
			method.Statements.AddRange (outAssign);
			
			// Begin Invoke Call
			
			CodeExpression expCallb = new CodeVariableReferenceExpression (varCallback);
			CodeExpression expAsyncs = new CodeVariableReferenceExpression (varAsyncState);
			inv = new CodeMethodInvokeExpression (ethis, "BeginInvoke", varMsgName, methodParams, expCallb, expAsyncs);
			methodBegin.Statements.Add (new CodeMethodReturnStatement (inv));
			
			// End Invoke call
			
			CodeExpression varAsyncr = new CodeVariableReferenceExpression (varAsyncResult);
			inv = new CodeMethodInvokeExpression (ethis, "EndInvoke", varAsyncr);
			dec = new CodeVariableDeclarationStatement (typeof(object[]), varResults, inv);
			methodEnd.Statements.Add (dec);
			methodEnd.Statements.AddRange (outAssign);
			
			// Attributes
			
			if (inputMembers.ElementName == "" && outputMembers.ElementName != "" || 
				inputMembers.ElementName != "" && outputMembers.ElementName == "")
				throw new Exception ("Parameter style is not the same for the input message and output message");

			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Web.Services.Protocols.SoapDocumentMethodAttribute");
			att.Arguments.Add (GetArg (soapOper.SoapAction));
			if (inputMembers.ElementName != "") {
				if (inputMembers.ElementName != method.Name) att.Arguments.Add (GetArg ("RequestElementName", inputMembers.ElementName));
				if (outputMembers.ElementName != (method.Name + "Response")) att.Arguments.Add (GetArg ("RequestElementName", outputMembers.ElementName));
				att.Arguments.Add (GetArg ("RequestNamespace", inputMembers.Namespace));
				att.Arguments.Add (GetArg ("ResponseNamespace", outputMembers.Namespace));
				att.Arguments.Add (GetEnumArg ("ParameterStyle", "System.Web.Services.Protocols.SoapParameterStyle", "Wrapped"));
			}
			else
				att.Arguments.Add (GetEnumArg ("ParameterStyle", "System.Web.Services.Protocols.SoapParameterStyle", "Bare"));
				
			att.Arguments.Add (GetEnumArg ("Use", "System.Web.Services.Description.SoapBindingUse", bodyBinding.Use.ToString()));
			AddCustomAttribute (method, att, true);
			
			att = new CodeAttributeDeclaration ("System.Web.Services.WebMethodAttribute");
			if (messageName != method.Name) att.Arguments.Add (GetArg ("MessageName",messageName));
			AddCustomAttribute (method, att, false);
			
			ImportHeaders (method);
			
			CodeTypeDeclaration.Members.Add (method);
			CodeTypeDeclaration.Members.Add (methodBegin);
			CodeTypeDeclaration.Members.Add (methodEnd);
			
			return method;
		}
		
		CodeParameterDeclarationExpression GenerateParameter (XmlMemberMapping member, FieldDirection dir)
		{
			CodeParameterDeclarationExpression par = new CodeParameterDeclarationExpression (member.TypeFullName, member.MemberName);
			par.Direction = dir;
			return par;
		}
		
		void GenerateMemberAttributes (XmlMembersMapping members, XmlMemberMapping member, CodeParameterDeclarationExpression param)
		{
			xmlExporter.AddMappingMetadata (param.CustomAttributes, member, members.Namespace);
		}
		
		void GenerateReturnAttributes (XmlMembersMapping members, XmlMemberMapping member, CodeMemberMethod method)
		{
			xmlExporter.AddMappingMetadata (method.ReturnTypeCustomAttributes, member, members.Namespace, (member.ElementName != method.Name + "Result"));
		}
		
		void ImportHeaders (CodeMemberMethod method)
		{
			foreach (object ob in OperationBinding.Input.Extensions)
			{
				SoapHeaderBinding hb = ob as SoapHeaderBinding;
				if (hb == null) continue;
				if (HasHeader (OperationBinding.Output, hb)) 
					ImportHeader (method, hb, SoapHeaderDirection.In | SoapHeaderDirection.Out);
				else
					ImportHeader (method, hb, SoapHeaderDirection.In);
			}
			
			foreach (object ob in OperationBinding.Output.Extensions)
			{
				SoapHeaderBinding hb = ob as SoapHeaderBinding;
				if (hb == null) continue;
				if (!HasHeader (OperationBinding.Input, hb)) 
					ImportHeader (method, hb, SoapHeaderDirection.Out);
			}
		}
		
		bool HasHeader (MessageBinding msg, SoapHeaderBinding hb)
		{
			foreach (object ob in msg.Extensions) 
			{
				SoapHeaderBinding mhb = ob as SoapHeaderBinding;
				if ((mhb != null) && (mhb.Message == hb.Message) && (mhb.Part == hb.Part)) 
					return true;
			}
			return false;
		}
		
		void ImportHeader (CodeMemberMethod method, SoapHeaderBinding hb, SoapHeaderDirection direction)
		{
			Message msg = ServiceDescriptions.GetMessage (hb.Message);
			if (msg == null) throw new Exception ("Message " + hb.Message + " not found");
			MessagePart part = msg.Parts [hb.Part];
			if (part == null) throw new Exception ("Message part " + hb.Part + " not found in message " + hb.Message);

			XmlTypeMapping map;
			if (hb.Use == SoapBindingUse.Literal)
				map = xmlImporter.ImportDerivedTypeMapping (part.Element, typeof (SoapHeader));
			else
				map = soapImporter.ImportDerivedTypeMapping (part.Type, typeof (SoapHeader), true);

			xmlExporter.ExportTypeMapping (map);
			bool required = false;

			string varName = headerVariables [map] as string;
			if (varName == null) 
			{
				varName = memberIds.AddUnique(CodeIdentifier.MakeValid (map.TypeName + "Value"),hb);
				headerVariables.Add (map, varName);
				CodeMemberField codeField = new CodeMemberField (map.TypeFullName, varName);
				codeField.Attributes = MemberAttributes.Public;
				CodeTypeDeclaration.Members.Add (codeField);
			}
			
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Web.Services.Protocols.SoapHeaderAttribute");
			att.Arguments.Add (GetArg (varName));
			att.Arguments.Add (GetArg ("Required", required));
			if (direction != SoapHeaderDirection.In) att.Arguments.Add (GetEnumArg ("Direction", "System.Web.Services.Protocols.SoapHeaderDirection", direction.ToString ()));
			AddCustomAttribute (method, att, true);
		}
		
		#endregion
	}
}
