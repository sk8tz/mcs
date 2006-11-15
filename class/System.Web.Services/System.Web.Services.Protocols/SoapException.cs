// 
// System.Web.Services.Protocols.SoapException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@novell.com)
//
// Copyright (C) Tim Coleman, 2002
//

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

using System.Xml;

namespace System.Web.Services.Protocols 
{
#if NET_2_0
	[Serializable]
#endif
	public class SoapException : SystemException 
	{
		#region Fields

		public static readonly XmlQualifiedName ClientFaultCode = new XmlQualifiedName ("Client", "http://schemas.xmlsoap.org/soap/envelope/");
		public static readonly XmlQualifiedName DetailElementName = new XmlQualifiedName ("detail");
		public static readonly XmlQualifiedName MustUnderstandFaultCode = new XmlQualifiedName ("MustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/");
		public static readonly XmlQualifiedName ServerFaultCode = new XmlQualifiedName ("Server", "http://schemas.xmlsoap.org/soap/envelope/");
		public static readonly XmlQualifiedName VersionMismatchFaultCode = new XmlQualifiedName ("VersionMismatch", "http://schemas.xmlsoap.org/soap/envelope/");

		string actor;
		XmlQualifiedName code;
		XmlNode detail;
		
#if NET_2_0
		string lang;
		string role;
		SoapFaultSubCode subcode;
#endif
		#endregion

		#region Constructors

		public SoapException (string message, XmlQualifiedName code)
			: base (message)
		{
			this.code = code;
		}

		public SoapException (string message, XmlQualifiedName code, Exception innerException)
			: base (message, innerException)
		{
			this.code = code;
		}

		public SoapException (string message, XmlQualifiedName code, string actor)
			: base (message)
		{
			this.code = code;
			this.actor = actor;
		}

		public SoapException (string message, XmlQualifiedName code, string actor, Exception innerException)
			: base (message, innerException)
		{
			this.code = code;
			this.actor = actor;
		}

		public SoapException (string message, XmlQualifiedName code, string actor, XmlNode detail)
			: base (message)
		{
			this.code = code;
			this.actor = actor;
			this.detail = detail;
		}

		public SoapException (string message, XmlQualifiedName code, string actor, XmlNode detail, Exception innerException)
			: base (message, innerException)
		{
			this.code = code;
			this.actor = actor;
			this.detail = detail;
		}

#if NET_2_0
		public SoapException (string message, XmlQualifiedName code, SoapFaultSubCode subcode)
			: base (message)
		{
			this.code = code;
			this.subcode = subcode;
		}
		
		public SoapException (string message, XmlQualifiedName code, string actor, string role, XmlNode detail, SoapFaultSubCode subcode, Exception innerException)
			: base (message, innerException)
		{
			this.code = code;
			this.subcode = subcode;
			this.detail = detail;
			this.actor = actor;
			this.role = role;
		}
		
		public SoapException (string message, XmlQualifiedName code, string actor, string role, string lang, XmlNode detail, SoapFaultSubCode subcode, Exception innerException)
		{
			this.code = code;
			this.subcode = subcode;
			this.detail = detail;
			this.actor = actor;
			this.role = role;
			this.lang = lang;
		}

		public static bool IsClientFaultCode (XmlQualifiedName code)
		{
			if (code == ClientFaultCode) return true;
			if (code == Soap12FaultCodes.SenderFaultCode) return true;
			return false;
		}

		public static bool IsMustUnderstandFaultCode (XmlQualifiedName code)
		{
			if (code == MustUnderstandFaultCode) return true;
			if (code == Soap12FaultCodes.MustUnderstandFaultCode) return true;
			return false;
		}
				
		public static bool IsServerFaultCode (XmlQualifiedName code)
		{
			if (code == ServerFaultCode) return true;
			if (code == Soap12FaultCodes.ReceiverFaultCode) return true;
			return false;
		}
				
		public static bool IsVersionMismatchFaultCode (XmlQualifiedName code)
		{
			if (code == VersionMismatchFaultCode) return true;
			if (code == Soap12FaultCodes.VersionMismatchFaultCode) return true;
			return false;
		}

#endif

		#endregion // Constructors

		#region Properties

		public string Actor {
			get { return actor; }
		}

		public XmlQualifiedName Code {
			get { return code; }
		}

		public XmlNode Detail {
			get { return detail; }
		}

#if NET_2_0
		[System.Runtime.InteropServices.ComVisible(false)]
		public string Lang {
			get { return lang; }
		}
		
		[System.Runtime.InteropServices.ComVisible(false)]
		public string Role {
			get { return role; }
		}
		
		[System.Runtime.InteropServices.ComVisible(false)]
		public SoapFaultSubCode SubCode {
			get { return subcode; }
		}
		
		// Same value as actor
		[System.Runtime.InteropServices.ComVisible(false)]
		public string Node {
			get { return actor; }
		}
#endif
		#endregion // Properties
	}
}
