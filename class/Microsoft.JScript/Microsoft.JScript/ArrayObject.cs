//
// ArrayObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;

namespace Microsoft.JScript {

	public class ArrayObject : JSObject {

		public virtual Object length {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}