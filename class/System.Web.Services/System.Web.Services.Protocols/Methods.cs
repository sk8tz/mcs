//
// Methods.cs: Information about a method and its mapping to a SOAP web service.
//
// Author:
//   Miguel de Icaza
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Ximian, Inc.
//

using System.Reflection;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Description;

namespace System.Web.Services.Protocols {

	//
	// This class represents all the information we extract from a MethodInfo
	// in the SoapHttpClientProtocol derivative stub class
	//
	internal class SoapMethodStubInfo : MethodStubInfo
	{
		internal string Action;
		internal string Binding;

		// The name/namespace of the request 
		internal string RequestName;
		internal string RequestNamespace;

		// The name/namespace of the response.
		internal string ResponseName;
		internal string ResponseNamespace;
		
		internal bool OneWay;
		internal SoapParameterStyle ParameterStyle;
		internal SoapBindingStyle SoapBindingStyle;
		internal SoapBindingUse Use;

		internal HeaderInfo[] Headers;
		internal SoapExtensionRuntimeConfig[] SoapExtensions;
		
		internal XmlMembersMapping InputMembersMapping;
		internal XmlMembersMapping OutputMembersMapping;
		
		private int requestSerializerId;
		private int responseSerializerId;
		
		internal XmlSerializer RequestSerializer
		{
			get { return TypeStub.GetSerializer (requestSerializerId); }
		}
		
		internal XmlSerializer ResponseSerializer
		{
			get { return TypeStub.GetSerializer (responseSerializerId); }
		}

		//
		// Constructor
		//
		public SoapMethodStubInfo (TypeStubInfo typeStub, LogicalMethodInfo source, object kind, XmlReflectionImporter xmlImporter, SoapReflectionImporter soapImporter)
		: base (typeStub, source)
		{
			SoapTypeStubInfo parent = (SoapTypeStubInfo) typeStub;
			XmlElementAttribute optional_ns = null;

			if (kind == null) {
				Use = parent.Use;
				RequestName = "";
				RequestNamespace = "";
				ResponseName = "";
				ResponseNamespace = "";
				ParameterStyle = parent.ParameterStyle;
				SoapBindingStyle = parent.SoapBindingStyle;
				OneWay = false;
			}
			else if (kind is SoapDocumentMethodAttribute){
				SoapDocumentMethodAttribute dma = (SoapDocumentMethodAttribute) kind;
				
				Use = dma.Use;
				if (Use == SoapBindingUse.Default) {
					if (parent.SoapBindingStyle == SoapBindingStyle.Document)
						Use = parent.Use;
					else
						Use = SoapBindingUse.Literal;
				}
				
				Action = dma.Action;
				Binding = dma.Binding;
				RequestName = dma.RequestElementName;
				RequestNamespace = dma.RequestNamespace;
				ResponseName = dma.ResponseElementName;
				ResponseNamespace = dma.ResponseNamespace;
				ParameterStyle = dma.ParameterStyle;
				if (ParameterStyle == SoapParameterStyle.Default)
					ParameterStyle = parent.ParameterStyle;
				OneWay = dma.OneWay;
				SoapBindingStyle = SoapBindingStyle.Document;
			} else {
				SoapRpcMethodAttribute rma = (SoapRpcMethodAttribute) kind;
				Use = SoapBindingUse.Encoded;	// RPC always use encoded

				Action = rma.Action;
				Binding = rma.Binding;
				RequestName = rma.RequestElementName;
				RequestNamespace = rma.RequestNamespace;
				ResponseNamespace = rma.ResponseNamespace;
				ResponseName = rma.ResponseElementName;
				ParameterStyle = SoapParameterStyle.Wrapped;
				OneWay = rma.OneWay;
				SoapBindingStyle = SoapBindingStyle.Rpc;

				// For RPC calls, make all arguments be part of the empty namespace
				optional_ns = new XmlElementAttribute ();
				optional_ns.Namespace = "";
			}

			if (OneWay){
				if (source.ReturnType != typeof (void))
					throw new Exception ("OneWay methods should not have a return value");
				if (source.OutParameters.Length != 0)
					throw new Exception ("OneWay methods should not have out/ref parameters");
			}
			
			if (Binding == null || Binding == "") Binding = parent.DefaultBinding;
			BindingInfo binfo = parent.GetBinding (Binding);
			if (binfo == null) throw new InvalidOperationException ("Type '" + parent.Type + "' is missing WebServiceBinding attribute that defines a binding named '" + Binding + "'");
			
			string serviceNamespace = binfo.Namespace;
				
			if (RequestNamespace == "") RequestNamespace = parent.LogicalType.GetWebServiceNamespace (serviceNamespace, Use);
			if (ResponseNamespace == "") ResponseNamespace = parent.LogicalType.GetWebServiceNamespace (serviceNamespace, Use);
			if (RequestName == "") RequestName = Name;
			if (ResponseName == "")	ResponseName = Name + "Response";
			if (Action == null || Action == "")
				Action = RequestNamespace.EndsWith("/") ? (RequestNamespace + Name) : (RequestNamespace + "/" + Name);
			
			bool hasWrappingElem = (ParameterStyle == SoapParameterStyle.Wrapped);
			bool writeAccessors = (SoapBindingStyle == SoapBindingStyle.Rpc);
			
			XmlReflectionMember [] in_members = BuildRequestReflectionMembers (optional_ns);
			XmlReflectionMember [] out_members = BuildResponseReflectionMembers (optional_ns);

			if (Use == SoapBindingUse.Literal) {
				InputMembersMapping = xmlImporter.ImportMembersMapping (RequestName, RequestNamespace, in_members, hasWrappingElem);
				OutputMembersMapping = xmlImporter.ImportMembersMapping (ResponseName, ResponseNamespace, out_members, hasWrappingElem);
			}
			else {
				InputMembersMapping = soapImporter.ImportMembersMapping (RequestName, RequestNamespace, in_members, hasWrappingElem, writeAccessors);
				OutputMembersMapping = soapImporter.ImportMembersMapping (ResponseName, ResponseNamespace, out_members, hasWrappingElem, writeAccessors);
			}

			requestSerializerId = parent.RegisterSerializer (InputMembersMapping);
			responseSerializerId = parent.RegisterSerializer (OutputMembersMapping);

			object[] o = source.GetCustomAttributes (typeof (SoapHeaderAttribute));
			Headers = new HeaderInfo[o.Length];
			for (int i = 0; i < o.Length; i++) {
				SoapHeaderAttribute att = (SoapHeaderAttribute) o[i];
				MemberInfo[] mems = source.DeclaringType.GetMember (att.MemberName);
				if (mems.Length == 0) throw new InvalidOperationException ("Member " + att.MemberName + " not found in class " + source.DeclaringType.FullName);
				
				Type headerType = (mems[0] is FieldInfo) ? ((FieldInfo)mems[0]).FieldType : ((PropertyInfo)mems[0]).PropertyType;
				Headers [i] = new HeaderInfo (mems[0], att);
				parent.RegisterHeaderType (headerType, serviceNamespace, Use);
			}

			SoapExtensions = SoapExtension.GetMethodExtensions (source);
		}

