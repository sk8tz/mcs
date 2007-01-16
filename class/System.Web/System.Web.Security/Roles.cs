//
// System.Web.Security.Roles
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Chris Toshok  <toshok@ximian.com>
//
// (C) 2003 Ben Maurer
// Copyright (c) 2005,2006 Novell, Inc (http://www.novell.com)
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

using System.Configuration.Provider;
using System.Web.Configuration;

namespace System.Web.Security {

	public static class Roles {
#if TARGET_J2EE
		const string Roles_cookie_protection = "Roles.cookie_protection";
		private static CookieProtection cookie_protection {
			get {
				object o = AppDomain.CurrentDomain.GetData (Roles_cookie_protection);
				if (o == null) {
					lock (AppDomain.CurrentDomain) {
						o = AppDomain.CurrentDomain.GetData (Roles_cookie_protection);
						if (o == null) {
							cookie_protection = CookieProtection.All;
							return cookie_protection;
						}
					}
				}

				return (CookieProtection) o;
			}

			set {
				AppDomain.CurrentDomain.SetData (Roles_cookie_protection, value);
			}
		}

		private static RoleManagerSection config {
			get {
				return (RoleManagerSection) WebConfigurationManager.GetSection ("system.web/roleManager");
			}
		}

		const string Roles_providersCollection = "Roles.providersCollection";
		static RoleProviderCollection providersCollection {
			get {
				return (RoleProviderCollection)AppDomain.CurrentDomain.GetData (Roles_providersCollection);
			}

			set {
				AppDomain.CurrentDomain.SetData (Roles_providersCollection, value);
			}
		}
#else
		private static CookieProtection cookie_protection;
		private static RoleManagerSection config;
		static RoleProviderCollection providersCollection;

		static Roles ()
		{
			// default values (when not supplied in web.config)
			cookie_protection = CookieProtection.All;

			config = (RoleManagerSection)WebConfigurationManager.GetSection ("system.web/roleManager");
		}
#endif


		public static void AddUsersToRole (string [] usernames, string rolename)
		{
			Provider.AddUsersToRoles (usernames, new string[] {rolename});
		}
		
		public static void AddUsersToRoles (string [] usernames, string [] rolenames)
		{
			Provider.AddUsersToRoles (usernames, rolenames);
		}
		
		public static void AddUserToRole (string username, string rolename)
		{
			Provider.AddUsersToRoles (new string[] {username}, new string[] {rolename});
		}
		
		public static void AddUserToRoles (string username, string [] rolenames)
		{
			Provider.AddUsersToRoles (new string[] {username}, rolenames);
		}
		
		public static void CreateRole (string rolename)
		{
			Provider.CreateRole (rolename);
		}
		
		[MonoTODO ("Not implemented")]
		public static void DeleteCookie ()
		{
			if (CacheRolesInCookie)
				throw new NotSupportedException ("Caching roles in cookie is not supported");
		}
		
		public static bool DeleteRole (string rolename)
		{
			return Provider.DeleteRole (rolename, true);
		}
		
		public static bool DeleteRole (string rolename, bool throwOnPopulatedRole)
		{
			return Provider.DeleteRole (rolename, throwOnPopulatedRole);
		}
		
		public static string [] GetAllRoles ()
		{
			return Provider.GetAllRoles ();
		}
		
		public static string [] GetRolesForUser ()
		{
			return Provider.GetRolesForUser (CurrentUser);
		}
		
		static string CurrentUser {
			get {
				if (HttpContext.Current != null && HttpContext.Current.User != null)
					return HttpContext.Current.User.Identity.Name;
				else
					return System.Threading.Thread.CurrentPrincipal.Identity.Name;
			}
		}
		
		public static string [] GetRolesForUser (string username)
		{
			return Provider.GetRolesForUser (username);
		}
		
		public static string [] GetUsersInRole (string rolename)
		{
			return Provider.GetUsersInRole (rolename);
		}
		
		public static bool IsUserInRole (string rolename)
		{
			return Provider.IsUserInRole (CurrentUser, rolename);
		}
		
		public static bool IsUserInRole (string username, string rolename)
		{
			return Provider.IsUserInRole (username, rolename);
		}
		
		public static void RemoveUserFromRole (string username, string rolename)
		{
			Provider.RemoveUsersFromRoles (new string[] {username}, new string[] {rolename});
		}
		
		public static void RemoveUserFromRoles (string username, string [] rolenames)
		{
			Provider.RemoveUsersFromRoles (new string[] {username}, rolenames);
		}
		
		public static void RemoveUsersFromRole (string [] usernames, string rolename)
		{
			Provider.RemoveUsersFromRoles (usernames, new string[] {rolename});
		}
		
		public static void RemoveUsersFromRoles (string [] usernames, string [] rolenames)
		{
			Provider.RemoveUsersFromRoles (usernames, rolenames);
		}
		
		public static bool RoleExists (string rolename)
		{
			return Provider.RoleExists (rolename);
		}
		
		public static string[] FindUsersInRole (string rolename, string usernameToMatch)
		{
			return Provider.FindUsersInRole (rolename, usernameToMatch);
		}
		
		public static string ApplicationName {
			get { return Provider.ApplicationName; }
			set { Provider.ApplicationName = value; }
		}
		
		public static bool CacheRolesInCookie {
			get { return config.CacheRolesInCookie; }
		}
		
		public static string CookieName {
			get { return config.CookieName; }
		}
		
		public static string CookiePath {
			get { return config.CookiePath; }
		}
		
		[MonoTODO ("read infos from web.config")]
		public static CookieProtection CookieProtectionValue {
			get { return cookie_protection; }
		}
		
		public static bool CookieRequireSSL {
			get { return config.CookieRequireSSL; }
		}
		
		public static bool CookieSlidingExpiration {
			get { return config.CookieSlidingExpiration; }
		}
		
		public static int CookieTimeout {
			get { return (int)config.CookieTimeout.TotalMinutes; }
		}

		public static bool CreatePersistentCookie {
			get { return config.CreatePersistentCookie; }
		}

		public static string Domain {
			get { return config.Domain; }
		}

		public static bool Enabled {
			get { return config.Enabled; }
		}

		public static int MaxCachedResults {
			get { return config.MaxCachedResults; }
		}
		
		public static RoleProvider Provider {
			get { return Providers[config.DefaultProvider]; }
		}
		
		public static RoleProviderCollection Providers {
			get {
				CheckEnabled ();
				if (providersCollection == null) {
					providersCollection = new RoleProviderCollection ();
					ProvidersHelper.InstantiateProviders (config.Providers, providersCollection, typeof (RoleProvider));
				}
				return providersCollection;
			}
		}

		// private stuff
		private static void CheckEnabled ()
		{
			if (!Enabled)
				throw new ProviderException ("This feature is not enabled.  To enable it, add <roleManager enabled=\"true\"> to your configuration file.");
		}
	}
}

#endif
