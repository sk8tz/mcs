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
//	Jackson Harper (jackson@ximian.com)
//
//


//
// TODO:
//  - dead chars are not translated properly
//  - There is a lot of potential for optimmization in here
// 
using System;
using System.Collections;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	internal class X11Keyboard {

		private IntPtr display;
		private IntPtr window;
		private IntPtr xic;
		private StringBuilder lookup_buffer;
		private int min_keycode, max_keycode, keysyms_per_keycode, syms;
		private int [] keyc2vkey = new int [256];
		private int [] keyc2scan = new int [256];
		private byte [] key_state_table = new byte [256];
		private int lcid;
		private bool num_state, cap_state;
		private bool initialized;

		private int NumLockMask;
		private int AltGrMask;

		public X11Keyboard (IntPtr display, IntPtr window)
		{
			this.display = display;
			this.window = window;
			lookup_buffer = new StringBuilder (24);
		}

		public void EnsureLayoutInitialized ()
		{
			if (initialized)
				return;

			KeyboardLayouts layouts = new KeyboardLayouts ();
			KeyboardLayout layout = DetectLayout (layouts);
			lcid = layout.Lcid;
			CreateConversionArray (layouts, layout);

			if (!XSupportsLocale ()) {
				Console.Error.WriteLine ("X does not support your locale");
			}

			if (!XSetLocaleModifiers (String.Empty)) {
				Console.Error.WriteLine ("Could not set X locale modifiers");
			}

			IntPtr xim = XOpenIM (display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			if (xim == IntPtr.Zero) {
				Console.Error.WriteLine ("Could not get XIM");
			}

			xic = CreateXic (window, xim);
			
			initialized = true;
		}

		public Keys ModifierKeys {
			get {
				Keys keys = Keys.None;
				if ((key_state_table [(int) VirtualKeys.VK_SHIFT] & 0x80) != 0)
					keys |= Keys.Shift;
				if ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) != 0)
					keys |= Keys.Control;
				if ((key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0)
					keys |= Keys.Alt;
				return keys;
			}
		}

		public void FocusIn (IntPtr focus_window)
		{
			if (xic != IntPtr.Zero)
				XSetICFocus (xic);
		}

		public void FocusOut (IntPtr focus_window)
		{
			if (xic != IntPtr.Zero)
				XUnsetICFocus (xic);
		}

		public bool ResetKeyState(IntPtr hwnd, ref MSG msg) {
			// FIXME - keep defining events/msg and return true until we've 'restored' all
			// pending keypresses
			if ((key_state_table [(int) VirtualKeys.VK_SHIFT] & 0x80) != 0) {
				key_state_table [(int) VirtualKeys.VK_SHIFT] &=  unchecked((byte)~0x80);
			}

			if ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) != 0) {
				key_state_table [(int) VirtualKeys.VK_CONTROL] &=  unchecked((byte)~0x80);
			}

			if ((key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0) {
				key_state_table [(int) VirtualKeys.VK_MENU] &=  unchecked((byte)~0x80);
			}
			return false;
		}

		public void KeyEvent (IntPtr hwnd, XEvent xevent, ref MSG msg)
		{
			EnsureLayoutInitialized ();

			XKeySym keysym;
			int ascii_chars;

			IntPtr status = IntPtr.Zero;
			ascii_chars = LookupString (ref xevent, 24, out keysym, out status);

			if (((int) keysym >= (int) MiscKeys.XK_ISO_Lock && 
				(int) keysym <= (int) MiscKeys.XK_ISO_Last_Group_Lock) ||
				(int) keysym == (int) MiscKeys.XK_Mode_switch) {
				UpdateKeyState (xevent);
				return;
			}

			if ((xevent.KeyEvent.keycode >> 8) == 0x10)
				xevent.KeyEvent.keycode = xevent.KeyEvent.keycode & 0xFF;

			int event_time = (int)xevent.KeyEvent.time;

			if (status == (IntPtr) 2) {
				// Copy chars into a globally accessible var, i don't think
				// this var is exposed anywhere though, so we can just ignore this
				return;
			}

			AltGrMask = xevent.KeyEvent.state & (0x6000 | (int) KeyMasks.ModMasks);
			int vkey = EventToVkey (xevent);
			if (vkey == 0 && ascii_chars != 0) {
				vkey = (int) VirtualKeys.VK_NONAME;
			}

			switch ((VirtualKeys) (vkey & 0xFF)) {
			case VirtualKeys.VK_NUMLOCK:
				GenerateMessage (VirtualKeys.VK_NUMLOCK, 0x45, xevent.type, event_time);
				break;
			case VirtualKeys.VK_CAPITAL:
				GenerateMessage (VirtualKeys.VK_CAPITAL, 0x3A, xevent.type, event_time);
				break;
			default:
				if (((key_state_table [(int) VirtualKeys.VK_NUMLOCK] & 0x01) == 0) != ((xevent.KeyEvent.state & NumLockMask) == 0)) {
					GenerateMessage (VirtualKeys.VK_NUMLOCK, 0x45, XEventName.KeyPress, event_time);
					GenerateMessage (VirtualKeys.VK_NUMLOCK, 0x45, XEventName.KeyRelease, event_time);
				}

				if (((key_state_table [(int) VirtualKeys.VK_CAPITAL] & 0x01) == 0) != ((xevent.KeyEvent.state & (int) KeyMasks.LockMask) == 0)) {
					GenerateMessage (VirtualKeys.VK_CAPITAL, 0x3A, XEventName.KeyPress, event_time);
					GenerateMessage (VirtualKeys.VK_CAPITAL, 0x3A, XEventName.KeyRelease, event_time);
				}

				num_state = false;
				cap_state = false;

				int bscan = (keyc2scan [xevent.KeyEvent.keycode] & 0xFF);
				KeybdEventFlags dw_flags = KeybdEventFlags.None;
				if (xevent.type == XEventName.KeyRelease)
					dw_flags |= KeybdEventFlags.KeyUp;
				if ((vkey & 0x100) != 0)
					dw_flags |= KeybdEventFlags.ExtendedKey;
				msg = SendKeyboardInput ((VirtualKeys) (vkey & 0xFF), bscan, dw_flags, event_time);
				msg.hwnd = hwnd;
				break;
			}
		}

		public bool TranslateMessage (ref MSG msg)
		{
			bool res = false;

			if (msg.message >= Msg.WM_KEYFIRST && msg.message <= Msg.WM_KEYLAST)
				res = true;

			if (msg.message != Msg.WM_KEYDOWN && msg.message != Msg.WM_SYSKEYDOWN)
				return res;

			EnsureLayoutInitialized ();

			string buffer;
			Msg message;
			int tu = ToUnicode ((int) msg.wParam, Control.HighOrder ((int) msg.lParam), out buffer);
			switch (tu) {
			case 1:
				message = (msg.message == Msg.WM_KEYDOWN ? Msg.WM_CHAR : Msg.WM_SYSCHAR);
				XplatUI.PostMessage (msg.hwnd, message, (IntPtr) buffer [0], msg.lParam);
				break;
			case -1:
				message = (msg.message == Msg.WM_KEYDOWN ? Msg.WM_DEADCHAR : Msg.WM_SYSDEADCHAR);
				XplatUI.PostMessage (msg.hwnd, message, (IntPtr) buffer [0], msg.lParam);
				return true;
			}
			
			return res;
		}

		private int ToUnicode (int vkey, int scan, out string buffer)
		{
			if ((scan & 0x8000) != 0) {
				buffer = String.Empty;
				return 0;
			}

			XEvent e = new XEvent ();
			e.AnyEvent.type = XEventName.KeyPress;
			e.KeyEvent.display = display;
			e.KeyEvent.keycode = 0;
			e.KeyEvent.state = 0;

			if ((key_state_table [(int) VirtualKeys.VK_SHIFT] & 0x80) != 0) {
				e.KeyEvent.state |= (int) KeyMasks.ShiftMask;
			}

			if ((key_state_table [(int) VirtualKeys.VK_CAPITAL] & 0x01) != 0) {
				e.KeyEvent.state |= (int) KeyMasks.LockMask;
			}

			if ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) != 0) {
				e.KeyEvent.state |= (int) KeyMasks.ControlMask;
			}

			if ((key_state_table [(int) VirtualKeys.VK_NUMLOCK] & 0x01) != 0) {
				e.KeyEvent.state |= NumLockMask;
			}

			e.KeyEvent.state |= AltGrMask;

			for (int keyc = min_keycode; (keyc <= max_keycode) && (e.KeyEvent.keycode == 0); keyc++) {
				// find keycode that could have generated this vkey
				if ((keyc2vkey [keyc] & 0xFF) == vkey) {
					// filter extended bit because it is not known
					e.KeyEvent.keycode = keyc;
					if ((EventToVkey (e) & 0xFF) != vkey) {
						// Wrong one (ex: because of num,lock state)
						e.KeyEvent.keycode = 0;
					}
				}
			}

			if ((vkey >= (int) VirtualKeys.VK_NUMPAD0) && (vkey <= (int) VirtualKeys.VK_NUMPAD9))
				e.KeyEvent.keycode = XKeysymToKeycode (display, vkey - (int) VirtualKeys.VK_NUMPAD0 + (int) KeypadKeys.XK_KP_0);

			if (vkey == (int) VirtualKeys.VK_DECIMAL)
				e.KeyEvent.keycode = XKeysymToKeycode (display, (int) KeypadKeys.XK_KP_Decimal);
			
			if (vkey == (int) VirtualKeys.VK_SEPARATOR)
				e.KeyEvent.keycode = XKeysymToKeycode(display, (int) KeypadKeys.XK_KP_Separator);

			if (e.KeyEvent.keycode == 0 && vkey != (int) VirtualKeys.VK_NONAME) {
				// And I couldn't find the keycode so i returned the vkey and was like whatever
				Console.Error.WriteLine ("unknown virtual key {0:X}", vkey);
				buffer = String.Empty;
				return vkey; 
			}

			XKeySym t;
			IntPtr status;
			int res = LookupString (ref e, 24, out t, out status);
			int keysym = (int) t;

			buffer = String.Empty;
			if (res == 0) {
				int dead_char = MapDeadKeySym (keysym);
				if (dead_char != 0) {
					byte [] bytes = new byte [1];
					bytes [0] = (byte) dead_char;
					Encoding encoding = Encoding.GetEncoding (new CultureInfo (lcid).TextInfo.ANSICodePage);
					buffer = new string (encoding.GetChars (bytes));
					res = -1;
				}
			} else {
				// Shift + arrow, shift + home, ....
				// X returns a char for it, but windows doesn't
				if (((e.KeyEvent.state & NumLockMask) == 0) && ((e.KeyEvent.state & (int) KeyMasks.ShiftMask) != 0) &&
						(keysym >= (int) KeypadKeys.XK_KP_0) && (keysym <= (int) KeypadKeys.XK_KP_9)) {
					buffer = String.Empty;
					res = 0;
				}

				// CTRL + number, X returns chars, windows does not
				if ((e.KeyEvent.state & (int) KeyMasks.ControlMask) != 0) {
					if (((keysym >= 33) && (keysym < 'A')) || ((keysym > 'Z') && (keysym < 'a'))) {
						buffer = String.Empty;
						res = 0;
					}
				}

				// X returns a char for delete key on extended keyboards, windows does not
				if (keysym == (int) TtyKeys.XK_Delete) {
					buffer = String.Empty;
					res = 0;
				}

				if (res != 0) {
					buffer = lookup_buffer.ToString ();
					res = buffer.Length;
				}
			}

			return res;
		}

		private MSG SendKeyboardInput (VirtualKeys vkey, int scan, KeybdEventFlags dw_flags, int time)
		{
			Msg message;

			if ((dw_flags & KeybdEventFlags.KeyUp) != 0) {
				bool sys_key = (key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0 &&
					      ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) == 0);
				key_state_table [(int) vkey] &= unchecked ((byte) ~0x80);
				message = (sys_key ? Msg.WM_SYSKEYUP : Msg.WM_KEYUP);
			} else {
				if ((key_state_table [(int) vkey] & 0x80) == 0) {
					key_state_table [(int) vkey] ^= 0x01;
				}
				key_state_table [(int) vkey] |= 0x80;
				bool sys_key = (key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0 &&
					      ((key_state_table [(int) VirtualKeys.VK_CONTROL] & 0x80) == 0);
				message = (sys_key ? Msg.WM_SYSKEYDOWN : Msg.WM_KEYDOWN);
			}

			MSG msg = new MSG ();
			msg.message = message;
			msg.wParam = (IntPtr) vkey;
			if ((key_state_table [(int) VirtualKeys.VK_MENU] & 0x80) != 0)
				msg.lParam = new IntPtr (0x20000000);
			else
				msg.lParam = IntPtr.Zero;

			return msg;
		}

		private void GenerateMessage (VirtualKeys vkey, int scan, XEventName type, int event_time)
		{
			bool state = (vkey == VirtualKeys.VK_NUMLOCK ? num_state : cap_state);
			KeybdEventFlags up, down;

			if (state) {
				// The INTERMEDIARY state means : just after a 'press' event, if a 'release' event comes,
				// don't treat it. It's from the same key press. Then the state goes to ON.
				// And from there, a 'release' event will switch off the toggle key.
				SetState (vkey, false);
			} else {
				down = (vkey == VirtualKeys.VK_NUMLOCK ? KeybdEventFlags.ExtendedKey : KeybdEventFlags.None);
				up = (vkey == VirtualKeys.VK_NUMLOCK ? KeybdEventFlags.ExtendedKey :
						KeybdEventFlags.None) | KeybdEventFlags.KeyUp;
				if ((key_state_table [(int) vkey] & 0x1) != 0) { // it was on
					if (type != XEventName.KeyPress) {
						SendKeyboardInput (vkey, scan, down, event_time);
						SendKeyboardInput (vkey, scan, up, event_time);
						SetState (vkey, false);
						key_state_table [(int) vkey] &= unchecked ((byte) ~0x01);
					}
				} else {
					if (type == XEventName.KeyPress) {
						SendKeyboardInput (vkey, scan, down, event_time);
						SendKeyboardInput (vkey, scan, up, event_time);
						SetState (vkey, true);
						key_state_table [(int) vkey] |= 0x01;
					}
				}
			}
		}

		private void UpdateKeyState (XEvent xevent)
		{
			int vkey = EventToVkey (xevent);

			switch (xevent.type) {
			case XEventName.KeyRelease:
				key_state_table [vkey & 0xff] &= unchecked ((byte) ~0x80);
				break;
			case XEventName.KeyPress:
				if ((key_state_table [vkey & 0xff] & 0x80) == 0) {
					key_state_table [vkey & 0xff] ^= 0x01;
				}
				key_state_table [vkey & 0xff] |= 0x80;
				break;
			}
		}

		private void SetState (VirtualKeys key, bool state)
		{
			if (VirtualKeys.VK_NUMLOCK == key)
				num_state = state;
			else
				cap_state = state;
		}

		public int EventToVkey (XEvent e)
		{
			EnsureLayoutInitialized ();

			IntPtr status;
			XKeySym ks;

			LookupString (ref e, 0, out ks, out status);
			int keysym = (int) ks;

			if (((e.KeyEvent.state & NumLockMask) != 0) &&
			    (keysym == (int)KeypadKeys.XK_KP_Separator || keysym == (int)KeypadKeys.XK_KP_Decimal ||
			    (keysym >= (int)KeypadKeys.XK_KP_0 && keysym <= (int)KeypadKeys.XK_KP_9))) {
				// Only the Keypad keys 0-9 and . send different keysyms
				// depending on the NumLock state
				return nonchar_key_vkey [keysym & 0xFF];
			}

			return keyc2vkey [e.KeyEvent.keycode];
		}

		private void CreateConversionArray (KeyboardLayouts layouts, KeyboardLayout layout)
		{
			XEvent e2 = new XEvent ();
			uint keysym = 0;
			int [] ckey = new int [] { 0, 0, 0, 0 };

			e2.KeyEvent.display = display;
			e2.KeyEvent.state = 0;

			for (int keyc = min_keycode; keyc <= max_keycode; keyc++) {
				int vkey = 0;
				int scan = 0;

				e2.KeyEvent.keycode = keyc;
				XKeySym t;

				IntPtr status;
				LookupString (ref e2, 0, out t, out status);

				keysym = (uint) t;
				if (keysym != 0) {
					if ((keysym >> 8) == 0xFF) {
						vkey = nonchar_key_vkey [keysym & 0xFF];
						scan = nonchar_key_scan [keysym & 0xFF];
						// Set extended bit
						if ((scan & 0x100) != 0)
							vkey |= 0x100;
					} else if (keysym == 0x20) { // spacebar
						vkey = (int) VirtualKeys.VK_SPACE;
						scan = 0x39;
					} else {
						// Search layout dependent scancodes
						int maxlen = 0;
						int maxval = -1;;
						
						for (int i = 0; i < syms; i++) {
							keysym = XKeycodeToKeysym (display, keyc, i);
							if ((keysym < 0x800) && (keysym != ' '))
								ckey [i] = (sbyte) (keysym & 0xFF);
							else
								ckey [i] = (sbyte) MapDeadKeySym ((int) keysym);
						}
						
						for (int keyn = 0; keyn < layout.Keys.Length; keyn++) {
							int ml = Math.Min (layout.Keys [keyn].Length, 4);
							int ok = -1;
							for (int i = 0; (ok != 0) && (i < ml); i++) {
								sbyte ck = (sbyte) layout.Keys [keyn][i];
								if (ck != ckey [i])
									ok = 0;
								if ((ok != 0) || (i > maxlen)) {
									maxlen = i;
									maxval = keyn;
								}
								if (ok != 0)
									break;
							}
						}
						if (maxval >= 0) {
							scan = layouts.scan_table [(int) layout.ScanIndex][maxval];
							vkey = layouts.vkey_table [(int) layout.VKeyIndex][maxval];
						}
						
					}
				}
				keyc2vkey [e2.KeyEvent.keycode] = vkey;
				keyc2scan [e2.KeyEvent.keycode] = scan;
			}
			
			
		}

		private KeyboardLayout DetectLayout (KeyboardLayouts layouts)
		{
			XDisplayKeycodes (display, out min_keycode, out max_keycode);
			IntPtr ksp = XGetKeyboardMapping (display, (byte) min_keycode,
					max_keycode + 1 - min_keycode, out keysyms_per_keycode);
			XplatUIX11.XFree (ksp);

			syms = keysyms_per_keycode;
			if (syms > 4) {
				//Console.Error.WriteLine ("{0} keysymbols per a keycode is not supported, setting to 4", syms);
				syms = 2;
			}

			IntPtr	modmap_unmanaged;
			XModifierKeymap xmk = new XModifierKeymap ();

			modmap_unmanaged = XGetModifierMapping (display);
			xmk = (XModifierKeymap) Marshal.PtrToStructure (modmap_unmanaged, typeof (XModifierKeymap));

			int mmp = 0;
			for (int i = 0; i < 8; i++) {
				for (int j = 0; j < xmk.max_keypermod; j++, mmp++) {
					byte b = Marshal.ReadByte (xmk.modifiermap, mmp);
					if (b != 0) {
						for (int k = 0; k < keysyms_per_keycode; k++) {
							if ((int) XKeycodeToKeysym (display, b, k) == (int) MiscKeys.XK_Num_Lock)
								NumLockMask = 1 << i;
						}
					}
				}
			}
			XFreeModifiermap (modmap_unmanaged);

			int [] ckey = new int [4];
			KeyboardLayout layout = null;
			int max_score = 0;
			int max_seq = 0;
			
			foreach (KeyboardLayout current in layouts.Layouts) {
				int ok = 0;
				int score = 0;
				int match = 0;
				int mismatch = 0;
				int seq = 0;
				int pkey = -1;
				int key = min_keycode;
				int i;

				for (int keyc = min_keycode; keyc <= max_keycode; keyc++) {
					for (i = 0; i < syms; i++) {
						uint keysym = XKeycodeToKeysym (display, keyc, i);
						
						if ((keysym < 0x800) && (keysym != ' ')) {
							ckey [i] = (sbyte) (keysym & 0xFF);
						} else {
							ckey [i] = (sbyte) MapDeadKeySym ((int) keysym);
						}
					}
					if (ckey [0] != 0) {
						for (key = 0; key < current.Keys.Length; key++) {
							int ml = Math.Min (syms, current.Keys [key].Length);
							for (ok = 0, i = 0; (ok >= 0) && (i < ml); i++) {
								sbyte ck = (sbyte) current.Keys [key][i];
								if (ck != 0 && ck == ckey[i])
									ok++;
								if (ck != 0 && ck != ckey[i])
									ok = -1;
							}
							if (ok > 0) {
								score += ok;
								break;
							}
						}
						if (ok > 0) {
							match++;
							/* and how much the keycode order matches */
							if (key > pkey)
								seq++;
							pkey = key;
						} else {
							/* print spaces instead of \0's */
							mismatch++;
							score -= syms;
						}
					}
				}

				if ((score > max_score) || ((score == max_score) && (seq > max_seq))) {
					// best match so far
					layout = current;
					max_score = score;
					max_seq = seq;
				}
			}

			if (layout != null)  {
                                return layout;
			} else {
				Console.WriteLine (Locale.GetText("Keyboard layout not recognized, using default layout: " +
								   layouts.Layouts [0].Name));
			}

			return layouts.Layouts [0];
		}

		// TODO
		private int MapDeadKeySym (int val)
		{
			switch (val) {
			case (int) DeadKeys.XK_dead_tilde :
			case 0x1000FE7E : // Xfree's Dtilde
				return '~';
			case (int) DeadKeys.XK_dead_acute :
			case 0x1000FE27 : // Xfree's XK_Dacute_accent
				return 0xb4;
			case (int) DeadKeys.XK_dead_circumflex:
			case 0x1000FE5E : // Xfree's XK_.Dcircumflex_accent
				return '^';
			case (int) DeadKeys.XK_dead_grave :
			case 0x1000FE60 : // Xfree's XK_.Dgrave_accent
				return '`';
			case (int) DeadKeys.XK_dead_diaeresis :
			case 0x1000FE22 : // Xfree's XK_.Ddiaeresis
				return 0xa8;
			case (int) DeadKeys.XK_dead_cedilla :
				return 0xb8;
			case (int) DeadKeys.XK_dead_macron :
				return '-';
			case (int) DeadKeys.XK_dead_breve :
				return 0xa2;
			case (int) DeadKeys.XK_dead_abovedot :
				return 0xff;
			case (int) DeadKeys.XK_dead_abovering :
				return '0';
			case (int) DeadKeys.XK_dead_doubleacute :
				return 0xbd;
			case (int) DeadKeys.XK_dead_caron :
				return 0xb7;
			case (int) DeadKeys.XK_dead_ogonek :
				return 0xb2;
			}

			return 0;
		}

		private IntPtr CreateXic (IntPtr window, IntPtr xim)
		{
			xic = XCreateIC (xim, 
				"inputStyle", XIMProperties.XIMPreeditNothing | XIMProperties.XIMStatusNothing,
				"clientWindow", window,
				"focusWindow", window,
				IntPtr.Zero);
			return xic;
		}

		private int LookupString (ref XEvent xevent, int len, out XKeySym keysym, out IntPtr status)
		{
			IntPtr keysym_res;
			int res;

			status = IntPtr.Zero;
			lookup_buffer.Length = 0;
			if (xic != IntPtr.Zero)
				res = XmbLookupString (xic, ref xevent, lookup_buffer, len, out keysym_res,  out status);
			else
				res = XLookupString (ref xevent, lookup_buffer, len, out keysym_res, IntPtr.Zero);

			keysym = (XKeySym) keysym_res.ToInt32 ();
			return res;
		}

		[DllImport ("libX11")]
		private static extern IntPtr XOpenIM (IntPtr display, IntPtr rdb, IntPtr res_name, IntPtr res_class);

		[DllImport ("libX11")]
		private static extern IntPtr XCreateIC (IntPtr xim, string name, XIMProperties im_style, string name2, IntPtr value2, string name3, IntPtr value3, IntPtr terminator);

		[DllImport ("libX11")]
		private static extern void XSetICFocus (IntPtr xic);

		[DllImport ("libX11")]
		private static extern void XUnsetICFocus (IntPtr xic);

		[DllImport ("libX11")]
		private static extern bool XSupportsLocale ();

		[DllImport ("libX11")]
		private static extern bool XSetLocaleModifiers (string mods);

		[DllImport ("libX11")]
		internal extern static int XLookupString(ref XEvent xevent, StringBuilder buffer, int num_bytes, out IntPtr keysym, IntPtr status);
		[DllImport ("libX11")]
		internal extern static int XmbLookupString(IntPtr xic, ref XEvent xevent, StringBuilder buffer, int num_bytes, out IntPtr keysym, out IntPtr status);

		internal static int XmbLookupString (IntPtr xic, ref XEvent xevent, StringBuilder buffer, int num_bytes, out XKeySym keysym, out IntPtr status) {
			IntPtr	keysym_ret;
			int	ret;

			ret = XmbLookupString (xic, ref xevent, buffer, num_bytes, out keysym_ret, out status);

			keysym = (XKeySym)keysym_ret.ToInt32();

			return ret;
		}

		internal static int XLookupString (ref XEvent xevent, StringBuilder buffer, int num_bytes, out XKeySym keysym, IntPtr status) {
			IntPtr	keysym_ret;
			int	ret;

			ret = XLookupString (ref xevent, buffer, num_bytes, out keysym_ret, status);
			keysym = (XKeySym)keysym_ret.ToInt32();

			return ret;
		}

		[DllImport ("libX11", EntryPoint="XLookupKeysym")]
		private static extern IntPtr XLookupKeysymX11(ref XEvent xevent, int index);
		private static XKeySym XLookupKeysym(ref XEvent xevent, int index) {
			return (XKeySym)XLookupKeysymX11(ref xevent, index).ToInt32();
		}

		[DllImport ("libX11")]
		private static extern IntPtr XGetKeyboardMapping (IntPtr display, byte first_keycode, int keycode_count, 
				out int keysyms_per_keycode_return);

		[DllImport ("libX11")]
		private static extern void XDisplayKeycodes (IntPtr display, out int min, out int max);

		[DllImport ("libX11", EntryPoint="XKeycodeToKeysym")]
		private static extern uint XKeycodeToKeysym (IntPtr display, int keycode, int index);

		[DllImport ("libX11")]
		private static extern int XKeysymToKeycode (IntPtr display, IntPtr keysym);
		private static int XKeysymToKeycode (IntPtr display, int keysym) {
			return XKeysymToKeycode(display, (IntPtr)keysym);
		}

		[DllImport ("libX11")]
		internal extern static IntPtr XGetModifierMapping (IntPtr display);

		[DllImport ("libX11")]
		internal extern static int XFreeModifiermap (IntPtr modmap);


		private readonly static int [] nonchar_key_vkey = new int []
		{
			/* unused */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF00 */
			/* special keys */
			(int) VirtualKeys.VK_BACK, (int) VirtualKeys.VK_TAB, 0, (int) VirtualKeys.VK_CLEAR, 0, (int) VirtualKeys.VK_RETURN, 0, 0,	    /* FF08 */
			0, 0, 0, (int) VirtualKeys.VK_PAUSE, (int) VirtualKeys.VK_SCROLL, 0, 0, 0,			     /* FF10 */
			0, 0, 0, (int) VirtualKeys.VK_ESCAPE, 0, 0, 0, 0,			      /* FF18 */
			/* unused */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF20 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF28 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF30 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF38 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF40 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF48 */
			/* cursor keys */
			(int) VirtualKeys.VK_HOME, (int) VirtualKeys.VK_LEFT, (int) VirtualKeys.VK_UP, (int) VirtualKeys.VK_RIGHT,			    /* FF50 */
			(int) VirtualKeys.VK_DOWN, (int) VirtualKeys.VK_PRIOR, (int) VirtualKeys.VK_NEXT, (int) VirtualKeys.VK_END,
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF58 */
			/* misc keys */
			(int) VirtualKeys.VK_SELECT, (int) VirtualKeys.VK_SNAPSHOT, (int) VirtualKeys.VK_EXECUTE, (int) VirtualKeys.VK_INSERT, 0, 0, 0, 0,  /* FF60 */
			(int) VirtualKeys.VK_CANCEL, (int) VirtualKeys.VK_HELP, (int) VirtualKeys.VK_CANCEL, (int) VirtualKeys.VK_CANCEL, 0, 0, 0, 0,	    /* FF68 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF70 */
			/* keypad keys */
			0, 0, 0, 0, 0, 0, 0, (int) VirtualKeys.VK_NUMLOCK,			      /* FF78 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF80 */
			0, 0, 0, 0, 0, (int) VirtualKeys.VK_RETURN, 0, 0,			      /* FF88 */
			0, 0, 0, 0, 0, (int) VirtualKeys.VK_HOME, (int) VirtualKeys.VK_LEFT, (int) VirtualKeys.VK_UP,			  /* FF90 */
			(int) VirtualKeys.VK_RIGHT, (int) VirtualKeys.VK_DOWN, (int) VirtualKeys.VK_PRIOR, (int) VirtualKeys.VK_NEXT,			    /* FF98 */
			(int) VirtualKeys.VK_END, 0, (int) VirtualKeys.VK_INSERT, (int) VirtualKeys.VK_DELETE,
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFA0 */
			0, 0, (int) VirtualKeys.VK_MULTIPLY, (int) VirtualKeys.VK_ADD,					/* FFA8 */
			(int) VirtualKeys.VK_SEPARATOR, (int) VirtualKeys.VK_SUBTRACT, (int) VirtualKeys.VK_DECIMAL, (int) VirtualKeys.VK_DIVIDE,
			(int) VirtualKeys.VK_NUMPAD0, (int) VirtualKeys.VK_NUMPAD1, (int) VirtualKeys.VK_NUMPAD2, (int) VirtualKeys.VK_NUMPAD3,		    /* FFB0 */
			(int) VirtualKeys.VK_NUMPAD4, (int) VirtualKeys.VK_NUMPAD5, (int) VirtualKeys.VK_NUMPAD6, (int) VirtualKeys.VK_NUMPAD7,
			(int) VirtualKeys.VK_NUMPAD8, (int) VirtualKeys.VK_NUMPAD9, 0, 0, 0, 0,				/* FFB8 */
			/* function keys */
			(int) VirtualKeys.VK_F1, (int) VirtualKeys.VK_F2,
			(int) VirtualKeys.VK_F3, (int) VirtualKeys.VK_F4, (int) VirtualKeys.VK_F5, (int) VirtualKeys.VK_F6, (int) VirtualKeys.VK_F7, (int) VirtualKeys.VK_F8, (int) VirtualKeys.VK_F9, (int) VirtualKeys.VK_F10,    /* FFC0 */
			(int) VirtualKeys.VK_F11, (int) VirtualKeys.VK_F12, (int) VirtualKeys.VK_F13, (int) VirtualKeys.VK_F14, (int) VirtualKeys.VK_F15, (int) VirtualKeys.VK_F16, 0, 0,	/* FFC8 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFD0 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFD8 */
			/* modifier keys */
			0, (int) VirtualKeys.VK_SHIFT, (int) VirtualKeys.VK_SHIFT, (int) VirtualKeys.VK_CONTROL,			  /* FFE0 */
			(int) VirtualKeys.VK_CONTROL, (int) VirtualKeys.VK_CAPITAL, 0, (int) VirtualKeys.VK_MENU,
			(int) VirtualKeys.VK_MENU, (int) VirtualKeys.VK_MENU, (int) VirtualKeys.VK_MENU, 0, 0, 0, 0, 0,			  /* FFE8 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFF0 */
			0, 0, 0, 0, 0, 0, 0, (int) VirtualKeys.VK_DELETE			      /* FFF8 */
		};

		private static readonly int [] nonchar_key_scan = new int []
		{
			/* unused */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF00 */
			/* special keys */
			0x0E, 0x0F, 0x00, /*?*/ 0, 0x00, 0x1C, 0x00, 0x00,	     /* FF08 */
			0x00, 0x00, 0x00, 0x45, 0x46, 0x00, 0x00, 0x00,		     /* FF10 */
			0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,		     /* FF18 */
			/* unused */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF20 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF28 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF30 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF38 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF40 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF48 */
			/* cursor keys */
			0x147, 0x14B, 0x148, 0x14D, 0x150, 0x149, 0x151, 0x14F,	     /* FF50 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF58 */
			/* misc keys */
			/*?*/ 0, 0x137, /*?*/ 0, 0x152, 0x00, 0x00, 0x00, 0x00,	     /* FF60 */
			/*?*/ 0, /*?*/ 0, 0x38, 0x146, 0x00, 0x00, 0x00, 0x00,	     /* FF68 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF70 */
			/* keypad keys */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x138, 0x145,	     /* FF78 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF80 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x11C, 0x00, 0x00,	     /* FF88 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x47, 0x4B, 0x48,		     /* FF90 */
			0x4D, 0x50, 0x49, 0x51, 0x4F, 0x4C, 0x52, 0x53,		     /* FF98 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFA0 */
			0x00, 0x00, 0x37, 0x4E, /*?*/ 0, 0x4A, 0x53, 0x135,	     /* FFA8 */
			0x52, 0x4F, 0x50, 0x51, 0x4B, 0x4C, 0x4D, 0x47,		     /* FFB0 */
			0x48, 0x49, 0x00, 0x00, 0x00, 0x00,			     /* FFB8 */
			/* function keys */
			0x3B, 0x3C,
			0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44,		     /* FFC0 */
			0x57, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFC8 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFD0 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFD8 */
			/* modifier keys */
			0x00, 0x2A, 0x36, 0x1D, 0x11D, 0x3A, 0x00, 0x38,	     /* FFE0 */
			0x138, 0x38, 0x138, 0x00, 0x00, 0x00, 0x00, 0x00,	     /* FFE8 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFF0 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x153		     /* FFF8 */
		};
	}

}

