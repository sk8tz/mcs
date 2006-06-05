#if NET_2_0
using System;
using System.Web;
using System.Web.Hosting;

namespace NunitWeb
{
	public class MyWorkerRequest: SimpleWorkerRequest
	{
		PageDelegates _pd;
		Exception _exception;
		bool _initInvoked;

		public PageDelegates Delegates
		{ get { return _pd; } }

		public Exception Exception
		{
			get { return _exception; }
			set { _exception = value; }
		}

#if !BUG_78583_FIXED
		System.Threading.AutoResetEvent done;
		public System.Threading.AutoResetEvent Done {
			get {
				if (done == null)
					done = new System.Threading.AutoResetEvent (false);
				return done;
			}
		}
#endif

		public bool InitInvoked
		{
			get { return _initInvoked; }
			set { _initInvoked = value; }
		}
		public MyWorkerRequest (PageDelegates pd, string page, string query, System.IO.TextWriter output)
			: base (page, query, output)
		{
			_pd = pd;
			_initInvoked = false;
			//_delegateInvoked = false;
			_exception = null;
		}
	}
}
#endif
