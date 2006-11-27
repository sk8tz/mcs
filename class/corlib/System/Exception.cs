//
// System.Exception.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ClassInterface (ClassInterfaceType.AutoDual)]
#if NET_2_0
	[ComVisible(true)]
#endif
	public class Exception : ISerializable 
#if NET_2_0
	, _Exception
#endif
	{
		IntPtr [] trace_ips;
		Exception inner_exception;
		internal string message;
		string help_link;
		string class_name;
		string stack_trace;
		string remote_stack_trace;
		int remote_stack_index;
		internal int hresult = unchecked ((int)0x80004005);
		string source;

		public Exception ()
		{
		}

		public Exception (string msg)
		{
			message = msg;
		}

		protected Exception (SerializationInfo info, StreamingContext sc)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			class_name          = info.GetString ("ClassName");
			message             = info.GetString ("Message");
			help_link           = info.GetString ("HelpURL");
			stack_trace         = info.GetString ("StackTraceString");
			remote_stack_trace  = info.GetString ("RemoteStackTraceString");
			remote_stack_index  = info.GetInt32  ("RemoteStackIndex");
			hresult             = info.GetInt32  ("HResult");
			source              = info.GetString ("Source");
			inner_exception     = (Exception) info.GetValue ("InnerException", typeof (Exception));
		}

		public Exception (string msg, Exception e)
		{
			inner_exception = e;
			message = msg;
		}

		public Exception InnerException {
			get { return inner_exception; }
		}

		public virtual string HelpLink {
			get { return help_link; }
			set { help_link = value; }
		}

		protected int HResult {
			get { return hresult; }
			set { hresult = value; }
		}

		internal void SetMessage (string s)
		{
			message = s;
		}

		internal void SetStackTrace (string s)
		{
			stack_trace = s;
		}

		public virtual string Message {
			get {
				if (message == null)
					message = string.Format (Locale.GetText ("Exception of type {0} was thrown."), GetType ().ToString());

				return message;
			}
		}

		public virtual string Source {
#if ONLY_1_1
			[ReflectionPermission (SecurityAction.Assert, TypeInformation = true)]
#endif
			get {
				if (source == null) {
					StackTrace st = new StackTrace (this, true);
					if (st.FrameCount > 0) {
						StackFrame sf = st.GetFrame (0);
						if (st != null) {
							MethodBase method = sf.GetMethod ();
							if (method != null) {
								source = method.DeclaringType.Assembly.UnprotectedGetName ().Name;
							}
						}
					}
				}

                                // source can be null
				return source;
			}

			set {
				source = value;
			}
		}

		public virtual string StackTrace {
			get {
				if (stack_trace == null) {
					if (trace_ips == null)
						/* Not thrown yet */
						return null;

					StackTrace st = new StackTrace (this, 0, true, true);

					StringBuilder sb = new StringBuilder ();

					string newline = String.Format ("{0}  {1} ", Environment.NewLine, Locale.GetText ("at"));
					string unknown = Locale.GetText ("<unknown method>");

					for (int i = 0; i < st.FrameCount; i++) {
						StackFrame frame = st.GetFrame (i);
						if (i == 0)
							sb.AppendFormat ("  {0} ", Locale.GetText ("at"));
						else
							sb.Append (newline);

						if (frame.GetMethod () == null) {
							string internal_name = frame.GetInternalMethodName ();
							if (internal_name != null)
								sb.Append (internal_name);
							else
								sb.AppendFormat ("<0x{0:x5}> {1}", frame.GetNativeOffset (), unknown);
						} else {
							sb.Append (GetFullNameForStackTrace (frame.GetMethod ()));

							if (frame.GetILOffset () == -1)
								sb.AppendFormat (" <0x{0:x5}> ", frame.GetNativeOffset ());
							else
								sb.AppendFormat (" [0x{0:x5}] ", frame.GetILOffset ());

							string fileName = frame.GetFileName ();
							if (fileName != null)
								sb.AppendFormat ("in {0}:{1} ", fileName, frame.GetFileLineNumber ());
							}
					}
					stack_trace = sb.ToString ();
				}

				return stack_trace;
			}
		}

		public MethodBase TargetSite {
#if ONLY_1_1
			[ReflectionPermission (SecurityAction.Demand, TypeInformation = true)]
#endif
			get {
				StackTrace st = new StackTrace (this, true);
				if (st.FrameCount > 0)
					return st.GetFrame (0).GetMethod ();
				
				return null;
			}
		}

