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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Ravindra (rkumar@novell.com)
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: FormBorderStyle.cs,v $
// Revision 1.1  2004/09/09 05:47:47  ravindra
// Enums for Form.
//
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[Serializable]
	public enum FormBorderStyle
	{
		None = 0,
		FixedSingle = 1,
		Fixed3D = 2,
		FixedDialog = 3,
		Sizable = 4,
		FixedToolWindow = 5,
		SizableToolWindow = 6
	}
}
