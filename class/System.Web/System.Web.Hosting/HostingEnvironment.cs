//
// System.Web.Hosting.HostingEnvironment.cs
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//

//
// Copyright (C) 2005,2006 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;
using System.Web.Caching;

namespace System.Web.Hosting {

	[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.High)]
	public sealed class HostingEnvironment : MarshalByRefObject
	{
		static bool is_hosted;
		static string site_name;
		static ApplicationShutdownReason shutdown_reason;
		static VirtualPathProvider vpath_provider; // This should get a default

		public HostingEnvironment ()
		{
		}

		public static string ApplicationID {
			get { return HttpRuntime.AppDomainAppId; }
		}

		public static string ApplicationPhysicalPath {
			get { return HttpRuntime.AppDomainAppPath; }
		}

		public static string ApplicationVirtualPath {
			get { return HttpRuntime.AppDomainAppVirtualPath; }
		}

		public static Cache Cache {
			get { return HttpRuntime.Cache; }
		}

		public static Exception InitializationException {
			get { return HttpApplication.InitializationException; }
		}

		public static bool IsHosted {
			get { return is_hosted; }
		}

		public static ApplicationShutdownReason ShutdownReason {
			get { return shutdown_reason; }
		}

		[MonoTODO]
		public static string SiteName {
			get { return site_name; }
		}

		public static VirtualPathProvider VirtualPathProvider {
			get { return vpath_provider; }
		}

		[MonoTODO]
		public static void DecrementBusyCount ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IDisposable Impersonate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IDisposable Impersonate (IntPtr token)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IDisposable Impersonate (IntPtr userToken, string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void IncrementBusyCount ()
		{
			throw new NotImplementedException ();
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}

		[MonoTODO]
		public static void InitiateShutdown ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string MapPath (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void RegisterObject (IRegisteredObject obj)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterVirtualPathProvider (VirtualPathProvider virtualPathProvider)
		{
			if (HttpRuntime.AppDomainAppVirtualPath == null)
				throw new InvalidOperationException ();

			if (virtualPathProvider == null)
				throw new ArgumentNullException ("virtualPathProvider");

			virtualPathProvider.SetPrevious (vpath_provider);
			vpath_provider = virtualPathProvider;
		}
		
		[MonoTODO]
		public static IDisposable SetCultures (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IDisposable SetCultures ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void UnregisterObject (IRegisteredObject obj)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
