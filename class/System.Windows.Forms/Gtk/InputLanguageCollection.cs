//
// System.Windows.Forms.InputLanguageCollection.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System.Collections;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class InputLanguageCollection : ReadOnlyCollectionBase {

		private InputLanguageCollection(){//For signiture compatablity. Prevents the auto creation of public construct
		}

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public InputLanguage this[int index] {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public bool Contains(InputLanguage value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo(InputLanguage[] array, int index) {
			//FIXME:
		}

		[MonoTODO]
		public int IndexOf(InputLanguage value) {
			throw new NotImplementedException ();
		}

	}
}
