/*
 * Encoding.cs - Implementation of the "System.Text.Encoding" class.
 *
 * Copyright (c) 2001, 2002  Southern Storm Software, Pty Ltd
 * Copyright (c) 2002, Ximian, Inc.
 * Copyright (c) 2003, 2004 Novell, Inc.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

namespace System.Text
{

using System;
using System.Reflection;
using System.Globalization;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[Serializable]
public abstract class Encoding
#if NET_2_0
	: ICloneable
#endif
{
	// Code page used by this encoding.
	internal int codePage;
	internal int windows_code_page;
	bool is_readonly = true;

	// Constructor.
	protected Encoding ()
	{
	}

#if ECMA_COMPAT
	protected internal
#else
	protected
#endif
	Encoding (int codePage)
	{
		this.codePage = windows_code_page = codePage;

#if NET_2_0
		switch (codePage) {
		default:
			// MS has "InternalBestFit{Decoder|Encoder}Fallback
			// here, but we dunno what they are for.
			decoder_fallback = DecoderFallback.ReplacementFallback;
			encoder_fallback = EncoderFallback.ReplacementFallback;
			break;
		case 20127: // ASCII
		case 54936: // GB18030
			decoder_fallback = DecoderFallback.ReplacementFallback;
			encoder_fallback = EncoderFallback.ReplacementFallback;
			break;
		case 1200: // UTF16
		case 1201: // UTF16
		case 12000: // UTF32
		case 12001: // UTF32
		case 65000: // UTF7
		case 65001: // UTF8
			decoder_fallback = new DecoderReplacementFallback (String.Empty);
			encoder_fallback = new EncoderReplacementFallback (String.Empty);
			break;
		}
#endif
	}

	// until we change the callers:
	internal static string _ (string arg) {
		return arg;
	}

#if NET_2_0
	DecoderFallback decoder_fallback;
	EncoderFallback encoder_fallback;

	[ComVisible (false)]
	public bool IsReadOnly {
		get { return is_readonly; }
	}

	[MonoTODO ("not used yet")]
	[ComVisible (false)]
	public DecoderFallback DecoderFallback {
		get {
			if (decoder_fallback == null)
				decoder_fallback = new DecoderReplacementFallback (String.Empty);
			return decoder_fallback;
		}
		set {
			if (IsReadOnly)
				throw new InvalidOperationException ("This Encoding is readonly.");
			if (value == null)
				throw new ArgumentNullException ();
			decoder_fallback = value;
		}
	}

	[MonoTODO ("not used yet")]
	[ComVisible (false)]
	public EncoderFallback EncoderFallback {
		get {
			if (encoder_fallback == null)
				encoder_fallback = new EncoderReplacementFallback (String.Empty);
			return encoder_fallback;
		}
		set {
			if (IsReadOnly)
				throw new InvalidOperationException ("This Encoding is readonly.");
			if (value == null)
				throw new ArgumentNullException ();
			encoder_fallback = value;
		}
	}

	internal void SetFallbackInternal (EncoderFallback e, DecoderFallback d)
	{
		if (e != null)
			encoder_fallback = e;
		if (d != null)
			decoder_fallback = d;
	}
#endif

	// Convert between two encodings.
	public static byte[] Convert (Encoding srcEncoding, Encoding dstEncoding,
								 byte[] bytes)
	{
		if (srcEncoding == null) {
			throw new ArgumentNullException ("srcEncoding");
		}
		if (dstEncoding == null) {
			throw new ArgumentNullException ("dstEncoding");
		}
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		return dstEncoding.GetBytes (srcEncoding.GetChars (bytes, 0, bytes.Length));
	}
	public static byte[] Convert (Encoding srcEncoding, Encoding dstEncoding,
								 byte[] bytes, int index, int count)
	{
		if (srcEncoding == null) {
			throw new ArgumentNullException ("srcEncoding");
		}
		if (dstEncoding == null) {
			throw new ArgumentNullException ("dstEncoding");
		}
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		if (index < 0 || index > bytes.Length) {
			throw new ArgumentOutOfRangeException
				("index", _("ArgRange_Array"));
		}
		if (count < 0 || (bytes.Length - index) < count) {
			throw new ArgumentOutOfRangeException
				("count", _("ArgRange_Array"));
		}
		return dstEncoding.GetBytes (srcEncoding.GetChars (bytes, index, count));
	}

	// Determine if two Encoding objects are equal.
	public override bool Equals (Object obj)
	{
		Encoding enc = (obj as Encoding);
		if (enc != null) {
			return (codePage == enc.codePage);
		} else {
			return false;
		}
	}

	// Get the number of characters needed to encode a character buffer.
	public abstract int GetByteCount (char[] chars, int index, int count);

	// Convenience wrappers for "GetByteCount".
	public virtual int GetByteCount (String s)
	{
		if (s != null) {
			char[] chars = s.ToCharArray ();
			return GetByteCount (chars, 0, chars.Length);
		} else {
			throw new ArgumentNullException ("s");
		}
	}
	public virtual int GetByteCount (char[] chars)
	{
		if (chars != null) {
			return GetByteCount (chars, 0, chars.Length);
		} else {
			throw new ArgumentNullException ("chars");
		}
	}

	// Get the bytes that result from encoding a character buffer.
	public abstract int GetBytes (char[] chars, int charIndex, int charCount,
								 byte[] bytes, int byteIndex);

	// Convenience wrappers for "GetBytes".
	public virtual int GetBytes (String s, int charIndex, int charCount,
								byte[] bytes, int byteIndex)
	{
		if (s == null) {
			throw new ArgumentNullException ("s");
		}
		return GetBytes (s.ToCharArray(), charIndex, charCount, bytes, byteIndex);
	}
	public virtual byte[] GetBytes (String s)
	{
		if (s == null) {
			throw new ArgumentNullException ("s");
		}
		char[] chars = s.ToCharArray ();
		int numBytes = GetByteCount (chars, 0, chars.Length);
		byte[] bytes = new byte [numBytes];
		GetBytes (chars, 0, chars.Length, bytes, 0);
		return bytes;
	}
	public virtual byte[] GetBytes (char[] chars, int index, int count)
	{
		int numBytes = GetByteCount (chars, index, count);
		byte[] bytes = new byte [numBytes];
		GetBytes (chars, index, count, bytes, 0);
		return bytes;
	}
	public virtual byte[] GetBytes (char[] chars)
	{
		int numBytes = GetByteCount (chars, 0, chars.Length);
		byte[] bytes = new byte [numBytes];
		GetBytes (chars, 0, chars.Length, bytes, 0);
		return bytes;
	}

	// Get the number of characters needed to decode a byte buffer.
	public abstract int GetCharCount (byte[] bytes, int index, int count);

	// Convenience wrappers for "GetCharCount".
	public virtual int GetCharCount (byte[] bytes)
	{
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		return GetCharCount (bytes, 0, bytes.Length);
	}

	// Get the characters that result from decoding a byte buffer.
	public abstract int GetChars (byte[] bytes, int byteIndex, int byteCount,
								 char[] chars, int charIndex);

	// Convenience wrappers for "GetChars".
	public virtual char[] GetChars (byte[] bytes, int index, int count)
	{
		int numChars = GetCharCount (bytes, index, count);
		char[] chars = new char [numChars];
		GetChars (bytes, index, count, chars, 0);
		return chars;
	}
	public virtual char[] GetChars (byte[] bytes)
	{
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		int numChars = GetCharCount (bytes, 0, bytes.Length);
		char[] chars = new char [numChars];
		GetChars (bytes, 0, bytes.Length, chars, 0);
		return chars;
	}

	// Get a decoder that forwards requests to this object.
	public virtual Decoder GetDecoder ()
	{
		return new ForwardingDecoder (this);
	}

	// Get an encoder that forwards requests to this object.
	public virtual Encoder GetEncoder ()
	{
		return new ForwardingEncoder (this);
	}

	// Loaded copy of the "I18N" assembly.  We need to move
	// this into a class in "System.Private" eventually.
	private static Assembly i18nAssembly;
	private static bool i18nDisabled;

	// Invoke a specific method on the "I18N" manager object.
	// Returns NULL if the method failed.
	private static Object InvokeI18N (String name, params Object[] args)
	{
		lock (lockobj) {
			// Bail out if we previously detected that there
			// is insufficent engine support for I18N handling.
			if (i18nDisabled) {
				return null;
			}

			// Find or load the "I18N" assembly.
			if (i18nAssembly == null) {
				try {
					try {
						i18nAssembly = Assembly.Load (Consts.AssemblyI18N);
					} catch (NotImplementedException) {
						// Assembly loading unsupported by the engine.
						i18nDisabled = true;
						return null;
					}
					if (i18nAssembly == null) {
						return null;
					}
				} catch (SystemException) {
					return null;
				}
			}

			// Find the "I18N.Common.Manager" class.
			Type managerClass;
			try {
				managerClass = i18nAssembly.GetType ("I18N.Common.Manager");
			} catch (NotImplementedException) {
				// "GetType" is not supported by the engine.
				i18nDisabled = true;
				return null;
			}
			if (managerClass == null) {
				return null;
			}

			// Get the value of the "PrimaryManager" property.
			Object manager;
			try {
				manager = managerClass.InvokeMember
						("PrimaryManager",
						 BindingFlags.GetProperty |
						 	BindingFlags.Static |
							BindingFlags.Public,
						 null, null, null, null, null, null);
				if (manager == null) {
					return null;
				}
			} catch (MissingMethodException) {
				return null;
			} catch (SecurityException) {
				return null;
			} catch (NotImplementedException) {
				// "InvokeMember" is not supported by the engine.
				i18nDisabled = true;
				return null;
			}

			// Invoke the requested method on the manager.
			try {
				return managerClass.InvokeMember
						(name,
						 BindingFlags.InvokeMethod |
						 	BindingFlags.Instance |
							BindingFlags.Public,
						 null, manager, args, null, null, null);
			} catch (MissingMethodException) {
				return null;
			} catch (SecurityException) {
				return null;
			}
		}
	}

	// Get an encoder for a specific code page.
#if ECMA_COMPAT
	private
#else
	public
#endif
	static Encoding GetEncoding (int codePage)
	{
		// Check for the builtin code pages first.
		switch (codePage) {
			case 0: return Default;

			case ASCIIEncoding.ASCII_CODE_PAGE:
				return ASCII;

			case UTF7Encoding.UTF7_CODE_PAGE:
				return UTF7;

			case UTF8Encoding.UTF8_CODE_PAGE:
				return UTF8;

			case UnicodeEncoding.UNICODE_CODE_PAGE:
				return Unicode;

			case UnicodeEncoding.BIG_UNICODE_CODE_PAGE:
				return BigEndianUnicode;

			case Latin1Encoding.ISOLATIN_CODE_PAGE:
				return ISOLatin1;

			default: break;
		}

		// Try to obtain a code page handler from the I18N handler.
		Encoding enc = (Encoding)(InvokeI18N ("GetEncoding", codePage));
		if (enc != null) {
			enc.is_readonly = true;
			return enc;
		}

		// Build a code page class name.
		String cpName = "System.Text.CP" + codePage.ToString ();

		// Look for a code page converter in this assembly.
		Assembly assembly = Assembly.GetExecutingAssembly ();
		Type type = assembly.GetType (cpName);
		if (type != null) {
			enc = (Encoding)(Activator.CreateInstance (type));
			enc.is_readonly = true;
			return enc;
		}

		// Look in any assembly, in case the application
		// has provided its own code page handler.
		type = Type.GetType (cpName);
		if (type != null) {
			enc = (Encoding)(Activator.CreateInstance (type));
			enc.is_readonly = true;
			return enc;
		}

		// We have no idea how to handle this code page.
		throw new NotSupportedException
			(String.Format ("CodePage {0} not supported", codePage.ToString ()));
	}

#if !ECMA_COMPAT

#if NET_2_0

	public virtual object Clone ()
	{
		return MemberwiseClone ();
	}

	public static Encoding GetEncoding (int codePage,
		EncoderFallback encoderFallback, DecoderFallback decoderFallback)
	{
		if (encoderFallback == null)
			throw new ArgumentNullException ("encoderFallback");
		if (decoderFallback == null)
			throw new ArgumentNullException ("decoderFallback");

		Encoding e = GetEncoding (codePage).Clone () as Encoding;
		e.is_readonly = false;
		e.encoder_fallback = encoderFallback;
		e.decoder_fallback = decoderFallback;
		return e;
	}

	public static Encoding GetEncoding (string name,
		EncoderFallback encoderFallback, DecoderFallback decoderFallback)
	{
		if (encoderFallback == null)
			throw new ArgumentNullException ("encoderFallback");
		if (decoderFallback == null)
			throw new ArgumentNullException ("decoderFallback");

		Encoding e = GetEncoding (name).Clone () as Encoding;
		e.is_readonly = false;
		e.encoder_fallback = encoderFallback;
		e.decoder_fallback = decoderFallback;
		return e;
	}
#endif

	// Table of builtin web encoding names and the corresponding code pages.
	private static readonly object[] encodings =
		{
			ASCIIEncoding.ASCII_CODE_PAGE,
			"ascii", "us_ascii", "us", "ansi_x3.4_1968",
			"ansi_x3.4_1986", "cp367", "csascii", "ibm367",
			"iso_ir_6", "iso646_us", "iso_646.irv:1991",

			UTF7Encoding.UTF7_CODE_PAGE,
			"utf_7", "csunicode11utf7", "unicode_1_1_utf_7",
			"unicode_2_0_utf_7", "x_unicode_1_1_utf_7",
			"x_unicode_2_0_utf_7",
			
			UTF8Encoding.UTF8_CODE_PAGE,
			"utf_8", "unicode_1_1_utf_8", "unicode_2_0_utf_8",
			"x_unicode_1_1_utf_8", "x_unicode_2_0_utf_8",

			UnicodeEncoding.UNICODE_CODE_PAGE,
			"utf_16", "UTF_16LE", "ucs_2", "unicode",
			"iso_10646_ucs2",

			UnicodeEncoding.BIG_UNICODE_CODE_PAGE,
			"unicodefffe", "utf_16be",

			Latin1Encoding.ISOLATIN_CODE_PAGE,
			"iso_8859_1", "latin1"
		};

	// Get an encoding object for a specific web encoding name.
	public static Encoding GetEncoding (String name)
	{
		// Validate the parameters.
		if (name == null) {
			throw new ArgumentNullException ("name");
		}

		string converted = name.ToLowerInvariant ().Replace ('-', '_');
		
		// Search the table for a name match.
		int code = 0;
		for (int i = 0; i < encodings.Length; ++i) {
			object o = encodings [i];
			
			if (o is int){
				code = (int) o;
				continue;
			}
			
			if (converted == ((string)encodings [i]))
				return GetEncoding (code);
		}

		// Try to obtain a web encoding handler from the I18N handler.
		Encoding enc = (Encoding)(InvokeI18N ("GetEncoding", name));
		if (enc != null) {
			return enc;
		}

		// Build a web encoding class name.
		String encName = "System.Text.ENC" + converted;
						 

		// Look for a code page converter in this assembly.
		Assembly assembly = Assembly.GetExecutingAssembly ();
		Type type = assembly.GetType (encName);
		if (type != null) {
			return (Encoding)(Activator.CreateInstance (type));
		}

		// Look in any assembly, in case the application
		// has provided its own code page handler.
		type = Type.GetType (encName);
		if (type != null) {
			return (Encoding)(Activator.CreateInstance (type));
		}

		// We have no idea how to handle this encoding name.
		throw new NotSupportedException (String.Format ("Encoding name `{0}' not supported", name));
	}

#endif // !ECMA_COMPAT

	// Get a hash code for this instance.
	public override int GetHashCode ()
	{
		return codePage;
	}

	// Get the maximum number of bytes needed to encode a
	// specified number of characters.
	public abstract int GetMaxByteCount (int charCount);

	// Get the maximum number of characters needed to decode a
	// specified number of bytes.
	public abstract int GetMaxCharCount (int byteCount);

	// Get the identifying preamble for this encoding.
	public virtual byte[] GetPreamble ()
	{
		return new byte [0];
	}

	// Decode a buffer of bytes into a string.
	public virtual String GetString (byte[] bytes, int index, int count)
	{
		return new String (GetChars(bytes, index, count));
	}
	public virtual String GetString (byte[] bytes)
	{
		return new String (GetChars(bytes));
	}

#if !ECMA_COMPAT

	internal string body_name;
	internal string encoding_name;
	internal string header_name;
	internal bool is_mail_news_display;
	internal bool is_mail_news_save;
	internal bool is_browser_save = false;
	internal bool is_browser_display = false;
	internal string web_name;

	// Get the mail body name for this encoding.
	public virtual String BodyName
	{
		get {
			return body_name;
		}
	}

	// Get the code page represented by this object.
	public virtual int CodePage
	{
		get {
			return codePage;
		}
	}

	// Get the human-readable name for this encoding.
	public virtual String EncodingName
	{
		get {
			return encoding_name;
		}
	}

	// Get the mail agent header name for this encoding.
	public virtual String HeaderName
	{
		get {
			return header_name;
		}
	}

	// Determine if this encoding can be displayed in a Web browser.
	public virtual bool IsBrowserDisplay
	{
		get {
			return is_browser_display;
		}
	}

	// Determine if this encoding can be saved from a Web browser.
	public virtual bool IsBrowserSave
	{
		get {
			return is_browser_save;
		}
	}

	// Determine if this encoding can be displayed in a mail/news agent.
	public virtual bool IsMailNewsDisplay
	{
		get {
			return is_mail_news_display;
		}
	}

	// Determine if this encoding can be saved from a mail/news agent.
	public virtual bool IsMailNewsSave
	{
		get {
			return is_mail_news_save;
		}
	}

	// Get the IANA-preferred Web name for this encoding.
	public virtual String WebName
	{
		get {
			return web_name;
		}
	}

	// Get the Windows code page represented by this object.
	public virtual int WindowsCodePage
	{
		get {
			// We make no distinction between normal and
			// Windows code pages in this implementation.
			return windows_code_page;
		}
	}

#endif // !ECMA_COMPAT

	// Storage for standard encoding objects.
	static volatile Encoding asciiEncoding;
	static volatile Encoding bigEndianEncoding;
	static volatile Encoding defaultEncoding;
	static volatile Encoding utf7Encoding;
	static volatile Encoding utf8EncodingWithMarkers;
	static volatile Encoding utf8EncodingWithoutMarkers;
	static volatile Encoding unicodeEncoding;
	static volatile Encoding isoLatin1Encoding;
	static volatile Encoding unixConsoleEncoding;
	
	static readonly object lockobj = new object ();

	// Get the standard ASCII encoding object.
	public static Encoding ASCII
	{
		get {
			if (asciiEncoding == null) {
				lock (lockobj) {
					if (asciiEncoding == null) {
						asciiEncoding = new ASCIIEncoding ();
						asciiEncoding.is_readonly = true;
					}
				}
			}

			return asciiEncoding;
		}
	}

	// Get the standard big-endian Unicode encoding object.
	public static Encoding BigEndianUnicode
	{
		get {
			if (bigEndianEncoding == null) {
				lock (lockobj) {
					if (bigEndianEncoding == null) {
						bigEndianEncoding = new UnicodeEncoding (true, true);
						bigEndianEncoding.is_readonly = true;
					}
				}
			}

			return bigEndianEncoding;
		}
	}

	[MethodImpl (MethodImplOptions.InternalCall)]
	extern internal static string InternalCodePage (ref int code_page);
	
	// Get the default encoding object.
	public static Encoding Default
	{
		get {
			if (defaultEncoding == null) {
				lock (lockobj) {
					if (defaultEncoding == null) {
						// See if the underlying system knows what
						// code page handler we should be using.
						int code_page = 1;
						
						string code_page_name = InternalCodePage (ref code_page);
						try {
							if (code_page == -1)
								defaultEncoding = GetEncoding (code_page_name);
							else {
								// map the codepage from internal to our numbers
								code_page = code_page & 0x0fffffff;
								switch (code_page){
								case 1: code_page = ASCIIEncoding.ASCII_CODE_PAGE; break;
								case 2: code_page = UTF7Encoding.UTF7_CODE_PAGE; break;
								case 3: code_page = UTF8Encoding.UTF8_CODE_PAGE; break;
								case 4: code_page = UnicodeEncoding.UNICODE_CODE_PAGE; break;
								case 5: code_page = UnicodeEncoding.BIG_UNICODE_CODE_PAGE; break;
								case 6: code_page = Latin1Encoding.ISOLATIN_CODE_PAGE; break;
								}
								defaultEncoding = GetEncoding (code_page);
							}
						} catch (NotSupportedException) {
							defaultEncoding = UTF8Unmarked;
						}
						defaultEncoding.is_readonly = true;
					}
				}
			}

			return defaultEncoding;
		}
	}

	// Get the ISO Latin1 encoding object.
	private static Encoding ISOLatin1
	{
		get {
			if (isoLatin1Encoding == null) {
				lock (lockobj) {
					if (isoLatin1Encoding == null) {
						isoLatin1Encoding = new Latin1Encoding ();
						isoLatin1Encoding.is_readonly = true;
					}
				}
			}

			return isoLatin1Encoding;
		}
	}

	// Get the standard UTF-7 encoding object.
#if ECMA_COMPAT
	private
#else
	public
#endif
	static Encoding UTF7
	{
		get {
			if (utf7Encoding == null) {
				lock (lockobj) {
					if (utf7Encoding == null) {
						utf7Encoding = new UTF7Encoding ();
						utf7Encoding.is_readonly = true;
					}
				}
			}

			return utf7Encoding;
		}
	}

	// Get the standard UTF-8 encoding object.
	public static Encoding UTF8
	{
		get {
			if (utf8EncodingWithMarkers == null) {
				lock (lockobj) {
					if (utf8EncodingWithMarkers == null) {
						utf8EncodingWithMarkers = new UTF8Encoding (true);
						utf8EncodingWithMarkers.is_readonly = true;
					}
				}
			}

			return utf8EncodingWithMarkers;
		}
	}

	//
	// Only internal, to be used by the class libraries: Unmarked and non-input-validating
	//
	internal static Encoding UTF8Unmarked {
		get {
			if (utf8EncodingWithoutMarkers == null) {
				lock (lockobj){
					if (utf8EncodingWithoutMarkers == null){
						utf8EncodingWithoutMarkers = new UTF8Encoding (false, false);
						utf8EncodingWithoutMarkers.is_readonly = true;
					}
				}
			}

			return utf8EncodingWithoutMarkers;
		}
	}
	
	// Get the standard little-endian Unicode encoding object.
	public static Encoding Unicode
	{
		get {
			if (unicodeEncoding == null) {
				lock (lockobj) {
					if (unicodeEncoding == null) {
						unicodeEncoding = new UnicodeEncoding (false, true);
						unicodeEncoding.is_readonly = true;
					}
				}
			}

			return unicodeEncoding;
		}
	}

	// Forwarding decoder implementation.
	private sealed class ForwardingDecoder : Decoder
	{
		private Encoding encoding;

		// Constructor.
		public ForwardingDecoder (Encoding enc)
		{
			encoding = enc;
#if NET_2_0
			Fallback = encoding.DecoderFallback;
#endif
		}

		// Override inherited methods.
		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			return encoding.GetCharCount (bytes, index, count);
		}
		public override int GetChars (byte[] bytes, int byteIndex,
									 int byteCount, char[] chars,
									 int charIndex)
		{
			return encoding.GetChars (bytes, byteIndex, byteCount, chars, charIndex);
		}

	} // class ForwardingDecoder

	// Forwarding encoder implementation.
	private sealed class ForwardingEncoder : Encoder
	{
		private Encoding encoding;

		// Constructor.
		public ForwardingEncoder (Encoding enc)
		{
			encoding = enc;
#if NET_2_0
			Fallback = encoding.EncoderFallback;
#endif
		}

		// Override inherited methods.
		public override int GetByteCount (char[] chars, int index, int count, bool flush)
		{
			return encoding.GetByteCount (chars, index, count);
		}
		public override int GetBytes (char[] chars, int charIndex,
									 int charCount, byte[] bytes,
									 int byteCount, bool flush)
		{
			return encoding.GetBytes (chars, charIndex, charCount, bytes, byteCount);
		}

	} // class ForwardingEncoder

#if NET_2_0
	[CLSCompliantAttribute(false)]
	public unsafe virtual int GetByteCount (char *chars, int count)
	{
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (count < 0)
			throw new ArgumentOutOfRangeException ("count");
		char [] c = new char [count];

		for (int p = 0; p < count; p++)
			c [p] = chars [p];

		return GetByteCount (c);
	}

	[CLSCompliantAttribute(false)]
	public unsafe virtual int GetCharCount (byte *bytes, int count)
	{
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (count < 0)
			throw new ArgumentOutOfRangeException ("count");
		
		byte [] ba = new byte [count];
		for (int i = 0; i < count; i++)
			ba [i] = bytes [i];
		return GetCharCount (ba, 0, count);
	}

	[CLSCompliantAttribute(false)]
	public unsafe virtual int GetChars (byte *bytes, int byteCount, char *chars, int charCount)
	{
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount");
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount");
		
		byte [] ba = new byte [byteCount];
		for (int i = 0; i < byteCount; i++)
			ba [i] = bytes [i];
		char [] ret = GetChars (ba, 0, byteCount);
		int top = ret.Length;

		if (top > charCount)
			throw new ArgumentException ("charCount is less than the number of characters produced", "charCount");
		
		for (int i = 0; i < top; i++)
			chars [i] = ret [i];
		return top;
	}

	[CLSCompliantAttribute(false)]
	public unsafe virtual int GetBytes (char *chars, int charCount, byte *bytes, int byteCount)
	{
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount");
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount");
		
		char [] c = new char [charCount];
		
		for (int i = 0; i < charCount; i++)
			c [i] = chars [i];

		byte [] b = GetBytes (c, 0, charCount);
		int top = b.Length;
		if (top > byteCount)
			throw new ArgumentException ("byteCount is less that the number of bytes produced", "byteCount");

		for (int i = 0; i < top; i++)
			bytes [i] = b [i];
		
		return b.Length;
	}
#endif

}; // class Encoding

}; // namespace System.Text
