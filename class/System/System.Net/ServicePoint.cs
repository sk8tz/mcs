//
// System.Net.ServicePoint
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Lawrence Pit
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace System.Net 
{
	public class ServicePoint
	{
		Uri uri;
		int connectionLimit;
		int maxIdleTime;
		int currentConnections;
		DateTime idleSince;
		Version protocolVersion;
		X509Certificate certificate;
		X509Certificate clientCertificate;
		IPHostEntry host;
		bool usesProxy;
		Hashtable groups;
		bool sendContinue = true;
		
		// Constructors

		internal ServicePoint (Uri uri, int connectionLimit, int maxIdleTime)
		{
			this.uri = uri;  
			this.connectionLimit = connectionLimit;
			this.maxIdleTime = maxIdleTime;			
			this.currentConnections = 0;
			this.idleSince = DateTime.Now;
		}
		
		// Properties
		
		public Uri Address {
			get { return uri; }
		}
		
		public X509Certificate Certificate {
			get { return certificate; }
		}
		
		public X509Certificate ClientCertificate {
			get { return clientCertificate; }
		}
		
		public int ConnectionLimit {
			get { return connectionLimit; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();

				connectionLimit = value;
			}
		}
		
		public string ConnectionName {
			get { return uri.Scheme; }
		}

		public int CurrentConnections {
			get {
				lock (this) {
					return currentConnections;
				}
			}
		}

		public DateTime IdleSince {
			get {
				lock (this) {
					return idleSince;
				}
			}
		}
		
		public int MaxIdleTime {
			get { return maxIdleTime; }
			set { 
				if (value < Timeout.Infinite || value > Int32.MaxValue)
					throw new ArgumentOutOfRangeException ();
				this.maxIdleTime = value; 
			}
		}
		
		public virtual Version ProtocolVersion {
			get { return protocolVersion; }
		}
		
		public bool SupportsPipelining {
			get { return HttpVersion.Version11.Equals (protocolVersion); }
		}
		
		internal bool SendContinue {
			get { return sendContinue &&
				     (protocolVersion == null || protocolVersion == HttpVersion.Version11); }
			set { sendContinue = value; }
		}
		// Methods
		
		public override int GetHashCode() 
		{
			return base.GetHashCode ();
		}
		
		// Internal Methods

		internal bool UsesProxy {
			get { return usesProxy; }
			set { usesProxy = value; }
		}

		internal bool AvailableForRecycling {
			get { 
				return CurrentConnections == 0
				    && maxIdleTime != Timeout.Infinite
			            && DateTime.Now >= IdleSince.AddMilliseconds (maxIdleTime);
			}
		}

		internal Hashtable Groups {
			get {
				if (groups == null)
					groups = new Hashtable ();

				return groups;
			}
		}

		internal IPHostEntry HostEntry
		{
			get {
				if (host == null) {
					string uriHost = uri.Host;

					// There is no need to do DNS resolution on literal IP addresses
					if (uri.HostNameType == UriHostNameType.IPv6 ||
						uri.HostNameType == UriHostNameType.IPv4) {

						if (uri.HostNameType == UriHostNameType.IPv6) {
							// Remove square brackets
							uriHost = uriHost.Substring(1,uriHost.Length-2);
						}

						// Creates IPHostEntry
						host = new IPHostEntry();
						host.AddressList = new IPAddress[] { IPAddress.Parse(uriHost) };

						return host;
					}

					// Try DNS resolution on host names
					try  {
						host = Dns.GetHostByName (uriHost);
					} 
					catch {
						return null;
					}
				}

				return host;
			}
		}

		internal void SetVersion (Version version)
		{
			protocolVersion = version;
		}
		
		internal WebConnectionGroup GetConnectionGroup (string name)
		{
			if (name == null)
				name = "";

			WebConnectionGroup group = Groups [name] as WebConnectionGroup;
			if (group != null)
				return group;

			group = new WebConnectionGroup (this, name);
			Groups [name] = group;
			return group;
		}

		internal EventHandler SendRequest (HttpWebRequest request, string groupName)
		{
			WebConnection cnc;
			
			lock (this) {
				WebConnectionGroup cncGroup = GetConnectionGroup (groupName);
				cnc = cncGroup.GetConnection ();
			}
			
			return cnc.SendRequest (request);
		}

		internal void IncrementConnection ()
		{
			lock (this) {
				currentConnections++;
				idleSince = DateTime.Now.AddMilliseconds (1000000);
				Console.WriteLine ("+CurerntCnc: {0} {1}", Address, currentConnections);
			}
		}

		internal void DecrementConnection ()
		{
			lock (this) {
				currentConnections--;
				if (currentConnections == 0)
					idleSince = DateTime.Now;
				Console.WriteLine ("-CurerntCnc: {0} {1}", Address, currentConnections);
			}
		}

		internal void SetCertificates (X509Certificate client, X509Certificate server) 
		{
			certificate = server;
			clientCertificate = client;
		}
	}
}
