//
// System.Configuration.AppSettingsSection.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Chris Toshok (toshok@ximian.com)
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
using System.Collections.Specialized;
using System.Xml;
using System.IO;

namespace System.Configuration {

	public sealed class AppSettingsSection : ConfigurationSection
	{
		KeyValueConfigurationCollection values;
		const string configFile = "file";

                private static ConfigurationPropertyCollection _properties;
                private static readonly ConfigurationProperty _propFile;

                static AppSettingsSection ()
                {
                        _properties     = new ConfigurationPropertyCollection ();
                        _propFile = new ConfigurationProperty (configFile, 
                                                               typeof(string), 
                                                               "", 
                                                               ConfigurationPropertyOptions.None);

                        _properties.Add (_propFile);
                }

		public AppSettingsSection ()
		{
		}

		protected internal override  bool IsModified ()
		{
			return Settings.IsModified ();
		}

		[MonoTODO ("Read file attribute")]
		protected internal override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			Settings.DeserializeElement (reader, serializeCollectionKey);
		}

		protected internal override void Reset (ConfigurationElement parentSection)
		{
			AppSettingsSection psec = parentSection as AppSettingsSection;
			if (psec != null)
				Settings.Reset (psec.Settings);
		}

		[MonoTODO]
		protected internal override string SerializeSection (
			ConfigurationElement parent, string name, ConfigurationSaveMode mode)
		{
			throw new NotImplementedException ();
		}

		[ConfigurationProperty ("file", DefaultValue = "")]
		public string File {
			get { return (string)base [configFile]; }
			set { base [configFile] = value; }
		}

		[ConfigurationProperty ("", DefaultValue = "System.Object", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public KeyValueConfigurationCollection Settings {
			get {
				if (values == null)
					values = new KeyValueConfigurationCollection();
				return values;
			}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get {
				return _properties;
			}
		}

		[MonoTODO]
		protected internal override object GetRuntimeObject ()
		{
			return base.GetRuntimeObject();
		}
	}
}
#endif
