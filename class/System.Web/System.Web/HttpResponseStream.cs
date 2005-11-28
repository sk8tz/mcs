//
// System.Web.HttpResponseStream.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Ben Maurer (bmaurer@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
	
namespace System.Web {

	//
	// HttpResponseStream implements the "OutputStream" from HttpResponse
	//
	// The MS implementation is broken in that it does not hook up this
	// to HttpResponse, so calling "Flush" on Response.OutputStream does not
	// flush the contents and produce the headers.
	//
	// You must call HttpResponse.Flush which does the actual header generation
	// and actual data flushing
	//
	internal class HttpResponseStream : Stream {
		Bucket first_bucket;
		Bucket cur_bucket;
		HttpResponse response;
		internal long total;
		Stream filter;
		byte [] chunk_buffer = new byte [24];

		public HttpResponseStream (HttpResponse response)
		{
			this.response = response;
		}

		internal bool HaveFilter {
			get { return filter != null; }
		}

		public Stream Filter {
			get {
				if (filter == null)
					filter = new OutputFilterStream (this);
				return filter;
			}
			set {
				filter = value;
			}
		}
#if TARGET_JVM
		class Chunk
		{
			public byte[] data;
			public int size;
			public const int Length = 32 * 1024;
			
			public Chunk Next, Prev;

			public Chunk () 
			{
				data = new byte[Length];
				size = Length;
			}
	
			public void Dispose ()
			{
			}
	
			public static Chunk Unlink (ref Chunk head, Chunk b)
			{
				if (head == b)
					head = head.Next;
				if (b.Prev != null)
					b.Prev.Next = b.Next;
				if (b.Next != null)
					b.Next.Prev = b.Prev;

				b.Next = b.Prev = null;
				return b;
			}
	
			public static Chunk Link (Chunk head, Chunk b)
			{
				b.Next = head;
				if (head != null)
					head.Prev = b;
				return b;
			}

			public static void Copy(byte[] buff, int offset, Chunk c, int pos, int len)
			{
				Array.Copy(buff, offset, c.data, pos, len);
			}

			public static void Copy(Chunk c, int pos, byte[] buff, int offset, int len)
			{
				Array.Copy(c.data, pos, buff, offset, len);
			}
		}

		sealed class BufferManager {
			static Chunk filled;
			static Chunk empty;
	
			public static Chunk GetChunk (int cb)
			{
				Chunk c;
				if (empty == null)
					empty = new Chunk (cb);
				
				c = empty;
				filled = Chunk.Link (filled, Chunk.Unlink (ref empty, c));
				return c;
			}
	
			public static void DisposeChunk (Chunk c)
			{
				empty = Chunk.Link (empty, Chunk.Unlink (ref filled, c));
			}
	
			public static void DisposeEmptyChunks ()
			{
				for (Chunk c = empty; c != null; c = c.Next)
					c.Dispose ();
				empty = null;
			}
			
	
			public static void PrintState ()
			{
				Console.WriteLine ("Filled blocks:");
				for (Chunk c = filled; c != null; c = c.Next)
					Console.WriteLine ("\t{0}", c);
				Console.WriteLine ("Empty blocks:");
				for (Chunk c = empty; c != null; c = c.Next)
					Console.WriteLine ("\t{0}", c);	
				
			}
		}

#else // TARGET_JVM
		unsafe sealed class BlockManager {
			const int PreferredLength = 128 * 1024;
			byte *data;
			int position;
			int block_size;

			public BlockManager ()
			{
			}

			public int Position {
				get { return position; }
			}

			void EnsureCapacity (int capacity)
			{
				if (block_size >= capacity)
					return;

				capacity += PreferredLength;
				capacity = (capacity / PreferredLength) * PreferredLength;
				data = (byte *) Marshal.ReAllocHGlobal ((IntPtr) data, (IntPtr) capacity);
				block_size = capacity;
			}

			public void Write (byte [] buffer, int offset, int count)
			{
				if (count == 0)
					return;
				
				EnsureCapacity (position + count);
				Marshal.Copy (buffer, offset, (IntPtr) (data + position), count);
				position += count;
			}

			public void Send (HttpWorkerRequest wr, int start, int end)
			{
				if (end - start <= 0)
					return;

				wr.SendResponseFromMemory ((IntPtr) (data + start), end - start);
			}

