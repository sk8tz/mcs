//
// System.Web.SessionState.ISessionHandler
//
// Authors:
//	Stefan G�rling, (stefan@gorling.se)
//
// (C) 2003 Stefan G�rling
//
// This interface is simple, but as it's internal it shouldn't be hard to change it if we need to.
//
namespace System.Web.SessionState
{
	internal interface ISessionHandler
	{
	      void Dispose ();
	      void Init (HttpApplication context, SessionConfig config);
	      bool UpdateContext (HttpContext context, SessionStateModule module);
	      void UpdateHandler (HttpContext context, SessionStateModule module);
	}
}

