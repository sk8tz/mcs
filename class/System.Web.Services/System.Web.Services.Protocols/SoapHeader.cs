// 
// System.Web.Services.Protocols.SoapHeader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
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

using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Web.Services.Protocols {
	[SoapType (IncludeInSchema = false)]
	[XmlType (IncludeInSchema = false)]
	public abstract class SoapHeader {

		#region Fields

		string actor;
		bool didUnderstand;
		bool mustUnderstand;

		#endregion // Fields

		#region Constructors

		protected SoapHeader ()
		{
			actor = String.Empty; 
			didUnderstand = false;
			mustUnderstand = false;
		}

		#endregion // Constructors

		#region Properties

		[DefaultValue ("")]
		[SoapAttribute ("actor", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		[XmlAttribute ("actor", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		public string Actor {	
			get { return actor; }
			set { actor = value; }
		}

		[SoapIgnore]
		[XmlIgnore]
		public bool DidUnderstand {
			get { return didUnderstand; }
			set { didUnderstand = value; }
		}

		[DefaultValue ("0")]
		[SoapAttribute ("mustUnderstand", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		[XmlAttribute ("mustUnderstand", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		public string EncodedMustUnderstand {
			get { return (MustUnderstand ? "1" : "0"); }
			set {	
				if (value == "true" || value == "1") 
					MustUnderstand = true;
				else if (value == "false" || value == "0")
					MustUnderstand = false;
				else
					throw new ArgumentException ();
			}
		}

		[SoapIgnore]
		[XmlIgnore]
		public bool MustUnderstand {
			get { return mustUnderstand; }
			set { mustUnderstand = value; }
		}

		#endregion // Properties
	}
}
