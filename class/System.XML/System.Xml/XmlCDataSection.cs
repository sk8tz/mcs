//
// System.Xml.XmlCDataSection.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Xml
{
	public class XmlCDataSection : XmlCharacterData
	{
		#region Constructors

		protected internal XmlCDataSection (string data, XmlDocument doc)
			: base (data, doc)
		{
		}

		#endregion

		#region Properties

		public override string LocalName {
			get { return "#cdata-section"; }
		}

		public override string Name	{
			get { return "#cdata-section"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.CDATA; }
		}

		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			XmlNode n = new XmlCDataSection (Data, OwnerDocument); // CDATA nodes have no children.
			if (IsReadOnly)
				n.SetReadOnly ();
			return n;
		}

		public override void WriteContentTo (XmlWriter w) {	}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteCData (Data);
		}

		#endregion
	}
}
