//
// SoapAttributeAttribute.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for SoapAttributeAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		 | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public class SoapAttributeAttribute : Attribute
	{
		private string attrName;
		private string dataType;
		private string ns;

		public SoapAttributeAttribute ()
		{
		}

		public SoapAttributeAttribute (string attrName) 
		{
			AttrName = attrName;
		}

		public string AttrName {
			get	{
				return attrName;
			} 
			set	{
				attrName = value;
			}
		}

		public string DataType {
			get {
				return dataType;
			} 
			set {
				dataType = value;
			}
		}

		public string Namespace {
			get {
				return ns;
			} 
			set {
				ns = value;
			}
		}
	}
}
