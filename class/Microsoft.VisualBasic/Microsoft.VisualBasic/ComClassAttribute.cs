//
// ComClassAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[System.AttributeUsageAttribute(System.AttributeTargets.Class)] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class ComClassAttribute : System.Attribute {
		// Declarations
		// Constructors
		ComClassAttribute(System.String _ClassID) {}
		ComClassAttribute(System.String _ClassID, System.String _InterfaceID) {}
		ComClassAttribute(System.String _ClassID, System.String _InterfaceID, System.String _EventId) {}
		// Properties
		public System.String EventID { get {return "";} }
		public System.Boolean InterfaceShadows { get {return false;} set {} }
		public System.String ClassID { get {return "";} }
		public System.String InterfaceID { get {return "";} }
		// Methods
		// Events
	};
}
