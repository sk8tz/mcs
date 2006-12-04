//
// X509CertificateTest.cs - NUnit Test Cases for 
//	Mono.Security.X509.X509Certificate
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Security.Cryptography;

using Mono.Security.X509;

using NUnit.Framework;

namespace MonoTests.Mono.Security.X509 {

	[TestFixture]
	public class X509CertificateTest {

		static public byte[] DSACACert_crt = { 0x30, 0x82, 0x03, 0x86, 0x30, 0x82, 0x02, 0xEF, 0xA0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x02, 0x07, 0xD1, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x30, 0x40, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x15, 0x30, 0x13, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x0C, 0x54, 0x72, 0x75, 0x73, 0x74, 0x20, 0x41, 0x6E, 0x63, 0x68, 0x6F, 0x72, 0x30, 0x1E, 0x17, 
			0x0D, 0x30, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x17, 0x0D, 0x31, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x30, 0x3A, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x0F, 0x30, 0x0D, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x06, 0x44, 0x53, 0x41, 0x20, 0x43, 0x41, 0x30, 0x82, 0x01, 0xB7, 0x30, 0x82, 0x01, 0x2C, 0x06, 0x07, 0x2A, 
			0x86, 0x48, 0xCE, 0x38, 0x04, 0x01, 0x30, 0x82, 0x01, 0x1F, 0x02, 0x81, 0x81, 0x00, 0xDF, 0xE5, 0x11, 0x3E, 0xDA, 0xE9, 0xB6, 0x6E, 0x10, 0xD6, 0xEA, 0x9D, 0xE2, 0x9D, 0x3A, 0xB0, 0x47, 0xBD, 0x44, 0x00, 0xC7, 0x56, 0xC8, 0xCC, 0x6E, 0xD0, 0x33, 0x84, 0x56, 0x47, 0x35, 0x3D, 0xF6, 0x54, 0xC8, 0xE3, 0xC2, 0xAD, 0xBB, 0xBA, 0x75, 0xF3, 0x2F, 0x33, 0x0C, 0xA6, 0xF9, 0x31, 0xEC, 0x67, 0xE3, 0xE5, 0x99, 0x6D, 0xFC, 0x29, 0x6E, 0xAA, 0x57, 0x88, 0x72, 0x34, 0xE2, 0x8E, 0x00, 0x90, 0xE0, 0xA3, 0xAC, 0x64, 0x8E, 0xC0, 0xF6, 0x3C, 0x1D, 0x8F, 0xB4, 0xC8, 0x4A, 0x48, 0x30, 0x5E, 0x7F, 0xAA, 0x9C, 0x76, 
			0x26, 0x0B, 0xDB, 0x13, 0x73, 0x33, 0x83, 0x97, 0xC0, 0xC5, 0xB3, 0xE6, 0x37, 0xF5, 0x3E, 0xFF, 0x15, 0x0D, 0xD4, 0xA1, 0x2E, 0xBA, 0x31, 0xF8, 0xAB, 0x87, 0xD8, 0x0B, 0xCC, 0x77, 0x98, 0x42, 0x6E, 0xAC, 0x93, 0x98, 0xC2, 0xBD, 0x2E, 0x7B, 0x34, 0x0B, 0x02, 0x15, 0x00, 0xCF, 0x06, 0x04, 0xD8, 0xFF, 0x8B, 0xEB, 0x00, 0xE9, 0xF6, 0x5C, 0x07, 0x96, 0x73, 0xFD, 0x96, 0x65, 0x3A, 0x2F, 0x07, 0x02, 0x81, 0x81, 0x00, 0xCC, 0xE1, 0x7C, 0xB0, 0xCE, 0x92, 0x5F, 0x63, 0xEC, 0x38, 0xBB, 0x44, 0xBA, 0xDD, 0x92, 0x34, 0xB6, 0x5E, 0xBE, 0x65, 0x7B, 0xD8, 0x71, 0x77, 0x04, 0x9D, 0xEC, 0x66, 0x7C, 0x3B, 0x04, 
			0xCE, 0xB6, 0xF3, 0x52, 0xFE, 0x0F, 0x92, 0x55, 0x02, 0xEF, 0x4E, 0x12, 0xAB, 0x5D, 0x9A, 0x2E, 0x2F, 0x6E, 0x56, 0xF3, 0x70, 0xEC, 0x6A, 0xED, 0x9B, 0x22, 0xB8, 0xA8, 0x13, 0xCB, 0x0C, 0x9C, 0x16, 0xEA, 0xC1, 0x0A, 0x8E, 0x21, 0x26, 0x44, 0xA5, 0x0C, 0xF9, 0xA0, 0xEC, 0x62, 0xE0, 0x70, 0x31, 0xCC, 0x68, 0xF5, 0x0B, 0x85, 0xA4, 0x4A, 0x1B, 0x6E, 0x79, 0xF4, 0xC1, 0xF9, 0x36, 0x5A, 0x38, 0x6F, 0x4E, 0xEF, 0x84, 0x53, 0xDF, 0x67, 0xFD, 0xCC, 0xF7, 0x59, 0x62, 0x8F, 0x9C, 0x9C, 0xCD, 0x10, 0x8F, 0x5C, 0xA4, 0x0F, 0x9C, 0xB7, 0x07, 0xEC, 0x60, 0xF3, 0xBE, 0xAF, 0x7E, 0x39, 0x98, 0x03, 0x81, 0x84, 
			0x00, 0x02, 0x81, 0x80, 0x11, 0xF2, 0xB9, 0xD8, 0xBE, 0x42, 0x2B, 0xC5, 0x84, 0xBE, 0x91, 0x02, 0x1C, 0xFC, 0x8C, 0x32, 0x72, 0x8B, 0xA8, 0x6C, 0x21, 0xD7, 0x88, 0x8A, 0x14, 0xBA, 0x30, 0x65, 0x75, 0xC0, 0x1C, 0x3D, 0x82, 0x69, 0x65, 0xA7, 0xAC, 0x90, 0x7A, 0x14, 0x1D, 0x85, 0x7B, 0xE5, 0x53, 0xC2, 0x60, 0xFC, 0xB1, 0xCF, 0x67, 0xAF, 0xC1, 0xF2, 0x2E, 0x08, 0x32, 0x6A, 0x38, 0xC7, 0x91, 0x4E, 0x3B, 0xBC, 0x3C, 0x09, 0xD0, 0xF9, 0x71, 0x6D, 0x08, 0xDF, 0x27, 0x49, 0x8D, 0x05, 0x74, 0xD8, 0xBD, 0x46, 0xD0, 0xDB, 0x51, 0xA5, 0x53, 0xBA, 0x87, 0xF3, 0xFA, 0x5D, 0x25, 0x83, 0x4F, 0x7F, 0x0A, 0x75, 
			0xE5, 0xA9, 0xE3, 0x89, 0xA7, 0x41, 0x77, 0x63, 0x40, 0x5F, 0x2B, 0x2C, 0x84, 0xD2, 0xC1, 0x71, 0x78, 0x0A, 0xDB, 0x6B, 0x57, 0x19, 0xE1, 0xE7, 0x14, 0x0C, 0x9E, 0xC4, 0xF6, 0x32, 0x39, 0x0E, 0xA3, 0x7C, 0x30, 0x7A, 0x30, 0x1D, 0x06, 0x03, 0x55, 0x1D, 0x0E, 0x04, 0x16, 0x04, 0x14, 0x74, 0x15, 0xD5, 0x24, 0x1C, 0xBD, 0x5E, 0x65, 0x88, 0x1F, 0xE1, 0x8B, 0x09, 0x7E, 0x7F, 0xEA, 0x19, 0x48, 0x4E, 0x61, 0x30, 0x1F, 0x06, 0x03, 0x55, 0x1D, 0x23, 0x04, 0x18, 0x30, 0x16, 0x80, 0x14, 0xFB, 0x6C, 0xD4, 0x2D, 0x81, 0x9E, 0xCA, 0x27, 0x7A, 0x9E, 0x0D, 0xB0, 0x3C, 0xEA, 0x9A, 0xBC, 0x87, 0xFF, 0x49, 0xEA, 
			0x30, 0x17, 0x06, 0x03, 0x55, 0x1D, 0x20, 0x04, 0x10, 0x30, 0x0E, 0x30, 0x0C, 0x06, 0x0A, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x02, 0x01, 0x30, 0x01, 0x30, 0x0E, 0x06, 0x03, 0x55, 0x1D, 0x0F, 0x01, 0x01, 0xFF, 0x04, 0x04, 0x03, 0x02, 0x01, 0x06, 0x30, 0x0F, 0x06, 0x03, 0x55, 0x1D, 0x13, 0x01, 0x01, 0xFF, 0x04, 0x05, 0x30, 0x03, 0x01, 0x01, 0xFF, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05, 0x05, 0x00, 0x03, 0x81, 0x81, 0x00, 0x3A, 0x3B, 0x72, 0x1F, 0x77, 0x2C, 0xBA, 0xDC, 0xEC, 0xF5, 0x1D, 0x5B, 0x45, 0xCE, 0x3F, 0x7F, 0xA4, 0x3F, 0xE4, 0xB2, 0xC1, 0xFA, 0x9F, 
			0xD8, 0x0A, 0x5C, 0x98, 0xE2, 0xEE, 0x7E, 0x63, 0x3A, 0xD2, 0xC8, 0xB0, 0xE2, 0xBC, 0xC0, 0xD6, 0xCB, 0x28, 0x21, 0x30, 0x76, 0x46, 0xCD, 0xD3, 0x3D, 0x02, 0x81, 0x88, 0x9C, 0xCC, 0x74, 0x52, 0xAB, 0xAB, 0xCB, 0x50, 0xFB, 0xC4, 0xDC, 0xA5, 0x72, 0x7D, 0x33, 0x84, 0x95, 0x7D, 0xB0, 0x05, 0x07, 0x43, 0xF4, 0xBC, 0x1E, 0x14, 0x0B, 0x61, 0x20, 0xEA, 0x24, 0xA7, 0x54, 0x96, 0xC1, 0xB6, 0xC6, 0x45, 0x8F, 0x5D, 0xA0, 0xA6, 0xAB, 0xF9, 0x19, 0xAC, 0x28, 0xDF, 0x25, 0x13, 0xC3, 0x7E, 0x21, 0xDE, 0x8A, 0x43, 0x19, 0x25, 0xF5, 0xA7, 0x3E, 0x9E, 0x65, 0x42, 0x19, 0x04, 0x52, 0xA9, 0x01, 0x7E, 0x60, 0xC8, 
			0x8A, 0x62, 0x10, 0x12, 0x36 };

