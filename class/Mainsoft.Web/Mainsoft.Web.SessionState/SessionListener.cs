//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Web.SessionState;
using System.Reflection;
using javax.servlet.http;
using Mainsoft.Web.Hosting;

namespace Mainsoft.Web.SessionState
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class SessionListener : javax.servlet.http.HttpSessionListener
	{

		public void sessionCreated (HttpSessionEvent se) {
		}

		public void sessionDestroyed (HttpSessionEvent se) {
			bool setDomain = vmw.@internal.EnvironmentUtils.getAppDomain () == null;
			if (setDomain) {
				AppDomain servletDomain = (AppDomain) se.getSession ().getServletContext ().getAttribute (J2EEConsts.APP_DOMAIN);
				if (servletDomain == null)
					return;
				vmw.@internal.EnvironmentUtils.setAppDomain (servletDomain);
			}
			try {
				HttpSessionStateContainer container =
					ServletSessionStateStoreProvider.CreateContainer (se.getSession ());

				SessionStateUtility.RaiseSessionEnd (container, this, EventArgs.Empty);
			}
#if DEBUG
			catch (Exception e) {
				Console.WriteLine (e.Message);
				Console.WriteLine (e.StackTrace);
			}
#endif
			finally {
				if (setDomain) {
					vmw.@internal.EnvironmentUtils.clearAppDomain ();
					BaseHttpServlet servlet = (BaseHttpServlet) se.getSession ().getServletContext ().getAttribute (J2EEConsts.CURRENT_SERVLET);
					if (servlet != null)
						servlet.CleanupDerby ();
				}
			}
		}
	}
}

namespace System.Web.GH
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class SessionListener : Mainsoft.Web.SessionState.SessionListener
	{
	}
}

namespace System.Web.J2EE
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class SessionListener : Mainsoft.Web.SessionState.SessionListener
	{
	}
}
