// 
// OciDefineHandle.cs 
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
// 
// Author: 
//     Tim Coleman <tim@timcoleman.com>
//         
// Copyright (C) Tim Coleman, 2003
// 

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciDefineHandle : IOciHandle, IDisposable
	{
		#region Fields

		IntPtr handle;
		IntPtr value;
		short indicator;
		OracleType type;
		OciDataType ociType;
		int definedSize;
	
		#endregion // Fields

		#region Constructors

		public OciDefineHandle (OciStatementHandle statement, int position)
		{
			int ociTypeInt;
			int status = 0;

			IntPtr parameterHandle = statement.CreateParameterHandle (position);

			status = OciGlue.OCIAttrGetInt32 (parameterHandle,
							(uint) OciDescriptorType.Parameter,
							out definedSize,
							IntPtr.Zero,
							OciAttributeType.DataSize,
							statement.ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = statement.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			status = OciGlue.OCIAttrGetInt32 (parameterHandle,
							(uint) OciDescriptorType.Parameter,
							out ociTypeInt,
							IntPtr.Zero,
							OciAttributeType.DataType,
							statement.ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = statement.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			ociType = (OciDataType) ociTypeInt;

			value = Marshal.AllocHGlobal (definedSize);

			status = OCIDefineByPos (statement.Handle,
							out handle,
							statement.ErrorHandle.Handle,
							position,
							value,
							definedSize,
							ociType,
							ref indicator,
							IntPtr.Zero,
							IntPtr.Zero,
							0);
			if (status != 0) {
				OciErrorInfo info = statement.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			if (value == IntPtr.Zero)
				Console.WriteLine ("BRRRRAAAAP");

			statement.FreeParameterHandle (parameterHandle);
		}

		#endregion // Constructors

		#region Properties

		public int DefinedSize {
			get { return definedSize; }
		}

		public IntPtr Handle {
			get { return handle; }
			set { handle = value; }
		}

		public OciHandleType HandleType {
			get { return OciHandleType.Define; }
		}

		public IntPtr Value {
			get { 
				if (value == IntPtr.Zero)
					Console.WriteLine ("BRRRRAAAAP! EXCEEEUUUUSE ME!");
				return value; 
			}
		}

		#endregion

		#region Methods

		[DllImport ("oci")]
		public static extern int OCIDefineByPos (IntPtr stmtp,
							out IntPtr defnpp,
							IntPtr errhp,
							[MarshalAs (UnmanagedType.U4)] int position,
							IntPtr valuep,
							int value_sz,
							[MarshalAs (UnmanagedType.U2)] OciDataType dty,
							ref short indp,
							IntPtr rlenp,
							IntPtr rcodep,
							uint mode);

		public void Dispose ()
		{
			Marshal.FreeHGlobal (value);
		}

		#endregion // Methods
	}
}
