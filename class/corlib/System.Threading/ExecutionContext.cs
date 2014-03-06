// 
// System.Threading.ExecutionContext.cs
//
// Authors:
//	Lluis Sanchez (lluis@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//  Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
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

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Runtime.Remoting.Messaging;
using System.Collections.Generic;

namespace System.Threading {
	[Serializable]
	public sealed class ExecutionContext : ISerializable
#if NET_4_0
		, IDisposable
#endif
	{
#if !MOBILE
		private SecurityContext _sc;
#endif
		private LogicalCallContext _lcc;
		private bool _suppressFlow;
		private bool _capture;
		Dictionary<string, object> local_data;

		internal ExecutionContext ()
		{
		}

		private ExecutionContext (ExecutionContext ec)
		{
#if !MOBILE
			if (ec._sc != null)
				_sc = new SecurityContext (ec._sc);
#endif
			if (ec._lcc != null)
				_lcc = (LogicalCallContext) ec._lcc.Clone ();

			_suppressFlow = ec._suppressFlow;
			_capture = true;
		}
		
		[MonoTODO]
		internal ExecutionContext (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public static ExecutionContext Capture ()
		{
			return Capture (true, false);
		}
		
		internal static ExecutionContext Capture (bool captureSyncContext, bool nullOnEmpty)
		{
			ExecutionContext ec = Current;
			if (ec.FlowSuppressed)
				return null;

			if (nullOnEmpty && ec._sc == null && (ec._lcc == null || !ec._lcc.HasInfo))
				return null;

			ExecutionContext capture = new ExecutionContext (ec);
#if !MOBILE
			if (SecurityManager.SecurityEnabled)
				capture.SecurityContext = SecurityContext.Capture ();
#endif
			return capture;
		}
		
		public ExecutionContext CreateCopy ()
		{
			if (!_capture)
				throw new InvalidOperationException ();

			return new ExecutionContext (this);
		}
		
#if NET_4_0
		public void Dispose ()
		{
#if !MOBILE
			if (_sc != null)
				_sc.Dispose ();
#endif
		}
#endif

		[MonoTODO]
		[ReflectionPermission (SecurityAction.Demand, MemberAccess = true)]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			throw new NotImplementedException ();
		}
		
		// internal stuff

		internal LogicalCallContext LogicalCallContext {
			get {
				if (_lcc == null)
					_lcc = new LogicalCallContext ();
				return _lcc;
			}
			set {
				_lcc = value;
			}
		}

		internal Dictionary<string, object> DataStore {
			get {
				if (local_data == null)
					local_data = new Dictionary<string, object> ();
				return local_data;
			}
			set {
				local_data = value;
			}
		}

#if !MOBILE
		internal SecurityContext SecurityContext {
			get {
				if (_sc == null)
					_sc = new SecurityContext ();
				return _sc;
			}
			set { _sc = value; }
		}
#endif

		internal bool FlowSuppressed {
			get { return _suppressFlow; }
			set { _suppressFlow = value; }
		}

		public static bool IsFlowSuppressed ()
		{
			return Current.FlowSuppressed;
		}

		public static void RestoreFlow ()
		{
			ExecutionContext ec = Current;
			if (!ec.FlowSuppressed)
				throw new InvalidOperationException ();

			ec.FlowSuppressed = false;
		}

		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		public static void Run (ExecutionContext executionContext, ContextCallback callback, object state)
		{
			if (executionContext == null) {
				throw new InvalidOperationException (Locale.GetText (
					"Null ExecutionContext"));
			}

			var prev = Current;
			try {
				Thread.CurrentThread.ExecutionContext = executionContext;
				callback (state);
			} finally {
				Thread.CurrentThread.ExecutionContext = prev;
			}
		}

		public static AsyncFlowControl SuppressFlow ()
		{
			Thread t = Thread.CurrentThread;
			t.ExecutionContext.FlowSuppressed = true;
			return new AsyncFlowControl (t, AsyncFlowControlType.Execution);
		}

		internal static LogicalCallContext CreateLogicalCallContext (bool createEmpty)
		{
			var lcc = Current._lcc;
			if (lcc == null) {
				if (createEmpty)
					lcc = new LogicalCallContext ();

				return lcc;
			}

			return (LogicalCallContext) lcc.Clone ();
		}

		internal void FreeNamedDataSlot (string name)
		{
			if (_lcc != null)
				_lcc.FreeNamedDataSlot (name);

			if (local_data != null)
				local_data.Remove (name);
		}

		internal static ExecutionContext Current {
			get {
				return Thread.CurrentThread.ExecutionContext;
			}
		}
	}
}