//
// SignCode.cs: secutil clone tool
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Mono.Security.Authenticode;
using Mono.Security.X509;

[assembly: AssemblyTitle("Mono SignCode")]
[assembly: AssemblyDescription("Sign assemblies and PE files using Authenticode(tm).")]

namespace Mono.Tools {

	class SignCode {

		static private void Header () 
		{
			Assembly a = Assembly.GetExecutingAssembly ();
			AssemblyName an = a.GetName ();

			object [] att = a.GetCustomAttributes (typeof (AssemblyTitleAttribute), false);
			string title = ((att.Length > 0) ? ((AssemblyTitleAttribute) att [0]).Title : "Mono SignCode");

			att = a.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
			string copyright = ((att.Length > 0) ? ((AssemblyCopyrightAttribute) att [0]).Copyright : "");

			Console.WriteLine ("{0} {1}", title, an.Version.ToString ());
			Console.WriteLine ("{0}{1}", copyright, Environment.NewLine);
		}

		static private void Help () 
		{
			Console.WriteLine ("Usage: signcode [options] filename{0}", Environment.NewLine);
			Console.WriteLine ("\t-spc spc\tSoftware Publisher Certificate file");
			Console.WriteLine ("\t-v pvk\t\tPrivate Key file");
			Console.WriteLine ("\t-a md5 | sha1\tHash Algorithm (default: MD5)");
			Console.WriteLine ("\t-$ indivisual | commercial\tSignature type");
			Console.WriteLine ("\t-n description\tDescription for the signed file");
			Console.WriteLine ("\t-i url\tURL for the signed file");
			Console.WriteLine ("Timestamp options");
			Console.WriteLine ("\t-t url\tTimestamp service http URL");
			Console.WriteLine ("\t-tr #\tNumber of retries for timestamp");
			Console.WriteLine ("\t-tw #\tDelay between retries");
			Console.WriteLine ("\t-x\tOnly timestamp (no signature)");
			Console.WriteLine ("CSP options");
			Console.WriteLine ("\t-k name\tKey Container Name");
			Console.WriteLine ("\t-p name\tProvider Name");
			Console.WriteLine ("\t-y #\tProvider Type");
			Console.WriteLine ("\t-ky [signature|exchange|#]\tKey Type");
			Console.WriteLine ("\t-r [localMachine|currentUser]\tKey Location");
		}

		static private RSA GetPrivateKey (string keyfile, CspParameters csp)
		{
			RSA rsa = null;

			if (keyfile != null) {
				if (!File.Exists (keyfile)) {
					Console.WriteLine ("Couldn't find '{0}' file.", keyfile);
					return null;
				}

				PrivateKey pvk = PrivateKey.CreateFromFile (keyfile);
				if (pvk.Encrypted) {
					Console.WriteLine ("Enter password for {0}: ", keyfile);
					string password = Console.ReadLine ();
					pvk = PrivateKey.CreateFromFile (keyfile, password);
					if (pvk.RSA == null)
						Console.WriteLine ("Invalid password!");
				}
				rsa = pvk.RSA;
			}
			else {
				rsa = new RSACryptoServiceProvider (csp);
			}

			return rsa;
		}

		static private X509CertificateCollection GetCertificates (string spcfile)
		{
			if (spcfile == null) {
				Console.WriteLine ("Missing SPC (certificate) file.");
				return null;
			}
			if (!File.Exists (spcfile)) {
				Console.WriteLine ("Couldn't find '{0}' file.", spcfile);
				return null;
			}

			SoftwarePublisherCertificate spc = SoftwarePublisherCertificate.CreateFromFile (spcfile);
			return spc.Certificates;
		}

		[STAThread]
		static int Main(string[] args)
		{
			Header ();
			if (args.Length < 1) {
				Help ();
				return 1;
			}

			CspParameters csp = new CspParameters ();
			string pvkFilename = null;
			string spcFilename = null;
			int timestampRetry = 0;
			int timestampDelay = 0;
			bool sign = true;

			// to be signed
			string tbsFilename = args [args.Length - 1];

			AuthenticodeFormatter af = new AuthenticodeFormatter ();

			int i = 0;
			while (i < args.Length) {
				switch (args[i++]) {
					case "-spc":
						spcFilename = args [i++];
						break;
					case "-v":
						pvkFilename = args [i++];
						break;
					case "-a":
						af.Hash = args [i++];
						break;
					case "-$":
						string auth = args [i++].ToLower ();
						switch (auth) {
							case "individual":
								af.Authority = Authority.Commercial;
								break;
							case "commercial":
								af.Authority = Authority.Individual;
								break;
							default:
								Console.WriteLine ("Unknown authority {0}", auth);
								return 1;
						}
						break;
					case "-i":
						af.Description = args [i++];
						break;
					case "-n":
						af.Url = new Uri (args [i++]);
						break;
					// timestamp options
					case "-t":
						af.TimestampUrl = new Uri (args [i++]);
						break;
					case "-tr":
						timestampRetry = Convert.ToInt32 (args [i++]);
						break;
					case "-tw":
						timestampDelay = Convert.ToInt32 (args [i++]) * 1000;
						break;
					case "-x":
						// only timestamp
						sign = false;  
						break;
					// CSP provider options
					case "-k":
						csp.KeyContainerName = args [i++];
						break;
					case "-p":
						csp.ProviderName = args [i++];
						break;
					case "-y":
						csp.ProviderType = Convert.ToInt32 (args [i++]);
						break;
					case "-ky":
						string key = args [i++];
						switch (key) {
							case "signature":
								csp.KeyNumber = 0;
								break;
							case "exchange":
								csp.KeyNumber = 0;
								break;
							default:
								csp.KeyNumber = Convert.ToInt32 (key);
								break;
						}
						break;
					case "-r":
						string location = args [i++];
						switch (location) {
							case "localMachine":
								csp.Flags = CspProviderFlags.UseMachineKeyStore;
								break;
							case "currentUser":
								csp.Flags = CspProviderFlags.UseDefaultKeyContainer;
								break;
							default:
								Console.WriteLine ("Unknown location {0}", location);
								return 1;
						}
						break;
					// unsupported options
					case "-j":
					case "-jp":
						Console.WriteLine ("Unsupported option {0}", args[i-1]);
						return 1;
				}
			}

			// no need to continue if we can't find the assembly
			// to be signed (and/or timestamped)
			if (!File.Exists (tbsFilename)) {
				Console.WriteLine ("Couldn't find {0}.", tbsFilename);
				return 1;
			}

			if (sign) {
				RSA rsa = GetPrivateKey (pvkFilename, csp);
				if (rsa == null) {
					Console.WriteLine ("No private key available to sign the assembly.");
					return 1;
				}
				af.RSA = rsa;

				X509CertificateCollection certs = GetCertificates (spcFilename);
				if ((certs == null) || (certs.Count == 0)) {
					Console.WriteLine ("No certificates available to sign the assembly.");
					return 1;
				}
				af.Certificates.AddRange (certs);

				af.Sign (tbsFilename);
			}
/* TODO
			if (af.TimestampURL != null) {
				for (int j=0; j < timestampRetry + 1; j++) {
					if (!af.Timestamp (tbsFilename)) {
						Thread.Sleep (timestampDelay);
						continue;
					}
					break;
				}
			}*/
			return 0;
		}
	}
}
