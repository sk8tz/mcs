//
// System.Runtime.Remoting.RemotingConfiguration.cs
//
// Author: Jaime Anguiano Olarra (jaime@gnome.org)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Jaime Anguiano Olarra
//

using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Lifetime;
using Mono.Xml;

namespace System.Runtime.Remoting
{	
	public class RemotingConfiguration
	{
		//
		// Private constructor: nobody instantiates this.
		//
		private RemotingConfiguration ()
		{
		}
		
		static string applicationID = null;
		static string applicationName = null;
		static string configFile = "";
		static MiniParser parser = null; 
		static string processGuid = null;
		static bool defaultConfigRead = false;
		static string _errorMode;

		static Hashtable wellKnownClientEntries = new Hashtable();
		static Hashtable activatedClientEntries = new Hashtable();
		static Hashtable wellKnownServiceEntries = new Hashtable();
		static Hashtable activatedServiceEntries = new Hashtable();
		
		static Hashtable channelTemplates = new Hashtable ();
		static Hashtable clientProviderTemplates = new Hashtable ();
		static Hashtable serverProviderTemplates = new Hashtable ();
		
		// public properties
		// At this time the ID will be the application name 
		public static string ApplicationId 
		{
			get 
			{ 
				applicationID = AppDomain.CurrentDomain.SetupInformation.ApplicationName; 
				return applicationID;
			}
		}
		
		public static string ApplicationName 
		{
			get { 
				try {
					applicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName; 
				}
				catch (Exception e) {
					throw e;
				}
				// We return null if the application name has not been set.
				return null;
			}
			set { applicationName = value; }
		}
		
		public static string ProcessId 
		{
			get {
				if (processGuid == null)
					processGuid = AppDomain.GetProcessGuid ();

				return processGuid;
			}
		}


		// public methods
		
		public static void Configure (string filename) 
		{
			if (!defaultConfigRead)
			{
				ReadConfigFile (Environment.GetMachineConfigPath ());
				defaultConfigRead = true;
			}
			
			if (filename != null)
				ReadConfigFile (filename);
		}

		private static void ReadConfigFile (string filename)
		{
			try
			{
				MiniParser parser = new MiniParser ();
				RReader rreader = new RReader (filename);
				ConfigHandler handler = new ConfigHandler ();
				parser.Parse (rreader, handler);
			}
			catch (Exception ex)
			{
				throw new RemotingException ("Configuration file '" + filename + "' could not be loaded: " + ex.Message);
			}
		}
	
		public static ActivatedClientTypeEntry[] GetRegisteredActivatedClientTypes () 
		{
			ActivatedClientTypeEntry[] entries = new ActivatedClientTypeEntry[activatedClientEntries.Count];
			activatedClientEntries.Values.CopyTo (entries,0);
			return entries;
		}

		public static ActivatedServiceTypeEntry[] GetRegisteredActivatedServiceTypes () 
		{
			ActivatedServiceTypeEntry[] entries = new ActivatedServiceTypeEntry[activatedServiceEntries.Count];
			activatedServiceEntries.Values.CopyTo (entries,0);
			return entries;
		}

		public static WellKnownClientTypeEntry[] GetRegisteredWellKnownClientTypes () 
		{
			WellKnownClientTypeEntry[] entries = new WellKnownClientTypeEntry[wellKnownClientEntries.Count];
			wellKnownClientEntries.Values.CopyTo (entries,0);
			return entries;
		}

		public static WellKnownServiceTypeEntry[] GetRegisteredWellKnownServiceTypes () 
		{
			WellKnownServiceTypeEntry[] entries = new WellKnownServiceTypeEntry[wellKnownServiceEntries.Count];
			wellKnownServiceEntries.Values.CopyTo (entries,0);
			return entries;
		}

		public static bool IsActivationAllowed (Type serverType) 
		{
			return activatedServiceEntries.ContainsKey (serverType);
		}

		public static ActivatedClientTypeEntry IsRemotelyActivatedClientType (Type serviceType) 
		{
			return activatedClientEntries [serviceType] as ActivatedClientTypeEntry;
		}

		public static ActivatedClientTypeEntry IsRemotelyActivatedClientType (string typeName, string assemblyName) 
		{
			return IsRemotelyActivatedClientType (Assembly.Load(assemblyName).GetType (typeName));
		}