		XmlReflectionMember [] BuildRequestReflectionMembers (XmlElementAttribute optional_ns)
		{
			ParameterInfo [] input = MethodInfo.InParameters;
			XmlReflectionMember [] in_members = new XmlReflectionMember [input.Length];

			for (int i = 0; i < input.Length; i++)
			{
				XmlReflectionMember m = new XmlReflectionMember ();
				m.IsReturnValue = false;
				m.MemberName = input [i].Name;
				m.MemberType = input [i].ParameterType;

				m.XmlAttributes = new XmlAttributes (input[i]);
				m.SoapAttributes = new SoapAttributes (input[i]);

				if (m.MemberType.IsByRef)
					m.MemberType = m.MemberType.GetElementType ();
				if (optional_ns != null)
					m.XmlAttributes.XmlElements.Add (optional_ns);
				in_members [i] = m;
			}
			return in_members;
		}
		
		XmlReflectionMember [] BuildResponseReflectionMembers (XmlElementAttribute optional_ns)
		{
			ParameterInfo [] output = MethodInfo.OutParameters;
			bool has_return_value = !(OneWay || MethodInfo.ReturnType == typeof (void));
			XmlReflectionMember [] out_members = new XmlReflectionMember [(has_return_value ? 1 : 0) + output.Length];
			XmlReflectionMember m;
			int idx = 0;

			if (has_return_value)
			{
				m = new XmlReflectionMember ();
				m.IsReturnValue = true;
				m.MemberName = RequestName + "Result";
				m.MemberType = MethodInfo.ReturnType;

				m.XmlAttributes = new XmlAttributes (MethodInfo.ReturnTypeCustomAttributeProvider);
				m.SoapAttributes = new SoapAttributes (MethodInfo.ReturnTypeCustomAttributeProvider);

				if (optional_ns != null)
					m.XmlAttributes.XmlElements.Add (optional_ns);
				idx++;
				out_members [0] = m;
			}
			
			for (int i = 0; i < output.Length; i++)
			{
				m = new XmlReflectionMember ();
				m.IsReturnValue = false;
				m.MemberName = output [i].Name;
				m.MemberType = output [i].ParameterType;
				m.XmlAttributes = new XmlAttributes (output[i]);
				m.SoapAttributes = new SoapAttributes (output[i]);

				if (m.MemberType.IsByRef)
					m.MemberType = m.MemberType.GetElementType ();
				if (optional_ns != null)
					m.XmlAttributes.XmlElements.Add (optional_ns);
				out_members [i + idx] = m;
			}
			return out_members;
		}

