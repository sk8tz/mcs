//
// SoapTypeAttribute.cs: 
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
	/// Summary description for SoapTypeAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
		 AttributeTargets.Enum | AttributeTargets.Interface)]
	public class SoapTypeAttribute : Attribute
	{
		private string ns;
		private string typeName;
		private bool includeInSchema = true;

		public SoapTypeAttribute ()
		{
		}
		public SoapTypeAttribute (string typeName)
		{
			TypeName = typeName;
		}
		public SoapTypeAttribute (string typeName, string ns)
		{
			TypeName = typeName;
			Namespace = ns;
		}
		
		public bool IncludeInSchema 
		{
			get { return  includeInSchema; }
			set { includeInSchema = value; }
		}

		public string Namespace {
			get { return ns;
			}
			set { ns = value;
			}
		}
		public string TypeName {
			get { return typeName;
			}
			set { typeName = value;
			}
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("STA ");
			KeyHelper.AddField (sb, 1, ns);
			KeyHelper.AddField (sb, 2, typeName);
			KeyHelper.AddField (sb, 3, includeInSchema);
			sb.Append ('|');
		}
	}
}
