using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace Commons.Xml.Nvdl
{
	public class NvdlValidationProvider
	{
		public virtual NvdlValidatorGenerator CreateGenerator (NvdlValidate validate, string schemaType, NvdlConfig config)
		{
			XmlReader schema = null;
			if (schemaType != "text/xml")
				return null;

			string schemaUri = validate.SchemaUri;
			XmlElement schemaBody = validate.SchemaBody;

			if (schemaUri != null) {
				if (schemaBody != null)
					throw new NvdlCompileException ("Both 'schema' attribute and 'schema' element are specified in a 'validate' element.", validate);
				schema = GetSchemaXmlStream (schemaUri, config, validate);
			}
			else if (validate.SchemaBody != null) {
				XmlReader r = new XmlNodeReader (schemaBody);
				r.MoveToContent ();
				r.Read (); // Skip "schema" element
				r.MoveToContent ();
				if (r.NodeType == XmlNodeType.Element)
					schema = r;
				else
					schema = GetSchemaXmlStream (r.ReadString (), config, validate);
			}

			if (schema == null)
				return null;

			return CreateGenerator (schema, config);
		}

		public virtual NvdlValidatorGenerator CreateGenerator (XmlReader schema, NvdlConfig config)
		{
			return null;
		}

		public string GetSchemaUri (NvdlValidate validate)
		{
			if (validate.SchemaUri != null)
				return validate.SchemaUri;
			if (validate.SchemaBody == null)
				return null;
			for (XmlNode n = validate.SchemaBody.FirstChild; n != null; n = n.NextSibling)
				if (n.NodeType == XmlNodeType.Element)
					return null; // not a URI
			return validate.SchemaBody.InnerText;
		}

		private static XmlReader GetSchemaXmlStream (string schemaUri, NvdlConfig config, NvdlValidate validate)
		{
			XmlResolver r = config.XmlResolverInternal;
			if (r == null)
				return null;
			Uri uri = r.ResolveUri (null, validate.SchemaUri);
			Stream stream = (Stream) r.GetEntity (
				uri, null, typeof (Stream));
			if (stream == null)
				return null;
			XmlTextReader xtr = new XmlTextReader (uri != null ? uri.ToString () : String.Empty, stream);
			xtr.XmlResolver = r;
			xtr.MoveToContent ();
			return xtr;
		}
	}

	public abstract class NvdlValidatorGenerator
	{
		// creates individual validator with schema
		// (which should be provided in derived constructor).
		public abstract XmlReader CreateValidator (XmlReader reader, XmlResolver resolver);

		public abstract bool AddOption (string name, string arg);
	}
}
