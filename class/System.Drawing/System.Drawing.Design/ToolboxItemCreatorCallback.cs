// System.Drawing.Design.ToolboxItemCreatorCallback.cs
// 
// Author:
//      Alejandro S�nchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro S�nchez Acosta
// 

namespace System.Drawing.Design
{
	[Serializable]
	public delegate ToolboxItem ToolboxItemCreatorCallback(
				   object serializedObject,
		        	   string format);
}
