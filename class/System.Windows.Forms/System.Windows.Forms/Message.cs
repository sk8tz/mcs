//
// System.Drawing.Message.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
//TODO uncomment and implment GetLParam.
using System;

namespace System.Windows.Forms {
	[Serializable]
	public struct Message { 

		private int msg;
		private intptr hwnd;
		private intptr lparam;
		private intptr wparam;
		private intptr result;


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
			Message msg_b)
		{
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

		public intptr HWnd {
			get{
				return hwnd;
			}
			set{
				hwnd = value;
			}
		}

		public intptr LParam {
			get{
				return lparam;
			}
			set{
				lparam = value;
			}
		}

		public intptr WParam {
			get{
				return wparam;
			}
			set{
				wparam = value;
			}
		}

		public intptr Result {
			get{
				return result;
			}
			set{
				result = value;
			}
		}

		public static Message create(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam){
			NewMessage = new Message();
			NewMessage.msg = msg;
			NewMessage.wparm = wparam;
			NewMessage.lparam = lparam;
			NewMessage.hWnd = hWnd;
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
			return (int)( msg ^ lparam ^ wparam ^ result ^ whnd);
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
			return String.Format ("[{0},{1},{2},{3},{4}]", msg.ToString, lparam.ToString, wparam.ToString, result.ToString, whnd.ToString);
		}

//		public object GetLParam(Type cls){
//			//	throw new NotImplementedException ();
//			//return (object) lparam;
//		}
	}
}
