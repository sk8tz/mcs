//
// Interaction.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class Interaction {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Int32 Shell (System.String Pathname, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(2)] Microsoft.VisualBasic.AppWinStyle Style, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean Wait, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 Timeout) { return 0;}
		public static void AppActivate (System.Int32 ProcessId) { }
		public static void AppActivate (System.String Title) { }
		public static System.String Command () { return "";}
		public static System.String Environ (System.Int32 Expression) { return "";}
		public static System.String Environ (System.String Expression) { return "";}
		public static void Beep () { }
		public static System.String InputBox (System.String Prompt, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String Title, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String DefaultResponse, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 XPos, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 YPos) { return "";}
		public static Microsoft.VisualBasic.MsgBoxResult MsgBox (System.Object Prompt, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.MsgBoxStyle Buttons, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object Title) { return 0;}
		public static System.Object CallByName (System.Object ObjectRef, System.String ProcName, Microsoft.VisualBasic.CallType UseCallType, params System.Object[] Args) { return null;}
		public static System.Object Choose (System.Double Index, params System.Object[] Choice) { return null;}
		public static System.Object IIf (System.Boolean Expression, System.Object TruePart, System.Object FalsePart) { return null;}
		public static System.String Partition (System.Int64 Number, System.Int64 Start, System.Int64 Stop, System.Int64 Interval) { return "";}
		public static System.Object Switch (params System.Object[] VarExpr) { return null;}
		public static void DeleteSetting (System.String AppName, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.String Section, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.String Key) { }
		public static System.String[,] GetAllSettings (System.String AppName, System.String Section) { return null;}
		public static System.String GetSetting (System.String AppName, System.String Section, System.String Key, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String Default) { return "";}
		public static void SaveSetting (System.String AppName, System.String Section, System.String Key, System.String Setting) { }
		public static System.Object CreateObject (System.String ProgId, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String ServerName) { return null;}
		public static System.Object GetObject ([System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.String PathName, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.String Class) { return null;}
		// Events
	};
}
