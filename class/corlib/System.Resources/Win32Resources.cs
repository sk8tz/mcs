//
// System.Resources/Win32Resources.cs
//
// Author:
//   Zoltan Varga (vargaz@freemail.hu)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Resources {

internal enum Win32ResourceType {
	RT_CURSOR = 1,
	RT_FONT = 8,
	RT_BITMAP = 2,
	RT_ICON = 3,
	RT_MENU = 4,
	RT_DIALOG = 5,
	RT_STRING = 6,
	RT_FONTDIR = 7,
	RT_ACCELERATOR = 9,
	RT_RCDATA = 10,
	RT_MESSAGETABLE = 11,
	RT_GROUP_CURSOR = 12,
	RT_GROUP_ICON = 14,
	RT_VERSION = 16,
	RT_DLGINCLUDE = 17,
	RT_PLUGPLAY = 19,
	RT_VXD = 20,
	RT_ANICURSOR = 21,
	RT_ANIICON = 22,
	RT_HTML = 23,
}

//
// This class represents the contents of a VS_VERSIONINFO structure and its
// children.
//
internal class Win32VersionResource {

	public string[] WellKnownProperties = {
		"Comments",
		"CompanyName",
		"FileDescription",
		"FileVersion",
		"InternalName",
		"LegalCopyright",
		"LegalTrademarks",
		"OriginalFilename",
		"ProductName",
		"ProductVersion"
	};

	long signature;
	int struct_version;
	long file_version;
	long product_version;
	int file_flags_mask;
	int file_flags;
	int file_os;
	int file_type;
	int file_subtype;
	long file_date;

	Hashtable properties;

	public Win32VersionResource () {
		// Initialize non-public members to the usual values used in
		// resources
		signature = 0xfeef04bd;
		struct_version = 1 << 16; /* 1.0 */
		file_flags_mask = 63;
		file_flags = 0;
		file_os = 4; /* VOS_WIN32 */
		file_type = 2;
		file_subtype = 0;
		file_date = 0;

		properties = new Hashtable ();

		// Well known properties
		foreach (string s in WellKnownProperties)
			// The value of properties can't be empty
			properties [s] = " ";
	}

	public string FileVersion {
		get {
			return 
				"" + (file_version >> 48) + 
				"." + ((file_version >> 32) & 0xffff) + 
				"." + ((file_version >> 16) & 0xffff) +
				"." + ((file_version >> 0) & 0xffff);
		}

		set {
			int[] ver = new int [4] { 0, 0, 0, 0 };
			if (value != null) {
				string[] parts = value.Split ('.');
				
				for (int i = 0; i < parts.Length; ++i) {
					try {
						if (i < ver.Length)
							ver [i] = Int32.Parse (parts [i]);
					}
					catch (FormatException) {
					}
				}
			}

			file_version = (ver [0] << 48) | (ver [1] << 32) | (ver [2] << 16) + ver [3];
		}
	}

	public virtual string this [string key] {
		set {
			properties [key] = value;
		}
	}

	// Accessors for well known properties

	public virtual string Comments {
		get {
			return (string)properties ["Comments"];
		}
		set {
			properties ["Comments"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string CompanyName {
		get {
			return (string)properties ["CompanyName"];
		}
		set {
			properties ["CompanyName"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string LegalCopyright {
		get {
			return (string)properties ["LegalCopyright"];
		}
		set {
			properties ["LegalCopyright"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string LegalTrademarks {
		get {
			return (string)properties ["LegalTrademarks"];
		}
		set {
			properties ["LegalTrademarks"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string OriginalFilename {
		get {
			return (string)properties ["OriginalFilename"];
		}
		set {
			properties ["OriginalFilename"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string ProductName {
		get {
			return (string)properties ["ProductName"];
		}
		set {
			properties ["ProductName"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string ProductVersion {
		get {
			return (string)properties ["ProductVersion"];
		}
		set {
			properties ["ProductVersion"] = value == String.Empty ? " " : value;
		}
	}

	private void emit_padding (BinaryWriter w) {
		MemoryStream ms = (MemoryStream)w.BaseStream;

		if ((ms.Position % 4) != 0)
			w.Write ((short)0);
	}

	private void patch_length (BinaryWriter w, long len_pos) {
		MemoryStream ms = (MemoryStream)w.BaseStream;

		long pos = ms.Position;
		ms.Position = len_pos;
		w.Write ((short)(pos - len_pos));
		ms.Position = pos;
	}

	public void WriteTo (MemoryStream ms)
	{
		using (BinaryWriter w = new BinaryWriter (ms, Encoding.Unicode)) {
			short len;
			long pos;

			//
			// See the documentation for the VS_VERSIONINFO structure and
			// its children on MSDN
			//

			// VS_VERSIONINFO
			w.Write ((short)0);
			w.Write ((short)0x34);
			w.Write ((short)0);
			w.Write ("VS_VERSION_INFO".ToCharArray ());
			w.Write ((short)0);

			emit_padding (w);

			// VS_FIXEDFILEINFO
			w.Write ((uint)signature);
			w.Write ((int)struct_version);
			w.Write ((int)(file_version >> 32));
			w.Write ((int)((file_version & 0xffffffff)));

			w.Write ((int)(product_version >> 32));
			w.Write ((int)(product_version & 0xffffffff));
			w.Write ((int)file_flags_mask);
			w.Write ((int)file_flags);
			w.Write ((int)file_os);
			w.Write ((int)file_type);
			w.Write ((int)file_subtype);
			w.Write ((int)(file_date >> 32));
			w.Write ((int)(file_date & 0xffffffff));

			emit_padding (w);

			// VarFileInfo
			long var_file_info_pos = ms.Position;
			w.Write ((short)0);
			w.Write ((short)0);
			w.Write ((short)1);
			w.Write ("VarFileInfo".ToCharArray ());
			w.Write ((short)0);

			if ((ms.Position % 4) != 0)
				w.Write ((short)0);

			// Var
			long var_pos = ms.Position;
			w.Write ((short)0);
			w.Write ((short)4);
			w.Write ((short)0);
			w.Write ("Translation".ToCharArray ());
			w.Write ((short)0);

			if ((ms.Position % 4) != 0)
				w.Write ((short)0);

			w.Write ((short)0x7f);
			w.Write ((short)1200);

			patch_length (w, var_pos);

			patch_length (w, var_file_info_pos);

			// StringFileInfo
			long string_file_info_pos = ms.Position;
			w.Write ((short)0);
			w.Write ((short)0);
			w.Write ((short)1);
			w.Write ("StringFileInfo".ToCharArray ());

			emit_padding (w);

			// StringTable
			long string_table_pos = ms.Position;
			w.Write ((short)0);
			w.Write ((short)0);
			w.Write ((short)1);
			w.Write ("007f04b0".ToCharArray ());

			emit_padding (w);

			// Strings
			foreach (string key in properties.Keys) {
				string value = (string)properties [key];

				long string_pos = ms.Position;
				w.Write ((short)0);
				w.Write ((short)(value.ToCharArray ().Length + 1));
				w.Write ((short)1);
				w.Write (key.ToCharArray ());
				w.Write ((short)0);

				emit_padding (w);

				w.Write (value.ToCharArray ());
				w.Write ((short)0);

				patch_length (w, string_pos);
			}

			patch_length (w, string_table_pos);

			patch_length (w, string_file_info_pos);

			patch_length (w, 0);
		}
	}
}

}