		public static WellKnownClientTypeEntry IsWellKnownClientType (Type serviceType) 
		{
			return wellKnownClientEntries [serviceType] as WellKnownClientTypeEntry;
		}

		public static WellKnownClientTypeEntry IsWellKnownClientType (string typeName, string assemblyName) 
		{
			return IsWellKnownClientType (Assembly.Load(assemblyName).GetType (typeName));
		}

		public static void RegisterActivatedClientType (ActivatedClientTypeEntry entry) 
		{
			if (wellKnownClientEntries.ContainsKey (entry.ObjectType) || activatedClientEntries.ContainsKey (entry.ObjectType))
				throw new RemotingException ("Attempt to redirect activation of type '" + entry.ObjectType.FullName + "' which is already redirected.");

			activatedClientEntries[entry.ObjectType] = entry;
			ActivationServices.EnableProxyActivation (entry.ObjectType, true);
		}

		public static void RegisterActivatedClientType (Type type, string appUrl) 
		{
			if (type == null) throw new ArgumentNullException ("type");
			if (appUrl == null) throw new ArgumentNullException ("appUrl");

			RegisterActivatedClientType (new ActivatedClientTypeEntry (type, appUrl));
		}

		public static void RegisterActivatedServiceType (ActivatedServiceTypeEntry entry) 
		{
			activatedServiceEntries.Add (entry.ObjectType, entry);
		}

		public static void RegisterActivatedServiceType (Type type) 
		{
			RegisterActivatedServiceType (new ActivatedServiceTypeEntry (type));
		}

		public static void RegisterWellKnownClientType (Type type, string objectUrl) 
		{
			if (type == null) throw new ArgumentNullException ("type");
			if (objectUrl == null) throw new ArgumentNullException ("objectUrl");

			RegisterWellKnownClientType (new WellKnownClientTypeEntry (type, objectUrl));
		}

		public static void RegisterWellKnownClientType (WellKnownClientTypeEntry entry) 
		{
			if (wellKnownClientEntries.ContainsKey (entry.ObjectType) || activatedClientEntries.ContainsKey (entry.ObjectType))
				throw new RemotingException ("Attempt to redirect activation of type '" + entry.ObjectType.FullName + "' which is already redirected.");

			wellKnownClientEntries[entry.ObjectType] = entry;
			ActivationServices.EnableProxyActivation (entry.ObjectType, true);
		}

		public static void RegisterWellKnownServiceType (Type type, string objectUrl, WellKnownObjectMode mode) 
		{
			RegisterWellKnownServiceType (new WellKnownServiceTypeEntry (type, objectUrl, mode));
		}

		public static void RegisterWellKnownServiceType (WellKnownServiceTypeEntry entry) 
		{
			wellKnownServiceEntries [entry.ObjectUri] = entry;
			RemotingServices.CreateWellKnownServerIdentity (entry.ObjectType, entry.ObjectUri, entry.Mode);
		}

		internal static void RegisterChannelTemplate (ChannelData channel)
		{
			channelTemplates [channel.Id] = channel;
		}
		
		internal static void RegisterClientProviderTemplate (ProviderData prov)
		{
			clientProviderTemplates [prov.Id] = prov;
		}
		
		internal static void RegisterServerProviderTemplate (ProviderData prov)
		{
			serverProviderTemplates [prov.Id] = prov;
		}
		
		internal static void RegisterChannels (ArrayList channels)
		{
			foreach (ChannelData channel in channels)
			{
				if (channel.Ref != null)
				{
					ChannelData template = (ChannelData) channelTemplates [channel.Ref];
					if (template == null) throw new RemotingException ("Channel template '" + channel.Ref + "' not found");
					channel.CopyFrom (template);
				}
				
				foreach (ProviderData prov in channel.ServerProviders)
				{
					if (prov.Ref != null)
					{
						ProviderData template = (ProviderData) serverProviderTemplates [prov.Ref];
						if (template == null) throw new RemotingException ("Provider template '" + prov.Ref + "' not found");
						prov.CopyFrom (template);
					}
				}
				
				foreach (ProviderData prov in channel.ClientProviders)
				{
					if (prov.Ref != null)
					{
						ProviderData template = (ProviderData) clientProviderTemplates [prov.Ref];
						if (template == null) throw new RemotingException ("Provider template '" + prov.Ref + "' not found");
						prov.CopyFrom (template);
					}
				}
				
				ChannelServices.RegisterChannelConfig (channel);
			}
		}
		
