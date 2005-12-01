//
// System.Xml.XmlSerializerTestClasses
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//
// Classes to use in the testing of the XmlSerializer
//

using System;
using System.ComponentModel;
using System.Collections;
using System.Xml.Serialization;
using System.Xml;

namespace MonoTests.System.Xml.TestClasses
{
	public enum SimpleEnumeration { FIRST, SECOND };

	[Flags]
	public enum EnumDefaultValue { e1 = 1, e2 = 2, e3 = 3 }
	public enum EnumDefaultValueNF { e1 = 1, e2 = 2, e3 = 3 }
	
	public class SimpleClass
	{
		public string something = null;
	}

	public class StringCollection : CollectionBase
	{
		public void Add (String parameter) 
		{
			List.Insert (Count, parameter);
		}
			
		public String this [int index]
		{
			get
			{ 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
					
				return (String) List [index]; 
			}
			set { List [index] = value; }
		}
	}
	
	public class StringCollectionContainer
	{
		StringCollection messages = new StringCollection();
		
		public StringCollection Messages
		{
			get { return messages; }
		}
	}

	public class ArrayContainer
	{
		public object [] items = null;
	}
	
	public class ClassArrayContainer
	{
		public SimpleClass [] items = null;
	}
	
	[XmlRoot("simple")]
	public class SimpleClassWithXmlAttributes
	{
		[XmlAttribute("member")]
		public string something = null;
	}
	
	[XmlRoot("field")]
	public class Field
	{
		[XmlAttribute("modifiers")]
		public MapModifiers Modifiers;
	}

	[Flags]
	public enum MapModifiers
	{
		[XmlEnum("public")]
		Public = 0,
		[XmlEnum("protected")]
		Protected = 1,
	}
	
	public class MyList : ArrayList
	{
		object container;
		
		// NOTE: MyList has no public constructor
		public MyList (object container) : base()
		{
			this.container = container;
		}
	}
	
	public class Container
	{
		public MyList Items;
		
		public Container () {
			Items = new MyList(this);
		}
	}
	
	public class Container2
	{
		public MyList Items;
		
		public Container2 () {
		}
		
		public Container2 (bool b) {
			Items = new MyList(this);
		}
	}

	public class MyElem: XmlElement
	{
		public MyElem (XmlDocument doc): base ("","myelem","", doc)
		{
			SetAttribute ("aa","1");
		}

		[XmlAttribute]
		public int kk=1;
	}

	public class MyDocument: XmlDocument
	{
		public MyDocument ()
		{
		}

		[XmlAttribute]
		public int kk=1;
	}
	
	public class CDataContainer
	{
		public XmlCDataSection cdata;
	}
	
	public class NodeContainer
	{
		public XmlNode node;
	}
	
	public class Choices
	{
		[XmlElementAttribute("ChoiceZero", typeof(string), IsNullable=false)]
		[XmlElementAttribute("ChoiceOne", typeof(string), IsNullable=false)]
		[XmlElementAttribute("ChoiceTwo", typeof(string), IsNullable=false)]
		[XmlChoiceIdentifier("ItemType")]
		public string MyChoice;

		[XmlIgnore]
		public ItemChoiceType ItemType;
	}
	
	[XmlType(IncludeInSchema = false)]
	public enum ItemChoiceType
	{
		ChoiceZero,
		[XmlEnum ("ChoiceOne")]
		StrangeOne,
		ChoiceTwo,
	}
	
	public class WrongChoices
	{
		[XmlElementAttribute("ChoiceZero", typeof(string), IsNullable=false)]
		[XmlElementAttribute("StrangeOne", typeof(string), IsNullable=false)]
		[XmlElementAttribute("ChoiceTwo", typeof(string), IsNullable=false)]
		[XmlChoiceIdentifier("ItemType")]
		public string MyChoice;

		[XmlIgnore]
		public ItemChoiceType ItemType;
	}
	
	[XmlType ("Type with space")]
	public class TestSpace
	{
	   [XmlElement (ElementName = "Element with space")]
	   public int elem;
	    
	   [XmlAttribute (AttributeName = "Attribute with space")]
	   public int attr; 
	}

	[Serializable]
	public class ReadOnlyProperties {
		string[] strArr = new string[2] { "string1", "string2" };

		public string[] StrArr {
			get { return strArr; }
		}
		
		public string dat {
			get { return "fff"; }
		} 
	}
	
	[XmlRoot("root")]
	public class ListDefaults
	{
		public ListDefaults ()
		{
			ed = new SimpleClass ();
			str = "hola";
		}
		
	    public ArrayList list2;
	    
	    public MyList list3;
	    
	    public string[] list4;
	    
		[XmlElement("e", typeof(SimpleClass))]
	    public ArrayList list5;
	    
		[DefaultValue (null)]
	    public SimpleClass ed;
	    
		[DefaultValue (null)]
	    public string str; 
	}
	
	public class clsPerson
	{
		public IList EmailAccounts;
	}
	
	public class ArrayClass
	{
		public object names = new object[] { "un","dos" };
	}
	
	public class CompositeValueType
	{
		public void Init ()
		{
	   		Items = new object[] { 1, 2 };
	   		ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.In, ItemsChoiceType.Es };
		}
	   
		[XmlElementAttribute("Es", typeof(int))]
		[XmlElementAttribute("In", typeof(int))]
		[XmlChoiceIdentifierAttribute("ItemsElementName")]
		public object[] Items;
	   
		[XmlElementAttribute("ItemsElementName")]
		[XmlIgnoreAttribute()]
		public ItemsChoiceType[] ItemsElementName;
	}

	public enum ItemsChoiceType {
	   In, Es
	}
	
	public class ArrayAttributeWithType
	{
		[XmlAttribute (DataType="anyURI")]
		public string[] at = new string [] { "a","b" };

		[XmlAttribute (DataType="base64Binary")]
		public byte[][] bin1 = new byte[][] { new byte[]{1,2},  new byte[]{1,2}};
		
		[XmlAttribute (DataType="base64Binary")]
		public byte[] bin2 = new byte[] { 1,2 };
	}
	
	public class ArrayAttributeWithWrongType
	{
		[XmlAttribute (DataType="int")]
		public string[] at = new string [] { "a","b" };
	}
}

