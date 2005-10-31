// ::MONO
//
// System.Globalization.CalendarWeekRule.cs
//
// Copyright (C) Wictor Wil�n 2001 (wictor@iBizkit.se)
//
// Contributors: Wictor Wil�n
//
// Revisions
// 2001-09-14:	First draft
// 2001-09-15:	First release

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;

namespace System.Globalization
{
	/// <summary>
	/// The System.Globalization.CalendarWeekRule enumeration
	/// </summary>
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible(true)]
#endif
	public enum CalendarWeekRule
	{
		FirstDay = 0,
		FirstFullWeek = 1,
		FirstFourDayWeek = 2

	}

}