		internal static void RegisterTypes (ArrayList types)
		{
			foreach (TypeEntry type in types)
			{
				if (type is ActivatedClientTypeEntry)
					RegisterActivatedClientType ((ActivatedClientTypeEntry)type);
				else if (type is ActivatedServiceTypeEntry)
					RegisterActivatedServiceType ((ActivatedServiceTypeEntry)type);
				else if (type is WellKnownClientTypeEntry)
					RegisterWellKnownClientType ((WellKnownClientTypeEntry)type);
				else if (type is WellKnownServiceTypeEntry)
					RegisterWellKnownServiceType ((WellKnownServiceTypeEntry)type);
			}
		}
		
#if NET_1_1
		public static bool CustomErrorsEnabled (bool isLocalRequest)
		{
			if (_errorMode == "off") return false;
			if (_errorMode == "on") return true;
			return !isLocalRequest;
		}
#endif

		internal static void SetCustomErrorsMode (string mode)
		{
			if (mode != "on" && mode != "off" && mode != "remoteOnly")
				throw new RemotingException ("Invalid custom error mode: " + mode);
				
			_errorMode = mode;
		}
	}

	/***************************************************************
	 * Internal classes used by RemotingConfiguration.Configure () *
	 ***************************************************************/
	 
	internal class RReader : MiniParser.IReader {
		private string xml; // custom remoting config file
		private int pos;

		public RReader (string filename)
		{
			try {
				StreamReader sr = new StreamReader (filename);
				xml = sr.ReadToEnd ();
				sr.Close ();
			}
			catch {
				xml = null;
			}
		}

		public int Read () {
			try {
				return (int) xml[pos++];
			}
			catch {
				return -1;
			}
		}
	}
	
	internal class ConfigHandler : MiniParser.IHandler 
	{
		ArrayList typeEntries = new ArrayList ();
		ArrayList channelInstances = new ArrayList ();
		
		ChannelData currentChannel = null;
		Stack currentProviderData = null;
		
		string currentClientUrl = null;
		string appName;
		
		string currentXmlPath = "";
		
		public ConfigHandler ()
		{
		}
		
		void ValidatePath (string element, params string[] paths)
		{
			foreach (string path in paths)
				if (CheckPath (path)) return;
				
			throw new RemotingException ("Element " + element + " not allowed in this context");
		}
		
		bool CheckPath (string path)
		{
			if (path.StartsWith ("/"))
				return path == currentXmlPath;
			else
				return currentXmlPath.EndsWith (path);
		}
		
		public void OnStartParsing (MiniParser parser) {}
		
		public void OnStartElement (string name, MiniParser.IAttrList attrs)
		{
			try
			{
				if (currentXmlPath.StartsWith ("/configuration/system.runtime.remoting"))
					ParseElement (name, attrs);
					
				currentXmlPath += "/" + name;
			}
			catch (Exception ex)
			{
				throw new RemotingException ("Error in element " + name + ": " + ex.Message);
			}
		}
		
