// System.Drawing.Design.IPropertyValueUIService.cs
//
// Author:
// 	Alejandro S�nchez Acosta  <raciel@es.gnu.org>
// 	
// (C) Alejandro S�nchez Acosta
// 

using System.Drawing;
using System.ComponentModel;

namespace System.Drawing.Design
{
	public interface IPropertyValueUIService
	{
		
		#region Methods		
		void AddPropertyValueUIHandler (PropertyValueUIHandler newHandler);
		PropertyValueUIItem[] GetPropertyUIValueItems (ITypeDescriptorContext context, PropertyDescriptor propDesc);

		void NotifyPropertyValueUIItemsChanged ();

		void RemovePropertyValueUIHandler (PropertyValueUIHandler newHandler);
		#endregion Methods

		#region Events
		event EventHandler PropertyUIValueItemsChanged;
		#endregion Events
	}
}

