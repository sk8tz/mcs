//
// System.Web.Configuration.TagMapInfo
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

#if NET_2_0

using System;
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	public sealed class TagMapInfo : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty mappedTagTypeProp;
		static ConfigurationProperty tagTypeProp;


		static TagMapInfo ()
		{
			mappedTagTypeProp = new ConfigurationProperty ("mappedTagType", typeof (string));
			tagTypeProp = new ConfigurationProperty ("tagType", typeof (string), "", ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (mappedTagTypeProp);
			properties.Add (tagTypeProp);
		}

		public TagMapInfo (string tagTypeName, string mappedTagTypeName)
		{
			this.TagType = tagTypeName;
			this.MappedTagType = mappedTagTypeName;
		}

		[MonoTODO]
		public override bool Equals (object map)
		{
			return base.Equals (map);
		}

		[MonoTODO]
		protected override bool SerializeElement (XmlWriter writer, bool serializeCollectionKey)
		{
			return base.SerializeElement (writer, serializeCollectionKey);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("mappedTagType")]
		public string MappedTagType {
			get { return (string) base[mappedTagTypeProp]; }
			set { base[mappedTagTypeProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("tagType", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string TagType {
			get { return (string) base[tagTypeProp]; }
			set { base[tagTypeProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
