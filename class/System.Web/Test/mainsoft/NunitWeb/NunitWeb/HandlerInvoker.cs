using System;

namespace MonoTests.SystemWeb.Framework
{
	public delegate void HandlerDelegate ();

	[Serializable]
	public class HandlerInvoker:BaseInvoker
	{
		HandlerDelegate callback;

		public HandlerInvoker (HandlerDelegate callback)
		{
			this.callback = callback;
		}

		public override void DoInvoke (object param)
		{
			base.DoInvoke (param);
			callback ();
		}

		public override string GetDefaultUrl ()
		{
			return "page.fake";
		}
	}
}
