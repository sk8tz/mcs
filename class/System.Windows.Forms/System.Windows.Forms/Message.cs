//
// System.Windows.Forms.Message.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
//
//TODO uncomment and implment GetLParam.

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

namespace System.Windows.Forms
{
	public struct Message
	{
		private int msg;
		private IntPtr hwnd;
		private IntPtr lparam;
		private IntPtr wparam;
		private IntPtr result;


		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Message objects. The return value is
		///	based on the equivalence of the Msg, HWnd, LParam,
		///	 WParam, and Result properties of the two objects.
		/// </remarks>

		public static bool operator == (Message msg_a, 
			Message msg_b) {

			return ((msg_a.msg == msg_b.msg) &&
				(msg_a.hwnd == msg_b.hwnd) &&
				(msg_a.lparam == msg_b.lparam) &&
				(msg_a.wparam == msg_b.wparam) &&
				(msg_a.result == msg_b.result));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Message objects. The return value is
		///	based on the equivalence of the Msg, HWnd, LParam,
		///	 WParam, and Result properties of the two objects.
		/// </remarks>

		public static bool operator != (Message msg_a, 
			Message msg_b) {
			return ((msg_a.msg != msg_b.msg) ||
				(msg_a.hwnd != msg_b.hwnd) ||
				(msg_a.lparam != msg_b.lparam) ||
				(msg_a.wparam != msg_b.wparam) ||
				(msg_a.result != msg_b.result));
		}
		
		// -----------------------
		// Public Instance Members
		// -----------------------

		public int Msg {
			get{
				return msg;
			}
			set{
				msg = value;
			}
		}

		public IntPtr HWnd {
			get{
				return hwnd;
			}
			set{
				hwnd = value;
			}
		}

		public IntPtr LParam {
			get{
				return lparam;
			}
			set{
				lparam = value;
			}
		}

		public IntPtr WParam {
			get{
				return wparam;
			}
			set{
				wparam = value;
			}
		}

		public IntPtr Result {
			get{
				return result;
			}
			set{
				result = value;
			}
		}

		internal uint HiWordWParam {
			get {
				return ((uint)WParam.ToInt32() & 0xFFFF0000) >> 16;
			}
		}

		internal uint LoWordWParam {
			get {
				return (uint)((uint)WParam.ToInt32() & 0x0000FFFFL);
			}
		}

		internal int HiWordLParam {
			get {
				return (int)(((uint)LParam.ToInt32() & 0xFFFF0000) >> 16);
			}
		}

		internal int LoWordLParam {
			get {
				return LParam.ToInt32() & 0x0000FFFF;
			}
		}

		internal bool IsMouseMessage {
			get {
				return (msg > (int) System.Windows.Forms.Msg.WM_MOUSEFIRST) && (msg < (int) System.Windows.Forms.Msg.WM_MOUSELAST);
			}
		}
		
		internal bool IsKeyboardMessage {
			get {
				return (msg > (int) System.Windows.Forms.Msg.WM_KEYFIRST) && (msg < (int) System.Windows.Forms.Msg.WM_KEYLAST);
			}
		}
		
		public static Message Create(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			Message NewMessage =  new Message();
			NewMessage.msg = msg;
			NewMessage.wparam = wparam;
			NewMessage.lparam = lparam;
			NewMessage.hwnd = hWnd;
			return NewMessage;
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this Message and another object.
		/// </remarks>
		
		public override bool Equals (object o)
		{
			if (!(o is Message))
				return false;

			return (this == (Message) o);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode ()
		{
			return base.GetHashCode();// (int)( msg ^ lparam ^ wparam ^ result ^ whnd);
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the Message as a string.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("[{0},{1},{2},{3},{4}]", msg.ToString(), lparam.ToString(), wparam.ToString(), result.ToString(), hwnd.ToString());
		}

//		public object GetLParam(Type cls){
//			//	throw new NotImplementedException ();
//			//return (object) lparam;
//		}
	}
}
