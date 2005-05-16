//
// System.Threading.Interlocked.cs
//
// Author:
//	  Patrik Torstensson (patrik.torstensson@labs2.com)
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

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
using System.Runtime.CompilerServices;

#if NET_2_0
using System.Runtime.ConstrainedExecution;
#endif

namespace System.Threading
{
	public
#if NET_2_0
	static
#else
	sealed
#endif
	class Interlocked 
	{

#if !NET_2_0
		private Interlocked () {}
#endif

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int CompareExchange(ref int location1, int value, int comparand);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static object CompareExchange(ref object location1, object value, object comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static float CompareExchange(ref float location1, float value, float comparand);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Decrement(ref int location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Decrement(ref long location);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Increment(ref int location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Increment(ref long location);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Exchange(ref int location1, int value);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static object Exchange(ref object location1, object value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static float Exchange(ref float location1, float value);

#if NET_2_0
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long CompareExchange(ref long location1, long value, long comparand);

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr CompareExchange(ref IntPtr location1, IntPtr value, IntPtr comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static double CompareExchange(ref double location1, double value, double comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Exchange(ref long location1, long value);

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr Exchange(ref IntPtr location1, IntPtr value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static double Exchange(ref double location1, double value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Read(ref long location1);

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Add(ref int location1, int add);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Add(ref long location1, long add);
#endif
	}
}

