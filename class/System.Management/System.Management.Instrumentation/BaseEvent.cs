//
// System.Management.Instrumentation.BaseEvent
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Management.Instrumentation
{
	[InstrumentationClass (InstrumentationType.Event)]
	public abstract class BaseEvent : IEvent {
		[MonoTODO]
		protected BaseEvent()
		{
		}

		[MonoTODO]
		public void Fire()
		{
			throw new NotImplementedException();
		}
	}
}
