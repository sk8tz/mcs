// System.ComponentModel.Design.IDesigner.cs
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
	public interface IDesigner : IDisposable
	{
		IComponent Component {get;}

		DesignerVerbCollection Verbs {get;}

		void DoDefaultAction ();

		void Initialize (IComponent component);		
	}
}
