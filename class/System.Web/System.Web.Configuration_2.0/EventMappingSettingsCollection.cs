//
// System.Web.Configuration.EventMappingSettingsCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//

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

using System;
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	[ConfigurationCollection (typeof (EventMappingSettings), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class EventMappingSettingsCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static EventMappingSettingsCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public void Add (EventMappingSettings eventMappingSettings)
		{
			BaseAdd (eventMappingSettings);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		[MonoTODO]
		public bool Contains (string name)
		{
			throw new NotImplementedException ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new EventMappingSettings ("", "");
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((EventMappingSettings)element).Name;
		}

		[MonoTODO]
		public int IndexOf (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, EventMappingSettings eventMappingSettings)
		{
			throw new NotImplementedException ();
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		public EventMappingSettings this [int index] {
			get { return (EventMappingSettings) BaseGet (index); }
			set { if (BaseGet (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}

		public new EventMappingSettings this [string name] {
			get { return (EventMappingSettings) BaseGet (name); }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

