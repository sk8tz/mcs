// 
// System.Runtime.Remoting.Messaging/AsyncResult.cs 
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Martin Baulig (martin@gnome.org)
//   Dietmar Maurer (dietmar@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting.Messaging {

public class AsyncResult : IAsyncResult, IMessageSink {

	object async_state;
	WaitHandle handle;
	object async_delegate;
	IntPtr data;
	bool sync_completed;
	bool completed;
	bool endinvoke_called;
	object[] out_args;
	object ret_val;
	Exception exception;
	IMessageCtrl message_ctrl;
	IMessage reply_message;
		
	public virtual object AsyncState
	{
		get {
			return async_state;
		}
	}

	public virtual WaitHandle AsyncWaitHandle
	{
		get {
			return handle;
		}
	}

	public virtual bool CompletedSynchronously
	{
		get {
			return sync_completed;
		}
	}

	public virtual bool IsCompleted
	{
		get {
			return completed;
		}
	}
		
	public bool EndInvokeCalled
	{
		get {
			return endinvoke_called;
		}
		set {
			endinvoke_called = value;
		}
	}
		
	public virtual object AsyncDelegate
	{
		get {
			return async_delegate;
		}
	}

	public IMessageSink NextSink {
		get {
			return null;
		}
	}

	public virtual IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
	{
		// Never called
		throw new NotSupportedException ();
	}

	public virtual IMessage GetReplyMessage()
	{
		return reply_message;
	}

	public virtual void SetMessageCtrl (IMessageCtrl mc)
	{
		message_ctrl = mc;
	}

	internal void SetCompletedSynchronously (bool completed)
	{
		sync_completed = completed;
	}

	public virtual IMessage SyncProcessMessage (IMessage msg)
	{
		IMethodReturnMessage retMsg = (IMethodReturnMessage) msg;
		reply_message = msg;
		out_args = retMsg.OutArgs;
		ret_val = retMsg.ReturnValue;
		exception = retMsg.Exception;

		completed = true;
		NativeEventCalls.SetEvent_internal (handle.Handle);
		// TODO: invoke callback

		return null;
	}
}
}
