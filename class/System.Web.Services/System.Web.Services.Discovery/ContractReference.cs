// 
// System.Web.Services.Discovery.ContractReference.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.IO;
using System.Web.Services.Description;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace System.Web.Services.Discovery {

	[XmlRootAttribute("contractRef", Namespace="http://schemas.xmlsoap.org/disco/scl/", IsNullable=true)]
	public class ContractReference : DiscoveryReference {

		#region Fields
		
		public const string Namespace = "http://schemas.xmlsoap.org/disco/scl/";

		private ServiceDescription contract;
		private string defaultFilename;
		private string docRef;
		private string href;
		
		#endregion // Fields
		
		#region Constructors

		public ContractReference () 
		{
		}
		
		public ContractReference (string href) : this() 
		{
			this.href = href;
		}
		
		public ContractReference (string href, string docRef)
		{
			this.href = href;
			this.docRef = docRef;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public ServiceDescription Contract {
			get {
				if (ClientProtocol == null) 
					throw new InvalidOperationException ("The ClientProtocol property is a null reference");
				
				ServiceDescription desc = ClientProtocol.Documents [Url] as ServiceDescription;
				if (desc == null)
					throw new Exception ("The Documents property of ClientProtocol does not contain a WSDL document with the url " + Url);
					
				return desc; 
			}
		}

		[XmlIgnore]
		public override string DefaultFilename {
			get { return FilenameFromUrl (Url) + ".wsdl"; }
		}
		
		[XmlAttribute("docRef")]
		public string DocRef {
			get { return docRef; }
			set { docRef = value; }
		}
		
		[XmlAttribute("ref")]
		public string Ref {
			get { return href; }
			set { href = value; }
		}
		
		[XmlIgnore]
		public override string Url {
			get { return href;}			
			set { href = value; }
		}
		
		#endregion // Properties

		#region Methods

		public override object ReadDocument (Stream stream)
		{
			return ServiceDescription.Read (stream);
		}
                
		protected internal override void Resolve (string contentType, Stream stream) 
		{
			ServiceDescription wsdl = ServiceDescription.Read (stream);
			
			if (!ClientProtocol.References.Contains (Url))
				ClientProtocol.References.Add (this);

			ClientProtocol.Documents.Add (Url, wsdl);
			ResolveInternal (ClientProtocol, wsdl);
		}
		
		internal void ResolveInternal (DiscoveryClientProtocol prot, ServiceDescription wsdl) 
		{
			if (wsdl.Imports == null) return;
			
			foreach (Import import in wsdl.Imports)
			{
				// Make relative uris to absoulte
				
				Uri uri = new Uri (Url);
				uri = new Uri (uri, import.Location);
				string url = uri.ToString ();

				if (prot.Documents.Contains (url)) 	// Already resolved
					continue;

				try
				{
					string contentType = null;
					Stream stream = prot.Download (ref url, ref contentType);
					XmlTextReader reader = new XmlTextReader (stream);
					reader.MoveToContent ();
					
					DiscoveryReference refe;
					if (ServiceDescription.CanRead (reader))
					{
						ServiceDescription refWsdl = ServiceDescription.Read (reader);
						refe = new ContractReference ();
						refe.ClientProtocol = prot;
						refe.Url = url;
						((ContractReference)refe).ResolveInternal (prot, refWsdl);
						prot.Documents.Add (url, refWsdl);
					}
					else
					{
						XmlSchema schema = XmlSchema.Read (reader, null);
						refe = new SchemaReference ();
						refe.ClientProtocol = prot;
						refe.Url = url;
						prot.Documents.Add (url, schema);
					}
					
					if (!prot.References.Contains (url))
						prot.References.Add (refe);
						
					reader.Close ();
				}
				catch (Exception ex)
				{
					ReportError (url, ex);
				}
			}
		}
                
        public override void WriteDocument (object document, Stream stream) 
		{
			((ServiceDescription)document).Write (stream);
		}

		#endregion // Methods
	}
}
