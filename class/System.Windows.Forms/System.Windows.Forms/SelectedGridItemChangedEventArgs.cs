//
// System.Windows.Forms.SelectedGridItemChangedEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

        public class SelectedGridItemChangedEventArgs : EventArgs {
			GridItem old;
			GridItem newGridItem;

		//
		//  --- Constructor
		//
		public SelectedGridItemChangedEventArgs(GridItem old, GridItem newGridItem)
		{
			this.newGridItem = newGridItem;
			this.old = old;
		}

		//
		//  --- Public Properties
		//
		public GridItem NewSelection {
			get {
				return newGridItem;
			}
		}
		public GridItem OldSelection {
			get {
				return old;
			}
		}

		//
		//  --- Public Methods
		//
		//[MonoTODO]
		//public virtual bool Equals(object o);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public static bool Equals(object o1, object o2);
		//{
		//	throw new NotImplementedException ();
		//}
	 }
}