		static public byte[] DSAParametersInheritedCACert_crt = { 0x30, 0x82, 0x02, 0x14, 0x30, 0x82, 0x01, 0xD3, 0xA0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x01, 0x02, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x03, 0x30, 0x3A, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x0F, 0x30, 0x0D, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x06, 0x44, 0x53, 0x41, 0x20, 0x43, 0x41, 0x30, 0x1E, 0x17, 0x0D, 0x30, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 
			0x32, 0x30, 0x5A, 0x17, 0x0D, 0x31, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x30, 0x4F, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x24, 0x30, 0x22, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1B, 0x44, 0x53, 0x41, 0x20, 0x50, 0x61, 0x72, 0x61, 0x6D, 0x65, 0x74, 0x65, 0x72, 0x73, 0x20, 0x49, 0x6E, 0x68, 0x65, 0x72, 0x69, 0x74, 0x65, 0x64, 0x20, 0x43, 0x41, 0x30, 
			0x81, 0x92, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x01, 0x03, 0x81, 0x84, 0x00, 0x02, 0x81, 0x80, 0x67, 0x8C, 0x47, 0xDA, 0x0C, 0x36, 0x81, 0x64, 0x39, 0xF8, 0x1A, 0x37, 0x0B, 0xF0, 0xBB, 0xD4, 0x13, 0xFD, 0x67, 0x3D, 0x40, 0xC8, 0x9C, 0x92, 0xE1, 0x3A, 0x89, 0x54, 0xB0, 0xF2, 0x97, 0xA5, 0x70, 0xD0, 0x64, 0x17, 0xA4, 0xA7, 0x7C, 0xA9, 0xE9, 0x27, 0x09, 0x65, 0xDF, 0xA0, 0xA5, 0x1C, 0xFC, 0x04, 0xDA, 0x3E, 0xDD, 0x62, 0x6B, 0xF0, 0x0C, 0xED, 0x81, 0x57, 0x4F, 0x5D, 0x29, 0xC8, 0x08, 0x59, 0x19, 0x0C, 0x5F, 0x8D, 0x88, 0xF0, 0xFF, 0x8C, 0xB6, 0xE8, 0x75, 0x01, 0xBE, 0x10, 
			0x0A, 0x64, 0xDF, 0x57, 0x7B, 0x8B, 0x6C, 0x56, 0xF0, 0x53, 0xCE, 0x43, 0x88, 0xB9, 0x09, 0x99, 0x08, 0x3C, 0x90, 0xAF, 0x01, 0xE8, 0xD9, 0x32, 0x5D, 0xED, 0x56, 0x02, 0xCF, 0x60, 0xD2, 0x01, 0x3A, 0xE3, 0x0A, 0x2B, 0x9E, 0x91, 0x41, 0xF6, 0xC7, 0x7C, 0xDE, 0x6C, 0x99, 0x63, 0x00, 0x0E, 0xA3, 0x7C, 0x30, 0x7A, 0x30, 0x1D, 0x06, 0x03, 0x55, 0x1D, 0x0E, 0x04, 0x16, 0x04, 0x14, 0x5D, 0x24, 0xEE, 0x8A, 0x55, 0x1A, 0xF2, 0xC6, 0xC9, 0xB2, 0xC2, 0xBF, 0x8A, 0xF0, 0xB2, 0x49, 0x4F, 0x3A, 0xB3, 0x1B, 0x30, 0x1F, 0x06, 0x03, 0x55, 0x1D, 0x23, 0x04, 0x18, 0x30, 0x16, 0x80, 0x14, 0x74, 0x15, 0xD5, 0x24, 
			0x1C, 0xBD, 0x5E, 0x65, 0x88, 0x1F, 0xE1, 0x8B, 0x09, 0x7E, 0x7F, 0xEA, 0x19, 0x48, 0x4E, 0x61, 0x30, 0x17, 0x06, 0x03, 0x55, 0x1D, 0x20, 0x04, 0x10, 0x30, 0x0E, 0x30, 0x0C, 0x06, 0x0A, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x02, 0x01, 0x30, 0x01, 0x30, 0x0E, 0x06, 0x03, 0x55, 0x1D, 0x0F, 0x01, 0x01, 0xFF, 0x04, 0x04, 0x03, 0x02, 0x01, 0x06, 0x30, 0x0F, 0x06, 0x03, 0x55, 0x1D, 0x13, 0x01, 0x01, 0xFF, 0x04, 0x05, 0x30, 0x03, 0x01, 0x01, 0xFF, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x03, 0x03, 0x30, 0x00, 0x30, 0x2D, 0x02, 0x15, 0x00, 0xA8, 0x59, 0x6F, 0x31, 0x77, 0xB6, 0x20, 
			0xEC, 0x36, 0x9B, 0xEB, 0x4B, 0x61, 0x0A, 0xAF, 0x44, 0xED, 0x72, 0xBA, 0x29, 0x02, 0x14, 0x6D, 0x22, 0xE1, 0xBD, 0x4D, 0x27, 0xF6, 0x2E, 0x3B, 0x1F, 0xD7, 0x9D, 0xD6, 0x59, 0x5E, 0xCB, 0x25, 0x86, 0x22, 0xD8 };

