//
// System.Configuration.Configuration.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
// 	Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using System.IO;

namespace System.Configuration {

	public sealed class Configuration
	{
		Configuration parent;
		Hashtable elementData = new Hashtable ();
		string fileName;
		ConfigurationSectionGroup rootSectionGroup;
		ConfigurationLocationCollection locations;
		SectionGroupInfo rootGroup;
		
		internal Configuration (): this (null, null)
		{
		}
		
		internal Configuration (string file): this (file, null)
		{
		}
		
		internal Configuration (Configuration parent): this (null, parent)
		{
		}
		
		internal Configuration (string file, Configuration parent)
		{
			fileName = file;
			this.parent = parent;
			if (parent != null)
				rootGroup = parent.rootGroup;
			else {
				rootGroup = new SectionGroupInfo ();
				rootGroup.FileName = file;
			}
			
			if (file != null && File.Exists (file)) Load (file);
		}
		
		internal Configuration Parent {
			get { return parent; }
		}
		
		internal string FileName {
			get { return fileName; }
		}

		public AppSettingsSection AppSettings {
			get { return Sections ["appSettings"] as AppSettingsSection; }
		}

		public ConnectionStringsSection ConnectionStrings {
			get { return (ConnectionStringsSection) GetSection ("connectionStrings"); }
		}

		public string FilePath {
			get { return fileName; }
		}

		public bool HasFile {
			get {
				if (File.Exists (fileName))
					return true;

				if (parent != null)
					return parent.HasFile;
				else
					return false; 
			 }
		}

		public ConfigurationLocationCollection Locations {
			get {
				if (locations == null) locations = new ConfigurationLocationCollection ();
				return locations;
			}
		}

		public ConfigurationSectionGroup RootSectionGroup {
			get {
				if (rootSectionGroup == null) {
					rootSectionGroup = new ConfigurationSectionGroup ();
					rootSectionGroup.Initialize (this, rootGroup);
				}
				return rootSectionGroup;
			}                        
		}

		public ConfigurationSectionGroupCollection SectionGroups {
			get { return RootSectionGroup.SectionGroups; }                        
		}

		public ConfigurationSectionCollection Sections {
			get { return RootSectionGroup.Sections; }
		}
		
		public ConfigurationSection GetSection (string path)
		{
			string[] parts = path.Split ('/');
			if (parts.Length == 1)
				return Sections [parts[0]];

			ConfigurationSectionGroup group = SectionGroups [parts[0]];
			for (int n=1; group != null && n<parts.Length-1; n++)
				group = group.SectionGroups [parts [n]];

			if (group != null)
				return group.Sections [parts [parts.Length - 1]];
			else
				return null;
		}
		
		public ConfigurationSectionGroup GetSectionGroup (string path)
		{
			string[] parts = path.Split ('/');
			ConfigurationSectionGroup group = SectionGroups [parts[0]];
			for (int n=1; group != null && n<parts.Length; n++)
				group = group.SectionGroups [parts [n]];
			return group;
		}
		
		internal ConfigurationSection GetSectionInstance (SectionInfo config, bool createDefaultInstance)
		{
			object data = elementData [config];
			ConfigurationSection sec = data as ConfigurationSection;
			if (sec != null || !createDefaultInstance) return sec;
			
			object secObj = config.CreateInstance () as ConfigurationSection;
			if (!(secObj is ConfigurationSection))
				sec = new RuntimeOnlySection ();
			else {
				sec = (ConfigurationSection) secObj;
			}
				
			ConfigurationSection parentSection = parent != null ? parent.GetSectionInstance (config, true) : null;
			sec.RawXml = data as string;
			sec.Reset (parentSection);
			
			if (data != null) {
				XmlTextReader r = new XmlTextReader (new StringReader (data as string));
				sec.DeserializeSection (r);
				r.Close ();
			}
			
			elementData [config] = sec;
			return sec;
		}
		
		internal ConfigurationSectionGroup GetSectionGroupInstance (SectionGroupInfo group)
		{
			ConfigurationSectionGroup gr = group.CreateInstance () as ConfigurationSectionGroup;
			if (gr != null) gr.Initialize (this, group);
			return gr;
		}
		
		internal void SetConfigurationSection (SectionInfo config, ConfigurationSection sec)
		{
			elementData [config] = sec;
		}
		
		internal void SetSectionXml (SectionInfo config, string data)
		{
			elementData [config] = data;
		}
		
