//
// System.ComponentModel.TypeDescriptorTests test cases
//
// Authors:
// 	Lluis Sanchez Gual (lluis@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.ximian.com)
//
using NUnit.Framework;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;

namespace MonoTests.System.ComponentModel
{
	class MyDesigner: IDesigner
	{
		public MyDesigner()
		{
		}

		public IComponent Component {get{return null; }}

		public DesignerVerbCollection Verbs {get{return null; }}

		public void DoDefaultAction () { }

		public void Initialize (IComponent component) { }

		public void Dispose () { }
	}

	class MyOtherDesigner: IDesigner
	{
		public MyOtherDesigner()
		{
		}

		public IComponent Component {get {return null; } }
		public DesignerVerbCollection Verbs { get {return null; } }
		public void DoDefaultAction () { }
		public void Initialize (IComponent component) { }
		public void Dispose () { }
	}
	
	class MySite: ISite
	{ 
		public IComponent Component { get {  return null; } }

		public IContainer Container { get {  return null; } }

		public bool DesignMode { get {  return true; } }

		public string Name { get { return "TestName"; } set { } }	

		public object GetService (Type t)
		{
			if (t == typeof(ITypeDescriptorFilterService)) return new MyFilter ();
			return null;
		}
	}
	
	class MyFilter: ITypeDescriptorFilterService
	{
		public bool FilterAttributes (IComponent component,IDictionary attributes)
		{
			Attribute ea = new DefaultEventAttribute ("AnEvent");
			attributes [ea.TypeId] = ea;
			ea = new DefaultPropertyAttribute ("TestProperty");
			attributes [ea.TypeId] = ea;
			ea = new EditorAttribute ();
			attributes [ea.TypeId] = ea;
			return true;
		}
		
		public bool FilterEvents (IComponent component, IDictionary events)
		{
			events.Remove ("AnEvent");
			return true;
		}
		
		public bool FilterProperties (IComponent component, IDictionary properties)
		{
			properties.Remove ("TestProperty");
			return true;
		}
	}

	class AnotherSite: ISite
	{ 
		public IComponent Component { get {  return null; } }

		public IContainer Container { get {  return null; } }

		public bool DesignMode { get {  return true; } }

		public string Name { get { return "TestName"; } set { } }

		public object GetService (Type t)
		{
			if (t == typeof(ITypeDescriptorFilterService)) {
				return new AnotherFilter ();
			}
			return null;
		}
	}

	class AnotherFilter: ITypeDescriptorFilterService
	{
		public bool FilterAttributes (IComponent component,IDictionary attributes) {
			Attribute ea = new DefaultEventAttribute ("AnEvent");
			attributes [ea.TypeId] = ea;
			ea = new DefaultPropertyAttribute ("TestProperty");
			attributes [ea.TypeId] = ea;
			ea = new EditorAttribute ();
			attributes [ea.TypeId] = ea;
			return true;
		}

		public bool FilterEvents (IComponent component, IDictionary events) {
			return true;
		}

		public bool FilterProperties (IComponent component, IDictionary properties) {
			return true;
		}
	}

	[DescriptionAttribute ("my test component")]
	[DesignerAttribute (typeof(MyDesigner), typeof(int))]
	public class MyComponent: Component
	{
		string prop;
		
		[DescriptionAttribute ("test")]
		public event EventHandler AnEvent;
		
		public event EventHandler AnotherEvent;
		
		public MyComponent  ()
		{
		}
		
		public MyComponent (ISite site)
		{
			Site = site;
		}
		
		[DescriptionAttribute ("test")]
		public virtual string TestProperty
		{
			get { return prop; }
			set { prop = value; }
		}
		
		public string AnotherProperty
		{
			get { return prop; }
			set { prop = value; }
		}
	}

	[DescriptionAttribute ("my test derived component")]
	[DesignerAttribute (typeof(MyOtherDesigner))]
	public class MyDerivedComponent: MyComponent
	{
		string prop;
		
		public MyDerivedComponent  ()
		{
		}
		
