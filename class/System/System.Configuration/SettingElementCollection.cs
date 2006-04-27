//
// System.Web.UI.WebControls.SettingElementCollection.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Configuration
{
	public sealed class SettingElementCollection
#if (CONFIGURATION_DEP)
		: ConfigurationElementCollection
#endif
	{
		public SettingElementCollection ()
		{
		}

		[MonoTODO]
		public void Add (SettingElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SettingElement Get (string elementKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (SettingElement element)
		{
			throw new NotImplementedException ();
		}

#if (CONFIGURATION_DEP)
		protected override ConfigurationElement CreateNewElement ()
		{
			return new SettingElement ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((SettingElement) element).Name;
		}

		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName {
			get { return "setting"; }
		}
#endif
	}

}

#endif
