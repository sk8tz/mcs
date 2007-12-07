//
// System.Web.Util.SettingsMappingWhat
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace System.Web.Util
{
	internal enum SettingsMappingWhatOperation
	{
		Add,
		Clear,
		Replace,
		Remove
	}

	internal class SettingsMappingWhatContents
	{
		SettingsMappingWhatOperation _operation;
		Dictionary <string, string> _attributes;

		public SettingsMappingWhatOperation Operation {
			get { return _operation; }
		}

		public Dictionary <string, string> Attributes {
			get { return _attributes; }
		}
    
		public SettingsMappingWhatContents (XPathNavigator nav, SettingsMappingWhatOperation operation)
		{
			_operation = operation;
      
			if (nav.HasAttributes) {
				_attributes = new Dictionary <string, string> ();
	
				nav.MoveToFirstAttribute ();
				_attributes.Add (nav.Name, nav.Value);
	
				while (nav.MoveToNextAttribute ())
					_attributes.Add (nav.Name, nav.Value);
			}
		}
	}
  
	internal class SettingsMappingWhat
	{    
		string _value;
		List <SettingsMappingWhatContents> _contents;
    
		public string Value {
			get { return _value; }
		}

		public List <SettingsMappingWhatContents> Contents {
			get { return _contents; }
		}
    
		public SettingsMappingWhat (XPathNavigator nav)
		{
			_value = nav.GetAttribute ("value", String.Empty);

			XPathNodeIterator iter = nav.Select ("./*");
			XPathNavigator cur;

			_contents = new List <SettingsMappingWhatContents> ();
			while (iter.MoveNext ()) {
				cur = iter.Current;
				switch (cur.LocalName) {
					case "replace":
						_contents.Add (new SettingsMappingWhatContents (cur, SettingsMappingWhatOperation.Replace));
						break;

					case "add":
						_contents.Add (new SettingsMappingWhatContents (cur, SettingsMappingWhatOperation.Add));
						break;

					case "clear":
						_contents.Add (new SettingsMappingWhatContents (cur, SettingsMappingWhatOperation.Clear));
						break;

					case "remove":
						_contents.Add (new SettingsMappingWhatContents (cur, SettingsMappingWhatOperation.Remove));
						break;
				}
			}
		}
	}
}
#endif
