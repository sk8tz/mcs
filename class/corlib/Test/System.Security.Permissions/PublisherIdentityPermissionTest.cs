//
// PublisherIdentityPermissionTest.cs - NUnit Test Cases for PublisherIdentityPermission
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class PublisherIdentityPermissionTest : Assertion {

		private static string className = "System.Security.Permissions.PublisherIdentityPermission, ";

		private static byte[] cert = { 0x30,0x82,0x05,0x0F,0x30,0x82,0x03,0xF7,0xA0,0x03,0x02,0x01,0x02,0x02,0x0A,0x61,0x07,0x11,0x43,0x00,0x00,0x00,0x00,0x00,0x34,0x30,0x0D,0x06,0x09,0x2A,0x86,0x48,0x86,0xF7,0x0D,0x01,0x01,0x05,0x05,0x00,0x30,0x81,0xA6,0x31,0x0B,0x30,0x09,0x06,0x03,0x55,0x04,0x06,0x13,0x02,0x55,0x53,0x31,0x13,0x30,0x11,0x06,0x03,0x55,0x04,0x08,0x13,0x0A,0x57,0x61,0x73,0x68,0x69,0x6E,0x67,0x74,0x6F,0x6E,0x31,0x10,0x30,0x0E,0x06,0x03,0x55,0x04,0x07,0x13,0x07,0x52,0x65,0x64,0x6D,0x6F,0x6E,0x64,0x31,0x1E,0x30,0x1C,0x06,0x03,
			0x55,0x04,0x0A,0x13,0x15,0x4D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x20,0x43,0x6F,0x72,0x70,0x6F,0x72,0x61,0x74,0x69,0x6F,0x6E,0x31,0x2B,0x30,0x29,0x06,0x03,0x55,0x04,0x0B,0x13,0x22,0x43,0x6F,0x70,0x79,0x72,0x69,0x67,0x68,0x74,0x20,0x28,0x63,0x29,0x20,0x32,0x30,0x30,0x30,0x20,0x4D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x20,0x43,0x6F,0x72,0x70,0x2E,0x31,0x23,0x30,0x21,0x06,0x03,0x55,0x04,0x03,0x13,0x1A,0x4D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x20,0x43,0x6F,0x64,0x65,0x20,0x53,0x69,0x67,
			0x6E,0x69,0x6E,0x67,0x20,0x50,0x43,0x41,0x30,0x1E,0x17,0x0D,0x30,0x32,0x30,0x35,0x32,0x35,0x30,0x30,0x35,0x35,0x34,0x38,0x5A,0x17,0x0D,0x30,0x33,0x31,0x31,0x32,0x35,0x30,0x31,0x30,0x35,0x34,0x38,0x5A,0x30,0x81,0xA1,0x31,0x0B,0x30,0x09,0x06,0x03,0x55,0x04,0x06,0x13,0x02,0x55,0x53,0x31,0x13,0x30,0x11,0x06,0x03,0x55,0x04,0x08,0x13,0x0A,0x57,0x61,0x73,0x68,0x69,0x6E,0x67,0x74,0x6F,0x6E,0x31,0x10,0x30,0x0E,0x06,0x03,0x55,0x04,0x07,0x13,0x07,0x52,0x65,0x64,0x6D,0x6F,0x6E,0x64,0x31,0x1E,0x30,0x1C,0x06,
			0x03,0x55,0x04,0x0A,0x13,0x15,0x4D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x20,0x43,0x6F,0x72,0x70,0x6F,0x72,0x61,0x74,0x69,0x6F,0x6E,0x31,0x2B,0x30,0x29,0x06,0x03,0x55,0x04,0x0B,0x13,0x22,0x43,0x6F,0x70,0x79,0x72,0x69,0x67,0x68,0x74,0x20,0x28,0x63,0x29,0x20,0x32,0x30,0x30,0x32,0x20,0x4D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x20,0x43,0x6F,0x72,0x70,0x2E,0x31,0x1E,0x30,0x1C,0x06,0x03,0x55,0x04,0x03,0x13,0x15,0x4D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x20,0x43,0x6F,0x72,0x70,0x6F,0x72,0x61,
			0x74,0x69,0x6F,0x6E,0x30,0x82,0x01,0x22,0x30,0x0D,0x06,0x09,0x2A,0x86,0x48,0x86,0xF7,0x0D,0x01,0x01,0x01,0x05,0x00,0x03,0x82,0x01,0x0F,0x00,0x30,0x82,0x01,0x0A,0x02,0x82,0x01,0x01,0x00,0xAA,0x99,0xBD,0x39,0xA8,0x18,0x27,0xF4,0x2B,0x3D,0x0B,0x4C,0x3F,0x7C,0x77,0x2E,0xA7,0xCB,0xB5,0xD1,0x8C,0x0D,0xC2,0x3A,0x74,0xD7,0x93,0xB5,0xE0,0xA0,0x4B,0x3F,0x59,0x5E,0xCE,0x45,0x4F,0x9A,0x79,0x29,0xF1,0x49,0xCC,0x1A,0x47,0xEE,0x55,0xC2,0x08,0x3E,0x12,0x20,0xF8,0x55,0xF2,0xEE,0x5F,0xD3,0xE0,0xCA,0x96,0xBC,0x30,
			0xDE,0xFE,0x58,0xC8,0x27,0x32,0xD0,0x85,0x54,0xE8,0xF0,0x91,0x10,0xBB,0xF3,0x2B,0xBE,0x19,0xE5,0x03,0x9B,0x0B,0x86,0x1D,0xF3,0xB0,0x39,0x8C,0xB8,0xFD,0x0B,0x1D,0x3C,0x73,0x26,0xAC,0x57,0x2B,0xCA,0x29,0xA2,0x15,0x90,0x82,0x15,0xE2,0x77,0xA3,0x40,0x52,0x03,0x8B,0x9D,0xC2,0x70,0xBA,0x1F,0xE9,0x34,0xF6,0xF3,0x35,0x92,0x4E,0x55,0x83,0xF8,0xDA,0x30,0xB6,0x20,0xDE,0x57,0x06,0xB5,0x5A,0x42,0x06,0xDE,0x59,0xCB,0xF2,0xDF,0xA6,0xBD,0x15,0x47,0x71,0x19,0x25,0x23,0xD2,0xCB,0x6F,0x9B,0x19,0x79,0xDF,0x6A,0x5B,
			0xF1,0x76,0x05,0x79,0x29,0xFC,0xC3,0x56,0xCA,0x8F,0x44,0x08,0x85,0x55,0x8A,0xCB,0xC8,0x0F,0x46,0x4B,0x55,0xCB,0x8C,0x96,0x77,0x4A,0x87,0xE8,0xA9,0x41,0x06,0xC7,0xFF,0x0D,0xE9,0x68,0x57,0x63,0x72,0xC3,0x69,0x57,0xB4,0x43,0xCF,0x32,0x3A,0x30,0xDC,0x1B,0xE9,0xD5,0x43,0x26,0x2A,0x79,0xFE,0x95,0xDB,0x22,0x67,0x24,0xC9,0x2F,0xD0,0x34,0xE3,0xE6,0xFB,0x51,0x49,0x86,0xB8,0x3C,0xD0,0x25,0x5F,0xD6,0xEC,0x9E,0x03,0x61,0x87,0xA9,0x68,0x40,0xC7,0xF8,0xE2,0x03,0xE6,0xCF,0x05,0x02,0x03,0x01,0x00,0x01,0xA3,0x82,
			0x01,0x40,0x30,0x82,0x01,0x3C,0x30,0x0E,0x06,0x03,0x55,0x1D,0x0F,0x01,0x01,0xFF,0x04,0x04,0x03,0x02,0x06,0xC0,0x30,0x13,0x06,0x03,0x55,0x1D,0x25,0x04,0x0C,0x30,0x0A,0x06,0x08,0x2B,0x06,0x01,0x05,0x05,0x07,0x03,0x03,0x30,0x1D,0x06,0x03,0x55,0x1D,0x0E,0x04,0x16,0x04,0x14,0x6B,0xC8,0xC6,0x51,0x20,0xF0,0xB4,0x2F,0xD3,0xA0,0xB6,0xAE,0x7F,0x5E,0x26,0xB2,0xB8,0x87,0x52,0x29,0x30,0x81,0xA9,0x06,0x03,0x55,0x1D,0x23,0x04,0x81,0xA1,0x30,0x81,0x9E,0x80,0x14,0x29,0x5C,0xB9,0x1B,0xB6,0xCD,0x33,0xEE,0xBB,0x9E,
			0x59,0x7D,0xF7,0xE5,0xCA,0x2E,0xC4,0x0D,0x34,0x28,0xA1,0x74,0xA4,0x72,0x30,0x70,0x31,0x2B,0x30,0x29,0x06,0x03,0x55,0x04,0x0B,0x13,0x22,0x43,0x6F,0x70,0x79,0x72,0x69,0x67,0x68,0x74,0x20,0x28,0x63,0x29,0x20,0x31,0x39,0x39,0x37,0x20,0x4D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x20,0x43,0x6F,0x72,0x70,0x2E,0x31,0x1E,0x30,0x1C,0x06,0x03,0x55,0x04,0x0B,0x13,0x15,0x4D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x20,0x43,0x6F,0x72,0x70,0x6F,0x72,0x61,0x74,0x69,0x6F,0x6E,0x31,0x21,0x30,0x1F,0x06,0x03,0x55,
			0x04,0x03,0x13,0x18,0x4D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x20,0x52,0x6F,0x6F,0x74,0x20,0x41,0x75,0x74,0x68,0x6F,0x72,0x69,0x74,0x79,0x82,0x10,0x6A,0x0B,0x99,0x4F,0xC0,0x00,0xDE,0xAA,0x11,0xD4,0xD8,0x40,0x9A,0xA8,0xBE,0xE6,0x30,0x4A,0x06,0x03,0x55,0x1D,0x1F,0x04,0x43,0x30,0x41,0x30,0x3F,0xA0,0x3D,0xA0,0x3B,0x86,0x39,0x68,0x74,0x74,0x70,0x3A,0x2F,0x2F,0x63,0x72,0x6C,0x2E,0x6D,0x69,0x63,0x72,0x6F,0x73,0x6F,0x66,0x74,0x2E,0x63,0x6F,0x6D,0x2F,0x70,0x6B,0x69,0x2F,0x63,0x72,0x6C,0x2F,0x70,0x72,
			0x6F,0x64,0x75,0x63,0x74,0x73,0x2F,0x43,0x6F,0x64,0x65,0x53,0x69,0x67,0x6E,0x50,0x43,0x41,0x2E,0x63,0x72,0x6C,0x30,0x0D,0x06,0x09,0x2A,0x86,0x48,0x86,0xF7,0x0D,0x01,0x01,0x05,0x05,0x00,0x03,0x82,0x01,0x01,0x00,0x35,0x23,0xFD,0x13,0x54,0xFC,0xE9,0xDC,0xF0,0xDD,0x0C,0x14,0x7A,0xFA,0xA7,0xB3,0xCE,0xFD,0xA7,0x3A,0xC8,0xBA,0xE5,0xE7,0xF6,0x03,0xFB,0x53,0xDB,0xA7,0x99,0xA9,0xA0,0x9B,0x36,0x9C,0x03,0xEB,0x82,0x47,0x1C,0x21,0xBD,0x14,0xCB,0xE7,0x67,0x40,0x09,0xC7,0x16,0x91,0x02,0x55,0xCE,0x43,0x42,0xB4,
			0xCD,0x1B,0x5D,0xB0,0xF3,0x32,0x04,0x3D,0x12,0xE5,0x1D,0xA7,0x07,0xA7,0x8F,0xA3,0x7E,0x45,0x55,0x76,0x1B,0x96,0x95,0x91,0x69,0xF0,0xDD,0x38,0xF3,0x48,0x89,0xEF,0x70,0x40,0xB7,0xDB,0xB5,0x55,0x80,0xC0,0x03,0xC4,0x2E,0xB6,0x28,0xDC,0x0A,0x82,0x0E,0xC7,0x43,0xE3,0x7A,0x48,0x5D,0xB8,0x06,0x89,0x92,0x40,0x6C,0x6E,0xC5,0xDC,0xF8,0x9A,0xEF,0x0B,0xBE,0x21,0x0A,0x8C,0x2F,0x3A,0xB5,0xED,0xA7,0xCE,0x71,0x87,0x68,0x23,0xE1,0xB3,0xE4,0x18,0x7D,0xB8,0x47,0x01,0xA5,0x2B,0xC4,0x58,0xCB,0xB2,0x89,0x6C,0x5F,0xFD,
			0xD3,0x2C,0xC4,0x6F,0xB8,0x23,0xB2,0x0D,0xFF,0x3C,0xF2,0x11,0x45,0x74,0xF2,0x09,0x06,0x99,0x18,0xDD,0x6F,0xC0,0x86,0x01,0x18,0x12,0x1D,0x2B,0x16,0xAF,0x56,0xEF,0x65,0x33,0xA1,0xEA,0x67,0x4E,0xF4,0x4B,0x82,0xAB,0xE9,0x0F,0xDC,0x01,0xFA,0xDF,0x60,0x7F,0x66,0x47,0x5D,0xCB,0x2C,0x70,0xCC,0x7B,0x4E,0xD9,0x06,0xB8,0x6E,0x8C,0x0C,0xFE,0x62,0x1E,0x42,0xF9,0x93,0x7C,0xA2,0xAB,0x0A,0x9E,0xD0,0x23,0x10,0xAE,0x4D,0x7B,0x27,0x91,0x6F,0x26,0xBE,0x68,0xFA,0xA6,0x3F,0x9F,0x23,0xEB,0xC8,0x9D,0xBB,0x87 };

		private static byte[] cert2 = { 0x30,0x82,0x09,0xB9,0x30,0x82,0x09,0x22,0xA0,0x03,0x02,0x01,0x02,0x02,0x10,0x20,0x0B,0x35,0x5E,0xCE,0xC4,0xB0,0x63,0xB7,0xDE,0xC6,0x34,0xB9,0x70,0x34,0x44,0x30,0x0D,0x06,0x09,0x2A,0x86,0x48,0x86,0xF7,0x0D,0x01,0x01,0x04,0x05,0x00,0x30,0x62,0x31,0x11,0x30,0x0F,0x06,0x03,0x55,0x04,0x07,0x13,0x08,0x49,0x6E,0x74,0x65,0x72,0x6E,0x65,0x74,0x31,0x17,0x30,0x15,0x06,0x03,0x55,0x04,0x0A,0x13,0x0E,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x2C,0x20,0x49,0x6E,0x63,0x2E,0x31,0x34,0x30,0x32,0x06,0x03,0x55,0x04,0x0B,
			0x13,0x2B,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x20,0x43,0x6C,0x61,0x73,0x73,0x20,0x31,0x20,0x43,0x41,0x20,0x2D,0x20,0x49,0x6E,0x64,0x69,0x76,0x69,0x64,0x75,0x61,0x6C,0x20,0x53,0x75,0x62,0x73,0x63,0x72,0x69,0x62,0x65,0x72,0x30,0x1E,0x17,0x0D,0x39,0x36,0x30,0x38,0x32,0x31,0x30,0x30,0x30,0x30,0x30,0x30,0x5A,0x17,0x0D,0x39,0x37,0x30,0x38,0x32,0x30,0x32,0x33,0x35,0x39,0x35,0x39,0x5A,0x30,0x82,0x01,0x0A,0x31,0x11,0x30,0x0F,0x06,0x03,0x55,0x04,0x07,0x13,0x08,0x49,0x6E,0x74,0x65,0x72,0x6E,0x65,0x74,
			0x31,0x17,0x30,0x15,0x06,0x03,0x55,0x04,0x0A,0x13,0x0E,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x2C,0x20,0x49,0x6E,0x63,0x2E,0x31,0x34,0x30,0x32,0x06,0x03,0x55,0x04,0x0B,0x13,0x2B,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x20,0x43,0x6C,0x61,0x73,0x73,0x20,0x31,0x20,0x43,0x41,0x20,0x2D,0x20,0x49,0x6E,0x64,0x69,0x76,0x69,0x64,0x75,0x61,0x6C,0x20,0x53,0x75,0x62,0x73,0x63,0x72,0x69,0x62,0x65,0x72,0x31,0x46,0x30,0x44,0x06,0x03,0x55,0x04,0x0B,0x13,0x3D,0x77,0x77,0x77,0x2E,0x76,0x65,0x72,0x69,0x73,0x69,
			0x67,0x6E,0x2E,0x63,0x6F,0x6D,0x2F,0x72,0x65,0x70,0x6F,0x73,0x69,0x74,0x6F,0x72,0x79,0x2F,0x43,0x50,0x53,0x20,0x49,0x6E,0x63,0x6F,0x72,0x70,0x2E,0x20,0x62,0x79,0x20,0x52,0x65,0x66,0x2E,0x2C,0x4C,0x49,0x41,0x42,0x2E,0x4C,0x54,0x44,0x28,0x63,0x29,0x39,0x36,0x31,0x26,0x30,0x24,0x06,0x03,0x55,0x04,0x0B,0x13,0x1D,0x44,0x69,0x67,0x69,0x74,0x61,0x6C,0x20,0x49,0x44,0x20,0x43,0x6C,0x61,0x73,0x73,0x20,0x31,0x20,0x2D,0x20,0x4E,0x65,0x74,0x73,0x63,0x61,0x70,0x65,0x31,0x16,0x30,0x14,0x06,0x03,0x55,0x04,0x03,
			0x13,0x0D,0x44,0x61,0x76,0x69,0x64,0x20,0x54,0x2E,0x20,0x47,0x72,0x61,0x79,0x31,0x1E,0x30,0x1C,0x06,0x09,0x2A,0x86,0x48,0x86,0xF7,0x0D,0x01,0x09,0x01,0x16,0x0F,0x64,0x61,0x76,0x69,0x64,0x40,0x66,0x6F,0x72,0x6D,0x61,0x6C,0x2E,0x69,0x65,0x30,0x5C,0x30,0x0D,0x06,0x09,0x2A,0x86,0x48,0x86,0xF7,0x0D,0x01,0x01,0x01,0x05,0x00,0x03,0x4B,0x00,0x30,0x48,0x02,0x41,0x00,0xC5,0x81,0x07,0xA2,0xEB,0x0F,0xB8,0xFF,0xF8,0xF8,0x1C,0xEE,0x32,0xFF,0xBF,0x12,0x35,0x6A,0xF9,0x6B,0xC8,0xBE,0x2F,0xFB,0x3E,0xAF,0x04,0x51,
			0x4A,0xAC,0xDD,0x10,0x29,0xA8,0xCD,0x40,0x5B,0x66,0x1E,0x98,0xEF,0xF2,0x4C,0x77,0xFA,0x8F,0x86,0xD1,0x21,0x67,0x92,0x44,0x4A,0xC4,0x89,0xC9,0x83,0xCF,0x88,0x9F,0x6F,0xE2,0x32,0x35,0x02,0x03,0x01,0x00,0x01,0xA3,0x82,0x07,0x08,0x30,0x82,0x07,0x04,0x30,0x09,0x06,0x03,0x55,0x1D,0x13,0x04,0x02,0x30,0x00,0x30,0x82,0x02,0x1F,0x06,0x03,0x55,0x1D,0x03,0x04,0x82,0x02,0x16,0x30,0x82,0x02,0x12,0x30,0x82,0x02,0x0E,0x30,0x82,0x02,0x0A,0x06,0x0B,0x60,0x86,0x48,0x01,0x86,0xF8,0x45,0x01,0x07,0x01,0x01,0x30,0x82,
			0x01,0xF9,0x16,0x82,0x01,0xA7,0x54,0x68,0x69,0x73,0x20,0x63,0x65,0x72,0x74,0x69,0x66,0x69,0x63,0x61,0x74,0x65,0x20,0x69,0x6E,0x63,0x6F,0x72,0x70,0x6F,0x72,0x61,0x74,0x65,0x73,0x20,0x62,0x79,0x20,0x72,0x65,0x66,0x65,0x72,0x65,0x6E,0x63,0x65,0x2C,0x20,0x61,0x6E,0x64,0x20,0x69,0x74,0x73,0x20,0x75,0x73,0x65,0x20,0x69,0x73,0x20,0x73,0x74,0x72,0x69,0x63,0x74,0x6C,0x79,0x20,0x73,0x75,0x62,0x6A,0x65,0x63,0x74,0x20,0x74,0x6F,0x2C,0x20,0x74,0x68,0x65,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x20,0x43,
			0x65,0x72,0x74,0x69,0x66,0x69,0x63,0x61,0x74,0x69,0x6F,0x6E,0x20,0x50,0x72,0x61,0x63,0x74,0x69,0x63,0x65,0x20,0x53,0x74,0x61,0x74,0x65,0x6D,0x65,0x6E,0x74,0x20,0x28,0x43,0x50,0x53,0x29,0x2C,0x20,0x61,0x76,0x61,0x69,0x6C,0x61,0x62,0x6C,0x65,0x20,0x61,0x74,0x3A,0x20,0x68,0x74,0x74,0x70,0x73,0x3A,0x2F,0x2F,0x77,0x77,0x77,0x2E,0x76,0x65,0x72,0x69,0x73,0x69,0x67,0x6E,0x2E,0x63,0x6F,0x6D,0x2F,0x43,0x50,0x53,0x3B,0x20,0x62,0x79,0x20,0x45,0x2D,0x6D,0x61,0x69,0x6C,0x20,0x61,0x74,0x20,0x43,0x50,0x53,0x2D,
			0x72,0x65,0x71,0x75,0x65,0x73,0x74,0x73,0x40,0x76,0x65,0x72,0x69,0x73,0x69,0x67,0x6E,0x2E,0x63,0x6F,0x6D,0x3B,0x20,0x6F,0x72,0x20,0x62,0x79,0x20,0x6D,0x61,0x69,0x6C,0x20,0x61,0x74,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x2C,0x20,0x49,0x6E,0x63,0x2E,0x2C,0x20,0x32,0x35,0x39,0x33,0x20,0x43,0x6F,0x61,0x73,0x74,0x20,0x41,0x76,0x65,0x2E,0x2C,0x20,0x4D,0x6F,0x75,0x6E,0x74,0x61,0x69,0x6E,0x20,0x56,0x69,0x65,0x77,0x2C,0x20,0x43,0x41,0x20,0x39,0x34,0x30,0x34,0x33,0x20,0x55,0x53,0x41,0x20,0x54,0x65,
			0x6C,0x2E,0x20,0x2B,0x31,0x20,0x28,0x34,0x31,0x35,0x29,0x20,0x39,0x36,0x31,0x2D,0x38,0x38,0x33,0x30,0x20,0x43,0x6F,0x70,0x79,0x72,0x69,0x67,0x68,0x74,0x20,0x28,0x63,0x29,0x20,0x31,0x39,0x39,0x36,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x2C,0x20,0x49,0x6E,0x63,0x2E,0x20,0x20,0x41,0x6C,0x6C,0x20,0x52,0x69,0x67,0x68,0x74,0x73,0x20,0x52,0x65,0x73,0x65,0x72,0x76,0x65,0x64,0x2E,0x20,0x43,0x45,0x52,0x54,0x41,0x49,0x4E,0x20,0x57,0x41,0x52,0x52,0x41,0x4E,0x54,0x49,0x45,0x53,0x20,0x44,0x49,0x53,0x43,
			0x4C,0x41,0x49,0x4D,0x45,0x44,0x20,0x61,0x6E,0x64,0x20,0x4C,0x49,0x41,0x42,0x49,0x4C,0x49,0x54,0x59,0x20,0x4C,0x49,0x4D,0x49,0x54,0x45,0x44,0x2E,0xA0,0x0E,0x06,0x0C,0x60,0x86,0x48,0x01,0x86,0xF8,0x45,0x01,0x07,0x01,0x01,0x01,0xA1,0x0E,0x06,0x0C,0x60,0x86,0x48,0x01,0x86,0xF8,0x45,0x01,0x07,0x01,0x01,0x02,0x30,0x2C,0x30,0x2A,0x16,0x28,0x68,0x74,0x74,0x70,0x73,0x3A,0x2F,0x2F,0x77,0x77,0x77,0x2E,0x76,0x65,0x72,0x69,0x73,0x69,0x67,0x6E,0x2E,0x63,0x6F,0x6D,0x2F,0x72,0x65,0x70,0x6F,0x73,0x69,0x74,0x6F,
			0x72,0x79,0x2F,0x43,0x50,0x53,0x20,0x30,0x11,0x06,0x09,0x60,0x86,0x48,0x01,0x86,0xF8,0x42,0x01,0x01,0x04,0x04,0x03,0x02,0x07,0x80,0x30,0x36,0x06,0x09,0x60,0x86,0x48,0x01,0x86,0xF8,0x42,0x01,0x08,0x04,0x29,0x16,0x27,0x68,0x74,0x74,0x70,0x73,0x3A,0x2F,0x2F,0x77,0x77,0x77,0x2E,0x76,0x65,0x72,0x69,0x73,0x69,0x67,0x6E,0x2E,0x63,0x6F,0x6D,0x2F,0x72,0x65,0x70,0x6F,0x73,0x69,0x74,0x6F,0x72,0x79,0x2F,0x43,0x50,0x53,0x30,0x82,0x04,0x87,0x06,0x09,0x60,0x86,0x48,0x01,0x86,0xF8,0x42,0x01,0x0D,0x04,0x82,0x04,
			0x78,0x16,0x82,0x04,0x74,0x43,0x41,0x55,0x54,0x49,0x4F,0x4E,0x3A,0x20,0x54,0x68,0x65,0x20,0x43,0x6F,0x6D,0x6D,0x6F,0x6E,0x20,0x4E,0x61,0x6D,0x65,0x20,0x69,0x6E,0x20,0x74,0x68,0x69,0x73,0x20,0x43,0x6C,0x61,0x73,0x73,0x20,0x31,0x20,0x44,0x69,0x67,0x69,0x74,0x61,0x6C,0x20,0x0A,0x49,0x44,0x20,0x69,0x73,0x20,0x6E,0x6F,0x74,0x20,0x61,0x75,0x74,0x68,0x65,0x6E,0x74,0x69,0x63,0x61,0x74,0x65,0x64,0x20,0x62,0x79,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x2E,0x20,0x49,0x74,0x20,0x6D,0x61,0x79,0x20,0x62,
			0x65,0x20,0x74,0x68,0x65,0x0A,0x68,0x6F,0x6C,0x64,0x65,0x72,0x27,0x73,0x20,0x72,0x65,0x61,0x6C,0x20,0x6E,0x61,0x6D,0x65,0x20,0x6F,0x72,0x20,0x61,0x6E,0x20,0x61,0x6C,0x69,0x61,0x73,0x2E,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x20,0x64,0x6F,0x65,0x73,0x20,0x61,0x75,0x74,0x68,0x2D,0x0A,0x65,0x6E,0x74,0x69,0x63,0x61,0x74,0x65,0x20,0x74,0x68,0x65,0x20,0x65,0x2D,0x6D,0x61,0x69,0x6C,0x20,0x61,0x64,0x64,0x72,0x65,0x73,0x73,0x20,0x6F,0x66,0x20,0x74,0x68,0x65,0x20,0x68,0x6F,0x6C,0x64,0x65,0x72,0x2E,
			0x0A,0x0A,0x54,0x68,0x69,0x73,0x20,0x63,0x65,0x72,0x74,0x69,0x66,0x69,0x63,0x61,0x74,0x65,0x20,0x69,0x6E,0x63,0x6F,0x72,0x70,0x6F,0x72,0x61,0x74,0x65,0x73,0x20,0x62,0x79,0x20,0x72,0x65,0x66,0x65,0x72,0x65,0x6E,0x63,0x65,0x2C,0x20,0x61,0x6E,0x64,0x20,0x0A,0x69,0x74,0x73,0x20,0x75,0x73,0x65,0x20,0x69,0x73,0x20,0x73,0x74,0x72,0x69,0x63,0x74,0x6C,0x79,0x20,0x73,0x75,0x62,0x6A,0x65,0x63,0x74,0x20,0x74,0x6F,0x2C,0x20,0x74,0x68,0x65,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x20,0x0A,0x43,0x65,0x72,
			0x74,0x69,0x66,0x69,0x63,0x61,0x74,0x69,0x6F,0x6E,0x20,0x50,0x72,0x61,0x63,0x74,0x69,0x63,0x65,0x20,0x53,0x74,0x61,0x74,0x65,0x6D,0x65,0x6E,0x74,0x20,0x28,0x43,0x50,0x53,0x29,0x2C,0x20,0x61,0x76,0x61,0x69,0x6C,0x61,0x62,0x6C,0x65,0x0A,0x69,0x6E,0x20,0x74,0x68,0x65,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x20,0x72,0x65,0x70,0x6F,0x73,0x69,0x74,0x6F,0x72,0x79,0x20,0x61,0x74,0x3A,0x20,0x0A,0x68,0x74,0x74,0x70,0x73,0x3A,0x2F,0x2F,0x77,0x77,0x77,0x2E,0x76,0x65,0x72,0x69,0x73,0x69,0x67,0x6E,0x2E,
			0x63,0x6F,0x6D,0x3B,0x20,0x62,0x79,0x20,0x45,0x2D,0x6D,0x61,0x69,0x6C,0x20,0x61,0x74,0x0A,0x43,0x50,0x53,0x2D,0x72,0x65,0x71,0x75,0x65,0x73,0x74,0x73,0x40,0x76,0x65,0x72,0x69,0x73,0x69,0x67,0x6E,0x2E,0x63,0x6F,0x6D,0x3B,0x20,0x6F,0x72,0x20,0x62,0x79,0x20,0x6D,0x61,0x69,0x6C,0x20,0x61,0x74,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x2C,0x0A,0x49,0x6E,0x63,0x2E,0x2C,0x20,0x32,0x35,0x39,0x33,0x20,0x43,0x6F,0x61,0x73,0x74,0x20,0x41,0x76,0x65,0x2E,0x2C,0x20,0x4D,0x6F,0x75,0x6E,0x74,0x61,0x69,0x6E,
			0x20,0x56,0x69,0x65,0x77,0x2C,0x20,0x43,0x41,0x20,0x39,0x34,0x30,0x34,0x33,0x20,0x55,0x53,0x41,0x0A,0x0A,0x43,0x6F,0x70,0x79,0x72,0x69,0x67,0x68,0x74,0x20,0x28,0x63,0x29,0x31,0x39,0x39,0x36,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x2C,0x20,0x49,0x6E,0x63,0x2E,0x20,0x20,0x41,0x6C,0x6C,0x20,0x52,0x69,0x67,0x68,0x74,0x73,0x20,0x0A,0x52,0x65,0x73,0x65,0x72,0x76,0x65,0x64,0x2E,0x20,0x43,0x45,0x52,0x54,0x41,0x49,0x4E,0x20,0x57,0x41,0x52,0x52,0x41,0x4E,0x54,0x49,0x45,0x53,0x20,0x44,0x49,0x53,0x43,
			0x4C,0x41,0x49,0x4D,0x45,0x44,0x20,0x41,0x4E,0x44,0x20,0x0A,0x4C,0x49,0x41,0x42,0x49,0x4C,0x49,0x54,0x59,0x20,0x4C,0x49,0x4D,0x49,0x54,0x45,0x44,0x2E,0x0A,0x0A,0x57,0x41,0x52,0x4E,0x49,0x4E,0x47,0x3A,0x20,0x54,0x48,0x45,0x20,0x55,0x53,0x45,0x20,0x4F,0x46,0x20,0x54,0x48,0x49,0x53,0x20,0x43,0x45,0x52,0x54,0x49,0x46,0x49,0x43,0x41,0x54,0x45,0x20,0x49,0x53,0x20,0x53,0x54,0x52,0x49,0x43,0x54,0x4C,0x59,0x0A,0x53,0x55,0x42,0x4A,0x45,0x43,0x54,0x20,0x54,0x4F,0x20,0x54,0x48,0x45,0x20,0x56,0x45,0x52,0x49,
			0x53,0x49,0x47,0x4E,0x20,0x43,0x45,0x52,0x54,0x49,0x46,0x49,0x43,0x41,0x54,0x49,0x4F,0x4E,0x20,0x50,0x52,0x41,0x43,0x54,0x49,0x43,0x45,0x0A,0x53,0x54,0x41,0x54,0x45,0x4D,0x45,0x4E,0x54,0x2E,0x20,0x20,0x54,0x48,0x45,0x20,0x49,0x53,0x53,0x55,0x49,0x4E,0x47,0x20,0x41,0x55,0x54,0x48,0x4F,0x52,0x49,0x54,0x59,0x20,0x44,0x49,0x53,0x43,0x4C,0x41,0x49,0x4D,0x53,0x20,0x43,0x45,0x52,0x54,0x41,0x49,0x4E,0x0A,0x49,0x4D,0x50,0x4C,0x49,0x45,0x44,0x20,0x41,0x4E,0x44,0x20,0x45,0x58,0x50,0x52,0x45,0x53,0x53,0x20,
			0x57,0x41,0x52,0x52,0x41,0x4E,0x54,0x49,0x45,0x53,0x2C,0x20,0x49,0x4E,0x43,0x4C,0x55,0x44,0x49,0x4E,0x47,0x20,0x57,0x41,0x52,0x52,0x41,0x4E,0x54,0x49,0x45,0x53,0x0A,0x4F,0x46,0x20,0x4D,0x45,0x52,0x43,0x48,0x41,0x4E,0x54,0x41,0x42,0x49,0x4C,0x49,0x54,0x59,0x20,0x4F,0x52,0x20,0x46,0x49,0x54,0x4E,0x45,0x53,0x53,0x20,0x46,0x4F,0x52,0x20,0x41,0x20,0x50,0x41,0x52,0x54,0x49,0x43,0x55,0x4C,0x41,0x52,0x0A,0x50,0x55,0x52,0x50,0x4F,0x53,0x45,0x2C,0x20,0x41,0x4E,0x44,0x20,0x57,0x49,0x4C,0x4C,0x20,0x4E,0x4F,
			0x54,0x20,0x42,0x45,0x20,0x4C,0x49,0x41,0x42,0x4C,0x45,0x20,0x46,0x4F,0x52,0x20,0x43,0x4F,0x4E,0x53,0x45,0x51,0x55,0x45,0x4E,0x54,0x49,0x41,0x4C,0x2C,0x0A,0x50,0x55,0x4E,0x49,0x54,0x49,0x56,0x45,0x2C,0x20,0x41,0x4E,0x44,0x20,0x43,0x45,0x52,0x54,0x41,0x49,0x4E,0x20,0x4F,0x54,0x48,0x45,0x52,0x20,0x44,0x41,0x4D,0x41,0x47,0x45,0x53,0x2E,0x20,0x53,0x45,0x45,0x20,0x54,0x48,0x45,0x20,0x43,0x50,0x53,0x0A,0x46,0x4F,0x52,0x20,0x44,0x45,0x54,0x41,0x49,0x4C,0x53,0x2E,0x0A,0x0A,0x43,0x6F,0x6E,0x74,0x65,0x6E,
			0x74,0x73,0x20,0x6F,0x66,0x20,0x74,0x68,0x65,0x20,0x56,0x65,0x72,0x69,0x53,0x69,0x67,0x6E,0x20,0x72,0x65,0x67,0x69,0x73,0x74,0x65,0x72,0x65,0x64,0x0A,0x6E,0x6F,0x6E,0x76,0x65,0x72,0x69,0x66,0x69,0x65,0x64,0x53,0x75,0x62,0x6A,0x65,0x63,0x74,0x41,0x74,0x74,0x72,0x69,0x62,0x75,0x74,0x65,0x73,0x20,0x65,0x78,0x74,0x65,0x6E,0x73,0x69,0x6F,0x6E,0x20,0x76,0x61,0x6C,0x75,0x65,0x20,0x73,0x68,0x61,0x6C,0x6C,0x20,0x0A,0x6E,0x6F,0x74,0x20,0x62,0x65,0x20,0x63,0x6F,0x6E,0x73,0x69,0x64,0x65,0x72,0x65,0x64,0x20,
			0x61,0x73,0x20,0x61,0x63,0x63,0x75,0x72,0x61,0x74,0x65,0x20,0x69,0x6E,0x66,0x6F,0x72,0x6D,0x61,0x74,0x69,0x6F,0x6E,0x20,0x76,0x61,0x6C,0x69,0x64,0x61,0x74,0x65,0x64,0x20,0x0A,0x62,0x79,0x20,0x74,0x68,0x65,0x20,0x49,0x41,0x2E,0x30,0x0D,0x06,0x09,0x2A,0x86,0x48,0x86,0xF7,0x0D,0x01,0x01,0x04,0x05,0x00,0x03,0x81,0x81,0x00,0x2B,0x3D,0x44,0xC7,0x32,0x59,0xAE,0xF1,0x5F,0x8F,0x3F,0x87,0xE3,0x3E,0xEB,0x81,0x30,0xF8,0xA9,0x96,0xDB,0x01,0x42,0x0B,0x04,0xEF,0x37,0x02,0x3F,0xD4,0x20,0x61,0x58,0xC4,0x4A,0x3A,
			0x39,0xB3,0xFB,0xD9,0xF8,0xA5,0xC4,0x5E,0x33,0x5A,0x0E,0xFA,0x93,0x56,0x2F,0x6F,0xD6,0x61,0xA2,0xAF,0xA5,0x0C,0x1D,0xE2,0x41,0x65,0xF3,0x40,0x75,0x66,0x83,0xD2,0x5A,0xB4,0xB7,0x56,0x0B,0x8E,0x0D,0xA1,0x33,0x13,0x7D,0x49,0xC3,0xB1,0x00,0x68,0x83,0x7F,0xB5,0x66,0xD4,0x32,0x32,0xFE,0x8B,0x9A,0x5A,0xD6,0x01,0x72,0x31,0x5D,0x85,0x91,0xBC,0x93,0x9B,0x65,0x60,0x25,0xC6,0x1F,0xBC,0xDD,0x69,0x44,0x62,0xC2,0xB2,0x6F,0x46,0xAB,0x2F,0x20,0xA5,0x6F,0xDA,0x48,0x6C,0x9C };

		private X509Certificate x509;

		[SetUp]
		private void SetUp () 
		{
			x509 = new X509Certificate (cert);
		}

		[Test]
		public void PermissionStateNone () 
		{
			PublisherIdentityPermission p = new PublisherIdentityPermission (PermissionState.None);
			AssertNotNull ("PublisherIdentityPermission(PermissionState.None)", p);
			PublisherIdentityPermission copy = (PublisherIdentityPermission) p.Copy ();
			SecurityElement se = p.ToXml ();
			Assert ("ToXml-class", (se.Attributes ["class"] as string).StartsWith (className));
			AssertEquals ("ToXml-version", "1", (se.Attributes ["version"] as string));
			AssertNull ("Certificate==null", p.Certificate);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionStateUnrestricted () 
		{
			// Unrestricted isn't permitted for identity permissions
			PublisherIdentityPermission p = new PublisherIdentityPermission (PermissionState.Unrestricted);
		}

		[Test]
		public void Certificate () 
		{
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (x509);

			PublisherIdentityPermission p2 = new PublisherIdentityPermission (PermissionState.None);
			p2.Certificate = x509;

			AssertEquals ("Certificate", p1.ToXml ().ToString (), p2.ToXml ().ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorCertificateNull () 
		{
			PublisherIdentityPermission p = new PublisherIdentityPermission (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PropertyCertificateNull () 
		{
			PublisherIdentityPermission p = new PublisherIdentityPermission (PermissionState.None);
			p.Certificate = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			ep.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalidPermission () 
		{
			PublisherIdentityPermission p = new PublisherIdentityPermission (PermissionState.None);
			SecurityElement se = p.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement ("IInvalidPermission", se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", se.Attribute ("version"));
			p.FromXml (se2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlWrongVersion () 
		{
			PublisherIdentityPermission p = new PublisherIdentityPermission (PermissionState.None);
			SecurityElement se = p.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement (se.Tag, se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", "2");
			p.FromXml (se2);
		}

		[Test]
		public void FromXml () 
		{
			PublisherIdentityPermission p = new PublisherIdentityPermission (PermissionState.None);
			SecurityElement se = p.ToXml ();
			AssertNotNull ("ToXml()", se);
			p.FromXml (se);

			se.AddAttribute ("X509v3Certificate", x509.GetRawCertDataString ());
			p.FromXml (se);
			AssertEquals ("CertificateHash", x509.GetCertHashString (), p.Certificate.GetCertHashString ());
		}

		[Test]
		public void UnionWithNull () 
		{
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (x509);
			PublisherIdentityPermission p2 = null;
			PublisherIdentityPermission p3 = (PublisherIdentityPermission) p1.Union (p2);
			AssertEquals ("P1 U null == P1", p1.ToXml ().ToString (), p3.ToXml ().ToString ());
		}

		[Test]
		public void Union () 
		{
			// with no certificates
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (PermissionState.None);
			PublisherIdentityPermission p2 = new PublisherIdentityPermission (PermissionState.None);
			PublisherIdentityPermission p3 = (PublisherIdentityPermission) p1.Union (p2);
			AssertNull ("None U None == null", p3);
			// with 1 certificate
			p1 = new PublisherIdentityPermission (x509);
			p2 = new PublisherIdentityPermission (PermissionState.None);
			p3 = (PublisherIdentityPermission) p1.Union (p2);
			AssertEquals ("cert U None == cert", p3.ToXml ().ToString (), p1.ToXml ().ToString ());
			// 2 different certificates
			X509Certificate x2 = new X509Certificate (cert2);
			p2 = new PublisherIdentityPermission (x2);
			p3 = (PublisherIdentityPermission) p1.Union (p2);
			AssertNull ("cert1 U cert2 == null", p3);
			// 2 certificates (same)
			x2 = new X509Certificate (cert);
			p2 = new PublisherIdentityPermission (x2);
			p3 = (PublisherIdentityPermission) p1.Union (p2);
			AssertEquals ("cert1 U cert1 == cert1", p3.ToString (), p1.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnionWithBadPermission () 
		{
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (PermissionState.None);
			EnvironmentPermission ep2 = new EnvironmentPermission (PermissionState.None);
			PublisherIdentityPermission p3 = (PublisherIdentityPermission) p1.Union (ep2);
		}

		[Test]
		public void IntersectWithNull () 
		{
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (x509);
			PublisherIdentityPermission p2 = null;
			PublisherIdentityPermission p3 = (PublisherIdentityPermission) p1.Intersect (p2);
			AssertNull ("P1 N null == null", p3);
		}

		[Test]
		public void Intersect () 
		{
			// intersect None with None
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (PermissionState.None);
			PublisherIdentityPermission p2 = new PublisherIdentityPermission (PermissionState.None);
			PublisherIdentityPermission p3 = (PublisherIdentityPermission) p1.Intersect (p2);
			AssertNull ("None N None == null", p3);
			// with 1 certificate
			p1 = new PublisherIdentityPermission (x509);
			p2 = new PublisherIdentityPermission (PermissionState.None);
			p3 = (PublisherIdentityPermission) p1.Intersect (p2);
			AssertNull ("cert N None == None", p3);
			// 2 different certificates
			X509Certificate x2 = new X509Certificate (cert2);
			p2 = new PublisherIdentityPermission (x2);
			p3 = (PublisherIdentityPermission) p1.Intersect (p2);
			AssertNull ("cert1 N cert2 == null", p3);
			// 2 certificates (same)
			x2 = new X509Certificate (cert);
			p2 = new PublisherIdentityPermission (x2);
			p3 = (PublisherIdentityPermission) p1.Intersect (p2);
			AssertEquals ("cert1 N cert1 == cert1", p3.ToString (), p1.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IntersectWithBadPermission () 
		{
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (x509);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			PublisherIdentityPermission p3 = (PublisherIdentityPermission) p1.Intersect (fdp2);
		}

		[Test]
		public void IsSubsetOfNull () 
		{
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (x509);
			Assert ("IsSubsetOf(null)", !p1.IsSubsetOf (null));
		}

		[Test]
		public void IsSubsetOf () 
		{
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (PermissionState.None);
			PublisherIdentityPermission p2 = new PublisherIdentityPermission (PermissionState.None);
			Assert ("None.IsSubsetOf(None)", p1.IsSubsetOf (p2));
			PublisherIdentityPermission p3 = new PublisherIdentityPermission (x509);
			Assert ("Cert.IsSubsetOf(None)", !p3.IsSubsetOf (p2));
			Assert ("None.IsSubsetOf(Cert)", p2.IsSubsetOf (p3));
			PublisherIdentityPermission p4 = new PublisherIdentityPermission (x509);
			Assert ("Cert.IsSubsetOf(Cert)", p3.IsSubsetOf (p4));
			X509Certificate x2 = new X509Certificate (cert2);
			PublisherIdentityPermission p5 = new PublisherIdentityPermission (x2);
			Assert ("Cert2.IsSubsetOf(Cert)", !p5.IsSubsetOf (p3));
			Assert ("Cert.IsSubsetOf(Cert2)", !p3.IsSubsetOf (p5));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission () 
		{
			PublisherIdentityPermission p1 = new PublisherIdentityPermission (x509);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			Assert ("IsSubsetOf(PublisherIdentityPermission)", p1.IsSubsetOf (fdp2));
		}
	}
}