		public void ParseElement (string name, MiniParser.IAttrList attrs)
		{
			if (currentProviderData != null)
			{
				ReadCustomProviderData (name, attrs);
				return;
			}
			
			switch (name) 
			{
				case "application":
					ValidatePath (name, "system.runtime.remoting");
					if (attrs.Names.Length > 0)
						appName = attrs.Values[0];
					break;
					
				case "lifetime":
					ValidatePath (name, "application");
					ReadLifetine (attrs);
					break;
					
				case "channels":
					ValidatePath (name, "system.runtime.remoting", "application");
					break;
					
				case "channel":
					ValidatePath (name, "channels");
					if (currentXmlPath.IndexOf ("application") != -1)
						ReadChannel (attrs, false);
					else
						ReadChannel (attrs, true);
					break;
					
				case "serverProviders":
					ValidatePath (name, "channelSinkProviders", "channel");
					break;
					
				case "clientProviders":
					ValidatePath (name, "channelSinkProviders", "channel");
					break;
					
				case "provider":
				case "formatter":
					ProviderData prov;
					
					if (CheckPath ("application/channels/channel/serverProviders") ||
						CheckPath ("channels/channel/serverProviders"))
					{
						prov = ReadProvider (name, attrs, false);
						currentChannel.ServerProviders.Add (prov);
					}
					else if (CheckPath ("application/channels/channel/clientProviders") ||
						CheckPath ("channels/channel/clientProviders"))
					{
						prov = ReadProvider (name, attrs, false);
						currentChannel.ClientProviders.Add (prov);
					}
					else if (CheckPath ("channelSinkProviders/serverProviders"))
					{
						prov = ReadProvider (name, attrs, true);
						RemotingConfiguration.RegisterServerProviderTemplate (prov);
					}
					else if (CheckPath ("channelSinkProviders/clientProviders"))
					{
						prov = ReadProvider (name, attrs, true);
						RemotingConfiguration.RegisterClientProviderTemplate (prov);
					}
					else 
						ValidatePath (name);
					break;
					
				case "client":
					ValidatePath (name, "application");
					currentClientUrl = attrs.GetValue ("url");
					break;
					
				case "service":
					ValidatePath (name, "application");
					break;
					
				case "wellknown":
					ValidatePath (name, "client", "service");
					if (CheckPath ("client"))
						ReadClientWellKnown (attrs);
					else
						ReadServiceWellKnown (attrs);
					break;
					
				case "activated":
					ValidatePath (name, "client", "service");
					if (CheckPath ("client"))
						ReadClientActivated (attrs);
					else
						ReadServiceActivated (attrs);
					break;
					
				case "soapInterop":
					ValidatePath (name, "application");
					break;
					
				case "interopXmlType":
					ValidatePath (name, "soapInterop");
					ReadInteropXml (attrs, false);
					break;
					
				case "interopXmlElement":
					ValidatePath (name, "soapInterop");
					ReadInteropXml (attrs, false);
					break;
					
				case "preLoad":
					ValidatePath (name, "soapInterop");
					ReadPreload (attrs);
					break;
					
				case "debug":
					ValidatePath (name, "system.runtime.remoting");
					break;
					
				case "channelSinkProviders":
					ValidatePath (name, "system.runtime.remoting");
					break;
					
				case "customErrors":
					ValidatePath (name, "system.runtime.remoting");
					RemotingConfiguration.SetCustomErrorsMode (attrs.GetValue ("mode"));
					break;
					
				default:
					throw new RemotingException ("Element '" + name + "' is not valid in system.remoting.configuration section");
			}
		}
		
		public void OnEndElement (string name)
		{
			if (currentProviderData != null)
			{
				currentProviderData.Pop ();
				if (currentProviderData.Count > 0) return;
				currentProviderData = null;
			}
			
			currentXmlPath = currentXmlPath.Substring (0, currentXmlPath.Length - name.Length - 1);
		}
		
		void ReadCustomProviderData (string name, MiniParser.IAttrList attrs)
		{
			SinkProviderData parent = (SinkProviderData) currentProviderData.Peek ();
			
			SinkProviderData data = new SinkProviderData (name);
			for (int i=0; i < attrs.Names.Length; ++i) 
				data.Properties [attrs.Names[i]] = attrs.GetValue (i);
				
			parent.Children.Add (data);
			currentProviderData.Push (data);
		}

		void ReadLifetine (MiniParser.IAttrList attrs)
		{
			for (int i=0; i < attrs.Names.Length; ++i) {
				switch (attrs.Names[i]) {
				case "leaseTime":
					LifetimeServices.LeaseTime = ParseTime (attrs.GetValue(i));
					break;
				case "sponsorShipTimeOut":
					LifetimeServices.SponsorshipTimeout = ParseTime (attrs.GetValue(i));
					break;
				case "renewOnCallTime":
					LifetimeServices.RenewOnCallTime = ParseTime (attrs.GetValue(i));
					break;
				case "leaseManagerPollTime":
					LifetimeServices.LeaseManagerPollTime = ParseTime (attrs.GetValue(i));
					break;
				default:
					throw new RemotingException ("Invalid attribute: " + attrs.Names[i]);
				}
			}
		}
		
		TimeSpan ParseTime (string s)
		{
			if (s == "" || s == null) throw new RemotingException ("Invalid time value");
			
			int i = s.IndexOfAny (new char[] { 'D','H','M','S' });
			
			string unit;
			if (i == -1) 
				unit = "S";
			else { 
				unit = s.Substring (i);
				s = s.Substring (0,i);
			}
			double val;
			
			try {
				val = double.Parse (s);
			}
			catch {
				throw new RemotingException ("Invalid time value: " + s);
			}
			
			if (unit == "D") return TimeSpan.FromDays (val);
			if (unit == "H") return TimeSpan.FromHours (val);
			if (unit == "M") return TimeSpan.FromMinutes (val);
			if (unit == "S") return TimeSpan.FromSeconds (val);
			if (unit == "MS") return TimeSpan.FromMilliseconds (val);
			throw new RemotingException ("Invalid time unit: " + unit);
		}
		
