#if NET_2_0
using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Resources;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections;

using NUnit.Framework;
using System.ComponentModel.Design;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResXDataNodeAliasTests : MonoTests.System.Windows.Forms.TestHelper {
		string _tempDirectory;
		string _otherTempDirectory;
		
		[Test, ExpectedException (typeof (TypeLoadException))]
		public void CantAccessValueWereOnlyFullNameInResXForEmbedded () // same as validity check in assemblynames tests
		{
			
			string filePath = GetFileFromString ("convertableResX.resx", convertableResX);

			using (ResXResourceReader reader = new ResXResourceReader (filePath)) {

				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				DictionaryEntry current = (DictionaryEntry) enumerator.Current;
				ResXDataNode node = (ResXDataNode) current.Value;

				object obj = node.GetValue ((AssemblyName[]) null);
			}
		}

		[Test, ExpectedException (typeof (TypeLoadException))]
		public void CantAccessValueWereOnlyFullNameAndAliasInResXForEmbedded ()
		{
			
			string filePath = GetFileFromString ("convertableResXAlias.resx", convertableResXAlias);

			using (ResXResourceReader reader = new ResXResourceReader (filePath)) {

				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				DictionaryEntry current = (DictionaryEntry) enumerator.Current;
				ResXDataNode node = (ResXDataNode) current.Value;

				object obj = node.GetValue ((AssemblyName []) null);
			}
		}

		[Test]
		public void CanAccessValueWereOnlyFullNameAndAssemblyInResXForEmbedded ()
		{

			string filePath = GetFileFromString ("convertableResXAssembly.resx", convertableResXAssembly);

			using (ResXResourceReader reader = new ResXResourceReader (filePath)) {

				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				DictionaryEntry current = (DictionaryEntry) enumerator.Current;
				ResXDataNode node = (ResXDataNode) current.Value;

				object obj = node.GetValue ((AssemblyName []) null);
				// this is the qualified name of the assembly found in dir
				string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

				Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName, "#A1");
			}
		}

		[Test]
		public void CanAccessValueWereOnlyFullNameAndQualifiedAssemblyInResXForEmbedded ()
		{

			string filePath = GetFileFromString ("convertableResXQAN.resx", convertableResXQualifiedAssemblyName);

			using (ResXResourceReader reader = new ResXResourceReader (filePath)) {

				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				DictionaryEntry current = (DictionaryEntry) enumerator.Current;
				ResXDataNode node = (ResXDataNode) current.Value;

				object obj = node.GetValue ((AssemblyName []) null);
				
				string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

				Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName, "#A1");
			}
		}

		/*
		[Test]
		public void GetValueAssemblyNameUsedWhereOnlyFullNameInResXForEmbedded ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly

			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			string filePath = GetFileFromString ("convertableResX.resx", convertableResX);

			using (ResXResourceReader reader = new ResXResourceReader (filePath)) {

				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				DictionaryEntry current = (DictionaryEntry) enumerator.Current;
				ResXDataNode node = (ResXDataNode) current.Value;

				object obj = node.GetValue (assemblyNames);

				Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName);
			}
		}

		[Test]
		public void GetValueTypeNameReturnsFullNameWereOnlyFullNameInResXForEmbedded ()
		{
			// just a check, if this passes other tests will give false results
			string filePath = GetFileFromString ("convertableWithOutAssembly.resx", convertableResX);

			using (ResXResourceReader reader = new ResXResourceReader (filePath)) {

				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				DictionaryEntry current = (DictionaryEntry) enumerator.Current;
				ResXDataNode node = (ResXDataNode) current.Value;

				string returnedType = node.GetValueTypeName ((AssemblyName []) null);

				Assert.AreEqual ("DummyAssembly.Convertable", returnedType);
			}
		}
		
		[Test]
		public void GetValueTypeNameAssemblyNameUsedWhereOnlyFullNameInResXForEmbedded ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly

			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			string filePath = GetFileFromString ("convertableWithOutAssembly.resx", convertableResX);

			using (ResXResourceReader reader = new ResXResourceReader (filePath)) {

				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				DictionaryEntry current = (DictionaryEntry) enumerator.Current;
				ResXDataNode node = (ResXDataNode) current.Value;

				string returnedType = node.GetValueTypeName (assemblyNames);

				Assert.AreEqual ("DummyAssembly.Convertable, " + aName, returnedType);
			}
		}
		*/
		


		static string convertableResX =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <!-- 
	Microsoft ResX Schema 
	
	Version 2.0
	
	The primary goals of this format is to allow a simple XML format 
	that is mostly human readable. The generation and parsing of the 
	various data types are done through the TypeConverter classes 
	associated with the data types.
	
	Example:
	
	... ado.net/XML headers & schema ...
	<resheader name=""resmimetype"">text/microsoft-resx</resheader>
	<resheader name=""version"">2.0</resheader>
	<resheader name=""reader"">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
	<resheader name=""writer"">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
	<data name=""Name1""><value>this is my long string</value><comment>this is a comment</comment></data>
	<data name=""Color1"" type=""System.Drawing.Color, System.Drawing"">Blue</data>
	<data name=""Bitmap1"" mimetype=""application/x-microsoft.net.object.binary.base64"">
		<value>[base64 mime encoded serialized .NET Framework object]</value>
	</data>
	<data name=""Icon1"" type=""System.Drawing.Icon, System.Drawing"" mimetype=""application/x-microsoft.net.object.bytearray.base64"">
		<value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
		<comment>This is a comment</comment>
	</data>
				
	There are any number of ""resheader"" rows that contain simple 
	name/value pairs.
	
	Each data row contains a name, and value. The row also contains a 
	type or mimetype. Type corresponds to a .NET class that support 
	text/value conversion through the TypeConverter architecture. 
	Classes that don't support this are serialized and stored with the 
	mimetype set.
	
	The mimetype is used for serialized objects, and tells the 
	ResXResourceReader how to depersist the object. This is currently not 
	extensible. For a given mimetype the value must be set accordingly:
	
	Note - application/x-microsoft.net.object.binary.base64 is the format 
	that the ResXResourceWriter will generate, however the reader can 
	read any of the formats listed below.
	
	mimetype: application/x-microsoft.net.object.binary.base64
	value   : The object must be serialized with 
			: System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
			: and then encoded with base64 encoding.
	
	mimetype: application/x-microsoft.net.object.soap.base64
	value   : The object must be serialized with 
			: System.Runtime.Serialization.Formatters.Soap.SoapFormatter
			: and then encoded with base64 encoding.

	mimetype: application/x-microsoft.net.object.bytearray.base64
	value   : The object must be serialized into a byte array 
			: using a System.ComponentModel.TypeConverter
			: and then encoded with base64 encoding.
	-->
  <xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
	<xsd:import namespace=""http://www.w3.org/XML/1998/namespace"" />
	<xsd:element name=""root"" msdata:IsDataSet=""true"">
	  <xsd:complexType>
		<xsd:choice maxOccurs=""unbounded"">
		  <xsd:element name=""metadata"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" use=""required"" type=""xsd:string"" />
			  <xsd:attribute name=""type"" type=""xsd:string"" />
			  <xsd:attribute name=""mimetype"" type=""xsd:string"" />
			  <xsd:attribute ref=""xml:space"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""assembly"">
			<xsd:complexType>
			  <xsd:attribute name=""alias"" type=""xsd:string"" />
			  <xsd:attribute name=""name"" type=""xsd:string"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""data"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
				<xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" msdata:Ordinal=""1"" />
			  <xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />
			  <xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />
			  <xsd:attribute ref=""xml:space"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""resheader"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />
			</xsd:complexType>
		  </xsd:element>
		</xsd:choice>
	  </xsd:complexType>
	</xsd:element>
  </xsd:schema>
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  
  <data name=""test"" type=""DummyAssembly.Convertable"">
	<value>im a name	im a value</value>
  </data>
