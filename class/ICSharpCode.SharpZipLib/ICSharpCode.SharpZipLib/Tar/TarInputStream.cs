// TarInputStream.cs
// Copyright (C) 2001 Mike Krueger
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using System.IO;
using System.Text;

namespace ICSharpCode.SharpZipLib.Tar {
	
	/// <summary>
	/// The TarInputStream reads a UNIX tar archive as an InputStream.
	/// methods are provided to position at each successive entry in
	/// the archive, and the read each entry as a normal input stream
	/// using read().
	/// </summary>
	public class TarInputStream : Stream
	{
		protected bool debug;
		protected bool hasHitEOF;
		
		protected int entrySize;
		protected int entryOffset;
		
		protected byte[] readBuf;
		
		protected TarBuffer buffer;
		protected TarEntry  currEntry;
		protected IEntryFactory eFactory;
		
		Stream inputStream;
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override bool CanRead {
			get {
				return inputStream.CanRead;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override bool CanSeek {
			get {
				return inputStream.CanSeek;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override bool CanWrite {
			get {
				return inputStream.CanWrite;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override long Length {
			get {
				return inputStream.Length;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override long Position {
			get {
				return inputStream.Position;
			}
			set {
				inputStream.Position = value;
			}
		}
		
		/// <summary>
		/// Flushes the baseInputStream
		/// </summary>
		public override void Flush()
		{
			inputStream.Flush();
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return inputStream.Seek(offset, origin);
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override void SetLength(long val)
		{
			inputStream.SetLength(val);
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override void Write(byte[] array, int offset, int count)
		{
			inputStream.Write(array, offset, count);
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override void WriteByte(byte val)
		{
			inputStream.WriteByte(val);
		}
			
		
		public TarInputStream(Stream inputStream) : this(inputStream, TarBuffer.DEFAULT_BLKSIZE, TarBuffer.DEFAULT_RCDSIZE)
		{
		}
		
		public TarInputStream(Stream inputStream, int blockSize) : this(inputStream, blockSize, TarBuffer.DEFAULT_RCDSIZE)
		{
		}
		
		public TarInputStream(Stream inputStream, int blockSize, int recordSize)
		{
			this.inputStream = inputStream;
			this.buffer      = TarBuffer.CreateInputTarBuffer(inputStream, blockSize, recordSize);
			
			this.readBuf = null;
			this.debug     = false;
			this.hasHitEOF = false;
			this.eFactory  = null;
		}
		
		public void SetDebug(bool debugF)
		{
			this.debug = debugF;
			SetBufferDebug(debugF);
		}
		public void SetBufferDebug(bool debug)
		{
			this.buffer.SetDebug(debug);
		}
		
		
		
		public void SetEntryFactory(IEntryFactory factory)
		{
			this.eFactory = factory;
		}
		
		/// <summary>
		/// Closes this stream. Calls the TarBuffer's close() method.
		/// The underlying stream is closed by the TarBuffer.
		/// </summary>
		public override void Close()
		{
			this.buffer.Close();
		}
		
		/// <summary>
		/// Get the record size being used by this stream's TarBuffer.
		/// </summary>
		/// <returns>
		/// TarBuffer record size.
		/// </returns>
		public int GetRecordSize()
		{
			return this.buffer.GetRecordSize();
		}
		
		/// <summary>
		/// Get the available data that can be read from the current
		/// entry in the archive. This does not indicate how much data
		/// is left in the entire archive, only in the current entry.
		/// This value is determined from the entry's size header field
		/// and the amount of data already read from the current entry.
		/// </summary>
		/// <returns>
		/// The number of available bytes for the current entry.
		/// </returns>
		public int Available {
			get {
				return this.entrySize - this.entryOffset;
			}
		}
		
		/// <summary>
		/// Skip bytes in the input buffer. This skips bytes in the
		/// current entry's data, not the entire archive, and will
		/// stop at the end of the current entry's data if the number
		/// to skip extends beyond that point.
		/// </summary>
		/// <param name="numToSkip">
		/// The number of bytes to skip.
		/// </param>
		public void Skip(int numToSkip)
		{
			// REVIEW
			// This is horribly inefficient, but it ensures that we
			// properly skip over bytes via the TarBuffer...
			//
			byte[] skipBuf = new byte[8 * 1024];
			
			for (int num = numToSkip; num > 0;){
				int numRead = this.Read(skipBuf, 0, (num > skipBuf.Length ? skipBuf.Length : num));
				
				if (numRead == -1) {
					break;
				}
				
				num -= numRead;
			}
		}
		
		/// <summary>
		/// Since we do not support marking just yet, we return false.
		/// </summary>
		public bool IsMarkSupported {
			get {
				return false;
			}
		}
		
		/// <summary>
		/// Since we do not support marking just yet, we do nothing.
		/// </summary>
		/// <param name ="markLimit">
		/// The limit to mark.
		/// </param>
		public void Mark(int markLimit)
		{
		}
		
		/// <summary>
		/// Since we do not support marking just yet, we do nothing.
		/// </summary>
		public void Reset()
		{
		}
		
		/// <summary>
		/// Get the next entry in this tar archive. This will skip
		/// over any remaining data in the current entry, if there
		/// is one, and place the input stream at the header of the
		/// next entry, and read the header and instantiate a new
		/// TarEntry from the header bytes and return that entry.
		/// If there are no more entries in the archive, null will
		/// be returned to indicate that the end of the archive has
		/// been reached.
		/// </summary>
		/// <returns>
		/// The next TarEntry in the archive, or null.
		/// </returns>
		public TarEntry GetNextEntry()
		{
			if (this.hasHitEOF) {
				return null;
			}
			
			if (this.currEntry != null) {
				int numToSkip = this.entrySize - this.entryOffset;
				
				if (this.debug) {
					Console.Error.WriteLine("TarInputStream: SKIP currENTRY '" + this.currEntry.Name + "' SZ " + this.entrySize + " OFF " + this.entryOffset + "  skipping " + numToSkip + " bytes");
				}
				
				if (numToSkip > 0) {
					this.Skip(numToSkip);
				}
				
				this.readBuf = null;
			}
			
			byte[] headerBuf = this.buffer.ReadRecord();
			
			if (headerBuf == null) {
				if (this.debug) {
					Console.Error.WriteLine("READ NULL RECORD");
				}
				
				this.hasHitEOF = true;
			} else if (this.buffer.IsEOFRecord(headerBuf)) {
				if (this.debug) {
					Console.Error.WriteLine( "READ EOF RECORD" );
				}
				
				this.hasHitEOF = true;
			}
			
			if (this.hasHitEOF) {
				this.currEntry = null;
			} else {
				try {
					if (this.eFactory == null) {
						this.currEntry = new TarEntry(headerBuf);
					} else {
						this.currEntry = this.eFactory.CreateEntry(headerBuf);
					}
					
					if (!(headerBuf[257] == 'u' && headerBuf[258] == 's' && headerBuf[259] == 't' && headerBuf[260] == 'a' && headerBuf[261] == 'r')) {
						throw new InvalidHeaderException("header magic is not 'ustar', but '" + headerBuf[257] + headerBuf[258] + headerBuf[259] + headerBuf[260] + headerBuf[261] + 
						                                 "', or (dec) " + ((int)headerBuf[257]) + ", " + ((int)headerBuf[258]) + ", " + ((int)headerBuf[259]) + ", " + ((int)headerBuf[260]) + ", " + ((int)headerBuf[261]));
					}
					        
					if (this.debug) {
			        	Console.Error.WriteLine("TarInputStream: SET CURRENTRY '" + this.currEntry.Name + "' size = " + this.currEntry.Size);
					}
			
					this.entryOffset = 0;
					
					// REVIEW How do we resolve this discrepancy?!
					this.entrySize = (int) this.currEntry.Size;
				} catch (InvalidHeaderException ex) {
					this.entrySize = 0;
					this.entryOffset = 0;
					this.currEntry = null;
					throw new InvalidHeaderException("bad header in block " + this.buffer.GetCurrentBlockNum() + " record " + this.buffer.GetCurrentRecordNum() + ", " + ex.Message);
				}
			}
			return this.currEntry;
		}
		
		/// <summary>
		/// Reads a byte from the current tar archive entry.
		/// This method simply calls read(byte[], int, int).
		/// </summary>
		public override int ReadByte()
		{
			byte[] oneByteBuffer = new byte[1];
			int num = this.Read(oneByteBuffer, 0, 1);
			if (num <= 0) { // return -1 to indicate that no byte was read.
				return -1;
			}
			return (int)oneByteBuffer[0];
		}
		
		/// <summary>
		/// Reads bytes from the current tar archive entry.
		/// 
		/// This method is aware of the boundaries of the current
		/// entry in the archive and will deal with them as if they
		/// entry in the archive and will deal with them as if they
		/// </summary>
		/// <param name="buf">
		/// The buffer into which to place bytes read.
		/// </param>
		/// <param name="offset">
		/// The offset at which to place bytes read.
		/// </param>
		/// <param name="numToRead">
		/// The number of bytes to read.
		/// </param>
		/// <returns>
		/// The number of bytes read, or -1 at EOF.
		/// </returns>
		public override int Read(byte[] buf, int offset, int numToRead)
		{
			int totalRead = 0;
			
			if (this.entryOffset >= this.entrySize) {
				return -1;
			}
			
			if ((numToRead + this.entryOffset) > this.entrySize) {
				numToRead = this.entrySize - this.entryOffset;
			}
			
			if (this.readBuf != null) {
				int sz = (numToRead > this.readBuf.Length) ? this.readBuf.Length : numToRead;
				
				Array.Copy(this.readBuf, 0, buf, offset, sz);
				
				if (sz >= this.readBuf.Length) {
					this.readBuf = null;
				} else {
					int newLen = this.readBuf.Length - sz;
					byte[] newBuf = new byte[ newLen ];
					Array.Copy(this.readBuf, sz, newBuf, 0, newLen);
					this.readBuf = newBuf;
				}
				
				totalRead += sz;
				numToRead -= sz;
				offset += sz;
			}
			
			while (numToRead > 0) {
				byte[] rec = this.buffer.ReadRecord();
				if (rec == null) {
					// Unexpected EOF!
					throw new IOException("unexpected EOF with " + numToRead + " bytes unread");
				}
				
				int sz     = numToRead;
				int recLen = rec.Length;
				
				if (recLen > sz) {
					Array.Copy(rec, 0, buf, offset, sz);
					this.readBuf = new byte[recLen - sz];
					Array.Copy(rec, sz, this.readBuf, 0, recLen - sz);
				} else {
					sz = recLen;
					Array.Copy(rec, 0, buf, offset, recLen);
				}
				
				totalRead += sz;
				numToRead -= sz;
				offset += sz;
			}
			
			this.entryOffset += totalRead;
			
			return totalRead;
		}
		
		/// <summary>
		/// Copies the contents of the current tar archive entry directly into
		/// an output stream.
		/// </summary>
		/// <param name="outputStream">
		/// The OutputStream into which to write the entry's data.
		/// </param>
		public void CopyEntryContents(Stream outputStream)
		{
			byte[] buf = new byte[32 * 1024];
			
			while (true) {
				int numRead = this.Read(buf, 0, buf.Length);
				if (numRead <= 0) {
					break;
				}
				outputStream.Write(buf, 0, numRead);
			}
		}
		
		/// <summary>
		/// This interface is provided, with the method setEntryFactory(), to allow
		/// the programmer to have their own TarEntry subclass instantiated for the
		/// entries return from getNextEntry().
		/// </summary>
		public interface IEntryFactory
		{
			TarEntry CreateEntry(string name);
			
			TarEntry CreateEntryFromFile(string fileName);
			
			TarEntry CreateEntry(byte[] headerBuf);
		}
		
		public class EntryFactoryAdapter : IEntryFactory
		{
			public TarEntry CreateEntry(string name)
			{
				return TarEntry.CreateTarEntry(name);
			}
			
			public TarEntry CreateEntryFromFile(string fileName)
			{
				return TarEntry.CreateEntryFromFile(fileName);
			}
			
			public TarEntry CreateEntry(byte[] headerBuf)
			{
				return new TarEntry(headerBuf);
			}
		}
	}
	
	
}

/* The original Java file had this header:
	** Authored by Timothy Gerard Endres
	** <mailto:time@gjt.org>  <http://www.trustice.com>
	**
	** This work has been placed into the public domain.
	** You may use this work in any way and for any purpose you wish.
	**
	** THIS SOFTWARE IS PROVIDED AS-IS WITHOUT WARRANTY OF ANY KIND,
	** NOT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY. THE AUTHOR
	** OF THIS SOFTWARE, ASSUMES _NO_ RESPONSIBILITY FOR ANY
	** CONSEQUENCE RESULTING FROM THE USE, MODIFICATION, OR
	** REDISTRIBUTION OF THIS SOFTWARE.
	**
	*/
	
