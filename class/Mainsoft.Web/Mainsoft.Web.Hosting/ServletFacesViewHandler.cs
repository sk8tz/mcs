using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.application;
using javax.servlet.http;
using java.util;
using javax.faces.context;
using javax.faces.component;
using java.lang;
using javax.faces.render;

namespace Mainsoft.Web.Hosting
{
	public class ServletFacesViewHandler : BaseFacesViewHandler
	{
		public ServletFacesViewHandler (ViewHandler viewHandler)
			: base (viewHandler) {
		}
	}
}
