// System.Drawing.Design.IToolboxUser.cs
// 
// Author:
//      Alejandro S�nchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro S�nchez Acosta
// 

namespace System.Drawing.Design
{
	public interface IToolboxUser
	{
		bool GetToolSupported (ToolboxItem tool);

		void ToolPicked (ToolboxItem tool);
	}
}
