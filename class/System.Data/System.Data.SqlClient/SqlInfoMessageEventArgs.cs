//
// System.Data.SqlClient.SqlInfoMessageEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;

namespace System.Data.SqlClient
{
	public sealed class SqlInfoMessageEventArgs : EventArgs
	{
		[MonoTODO]
		public SqlErrorCollection Errors {
			get { 
				throw new NotImplementedException (); 
			}
		}	

		[MonoTODO]
		public string Message 
		{
			get { 
				throw new NotImplementedException (); 
			}
		}	

		[MonoTODO]
		public string Source {
			get { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public override string ToString() {
			// representation of InfoMessage event
			return "'ToString() for SqlInfoMessageEventArgs Not Implemented'";
		}

		//[MonoTODO]
		//~SqlInfoMessageEventArgs() {
			// FIXME: destructor needs to release resources
		//}
	}
}
