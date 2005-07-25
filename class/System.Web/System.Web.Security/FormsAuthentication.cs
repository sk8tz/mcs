//
// System.Web.Security.FormsAuthentication
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Security
{
	public sealed class FormsAuthentication
	{
		const int MD5_hash_size = 16;
		const int SHA1_hash_size = 20;

		static string authConfigPath = "system.web/authentication";
		static bool initialized;
		static string cookieName;
		static string cookiePath;
		static int timeout;
		static FormsProtectionEnum protection;
		static object locker = new object ();
		static byte [] init_vector; // initialization vector used for 3DES
#if NET_1_1
		static bool requireSSL;
		static bool slidingExpiration;
#endif

		// same names and order used in xsp
		static string [] indexFiles = { "index.aspx",
						"Default.aspx",
						"default.aspx",
						"index.html",
						"index.htm" };

		public static bool Authenticate (string name, string password)
		{
			if (name == null || password == null)
				return false;

			Initialize ();
			HttpContext context = HttpContext.Current;
			if (context == null)
				throw new HttpException ("Context is null!");

			AuthConfig config = context.GetConfig (authConfigPath) as AuthConfig;
			Hashtable users = config.CredentialUsers;
			string stored = users [name] as string;
			if (stored == null)
				return false;

			switch (config.PasswordFormat) {
			case FormsAuthPasswordFormat.Clear:
				/* Do nothing */
				break;
			case FormsAuthPasswordFormat.MD5:
				password = HashPasswordForStoringInConfigFile (password, "MD5");
				break;
			case FormsAuthPasswordFormat.SHA1:
				password = HashPasswordForStoringInConfigFile (password, "SHA1");
				break;
			}

			return (password == stored);
		}

		static FormsAuthenticationTicket Decrypt2 (byte [] bytes)
		{
			if (protection == FormsProtectionEnum.None)
				return FormsAuthenticationTicket.FromByteArray (bytes);

			MachineKeyConfig config = HttpContext.GetAppConfig ("system.web/machineKey") as MachineKeyConfig;
			bool all = (protection == FormsProtectionEnum.All);

			byte [] result = bytes;
			if (all || protection == FormsProtectionEnum.Encryption) {
				ICryptoTransform decryptor;
				decryptor = new TripleDESCryptoServiceProvider().CreateDecryptor (config.DecryptionKey192Bits, init_vector);
				result = decryptor.TransformFinalBlock (bytes, 0, bytes.Length);
				bytes = null;
			}

			if (all || protection == FormsProtectionEnum.Validation) {
				int count;

				if (config.ValidationType == MachineKeyValidation.MD5)
					count = MD5_hash_size;
				else
					count = SHA1_hash_size; // 3DES and SHA1

				byte [] vk = config.ValidationKey;
				byte [] mix = new byte [result.Length - count + vk.Length];
				Buffer.BlockCopy (result, 0, mix, 0, result.Length - count);
				Buffer.BlockCopy (vk, 0, mix, result.Length - count, vk.Length);

				byte [] hash = null;
				switch (config.ValidationType) {
				case MachineKeyValidation.MD5:
					hash = MD5.Create ().ComputeHash (mix);
					break;
				// From MS docs: "When 3DES is specified, forms authentication defaults to SHA1"
				case MachineKeyValidation.TripleDES:
				case MachineKeyValidation.SHA1:
					hash = SHA1.Create ().ComputeHash (mix);
					break;
				}

				if (result.Length < count)
					throw new ArgumentException ("Error validating ticket (length).", "encryptedTicket");

				int i, k;
				for (i = result.Length - count, k = 0; k < count; i++, k++) {
					if (result [i] != hash [k])
						throw new ArgumentException ("Error validating ticket.", "encryptedTicket");
				}
			}

			return FormsAuthenticationTicket.FromByteArray (result);
		}

		public static FormsAuthenticationTicket Decrypt (string encryptedTicket)
		{
			if (encryptedTicket == null || encryptedTicket == String.Empty)
				throw new ArgumentException ("Invalid encrypted ticket", "encryptedTicket");

			Initialize ();

			FormsAuthenticationTicket ticket;
			byte [] bytes = MachineKeyConfig.GetBytes (encryptedTicket, encryptedTicket.Length);
			try {
				ticket = Decrypt2 (bytes);
			} catch (Exception) {
				ticket = null;
			}

			return ticket;
		}

		public static string Encrypt (FormsAuthenticationTicket ticket)
		{
			if (ticket == null)
				throw new ArgumentNullException ("ticket");

			Initialize ();
			byte [] ticket_bytes = ticket.ToByteArray ();
			if (protection == FormsProtectionEnum.None)
				return GetHexString (ticket_bytes);

			byte [] result = ticket_bytes;
			MachineKeyConfig config = HttpContext.GetAppConfig ("system.web/machineKey") as MachineKeyConfig;
			bool all = (protection == FormsProtectionEnum.All);
			if (all || protection == FormsProtectionEnum.Validation) {
				byte [] valid_bytes = null;
				byte [] vk = config.ValidationKey;
				byte [] mix = new byte [ticket_bytes.Length + vk.Length];
				Buffer.BlockCopy (ticket_bytes, 0, mix, 0, ticket_bytes.Length);
				Buffer.BlockCopy (vk, 0, mix, result.Length, vk.Length);

				switch (config.ValidationType) {
				case MachineKeyValidation.MD5:
					valid_bytes = MD5.Create ().ComputeHash (mix);
					break;
				// From MS docs: "When 3DES is specified, forms authentication defaults to SHA1"
				case MachineKeyValidation.TripleDES:
				case MachineKeyValidation.SHA1:
					valid_bytes = SHA1.Create ().ComputeHash (mix);
					break;
				}

				int tlen = ticket_bytes.Length;
				int vlen = valid_bytes.Length;
				result = new byte [tlen + vlen];
				Buffer.BlockCopy (ticket_bytes, 0, result, 0, tlen);
				Buffer.BlockCopy (valid_bytes, 0, result, tlen, vlen);
			}

			if (all || protection == FormsProtectionEnum.Encryption) {
				ICryptoTransform encryptor;
				encryptor = new TripleDESCryptoServiceProvider().CreateEncryptor (config.DecryptionKey192Bits, init_vector);
				result = encryptor.TransformFinalBlock (result, 0, result.Length);
			}

			return GetHexString (result);
		}

		public static HttpCookie GetAuthCookie (string userName, bool createPersistentCookie)
		{
			return GetAuthCookie (userName, createPersistentCookie, null);
		}

		public static HttpCookie GetAuthCookie (string userName, bool createPersistentCookie, string strCookiePath)
		{
			Initialize ();

			if (userName == null)
				userName = String.Empty;

			if (strCookiePath == null || strCookiePath.Length == 0)
				strCookiePath = cookiePath;

			DateTime now = DateTime.Now;
			DateTime then;
			if (createPersistentCookie)
				then = now.AddYears (50);
			else
				then = now.AddMinutes (timeout);

			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket (1,
											  userName,
											  now,
											  then,
											  createPersistentCookie,
											  String.Empty,
											  cookiePath);

			if (!createPersistentCookie)
				then = DateTime.MinValue;

			return new HttpCookie (cookieName, Encrypt (ticket), strCookiePath, then);
		}

		public static string GetRedirectUrl (string userName, bool createPersistentCookie)
		{
			if (userName == null)
				return null;

			Initialize ();
			HttpRequest request = HttpContext.Current.Request;
			string returnUrl = request ["RETURNURL"];
			if (returnUrl != null)
				return returnUrl;

			returnUrl = request.ApplicationPath;
			string apppath = request.PhysicalApplicationPath;
			bool found = false;

			foreach (string indexFile in indexFiles) {
				string filePath = Path.Combine (apppath, indexFile);
				if (File.Exists (filePath)) {
					returnUrl = UrlUtils.Combine (returnUrl, indexFile);
					found = true;
					break;
				}
			}

			if (!found)
				returnUrl = UrlUtils.Combine (returnUrl, "index.aspx");

			return returnUrl;
		}

		static string GetHexString (string str)
		{
			return GetHexString (Encoding.UTF8.GetBytes (str));
		}

		static string GetHexString (byte [] bytes)
		{
			StringBuilder result = new StringBuilder (bytes.Length * 2);
			foreach (byte b in bytes)
				result.AppendFormat ("{0:X2}", (int) b);

			return result.ToString ();
		}

		public static string HashPasswordForStoringInConfigFile (string password, string passwordFormat)
		{
			if (password == null)
				throw new ArgumentNullException ("password");

			if (passwordFormat == null)
				throw new ArgumentNullException ("passwordFormat");

			byte [] bytes;
			if (String.Compare (passwordFormat, "MD5", true) == 0) {
				bytes = MD5.Create ().ComputeHash (Encoding.UTF8.GetBytes (password));
			} else if (String.Compare (passwordFormat, "SHA1", true) == 0) {
				bytes = SHA1.Create ().ComputeHash (Encoding.UTF8.GetBytes (password));
			} else {
				throw new ArgumentException ("The format must be either MD5 or SHA1", "passwordFormat");
			}

			return GetHexString (bytes);
		}

		public static void Initialize ()
		{
			if (initialized)
				return;

			lock (locker) {
				if (initialized)
					return;

				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new HttpException ("Context is null!");

				AuthConfig authConfig = context.GetConfig (authConfigPath) as AuthConfig;
				if (authConfig != null) {
					cookieName = authConfig.CookieName;
					timeout = authConfig.Timeout;
					cookiePath = authConfig.CookiePath;
					protection = authConfig.Protection;
#if NET_1_1
					requireSSL = authConfig.RequireSSL;
					slidingExpiration = authConfig.SlidingExpiration;
#endif
				} else {
					cookieName = ".MONOAUTH";
					timeout = 30;
					cookiePath = "/";
					protection = FormsProtectionEnum.All;
#if NET_1_1
					slidingExpiration = true;
#endif
				}
				TripleDESCryptoServiceProvider tDES = new TripleDESCryptoServiceProvider ();
				tDES.GenerateIV ();
				init_vector = tDES.IV;
				initialized = true;
			}
		}

		public static void RedirectFromLoginPage (string userName, bool createPersistentCookie)
		{
			RedirectFromLoginPage (userName, createPersistentCookie, null);
		}

		public static void RedirectFromLoginPage (string userName, bool createPersistentCookie, string strCookiePath)
		{
			if (userName == null)
				return;

			Initialize ();
			SetAuthCookie (userName, createPersistentCookie, strCookiePath);
			HttpResponse resp = HttpContext.Current.Response;
			resp.Redirect (GetRedirectUrl (userName, createPersistentCookie), false);
		}

		public static FormsAuthenticationTicket RenewTicketIfOld (FormsAuthenticationTicket tOld)
		{
			if (tOld == null)
				return null;

			DateTime now = DateTime.Now;
			TimeSpan toIssue = now - tOld.IssueDate;
			TimeSpan toExpiration = tOld.Expiration - now;
			if (toExpiration > toIssue)
				return tOld;

			FormsAuthenticationTicket tNew = tOld.Clone ();
			tNew.SetDates (now, now + (tOld.Expiration - tOld.IssueDate));
			return tNew;
		}

		public static void SetAuthCookie (string userName, bool createPersistentCookie)
		{
			Initialize ();
			SetAuthCookie (userName, createPersistentCookie, cookiePath);
		}

		public static void SetAuthCookie (string userName, bool createPersistentCookie, string strCookiePath)
		{
			HttpContext context = HttpContext.Current;
			if (context == null)
				throw new HttpException ("Context is null!");

			HttpResponse response = context.Response;
			if (response == null)
				throw new HttpException ("Response is null!");

			response.Cookies.Add (GetAuthCookie (userName, createPersistentCookie, strCookiePath));
		}

		public static void SignOut ()
		{
			Initialize ();

			HttpContext context = HttpContext.Current;
			if (context == null)
				throw new HttpException ("Context is null!");

			HttpResponse response = context.Response;
			if (response == null)
				throw new HttpException ("Response is null!");

			response.Cookies.MakeCookieExpire (cookieName, cookiePath);
		}

		public static string FormsCookieName
		{
			get {
				Initialize ();
				return cookieName;
			}
		}

		public static string FormsCookiePath
		{
			get {
				Initialize ();
				return cookiePath;
			}
		}
#if NET_1_1
		public static bool RequireSSL {
			get {
				Initialize ();
				return requireSSL;
			}
		}

		public static bool SlidingExpiration {
			get {
				Initialize ();
				return slidingExpiration;
			}
		}
#endif
	}
}

