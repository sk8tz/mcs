//
// System.Web.UI.WebControls.XmlHierarchyData
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using System.ComponentModel;
using AttributeCollection = System.ComponentModel.AttributeCollection;

namespace System.Web.UI.WebControls {
	public class XmlHierarchyData : IHierarchyData, ICustomTypeDescriptor {
		internal XmlHierarchyData (XmlNode item)
		{
			this.item = item;
		}
		
		public override string ToString ()
		{
			return item.Name;
		}
		
		#region ICustomTypeDescriptor
		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			return AttributeCollection.Empty;
		}
		
		string ICustomTypeDescriptor.GetClassName ()
		{
			return "XmlHierarchyData";
		}
		
		string ICustomTypeDescriptor.GetComponentName ()
		{
			return null;
		}
		
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			return null;
		}
		
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			return null;
		}
		
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			return new XmlHierarchyDataPropertyDescriptor (item, "##Name##");
		}
		
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			return null;
		}
		
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			return null;
		}
		
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute [] attrs)
		{
			return null;
		}
		
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			return ((ICustomTypeDescriptor)this).GetProperties (null);
		}
		
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute [] attrFilter)
		{
			ArrayList ret = new ArrayList ();
			ret.Add (new XmlHierarchyDataPropertyDescriptor (item, "##Name##"));
			ret.Add (new XmlHierarchyDataPropertyDescriptor (item, "##Value##"));
			ret.Add (new XmlHierarchyDataPropertyDescriptor (item, "##InnerText##"));
			
			if (item.Attributes != null)
				foreach (XmlAttribute a in item.Attributes)
					ret.Add (new XmlHierarchyDataPropertyDescriptor (item, a.Name));
			
			return new PropertyDescriptorCollection ((PropertyDescriptor[]) ret.ToArray (typeof (PropertyDescriptor)));
		}
		
		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			if (pd is XmlHierarchyDataPropertyDescriptor)
				return this;
			return null;
		}
		#endregion
		
		#region IHierarchyData
		IHierarchicalEnumerable IHierarchyData.GetChildren ()
		{
			return new XmlHierarchicalEnumerable (item.ChildNodes);
		}
		
		IHierarchicalEnumerable IHierarchyData.GetParent ()
		{
			if (item.ParentNode == null)
				return null;
			return new XmlHierarchicalEnumerable (item.ParentNode.ChildNodes);
		}
		
		bool IHierarchyData.HasChildren {
			get { return item.HasChildNodes; }
		}
		
		object IHierarchyData.Item {
			get { return item; }
		}
		
		[MonoTODO]
		string IHierarchyData.Path {
			get { throw new NotImplementedException (); }
		}
		
		string IHierarchyData.Type {
			get { return item.Name; }
		}
		#endregion
			
		XmlNode item;
		
		class XmlHierarchyDataPropertyDescriptor : PropertyDescriptor {
			public XmlHierarchyDataPropertyDescriptor (XmlNode xmlNode, string name) : base (name, null)
			{
				this.xmlNode = xmlNode;
				this.name = name;
			}
			
			public override bool CanResetValue (object o)
			{
				return false;
			}
			
			public override void ResetValue (object o)
			{
			}
			
			public override object GetValue (object o)
			{
				if (o is XmlHierarchyData) {
					switch (name) {
						case "##Name##": return xmlNode.Name;
						case "##Value##": return xmlNode.Value;
						case "##InnerText##": return xmlNode.InnerText;
						case null: return String.Empty;
						default:
							if (xmlNode.Attributes != null) {
								XmlAttribute a = xmlNode.Attributes [name];
								if (a != null)
									return a.Value;
							}
							break;
					}
				}
				return String.Empty;
			}
			
			public override void SetValue (object o, object value)
			{
				if (o is XmlHierarchyData) {
					switch (name) {
						case "##Name##": break;
						case "##Value##": xmlNode.Value = value.ToString (); break;
						case "##InnerText##": xmlNode.InnerText = value.ToString (); break;
						case null: break;
						default:
							if (xmlNode.Attributes != null) {
								XmlAttribute a = xmlNode.Attributes [name];
								if (a != null)
									a.Value = value.ToString ();
							}
							break;
					}
				}
			}
			
			public override bool ShouldSerializeValue (object o)
			{
				return o is XmlNode;
			}
			
			public override Type ComponentType {
				get { return typeof (XmlHierarchyData); }
			}
			
			public override bool IsReadOnly {
				get { return xmlNode.IsReadOnly; }
			}
			
			public override Type PropertyType {
				get { return typeof (string); }
			}
			
			string name;
			XmlNode xmlNode;
		}
	
	}
	
}
#endif

