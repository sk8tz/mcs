// System.Drawing.Design.PropertyValueUIHandler.cs
//
// Author:
// 	Alejandro S�nchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro S�nchez Acosta
//

using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace System.Drawing.Design
{
	
	[Serializable]
	public delegate void PropertyValueUIHandler (ITypeDescriptorContext context, PropertyDescriptor propDesc, ArrayList valueUIItemList);
	
}
