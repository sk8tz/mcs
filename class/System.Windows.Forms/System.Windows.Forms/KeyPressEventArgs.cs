//
// System.Windows.Forms.KeyPressEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class KeyPressEventArgs : EventArgs {

		//
		//  --- Constructor
		//

		[MonoTODO]
		public KeyPressEventArgs (char keyChar)
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public bool Handled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public char KeyChar {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
	}
}
