//
// System.Windows.Forms.PrintControllerWithStatusDialog
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
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
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

        public class PrintControllerWithStatusDialog : PrintController {

		//
		//  --- Constructor
		//
		[MonoTODO]
			public PrintControllerWithStatusDialog(PrintController underlyingController)
		{
			
		}
		[MonoTODO]
			public PrintControllerWithStatusDialog(PrintController underlyingController, string dialogTitle)
		{
			
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
		{
			//FIXME:
			base.OnEndPage(document, e);
		}
		[MonoTODO]
		public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
		{
			//FIXME:
			base.OnEndPrint(document, e);
		}
		[MonoTODO]
		public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
		{
			//FIXME:
			return base.OnStartPage(document, e);
		}
		[MonoTODO]
		public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
		{
			//FIXME:
			base.OnStartPrint(document, e);
		}
	 }
}