		public MyDerivedComponent (ISite site) : base (site)
		{
		}
		
		[DescriptionAttribute ("test derived")]
		public override string TestProperty
		{
			get { return prop; }
			set { prop = value; }
		}
	}
	

	[DefaultProperty("AnotherProperty")]
	[DefaultEvent("AnotherEvent")]
	[DescriptionAttribute ("my test component")]
	[DesignerAttribute (typeof(MyDesigner), typeof(int))]
	public class AnotherComponent: Component {
		string prop;
		
		[DescriptionAttribute ("test")]
		public event EventHandler AnEvent;
		
		public event EventHandler AnotherEvent;
		
		public AnotherComponent () {
		}
		
		public AnotherComponent (ISite site) {
			Site = site;
		}
		
		[DescriptionAttribute ("test")]
		public string TestProperty {
			get { return prop; }
			set { prop = value; }
		}
		
		public string AnotherProperty {
			get { return prop; }
			set { prop = value; }
		}
	}

	public interface ITestInterface
	{
		void TestFunction ();
	}
	
	public class TestClass
	{
		public TestClass()
		{}
			
		void TestFunction ()
		{}
	}
	
	public struct TestStruct
	{
		public int TestVal;
	}

	public class TestCustomTypeDescriptor : ICustomTypeDescriptor
	{
		public string methods_called = "";

		public void ResetMethodsCalled ()
		{
			methods_called = "";
		}

		public TypeConverter GetConverter()
		{
			return new StringConverter();
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			methods_called += "1";
			return null;
		}

		public EventDescriptorCollection GetEvents()
		{
			methods_called += "2";
			return null;
		}

		public string GetComponentName()
		{
			return "MyComponentnName";
		}

		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}

		public AttributeCollection GetAttributes()
		{
			methods_called += "3";
			return null;
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			methods_called += "4";
			return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		}

		public PropertyDescriptorCollection GetProperties()
		{
			methods_called += "5";
			return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		}

		public object GetEditor(Type editorBaseType)
		{
			return null;
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			methods_called += "6";
			return null;
		}

		public EventDescriptor GetDefaultEvent()
		{
			methods_called += "7";
			return null;
		}

