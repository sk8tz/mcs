// 
// System.Web.Services.Protocols.DiscoveryClientProtocol.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.Collections;
using System.IO;
using System.Web.Services.Protocols;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Net;
using System.Text.RegularExpressions;

namespace System.Web.Services.Discovery {
	public class DiscoveryClientProtocol : HttpWebClientProtocol {

		#region Fields

		private IList additionalInformation;
		private DiscoveryClientDocumentCollection documents = new DiscoveryClientDocumentCollection();
		private DiscoveryExceptionDictionary errors = new DiscoveryExceptionDictionary();
		private DiscoveryClientReferenceCollection references = new DiscoveryClientReferenceCollection();

		#endregion // Fields

		#region Constructors

		public DiscoveryClientProtocol () 
		{
		}
		
		#endregion // Constructors

		#region Properties

		public IList AdditionalInformation {
			get { return additionalInformation; }
		}
		
		public DiscoveryClientDocumentCollection Documents {
			get { return documents; }
		}
		
		public DiscoveryExceptionDictionary Errors {
			get { return errors; }
		}

		public DiscoveryClientReferenceCollection References {
			get { return references; }
		}		
		
		#endregion // Properties

		#region Methods

		public DiscoveryDocument Discover (string url)
		{
			Stream stream = Download (ref url);
			XmlTextReader reader = new XmlTextReader (stream);
			if (!DiscoveryDocument.CanRead (reader)) 
				throw new InvalidOperationException ("The url '" + url + "' does not point to a valid discovery document");
				
			DiscoveryDocument doc = DiscoveryDocument.Read (reader);
			reader.Close ();
			documents.Add (url, doc);
			
			foreach (DiscoveryReference re in doc.References)
			{
				re.ClientProtocol = this;
				references.Add (re);
			}
				
			return doc;
		}

		public DiscoveryDocument DiscoverAny (string url)
		{
			string contentType = null;
			Stream stream = Download (ref url, ref contentType);

			if (contentType.IndexOf ("text/html") != -1)
			{
				// Look for an alternate url
				
				StreamReader sr = new StreamReader (stream);
				string str = sr.ReadToEnd ();
				
				string rex = "link\\s*rel\\s*=\\s*[\"']?alternate[\"']?\\s*";
				rex += "type\\s*=\\s*[\"']?text/xml[\"']?\\s*href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|'(?<1>[^']*)'|(?<1>\\S+))";
				Regex rob = new Regex (rex, RegexOptions.IgnoreCase);
				Match m = rob.Match (str);
				if (!m.Success) 
					throw new InvalidOperationException ("The HTML document does not contain Web service discovery information");
				
				if (url.StartsWith ("/"))
				{
					Uri uri = new Uri (url);
					url = uri.GetLeftPart (UriPartial.Authority) + m.Groups[1];
				}
				else
				{
					int i = url.LastIndexOf ('/');
					if (i == -1)
						throw new InvalidOperationException ("The HTML document does not contain Web service discovery information");
					url = url.Substring (0,i+1) + m.Groups[1];
				}
				stream = Download (ref url);
			}
			
			XmlTextReader reader = new XmlTextReader (stream);
			reader.MoveToContent ();
			DiscoveryDocument doc;
			DiscoveryReference refe = null;
			
			if (DiscoveryDocument.CanRead (reader))
			{
				doc = DiscoveryDocument.Read (reader);
				documents.Add (url, doc);
				refe = new DiscoveryDocumentReference ();
				foreach (DiscoveryReference re in doc.References)
				{
					re.ClientProtocol = this;
					references.Add (re.Url, re);
				}
			}
			else if (ServiceDescription.CanRead (reader))
			{
				ServiceDescription wsdl = ServiceDescription.Read (reader);
				documents.Add (url, wsdl);
				doc = new DiscoveryDocument ();
				refe = new ContractReference ();
				doc.References.Add (refe);
				((ContractReference)refe).ResolveInternal (this, wsdl);
			}
			else
			{
				XmlSchema schema = XmlSchema.Read (reader, null);
				documents.Add (url, schema);
				doc = new DiscoveryDocument ();
				refe = new SchemaReference ();
				doc.References.Add (refe);
			}
			
			refe.ClientProtocol = this;
			refe.Url = url;
			references.Add (url, refe);
				
			reader.Close ();
			return doc;
		}
		
