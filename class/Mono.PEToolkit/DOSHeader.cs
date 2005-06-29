
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
/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	public class DOSHeader {
		
		private readonly int OpenSize = 60;
		private readonly int CloseSize = 64;

		private byte[] open_data; 	// First 60 bytes of data
		private byte[] close_data;	// Last 64 bytes of data

		// File address of new exe header.
		private uint lfanew;

		public DOSHeader ()
		{
			Init ();
		}

		public DOSHeader (BinaryReader reader)
		{
			Read (reader);
		}

		public uint Lfanew {
			get { return lfanew; }
		}

		public void Read (BinaryReader reader)
		{
			open_data = reader.ReadBytes (OpenSize);
			lfanew = reader.ReadUInt32 ();
			close_data = reader.ReadBytes (CloseSize);
		}

		public void Write (BinaryWriter writer)
		{
			writer.Write (open_data);
			writer.Write (lfanew);
			writer.Write (close_data);
		}

		public void Init ()
		{
			open_data = new byte[] { 0x4D, 0x5A, 0x0, 0x0, 0xE7, 0x0, 0x0, 0x0, 
						 0x4, 0x0, 0x0, 0x0, 0xFF, 0xFF, 0x0, 0x0, 
						 0xB8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 
						 0x40, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 
						 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 
						 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 
						 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 
						 0x0, 0x0, 0x0, 0x0 };
			
			close_data = new byte[] { 0xE, 0x1F, 0xBA, 0xE, 0x0, 0xB4, 0x9, 0xCD, 
						  0x21, 0xB8, 0x1, 0x4C, 0xCD, 0x21,0x54, 0x68, 
						  0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72, 
						  0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F, 
						  0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E, 
						  0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20, 
						  0x6D, 0x6F, 0x64, 0x65, 0x2E, 0xD, 0xD, 0xA, 
						  0x24, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

		}

		/// <summary>
		/// </summary>
		/// <param name="writer"></param>
		public void Dump(TextWriter writer)
		{
			writer.WriteLine(
				"New header offset   : {0}",
				lfanew + " (0x" + lfanew.ToString("X") + ")"
			);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}

}