</root>";


		static string convertableResXAssembly =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <!-- 
	Microsoft ResX Schema 
	
	Version 2.0
	
	The primary goals of this format is to allow a simple XML format 
	that is mostly human readable. The generation and parsing of the 
	various data types are done through the TypeConverter classes 
	associated with the data types.
	
	Example:
	
	... ado.net/XML headers & schema ...
	<resheader name=""resmimetype"">text/microsoft-resx</resheader>
	<resheader name=""version"">2.0</resheader>
	<resheader name=""reader"">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
	<resheader name=""writer"">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
	<data name=""Name1""><value>this is my long string</value><comment>this is a comment</comment></data>
	<data name=""Color1"" type=""System.Drawing.Color, System.Drawing"">Blue</data>
	<data name=""Bitmap1"" mimetype=""application/x-microsoft.net.object.binary.base64"">
		<value>[base64 mime encoded serialized .NET Framework object]</value>
	</data>
	<data name=""Icon1"" type=""System.Drawing.Icon, System.Drawing"" mimetype=""application/x-microsoft.net.object.bytearray.base64"">
		<value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
		<comment>This is a comment</comment>
	</data>
				
	There are any number of ""resheader"" rows that contain simple 
	name/value pairs.
	
	Each data row contains a name, and value. The row also contains a 
	type or mimetype. Type corresponds to a .NET class that support 
	text/value conversion through the TypeConverter architecture. 
	Classes that don't support this are serialized and stored with the 
	mimetype set.
	
	The mimetype is used for serialized objects, and tells the 
	ResXResourceReader how to depersist the object. This is currently not 
	extensible. For a given mimetype the value must be set accordingly:
	
	Note - application/x-microsoft.net.object.binary.base64 is the format 
	that the ResXResourceWriter will generate, however the reader can 
	read any of the formats listed below.
	
	mimetype: application/x-microsoft.net.object.binary.base64
	value   : The object must be serialized with 
			: System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
			: and then encoded with base64 encoding.
	
	mimetype: application/x-microsoft.net.object.soap.base64
	value   : The object must be serialized with 
			: System.Runtime.Serialization.Formatters.Soap.SoapFormatter
			: and then encoded with base64 encoding.

	mimetype: application/x-microsoft.net.object.bytearray.base64
	value   : The object must be serialized into a byte array 
			: using a System.ComponentModel.TypeConverter
			: and then encoded with base64 encoding.
	-->
  <xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
	<xsd:import namespace=""http://www.w3.org/XML/1998/namespace"" />
	<xsd:element name=""root"" msdata:IsDataSet=""true"">
	  <xsd:complexType>
		<xsd:choice maxOccurs=""unbounded"">
		  <xsd:element name=""metadata"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" use=""required"" type=""xsd:string"" />
			  <xsd:attribute name=""type"" type=""xsd:string"" />
			  <xsd:attribute name=""mimetype"" type=""xsd:string"" />
			  <xsd:attribute ref=""xml:space"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""assembly"">
			<xsd:complexType>
			  <xsd:attribute name=""alias"" type=""xsd:string"" />
			  <xsd:attribute name=""name"" type=""xsd:string"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""data"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
				<xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" msdata:Ordinal=""1"" />
			  <xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />
			  <xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />
			  <xsd:attribute ref=""xml:space"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""resheader"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />
			</xsd:complexType>
		  </xsd:element>
		</xsd:choice>
	  </xsd:complexType>
	</xsd:element>
  </xsd:schema>
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  
  <data name=""test"" type=""DummyAssembly.Convertable, DummyAssembly"">
	<value>im a name	im a value</value>
  </data>
