//
// ToBase64TransformTest.cs - NUnit Test Cases for ToBase64Transform
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class ToBase64TransformTest : Assertion {

		[Test]
		public void Properties ()
		{
			ICryptoTransform t = new ToBase64Transform ();
			Assert ("CanReuseTransform", t.CanReuseTransform);
			Assert ("CanTransformMultipleBlocks", !t.CanTransformMultipleBlocks);
			AssertEquals ("InputBlockSize", 3, t.InputBlockSize);
			AssertEquals ("OutputBlockSize", 4, t.OutputBlockSize);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformBlock_NullInput () 
		{
			byte[] output = new byte [4];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformBlock (null, 0, 0, output, 0);
		}

		[Test]
		public void TransformBlock_WrongLength () 
		{
			byte[] input = new byte [6];
			byte[] output = new byte [8];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformBlock (input, 0, 6, output, 0);
			// note only the first block has been processed
			AssertEquals ("WrongLength", "41-41-41-41-00-00-00-00", BitConverter.ToString (output));
		}

		[Test]
//		[ExpectedException (typeof (ArgumentNullException))]
		[Ignore ("MS throw a ExecutionEngineException")]
		public void TransformBlock_NullOutput () 
		{
			byte[] input = new byte [3];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformBlock (input, 0, 3, null, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void TransformBlock_Dispose () 
		{
			byte[] input = new byte [3];
			byte[] output = new byte [4];
			ToBase64Transform t = new ToBase64Transform ();
			t.Clear ();
			t.TransformBlock (input, 0, input.Length, output, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformFinalBlock_Null () 
		{
			byte[] input = new byte [3];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformFinalBlock (null, 0, 3);
		}

		[Test]
		public void TransformFinalBlock_SmallLength () 
		{
			byte[] input = new byte [2]; // smaller than InputBlockSize
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformFinalBlock (input, 0, 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformFinalBlock_WrongLength () 
		{
			byte[] input = new byte [6];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformFinalBlock (input, 0, 6);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void TransformFinalBlock_Dispose () 
		{
			byte[] input = new byte [3];
			ToBase64Transform t = new ToBase64Transform ();
			t.Clear ();
			t.TransformFinalBlock (input, 0, input.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformBlock_InputOffset_Negative () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, -1, input.Length, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_InputOffset_Overflow () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, Int32.MaxValue, input.Length, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_InputCount_Negative () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, 0, -1, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_InputCount_Overflow () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, 0, Int32.MaxValue, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void TransformBlock_OutputOffset_Negative () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, 0, input.Length, output, -1);
			}
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void TransformBlock_OutputOffset_Overflow () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, 0, input.Length, output, Int32.MaxValue);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformFinalBlock_Input_Null () 
		{
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (null, 0, 15);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformFinalBlock_InputOffset_Negative () 
		{
			byte[] input = new byte [15];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (input, -1, input.Length);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformFinalBlock_InputOffset_Overflow () 
		{
			byte[] input = new byte [15];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (input, Int32.MaxValue, input.Length);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformFinalBlock_InputCount_Negative () 
		{
			byte[] input = new byte [15];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (input, 0, -1);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformFinalBlock_InputCount_Overflow () 
		{
			byte[] input = new byte [15];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (input, 0, Int32.MaxValue);
			}
		}
	}
}
