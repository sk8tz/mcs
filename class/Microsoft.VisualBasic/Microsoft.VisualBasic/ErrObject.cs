//
// ErrObject.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class ErrObject {
		// Declarations
		// Constructors
		// Properties
		public System.Int32 HelpContext { get {return 0;} set {} }
		public System.Int32 LastDllError { get {return 0;} }
		public System.Int32 Number { get {return 0;} set {} }
		public System.Int32 Erl { get {return 0;} }
		public System.String Source { get {return "";} set {} }
		public System.String HelpFile { get {return "";} set {} }
		public System.String Description { get {return "";} set {} }
		// Methods
		public System.Exception GetException () { return null;}
		public void Clear () { }
		public void Raise (System.Int32 Number, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object Source, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object Description, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object HelpFile, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object HelpContext) { }
		// Events
	};
}