</root>";


		static string convertableResXAlias =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <!-- 
	Microsoft ResX Schema 
	
	Version 2.0
	
	The primary goals of this format is to allow a simple XML format 
	that is mostly human readable. The generation and parsing of the 
	various data types are done through the TypeConverter classes 
	associated with the data types.
	
	Example:
	
	... ado.net/XML headers & schema ...
	<resheader name=""resmimetype"">text/microsoft-resx</resheader>
	<resheader name=""version"">2.0</resheader>
	<resheader name=""reader"">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
	<resheader name=""writer"">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
	<data name=""Name1""><value>this is my long string</value><comment>this is a comment</comment></data>
	<data name=""Color1"" type=""System.Drawing.Color, System.Drawing"">Blue</data>
	<data name=""Bitmap1"" mimetype=""application/x-microsoft.net.object.binary.base64"">
		<value>[base64 mime encoded serialized .NET Framework object]</value>
	</data>
	<data name=""Icon1"" type=""System.Drawing.Icon, System.Drawing"" mimetype=""application/x-microsoft.net.object.bytearray.base64"">
		<value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
		<comment>This is a comment</comment>
	</data>
				
	There are any number of ""resheader"" rows that contain simple 
	name/value pairs.
	
	Each data row contains a name, and value. The row also contains a 
	type or mimetype. Type corresponds to a .NET class that support 
	text/value conversion through the TypeConverter architecture. 
	Classes that don't support this are serialized and stored with the 
	mimetype set.
	
	The mimetype is used for serialized objects, and tells the 
	ResXResourceReader how to depersist the object. This is currently not 
	extensible. For a given mimetype the value must be set accordingly:
	
	Note - application/x-microsoft.net.object.binary.base64 is the format 
	that the ResXResourceWriter will generate, however the reader can 
	read any of the formats listed below.
	
	mimetype: application/x-microsoft.net.object.binary.base64
	value   : The object must be serialized with 
			: System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
			: and then encoded with base64 encoding.
	
	mimetype: application/x-microsoft.net.object.soap.base64
	value   : The object must be serialized with 
			: System.Runtime.Serialization.Formatters.Soap.SoapFormatter
			: and then encoded with base64 encoding.

	mimetype: application/x-microsoft.net.object.bytearray.base64
	value   : The object must be serialized into a byte array 
			: using a System.ComponentModel.TypeConverter
			: and then encoded with base64 encoding.
	-->
  <xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
	<xsd:import namespace=""http://www.w3.org/XML/1998/namespace"" />
	<xsd:element name=""root"" msdata:IsDataSet=""true"">
	  <xsd:complexType>
		<xsd:choice maxOccurs=""unbounded"">
		  <xsd:element name=""metadata"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" use=""required"" type=""xsd:string"" />
			  <xsd:attribute name=""type"" type=""xsd:string"" />
			  <xsd:attribute name=""mimetype"" type=""xsd:string"" />
			  <xsd:attribute ref=""xml:space"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""assembly"">
			<xsd:complexType>
			  <xsd:attribute name=""alias"" type=""xsd:string"" />
			  <xsd:attribute name=""name"" type=""xsd:string"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""data"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
				<xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" msdata:Ordinal=""1"" />
			  <xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />
			  <xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />
			  <xsd:attribute ref=""xml:space"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""resheader"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />
			</xsd:complexType>
		  </xsd:element>
		</xsd:choice>
	  </xsd:complexType>
	</xsd:element>
  </xsd:schema>
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <assembly alias=""DummyAssembly"" name=""DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" />
  <data name=""test"" type=""DummyAssembly.Convertable"">
	<value>im a name	im a value</value>
  </data>
