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
			get { return soapAttributes; }
			set { soapAttributes = value; }
		}

		public XmlAttributes XmlAttributes {
			get { return xmlAttributes; }
			set { xmlAttributes = value; }
		}

		#endregion // Properties
	}
}