		void ReadChannel (MiniParser.IAttrList attrs, bool isTemplate)
		{
			ChannelData channel = new ChannelData ();
			
			for (int i=0; i < attrs.Names.Length; ++i) 
			{
				string at = attrs.Names[i];
				string val = attrs.Values[i];
				
				if (at == "ref" && !isTemplate)
					channel.Ref = val;
				else if (at == "delayLoadAsClientChannel")
					channel.DelayLoadAsClientChannel = val;
				else if (at == "id" && isTemplate)
					channel.Id = val;
				else if (at == "type")
					channel.Type = val;
				else
					channel.CustomProperties.Add (at, val);
			}
			
			if (isTemplate)
			{
				if (channel.Id == null) throw new RemotingException ("id attribute is required");
				if (channel.Type == null) throw new RemotingException ("id attribute is required");
				RemotingConfiguration.RegisterChannelTemplate (channel);
			}
			else
				channelInstances.Add (channel);
				
			currentChannel = channel;
		}
		
		ProviderData ReadProvider (string name, MiniParser.IAttrList attrs, bool isTemplate)
		{
			ProviderData prov = (name == "provider") ? new ProviderData () : new FormatterData ();
			SinkProviderData data = new SinkProviderData ("root");
			prov.CustomData = data.Children;
			
			currentProviderData = new Stack ();
			currentProviderData.Push (data);
			
			for (int i=0; i < attrs.Names.Length; ++i) 
			{
				string at = attrs.Names[i];
				string val = attrs.Values[i];
				
				if (at == "id" && isTemplate)
					prov.Id = val;
				if (at == "type")
					prov.Type = val;
				if (at == "ref" && !isTemplate)
					prov.Ref = val;
				else
					prov.CustomProperties.Add (at, val);
			}
			
			if (prov.Id == null && isTemplate) throw new RemotingException ("id attribute is required");
			return prov;
		}
		
		void ReadClientActivated (MiniParser.IAttrList attrs)
		{
			string type = GetNotNull (attrs, "type");
			string assm = ExtractAssembly (ref type);
			
			if (currentClientUrl == null || currentClientUrl == "") 
				throw new RemotingException ("url attribute is required in client element when it contains activated entries");

			typeEntries.Add (new ActivatedClientTypeEntry (type, assm, currentClientUrl));
		}
		
		void ReadServiceActivated (MiniParser.IAttrList attrs)
		{
			string type = GetNotNull (attrs, "type");
			string assm = ExtractAssembly (ref type);
			
			typeEntries.Add (new ActivatedServiceTypeEntry (type, assm));
		}
		
		void ReadClientWellKnown (MiniParser.IAttrList attrs)
		{
			string url = GetNotNull (attrs, "url");
			string type = GetNotNull (attrs, "type");
			string assm = ExtractAssembly (ref type);
			
			typeEntries.Add (new WellKnownClientTypeEntry (type, assm, url));
		}
		
		void ReadServiceWellKnown (MiniParser.IAttrList attrs)
		{
			string objectUri = GetNotNull (attrs, "objectUri");
			string smode = GetNotNull (attrs, "mode");
			string type = GetNotNull (attrs, "type");
			string assm = ExtractAssembly (ref type);
			
			WellKnownObjectMode mode;
			if (smode == "SingleCall") mode = WellKnownObjectMode.SingleCall;
			else if (smode == "Singleton") mode = WellKnownObjectMode.Singleton;
			else throw new RemotingException ("wellknown object mode '" + smode + "' is invalid");
			
			typeEntries.Add (new WellKnownServiceTypeEntry (type, assm, objectUri, mode));
		}
		
		void ReadInteropXml (MiniParser.IAttrList attrs, bool isElement)
		{
			Type t = Type.GetType (GetNotNull (attrs, "clr"));
			string[] xmlName = GetNotNull (attrs, "xml").Split (',');
			string localName = xmlName [0].Trim ();
			string ns = xmlName.Length > 0 ? xmlName[1].Trim() : null;
			
			if (isElement) SoapServices.RegisterInteropXmlElement (localName, ns, t);
			else SoapServices.RegisterInteropXmlType (localName, ns, t);
		}
		