		static public byte[] ValidDSASignaturesTest4EE_crt = { 0x30, 0x82, 0x03, 0x36, 0x30, 0x82, 0x02, 0xF5, 0xA0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x01, 0x01, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x03, 0x30, 0x3A, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x0F, 0x30, 0x0D, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x06, 0x44, 0x53, 0x41, 0x20, 0x43, 0x41, 0x30, 0x1E, 0x17, 0x0D, 0x30, 
			0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x17, 0x0D, 0x31, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x30, 0x5D, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x32, 0x30, 0x30, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x29, 0x56, 0x61, 0x6C, 0x69, 0x64, 0x20, 0x44, 0x53, 0x41, 0x20, 0x53, 0x69, 0x67, 0x6E, 0x61, 0x74, 0x75, 0x72, 0x65, 
			0x73, 0x20, 0x45, 0x45, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x20, 0x54, 0x65, 0x73, 0x74, 0x34, 0x30, 0x82, 0x01, 0xB6, 0x30, 0x82, 0x01, 0x2B, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x01, 0x30, 0x82, 0x01, 0x1E, 0x02, 0x81, 0x81, 0x00, 0xE4, 0x8B, 0xAF, 0x40, 0x8C, 0x15, 0xD7, 0x3D, 0x7C, 0xEE, 0x03, 0x96, 0x12, 0x68, 0xC1, 0x20, 0x05, 0xE8, 0x17, 0xCA, 0x9E, 0x74, 0x65, 0x4B, 0x9A, 0x54, 0x97, 0x2A, 0x78, 0x33, 0xDA, 0xA5, 0xC5, 0x72, 0xEA, 0x34, 0xB3, 0x94, 0x68, 0x42, 0xD5, 0x1B, 0xFD, 0x77, 0xF0, 0xA8, 0x42, 0x9E, 0x64, 0x93, 0x90, 0xB6, 0xC2, 
			0x02, 0x31, 0x46, 0x13, 0x7A, 0x03, 0x69, 0xCC, 0x98, 0x2D, 0x56, 0x9D, 0x5E, 0x23, 0x5F, 0x28, 0xBF, 0xAD, 0x7F, 0x0F, 0xD0, 0x69, 0x95, 0x62, 0x5C, 0x1A, 0x07, 0x2A, 0x5E, 0x8C, 0x73, 0x49, 0x7E, 0xFD, 0x12, 0x22, 0x8E, 0x55, 0xE5, 0x56, 0xE9, 0xAE, 0x72, 0x29, 0x96, 0x87, 0x08, 0x27, 0xD7, 0x77, 0x43, 0xF0, 0x86, 0xB8, 0x09, 0x0A, 0x1B, 0x14, 0xA5, 0xE6, 0x03, 0xE6, 0x45, 0x79, 0x4D, 0xE9, 0xFA, 0x53, 0x5F, 0xC1, 0x09, 0xBD, 0x7E, 0xC5, 0xC3, 0x02, 0x15, 0x00, 0x80, 0x3F, 0xE4, 0xFC, 0xF3, 0x4C, 0xE5, 0x3E, 0xCB, 0x0F, 0x17, 0x0A, 0x2A, 0x54, 0x6C, 0xD0, 0x67, 0x6C, 0x0D, 0x3B, 0x02, 0x81, 
			0x80, 0x66, 0xD4, 0x16, 0x8A, 0x20, 0xAD, 0xFE, 0xD1, 0x32, 0x9F, 0xA5, 0x7F, 0xA7, 0xB3, 0xD0, 0xEA, 0x77, 0x3F, 0xEB, 0x6C, 0xA2, 0xE4, 0x29, 0xD8, 0xD8, 0xBC, 0x21, 0xDD, 0x9A, 0xF7, 0xCC, 0xE5, 0xB4, 0x77, 0x4D, 0xDF, 0xEC, 0xDA, 0xA2, 0x8C, 0x9C, 0x75, 0x12, 0x5A, 0x1F, 0xFE, 0x66, 0xD3, 0x11, 0xC2, 0xEF, 0x84, 0x43, 0xEC, 0xA9, 0x88, 0x6E, 0x4C, 0xBA, 0x4C, 0x3F, 0x35, 0x96, 0xC7, 0x67, 0xFC, 0x99, 0xBD, 0x0F, 0x99, 0x29, 0x13, 0x91, 0x4E, 0xD8, 0x02, 0xAE, 0xFE, 0x6B, 0xAF, 0x50, 0x56, 0xAA, 0x2F, 0x1E, 0xB5, 0x2A, 0xC8, 0xEE, 0x22, 0x47, 0x25, 0x78, 0x6B, 0x21, 0xDD, 0x14, 0x3F, 0xCE, 
			0xF0, 0x10, 0x81, 0xF7, 0x96, 0x1D, 0x9C, 0x41, 0xBB, 0x5E, 0x44, 0x92, 0x5D, 0x2E, 0xC6, 0x1E, 0xB8, 0xCD, 0x69, 0xC8, 0x8F, 0x3F, 0x3B, 0x3E, 0xD0, 0x4F, 0xA2, 0xCE, 0xD9, 0x03, 0x81, 0x84, 0x00, 0x02, 0x81, 0x80, 0x0F, 0x53, 0x28, 0xAC, 0x38, 0x2F, 0x3D, 0xF3, 0x11, 0x61, 0x41, 0xC3, 0x3D, 0xA7, 0xD6, 0x7A, 0xF7, 0xF6, 0x01, 0x32, 0xD3, 0x21, 0x71, 0x10, 0x14, 0xD5, 0xC3, 0x3E, 0x4D, 0xEB, 0x19, 0xA2, 0x8C, 0xAF, 0x9E, 0x08, 0x3D, 0x41, 0x0A, 0xFF, 0xCC, 0xA2, 0x47, 0x82, 0x7A, 0x56, 0xA1, 0xA3, 0xFD, 0xEC, 0xB2, 0x8B, 0xB7, 0x39, 0xB5, 0xCA, 0xA0, 0x19, 0x85, 0x82, 0x9B, 0x96, 0x68, 0xA8, 
			0xBB, 0x6B, 0xBA, 0x90, 0xA4, 0xE1, 0xAD, 0x65, 0xB6, 0x44, 0x31, 0xD2, 0x1E, 0x22, 0x2F, 0x53, 0x41, 0x4F, 0xFA, 0x9C, 0xF8, 0x2F, 0xE8, 0x2C, 0x43, 0x24, 0x16, 0x7E, 0x2B, 0xD8, 0x64, 0xF7, 0x64, 0xFA, 0xF0, 0x79, 0x48, 0x1D, 0xB9, 0x02, 0x6D, 0x90, 0x36, 0xCC, 0xEB, 0x36, 0x0F, 0xF2, 0x39, 0xAB, 0x7D, 0x27, 0xEC, 0xF7, 0x47, 0x6F, 0xDD, 0x33, 0xC4, 0x7E, 0x4D, 0xEE, 0x24, 0x57, 0xA3, 0x6B, 0x30, 0x69, 0x30, 0x1D, 0x06, 0x03, 0x55, 0x1D, 0x0E, 0x04, 0x16, 0x04, 0x14, 0xB3, 0x33, 0xD7, 0x51, 0xA2, 0x04, 0x0D, 0x44, 0xFB, 0x9D, 0x40, 0xF1, 0x12, 0x62, 0x71, 0xB0, 0x53, 0xF6, 0x69, 0x0D, 0x30, 
			0x1F, 0x06, 0x03, 0x55, 0x1D, 0x23, 0x04, 0x18, 0x30, 0x16, 0x80, 0x14, 0x74, 0x15, 0xD5, 0x24, 0x1C, 0xBD, 0x5E, 0x65, 0x88, 0x1F, 0xE1, 0x8B, 0x09, 0x7E, 0x7F, 0xEA, 0x19, 0x48, 0x4E, 0x61, 0x30, 0x17, 0x06, 0x03, 0x55, 0x1D, 0x20, 0x04, 0x10, 0x30, 0x0E, 0x30, 0x0C, 0x06, 0x0A, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x02, 0x01, 0x30, 0x01, 0x30, 0x0E, 0x06, 0x03, 0x55, 0x1D, 0x0F, 0x01, 0x01, 0xFF, 0x04, 0x04, 0x03, 0x02, 0x06, 0xC0, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x03, 0x03, 0x30, 0x00, 0x30, 0x2D, 0x02, 0x15, 0x00, 0x8C, 0xA7, 0xC8, 0xD2, 0x99, 0xD4, 0x40, 0x9B, 
			0xF9, 0x21, 0x92, 0x68, 0xF3, 0x27, 0x26, 0x09, 0x73, 0xA2, 0x59, 0x18, 0x02, 0x14, 0x4C, 0xFE, 0x1F, 0x80, 0xBB, 0x30, 0x80, 0xD7, 0xD8, 0x70, 0xC6, 0x4E, 0x76, 0xA0, 0xD9, 0x9D, 0xB4, 0xF6, 0x40, 0xEA };

