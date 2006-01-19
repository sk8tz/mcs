﻿//
// UnicodeEncodingTest.cs - NUnit Test Cases for System.Text.UnicodeEncoding
//
// Author:
//     Patrick Kalkman  kalkman@cistron.nl
//
// (C) 2003 Patrick Kalkman
// 
using NUnit.Framework;
using System;
using System.Text;

namespace MonoTests.System.Text
{
        [TestFixture]
        public class UnicodeEncodingTest 
        {
                [Test]
                public void TestEncodingGetBytes1()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        byte[] UniBytes;
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (); //little-endian
                        UniBytes = UnicodeEnc.GetBytes (Unicode);
                        
                        Assertion.AssertEquals ("Uni #1", 0xA0, UniBytes [0]);
                        Assertion.AssertEquals ("Uni #2", 0x03, UniBytes [1]);
                        Assertion.AssertEquals ("Uni #3", 0xA3, UniBytes [2]);
                        Assertion.AssertEquals ("Uni #4", 0x03, UniBytes [3]);
                }
        
                [Test]
                public void TestEncodingGetBytes2()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        byte[] UniBytes;
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (true, true); //big-endian
                        UniBytes = UnicodeEnc.GetBytes (Unicode);
                        
                        Assertion.AssertEquals ("Uni #1", 0x03, UniBytes [0]);
                        Assertion.AssertEquals ("Uni #2", 0xA0, UniBytes [1]);
                        Assertion.AssertEquals ("Uni #3", 0x03, UniBytes [2]);
                        Assertion.AssertEquals ("Uni #4", 0xA3, UniBytes [3]);
                }

                [Test]
                public void TestEncodingGetBytes3()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        byte[] UniBytes = new byte [4];
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (); //little-endian 
                        int Cnt = UnicodeEnc.GetBytes (Unicode.ToCharArray(), 0, Unicode.Length, UniBytes, 0);
                        
                        Assertion.AssertEquals ("Uni #1", 4, Cnt);
                        Assertion.AssertEquals ("Uni #2", 0xA0, UniBytes [0]);
                        Assertion.AssertEquals ("Uni #3", 0x03, UniBytes [1]);
                        Assertion.AssertEquals ("Uni #4", 0xA3, UniBytes [2]);
                        Assertion.AssertEquals ("Uni #5", 0x03, UniBytes [3]);
                }
        
                [Test]
                public void TestEncodingDecodingGetBytes1()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (); //little-endian 
                        //Encode the unicode string.
                        byte[] UniBytes = UnicodeEnc.GetBytes (Unicode);
                        //Decode the bytes to a unicode char array.
                        char[] UniChars = UnicodeEnc.GetChars (UniBytes);
                        string Result = new string(UniChars);
                        
                        Assertion.AssertEquals ("Uni #1", Unicode, Result);
                }

                [Test]
                public void TestEncodingDecodingGetBytes2()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (true, true); //big-endian 
                        //Encode the unicode string.
                        byte[] UniBytes = UnicodeEnc.GetBytes (Unicode);
                        //Decode the bytes to a unicode char array.
                        char[] UniChars = UnicodeEnc.GetChars (UniBytes);
                        string Result = new string(UniChars);
                        
                        Assertion.AssertEquals ("Uni #1", Unicode, Result);
                }

		[Test]
		public void TestEncodingGetCharCount ()
		{
			byte[] b = new byte[] {255, 254, 115, 0, 104, 0, 105, 0};
			UnicodeEncoding encoding = new UnicodeEncoding ();

			Assertion.AssertEquals ("GetCharCount #1", 3,
				encoding.GetCharCount (b, 2, b.Length - 2));
		}

	
                
                [Test]
                public void TestPreamble1()
                {
                        //litle-endian with byte order mark.
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (false, true); 
                        byte[] PreAmble = UnicodeEnc.GetPreamble();

                        Assertion.AssertEquals ("Uni #1", 0xFF, PreAmble [0]);
                        Assertion.AssertEquals ("Uni #2", 0xFE, PreAmble [1]);
                }

                [Test]
                public void TestPreamble2()
                {
                        //big-endian with byte order mark.
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (true, true); 
                        byte[] PreAmble = UnicodeEnc.GetPreamble();

                        Assertion.AssertEquals ("Uni #1", 0xFE, PreAmble [0]);
                        Assertion.AssertEquals ("Uni #2", 0xFF, PreAmble [1]);
                }

                [Test]
                public void TestPreamble3()
                {
                        //little-endian without byte order mark.
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (false, false); 
                        byte[] PreAmble = UnicodeEnc.GetPreamble();

                        Assertion.AssertEquals ("Uni #1", 0, PreAmble.Length);
                }
                
                [Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
                public void TestMaxCharCount()
                {
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding ();
#if NET_2_0
                        // where is this extra 1 coming from?
                        Assertion.AssertEquals ("UTF #1", 25, UnicodeEnc.GetMaxCharCount(51));
#else
                        Assertion.AssertEquals ("UTF #1", 25, UnicodeEnc.GetMaxCharCount(50));
#endif
                }
        
                [Test]
                public void TestMaxByteCount()
                {
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding ();
#if NET_2_0
                        // is this extra 2 BOM?
                        Assertion.AssertEquals ("UTF #1", 102, UnicodeEnc.GetMaxByteCount(50));
#else
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding ();
                        Assertion.AssertEquals ("UTF #1", 100, UnicodeEnc.GetMaxByteCount(50));
#endif
                }
        }
}



