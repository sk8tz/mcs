// 
// System.Web.Services.Protocols.XmlReturnWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace System.Web.Services.Protocols {
	internal class XmlReturnWriter : MimeReturnWriter {

		XmlSerializer serializer;
		
		#region Methods

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			string namesp = WebServiceHelper.GetServiceNamespace (methodInfo.DeclaringType);
			return new XmlSerializer (methodInfo.ReturnType, namesp);
		}
		
		public override object[] GetInitializers (LogicalMethodInfo[] methodInfos)
		{
			XmlReflectionImporter importer = new XmlReflectionImporter ();
			XmlMapping[] sers = new XmlMapping [methodInfos.Length];
			for (int n=0; n<sers.Length; n++)
			{
				LogicalMethodInfo metinfo = methodInfos[n];
				if (metinfo.IsVoid) 
					sers[n] = null;
				else
				{
					string namesp = WebServiceHelper.GetServiceNamespace (metinfo.DeclaringType);
					sers[n] = importer.ImportTypeMapping (methodInfos[n].ReturnType, namesp);
				}
			}
			return XmlSerializer.FromMappings (sers);
		}

		public override void Initialize (object initializer) 
		{
			serializer = (XmlSerializer) initializer;
		}

		public override void Write (HttpResponse response, Stream outputStream, object returnValue)
		{
			if (serializer != null)
			{
				response.ContentType = "text/xml; charset=utf-8";
				serializer.Serialize (outputStream, returnValue);
			}
			outputStream.Close ();
		}

		#endregion // Methods
	}
}