		void ReadPreload (MiniParser.IAttrList attrs)
		{
			string type = attrs.GetValue ("type");
			string assm = attrs.GetValue ("assembly");
			
			if (type != null && assm != null)
				throw new RemotingException ("Type and assembly attributes cannot be specified together");
				
			if (type != null)
				SoapServices.PreLoad (Type.GetType (type));
			else if (assm != null)
				SoapServices.PreLoad (Assembly.Load (assm));
			else
				throw new RemotingException ("Either type or assembly attributes must be specified");
		}
					
		string GetNotNull (MiniParser.IAttrList attrs, string name)
		{
			string value = attrs.GetValue (name);
			if (value == null || value == "") 
				throw new RemotingException (name + " attribute is required");
			return value;
		}
		
		string ExtractAssembly (ref string type)
		{
			int i = type.IndexOf (',');
			if (i == -1) return "";
			
			string asm = type.Substring (i+1).Trim();
			type = type.Substring (0, i).Trim();
			return asm;
		}
		
		public void OnChars (string ch) {}
		
		public void OnEndParsing (MiniParser parser)
		{
			RemotingConfiguration.RegisterChannels (channelInstances);
			RemotingConfiguration.RegisterTypes (typeEntries);
		}
	}


		/*******************************************************************
         * Internal data structures used by ConfigHandler, to store             *
         * machine.config's remoting related data.                         *
         * If having them implemented this way, makes configuration too    *
         * slow, we can use string arrays.                                 *
         *******************************************************************/
		 
	internal class ChannelData {
		internal string Ref;
		internal string Type;
		internal string Id;
		internal string DelayLoadAsClientChannel;
		
		ArrayList _serverProviders = new ArrayList ();
		ArrayList _clientProviders = new ArrayList ();
		Hashtable _customProperties = new Hashtable ();
		
		internal ArrayList ServerProviders {
			get {
				if (_serverProviders == null) _serverProviders = new ArrayList ();
				return _serverProviders;
			}
		}
		
		public ArrayList ClientProviders {
			get {
				if (_clientProviders == null) _clientProviders = new ArrayList ();
				return _clientProviders;
			}
		}
		
		public Hashtable CustomProperties {
			get {
				if (_customProperties == null) _customProperties = new Hashtable ();
				return _customProperties;
			}
		}
		
		public void CopyFrom (ChannelData other)
		{
			if (Ref == null) Ref = other.Ref;
			if (Id == null) Id = other.Id;
			if (Type == null) Type = other.Type;
			if (DelayLoadAsClientChannel == null) DelayLoadAsClientChannel = other.DelayLoadAsClientChannel;

			if (other._customProperties != null)
			{
				foreach (DictionaryEntry entry in other._customProperties)
					if (!CustomProperties.ContainsKey (entry.Key))
						CustomProperties [entry.Key] = entry.Value;
			}
			
			if (_serverProviders == null && other._serverProviders != null)
			{
				foreach (ProviderData prov in other._serverProviders)
				{
					ProviderData np = new ProviderData();
					np.CopyFrom (prov);
					ServerProviders.Add (np);
				}
			}
			
			if (_clientProviders == null && other._clientProviders != null)
			{
				foreach (ProviderData prov in other._clientProviders)
				{
					ProviderData np = new ProviderData();
					np.CopyFrom (prov);
					ClientProviders.Add (np);
				}
			}
		}
	}
	
	internal class ProviderData {
		internal string Ref;
		internal string Type;
		internal string Id;
		
		internal Hashtable CustomProperties = new Hashtable ();
		internal IList CustomData;
		
		public void CopyFrom (ProviderData other)
		{
			if (Ref == null) Ref = other.Ref;
			if (Id == null) Id = other.Id;
			if (Type == null) Type = other.Type;
			
			foreach (DictionaryEntry entry in other.CustomProperties)
				if (!CustomProperties.ContainsKey (entry.Key))
					CustomProperties [entry.Key] = entry.Value;
					
			if (other.CustomData != null)
			{
				if (CustomData == null) CustomData = new ArrayList ();
				foreach (SinkProviderData data in other.CustomData)
					CustomData.Add (data);
			}
		}
	}
	
	internal class FormatterData: ProviderData {
	}	
}
