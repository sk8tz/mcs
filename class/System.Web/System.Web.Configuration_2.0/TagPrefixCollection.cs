//
// System.Web.Configuration.TagPrefixCollection
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
using System.Collections;
using System.Configuration;

namespace System.Web.Configuration
{
	[ConfigurationCollection (typeof (TagPrefixInfo))]
	public sealed class TagPrefixCollection : ConfigurationElementCollection, ICollection, IEnumerable
	{
		static ConfigurationPropertyCollection properties;

		static TagPrefixCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public TagPrefixCollection ()
		{
		}

		public void Add (TagPrefixInfo tagPrefixInformation)
		{
			BaseAdd (tagPrefixInformation);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new TagPrefixInfo ("", "", "", "", "");
		}

		[MonoTODO]
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((TagPrefixInfo)element).TagPrefix;
		}

		public void Remove (TagPrefixInfo tagPrefixInformation)
		{
			BaseRemove (tagPrefixInformation.TagPrefix);
		}

		[MonoTODO]
		protected override ConfigurationElementCollectionType CollectionType {
			get {
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		[MonoTODO]
		protected override string ElementName {
			get {
				throw new NotImplementedException ();
			}
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public TagPrefixInfo this[int index] {
			get { return (TagPrefixInfo) BaseGet (index); }
			set { if (BaseGet (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}

		protected override bool ThrowOnDuplicate {
			get { return false; }
		}
	}
}

#endif
