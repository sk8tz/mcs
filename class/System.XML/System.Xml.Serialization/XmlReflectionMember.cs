// 
// System.Xml.Serialization.XmlReflectionMember 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Xml.Serialization {
	public class XmlReflectionMember {

		#region Fields

		bool isReturnValue;
		string memberName;
		Type memberType;
		bool overrideIsNullable;
		SoapAttributes soapAttributes;
		XmlAttributes xmlAttributes;

		#endregion

		#region Constructors

		public XmlReflectionMember ()
		{
		}

		internal XmlReflectionMember (string name, Type type, XmlAttributes attributes)
		{
			memberName = name;
			memberType = type;
			xmlAttributes = attributes;
		}

		internal XmlReflectionMember (string name, Type type, SoapAttributes attributes)
		{
			memberName = name;
			memberType = type;
			soapAttributes = attributes;
		}

		#endregion // Constructors

		#region Properties

		public bool IsReturnValue {
			get { return isReturnValue; }
			set { isReturnValue = value; }
		}

		public string MemberName {
			get { return memberName; }
			set { memberName = value; }
		}

		public Type MemberType {
			get { return memberType; }
			set { memberType = value; }
		}

		public bool OverrideIsNullable {
			get { return overrideIsNullable; }
			set { overrideIsNullable = value; }
		}

		public SoapAttributes SoapAttributes {
			get { 
				if (soapAttributes == null) soapAttributes = new SoapAttributes();
				return soapAttributes; 
			}
			set { soapAttributes = value; }
		}

		public XmlAttributes XmlAttributes {
			get { 
				if (xmlAttributes == null) xmlAttributes = new XmlAttributes();
				return xmlAttributes; 
			}
			set { xmlAttributes = value; }
		}
		
		internal bool InternalEquals (XmlReflectionMember other)
		{
			if (other == null) return false;
			
			if (isReturnValue != other.isReturnValue) return false;
			if (memberName != other.memberName) return false;
			if (memberType != other.memberType) return false;
			if (overrideIsNullable != other.overrideIsNullable) return false;
			
			if (soapAttributes == null) {
				if (other.soapAttributes != null) return false; }
			else
				if (!soapAttributes.InternalEquals (other.soapAttributes)) return false;
			
			if (xmlAttributes == null) {
				if (other.xmlAttributes != null) return false; }
			else
				if (!xmlAttributes.InternalEquals (other.xmlAttributes)) return false;
				
			return true;
		}

		#endregion // Properties
	}
}
