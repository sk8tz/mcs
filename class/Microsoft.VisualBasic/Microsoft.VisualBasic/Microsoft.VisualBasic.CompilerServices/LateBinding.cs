//
// LateBinding.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic.CompilerServices {
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class LateBinding {
		// Declarations
		// Constructors
		// Properties
		// Methods
		[System.Diagnostics.DebuggerHiddenAttribute] 
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		public static System.Object LateGet (System.Object o, System.Type objType, System.String name, System.Object[] args, System.String[] paramnames, System.Boolean[] CopyBack) { return null;}
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static void LateSetComplex (System.Object o, System.Type objType, System.String name, System.Object[] args, System.String[] paramnames, System.Boolean OptimisticSet, System.Boolean RValueBase) { }
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static void LateSet (System.Object o, System.Type objType, System.String name, System.Object[] args, System.String[] paramnames) { }
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static System.Object LateIndexGet (System.Object o, System.Object[] args, System.String[] paramnames) { return null;}
		[System.Diagnostics.DebuggerHiddenAttribute] 
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		public static void LateIndexSetComplex (System.Object o, System.Object[] args, System.String[] paramnames, System.Boolean OptimisticSet, System.Boolean RValueBase) { }
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static void LateIndexSet (System.Object o, System.Object[] args, System.String[] paramnames) { }
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static void LateCall (System.Object o, System.Type objType, System.String name, System.Object[] args, System.String[] paramnames, System.Boolean[] CopyBack) { }
		// Events
	};
}