		public HeaderInfo GetHeaderInfo (Type headerType)
		{
			foreach (HeaderInfo headerInfo in Headers)
				if (headerInfo.HeaderType == headerType) return headerInfo;
			return null;
		}
	}

	internal class HeaderInfo
	{
		internal MemberInfo Member;
		internal SoapHeaderAttribute AttributeInfo;
		internal Type HeaderType;

		public HeaderInfo (MemberInfo member, SoapHeaderAttribute attributeInfo)
		{
			Member = member;
			AttributeInfo = attributeInfo;
			if (Member is PropertyInfo) HeaderType = ((PropertyInfo)Member).PropertyType;
			else HeaderType = ((FieldInfo)Member).FieldType;
		}
		
		public object GetHeaderValue (object ob)
		{
			if (Member is PropertyInfo) return ((PropertyInfo)Member).GetValue (ob, null);
			else return ((FieldInfo)Member).GetValue (ob);
		}

		public void SetHeaderValue (object ob, object value)
		{
			if (Member is PropertyInfo) ((PropertyInfo)Member).SetValue (ob, value, null);
			else ((FieldInfo)Member).SetValue (ob, value);
		}

		public SoapHeaderDirection Direction
		{
			get { return AttributeInfo.Direction; }
		}
	}

	// FIXME: this class should be internal, but it needs to be public in
	// order to be serialized using XmlSerializer.
	[SoapType (Namespace="http://schemas.xmlsoap.org/soap/envelope/")]
	[XmlType (Namespace="http://schemas.xmlsoap.org/soap/envelope/")]
	public class Fault
	{
		public Fault () {}

		public Fault (SoapException ex) 
		{
			faultcode = ex.Code;
			faultstring = ex.Message;
			faultactor = ex.Actor;
			detail = ex.Detail;
		}

		public XmlQualifiedName faultcode;
		public string faultstring;
		public string faultactor;
		
		[SoapIgnore]
		public XmlNode detail;
	}
	
	//
	// Holds the metadata loaded from the type stub, as well as
	// the metadata for all the methods in the type
	//
	internal class SoapTypeStubInfo : TypeStubInfo
	{
		Hashtable[] header_serializers = new Hashtable [3];
		Hashtable[] header_serializers_byname = new Hashtable [3];
		Hashtable methods_byaction = new Hashtable (); 

		// Precomputed
		internal SoapParameterStyle      ParameterStyle;
		internal SoapServiceRoutingStyle RoutingStyle;
		internal SoapBindingUse          Use;
		internal int                     faultSerializerId = -1;
		internal SoapExtensionRuntimeConfig[][] SoapExtensions;
		internal SoapBindingStyle SoapBindingStyle;
		internal XmlReflectionImporter 	xmlImporter;
		internal SoapReflectionImporter soapImporter;

		public SoapTypeStubInfo (LogicalTypeInfo logicalTypeInfo)
		: base (logicalTypeInfo)
		{
			xmlImporter = new XmlReflectionImporter ();
			soapImporter = new SoapReflectionImporter ();
			
			object [] o;

			o = Type.GetCustomAttributes (typeof (WebServiceBindingAttribute), false);
			foreach (WebServiceBindingAttribute at in o)
				AddBinding (new BindingInfo (at, LogicalType.WebServiceNamespace));

			o = Type.GetCustomAttributes (typeof (SoapDocumentServiceAttribute), false);
			if (o.Length == 1){
				SoapDocumentServiceAttribute a = (SoapDocumentServiceAttribute) o [0];

				ParameterStyle = a.ParameterStyle;
				RoutingStyle = a.RoutingStyle;
				Use = a.Use;
				SoapBindingStyle = SoapBindingStyle.Document;
			} else {
				o = Type.GetCustomAttributes (typeof (SoapRpcServiceAttribute), false);
				if (o.Length == 1){
					SoapRpcServiceAttribute srs = (SoapRpcServiceAttribute) o [0];
					
					ParameterStyle = SoapParameterStyle.Wrapped;
					RoutingStyle = srs.RoutingStyle;
					Use = SoapBindingUse.Encoded;
					SoapBindingStyle = SoapBindingStyle.Rpc;
				} else {
					ParameterStyle = SoapParameterStyle.Wrapped;
					RoutingStyle = SoapServiceRoutingStyle.SoapAction;
					Use = SoapBindingUse.Literal;
					SoapBindingStyle = SoapBindingStyle.Document;
				}
			}
			
			if (ParameterStyle == SoapParameterStyle.Default) ParameterStyle = SoapParameterStyle.Wrapped;
			if (Use == SoapBindingUse.Default) Use = SoapBindingUse.Literal;

			SoapExtensions = SoapExtension.GetTypeExtensions (Type);
		}