			public void Send (Stream stream, int start, int end)
			{
				int len = end - start;
				if (len <= 0)
					return;

				byte [] buffer = new byte [Math.Min (len, 32 * 1024)];
				int size = buffer.Length;
				while (len > 0) {
					Marshal.Copy ((IntPtr) (data + start), buffer, 0, size);
					stream.Write (buffer, 0, size);
					start += size;
					len -= size;
					if (len > 0 && len < size)
						size = len;
				}
			}
			
			public void Dispose ()
			{
				if ((IntPtr) data != IntPtr.Zero) {
					Marshal.FreeHGlobal ((IntPtr) data);
					data = (byte *) IntPtr.Zero;
				}
			}
		}

#endif
		abstract class Bucket {
			public Bucket Next;

			public virtual void Dispose ()
			{
			}

			public abstract void Send (HttpWorkerRequest wr);
			public abstract void Send (Stream stream);
			public abstract int Length { get; }
		}

#if !TARGET_JVM
		unsafe
#endif
		class ByteBucket : Bucket {
			int start;
			int length;
			public BlockManager blocks;
			public bool Expandable = true;

			public ByteBucket () : this (null)
			{
			}

			public ByteBucket (BlockManager blocks)
			{
				if (blocks == null)
					blocks = new BlockManager ();

				this.blocks = blocks;
				start = blocks.Position;
			}

			public override int Length {
				get { return length; }
			}

			public int Write (byte [] buf, int offset, int count)
			{
				if (Expandable == false)
					throw new Exception ("This should not happen.");

				blocks.Write (buf, offset, count);
				length += count;
				return count;
			}

			public override void Dispose ()
			{
				blocks.Dispose ();
			}

			public override void Send (HttpWorkerRequest wr)
			{
				if (length == 0)
					return;

				blocks.Send (wr, start, length);
			}

			public override void Send (Stream stream)
			{
				if (length == 0)
					return;

				blocks.Send (stream, start, length);
			}
		}
	
		class BufferedFileBucket : Bucket {
			string file;
			long offset;
			long length;
	
			public BufferedFileBucket (string f, long off, long len)
			{
				file = f;
				offset = off;
				length = len;
			}

			public override int Length {
				get { return (int) length; }
			}

			public override void Send (HttpWorkerRequest wr)
			{
				wr.SendResponseFromFile (file, offset, length);
			}

			public override void Send (Stream stream)
			{
				using (FileStream fs = File.OpenRead (file)) {
					byte [] buffer = new byte [Math.Min (fs.Length, 32*1024)];

					long remain = fs.Length;
					int n;
					while (remain > 0 && (n = fs.Read (buffer, 0, (int) Math.Min (remain, 32*1024))) != 0){
						remain -= n;
						stream.Write (buffer, 0, n);
					}
				}
			}

			public override string ToString ()
			{
				return String.Format ("file {0} {1} bytes from position {2}", file, length, offset);
			}	
		}
	
		void AppendBucket (Bucket b)
		{
			if (first_bucket == null) {
				cur_bucket = first_bucket = b;
				return;
			}
	
			cur_bucket.Next = b;
			cur_bucket = b;
		}
	
		//
		// Nothing happens here, broken by requirement.
		// See note at the start
		//
	        public override void Flush () 
		{
		}

		void SendChunkSize (long l, bool last)
		{
			if (l == 0 && !last)
				return;

			int i = 0;
			if (l >= 0) {
				string s = String.Format ("{0:x}", l);
				for (; i < s.Length; i++)
					chunk_buffer [i] = (byte) s [i];
			}

			chunk_buffer [i++] = 13;
			chunk_buffer [i++] = 10;
			if (last) {
				chunk_buffer [i++] = 13;
				chunk_buffer [i++] = 10;
			}

			response.WorkerRequest.SendResponseFromMemory (chunk_buffer, i);
		}

		internal void Flush (HttpWorkerRequest wr, bool final_flush)
		{
			if (total == 0 && !final_flush)
				return;

			if (response.use_chunked) 
				SendChunkSize (total, false);

			for (Bucket b = first_bucket; b != null; b = b.Next) {
				b.Send (wr);
			}

			if (response.use_chunked) {
				SendChunkSize (-1, false);
				if (final_flush)
					SendChunkSize (0, true);
			}

			wr.FlushResponse (final_flush);

			Clear ();
		}