		public Stream Download (ref string url)
		{
			string contentType = null;
			return Download (ref url, ref contentType);
		}
		
		public Stream Download (ref string url, ref string contentType)
		{
			if (url.StartsWith ("http://") || url.StartsWith ("https://"))
			{
				WebRequest request = GetWebRequest (new Uri(url));
				WebResponse resp = request.GetResponse ();
				contentType = resp.ContentType;
				return resp.GetResponseStream ();
			}
			else
			{
				string ext = Path.GetExtension (url).ToLower();
				if (ext == ".wsdl" || ext == ".xsd")
				{
					contentType = "text/xml";
					return new FileStream (url, FileMode.Open, FileAccess.Read);
				}
				else
					throw new InvalidOperationException ("Unrecognized file type '" + url + "'. Extension must be one of .wsdl or .xsd");
			}
		}
		
		public DiscoveryClientResultCollection ReadAll (string topLevelFilename)
		{
			StreamReader sr = new StreamReader (topLevelFilename);
			XmlSerializer ser = new XmlSerializer (typeof (DiscoveryClientResultsFile));
			DiscoveryClientResultsFile resfile = (DiscoveryClientResultsFile) ser.Deserialize (sr);
			sr.Close ();
			
			foreach (DiscoveryClientResult dcr in resfile.Results)
			{
				Type type = Type.GetType (dcr.ReferenceTypeName);
				DiscoveryReference dr = (DiscoveryReference) Activator.CreateInstance (type);
				dr.Url = dcr.Url;
				FileStream fs = new FileStream (dcr.Filename, FileMode.Open, FileAccess.Read);
				Documents.Add (dr.Url, dr.ReadDocument (fs));
				fs.Close ();
				References.Add (dr.Url, dr);
			}
			return resfile.Results;
		}

		public void ResolveAll ()
		{
			ArrayList list = new ArrayList (References.Values);
			foreach (DiscoveryReference re in list)
			{
				if (re is DiscoveryDocumentReference)
					((DiscoveryDocumentReference)re).ResolveAll ();
				else
					re.Resolve ();
			}
		}
		
		public void ResolveOneLevel ()
		{
			ArrayList list = new ArrayList (References.Values);
			foreach (DiscoveryReference re in list)
				re.Resolve ();
		}
		
		public DiscoveryClientResultCollection WriteAll (string directory, string topLevelFilename)
		{
			DiscoveryClientResultsFile resfile = new DiscoveryClientResultsFile();
			
			foreach (DiscoveryReference re in References.Values)
			{
				object doc = Documents [re.Url];
				if (doc == null) continue;
				
				string fileName = FindValidName (resfile, re.DefaultFilename);
				resfile.Results.Add (new DiscoveryClientResult (re.GetType(), re.Url, fileName));
				
				string filepath = Path.Combine (directory, fileName);
				FileStream fs = new FileStream (filepath, FileMode.Create, FileAccess.Write);
				re.WriteDocument (doc, fs);
				fs.Close ();
			}
			
			StreamWriter sw = new StreamWriter (Path.Combine (directory, topLevelFilename));
			XmlSerializer ser = new XmlSerializer (typeof (DiscoveryClientResultsFile));
			ser.Serialize (sw, resfile);
			sw.Close ();
			return resfile.Results;
		}
		
		string FindValidName (DiscoveryClientResultsFile resfile, string baseName)
		{
			string name = baseName;
			int id = 0;
			bool found;
			do
			{
				found = false;
				foreach (DiscoveryClientResult res in resfile.Results)
				{
					if (name == res.Filename) {
						found = true; break;
					}
				}
				if (found)
					name = Path.GetFileNameWithoutExtension (baseName) + (++id) + Path.GetExtension (baseName);
			}
			while (found);
			
			return name;
		}
		
		#endregion // Methods
		
		#region Classes
		
		public sealed class DiscoveryClientResultsFile {
			
			#region Fields
			
			private DiscoveryClientResultCollection results;

			#endregion // Fields

			#region Contructors
			
			public DiscoveryClientResultsFile () 
			{
				results = new DiscoveryClientResultCollection ();
			}
		
			#endregion // Constructors
			
			#region Properties
		
			public DiscoveryClientResultCollection Results {				
				get { return results; }
			}
			
			#endregion // Properties
		}
		#endregion // Classes
	}
}
