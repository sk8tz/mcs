//
// System.Web.Configuration.WebControlsSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml;

namespace System.Web.Configuration
{
	class WebControlsConfig
	{
		static WebControlsConfig instance;
		string scriptsVDir;
		string configFilePath;
		
		public WebControlsConfig (WebControlsConfig parent, object context)
		{
			configFilePath = context as string;
			if (parent == null)
				return;
			
			scriptsVDir = parent.scriptsVDir;
			if (scriptsVDir != null)
				configFilePath = parent.configFilePath;
		}
		
		public void SetClientScriptsLocation (string location, out string error)
		{
			error = null;
			if (location == null || location.Length == 0) {
				error = "empty or null value for clientScriptsLocation";
				return;
			}

			if (location [0] != '/')
				location = "/" + location;

			string [] splitted = location.Split ('/');
			int end = splitted.Length;
			for (int i = 0; i < end; i++)
				splitted [i] = HttpUtility.UrlEncode (splitted [i]);

			scriptsVDir = String.Join ("/", splitted);
		}

		public string ScriptsPhysicalDirectory {
			get { return Path.Combine (Path.GetDirectoryName (configFilePath), "web_scripts"); }
		}

		public string ScriptsVirtualDirectory {
			get { return scriptsVDir; }
			set { scriptsVDir = value; }
		}

		static public WebControlsConfig Instance {
			get {
				//TODO: use HttpContext to get the configuration
				if (instance != null)
					return instance;

				lock (typeof (WebControlsConfig)) {
					if (instance != null)
						return instance;

					instance = (WebControlsConfig) ConfigurationSettings.GetConfig ("system.web/webControls");
				}

				return instance;
			}
		}
	}
	
	class WebControlsSectionHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			WebControlsConfig config = new WebControlsConfig (parent as WebControlsConfig, context);

			if (section.Attributes == null && section.Attributes.Count == 0)
				ThrowException ("Lack of clientScriptsLocation attribute", section);

			string clientLocation = AttValue ("clientScriptsLocation", section, false);
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			string error;
			config.SetClientScriptsLocation (clientLocation, out error);
			if (error != null)
				HandlersUtil.ThrowException (error, section);

			return config;
		}

		// To save some typing...
		static string AttValue (string name, XmlNode node, bool optional)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, optional);
		}

		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
		//
	}
}

