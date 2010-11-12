//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class XamlXmlReaderTest : XamlReaderTestBase
	{
		// read test

		XamlReader GetReader (string filename)
		{
			return new XamlXmlReader (XmlReader.Create (Path.Combine ("Test/XmlFiles", filename), new XmlReaderSettings () { CloseInput =true }));
		}

		void ReadTest (string filename)
		{
			var r = GetReader (filename);
			while (!r.IsEof)
				r.Read ();
		}

		T LoadTest<T> (string filename)
		{
			Type type = typeof (T);
			var obj = XamlServices.Load (GetReader (filename));
			Assert.AreEqual (type, obj.GetType (), "type");
			return (T) obj;
		}

		[Test]
		public void SchemaContext ()
		{
			Assert.AreNotEqual (XamlLanguage.Type.SchemaContext, new XamlXmlReader (XmlReader.Create (new StringReader ("<root/>"))).SchemaContext, "#1");
		}

		[Test]
		public void Read_Int32 ()
		{
			ReadTest ("Int32.xml");
			var ret = LoadTest<int> ("Int32.xml");
			Assert.AreEqual (5, ret, "ret");
		}

		[Test]
		public void Read_DateTime ()
		{
			ReadTest ("DateTime.xml");
			var ret = LoadTest<DateTime> ("DateTime.xml");
			Assert.AreEqual (new DateTime (2010, 4, 14), ret, "ret");
		}

		[Test]
		public void Read_TimeSpan ()
		{
			ReadTest ("TimeSpan.xml");
			var ret = LoadTest<TimeSpan> ("TimeSpan.xml");
			Assert.AreEqual (TimeSpan.FromMinutes (7), ret, "ret");
		}

		[Test]
		public void Read_ArrayInt32 ()
		{
			ReadTest ("Array_Int32.xml");
			var ret = LoadTest<int[]> ("Array_Int32.xml");
			Assert.AreEqual (5, ret.Length, "#1");
			Assert.AreEqual (2147483647, ret [4], "#2");
		}

		[Test]
		public void Read_DictionaryInt32String ()
		{
			ReadTest ("Dictionary_Int32_String.xml");
			//LoadTest<Dictionary<int,string>> ("Dictionary_Int32_String.xml");
		}

		[Test]
		public void Read_DictionaryStringType ()
		{
			ReadTest ("Dictionary_String_Type.xml");
			//LoadTest<Dictionary<string,Type>> ("Dictionary_String_Type.xml");
		}

		[Test]
		public void Read_SilverlightApp1 ()
		{
			ReadTest ("SilverlightApp1.xaml");
		}

		[Test]
		public void Read_Guid ()
		{
			ReadTest ("Guid.xml");
			var ret = LoadTest<Guid> ("Guid.xml");
			Assert.AreEqual (Guid.Parse ("9c3345ec-8922-4662-8e8d-a4e41f47cf09"), ret, "ret");
		}

		[Test]
		public void Read_GuidFactoryMethod ()
		{
			ReadTest ("GuidFactoryMethod.xml");
			//var ret = LoadTest<Guid> ("GuidFactoryMethod.xml");
			//Assert.AreEqual (Guid.Parse ("9c3345ec-8922-4662-8e8d-a4e41f47cf09"), ret, "ret");
		}

		[Test]
		public void ReadInt32Details ()
		{
			var r = GetReader ("Int32.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns#3");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			Assert.AreEqual (XamlLanguage.Int32, r.Type, "so#3");

			ReadBase (r);

			Assert.IsTrue (r.Read (), "sinit#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sinit#2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "sinit#3");

			Assert.IsTrue (r.Read (), "vinit#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vinit#2");
			Assert.AreEqual ("5", r.Value, "vinit#3"); // string

			Assert.IsTrue (r.Read (), "einit#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "einit#2");

			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");

			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadDateTimeDetails ()
		{
			var r = GetReader ("DateTime.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual ("clr-namespace:System;assembly=mscorlib", r.Namespace.Namespace, "ns#3");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			Assert.AreEqual (r.SchemaContext.GetXamlType (typeof (DateTime)), r.Type, "so#3");

			ReadBase (r);

			Assert.IsTrue (r.Read (), "sinit#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sinit#2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "sinit#3");

			Assert.IsTrue (r.Read (), "vinit#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vinit#2");
			Assert.AreEqual ("2010-04-14", r.Value, "vinit#3"); // string

			Assert.IsTrue (r.Read (), "einit#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "einit#2");

			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");
			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadGuidFactoryMethodDetails ()
		{
			var r = GetReader ("GuidFactoryMethod.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual ("clr-namespace:System;assembly=mscorlib", r.Namespace.Namespace, "ns#3");
			Assert.AreEqual (String.Empty, r.Namespace.Prefix, "ns#4");

			Assert.IsTrue (r.Read (), "ns2#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns2#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns2#3");
			Assert.AreEqual ("x", r.Namespace.Prefix, "ns2#4");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			var xt = r.SchemaContext.GetXamlType (typeof (Guid));
			Assert.AreEqual (xt, r.Type, "so#3");

			ReadBase (r);

			Assert.IsTrue (r.Read (), "sfactory#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sfactory#2");
			Assert.AreEqual (XamlLanguage.FactoryMethod, r.Member, "sfactory#3");

			Assert.IsTrue (r.Read (), "vfactory#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vfactory#2");
			Assert.AreEqual ("Parse", r.Value, "vfactory#3"); // string

			Assert.IsTrue (r.Read (), "efactory#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "efactory#2");

			Assert.IsTrue (r.Read (), "sarg#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sarg#2");
			Assert.AreEqual (XamlLanguage.Arguments, r.Member, "sarg#3");

			Assert.IsTrue (r.Read (), "sarg1#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "sarg1#2");
			Assert.AreEqual (XamlLanguage.String, r.Type, "sarg1#3");

			Assert.IsTrue (r.Read (), "sInit#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sInit#2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "sInit#3");

			Assert.IsTrue (r.Read (), "varg1#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "varg1#2");
			Assert.AreEqual ("9c3345ec-8922-4662-8e8d-a4e41f47cf09", r.Value, "varg1#3");

			Assert.IsTrue (r.Read (), "eInit#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "eInit#2");

			Assert.IsTrue (r.Read (), "earg1#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "earg1#2");

			Assert.IsTrue (r.Read (), "earg#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "earg#2");


			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");

			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void Read_String ()
		{
			var r = GetReader ("String.xml");
			Read_String (r);
			var ret = LoadTest<string> ("String.xml");
			Assert.AreEqual ("foo", ret, "ret");
		}

		[Test]
		public void WriteNullMemberAsObject ()
		{
			var r = GetReader ("TestClass4.xml");
			WriteNullMemberAsObject (r, null);
		}
		
		[Test]
		public void StaticMember ()
		{
			var r = GetReader ("TestClass5.xml");
			StaticMember (r);
		}

		[Test]
		public void Skip ()
		{
			var r = GetReader ("String.xml");
			Skip (r);
		}
		
		[Test]
		public void Skip2 ()
		{
			var r = GetReader ("String.xml");
			Skip2 (r);
		}

		[Test]
		public void Read_XmlDocument ()
		{
			var doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns='urn:foo'><elem attr='val' /></root>");
			// note that corresponding XamlXmlWriter is untested yet.
			var r = GetReader ("XmlDocument.xml");
			Read_XmlDocument (r);
		}

		[Test]
		public void Read_NonPrimitive ()
		{
			var r = GetReader ("NonPrimitive.xml");
			Read_NonPrimitive (r);
		}
		
		[Test]
		public void Read_TypeExtension ()
		{
			var r = GetReader ("Type.xml");
			Read_TypeOrTypeExtension (r, null, XamlLanguage.Type.GetMember ("Type"));
		}
		
		[Test]
		public void Read_Type2 ()
		{
			var r = GetReader ("Type2.xml");
			Read_TypeOrTypeExtension2 (r, null, XamlLanguage.Type.GetMember ("Type"));
		}
		
		[Test]
		public void Read_Reference ()
		{
			var r = GetReader ("Reference.xml");
			Read_Reference (r);
			var ret = XamlServices.Load (GetReader ("Reference.xml"));
			Assert.IsNotNull (ret, "#1"); // the returned value is however not a Reference (in .NET 4.0 it is MS.Internal.Xaml.Context.NameFixupToken).
		}
		
		[Test]
		public void Read_Null ()
		{
			var r = GetReader ("NullExtension.xml");
			Read_NullOrNullExtension (r, null);
			Assert.IsNull (XamlServices.Load (GetReader ("NullExtension.xml")));
		}
		
		[Test]
		public void Read_StaticExtension ()
		{
			var r = GetReader ("StaticExtension.xml");
			Read_StaticExtension (r, XamlLanguage.Static.GetMember ("Member"));
		}
		
		[Test]
		public void Read_ListInt32 ()
		{
			var r = GetReader ("List_Int32.xml");
			Read_ListInt32 (r, null, new int [] {5, -3, int.MaxValue, 0}.ToList ());
			var ret = LoadTest<List<int>> ("List_Int32.xml");
			Assert.AreEqual (4, ret.Count, "#1");
			Assert.AreEqual (2147483647, ret [2], "#2");
		}
		
		[Test]
		public void Read_ListInt32_2 ()
		{
			var r = GetReader ("List_Int32_2.xml");
			Read_ListInt32 (r, null, new int [0].ToList ());
		}

		[Test]
		public void Read_ArrayList ()
		{
			var r = GetReader ("ArrayList.xml");
			Read_ArrayList (r);
		}
		
		[Test]
		public void Read_Array ()
		{
			var r = GetReader ("ArrayExtension.xml");
			Read_ArrayOrArrayExtensionOrMyArrayExtension (r, null, typeof (ArrayExtension));
		}
		
		[Test]
		public void Read_MyArrayExtension ()
		{
			var r = GetReader ("MyArrayExtension.xml");
			Read_ArrayOrArrayExtensionOrMyArrayExtension (r, null, typeof (MyArrayExtension));
		}

		[Test]
		public void Read_ArrayExtension2 ()
		{
			var r = GetReader ("ArrayExtension2.xml");
			Read_ArrayExtension2 (r);
		}

		[Test]
		public void Read_CustomMarkupExtension ()
		{
			var r = GetReader ("MyExtension.xml");
			Read_CustomMarkupExtension (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension2 ()
		{
			var r = GetReader ("MyExtension2.xml");
			Read_CustomMarkupExtension2 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension3 ()
		{
			var r = GetReader ("MyExtension3.xml");
			Read_CustomMarkupExtension3 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension4 ()
		{
			var r = GetReader ("MyExtension4.xml");
			Read_CustomMarkupExtension4 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension6 ()
		{
			var r = GetReader ("MyExtension6.xml");
			Read_CustomMarkupExtension6 (r);
		}

		[Test]
		[Category ("NotWorking")]
		public void Read_Dictionary ()
		{
			var obj = new Dictionary<string,object> ();
			obj ["Foo"] = 5.0;
			obj ["Bar"] = -6.5;
			var r = GetReader ("Dictionary_String_Double.xml");
			Read_Dictionary (r);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void Read_Dictionary2 ()
		{
			var obj = new Dictionary<string,Type> ();
			obj ["Foo"] = typeof (int);
			obj ["Bar"] = typeof (Dictionary<Type,XamlType>);
			var r = GetReader ("Dictionary_String_Type_2.xml");
			Read_Dictionary2 (r, XamlLanguage.Type.GetMember ("Type"));
		}
		
		[Test]
		[Category ("NotWorking")]
		public void PositionalParameters2 ()
		{
			var r = GetReader ("PositionalParametersWrapper.xml");
			PositionalParameters2 (r);
		}

		[Test]
		public void ComplexPositionalParameters ()
		{
			var r = GetReader ("ComplexPositionalParameterWrapper.xml");
			ComplexPositionalParameters (r);
		}
		
		[Test]
		public void Read_ListWrapper ()
		{
			var r = GetReader ("ListWrapper.xml");
			Read_ListWrapper (r);
		}
		
		[Test]
		public void Read_ListWrapper2 () // read-write list member.
		{
			var r = GetReader ("ListWrapper2.xml");
			Read_ListWrapper2 (r);
		}

		[Test]
		public void Read_ContentIncluded ()
		{
			var r = GetReader ("ContentIncluded.xml");
			Read_ContentIncluded (r);
		}

		[Test]
		public void Read_PropertyDefinition ()
		{
			var r = GetReader ("PropertyDefinition.xml");
			Read_PropertyDefinition (r);
		}
	}
}
