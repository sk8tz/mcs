// System.Configuration.Install.InstallEventArgs.cs
//
// Author: 
// 	Alejandro S�nchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro S�nchez Acosta
// 

using System.Collections;

namespace System.Configuration.Install
{
	public class InstallEventArgs : EventArgs
	{
		private IDictionary savedstate;
		
		public InstallEventArgs() {
		}

		public InstallEventArgs (IDictionary savedState) {
			this.savedstate = savedState;
		}
		
		public IDictionary SavedState {
			get {
				return savedstate;	
			}

			set {
				savedstate = value;
			}
		}		
	}
}