		static public byte[] ValidDSAParameterInheritanceTest5EE_crt = { 0x30, 0x82, 0x02, 0x32, 0x30, 0x82, 0x01, 0xF1, 0xA0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x01, 0x01, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x03, 0x30, 0x4F, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x24, 0x30, 0x22, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1B, 0x44, 0x53, 0x41, 0x20, 0x50, 0x61, 0x72, 0x61, 0x6D, 0x65, 0x74, 0x65, 0x72, 0x73, 0x20, 0x49, 0x6E, 0x68, 0x65, 0x72, 
			0x69, 0x74, 0x65, 0x64, 0x20, 0x43, 0x41, 0x30, 0x1E, 0x17, 0x0D, 0x30, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x17, 0x0D, 0x31, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x30, 0x68, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x3D, 0x30, 0x3B, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x34, 0x56, 0x61, 0x6C, 0x69, 0x64, 0x20, 0x44, 
			0x53, 0x41, 0x20, 0x50, 0x61, 0x72, 0x61, 0x6D, 0x65, 0x74, 0x65, 0x72, 0x20, 0x49, 0x6E, 0x68, 0x65, 0x72, 0x69, 0x74, 0x61, 0x6E, 0x63, 0x65, 0x20, 0x45, 0x45, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x20, 0x54, 0x65, 0x73, 0x74, 0x35, 0x30, 0x81, 0x93, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x01, 0x03, 0x81, 0x85, 0x00, 0x02, 0x81, 0x81, 0x00, 0xCE, 0x8B, 0x36, 0xD8, 0x5C, 0x44, 0x41, 0xC7, 0xEA, 0x9A, 0xB0, 0xD8, 0x94, 0x39, 0x45, 0xB3, 0x49, 0xB4, 0x6C, 0x66, 0xBD, 0xD6, 0x9D, 0xB0, 0xEB, 0x89, 0xBE, 0x7E, 0x5B, 0xD0, 0xDB, 0x33, 0x21, 
			0x83, 0x1B, 0x4A, 0x92, 0x6A, 0xD7, 0x76, 0xD2, 0xA3, 0xC8, 0x4B, 0xC7, 0x9B, 0x95, 0x7C, 0x4B, 0xE4, 0x19, 0xD7, 0x34, 0x06, 0x9A, 0x18, 0x1D, 0xD6, 0xB1, 0xFD, 0xDF, 0xB4, 0xC5, 0x07, 0x2A, 0xD4, 0x6D, 0x9B, 0xC8, 0xAC, 0x67, 0x2B, 0xE6, 0xD8, 0x25, 0xB2, 0x61, 0x5E, 0xEB, 0xEF, 0x7C, 0x4B, 0x50, 0x25, 0x75, 0x68, 0x35, 0x19, 0xDE, 0x02, 0xE0, 0xFE, 0x51, 0x7E, 0x6C, 0x00, 0xA8, 0xDA, 0xD1, 0x3F, 0x34, 0xC3, 0xC9, 0x13, 0x03, 0x5F, 0xF6, 0x2B, 0x4B, 0xC9, 0x31, 0x5C, 0x04, 0xC6, 0xE6, 0x5B, 0x38, 0xF8, 0x58, 0x6E, 0x58, 0x95, 0xD7, 0x5F, 0xFA, 0x6E, 0xFC, 0x4F, 0xA3, 0x6B, 0x30, 0x69, 0x30, 
			0x1D, 0x06, 0x03, 0x55, 0x1D, 0x0E, 0x04, 0x16, 0x04, 0x14, 0x00, 0x78, 0x42, 0x32, 0x52, 0x64, 0x80, 0x14, 0xEB, 0x26, 0xBA, 0x16, 0x29, 0xED, 0x65, 0x95, 0xBF, 0x2A, 0x1F, 0x1F, 0x30, 0x1F, 0x06, 0x03, 0x55, 0x1D, 0x23, 0x04, 0x18, 0x30, 0x16, 0x80, 0x14, 0x5D, 0x24, 0xEE, 0x8A, 0x55, 0x1A, 0xF2, 0xC6, 0xC9, 0xB2, 0xC2, 0xBF, 0x8A, 0xF0, 0xB2, 0x49, 0x4F, 0x3A, 0xB3, 0x1B, 0x30, 0x17, 0x06, 0x03, 0x55, 0x1D, 0x20, 0x04, 0x10, 0x30, 0x0E, 0x30, 0x0C, 0x06, 0x0A, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x02, 0x01, 0x30, 0x01, 0x30, 0x0E, 0x06, 0x03, 0x55, 0x1D, 0x0F, 0x01, 0x01, 0xFF, 0x04, 0x04, 
			0x03, 0x02, 0x06, 0xC0, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x03, 0x03, 0x30, 0x00, 0x30, 0x2D, 0x02, 0x14, 0x0D, 0x7C, 0x88, 0xAB, 0x9B, 0x29, 0x0C, 0xA9, 0x36, 0xDF, 0x0C, 0xBE, 0x17, 0x89, 0xF6, 0xCB, 0xEC, 0xA1, 0xBA, 0x60, 0x02, 0x15, 0x00, 0xCE, 0x89, 0xDE, 0x67, 0xA9, 0x89, 0xB8, 0x16, 0xA1, 0x35, 0xFB, 0x76, 0x27, 0x27, 0x8E, 0xFD, 0x80, 0xF4, 0xC5, 0xEB };

