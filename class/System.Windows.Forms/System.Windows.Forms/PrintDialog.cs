//
// System.Windows.Forms.PrintDialog
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//	Implemented by Jordi Mas i Hern�ndez <jmas@softcatala.org>
//
// (C) 2002-3 Ximian, Inc
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Drawing.Printing;
using System.Runtime.Remoting;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

        public sealed class PrintDialog : CommonDialog {
        	
		private bool allowPrintToFile;	
		private bool allowSelection;	
		private bool allowSomePages;	
		private bool showHelp;
		private bool showNetwork;	
		private bool printToFile;
		private PrintDocument document = null;
		private PrinterSettings printerSettings;

		//
		//  --- Constructor
		//		
		public PrintDialog(){
			Reset();			
		}

		//
		//  --- Public Properties
		//		
		public bool AllowPrintToFile {
			get {return allowPrintToFile;}
			set {allowPrintToFile = value;}
		}
		
		public bool AllowSelection {
			get {return allowSelection;}
			set {allowSelection = value;}
		}
		
		public bool AllowSomePages {
			get {return allowSomePages;}
			set {allowSomePages = value;}
		}

		
		public PrintDocument Document {
			get {return document;}
			set {document = value;}
		}		
		
		public PrinterSettings PrinterSettings {
			get {return printerSettings;}
			set {printerSettings = value;}
		}
		
		public bool PrintToFile {
			get {return printToFile;}
			set {printToFile = value;}
		}

		public bool ShowHelp {		
			get {return showHelp;}
			set {showHelp = value;}
		}
		
		public bool ShowNetwork {
			get {return showNetwork;}
			set {showNetwork = value;}
		}
		
		

		//
		//  --- Public Methods
		//
				
		public override void Reset(){
			
			allowPrintToFile = true;	
			allowSelection = false;	
			allowSomePages = false;	
			showHelp = false;
			showNetwork = true;	
			printToFile = false;
		}

		//
		//  --- Protected Methods
		//		
		protected override bool RunDialog(IntPtr hwndOwner)	{			
			
			PRINTDLG pdlg = new PRINTDLG();
			pdlg.hwndOwner = hwndOwner;						
			pdlg.lStructSize  = (uint)Marshal.SizeOf(pdlg);				
			pdlg.hDevMode = (IntPtr)0;
			pdlg.hDevNames = (IntPtr)0;
			pdlg.nFromPage = 0;
			pdlg.nToPage = 0;
			pdlg.nMinPage = 0;
			pdlg.nMaxPage = 0;	
			pdlg.nCopies = 0;
			pdlg.hInstance = (IntPtr)0;
			pdlg.lCustData = (IntPtr)0;
			pdlg.lpfnPrintHook = (IntPtr)0;
			pdlg.lpfnSetupHook = (IntPtr)0;
			pdlg.lpPrintTemplateName = (IntPtr)0;
			pdlg.lpSetupTemplateName = (IntPtr)0;
			pdlg.hPrintTemplate = (IntPtr)0;
			pdlg.hSetupTemplate = (IntPtr)0;
			pdlg.Flags = 0;			
			
			if (!allowPrintToFile) pdlg.Flags |=  PrintDlgFlags.PD_DISABLEPRINTTOFILE;		
			if (!allowSelection) pdlg.Flags |=  PrintDlgFlags.PD_NOSELECTION;				
			if (!allowSomePages) pdlg.Flags |=  PrintDlgFlags.PD_NOPAGENUMS;
			if (showHelp) pdlg.Flags |=  PrintDlgFlags.PD_SHOWHELP;				
			if (!showNetwork) pdlg.Flags |=  PrintDlgFlags.PD_NONETWORKBUTTON;
			if (!printToFile) pdlg.Flags |=  PrintDlgFlags.PD_DISABLEPRINTTOFILE;
									
			IntPtr lfBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(pdlg));
      		Marshal.StructureToPtr(pdlg, lfBuffer, false);			
			
			if (Win32.PrintDlg(lfBuffer)){
				
				pdlg = (PRINTDLG)Marshal.PtrToStructure (lfBuffer, typeof (PRINTDLG));
				
				// TODO: PrinterSettings is not yet implemented, we should pass the values
				// to that struct
				//PrinterSettings.Copies =  pdlg.nCopies;
				}
			
			return true;
		}
	 }
}
