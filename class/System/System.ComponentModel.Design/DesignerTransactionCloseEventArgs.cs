// System.ComponentModel.Design.DesignerTransactionCloseEventArgs.cs
//
// Author:
// 	Alejandro S�nchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro S�nchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public class DesignerTransactionCloseEventArgs : EventArgs
	{
		private bool commit;
		public DesignerTransactionCloseEventArgs (bool commit) {
			this.commit = commit;
		}

		public bool TransactionCommitted 
		{
			get {
				return commit;
			}
		}
	}
}
