//
// System.Reflection.Emit/Label.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System.Reflection.Emit {
	[Serializable]
	public struct Label {
		internal int label;

		internal Label (int val) {
			label = val;
		}

		[MonoTODO]
		public override bool Equals (object obj) {
			/* FIXME */
			return false;
		}

		public override int GetHashCode () {
			return label.GetHashCode ();
		}
	}
}
