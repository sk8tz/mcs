// 
// OciEnvironmentHandle.cs 
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

namespace System.Data.OracleClient.Oci {
	internal class OciEnvironmentHandle : OciHandle, IDisposable
	{
		#region Constructors

		public OciEnvironmentHandle ()
			: this (OciEnvironmentMode.Default)
		{ 
		}

		public OciEnvironmentHandle (OciEnvironmentMode mode)
			: base (OciHandleType.Environment, null, IntPtr.Zero)
		{
			IntPtr newHandle = IntPtr.Zero;
			OciCalls.OCIEnvCreate (out newHandle, 
						mode, 
						IntPtr.Zero, 
						IntPtr.Zero, 
						IntPtr.Zero, 
			 			IntPtr.Zero, 
						0, 
						IntPtr.Zero);

			SetHandle (newHandle);
		}

		#endregion // Constructors

		#region Methods

		public OciErrorInfo HandleError ()
		{
			int errbufSize = 512;
			IntPtr errbuf = Marshal.AllocHGlobal (errbufSize);

			OciErrorInfo info;
			info.ErrorCode = 0;
			info.ErrorMessage = String.Empty;

			OciCalls.OCIErrorGet (Handle,
					1,
					IntPtr.Zero,
					out info.ErrorCode,
					errbuf,
					(uint) errbufSize,
					OciHandleType.Environment);

			object err = Marshal.PtrToStringAnsi (errbuf);
			if (err != null) {
				string errmsg = (string) err;
				info.ErrorMessage = String.Copy (errmsg);
				Marshal.FreeHGlobal (errbuf);
			}

			return info;
		}

		#endregion // Methods
	}
}
