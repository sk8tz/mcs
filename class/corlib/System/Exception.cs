//
// System.Exception.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;
using System.Reflection;

namespace System {

	public class Exception : ISerializable {
		Exception inner_exception;
		string message;
		string help_link;
		string stack_trace = "TODO: implement stack traces";
		int hresult;
		private string source;
		
		public Exception ()
		{
			inner_exception = null;
			message = "";
		}

		public Exception (string msg)
		{
			inner_exception = null;
			message = msg;
		}

		protected Exception (SerializationInfo info, StreamingContext sc)
		{
			if (info == null){
				throw new ArgumentNullException ("info");
			}
			
			// TODO: Implement the restoration of an Exception
			// from a stream.
		}

		public Exception (string msg, Exception e)
		{
			inner_exception = e;
			message = msg;
		}

		public Exception InnerException {
			get {
				return inner_exception;
			}
		}

		public string HelpLink {
			get {
				return help_link;
			}

			set {
				help_link = value;
			}
		}

		protected int HResult {
			get {
				return hresult;
			}

			set {
				hresult = value;
			}
		}

		public string Message {
			get {
				return message;
			}
		}

		public string Source {
			get {
				// TODO: if source is null, we must return
				// the name of the assembly where the error
				// originated.
				return source;
			}

			set {
				source = value;
			}
		}

		public string StackTrace {
			get {
				return stack_trace;
			}
		}

		public MethodBase TargetSite {
			get {
				// TODO: Implement this.
				return null;
			}
		}

		public virtual Exception GetBaseException ()
		{
			Exception inner = inner_exception;
				
			while (inner != null){
				if (inner.InnerException != null)
					inner = inner.InnerException;
				else
					return inner;
			}

			return null;
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// TODO: implement me.
		}

		public override string ToString ()
		{
			return this.GetType ().FullName + "\n" +
				message +
				GetBaseException ().GetType ().FullName +
				stack_trace;
		}
	}
}

