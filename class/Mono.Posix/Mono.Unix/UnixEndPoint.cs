//
// Mono.Unix.UnixEndPoint: EndPoint derived class for AF_UNIX family sockets.
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.Unix
{
	[Serializable]
	public class UnixEndPoint : EndPoint
	{
		string filename;
		
		public UnixEndPoint (string filename)
		{
			this.filename = filename;
		}
		
		public string Filename {
			get {
				return(filename);
			}
			set {
				filename=value;
			}
		}

		public override AddressFamily AddressFamily {
			get { return AddressFamily.Unix; }
		}

		public override EndPoint Create (SocketAddress socketAddress)
		{
			int size = socketAddress.Size;
			byte [] bytes = new byte [size];
			/*
			 * Should also check this
			 *
			int addr = (int) AddressFamily.Unix;
			if (socketAddress [0] != (addr & 0xFF))
				throw new ArgumentException ("socketAddress is not a unix socket address.");

			if (socketAddress [1] != ((addr & 0xFF00) >> 8))
				throw new ArgumentException ("socketAddress is not a unix socket address.");
			 */

			for (int i = 2; i < size - 2; i++) {
				bytes [i] = socketAddress [i];
			}

			string name = Encoding.Default.GetString (bytes);
			return new UnixEndPoint (name);
		}

		public override SocketAddress Serialize ()
		{
			byte [] bytes = Encoding.Default.GetBytes (filename);
			SocketAddress sa = new SocketAddress (AddressFamily, bytes.Length + 2);
			// sa [0] -> family low byte, sa [1] -> family high byte
			for (int i = 0; i < bytes.Length; i++)
				sa [i + 2] = bytes [i];

			return sa;
		}

		public override string ToString() {
			return(filename);
		}
	}
}

