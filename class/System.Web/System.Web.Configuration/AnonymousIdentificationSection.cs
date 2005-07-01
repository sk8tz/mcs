//
// System.Web.Configuration.AnonymousIdentificationSection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Configuration;
using System.Web.Security;
using System.ComponentModel;

namespace System.Web.Configuration
{
	public sealed class AnonymousIdentificationSection: InternalSection
	{
/*		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty cookieNameProp;
		static ConfigurationProperty cookieTimeoutProp;
		static ConfigurationProperty cookiePathProp;
		static ConfigurationProperty cookieRequireSSLProp;
		static ConfigurationProperty cookieSlidingExpirationProp;
		static ConfigurationProperty cookieProtectionProp;
		static ConfigurationProperty cookilessProp;
		static ConfigurationProperty domainProp;
*/
		static AnonymousIdentificationSection ()
		{
/*			enabledProp = new ConfigurationProperty ("enabled", typeof(bool), false);
			cookieNameProp = new NonEmptyStringConfigurationProperty ("cookieName", ".ASPXANONYMOUS", ConfigurationPropertyFlags.None);
			cookieTimeoutProp = new TimeSpanConfigurationProperty ("cookieTimeout", new TimeSpan (69,10,40,0), TimeSpanSerializedFormat.Minutes, TimeSpanPropertyFlags.AllowInfinite | TimeSpanPropertyFlags.ProhibitZero, ConfigurationPropertyFlags.None);
			cookiePathProp = new NonEmptyStringConfigurationProperty ("cookiePath", "/", ConfigurationPropertyFlags.None);
			cookieRequireSSLProp = new ConfigurationProperty ("cookieRequireSSL", typeof(bool), false);
			cookieSlidingExpirationProp = new ConfigurationProperty ("cookieSlidingExpiration", typeof(bool), true);
			cookieProtectionProp = new ConfigurationProperty ("cookieProtection", typeof(CookieProtection), CookieProtection.Validation);
			cookilessProp = new ConfigurationProperty ("cookiless", typeof(HttpCookieMode), HttpCookieMode.UseDeviceProfile);
			domainProp = new ConfigurationProperty ("domain", typeof(string), null);
			
			properties = new ConfigurationPropertyCollection ();
			properties.Add (enabledProp);
			properties.Add (cookieNameProp);
			properties.Add (cookieTimeoutProp);
			properties.Add (cookiePathProp);
			properties.Add (cookieRequireSSLProp);
			properties.Add (cookieSlidingExpirationProp);
			properties.Add (cookieProtectionProp);
			properties.Add (cookilessProp);
			properties.Add (domainProp);
*/		}
		
		[ConfigurationProperty ("cookieless", DefaultValue = HttpCookieMode.UseCookies)]
		public HttpCookieMode Cookieless {
			get { return (HttpCookieMode) base ["cookieless"]; }
			set { base ["cookieless"] = value; }
		}
		
	    [StringValidator (MaxLength = 1)]
	    [ConfigurationProperty ("cookieName", DefaultValue = ".ASPXANONYMOUS")]
		public string CookieName {
			get { return (string) base ["cookieName"]; }
			set { base ["cookieName"] = value; }
		}
		
	    [StringValidator (MaxLength = 1)]
	    [ConfigurationProperty ("cookiePath", DefaultValue = "/")]
		public string CookiePath {
			get { return (string) base ["cookiePath"]; }
			set { base ["cookiePath"] = value; }
		}
		
		[ConfigurationProperty ("cookieProtection", DefaultValue = CookieProtection.Validation)]
		public CookieProtection CookieProtection {
			get { return (CookieProtection) base ["cookieProtection"]; }
			set { base ["cookieProtection"] = value; }
		}
		
		[ConfigurationProperty ("cookieRequireSSL", DefaultValue = false)]
		public bool CookieRequireSSL {
			get { return (bool) base ["cookieRequireSSL"]; }
			set { base ["cookieRequireSSL"] = value; }
		}
		
		[ConfigurationProperty ("cookieSlidingExpiration", DefaultValue = true)]
		public bool CookieSlidingExpiration {
			get { return (bool) base ["cookieSlidingExpiration"]; }
			set { base ["cookieSlidingExpiration"] = value; }
		}
		
		[ConfigurationValidator (typeof(PositiveTimeSpanValidator))]
		[TypeConverter (typeof(TimeSpanMinutesOrInfiniteConverter))]
		[ConfigurationProperty ("cookieTimeout", DefaultValue = "69.10:40:00")]
		public TimeSpan CookieTimeout {
			get { return (TimeSpan) base ["cookieTimeout"]; }
			set { base ["cookieTimeout"] = value; }
		}
		
		[ConfigurationProperty ("domain", DefaultValue = "")]
		public string Domain {
			get { return (string) base ["domain"]; }
			set { base ["domain"] = value; }
		}
		
	    [ConfigurationProperty ("enabled", DefaultValue = false)]
		public bool Enabled {
			get { return (bool) base ["enabled"]; }
			set { base ["enabled"] = value; }
		}

/*		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
*/
	}
}

#endif