		internal int GetTotalLength ()
		{
			int size = 0;
			for (Bucket b = first_bucket; b != null; b = b.Next)
				size += b.Length;

			return size;
		}

		internal MemoryStream GetData ()
		{
			MemoryStream stream = new MemoryStream ();
			for (Bucket b = first_bucket; b != null; b = b.Next)
				b.Send (stream);
			return stream;
		}

		public void WriteFile (string f, long offset, long length)
		{
			if (length == 0)
				return;

			ByteBucket bb = cur_bucket as ByteBucket;

			if (bb != null) {
				bb.Expandable = false;
				bb = new ByteBucket (bb.blocks);
			}

			total += length;
			
			AppendBucket (new BufferedFileBucket (f, offset, length));
			if (bb != null)
				AppendBucket (bb);
			// Flush () is called from HttpResponse if needed (WriteFile/TransmitFile)
		}

		bool filtering;
		internal void ApplyFilter (bool close)
		{
			if (filter == null)
				return;

			filtering = true;
			Bucket one = first_bucket;
			first_bucket = null; // This will recreate new buckets for the filtered content
			cur_bucket = null;
			total = 0;
			for (Bucket b = one; b != null; b = b.Next)
				b.Send (filter);

			for (Bucket b = one; b != null; b = b.Next)
				b.Dispose ();

			if (close) {
				filter.Flush ();
				filter.Close ();
				filter = null;
			} else {
				filter.Flush ();
			}
			filtering = false;
		}

	        public override void Write (byte [] buffer, int offset, int count)
		{
			bool buffering = response.Buffer;

			if (buffering) {
				// It does not matter whether we're in ApplyFilter or not
				AppendBuffer (buffer, offset, count);
			} else if (filter == null || filtering) {
				response.WriteHeaders (false);
				HttpWorkerRequest wr = response.WorkerRequest;
				// Direct write because not buffering
				if (offset == 0) {
					wr.SendResponseFromMemory (buffer, count);
				} else {
					UnsafeWrite (wr, buffer, offset, count);
				}
				wr.FlushResponse (false);
			} else {
				// Write to the filter, which will call us back, and then Flush
				filtering = true;
				try {
					filter.Write (buffer, offset, count);
				} finally {
					filtering = false;
				}
				Flush (response.WorkerRequest, false);
			}
		}

		unsafe void UnsafeWrite (HttpWorkerRequest wr, byte [] buffer, int offset, int count)
		{
			fixed (byte *ptr = buffer) {
				wr.SendResponseFromMemory ((IntPtr) (ptr + offset), count);
			}
		}

		void AppendBuffer (byte [] buffer, int offset, int count)
		{
			if (!(cur_bucket is ByteBucket))
				AppendBucket (new ByteBucket ());

			total += count;
			((ByteBucket) cur_bucket).Write (buffer, offset, count);
		}

		//
		// This should not flush/close or anything else, its called
		// just to free any memory we might have allocated (when we later
		// implement something with unmanaged memory).
		//
		internal void ReleaseResources (bool close_filter)
		{
			if (close_filter && filter != null) {
				filter.Close ();
				filter = null;
			}

			for (Bucket b = first_bucket; b != null; b = b.Next)
				b.Dispose ();

			first_bucket = null;
			cur_bucket = null;
		}

		public void Clear ()
		{
			//
			// IMPORTANT: you must dispose *AFTER* using all the buckets Byte chunks might be
			// split across two buckets if there is a file between the data.
			//
			ReleaseResources (false);
			total = 0;
		}
		
	        public override bool CanRead {
	                get {
				return false;
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
		
		const string notsupported = "HttpResponseStream is a forward, write-only stream";
		
	        public override long Length {
	                get {
				throw new InvalidOperationException (notsupported);
			}
	        }
	
		public override long Position {
	                get {
				throw new InvalidOperationException (notsupported);
			}
	                set {
				throw new InvalidOperationException (notsupported);
			}
	        }
		
	        public override long Seek (long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException (notsupported);
		}
		
	        public override void SetLength (long value) 
		{
			throw new InvalidOperationException (notsupported);
		}
	
		public override int Read (byte [] buffer, int offset, int count)
		{
			throw new InvalidOperationException (notsupported);
		}
	}
}

