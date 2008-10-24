// Vector16sb.cs
//
// Author:
//   Rodrigo Kumpera (rkumpera@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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
using System.Runtime.InteropServices;

namespace Mono.Simd
{
	[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 16)]
	[CLSCompliant(false)]
	public struct Vector16sb
	{
		private sbyte v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15;
		public Vector16sb (sbyte v0, sbyte v1, sbyte v2, sbyte v3, sbyte v4, sbyte v5, sbyte v6, sbyte v7, sbyte v8, sbyte v9, sbyte v10, sbyte v11, sbyte v12, sbyte v13, sbyte v14, sbyte v15)
		{
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
			this.v4 = v4;
			this.v5 = v5;
			this.v6 = v6;
			this.v7 = v7;
			this.v8 = v8;
			this.v9 = v9;
			this.v10 = v10;
			this.v11 = v11;
			this.v12 = v12;
			this.v13 = v13;
			this.v14 = v14;
			this.v15 = v15;		}

		public sbyte V0 { get { return v0; } set { v0 = value; } }
		public sbyte V1 { get { return v1; } set { v1 = value; } }
		public sbyte V2 { get { return v2; } set { v2 = value; } }
		public sbyte V3 { get { return v3; } set { v3 = value; } }
		public sbyte V4 { get { return v4; } set { v4 = value; } }
		public sbyte V5 { get { return v5; } set { v5 = value; } }
		public sbyte V6 { get { return v6; } set { v6 = value; } }
		public sbyte V7 { get { return v7; } set { v7 = value; } }
		public sbyte V8 { get { return v8; } set { v8 = value; } }
		public sbyte V9 { get { return v9; } set { v9 = value; } }
		public sbyte V10 { get { return v10; } set { v10 = value; } }
		public sbyte V11 { get { return v11; } set { v11 = value; } }
		public sbyte V12 { get { return v12; } set { v12 = value; } }
		public sbyte V13 { get { return v13; } set { v13 = value; } }
		public sbyte V14 { get { return v14; } set { v14 = value; } }
		public sbyte V15 { get { return v15; } set { v15 = value; } }

		public static unsafe Vector16sb operator + (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte)(*a++ + *b++);
			return res;
		}

		public static unsafe Vector16sb operator - (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte)(*a++ - *b++);
			return res;
		}

		public static unsafe Vector16sb operator & (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte)(*a++ & *b++);
			return res;
		}

		public static unsafe Vector16sb operator | (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte)((uint)*a++ | (uint)*b++);
			return res;
		}

		public static unsafe Vector16sb operator ^ (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte)(*a++ ^ *b++);
			return res;
		}

		public static unsafe Vector16sb UnpackLow (Vector16sb va, Vector16sb vb)
		{
			return new Vector16sb (va.v0, vb.v0, va.v1, vb.v1, va.v2, vb.v2, va.v3, vb.v3, va.v4, vb.v4, va.v5, vb.v5, va.v6, vb.v6, va.v7, vb.v7);
		}

		public static unsafe Vector16sb UnpackHigh (Vector16sb va, Vector16sb vb)
		{
			return new Vector16sb (va.v8, vb.v8, va.v9, vb.v9, va.v10, vb.v10, va.v11, vb.v11, va.v12, vb.v12, va.v13, vb.v13, va.v14, vb.v14, va.v15, vb.v15);
		}

		public static unsafe Vector16sb AddWithSaturation (Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) System.Math.Max (System.Math.Min (*a++ + *b++, sbyte.MaxValue), sbyte.MinValue);
			return res;
		}

		public static unsafe Vector16sb SubWithSaturation (Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) System.Math.Max (System.Math.Min (*a++ - *b++, sbyte.MaxValue), sbyte.MinValue);
			return res;
		}

		/*Requires SSE 4.1*/
		public static unsafe Vector16sb Max (Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) System.Math.Max (*a++, *b++);
			return res;
		}

		/*Requires SSE 4.1*/
		public static unsafe Vector16sb Min (Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) System.Math.Min(*a++, *b++);
			return res;
		}

		public static unsafe int ExtractByteMask (Vector16sb va) {
			int res = 0;
			sbyte *a = (sbyte*)&va;
			for (int i = 0; i < 16; ++i)
				res |= (*a++ & 0x80) >> 7 << i;
			return res;
		}

		public static unsafe Vector16sb CompareEqual (Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) (*a++ == *b++ ? -1 : 0);
			return res;
		}

		public static unsafe Vector16sb CompareGreaterThan (Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) (*a++ > *b++ ? -1 : 0);
			return res;
		}

		public static unsafe explicit operator Vector4f(Vector16sb v)
		{
			Vector4f* p = (Vector4f*)&v;
			return *p;
		}

		public static unsafe explicit operator Vector4ui(Vector16sb v)
		{
			Vector4ui* p = (Vector4ui*)&v;
			return *p;
		}

		public static unsafe explicit operator Vector8us(Vector16sb v)
		{
			Vector8us* p = (Vector8us*)&v;
			return *p;
		}

		public static Vector16sb LoadAligned (ref Vector16sb v)
		{
			return v;
		}

		public static void StoreAligned (ref Vector16sb res, Vector16sb val)
		{
			res = val;
		}

		public static unsafe Vector16sb LoadAligned (Vector16sb *v)
		{
			return *v;
		}

		public static unsafe void StoreAligned (Vector16sb *res, Vector16sb val)
		{
			*res = val;
		}
	}
}
