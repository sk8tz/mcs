// System.ComponentModel.Design.IServiceContainer.cs
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
	public interface IServiceContainer : IServiceProvider
	{
		void AddService(
				   Type serviceType,
				      object serviceInstance
			       );

		void AddService(
				   Type serviceType,
				      ServiceCreatorCallback callback
			       );

		void AddService(
				   Type serviceType,
				      object serviceInstance,
				         bool promote
			       );

		void AddService(
				   Type serviceType,
				      ServiceCreatorCallback callback,
				         bool promote
			       );

		void RemoveService(
				   Type serviceType
				);

		void RemoveService(
				   Type serviceType,
				      bool promote
				);
	}
}
