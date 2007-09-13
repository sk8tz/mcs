/*
 * System.DateTimeOffset
 *
 * Author(s)
 * 	Stephane Delcroix <stephane@delcroix.org>
 *	Marek Safar (marek.safar@gmail.com)
 *
 *  Copyright (C) 2007 Novell, Inc (http://www.novell.com) 
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


#if NET_2_0 // Introduced by .NET 3.5 for 2.0 mscorlib

using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[StructLayout (LayoutKind.Auto)]
	public struct DateTimeOffset
	{
		public static readonly DateTimeOffset MaxValue = new DateTimeOffset (DateTime.MaxValue);
		public static readonly DateTimeOffset MinValue = new DateTimeOffset (DateTime.MaxValue);
		
		DateTime dt;
		
		public DateTimeOffset (DateTime dateTime)
		{
			this.dt = dateTime;
		}
		
		public DateTimeOffset AddSeconds (double seconds)
		{
			return new DateTimeOffset (this.dt.AddSeconds (seconds));
		}
		
		public DateTime DateTime {
			get {
				return dt;
			}
		}
		
		public static DateTimeOffset Now {
			get {
				return new DateTimeOffset (DateTime.Now);
			}
		}
		
		public static bool operator < (DateTimeOffset dto1, DateTimeOffset dto2)
		{
			return dto1.dt < dto2.dt;
		}
		
		public static bool operator > (DateTimeOffset dto1, DateTimeOffset dto2)
		{
			return dto1.dt > dto2.dt;
		}			
	}
}

#endif
