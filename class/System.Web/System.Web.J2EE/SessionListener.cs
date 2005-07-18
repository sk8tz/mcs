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

namespace System.Web.J2EE
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class SessionListener: javax.servlet.http.HttpSessionListener
	{
		private MethodInfo method;
		private bool firstTime = true;
		public SessionListener()
		{
		}

		public void sessionCreated(HttpSessionEvent se)
		{
		}

		public void sessionDestroyed(HttpSessionEvent se) 
		{
			try
			{
				bool  getHttpApplication = false;
				object o  = se.getSession().getAttribute("GH_SESSION_STATE");
				if (o == null)
					return;
				object app  = ((HttpSessionState)o).App;
				HttpApplicationFactory appFactory = null;
				if (app == null)
				{
					try
					{
						appFactory = (HttpApplicationFactory)AppDomain.CurrentDomain.GetData("HttpApplicationFactory");
					}
					catch (Exception)
					{
						return;
					}
					if (appFactory != null)
						app = appFactory.GetPublicInstance();
					if (app == null)
						return;
					else
						getHttpApplication = true; 
				}
				if (method == null && firstTime)
				{
					firstTime = false;
					Type appType = app.GetType();
					method = appType.GetMethod("Session_End",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
					if (method == null)
						return;
				}
				else if (method == null)
					return;
				method.Invoke(app, new object[]{app,EventArgs.Empty});
				if (getHttpApplication)
					appFactory.RecyclePublicInstance((HttpApplication)app);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}
	}
}