#if NET_2_0
		private IDictionary _data;

		public virtual IDictionary Data {
			get {
				if (_data == null) {
					// default to empty dictionary
					_data = (IDictionary) new Hashtable ();
				}
				return _data;
			}
		}
#endif

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

#if ONLY_1_1
		[ReflectionPermission (SecurityAction.Assert, TypeInformation = true)]
#endif
		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			if (class_name == null)
				class_name = GetType ().FullName;

			info.AddValue ("ClassName", class_name);
			info.AddValue ("Message", message);
			info.AddValue ("InnerException", inner_exception);
			info.AddValue ("HelpURL", help_link);
			info.AddValue ("StackTraceString", StackTrace);
			info.AddValue ("RemoteStackTraceString", remote_stack_trace);
			info.AddValue ("RemoteStackIndex", remote_stack_index);
			info.AddValue ("HResult", hresult);
			info.AddValue ("Source", Source);
			info.AddValue ("ExceptionMethod", null);
		}

#if ONLY_1_1
		[ReflectionPermission (SecurityAction.Assert, TypeInformation = true)]
#endif
		public override string ToString ()
		{
			System.Text.StringBuilder result = new System.Text.StringBuilder (this.GetType ().FullName);
			result.Append (": ").Append (Message);

			if (null != remote_stack_trace)
				result.Append (remote_stack_trace);
				
			if (inner_exception != null) 
			{
				result.Append (" ---> ").Append (inner_exception.ToString ());
				result.Append (Locale.GetText ("--- End of inner exception stack trace ---"));
				result.Append (Environment.NewLine);
			}

			if (StackTrace != null)
				result.Append (Environment.NewLine).Append (StackTrace);
			return result.ToString();
		}

		internal Exception FixRemotingException ()
		{
			string message = (0 == remote_stack_index) ?
				Locale.GetText ("{0}{0}Server stack trace: {0}{1}{0}{0}Exception rethrown at [{2}]: {0}") :
				Locale.GetText ("{1}{0}{0}Exception rethrown at [{2}]: {0}");
			string tmp = String.Format (message, Environment.NewLine, StackTrace, remote_stack_index);

			remote_stack_trace = tmp;
			remote_stack_index++;

			stack_trace = null;

			return this;
		}

		internal string GetFullNameForStackTrace (MethodBase mi)
		{
			string parms = String.Empty;
			ParameterInfo[] p = mi.GetParameters ();
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					parms = parms + ", ";
				string paramName = (p [i].Name == null) ? String.Empty : (" " + p [i].Name);
				Type pt = p[i].ParameterType;
				if (pt.IsClass && pt.Namespace != String.Empty)
					parms = parms + pt.Namespace + "." + pt.Name + paramName;
				else
					parms = parms + pt.Name + paramName;
			}

			string generic = String.Empty;
#if NET_2_0 || BOOTSTRAP_NET_2_0
			if (mi.IsGenericMethod) {
				Type[] gen_params = mi.GetGenericArguments ();
				generic = "[";
				for (int j = 0; j < gen_params.Length; j++) {
					if (j > 0)
						generic += ",";
					generic += gen_params [j].Name;
				}
				generic += "]";
			}
#endif
			return mi.DeclaringType.ToString () + "." + mi.Name + generic + " (" + parms + ")";
		}

#if NET_2_0
		//
		// The documentation states that this is available in 1.x,
		// but it was not available (MemberRefing this would fail)
		// and it states the signature is `override sealed', but the
		// correct value is `newslot' 
		//
		public new Type GetType ()
		{
			return base.GetType ();
		}
#endif
	}
}
