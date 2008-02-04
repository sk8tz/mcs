//
// XmlSchemaValidatorTests.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2008 Novell Inc.
//

#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaValidatorTests
	{
		[Test]
		public void XsdAnyToSkipAttributeValidation ()
		{
			// bug #358408
			XmlSchemaSet schemas = new XmlSchemaSet ();
			schemas.Add (null, "Test/XmlFiles/xsd/358408.xsd");
			XmlSchemaValidator v = new XmlSchemaValidator (
				new NameTable (),
				schemas,
				new XmlNamespaceManager (new NameTable ()),
				XmlSchemaValidationFlags.ProcessIdentityConstraints);
			v.Initialize ();
			v.ValidateWhitespace (" ");
			XmlSchemaInfo info = new XmlSchemaInfo ();
			ArrayList list = new ArrayList ();

			v.ValidateElement ("configuration", "", info, null, null, null, null);
			v.GetUnspecifiedDefaultAttributes (list);
			v.ValidateEndOfAttributes (info);

			v.ValidateWhitespace (" ");

			v.ValidateElement ("host", "", info, null, null, null, null);
			v.ValidateAttribute ("auto-start", "", "true", info);
			list.Clear ();
			v.GetUnspecifiedDefaultAttributes (list);
			v.ValidateEndOfAttributes (info);
			v.ValidateEndElement (null);//info);

			v.ValidateWhitespace (" ");

			v.ValidateElement ("service-managers", "", info, null, null, null, null);
			list.Clear ();
			v.GetUnspecifiedDefaultAttributes (list);
			v.ValidateEndOfAttributes (info);

			v.ValidateWhitespace (" ");

			v.ValidateElement ("service-manager", "", info, null, null, null, null);
			list.Clear ();
			v.GetUnspecifiedDefaultAttributes (list);
			v.ValidateEndOfAttributes (info);

			v.ValidateWhitespace (" ");

			v.ValidateElement ("foo", "", info, null, null, null, null);
			v.ValidateAttribute ("bar", "", "", info);
		}
	}
}

#endif
