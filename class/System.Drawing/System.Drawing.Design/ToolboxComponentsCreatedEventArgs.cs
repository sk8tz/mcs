// System.Drawing.Design.ToolboxComponentsCreatedEventArgs.cs
// 
// Author:
//      Alejandro S�nchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro S�nchez Acosta
// 
//

using System.ComponentModel;

namespace System.Drawing.Design
{
	public class ToolboxComponentsCreatedEventArgs : EventArgs
	{
		private IComponent[] components;
		
		public ToolboxComponentsCreatedEventArgs (IComponent[] components) {
			this.components = components;
		}

		public IComponent[] Components {
			get {
				return components;
			}
		}	
	}
}