		static public byte[] InvalidDSASignatureTest6EE_crt = { 0x30, 0x82, 0x03, 0x37, 0x30, 0x82, 0x02, 0xF6, 0xA0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x01, 0x03, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x03, 0x30, 0x3A, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x0F, 0x30, 0x0D, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x06, 0x44, 0x53, 0x41, 0x20, 0x43, 0x41, 0x30, 0x1E, 0x17, 0x0D, 0x30, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 
			0x32, 0x30, 0x5A, 0x17, 0x0D, 0x31, 0x31, 0x30, 0x34, 0x31, 0x39, 0x31, 0x34, 0x35, 0x37, 0x32, 0x30, 0x5A, 0x30, 0x5E, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x55, 0x53, 0x31, 0x1A, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x11, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x73, 0x31, 0x33, 0x30, 0x31, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x2A, 0x49, 0x6E, 0x76, 0x61, 0x6C, 0x69, 0x64, 0x20, 0x44, 0x53, 0x41, 0x20, 0x53, 0x69, 0x67, 0x6E, 0x61, 0x74, 0x75, 0x72, 0x65, 0x20, 0x45, 0x45, 0x20, 0x43, 0x65, 0x72, 
			0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x65, 0x20, 0x54, 0x65, 0x73, 0x74, 0x36, 0x30, 0x82, 0x01, 0xB6, 0x30, 0x82, 0x01, 0x2B, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x01, 0x30, 0x82, 0x01, 0x1E, 0x02, 0x81, 0x81, 0x00, 0xBD, 0xB0, 0x7A, 0x1B, 0x3C, 0xB4, 0x49, 0x7D, 0x8D, 0xE6, 0xC9, 0x91, 0xCD, 0x63, 0x2A, 0xCF, 0x45, 0x68, 0x65, 0x17, 0x79, 0x25, 0x1D, 0xA9, 0x65, 0xB9, 0x50, 0xCB, 0xAF, 0x6A, 0x17, 0xB2, 0xFD, 0xC1, 0x73, 0xDE, 0x99, 0xAC, 0x11, 0x65, 0xCE, 0x27, 0xF3, 0x0F, 0x06, 0x07, 0xEE, 0x3E, 0x20, 0x31, 0xEA, 0xD8, 0xBF, 0x14, 0x7B, 0x1F, 0xB6, 0x28, 0xFF, 0x65, 0x46, 
			0xE5, 0xCD, 0x9D, 0xA5, 0x29, 0xF8, 0x2F, 0xC1, 0x08, 0x80, 0x75, 0x70, 0x70, 0x41, 0x27, 0x00, 0x36, 0x5B, 0x3A, 0xA1, 0x7B, 0x4B, 0xB2, 0xA1, 0x56, 0xFD, 0xAA, 0xF0, 0xCE, 0x42, 0xE5, 0x36, 0xEF, 0x34, 0x54, 0x77, 0x29, 0x9A, 0xAE, 0x63, 0xAA, 0x49, 0xF7, 0x3E, 0xAE, 0xE1, 0xC2, 0xF9, 0xF6, 0x89, 0x32, 0xFF, 0x99, 0x68, 0x7B, 0xF9, 0xCE, 0x34, 0x5D, 0xF1, 0x7C, 0x29, 0x64, 0x7B, 0x34, 0x64, 0xAD, 0x02, 0x15, 0x00, 0x97, 0x63, 0x08, 0x0D, 0x42, 0xCA, 0x9F, 0x17, 0xB8, 0x0D, 0x7E, 0x00, 0x3A, 0xE5, 0x05, 0x33, 0xA4, 0x2E, 0x5B, 0xC3, 0x02, 0x81, 0x80, 0x60, 0x5F, 0x6D, 0x43, 0x46, 0x3C, 0x82, 
			0xF0, 0x7E, 0x8A, 0xC2, 0x46, 0xBD, 0x3A, 0x40, 0xEB, 0x81, 0x10, 0xA7, 0x6E, 0x2D, 0xD5, 0x84, 0x66, 0x78, 0x12, 0xED, 0x19, 0x30, 0xAC, 0xAE, 0x4D, 0xD7, 0x68, 0xF1, 0x0F, 0x0D, 0xF8, 0x6E, 0xF6, 0xF3, 0x8F, 0x3A, 0xA5, 0x95, 0xD9, 0x9D, 0x29, 0x1B, 0xBC, 0x91, 0x41, 0xB6, 0x6E, 0x14, 0x95, 0xF5, 0xA1, 0x7F, 0x13, 0x3B, 0xF2, 0xA6, 0x91, 0x24, 0x54, 0x16, 0x74, 0x8F, 0x83, 0x66, 0x9D, 0x0B, 0x4E, 0xFE, 0x4B, 0xE2, 0x80, 0x22, 0xF5, 0xDA, 0x19, 0x92, 0xB9, 0xC9, 0xCD, 0xDC, 0x8A, 0xF8, 0xFB, 0x7D, 0xA9, 0xAC, 0x95, 0xF8, 0xC9, 0xCC, 0x6E, 0x10, 0x22, 0x58, 0xB1, 0xB5, 0x39, 0x2C, 0xF7, 0xC7, 
			0x89, 0xC2, 0x53, 0xF7, 0x1E, 0x68, 0xF1, 0x8C, 0xB8, 0x21, 0xCC, 0x49, 0x93, 0x37, 0xE4, 0x2F, 0xF3, 0xB7, 0x58, 0x4D, 0x4A, 0x03, 0x81, 0x84, 0x00, 0x02, 0x81, 0x80, 0x2D, 0x69, 0xAF, 0xDD, 0x30, 0x2B, 0x5B, 0x43, 0xBB, 0x60, 0x5E, 0x83, 0x20, 0x81, 0xD0, 0xCE, 0x4C, 0x39, 0xEA, 0xC3, 0xB6, 0x86, 0x13, 0x27, 0xE8, 0xEC, 0xFF, 0x93, 0x74, 0xCA, 0xB3, 0x00, 0xA6, 0xA8, 0xDD, 0x82, 0xE1, 0xD2, 0x46, 0x8A, 0xBA, 0x9F, 0x27, 0x3F, 0xDF, 0x77, 0x67, 0x80, 0x09, 0x4E, 0x2A, 0xC9, 0x98, 0xB8, 0x29, 0x68, 0xE1, 0x5F, 0x6F, 0xE6, 0xC4, 0x35, 0xAC, 0xF6, 0x18, 0x6B, 0x83, 0xED, 0xF4, 0x32, 0xF5, 0xD3, 
			0x9A, 0x02, 0xBB, 0x7B, 0x99, 0x02, 0x50, 0x12, 0x4D, 0x2A, 0xA2, 0x51, 0xCE, 0x85, 0x57, 0x52, 0x18, 0x94, 0x41, 0x32, 0x12, 0xB5, 0xE9, 0x24, 0xCE, 0x6C, 0x60, 0x51, 0x2D, 0x7A, 0xDD, 0x0B, 0xA0, 0x8A, 0xB6, 0x42, 0xF9, 0x9C, 0x6C, 0x7C, 0x77, 0x39, 0x95, 0xEB, 0x4D, 0x2E, 0xD9, 0x82, 0xF2, 0x82, 0x37, 0x03, 0x45, 0x05, 0x0D, 0xD0, 0xA3, 0x6B, 0x30, 0x69, 0x30, 0x1D, 0x06, 0x03, 0x55, 0x1D, 0x0E, 0x04, 0x16, 0x04, 0x14, 0xDC, 0x66, 0x69, 0x39, 0x72, 0xA7, 0xB0, 0x3A, 0xDF, 0x7D, 0x1B, 0x13, 0xD3, 0x30, 0xBF, 0x4E, 0xB4, 0x7E, 0xA0, 0xF1, 0x30, 0x1F, 0x06, 0x03, 0x55, 0x1D, 0x23, 0x04, 0x18, 
			0x30, 0x16, 0x80, 0x14, 0x74, 0x15, 0xD5, 0x24, 0x1C, 0xBD, 0x5E, 0x65, 0x88, 0x1F, 0xE1, 0x8B, 0x09, 0x7E, 0x7F, 0xEA, 0x19, 0x48, 0x4E, 0x61, 0x30, 0x17, 0x06, 0x03, 0x55, 0x1D, 0x20, 0x04, 0x10, 0x30, 0x0E, 0x30, 0x0C, 0x06, 0x0A, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x02, 0x01, 0x30, 0x01, 0x30, 0x0E, 0x06, 0x03, 0x55, 0x1D, 0x0F, 0x01, 0x01, 0xFF, 0x04, 0x04, 0x03, 0x02, 0x06, 0xC0, 0x30, 0x09, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x38, 0x04, 0x03, 0x03, 0x30, 0x00, 0x30, 0x2D, 0x02, 0x15, 0x00, 0xBC, 0x8D, 0x6E, 0xA3, 0x26, 0xD2, 0xC5, 0x39, 0x56, 0xFF, 0xDE, 0x00, 0x7C, 0x51, 0xDE, 0xB3, 
			0x0C, 0x00, 0xC2, 0xD4, 0x02, 0x14, 0x5E, 0x08, 0x09, 0x4B, 0xEC, 0xE7, 0x4D, 0x07, 0xEA, 0xCA, 0xE3, 0x1E, 0xF9, 0x21, 0xE7, 0x76, 0x39, 0xD5, 0xDC, 0x6C };


