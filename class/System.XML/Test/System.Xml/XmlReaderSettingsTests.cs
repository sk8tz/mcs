//
// System.Xml.XmlReaderSettingsTests.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

#if NET_2_0
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

using ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlReaderSettingsTests : Assertion
	{
		public Stream CreateStream (string xml)
		{
			return new MemoryStream (Encoding.UTF8.GetBytes (xml));
		}

		[Test]
		public void DefaultValue ()
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			AssertEquals (true, s.CheckCharacters);
			AssertEquals (ConformanceLevel.Document,
				s.ConformanceLevel);
			Assert (s.ValidationType != ValidationType.DTD);
			AssertEquals (false, s.IgnoreComments);
			Assert (0 == (s.ValidationFlags &
				ValidationFlags.ProcessInlineSchema));
			AssertEquals (false, s.IgnoreProcessingInstructions);
			Assert (0 == (s.ValidationFlags &
				ValidationFlags.ProcessSchemaLocation));
			Assert (0 == (s.ValidationFlags &
				ValidationFlags.ReportValidationWarnings));
			Assert (0 != (s.ValidationFlags &
				ValidationFlags.ProcessIdentityConstraints));
			Assert (0 == (s.ValidationFlags &
				ValidationFlags.AllowXmlAttributes));
			AssertEquals (false, s.IgnoreWhitespace);
			AssertEquals (0, s.LineNumberOffset);
			AssertEquals (0, s.LinePositionOffset);
			AssertNull (s.NameTable);
			AssertEquals (0, s.Schemas.Count);
			Assert (s.ValidationType != ValidationType.Schema);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void SetSchemas ()
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			s.Schemas = new XmlSchemaSet ();
		}

		[Test]
		public void CloseInput ()
		{
			StringReader sr = new StringReader ("<root/><root/>");
			XmlReader xtr = XmlReader.Create (sr); // default false
			xtr.Read ();
			xtr.Close ();
			// It should without error, unlike usual XmlTextReader.
			sr.ReadLine ();
		}

		[Test]
		public void CreateAndNormalization ()
		{
			StringReader sr = new StringReader (
				"<root attr='   value   '>test\rstring</root>");
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.CheckCharacters = false;
			XmlReader xtr = XmlReader.Create (
				sr, settings);
			xtr.Read ();
			xtr.MoveToFirstAttribute ();
			AssertEquals ("   value   ", xtr.Value);
			xtr.Read ();
			// Text string is normalized
			AssertEquals ("test\nstring", xtr.Value);
		}

		[Test]
		public void CheckCharactersAndNormalization ()
		{
			// It should *not* raise an error (even Normalization
			// is set by default).
			StringReader sr = new StringReader (
				"<root attr='&#0;'>&#x0;</root>");
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.CheckCharacters = false;
			XmlReader xtr = XmlReader.Create (
				sr, settings);
			// After creation, changes on source XmlReaderSettings
			// does not matter.
			settings.CheckCharacters = false;
			xtr.Read ();
			xtr.MoveToFirstAttribute ();
			AssertEquals ("\0", xtr.Value);
			xtr.Read ();
			AssertEquals ("\0", xtr.Value);
		}

		// Hmm, does it really make sense? :-/
		[Test]
		public void CheckCharactersForNonTextReader ()
		{
			// It should *not* raise an error (even Normalization
			// is set by default).
			StringReader sr = new StringReader (
				"<root attr='&#0;'>&#x0;</root>");
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.CheckCharacters = false;
			XmlReader xr = XmlReader.Create (
				sr, settings);

			// Enable character checking for XmlNodeReader.
			settings.CheckCharacters = true;
			XmlDocument doc = new XmlDocument ();
			doc.Load (xr);
			xr = XmlReader.Create (new XmlNodeReader (doc), settings);

			// But it won't work against XmlNodeReader.
			xr.Read ();
			xr.MoveToFirstAttribute ();
			AssertEquals ("\0", xr.Value);
			xr.Read ();
			AssertEquals ("\0", xr.Value);
		}

		[Test]
		public void CreateAndSettings ()
		{
			AssertNotNull (XmlReader.Create (CreateStream ("<xml/>")).Settings);
			AssertNotNull (XmlReader.Create ("Test/XmlFiles/simple.xml").Settings);
		}

		[Test]
		public void CreateAndNameTable ()
		{
			// By default NameTable is null, but some of
			// XmlReader.Create() should not result in null
			// reference exceptions.
			XmlReaderSettings s = new XmlReaderSettings ();
			XmlReader.Create (new StringReader ("<root/>"), s, String.Empty)
				.Read ();
			XmlReader.Create (new StringReader ("<root/>"), s, (XmlParserContext) null)
				.Read ();
			XmlReader.Create (CreateStream ("<root/>"), s, String.Empty)
				.Read ();
			XmlReader.Create (CreateStream ("<root/>"), s, (XmlParserContext) null)
				.Read ();
		}
	}
}
#endif
