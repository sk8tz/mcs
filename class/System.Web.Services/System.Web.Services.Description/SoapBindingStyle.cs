// 
// System.Web.Services.Description.SoapBindingStyle.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public enum SoapBindingStyle {
		[XmlIgnore]
		Default,
		[XmlEnum ("document")]
		Document,
		[XmlEnum ("rpc")]
		Rpc
	}
}
