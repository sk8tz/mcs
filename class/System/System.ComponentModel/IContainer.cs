//
// System.ComponentModel.IContainer.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.ComponentModel {

	public interface IContainer {

		ComponentCollection Components {
			get;
		}

		void Add (IComponent component);

		void Add (IComponent component, string name);

		void Remove (IComponent component);
	}
}
