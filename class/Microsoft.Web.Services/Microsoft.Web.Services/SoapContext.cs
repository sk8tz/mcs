//
// SoapContext.cs: SOAP Context
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using Microsoft.Web.Services.Dime;
using Microsoft.Web.Services.Referral;
using Microsoft.Web.Services.Routing;
using Microsoft.Web.Services.Security;
using Microsoft.Web.Services.Timestamp;

using System;
using System.Collections;

namespace Microsoft.Web.Services {

	public sealed class SoapContext {

		private SoapEnvelope envelope;
		private Uri actor;
		private Microsoft.Web.Services.Timestamp.Timestamp timestamp;
		private Microsoft.Web.Services.Security.Security security;
		private Hashtable table;
		private DimeAttachmentCollection attachments;
		private string contentType;
		private SecurityCollection extendedSecurity;
		private ReferralCollection referrals;

		internal SoapContext () 
		{
			timestamp = new Microsoft.Web.Services.Timestamp.Timestamp ();
			table = new Hashtable ();
		}

		internal SoapContext (SoapEnvelope env) 
		{
			envelope = env;
			timestamp = new Microsoft.Web.Services.Timestamp.Timestamp ();
			table = new Hashtable ();
		}

		public Uri Actor { 
			get { return actor; }
		}

		public DimeAttachmentCollection Attachments { 
			get { 
				if (attachments == null)
					attachments = new DimeAttachmentCollection ();
				return attachments; 
			}
		}

		public string ContentType { 
			get { return contentType; }
		}

		public SoapEnvelope Envelope { 
			get { return envelope; }
		}

		public SecurityCollection ExtendedSecurity {
			get { return extendedSecurity; }
		}

		public object this [string key] { 
			get { return table [key]; }
			set { 
				if (key == null)
					throw new ArgumentNullException ("key");
				table [key] = value;
			} 
		}

		public Path Path { 
			get { return null; }
			set {;} 
		}

		public ReferralCollection Referrals { 
			get { return referrals; }
		}

		public Microsoft.Web.Services.Security.Security Security { 
			get { 
				if (security == null) {
					if (actor != null)
						security = new Microsoft.Web.Services.Security.Security (actor.ToString ());
					else
						security = new Microsoft.Web.Services.Security.Security ();
				}
				return security; 
			}
		}

		public Microsoft.Web.Services.Timestamp.Timestamp Timestamp { 
			get { return timestamp; }
		}

		internal bool IsReserved (string key) 
		{
			switch (key) {
				case "Actor":
				case "Attachments":
				case "ContentType":
				case "Envelope":
				case "ExtendedSecurity":
				case "IsInbound":
				case "IsIntermediary":
				case "Referrals":
				case "Path":
				case "Security":
				case "Timestamp":
				case "WebRequest":
				case "WebResponse":
					return true;
				default:
					return false;
			}
		}

		public void Add (string key, object value) 
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (IsReserved (key))
				throw new ArgumentException ("reserved key");
			table.Add (key, value);
		}

		public void Clear () 
		{
			foreach (DictionaryEntry entry in table) {
				string key = (string) entry.Key;
				// remove all except reserved names
				if (!IsReserved (key))
					table.Remove (key);
			}
		}

		public bool Contains (string key) 
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			return table.Contains (key);
		}

		public void CopyTo (SoapContext context) 
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			context.actor = this.actor;
			foreach (DimeAttachment da in Attachments) {
				context.Attachments.Add (da);
			}
			context.contentType = contentType;
			context.envelope = envelope;
			context.extendedSecurity = ExtendedSecurity;
			context.Path = Path;
			context.referrals = Referrals;
			context.security = security;
			context.timestamp = timestamp;
			foreach (DictionaryEntry de in table) {
				context.table.Add (de.Key, de.Value);
			}
		}

		public IDictionaryEnumerator GetEnumerator () 
		{
			return table.GetEnumerator ();
		}

		public void Remove (string key) 
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (IsReserved (key))
				throw new ArgumentException ("reserved key");
			table.Remove (key);
		}
	}
}