		public override XmlReflectionImporter XmlImporter 
		{
			get { return xmlImporter; }
		}

		public override SoapReflectionImporter SoapImporter 
		{
			get { return soapImporter; }
		}
		
		public override string ProtocolName
		{
			get { return "Soap"; }
		}
		
		protected override MethodStubInfo CreateMethodStubInfo (TypeStubInfo parent, LogicalMethodInfo lmi, bool isClientProxy)
		{
			SoapMethodStubInfo res = null;
			object [] ats = lmi.GetCustomAttributes (typeof (SoapDocumentMethodAttribute));
			if (ats.Length == 0) ats = lmi.GetCustomAttributes (typeof (SoapRpcMethodAttribute));

			if (ats.Length == 0 && isClientProxy)
				return null;
			else if (ats.Length == 0)
				res = new SoapMethodStubInfo (parent, lmi, null, xmlImporter, soapImporter);
			else
				res = new SoapMethodStubInfo (parent, lmi, ats[0], xmlImporter, soapImporter);
				
			if (faultSerializerId == -1)
			{
				XmlReflectionImporter ri = new XmlReflectionImporter ();
				XmlTypeMapping tm = ri.ImportTypeMapping (typeof(Fault));
				faultSerializerId = RegisterSerializer (tm);
			}
			methods_byaction [res.Action] = res;
			return res;
		}
		
		public XmlSerializer GetFaultSerializer ()
		{
			return GetSerializer (faultSerializerId);
		}
		
		internal void RegisterHeaderType (Type type, string serviceNamespace, SoapBindingUse use)
		{
			Hashtable serializers = header_serializers [(int)use];
			if (serializers == null) {
				serializers = new Hashtable ();
				header_serializers [(int)use] = serializers;
				header_serializers_byname [(int)use] = new Hashtable ();
			}
			
			if (serializers.ContainsKey (type)) 
				return;

			XmlTypeMapping tm;
			if (use == SoapBindingUse.Literal) {
				XmlReflectionImporter ri = new XmlReflectionImporter ();
				
				// MS.NET reflects header classes in a weird way. The root element
				// name is the CLR class name unless it is specified in an XmlRootAttribute.
				// The usual is to use the xml type name by default, but not in this case.
				
				XmlRootAttribute root;
				XmlAttributes ats = new XmlAttributes (type);
				if (ats.XmlRoot != null) root = ats.XmlRoot;
				else root = new XmlRootAttribute (type.Name);
				
				if (root.Namespace == null) root.Namespace = LogicalType.GetWebServiceLiteralNamespace (serviceNamespace);
				if (root.ElementName == null) root.ElementName = type.Name;
				
				tm = ri.ImportTypeMapping (type, root);
			}
			else {
				SoapReflectionImporter ri = new SoapReflectionImporter ();
				tm = ri.ImportTypeMapping (type, LogicalType.GetWebServiceEncodedNamespace (serviceNamespace));
			}
			
			int sid = RegisterSerializer (tm);

			serializers [type] = sid;
			header_serializers_byname [(int)use] [new XmlQualifiedName (tm.ElementName, tm.Namespace)] = sid;
		}

		internal XmlSerializer GetHeaderSerializer (Type type, SoapBindingUse use)
		{
			Hashtable table = header_serializers [(int)use];
			if (table == null) return null;
				
			return GetSerializer ((int) table [type]);
		}
	
		internal XmlSerializer GetHeaderSerializer (XmlQualifiedName qname, SoapBindingUse use)
		{
			Hashtable table = header_serializers_byname [(int)use];
			if (table == null) return null;
				
			return GetSerializer ((int) table [qname]);
		}		
		
		public SoapMethodStubInfo GetMethodForSoapAction (string name)
		{
			return (SoapMethodStubInfo) methods_byaction [name.Trim ('"',' ')];
		}
	}
}