		public string GetClassName()
		{
			return this.GetType().Name;
		}
	}

	[TestFixture]
	public class TypeDescriptorTests: Assertion
	{
		MyComponent com = new MyComponent ();
		MyComponent sitedcom = new MyComponent (new MySite ());
		AnotherComponent anothercom = new AnotherComponent ();
		
		[Test]
		public void TestICustomTypeDescriptor ()
		{
			TestCustomTypeDescriptor test = new TestCustomTypeDescriptor ();

			PropertyDescriptorCollection props;
			PropertyDescriptor prop;
			EventDescriptorCollection events;

			test.ResetMethodsCalled ();
			props = TypeDescriptor.GetProperties (test);
			AssertEquals ("t1", "5", test.methods_called);

			test.ResetMethodsCalled ();
			props = TypeDescriptor.GetProperties (test, new Attribute[0]);
			AssertEquals ("t2", "4", test.methods_called);

			test.ResetMethodsCalled ();
			props = TypeDescriptor.GetProperties (test, new Attribute[0], false);
			AssertEquals ("t3", "4", test.methods_called);

			test.ResetMethodsCalled ();
			props = TypeDescriptor.GetProperties (test, false);
			AssertEquals ("t4", "5", test.methods_called);

			test.ResetMethodsCalled ();
			prop = TypeDescriptor.GetDefaultProperty (test);
			AssertEquals ("t5", "6", test.methods_called);

			test.ResetMethodsCalled ();
			events = TypeDescriptor.GetEvents (test);
			AssertEquals ("t6", "2", test.methods_called);

			test.ResetMethodsCalled ();
			events = TypeDescriptor.GetEvents (test, new Attribute[0]);
			AssertEquals ("t7", "1", test.methods_called);

			test.ResetMethodsCalled ();
			events = TypeDescriptor.GetEvents (test, false);
			AssertEquals ("t8", "2", test.methods_called);
		}

		[Test]
		public void TestCreateDesigner ()
		{
			IDesigner des = TypeDescriptor.CreateDesigner (com, typeof(int));
			Assert ("t1", des is MyDesigner);
			
			des = TypeDescriptor.CreateDesigner (com, typeof(string));
			AssertNull ("t2", des);
		}
		
		[Test]
		public void TestCreateEvent ()
		{
			EventDescriptor ed = TypeDescriptor.CreateEvent (typeof(MyComponent), "AnEvent", typeof(EventHandler), null);
			AssertEquals ("t1", typeof(MyComponent), ed.ComponentType);
			AssertEquals ("t2", typeof(EventHandler), ed.EventType);
			AssertEquals ("t3", true, ed.IsMulticast);
			AssertEquals ("t4", "AnEvent", ed.Name);
		}
		
		[Test]
		public void TestCreateProperty ()
		{
			PropertyDescriptor pd = TypeDescriptor.CreateProperty (typeof(MyComponent), "TestProperty", typeof(string), null);
			AssertEquals ("t1", typeof(MyComponent), pd.ComponentType);
			AssertEquals ("t2", "TestProperty", pd.Name);
			AssertEquals ("t3", typeof(string), pd.PropertyType);
			AssertEquals ("t4", false, pd.IsReadOnly);
			
			pd.SetValue (com, "hi");
			AssertEquals ("t5", "hi", pd.GetValue(com));
		}
		
		[Test]
		public void TestGetAttributes ()
		{
			AttributeCollection col = TypeDescriptor.GetAttributes (typeof(MyComponent));
			Assert ("t2", col[typeof(DescriptionAttribute)] != null);
			Assert ("t3", col[typeof(DesignerAttribute)] != null);
			Assert ("t4", col[typeof(EditorAttribute)] == null);
			
			col = TypeDescriptor.GetAttributes (com);
			Assert ("t6", col[typeof(DescriptionAttribute)] != null);
			Assert ("t7", col[typeof(DesignerAttribute)] != null);
			Assert ("t8", col[typeof(EditorAttribute)] == null);
			
			col = TypeDescriptor.GetAttributes (sitedcom);
			Assert ("t10", col[typeof(DescriptionAttribute)] != null);
			Assert ("t11", col[typeof(DesignerAttribute)] != null);
			Assert ("t12", col[typeof(EditorAttribute)] != null);

			col = TypeDescriptor.GetAttributes (typeof (MyDerivedComponent));
			Assert ("t13", col[typeof(DesignerAttribute)] != null);
			Assert ("t14", col[typeof(DescriptionAttribute)] != null);
			DesignerAttribute attribute = col[typeof(DesignerAttribute)] as DesignerAttribute;
			Assert ("t15", attribute != null);
			// there are multiple DesignerAttribute present and their order in the collection isn't deterministic
			bool found = false;
			for (int i = 0; i < col.Count; i++) {
				attribute = (col [i] as DesignerAttribute);
				if (attribute != null) {
					found = typeof(MyOtherDesigner).AssemblyQualifiedName == attribute.DesignerTypeName;
					if (found)
						break;
				}
			}
			Assert ("t16", found);
		}
		
		[Test]
		public void TestGetClassName ()
		{
			AssertEquals ("t1", typeof(MyComponent).FullName, TypeDescriptor.GetClassName (com));
		}
		
		[Test]
		public void TestGetComponentName ()
		{
#if !NET_2_0
			AssertEquals ("t1", "MyComponent", TypeDescriptor.GetComponentName (com));
			AssertEquals ("t2", "MyComponent", TypeDescriptor.GetComponentName (com, false));
			AssertEquals ("t3", "Exception", TypeDescriptor.GetComponentName (new Exception ()));
			AssertEquals ("t4", "Exception", TypeDescriptor.GetComponentName (new Exception (), false));
			AssertNotNull ("t5", TypeDescriptor.GetComponentName (typeof (Exception)));
			AssertNotNull ("t6", TypeDescriptor.GetComponentName (typeof (Exception), false));
#else
			// in MS.NET 2.0, GetComponentName no longer returns
			// the type name if there's no custom typedescriptor
			// and no site
			AssertNull ("t1", TypeDescriptor.GetComponentName (com));
			AssertNull ("t2", TypeDescriptor.GetComponentName (com, false));
			AssertNull ("t3", TypeDescriptor.GetComponentName (new Exception ()));
			AssertNull ("t4", TypeDescriptor.GetComponentName (new Exception (), false));
			AssertNull ("t5", TypeDescriptor.GetComponentName (typeof (Exception)));
			AssertNull ("t6", TypeDescriptor.GetComponentName (typeof (Exception), false));
#endif
			AssertEquals ("t7", "TestName", TypeDescriptor.GetComponentName (sitedcom));
			AssertEquals ("t8", "TestName", TypeDescriptor.GetComponentName (sitedcom));
		}
		
		[Test]
		public void TestGetConverter ()
		{
			AssertEquals (typeof(BooleanConverter), TypeDescriptor.GetConverter (typeof (bool)).GetType());
			AssertEquals (typeof(ByteConverter), TypeDescriptor.GetConverter (typeof (byte)).GetType());
			AssertEquals (typeof(SByteConverter), TypeDescriptor.GetConverter (typeof (sbyte)).GetType());
			AssertEquals (typeof(StringConverter), TypeDescriptor.GetConverter (typeof (string)).GetType());
			AssertEquals (typeof(CharConverter), TypeDescriptor.GetConverter (typeof (char)).GetType());
			AssertEquals (typeof(Int16Converter), TypeDescriptor.GetConverter (typeof (short)).GetType());
			AssertEquals (typeof(Int32Converter), TypeDescriptor.GetConverter (typeof (int)).GetType());
			AssertEquals (typeof(Int64Converter), TypeDescriptor.GetConverter (typeof (long)).GetType());
			AssertEquals (typeof(UInt16Converter), TypeDescriptor.GetConverter (typeof (ushort)).GetType());
			AssertEquals (typeof(UInt32Converter), TypeDescriptor.GetConverter (typeof (uint)).GetType());
			AssertEquals (typeof(UInt64Converter), TypeDescriptor.GetConverter (typeof (ulong)).GetType());
			AssertEquals (typeof(SingleConverter), TypeDescriptor.GetConverter (typeof (float)).GetType());
			AssertEquals (typeof(DoubleConverter), TypeDescriptor.GetConverter (typeof (double)).GetType());
			AssertEquals (typeof(DecimalConverter), TypeDescriptor.GetConverter (typeof (decimal)).GetType());
			AssertEquals (typeof(ArrayConverter), TypeDescriptor.GetConverter (typeof (Array)).GetType());
			AssertEquals (typeof(CultureInfoConverter), TypeDescriptor.GetConverter (typeof (CultureInfo)).GetType());
			AssertEquals (typeof(DateTimeConverter), TypeDescriptor.GetConverter (typeof (DateTime)).GetType());
			AssertEquals (typeof(GuidConverter), TypeDescriptor.GetConverter (typeof (Guid)).GetType());
			AssertEquals (typeof(TimeSpanConverter), TypeDescriptor.GetConverter (typeof (TimeSpan)).GetType());
			AssertEquals (typeof(CollectionConverter), TypeDescriptor.GetConverter (typeof (ICollection)).GetType());

			// Tests from bug #71444
			AssertEquals (typeof(CollectionConverter), TypeDescriptor.GetConverter (typeof (IDictionary)).GetType());
			AssertEquals (typeof(ReferenceConverter), TypeDescriptor.GetConverter (typeof (ITestInterface)).GetType());
			AssertEquals (typeof(TypeConverter), TypeDescriptor.GetConverter (typeof (TestClass)).GetType());
			AssertEquals (typeof(TypeConverter), TypeDescriptor.GetConverter (typeof (TestStruct)).GetType());
			
			AssertEquals (typeof(TypeConverter), TypeDescriptor.GetConverter (new TestClass ()).GetType());
			AssertEquals (typeof(TypeConverter), TypeDescriptor.GetConverter (new TestStruct ()).GetType());
			AssertEquals (typeof(CollectionConverter), TypeDescriptor.GetConverter (new Hashtable ()).GetType());

#if NET_2_0
			// Test from bug #76686
			AssertEquals (typeof (Int32Converter), TypeDescriptor.GetConverter ((int?) 1).GetType ());
#endif
		}
		
		[Test]
		public void TestGetDefaultEvent ()
		{
			EventDescriptor des = TypeDescriptor.GetDefaultEvent (typeof(MyComponent));
			AssertNull ("t1", des);
			
			des = TypeDescriptor.GetDefaultEvent (com);
			AssertNull ("t2", des);

			des = TypeDescriptor.GetDefaultEvent (typeof(AnotherComponent));
			AssertNotNull ("t3", des);
			AssertEquals ("t4", "AnotherEvent", des.Name);

			des = TypeDescriptor.GetDefaultEvent (anothercom);
			AssertNotNull ("t5", des);
			AssertEquals ("t6", "AnotherEvent", des.Name);

			des = TypeDescriptor.GetDefaultEvent (sitedcom);
#if NET_2_0
			AssertNull ("t7", des);
#else
			AssertNotNull ("t7/1", des);
			AssertEquals ("t7/2", "AnotherEvent", des.Name);
#endif

			des = TypeDescriptor.GetDefaultEvent (new MyComponent(new AnotherSite ()));
			AssertNotNull ("t8", des);
			AssertEquals ("t9", "AnEvent", des.Name);

			des = TypeDescriptor.GetDefaultEvent (new AnotherComponent(new AnotherSite ()));
			AssertNotNull ("t10", des);
			AssertEquals ("t11", "AnEvent", des.Name);
		}
		
		[Test]
		public void TestGetDefaultProperty ()
		{
			PropertyDescriptor des = TypeDescriptor.GetDefaultProperty (typeof(MyComponent));
			AssertNull ("t1", des);
			
			des = TypeDescriptor.GetDefaultProperty (com);
			AssertNull ("t2", des);

			des = TypeDescriptor.GetDefaultProperty (typeof(AnotherComponent));
			AssertNotNull ("t1", des);
			AssertEquals ("t2", "AnotherProperty", des.Name);

			des = TypeDescriptor.GetDefaultProperty (anothercom);
			AssertNotNull ("t1", des);
			AssertEquals ("t2", "AnotherProperty", des.Name);
		}
		
		[Test]
#if !NET_2_0
		// throws NullReferenceException on MS.NET 1.x due to bug
		// which is fixed in MS.NET 2.0
		[NUnit.Framework.Category("NotDotNet")]
#endif
		public void TestGetDefaultProperty2 ()
		{
			PropertyDescriptor des = TypeDescriptor.GetDefaultProperty (sitedcom);
			AssertNull ("t1", des);

			des = TypeDescriptor.GetDefaultProperty (new MyComponent (new AnotherSite ()));
			AssertNotNull ("t2", des);
			AssertEquals ("t3", "TestProperty", des.Name);

			des = TypeDescriptor.GetDefaultProperty (new AnotherComponent (new AnotherSite ()));
			AssertNotNull ("t4", des);
			AssertEquals ("t5", "TestProperty", des.Name);

			des = TypeDescriptor.GetDefaultProperty (new AnotherComponent (new MySite ()));
			AssertNull ("t6", des);
		}

		[Test]
		public void TestGetEvents ()
		{
			EventDescriptorCollection col = TypeDescriptor.GetEvents (typeof(MyComponent));
				
			AssertEquals ("t1.1", 3, col.Count);
			Assert ("t1.2", col.Find ("AnEvent", true) != null);
			Assert ("t1.3", col.Find ("AnotherEvent", true) != null);
			Assert ("t1.4", col.Find ("Disposed", true) != null);
			
			col = TypeDescriptor.GetEvents (com);
			AssertEquals ("t2.1", 3, col.Count);
			Assert ("t2.2", col.Find ("AnEvent", true) != null);
			Assert ("t2.3", col.Find ("AnotherEvent", true) != null);
			Assert ("t2.4", col.Find ("Disposed", true) != null);
			
			col = TypeDescriptor.GetEvents (sitedcom);
			AssertEquals ("t3.1", 2, col.Count);
			Assert ("t3.2", col.Find ("AnotherEvent", true) != null);
			Assert ("t3.3", col.Find ("Disposed", true) != null);
			
			Attribute[] filter = new Attribute[] { new DescriptionAttribute ("test") };
			
			col = TypeDescriptor.GetEvents (typeof(MyComponent), filter);
			AssertEquals ("t4.1", 1, col.Count);
			Assert ("t4.2", col.Find ("AnEvent", true) != null);
			
			col = TypeDescriptor.GetEvents (com, filter);
			AssertEquals ("t5.1", 1, col.Count);
			Assert ("t5.2", col.Find ("AnEvent", true) != null);
			
			col = TypeDescriptor.GetEvents (sitedcom, filter);
			AssertEquals ("t6", 0, col.Count);
		}
		
		[Test]
		public void TestGetProperties ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof(MyComponent));
			Assert ("t1.1", col.Find ("TestProperty", true) != null);
			Assert ("t1.2", col.Find ("AnotherProperty", true) != null);
			
			col = TypeDescriptor.GetProperties (com);
			Assert ("t2.1", col.Find ("TestProperty", true) != null);
			Assert ("t2.2", col.Find ("AnotherProperty", true) != null);
			
			Attribute[] filter = new Attribute[] { new DescriptionAttribute ("test") };
			
			col = TypeDescriptor.GetProperties (typeof(MyComponent), filter);
			Assert ("t4.1", col.Find ("TestProperty", true) != null);
			Assert ("t4.2", col.Find ("AnotherProperty", true) == null);
			
			col = TypeDescriptor.GetProperties (com, filter);
			Assert ("t5.1", col.Find ("TestProperty", true) != null);
			Assert ("t5.2", col.Find ("AnotherProperty", true) == null);
			
		}

		[Test]
