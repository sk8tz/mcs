// System.Drawing.Design.ToolboxComponentsCreatingEventArgs.cs
// 
// Author:
//      Alejandro S�nchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro S�nchez Acosta
// 

namespace System.Drawing.Design
{
	public class ToolboxComponentsCreatingEventArgs : EventArgs
	{
		private IDesignerHost host;
		
		public ToolboxComponentsCreatingEventArgs (IDesignerHost host)
		{
			this.host = host;
		}

		public IDesignerHost DesignerHost {
			get {
				return host;				
			}
		}
	}	
}


