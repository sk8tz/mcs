//
// System.Exception.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;

namespace System {
    
	[Serializable]
	[ClassInterface (ClassInterfaceType.AutoDual)]
	public class Exception : ISerializable 
	{
		IntPtr [] trace_ips;
		Exception inner_exception;
		string message;
		string help_link;
		string class_name;
		string stack_trace = null;
		string remote_stack_trace = "";
		int remote_stack_index = 0;
		int hresult;
		string source;

		public Exception ()
		{
			inner_exception = null;
			message = null;
		}

		public Exception (string msg)
		{
			inner_exception = null;
			message = msg;
		}

		protected Exception (SerializationInfo info, StreamingContext sc)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			class_name		= info.GetString ("ClassName");
			message			= info.GetString ("Message");
			inner_exception	= (Exception) info.GetValue  ("InnerException", typeof (Exception));
			help_link		= info.GetString ("HelpURL");
			stack_trace		= info.GetString ("StackTraceString");
			remote_stack_trace	= info.GetString ("RemoteStackTrace");
			remote_stack_index	= info.GetInt32  ("RemoteStackIndex");
			hresult				= info.GetInt32  ("HResult");
			source				= info.GetString ("Source");
		}

		public Exception (string msg, Exception e)
		{
			inner_exception = e;
			message = msg;
		}

		public Exception InnerException 
		{
			get
			{
				return inner_exception;
			}
		}

		public virtual string HelpLink 
		{
			get 
			{
				return help_link;
			}

			set
			{
				help_link = value;
			}
		}

		protected int HResult 
		{
			get
			{
				return hresult;
			}

			set 
			{
				hresult = value;
			}
		}

		public virtual string Message 
		{
			get 
			{
				if (message == null)
					message = "Exception of type " + GetType () + " was thrown.";

				return message;
			}
		}

		public virtual string Source 
		{
			get 
			{
				if (source == null) {
					StackTrace st = new StackTrace (this, true);
					if (st.FrameCount > 0) {
						StackFrame sf = st.GetFrame (0);
						if (st != null) {
							MethodBase method = sf.GetMethod ();
							if (method != null) {
								source = method.DeclaringType.Assembly.GetName ().Name;
							}
						}
					}
					if (source == null)
						source = "";
				}
				
				return source;
			}

			set 
			{
				source = value;
			}
		}

		public virtual string StackTrace 
		{
			get 
			{
				return stack_trace;
			}
		}

		public MethodBase TargetSite 
		{
			get 
			{
				StackTrace st = new StackTrace (this, true);
				if (st.FrameCount > 0)
					return st.GetFrame (0).GetMethod ();
				
				return null;
			}
		}

		public virtual Exception GetBaseException ()
		{
			Exception inner = inner_exception;
				
			while (inner != null)
			{
				if (inner.InnerException != null)
					inner = inner.InnerException;
				else
					return inner;
			}

			return this;
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("ClassName", class_name);
			info.AddValue ("Message",   message);
			info.AddValue ("InnerException", inner_exception);
			info.AddValue ("HelpURL",   help_link);
			info.AddValue ("StackTraceString", stack_trace);
			info.AddValue ("RemoteStackTrace", remote_stack_trace);
			info.AddValue ("RemoteStackIndex", remote_stack_index);
			info.AddValue ("HResult", hresult);
			info.AddValue ("Source", source);
		}

		public override string ToString ()
		{
			string result = this.GetType ().FullName + ": " + Message;

			if (null != remote_stack_trace)
				result = result + remote_stack_trace;
				
			if (inner_exception != null) 
			{
				result += " ---> " + inner_exception.ToString ();
				result += "--- End of inner exception stack trace ---\n";
			}

			return result + "\n" + stack_trace;
		}

		internal Exception FixRemotingException()
		{
			String tmp = null;

			if (0 == remote_stack_index)
			{
				tmp = "\n\nServer stack trace: \n"
					+ StackTrace 
					+ "\n\nException rethrown at [" + remote_stack_index + "]: \n";
			}
			else
			{
				tmp = StackTrace 
					+ "\n\nException rethrown at [" + remote_stack_index + "]: \n";
			}

			remote_stack_trace = tmp;
			remote_stack_index++;

			stack_trace = null;

			return this;
		}
	}
}