#if !NET_2_0
		// throws NullReferenceException on MS.NET 1.x due to bug
		// which is fixed in MS.NET 2.0
		[NUnit.Framework.Category("NotDotNet")]
#endif
		public void TestGetProperties2 ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (sitedcom);
			Assert ("t3.1", col.Find ("TestProperty", true) == null);
			Assert ("t3.2", col.Find ("AnotherProperty", true) != null);

			Attribute[] filter = new Attribute[] { new DescriptionAttribute ("test") };
			col = TypeDescriptor.GetProperties (sitedcom, filter);
			Assert ("t6.1", col.Find ("TestProperty", true) == null);
			Assert ("t6.2", col.Find ("AnotherProperty", true) == null);
		}

		[TypeConverter (typeof (TestConverter))]
		class TestConverterClass {
		}

		class TestConverter : TypeConverter {
			public Type Type;

			public TestConverter (Type type)
			{
				this.Type = type;
			}
		}

		[Test]
		public void TestConverterCtorWithArgument ()
		{
			TypeConverter t = TypeDescriptor.GetConverter (typeof (TestConverterClass));
			Assert ("#01", null != t.GetType ());
			AssertEquals ("#02", typeof (TestConverter), t.GetType ());
			TestConverter converter = (TestConverter) t;
			AssertEquals ("#03", typeof (TestConverterClass), converter.Type);
		}

		[Test]
		public void GetPropertiesIgnoreIndexers ()
		{
			PropertyDescriptorCollection pc =
				TypeDescriptor.GetProperties (typeof (string));
			// There are two string properties: Length and Chars.
			// Chars is an indexer.
			//
			// Future version of CLI might contain some additional
			// properties. In that case simply increase the
			// number. (Also, it is fine to just remove #2.)
			AssertEquals ("#1", 1, pc.Count);
			AssertEquals ("#2", "Length", pc [0].Name);
		}
	}
}

