//
// ManagedXslTransform
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

using System;
using System.Collections;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Xml.XPath;
using Mono.Xml.Xsl;


namespace System.Xml.Xsl {
	internal class ManagedXslTransform : XslTransformImpl {
		CompiledStylesheet s;
		
		
		public override void Load (XPathNavigator stylesheet, XmlResolver resolver, Evidence evidence)
		{
			s = new Compiler ().Compile (stylesheet, resolver, evidence);
		}

		public override void Transform (XPathNavigator input, XsltArgumentList args, XmlWriter output, XmlResolver resolver)
		{
			Outputter outputter = new GenericOutputter(output, s.Outputs);
			bool wroteStartDocument = false;
			if (output.WriteState == WriteState.Start) {
				outputter.WriteStartDocument ();
				wroteStartDocument = true;
			}
			new XslTransformProcessor (s).Process (input, outputter, args, resolver);
			if (wroteStartDocument)
				outputter.WriteEndDocument ();
		}

		public override void Transform (XPathNavigator input, XsltArgumentList args, TextWriter output, XmlResolver resolver) {
			Outputter outputter = new GenericOutputter(output, s.Outputs);			
			outputter.WriteStartDocument();
			new XslTransformProcessor (s).Process (input, outputter, args, resolver);
			outputter.WriteEndDocument();			
		}
		
		public override void Transform (XPathNavigator input, XsltArgumentList args, Stream output, XmlResolver resolver)
		{
			XslOutput xslOutput = (XslOutput)s.Outputs[String.Empty];
			if (xslOutput == null)
				Transform (input, args, new StreamWriter (output), resolver);
			else
				Transform (input, args, new StreamWriter (output, xslOutput.Encoding), resolver);
		}
	}
}
