//
// System.Security.Principal.GenericIdentity.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Security.Principal {

	public class GenericIdentity : IIdentity {
		string user_name;
		string authentication_type;
		
		public GenericIdentity (string user_name, string authentication_type)
		{
			this.user_name = user_name;
			this.authentication_type = authentication_type;
		}

		public GenericIdentity (string name)
		{
			this.user_name = user_name;
		}

		public string AuthenticationType {
			get {
				return authentication_type;
			}
		}

		public string Name {
			get {
				return user_name;
			}
		}

		public bool IsAuthenticated {
			get {
				return true;
			}
		}
	}
}
