/*
 * Decoder.cs - Implementation of the "System.Text.Decoder" class.
 *
 * Copyright (c) 2001  Southern Storm Software, Pty Ltd
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

[Serializable]
public abstract class Decoder
{

	// Constructor.
	protected Decoder () {}

#if NET_2_0
	DecoderFallback fallback;

	public DecoderFallback Fallback {
		get { return fallback; }
		set {
			if (value == null)
				throw new ArgumentNullException ();
			fallback = value;
		}
	}
#endif

	// Get the number of characters needed to decode a buffer.
	public abstract int GetCharCount (byte[] bytes, int index, int count);

	// Get the characters that result from decoding a buffer.
	public abstract int GetChars (byte[] bytes, int byteIndex, int byteCount,
								 char[] chars, int charIndex);

}; // class Decoder

}; // namespace System.Text
