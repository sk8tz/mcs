//
// SignerInfoTest.cs - NUnit tests for SignerInfo
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class SignerInfoTest : Assertion {

		static byte[] asnNull = { 0x05, 0x00 };
		static string sha1Oid = "1.3.14.3.2.26";
		static string sha1Name = "sha1";

		static byte[] issuerAndSerialNumberSignature = { 0x30, 0x82, 0x03, 0x4C, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x02, 0xA0, 0x82, 0x03, 0x3D, 0x30, 0x82, 0x03, 0x39, 0x02, 0x01, 0x01, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1A, 0x05, 0x00, 0x30, 0x11, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x01, 0xA0, 0x04, 0x04, 0x02, 0x05, 0x00, 0xA0, 0x82, 0x02, 0x2E, 0x30, 0x82, 0x02, 0x2A, 0x30, 0x82, 0x01, 0x97, 0xA0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x10, 0x91, 0xC4, 0x4B, 0x0D, 0xB7, 0xD8, 0x10, 0x84, 0x42, 0x26, 0x71, 0xB3, 0x97, 0xB5, 0x00, 0x97, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1D, 0x05, 0x00, 0x30, 0x28, 0x31, 0x26, 0x30, 0x24, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1D, 0x4D, 0x6F, 0x74, 0x75, 0x73, 0x20, 0x54, 0x65,
			0x63, 0x68, 0x6E, 0x6F, 0x6C, 0x6F, 0x67, 0x69, 0x65, 0x73, 0x20, 0x69, 0x6E, 0x63, 0x2E, 0x28, 0x74, 0x65, 0x73, 0x74, 0x29, 0x30, 0x1E, 0x17, 0x0D, 0x30, 0x33, 0x30, 0x38, 0x31, 0x33, 0x30, 0x30, 0x34, 0x33, 0x34, 0x37, 0x5A, 0x17, 0x0D, 0x33, 0x39, 0x31, 0x32, 0x33, 0x31, 0x32, 0x33, 0x35, 0x39, 0x35, 0x39, 0x5A, 0x30, 0x13, 0x31, 0x11, 0x30, 0x0F, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x08, 0x46, 0x41, 0x52, 0x53, 0x43, 0x41, 0x50, 0x45, 0x30, 0x81, 0x9F, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03, 0x81, 0x8D, 0x00, 0x30, 0x81, 0x89, 0x02, 0x81, 0x81, 0x00, 0xD2, 0xCB, 0x47, 0x21, 0xF5, 0x62, 0xDD, 0x35, 0xBF, 0x1D, 0xEC, 0x9A, 0x4C, 0x07, 0x2C, 0x01, 0xF0, 0x28, 0xC2, 0x82, 0x17, 0x8E, 0x58, 0x32, 
			0xD5, 0x4C, 0xAC, 0x86, 0xB4, 0xC9, 0xEB, 0x21, 0x26, 0xF3, 0x22, 0x30, 0xC5, 0x7A, 0xA3, 0x5A, 0xDD, 0x53, 0xAB, 0x1C, 0x06, 0x3E, 0xB2, 0x13, 0xC4, 0x05, 0x1D, 0x95, 0x8B, 0x0A, 0x71, 0x71, 0x11, 0xA7, 0x47, 0x26, 0x61, 0xF1, 0x76, 0xBE, 0x35, 0x72, 0x32, 0xC5, 0xCB, 0x47, 0xA4, 0x22, 0x41, 0x1E, 0xAD, 0x29, 0x11, 0x0D, 0x39, 0x22, 0x0C, 0x79, 0x90, 0xC6, 0x52, 0xA1, 0x10, 0xF6, 0x55, 0x09, 0x4E, 0x51, 0x26, 0x47, 0x0E, 0x94, 0xE6, 0x81, 0xF5, 0x18, 0x6B, 0x99, 0xF0, 0x76, 0xF3, 0xB2, 0x4C, 0x91, 0xE9, 0xBA, 0x3B, 0x3F, 0x6E, 0x63, 0xDA, 0x12, 0xD1, 0x0B, 0x73, 0x0E, 0x12, 0xC7, 0x70, 0x77, 0x22, 0x03, 0x9D, 0x5D, 0x02, 0x03, 0x01, 0x00, 0x01, 0xA3, 0x72, 0x30, 0x70, 0x30, 0x13, 0x06, 0x03, 0x55, 0x1D, 0x25, 0x04, 0x0C, 0x30, 0x0A, 0x06, 0x08, 0x2B, 
			0x06, 0x01, 0x05, 0x05, 0x07, 0x03, 0x01, 0x30, 0x59, 0x06, 0x03, 0x55, 0x1D, 0x01, 0x04, 0x52, 0x30, 0x50, 0x80, 0x10, 0xAE, 0xD7, 0x80, 0x88, 0xA6, 0x3D, 0xBA, 0x50, 0xA1, 0x7E, 0x57, 0xE5, 0x40, 0xC9, 0x6F, 0xC5, 0xA1, 0x2A, 0x30, 0x28, 0x31, 0x26, 0x30, 0x24, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1D, 0x4D, 0x6F, 0x74, 0x75, 0x73, 0x20, 0x54, 0x65, 0x63, 0x68, 0x6E, 0x6F, 0x6C, 0x6F, 0x67, 0x69, 0x65, 0x73, 0x20, 0x69, 0x6E, 0x63, 0x2E, 0x28, 0x74, 0x65, 0x73, 0x74, 0x29, 0x82, 0x10, 0x9D, 0xAE, 0xA3, 0x39, 0x47, 0x0E, 0xD4, 0xA2, 0x49, 0x78, 0xEA, 0x6C, 0xBA, 0x0D, 0xDE, 0x9C, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1D, 0x05, 0x00, 0x03, 0x81, 0x81, 0x00, 0x32, 0x8A, 0x7E, 0xAD, 0xE7, 0x67, 0x9E, 0x5C, 0x4C, 0xD8, 0x33, 0x59, 0x68, 0xCF, 
			0x94, 0xC0, 0x36, 0x47, 0x7A, 0xA7, 0x85, 0xC2, 0xDD, 0xD8, 0xDA, 0x11, 0x3C, 0x66, 0xC1, 0x83, 0xE3, 0xAB, 0x33, 0x06, 0x7C, 0xE3, 0x6A, 0x15, 0x72, 0xB8, 0x83, 0x3D, 0x0B, 0xAB, 0x3C, 0xEE, 0x75, 0x13, 0xBD, 0x5C, 0x96, 0x25, 0x56, 0x36, 0x05, 0xFA, 0xAE, 0xD4, 0xF4, 0xCF, 0x52, 0xEC, 0x11, 0xB5, 0xEA, 0x9F, 0x20, 0xA3, 0xC8, 0x34, 0x72, 0x59, 0x09, 0x51, 0xE7, 0x36, 0x87, 0x86, 0x86, 0x98, 0xB5, 0x30, 0x7B, 0xFB, 0x3D, 0xCC, 0x5E, 0xE8, 0xC9, 0x49, 0xE0, 0xC6, 0xEA, 0x02, 0x76, 0x01, 0xE0, 0xBB, 0x8A, 0x70, 0xEB, 0x07, 0x86, 0xE8, 0x04, 0xE7, 0x48, 0xE4, 0x6C, 0x90, 0xE6, 0x16, 0x42, 0xB4, 0xBB, 0xC0, 0xC4, 0x82, 0x5F, 0xF8, 0xFB, 0x7E, 0xB2, 0x9E, 0xC2, 0x78, 0x26, 0x86, 0x31, 0x81, 0xE1, 0x30, 0x81, 0xDE, 0x02, 0x01, 0x01, 0x30, 0x3C, 0x30, 0x28, 
			0x31, 0x26, 0x30, 0x24, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1D, 0x4D, 0x6F, 0x74, 0x75, 0x73, 0x20, 0x54, 0x65, 0x63, 0x68, 0x6E, 0x6F, 0x6C, 0x6F, 0x67, 0x69, 0x65, 0x73, 0x20, 0x69, 0x6E, 0x63, 0x2E, 0x28, 0x74, 0x65, 0x73, 0x74, 0x29, 0x02, 0x10, 0x91, 0xC4, 0x4B, 0x0D, 0xB7, 0xD8, 0x10, 0x84, 0x42, 0x26, 0x71, 0xB3, 0x97, 0xB5, 0x00, 0x97, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1A, 0x05, 0x00, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x04, 0x81, 0x80, 0x45, 0x88, 0x80, 0x58, 0xC7, 0x4F, 0xE4, 0xD8, 0x88, 0xB0, 0xC0, 0x08, 0x70, 0x84, 0xCC, 0x8E, 0xA7, 0xF1, 0xA4, 0x07, 0x41, 0x14, 0x3E, 0xF5, 0xEA, 0x6E, 0x05, 0x75, 0xB8, 0x58, 0xAA, 0x5C, 0x0E, 0xFD, 0x7A, 0x07, 0x09, 0xE1, 0x80, 0x94, 
			0xBD, 0xAA, 0x45, 0xBB, 0x55, 0x9C, 0xC2, 0xD9, 0x72, 0x14, 0x4B, 0xA4, 0x64, 0xFB, 0x38, 0x9F, 0xD3, 0x22, 0xED, 0xB3, 0x0B, 0xF7, 0xAE, 0x4D, 0xE6, 0x65, 0x4D, 0x2A, 0x31, 0x18, 0xB5, 0xB4, 0x2D, 0x9E, 0x4E, 0xD7, 0xC0, 0x44, 0x5F, 0xAC, 0x43, 0xDC, 0x4F, 0x3D, 0x6D, 0x2C, 0x8C, 0xA1, 0xFE, 0x08, 0x38, 0xB7, 0xC4, 0xC4, 0x08, 0xDB, 0xF8, 0xF0, 0xC1, 0x55, 0x54, 0x49, 0x9D, 0xA4, 0x7F, 0x76, 0xDE, 0xF4, 0x29, 0x1C, 0x0B, 0x95, 0x10, 0x90, 0xB5, 0x0A, 0x9A, 0xEC, 0xCA, 0x89, 0x9A, 0x85, 0x92, 0x76, 0x78, 0x6F, 0x97, 0x67 };

		static byte[] subjectKeyIdentifierSignature = { 0x30, 0x82, 0x03, 0x24, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x02, 0xA0, 0x82, 0x03, 0x15, 0x30, 0x82, 0x03, 0x11, 0x02, 0x01, 0x03, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1A, 0x05, 0x00, 0x30, 0x11, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x01, 0xA0, 0x04, 0x04, 0x02, 0x05, 0x00, 0xA0, 0x82, 0x02, 0x2E, 0x30, 0x82, 0x02, 0x2A, 0x30, 0x82, 0x01, 0x97, 0xA0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x10, 0x91, 0xC4, 0x4B, 0x0D, 0xB7, 0xD8, 0x10, 0x84, 0x42, 0x26, 0x71, 0xB3, 0x97, 0xB5, 0x00, 0x97, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1D, 0x05, 0x00, 0x30, 0x28, 0x31, 0x26, 0x30, 0x24, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1D, 0x4D, 0x6F, 0x74, 0x75, 0x73, 0x20, 0x54, 0x65, 
			0x63, 0x68, 0x6E, 0x6F, 0x6C, 0x6F, 0x67, 0x69, 0x65, 0x73, 0x20, 0x69, 0x6E, 0x63, 0x2E, 0x28, 0x74, 0x65, 0x73, 0x74, 0x29, 0x30, 0x1E, 0x17, 0x0D, 0x30, 0x33, 0x30, 0x38, 0x31, 0x33, 0x30, 0x30, 0x34, 0x33, 0x34, 0x37, 0x5A, 0x17, 0x0D, 0x33, 0x39, 0x31, 0x32, 0x33, 0x31, 0x32, 0x33, 0x35, 0x39, 0x35, 0x39, 0x5A, 0x30, 0x13, 0x31, 0x11, 0x30, 0x0F, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x08, 0x46, 0x41, 0x52, 0x53, 0x43, 0x41, 0x50, 0x45, 0x30, 0x81, 0x9F, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03, 0x81, 0x8D, 0x00, 0x30, 0x81, 0x89, 0x02, 0x81, 0x81, 0x00, 0xD2, 0xCB, 0x47, 0x21, 0xF5, 0x62, 0xDD, 0x35, 0xBF, 0x1D, 0xEC, 0x9A, 0x4C, 0x07, 0x2C, 0x01, 0xF0, 0x28, 0xC2, 0x82, 0x17, 0x8E, 0x58, 0x32, 
			0xD5, 0x4C, 0xAC, 0x86, 0xB4, 0xC9, 0xEB, 0x21, 0x26, 0xF3, 0x22, 0x30, 0xC5, 0x7A, 0xA3, 0x5A, 0xDD, 0x53, 0xAB, 0x1C, 0x06, 0x3E, 0xB2, 0x13, 0xC4, 0x05, 0x1D, 0x95, 0x8B, 0x0A, 0x71, 0x71, 0x11, 0xA7, 0x47, 0x26, 0x61, 0xF1, 0x76, 0xBE, 0x35, 0x72, 0x32, 0xC5, 0xCB, 0x47, 0xA4, 0x22, 0x41, 0x1E, 0xAD, 0x29, 0x11, 0x0D, 0x39, 0x22, 0x0C, 0x79, 0x90, 0xC6, 0x52, 0xA1, 0x10, 0xF6, 0x55, 0x09, 0x4E, 0x51, 0x26, 0x47, 0x0E, 0x94, 0xE6, 0x81, 0xF5, 0x18, 0x6B, 0x99, 0xF0, 0x76, 0xF3, 0xB2, 0x4C, 0x91, 0xE9, 0xBA, 0x3B, 0x3F, 0x6E, 0x63, 0xDA, 0x12, 0xD1, 0x0B, 0x73, 0x0E, 0x12, 0xC7, 0x70, 0x77, 0x22, 0x03, 0x9D, 0x5D, 0x02, 0x03, 0x01, 0x00, 0x01, 0xA3, 0x72, 0x30, 0x70, 0x30, 0x13, 0x06, 0x03, 0x55, 0x1D, 0x25, 0x04, 0x0C, 0x30, 0x0A, 0x06, 0x08, 0x2B, 
			0x06, 0x01, 0x05, 0x05, 0x07, 0x03, 0x01, 0x30, 0x59, 0x06, 0x03, 0x55, 0x1D, 0x01, 0x04, 0x52, 0x30, 0x50, 0x80, 0x10, 0xAE, 0xD7, 0x80, 0x88, 0xA6, 0x3D, 0xBA, 0x50, 0xA1, 0x7E, 0x57, 0xE5, 0x40, 0xC9, 0x6F, 0xC5, 0xA1, 0x2A, 0x30, 0x28, 0x31, 0x26, 0x30, 0x24, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1D, 0x4D, 0x6F, 0x74, 0x75, 0x73, 0x20, 0x54, 0x65, 0x63, 0x68, 0x6E, 0x6F, 0x6C, 0x6F, 0x67, 0x69, 0x65, 0x73, 0x20, 0x69, 0x6E, 0x63, 0x2E, 0x28, 0x74, 0x65, 0x73, 0x74, 0x29, 0x82, 0x10, 0x9D, 0xAE, 0xA3, 0x39, 0x47, 0x0E, 0xD4, 0xA2, 0x49, 0x78, 0xEA, 0x6C, 0xBA, 0x0D, 0xDE, 0x9C, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1D, 0x05, 0x00, 0x03, 0x81, 0x81, 0x00, 0x32, 0x8A, 0x7E, 0xAD, 0xE7, 0x67, 0x9E, 0x5C, 0x4C, 0xD8, 0x33, 0x59, 0x68, 0xCF, 
			0x94, 0xC0, 0x36, 0x47, 0x7A, 0xA7, 0x85, 0xC2, 0xDD, 0xD8, 0xDA, 0x11, 0x3C, 0x66, 0xC1, 0x83, 0xE3, 0xAB, 0x33, 0x06, 0x7C, 0xE3, 0x6A, 0x15, 0x72, 0xB8, 0x83, 0x3D, 0x0B, 0xAB, 0x3C, 0xEE, 0x75, 0x13, 0xBD, 0x5C, 0x96, 0x25, 0x56, 0x36, 0x05, 0xFA, 0xAE, 0xD4, 0xF4, 0xCF, 0x52, 0xEC, 0x11, 0xB5, 0xEA, 0x9F, 0x20, 0xA3, 0xC8, 0x34, 0x72, 0x59, 0x09, 0x51, 0xE7, 0x36, 0x87, 0x86, 0x86, 0x98, 0xB5, 0x30, 0x7B, 0xFB, 0x3D, 0xCC, 0x5E, 0xE8, 0xC9, 0x49, 0xE0, 0xC6, 0xEA, 0x02, 0x76, 0x01, 0xE0, 0xBB, 0x8A, 0x70, 0xEB, 0x07, 0x86, 0xE8, 0x04, 0xE7, 0x48, 0xE4, 0x6C, 0x90, 0xE6, 0x16, 0x42, 0xB4, 0xBB, 0xC0, 0xC4, 0x82, 0x5F, 0xF8, 0xFB, 0x7E, 0xB2, 0x9E, 0xC2, 0x78, 0x26, 0x86, 0x31, 0x81, 0xB9, 0x30, 0x81, 0xB6, 0x02, 0x01, 0x03, 0x80, 0x14, 0x02, 0xE1, 
			0xA7, 0x32, 0x54, 0xAE, 0xFD, 0xC0, 0xA4, 0x32, 0x36, 0xF6, 0xFE, 0x23, 0x6A, 0x03, 0x72, 0x28, 0xB1, 0xF7, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1A, 0x05, 0x00, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x04, 0x81, 0x80, 0x45, 0x88, 0x80, 0x58, 0xC7, 0x4F, 0xE4, 0xD8, 0x88, 0xB0, 0xC0, 0x08, 0x70, 0x84, 0xCC, 0x8E, 0xA7, 0xF1, 0xA4, 0x07, 0x41, 0x14, 0x3E, 0xF5, 0xEA, 0x6E, 0x05, 0x75, 0xB8, 0x58, 0xAA, 0x5C, 0x0E, 0xFD, 0x7A, 0x07, 0x09, 0xE1, 0x80, 0x94, 0xBD, 0xAA, 0x45, 0xBB, 0x55, 0x9C, 0xC2, 0xD9, 0x72, 0x14, 0x4B, 0xA4, 0x64, 0xFB, 0x38, 0x9F, 0xD3, 0x22, 0xED, 0xB3, 0x0B, 0xF7, 0xAE, 0x4D, 0xE6, 0x65, 0x4D, 0x2A, 0x31, 0x18, 0xB5, 0xB4, 0x2D, 0x9E, 0x4E, 0xD7, 0xC0, 0x44, 0x5F, 0xAC, 
			0x43, 0xDC, 0x4F, 0x3D, 0x6D, 0x2C, 0x8C, 0xA1, 0xFE, 0x08, 0x38, 0xB7, 0xC4, 0xC4, 0x08, 0xDB, 0xF8, 0xF0, 0xC1, 0x55, 0x54, 0x49, 0x9D, 0xA4, 0x7F, 0x76, 0xDE, 0xF4, 0x29, 0x1C, 0x0B, 0x95, 0x10, 0x90, 0xB5, 0x0A, 0x9A, 0xEC, 0xCA, 0x89, 0x9A, 0x85, 0x92, 0x76, 0x78, 0x6F, 0x97, 0x67 };

		private SignerInfo GetSignerInfo (byte[] signature) 
		{
			SignedCms sp = new SignedCms ();
			sp.Decode (signature);
			return sp.SignerInfos [0];
		}

		[Test]
		public void IssuerAndSerialNumberProperties () 
		{
			SignerInfo si = GetSignerInfo (issuerAndSerialNumberSignature);
			// default properties
			AssertEquals ("SignedAttributes", 0, si.SignedAttributes.Count);
			AssertNotNull ("Certificate", si.Certificate);
			AssertEquals ("CounterSignerInfos", 0, si.CounterSignerInfos.Count);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, si.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, si.DigestAlgorithm.Value);
			AssertEquals ("SignerIdentifier.Type", SubjectIdentifierType.IssuerAndSerialNumber, si.SignerIdentifier.Type);
			Assert ("SignerIdentifier.Value", (si.SignerIdentifier.Value.GetType () == typeof (X509IssuerSerial)));
			AssertEquals ("UnsignedAttributes", 0, si.UnsignedAttributes.Count);
			AssertEquals ("Version", 1, si.Version);
		}

		[Test]
		public void SubjectKeyIdentifierProperties () 
		{
			SignerInfo si = GetSignerInfo (subjectKeyIdentifierSignature);
			// default properties
			AssertEquals ("SignedAttributes", 0, si.SignedAttributes.Count);
			AssertNotNull ("Certificate", si.Certificate);
			AssertEquals ("CounterSignerInfos", 0, si.CounterSignerInfos.Count);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, si.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, si.DigestAlgorithm.Value);
			AssertEquals ("SignerIdentifier.Type", SubjectIdentifierType.SubjectKeyIdentifier, si.SignerIdentifier.Type);
			AssertEquals ("SignerIdentifier.Value", "02E1A73254AEFDC0A43236F6FE236A037228B1F7", (string)si.SignerIdentifier.Value);
			AssertEquals ("UnsignedAttributes", 0, si.UnsignedAttributes.Count);
			AssertEquals ("Version", 3, si.Version);
		}
	}
}

#endif
