//
// System.Web.Configuration.AuthorizationSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration;
using System.Security.Principal;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class AuthorizationSection : ConfigurationSection
	{
		static ConfigurationProperty rulesProp;
		static ConfigurationPropertyCollection properties;

		static AuthorizationSection ()
		{
			rulesProp = new ConfigurationProperty ("", typeof (AuthorizationRuleCollection), null,
							       null, PropertyHelper.DefaultValidator,
							       ConfigurationPropertyOptions.IsDefaultCollection);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (rulesProp);
		}

		[MonoTODO]
		protected override void PostDeserialize()
		{
			base.PostDeserialize ();
		}

		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public AuthorizationRuleCollection Rules {
			get { return (AuthorizationRuleCollection) base [rulesProp];}
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


		internal bool IsValidUser (IPrincipal user, string verb)
		{
			foreach (AuthorizationRule rule in Rules) {
				if (!rule.CheckVerb (verb))
					continue;

				if (rule.CheckUser (user.Identity.Name) || rule.CheckRole(user))
					return (rule.Action == AuthorizationRuleAction.Allow);
			}

			return true;
		}

	}

}

#endif

