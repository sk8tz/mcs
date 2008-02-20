//
// System.Configuration.ConfigurationSection.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//  Lluis Sanchez Gual (lluis@novell.com)
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
using System.Collections;
using System.Xml;
using System.IO;
#if !TARGET_JVM
using System.Security.Cryptography.Xml;
#endif

namespace System.Configuration
{
	public abstract class ConfigurationSection : ConfigurationElement
	{
		SectionInformation sectionInformation;
		IConfigurationSectionHandler section_handler;

		protected ConfigurationSection ()
		{
		}
		
		internal IConfigurationSectionHandler SectionHandler {
			get { return section_handler; }
			set { section_handler = value; }
		}

		[MonoTODO]
		public SectionInformation SectionInformation {
			get {
				if (sectionInformation == null)
					sectionInformation = new SectionInformation ();
				return sectionInformation;
			}
		}

		[MonoTODO ("Provide ConfigContext. Likely the culprit of bug #322493")]
		protected internal virtual object GetRuntimeObject ()
		{
			if (SectionHandler != null) {
				ConfigurationSection parentSection = sectionInformation != null ? sectionInformation.GetParentSection () : null;
				object parent = parentSection != null ? parentSection.GetRuntimeObject () : null;
				if (RawXml == null)
					return parent;
				XmlDocument doc = new XmlDocument ();
				doc.LoadXml (RawXml);
				return SectionHandler.Create (parent, null, doc.DocumentElement);
			}
			return this;
		}

		[MonoTODO]
		protected internal override bool IsModified ()
		{
			return base.IsModified ();
		}

		[MonoTODO]
		protected internal override void ResetModified ()
		{
			base.ResetModified ();
		}

		ConfigurationElement CreateElement (Type t)
		{
			ConfigurationElement elem = (ConfigurationElement) Activator.CreateInstance (t);
			elem.Init ();
			if (IsReadOnly ())
				elem.SetReadOnly ();
			return elem;
		}

		[MonoInternalNote ("find the proper location for the decryption stuff")]
		protected internal virtual void DeserializeSection (XmlReader reader)
		{
			reader.MoveToContent ();

			string protection_provider = null;
			string config_source = null;
			string localName;
			
			while (reader.MoveToNextAttribute ()) {
				localName = reader.LocalName;
				
				if (localName == "configProtectionProvider")
					protection_provider = reader.Value;
				else if (localName == "configSource")
					config_source = reader.Value;
			}

			/* XXX this stuff shouldn't be here */
			{
				if (protection_provider != null) {
					ProtectedConfigurationProvider prov = ProtectedConfiguration.GetProvider (protection_provider, true);
					XmlDocument doc = new XmlDocument ();

					reader.MoveToElement ();

					doc.Load (new StringReader (reader.ReadInnerXml ()));

					XmlNode n = prov.Decrypt (doc);

					reader = new XmlNodeReader (n);

					SectionInformation.ProtectSection (protection_provider);

					reader.MoveToContent ();
				}
			}

			XmlReader r = reader;
			if (config_source != null) {
				if (config_source.Length == 0 || Path.IsPathRooted (config_source))
					throw new ConfigurationException ("The configSource attribute must be a relative physical path.");
				
				if (HasValues ())
					throw new ConfigurationException ("A section using 'configSource' may contain no other attributes or elements.");
				
				SectionInformation.ConfigSource = config_source;
				if (File.Exists (config_source)) {
					RawXml = File.ReadAllText (config_source);
					r = new XmlTextReader (new StringReader (RawXml));
				} else
					RawXml = null;
			}
				
			SectionInformation.SetRawXml (RawXml);
			DeserializeElement (r, false);
		}

		protected internal virtual string SerializeSection (ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
		{
			ConfigurationElement elem;
			if (parentElement != null) {
				elem = (ConfigurationElement) CreateElement (GetType());
				elem.Unmerge (this, parentElement, saveMode);
			}
			else
				elem = this;

			StringWriter sw = new StringWriter ();
			XmlTextWriter tw = new XmlTextWriter (sw);
			tw.Formatting = Formatting.Indented;
			elem.SerializeToXmlElement (tw, name);
			tw.Close ();
			return sw.ToString ();
		}
	}
}
#endif