		[Test]
		public void DSACA ()
		{
			X509Certificate ca = new X509Certificate (DSACACert_crt);
			Assert.AreEqual ("<DSAKeyValue><P>3+URPtrptm4Q1uqd4p06sEe9RADHVsjMbtAzhFZHNT32VMjjwq27unXzLzMMpvkx7Gfj5Zlt/CluqleIcjTijgCQ4KOsZI7A9jwdj7TISkgwXn+qnHYmC9sTczODl8DFs+Y39T7/FQ3UoS66Mfirh9gLzHeYQm6sk5jCvS57NAs=</P><Q>zwYE2P+L6wDp9lwHlnP9lmU6Lwc=</Q><G>zOF8sM6SX2PsOLtEut2SNLZevmV72HF3BJ3sZnw7BM6281L+D5JVAu9OEqtdmi4vblbzcOxq7ZsiuKgTywycFurBCo4hJkSlDPmg7GLgcDHMaPULhaRKG2559MH5Nlo4b07vhFPfZ/3M91lij5yczRCPXKQPnLcH7GDzvq9+OZg=</G><Y>EfK52L5CK8WEvpECHPyMMnKLqGwh14iKFLowZXXAHD2CaWWnrJB6FB2Fe+VTwmD8sc9nr8HyLggyajjHkU47vDwJ0PlxbQjfJ0mNBXTYvUbQ21GlU7qH8/pdJYNPfwp15anjiadBd2NAXysshNLBcXgK22tXGeHnFAyexPYyOQ4=</Y></DSAKeyValue>", ca.DSA.ToXmlString (false), "DSA");
			Assert.AreEqual (5, ca.Extensions.Count, "Extensions");
			Assert.AreEqual ("20-98-19-23-3C-FC-D9-C9-02-BA-BE-C6-42-BF-87-15-54-A1-7A-39", BitConverter.ToString (ca.Hash), "Hash");
			Assert.IsTrue (ca.IsCurrent, "IsCurrent"); // true until 2011
			Assert.IsFalse (ca.IsSelfSigned, "IsSelfSigned");
			Assert.AreEqual ("C=US, O=Test Certificates, CN=Trust Anchor", ca.IssuerName, "IssuerName");
			Assert.AreEqual ("1.2.840.10040.4.1", ca.KeyAlgorithm, "KeyAlgorithm");
			Assert.AreEqual ("30-82-01-1F-02-81-81-00-DF-E5-11-3E-DA-E9-B6-6E-10-D6-EA-9D-E2-9D-3A-B0-47-BD-44-00-C7-56-C8-CC-6E-D0-33-84-56-47-35-3D-F6-54-C8-E3-C2-AD-BB-BA-75-F3-2F-33-0C-A6-F9-31-EC-67-E3-E5-99-6D-FC-29-6E-AA-57-88-72-34-E2-8E-00-90-E0-A3-AC-64-8E-C0-F6-3C-1D-8F-B4-C8-4A-48-30-5E-7F-AA-9C-76-26-0B-DB-13-73-33-83-97-C0-C5-B3-E6-37-F5-3E-FF-15-0D-D4-A1-2E-BA-31-F8-AB-87-D8-0B-CC-77-98-42-6E-AC-93-98-C2-BD-2E-7B-34-0B-02-15-00-CF-06-04-D8-FF-8B-EB-00-E9-F6-5C-07-96-73-FD-96-65-3A-2F-07-02-81-81-00-CC-E1-7C-B0-CE-92-5F-63-EC-38-BB-44-BA-DD-92-34-B6-5E-BE-65-7B-D8-71-77-04-9D-EC-66-7C-3B-04-CE-B6-F3-52-FE-0F-92-55-02-EF-4E-12-AB-5D-9A-2E-2F-6E-56-F3-70-EC-6A-ED-9B-22-B8-A8-13-CB-0C-9C-16-EA-C1-0A-8E-21-26-44-A5-0C-F9-A0-EC-62-E0-70-31-CC-68-F5-0B-85-A4-4A-1B-6E-79-F4-C1-F9-36-5A-38-6F-4E-EF-84-53-DF-67-FD-CC-F7-59-62-8F-9C-9C-CD-10-8F-5C-A4-0F-9C-B7-07-EC-60-F3-BE-AF-7E-39-98", BitConverter.ToString (ca.KeyAlgorithmParameters), "KeyAlgorithmParameters");
			Assert.AreEqual ("02-81-80-11-F2-B9-D8-BE-42-2B-C5-84-BE-91-02-1C-FC-8C-32-72-8B-A8-6C-21-D7-88-8A-14-BA-30-65-75-C0-1C-3D-82-69-65-A7-AC-90-7A-14-1D-85-7B-E5-53-C2-60-FC-B1-CF-67-AF-C1-F2-2E-08-32-6A-38-C7-91-4E-3B-BC-3C-09-D0-F9-71-6D-08-DF-27-49-8D-05-74-D8-BD-46-D0-DB-51-A5-53-BA-87-F3-FA-5D-25-83-4F-7F-0A-75-E5-A9-E3-89-A7-41-77-63-40-5F-2B-2C-84-D2-C1-71-78-0A-DB-6B-57-19-E1-E7-14-0C-9E-C4-F6-32-39-0E", BitConverter.ToString (ca.PublicKey), "PublicKey");
			Assert.AreEqual (DSACACert_crt, ca.RawData, "RawData");
			Assert.IsNull (ca.RSA, "RSA");
			Assert.AreEqual ("D1-07", BitConverter.ToString (ca.SerialNumber), "SerialNumber");
			Assert.AreEqual ("3A-3B-72-1F-77-2C-BA-DC-EC-F5-1D-5B-45-CE-3F-7F-A4-3F-E4-B2-C1-FA-9F-D8-0A-5C-98-E2-EE-7E-63-3A-D2-C8-B0-E2-BC-C0-D6-CB-28-21-30-76-46-CD-D3-3D-02-81-88-9C-CC-74-52-AB-AB-CB-50-FB-C4-DC-A5-72-7D-33-84-95-7D-B0-05-07-43-F4-BC-1E-14-0B-61-20-EA-24-A7-54-96-C1-B6-C6-45-8F-5D-A0-A6-AB-F9-19-AC-28-DF-25-13-C3-7E-21-DE-8A-43-19-25-F5-A7-3E-9E-65-42-19-04-52-A9-01-7E-60-C8-8A-62-10-12-36", BitConverter.ToString (ca.Signature), "Signature");
			Assert.AreEqual ("1.2.840.113549.1.1.5", ca.SignatureAlgorithm, "SignatureAlgorithm");
			Assert.AreEqual ("05-00", BitConverter.ToString (ca.SignatureAlgorithmParameters), "SignatureAlgorithmParameters");
			Assert.AreEqual ("C=US, O=Test Certificates, CN=DSA CA", ca.SubjectName, "SubjectName");
			Assert.AreEqual (631232890400000000, ca.ValidFrom.ToUniversalTime ().Ticks, "ValidFrom");
			Assert.AreEqual (634388218400000000, ca.ValidUntil.ToUniversalTime ().Ticks, "ValidUntil");
			Assert.AreEqual (3, ca.Version, "Version");
		}

