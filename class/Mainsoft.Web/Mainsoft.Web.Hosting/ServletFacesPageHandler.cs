using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using javax.faces.component;
using System.Web.UI;
using javax.faces.context;
using javax.faces.lifecycle;
using javax.faces;
using javax.servlet;
using javax.faces.webapp;
using javax.servlet.http;
using vmw.common;

namespace Mainsoft.Web.Hosting
{
	public sealed class ServletFacesPageHandler : IHttpHandler
	{
		readonly FacesContextFactory _facesContextFactory;
		readonly Lifecycle _lifecycle;
		readonly string _executionFilePath;

		public bool IsReusable {
			get { return false; }
		}

		public ServletFacesPageHandler (string executionFilePath, FacesContextFactory facesContextFactory, Lifecycle lifecycle) {
			_facesContextFactory = facesContextFactory;
			_lifecycle = lifecycle;
			_executionFilePath = executionFilePath;
		}

		public void ProcessRequest (HttpContext context) {
			ServletWorkerRequest wr = (ServletWorkerRequest) ((IServiceProvider) context).GetService (typeof (HttpWorkerRequest));
			ServletContext servletContext = wr.GetContext ();
			HttpServletRequest request = wr.ServletRequest;
			HttpServletResponse response = wr.ServletResponse;

			FacesContext facesContext = ServletFacesContext.GetFacesContext (_facesContextFactory, servletContext, request, response, _lifecycle, context, _executionFilePath);
			try {
				try {
					_lifecycle.execute (facesContext);
#if DEBUG
					Console.WriteLine ("FacesPageHandler: before render");
#endif
					_lifecycle.render (facesContext);
#if DEBUG
					Console.WriteLine ("FacesPageHandler: after render");
#endif
				}
				catch (FacesException fex) {
					Exception inner = fex.InnerException;
					if (inner != null)
						TypeUtils.Throw (inner);
					throw;
				}
			}
			finally {
				facesContext.release ();
			}
		}
	}
}