		internal string GetSectionXml (SectionInfo config)
		{
			return elementData [config] as string;
		}
		
		internal void CreateSection (SectionGroupInfo group, string name, ConfigurationSection sec)
		{
			if (group.HasChild (name)) throw new ConfigurationException ("Cannot add a ConfigurationSection. A section or section group already exists with the name '" + name + "'");
			if (sec.SectionInformation.Type == null) sec.SectionInformation.Type = sec.GetType ().AssemblyQualifiedName;
			sec.SectionInformation.SetName (name);

			SectionInfo section = new SectionInfo (name, sec.SectionInformation.Type, sec.SectionInformation.AllowLocation, sec.SectionInformation.AllowDefinition);
			section.FileName = FileName;
			group.AddChild (section);
			elementData [section] = sec;
		}
		
		internal void CreateSectionGroup (SectionGroupInfo parentGroup, string name, ConfigurationSectionGroup sec)
		{
			if (parentGroup.HasChild (name)) throw new ConfigurationException ("Cannot add a ConfigurationSectionGroup. A section or section group already exists with the name '" + name + "'");
			if (sec.Type == null) sec.Type = sec.GetType ().AssemblyQualifiedName;
			sec.SetName (name);

			SectionGroupInfo section = new SectionGroupInfo (name, sec.Type);
			section.FileName = FileName;
			parentGroup.AddChild (section);
			elementData [section] = sec;
		}
		
		internal void RemoveConfigInfo (ConfigInfo config)
		{
			elementData.Remove (config);
		}
		
		public void Save ()
		{
			SaveAs (fileName, ConfigurationSaveMode.Modified, false);
		}
		
		public void Save (ConfigurationSaveMode mode)
		{
			SaveAs (fileName, mode, false);
		}
		
		public void Save (ConfigurationSaveMode mode, bool forceUpdateAll)
		{
			SaveAs (fileName, mode, forceUpdateAll);
		}
		
		public void SaveAs (string filename)
		{
			SaveAs (filename, ConfigurationSaveMode.Modified, false);
		}
		
		public void SaveAs (string filename, ConfigurationSaveMode mode)
		{
			SaveAs (filename, mode, false);
		}
		
		[MonoTODO ("Detect if file has changed")]
		public void SaveAs (string filename, ConfigurationSaveMode mode, bool forceUpdateAll)
		{
			XmlTextWriter tw = new XmlTextWriter (new StreamWriter (filename));
			tw.Formatting = Formatting.Indented;
			try {
				tw.WriteStartElement ("configuration");
				if (rootGroup.HasConfigContent (this)) {
					rootGroup.WriteConfig (this, tw, mode);
				}
				rootGroup.WriteRootData (tw, this, mode);
				tw.WriteEndElement ();
			}
			finally {
				tw.Close ();
			}
		}
		
		internal bool Load (string fileName)
		{
			this.fileName = fileName;
			if (!File.Exists (fileName))
				throw new ConfigurationException ("File '" + fileName + "' not found");

			XmlTextReader reader = null;

			try {
				FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read);
				reader = new XmlTextReader (fs);
				ReadConfigFile (reader, fileName);
/*			} catch (ConfigurationException) {
				throw;
			} catch (Exception e) {
				throw new ConfigurationException ("Error reading " + fileName, e);
*/			} finally {
				if (reader != null)
					reader.Close();
			}
			return true;
		}


		internal void ReadConfigFile (XmlTextReader reader, string fileName)
		{
			reader.MoveToContent ();
			if (reader.NodeType != XmlNodeType.Element || reader.Name != "configuration")
				ThrowException ("Configuration file does not have a valid root element", reader);

			if (reader.HasAttributes)
				ThrowException ("Unrecognized attribute in root element", reader);

			if (reader.IsEmptyElement) {
				reader.Skip ();
				return;
			}
			
			reader.ReadStartElement ();
			reader.MoveToContent ();

			if (reader.LocalName == "configSections") {
				if (reader.HasAttributes)
					ThrowException ("Unrecognized attribute in <configSections>.", reader);
				
				rootGroup.ReadConfig (this, reader);
			}
			
			rootGroup.ReadRootData (reader, this);
		}
		

		private void ThrowException (string text, XmlTextReader reader)
		{
			throw new ConfigurationException (text, fileName, reader.LineNumber);
		}
	}
}

#endif
