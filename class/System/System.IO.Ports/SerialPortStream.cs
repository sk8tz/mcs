//
// System.IO.Ports.SerialPortStream.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//


#if NET_2_0

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace System.IO.Ports
{
	class SerialPortStream : Stream, ISerialStream, IDisposable
	{
		int fd;
		int read_timeout;
		int write_timeout;
		bool disposed;

		[DllImport ("MonoPosixHelper")]
		static extern int open_serial (string portName);

		public SerialPortStream (string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits,
				bool dtrEnable, bool rtsEnable, Handshake handshake, int readTimeout, int writeTimeout,
				int readBufferSize, int writeBufferSize)
		{
			fd = open_serial (portName);
			if (fd == -1)
				throw new IOException ();
			
			if (!set_attributes (fd, baudRate, parity, dataBits, stopBits, handshake))
				throw new IOException (); // Probably Win32Exc for compatibility

			read_timeout = readTimeout;
			write_timeout = writeTimeout;
			
			SetSignal (SerialSignal.Dtr, dtrEnable);
			
			if (handshake != Handshake.RequestToSend && 
					handshake != Handshake.RequestToSendXOnXOff)
				SetSignal (SerialSignal.Rts, rtsEnable);
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return true;
			}
		}

		public override bool CanTimeout {
			get {
				return true;
			}
		}

		public override int ReadTimeout {
			get {
				return read_timeout;
			}
			set {
				if (value < 0 && value != SerialPort.InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				read_timeout = value;
			}
		}

		public override int WriteTimeout {
			get {
				return write_timeout;
			}
			set {
				if (value < 0 && value != SerialPort.InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				write_timeout = value;
			}
		}

		public override long Length {
			get {
				throw new NotSupportedException ();
			}
		}

		public override long Position {
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public override void Flush ()
		{
			// If used, this _could_ flush the serial port
			// buffer (not the SerialPort class buffer)
		}

		[DllImport ("MonoPosixHelper")]
		static extern int read_serial (int fd, byte [] buffer, int offset, int count, int timeout);

		public override int Read ([In,Out] byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("count > buffer.Length - offset");
			
			return read_serial (fd, buffer, offset, count, read_timeout);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		[DllImport ("MonoPosixHelper")]
		static extern void write_serial (int fd, byte [] buffer, int offset, int count, int timeout);

		public override void Write (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("offset+count > buffer.Length");

			write_serial (fd, buffer, offset, count, write_timeout);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposed)
				return;
			
			disposed = true;
			close_serial (fd);
		}

		[DllImport ("MonoPosixHelper")]
		static extern void close_serial (int fd);

		public override void Close ()
		{
			((IDisposable) this).Dispose ();
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~SerialPortStream ()
		{
			Dispose (false);
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		[DllImport ("MonoPosixHelper")]
		static extern bool set_attributes (int fd, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake);

		public void SetAttributes (int baud_rate, Parity parity, int data_bits, StopBits sb, Handshake hs)
		{
			if (!set_attributes (fd, baud_rate, parity, data_bits, sb, hs))
				throw new IOException ();
		}

		public int BytesToRead {
			get {
				return 0; // Not implemented yet
			}
		}

		public int BytesToWrite {
			get {
				return 0; // Not implemented yet
			}
		}

		[DllImport ("MonoPosixHelper")]
		static extern void discard_buffer (int fd, bool inputBuffer);
		
		public void DiscardInBuffer ()
		{
			discard_buffer (fd, true);
		}

		public void DiscardOutBuffer ()
		{
			discard_buffer (fd, false);
		}
		
		[DllImport ("MonoPosixHelper")]
		static extern SerialSignal get_signals (int fd, out int error);

		public SerialSignal GetSignals ()
		{
			int error;
			SerialSignal signals = get_signals (fd, out error);
			if (error == -1)
				throw new IOException ();

			return signals;
		}

		[DllImport ("MonoPosixHelper")]
		static extern int set_signal (int fd, SerialSignal signal, bool value);

		public void SetSignal (SerialSignal signal, bool value)
		{
			if (signal < SerialSignal.Cd || signal > SerialSignal.Rts ||
					signal == SerialSignal.Cd ||
					signal == SerialSignal.Cts ||
					signal == SerialSignal.Dsr)
				throw new Exception ("Invalid internal value");

			if (set_signal (fd, signal, value) == -1)
				throw new IOException ();
		}

	}
}

#endif


