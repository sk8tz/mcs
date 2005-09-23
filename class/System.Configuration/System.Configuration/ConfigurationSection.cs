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

namespace System.Configuration
{
	public abstract class ConfigurationSection : ConfigurationElement
	{
		SectionInformation sectionInformation;
		
		public ConfigurationSection ()
		{
		}
		
		[MonoTODO]
		public SectionInformation SectionInformation {
			get {
				if (sectionInformation == null)
					sectionInformation = new SectionInformation ();
				return sectionInformation;
			}
		}
		
		protected internal virtual object GetRuntimeObject ()
		{
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
			throw new NotImplementedException ();
		}

		ConfigurationElement CreateElement (Type t)
		{
			ConfigurationElement elem = (ConfigurationElement) Activator.CreateInstance (t);
			elem.Init ();
			if (IsReadOnly ())
				elem.SetReadOnly ();
			return elem;
		}

		protected internal virtual void DeserializeSection (XmlReader reader)
		{
			DeserializeElement (reader, false);
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