		[Test]
		public void DSAWithoutParameters ()
		{
			X509Certificate ca = new X509Certificate (DSAParametersInheritedCACert_crt);
			Assert.AreEqual (5, ca.Extensions.Count, "Extensions");
			Assert.AreEqual ("F5-F0-0D-21-1E-87-B9-F6-E1-85-AB-04-5F-43-2A-FD-EA-96-BC-D9", BitConverter.ToString (ca.Hash), "Hash");
			Assert.IsTrue (ca.IsCurrent, "IsCurrent"); // true until 2011
			Assert.IsFalse (ca.IsSelfSigned, "IsSelfSigned");
			Assert.AreEqual ("C=US, O=Test Certificates, CN=DSA CA", ca.IssuerName, "IssuerName");
			Assert.AreEqual ("1.2.840.10040.4.1", ca.KeyAlgorithm, "KeyAlgorithm");
			Assert.IsNull (ca.KeyAlgorithmParameters, "KeyAlgorithmParameters");
			Assert.AreEqual ("02-81-80-67-8C-47-DA-0C-36-81-64-39-F8-1A-37-0B-F0-BB-D4-13-FD-67-3D-40-C8-9C-92-E1-3A-89-54-B0-F2-97-A5-70-D0-64-17-A4-A7-7C-A9-E9-27-09-65-DF-A0-A5-1C-FC-04-DA-3E-DD-62-6B-F0-0C-ED-81-57-4F-5D-29-C8-08-59-19-0C-5F-8D-88-F0-FF-8C-B6-E8-75-01-BE-10-0A-64-DF-57-7B-8B-6C-56-F0-53-CE-43-88-B9-09-99-08-3C-90-AF-01-E8-D9-32-5D-ED-56-02-CF-60-D2-01-3A-E3-0A-2B-9E-91-41-F6-C7-7C-DE-6C-99-63-00-0E", BitConverter.ToString (ca.PublicKey), "PublicKey");
			Assert.AreEqual (DSAParametersInheritedCACert_crt, ca.RawData, "RawData");
			Assert.IsNull (ca.RSA, "RSA");
			Assert.AreEqual ("02", BitConverter.ToString (ca.SerialNumber), "SerialNumber");
			Assert.AreEqual ("A8-59-6F-31-77-B6-20-EC-36-9B-EB-4B-61-0A-AF-44-ED-72-BA-29-6D-22-E1-BD-4D-27-F6-2E-3B-1F-D7-9D-D6-59-5E-CB-25-86-22-D8", BitConverter.ToString (ca.Signature), "Signature");
			Assert.AreEqual ("1.2.840.10040.4.3", ca.SignatureAlgorithm, "SignatureAlgorithm");
			Assert.IsNull (ca.SignatureAlgorithmParameters, "SignatureAlgorithmParameters");
			Assert.AreEqual ("C=US, O=Test Certificates, CN=DSA Parameters Inherited CA", ca.SubjectName, "SubjectName");
			Assert.AreEqual (631232890400000000, ca.ValidFrom.ToUniversalTime ().Ticks, "ValidFrom");
			Assert.AreEqual (634388218400000000, ca.ValidUntil.ToUniversalTime ().Ticks, "ValidUntil");
			Assert.AreEqual (3, ca.Version, "Version");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void DSAWithoutParameters_DSA ()
		{
			new X509Certificate (DSAParametersInheritedCACert_crt).DSA.ToXmlString (false);
		}

		[Test]
		public void InheritedDSAParameters ()
		{
			X509Certificate ca = new X509Certificate (DSACACert_crt);
			X509Certificate subca = new X509Certificate (DSAParametersInheritedCACert_crt);
			subca.KeyAlgorithmParameters = ca.KeyAlgorithmParameters;
			Assert.AreEqual ("<DSAKeyValue><P>3+URPtrptm4Q1uqd4p06sEe9RADHVsjMbtAzhFZHNT32VMjjwq27unXzLzMMpvkx7Gfj5Zlt/CluqleIcjTijgCQ4KOsZI7A9jwdj7TISkgwXn+qnHYmC9sTczODl8DFs+Y39T7/FQ3UoS66Mfirh9gLzHeYQm6sk5jCvS57NAs=</P><Q>zwYE2P+L6wDp9lwHlnP9lmU6Lwc=</Q><G>zOF8sM6SX2PsOLtEut2SNLZevmV72HF3BJ3sZnw7BM6281L+D5JVAu9OEqtdmi4vblbzcOxq7ZsiuKgTywycFurBCo4hJkSlDPmg7GLgcDHMaPULhaRKG2559MH5Nlo4b07vhFPfZ/3M91lij5yczRCPXKQPnLcH7GDzvq9+OZg=</G><Y>Z4xH2gw2gWQ5+Bo3C/C71BP9Zz1AyJyS4TqJVLDyl6Vw0GQXpKd8qeknCWXfoKUc/ATaPt1ia/AM7YFXT10pyAhZGQxfjYjw/4y26HUBvhAKZN9Xe4tsVvBTzkOIuQmZCDyQrwHo2TJd7VYCz2DSATrjCiuekUH2x3zebJljAA4=</Y></DSAKeyValue>", subca.DSA.ToXmlString (false), "DSA");
			Assert.IsTrue (subca.VerifySignature (ca.DSA), "CA signed SubCA");

			X509Certificate ee = new X509Certificate (ValidDSAParameterInheritanceTest5EE_crt);
			ee.KeyAlgorithmParameters = subca.KeyAlgorithmParameters;
			Assert.AreEqual ("<DSAKeyValue><P>3+URPtrptm4Q1uqd4p06sEe9RADHVsjMbtAzhFZHNT32VMjjwq27unXzLzMMpvkx7Gfj5Zlt/CluqleIcjTijgCQ4KOsZI7A9jwdj7TISkgwXn+qnHYmC9sTczODl8DFs+Y39T7/FQ3UoS66Mfirh9gLzHeYQm6sk5jCvS57NAs=</P><Q>zwYE2P+L6wDp9lwHlnP9lmU6Lwc=</Q><G>zOF8sM6SX2PsOLtEut2SNLZevmV72HF3BJ3sZnw7BM6281L+D5JVAu9OEqtdmi4vblbzcOxq7ZsiuKgTywycFurBCo4hJkSlDPmg7GLgcDHMaPULhaRKG2559MH5Nlo4b07vhFPfZ/3M91lij5yczRCPXKQPnLcH7GDzvq9+OZg=</G><Y>zos22FxEQcfqmrDYlDlFs0m0bGa91p2w64m+flvQ2zMhgxtKkmrXdtKjyEvHm5V8S+QZ1zQGmhgd1rH937TFByrUbZvIrGcr5tglsmFe6+98S1AldWg1Gd4C4P5RfmwAqNrRPzTDyRMDX/YrS8kxXATG5ls4+FhuWJXXX/pu/E8=</Y></DSAKeyValue>", ee.DSA.ToXmlString (false), "DSA");
			Assert.IsTrue (ee.VerifySignature (subca.DSA), "SubCA signed EE");
		}

		[Test]
		public void VerifyDSASignature ()
		{
			X509Certificate ca = new X509Certificate (DSACACert_crt);
			// note: the DSA signature has 41 bytes because part1 first byte would be 
			// negative (bad for ASN.1) so a 0x00 was prepended
			X509Certificate signed = new X509Certificate (ValidDSASignaturesTest4EE_crt);
			Assert.IsTrue (signed.VerifySignature (ca.DSA), "VerifySignature(dsa)");
		}

		[Test]
		public void VerifyDSASignature_Bad ()
		{
			X509Certificate ca = new X509Certificate (DSACACert_crt);
			X509Certificate signed = new X509Certificate (InvalidDSASignatureTest6EE_crt);
			Assert.IsFalse (signed.VerifySignature (ca.DSA), "VerifySignature(dsa)");
		}
	}
}
