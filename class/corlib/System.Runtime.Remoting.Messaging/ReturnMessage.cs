//
// System.Runtime.Remoting.Messaging.ReturnMessage.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Runtime.Remoting.Messaging {

	internal class ReturnMessage : IMethodReturnMessage, IMethodMessage {

		MonoMethodMessage msg;
		IMethodCallMessage request;
		
		public ReturnMessage (object returnValue, object [] outArgs,
			       int outArgCount, LogicalCallContext callCtx,
			       IMethodCallMessage request)
		{
			// fixme: why do we need outArgCount?
			msg = new MonoMethodMessage ((MonoMethod)request.MethodBase, outArgs);
			this.request = request;
			msg.rval = returnValue;
			msg.ctx = callCtx;
		}

		public ReturnMessage (Exception exc, IMethodCallMessage request)
		{
			msg = new MonoMethodMessage ((MonoMethod)request.MethodBase, null);
			this.request = request;
			msg.exc = exc;
			msg.ctx = request.LogicalCallContext;
		}
		
		public int ArgCount {
			get {
				return msg.ArgCount;
			}
		}
		
		public object [] Args {
			get {
				return msg.Args;
			}
		}
		
		public bool HasVarArgs {
			get {
				return msg.HasVarArgs;
			}
		}

		public LogicalCallContext LogicalCallContext {
			get {
				return msg.ctx;
			}
		}

		public MethodBase MethodBase {
			get {
				return msg.MethodBase;
			}
		}

		public string MethodName {
			get {
				return msg.MethodName;
			}
		}

		public object MethodSignature {
			get {
				return msg.MethodSignature;
			}
		}

		public string TypeName {
			get {
				return msg.TypeName;
			}
		}

		public string Uri {
			get {
				return msg.Uri;
			}
		}

		public object GetArg (int arg_num)
		{
			return msg.GetArg (arg_num);
		}
		
		public string GetArgName (int arg_num)
		{
			return msg.GetArgName (arg_num);
		}

		public Exception Exception {
			get {
				return msg.exc;
			}
		}

		public int OutArgCount {
			get {
				return msg.OutArgCount;
			}
		}

		public object [] OutArgs {
			get {
				return msg.OutArgs;
			}
		}

		public object ReturnValue {
			get {
				return msg.rval;
			}
		}

		public object GetOutArg (int arg_num)
		{
			return msg.GetOutArg (arg_num);
		}

		public string GetOutArgName (int arg_num)
		{
			return msg.GetOutArgName (arg_num);
		}

	}
}