</root>";

		static string convertableResXQualifiedAssemblyName =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <!-- 
	Microsoft ResX Schema 
	
	Version 2.0
	
	The primary goals of this format is to allow a simple XML format 
	that is mostly human readable. The generation and parsing of the 
	various data types are done through the TypeConverter classes 
	associated with the data types.
	
	Example:
	
	... ado.net/XML headers & schema ...
	<resheader name=""resmimetype"">text/microsoft-resx</resheader>
	<resheader name=""version"">2.0</resheader>
	<resheader name=""reader"">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
	<resheader name=""writer"">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
	<data name=""Name1""><value>this is my long string</value><comment>this is a comment</comment></data>
	<data name=""Color1"" type=""System.Drawing.Color, System.Drawing"">Blue</data>
	<data name=""Bitmap1"" mimetype=""application/x-microsoft.net.object.binary.base64"">
		<value>[base64 mime encoded serialized .NET Framework object]</value>
	</data>
	<data name=""Icon1"" type=""System.Drawing.Icon, System.Drawing"" mimetype=""application/x-microsoft.net.object.bytearray.base64"">
		<value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
		<comment>This is a comment</comment>
	</data>
				
	There are any number of ""resheader"" rows that contain simple 
	name/value pairs.
	
	Each data row contains a name, and value. The row also contains a 
	type or mimetype. Type corresponds to a .NET class that support 
	text/value conversion through the TypeConverter architecture. 
	Classes that don't support this are serialized and stored with the 
	mimetype set.
	
	The mimetype is used for serialized objects, and tells the 
	ResXResourceReader how to depersist the object. This is currently not 
	extensible. For a given mimetype the value must be set accordingly:
	
	Note - application/x-microsoft.net.object.binary.base64 is the format 
	that the ResXResourceWriter will generate, however the reader can 
	read any of the formats listed below.
	
	mimetype: application/x-microsoft.net.object.binary.base64
	value   : The object must be serialized with 
			: System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
			: and then encoded with base64 encoding.
	
	mimetype: application/x-microsoft.net.object.soap.base64
	value   : The object must be serialized with 
			: System.Runtime.Serialization.Formatters.Soap.SoapFormatter
			: and then encoded with base64 encoding.

	mimetype: application/x-microsoft.net.object.bytearray.base64
	value   : The object must be serialized into a byte array 
			: using a System.ComponentModel.TypeConverter
			: and then encoded with base64 encoding.
	-->
  <xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
	<xsd:import namespace=""http://www.w3.org/XML/1998/namespace"" />
	<xsd:element name=""root"" msdata:IsDataSet=""true"">
	  <xsd:complexType>
		<xsd:choice maxOccurs=""unbounded"">
		  <xsd:element name=""metadata"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" use=""required"" type=""xsd:string"" />
			  <xsd:attribute name=""type"" type=""xsd:string"" />
			  <xsd:attribute name=""mimetype"" type=""xsd:string"" />
			  <xsd:attribute ref=""xml:space"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""assembly"">
			<xsd:complexType>
			  <xsd:attribute name=""alias"" type=""xsd:string"" />
			  <xsd:attribute name=""name"" type=""xsd:string"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""data"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
				<xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" msdata:Ordinal=""1"" />
			  <xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />
			  <xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />
			  <xsd:attribute ref=""xml:space"" />
			</xsd:complexType>
		  </xsd:element>
		  <xsd:element name=""resheader"">
			<xsd:complexType>
			  <xsd:sequence>
				<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
			  </xsd:sequence>
			  <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />
			</xsd:complexType>
		  </xsd:element>
		</xsd:choice>
	  </xsd:complexType>
	</xsd:element>
  </xsd:schema>
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  
  <data name=""test"" type=""DummyAssembly.Convertable, DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"">
	<value>im a name	im a value</value>
  </data>
</root>";


		[TearDown]
		protected override void TearDown ()
		{
			//teardown
			if (Directory.Exists (_tempDirectory))
				Directory.Delete (_tempDirectory, true);

			base.TearDown ();
		}
		
		private string GetFileFromString (string filename, string filecontents)
		{
			_tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
			_otherTempDirectory = Path.Combine (_tempDirectory, "in");
			if (!Directory.Exists (_otherTempDirectory)) {
				Directory.CreateDirectory (_otherTempDirectory);
			}

			string filepath = Path.Combine (_tempDirectory, filename);
			
			StreamWriter writer = new StreamWriter(filepath,false);

			writer.Write (filecontents);
			writer.Close ();

			return filepath;
		}

	}

}
#endif
