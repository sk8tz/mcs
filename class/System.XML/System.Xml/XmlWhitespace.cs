//
// System.Xml.XmlWhitespace.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Xml
{
	public class XmlWhitespace : XmlCharacterData
	{
		// Constructor
		protected internal XmlWhitespace (string strData, XmlDocument doc)
			: base (strData, doc)
		{
		}
		
		// Properties
		public override string LocalName {
			get { return "#whitespace"; }
		}

		public override string Name {
			get { return "#whitespace"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Whitespace; }
		}

		[MonoTODO]
		public override string Value {
			get { return null; }
			set {}
		}

		// Methods
		[MonoTODO]
		public override XmlNode CloneNode (bool deep)
		{
			return null;
		}

		[MonoTODO]
		public override void WriteContentTo (XmlWriter w)
		{			
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter w)
		{			
		}
	}
}
