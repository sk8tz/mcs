/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Text;

namespace FirebirdSql.Data.Common
{
	internal sealed class Charset
	{
		#region Static Fields

		private static CharsetCollection supportedCharsets;

		#endregion

		#region Static Properties

		public static CharsetCollection SupportedCharsets
		{
			get
			{
				if (Charset.supportedCharsets == null)
				{
					Charset.InitializeSupportedCharsets();
				}

				return Charset.supportedCharsets;
			}
		}

		public static Charset DefaultCharset
		{
			get { return Charset.SupportedCharsets[0]; }
		}

		#endregion

		#region Fields

		private int		id;
		private string	name;
		private string	systemName;
		private int		bytesPerCharacter;

		#endregion

		#region Properties

		public int ID
		{
			get { return this.id; }
		}

		public string Name
		{
			get { return this.name; }
		}

		public int BytesPerCharacter
		{
			get { return this.bytesPerCharacter; }
		}

		#endregion

		#region Constructors

		public Charset(
			int		id,
			string	name,
			int		bytesPerCharacter,
			string	systemName)
		{
			this.id                 = id;
			this.name               = name;
			this.bytesPerCharacter  = bytesPerCharacter;
			this.systemName         = systemName;
		}

		#endregion

		#region Methods

		/*
		public int GetByteCount(string s)
		{
			return this.encoding.GetByteCount(s);
		}
		*/

		public byte[] GetBytes(string s)
		{
			return this.GetEncoding().GetBytes(s);
		}

		public int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return this.GetEncoding().GetBytes(s, charIndex, charCount, bytes, byteIndex);
		}	

		public string GetString(byte[] buffer)
		{
			return this.GetString(buffer, 0, buffer.Length);
		}

		public string GetString(byte[] buffer, int index, int count)
		{
			return this.GetEncoding().GetString(buffer, index, count);
		}

		#endregion

		#region Private Methods

		private Encoding GetEncoding()
		{
			switch (this.systemName)
			{
				case "NONE":
					return Encoding.Default;

				case "OCTETS":
					return new BinaryEncoding();

				default:
					return Encoding.GetEncoding(this.systemName);
			}			
		}

		#endregion

		#region Static Methods

		public static void InitializeSupportedCharsets()
		{
			Charset.supportedCharsets = new CharsetCollection();

			// NONE
			Charset.AddCharset(0, "NONE", 1, "NONE");
			// OCTETS
			Charset.AddCharset(1, "OCTETS", 1, "OCTETS");
			// American Standard Code for Information Interchange	
			Charset.AddCharset(2, "ASCII", 1, "ascii");
			// Eight-bit Unicode Transformation Format
			Charset.AddCharset(3, "UNICODE_FSS", 3, "UTF-8");
			// Shift-JIS, Japanese
			Charset.AddCharset(5, "SJIS_0208", 2, "shift_jis");
			// JIS X 0201, 0208, 0212, EUC encoding, Japanese
			Charset.AddCharset(6, "EUCJ_0208", 2, "euc-jp");
			// Windows Japanese	
			Charset.AddCharset(7, "ISO2022-JP", 2, "iso-2022-jp");
			// MS-DOS United States, Australia, New Zealand, South Africa	
			Charset.AddCharset(10, "DOS437", 1, "IBM437");
			// MS-DOS Latin-1				
			Charset.AddCharset(11, "DOS850", 1, "ibm850");
			// MS-DOS Nordic	
			Charset.AddCharset(12, "DOS865", 1, "IBM865");
			// MS-DOS Portuguese	
			Charset.AddCharset(13, "DOS860", 1, "IBM860");
			// MS-DOS Canadian French	
			Charset.AddCharset(14, "DOS863", 1, "IBM863");
			// ISO 8859-1, Latin alphabet No. 1
			Charset.AddCharset(21, "ISO8859_1", 1, "iso-8859-1");
			// ISO 8859-2, Latin alphabet No. 2
			Charset.AddCharset(22, "ISO8859_2", 1, "iso-8859-2");
			// Windows Korean	
			Charset.AddCharset(44, "KSC_5601", 2, "ks_c_5601-1987");
			// MS-DOS Icelandic	
			Charset.AddCharset(47, "DOS861", 1, "ibm861");
			// Windows Eastern European
			Charset.AddCharset(51, "WIN1250", 1, "windows-1250");
			// Windows Cyrillic
			Charset.AddCharset(52, "WIN1251", 1, "windows-1251");
			// Windows Latin-1
			Charset.AddCharset(53, "WIN1252", 1, "windows-1252");
			// Windows Greek
			Charset.AddCharset(54, "WIN1253", 1, "windows-1253");
			// Windows Turkish
			Charset.AddCharset(55, "WIN1254", 1, "windows-1254");
			// Big5, Traditional Chinese
			Charset.AddCharset(56, "BIG_5", 2, "big5");
			// GB2312, EUC encoding, Simplified Chinese	
			Charset.AddCharset(57, "GB_2312", 2, "gb2312");
			// Windows Hebrew
			Charset.AddCharset(58, "WIN1255", 1, "windows-1255");
			// Windows Arabic	
			Charset.AddCharset(59, "WIN1256", 1, "windows-1256");
			// Windows Baltic	
			Charset.AddCharset(60, "WIN1257", 1, "windows-1257");
		}

		private static void AddCharset(
			int		id,
			string	charset,
			int		bytesPerCharacter,
			string	systemName)
		{
			Charset.SupportedCharsets.Add(
					id,
					charset,
					bytesPerCharacter,
					systemName);
		}

		#endregion
	}
}
