// 
// OciTransactionHandle.cs 
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
	internal sealed class OciTransactionHandle : OciHandle, IDisposable
	{
		#region Fields

		bool disposed = false;
		OciErrorHandle errorHandle;
		OciServiceHandle serviceHandle;

		#endregion // Fields

		#region Constructors

		public OciTransactionHandle (OciHandle parent, IntPtr handle)
			: base (OciHandleType.Transaction, parent, handle)
		{
		}

		#endregion // Constructors

		#region Properties

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		public OciServiceHandle Service {
			get { return serviceHandle; }
			set { serviceHandle = value; }
		}

		#endregion // Properties

		#region Methods

		[DllImport ("oci")]
		public static extern int OCITransCommit (IntPtr svchp,
							IntPtr errhp,
							uint flags);

		[DllImport ("oci")]
		public static extern int OCITransRollback (IntPtr svchp,
							IntPtr errhp,
							uint flags);

		[DllImport ("oci")]
		public static extern int OCITransStart (IntPtr svchp,
							IntPtr errhp,
							uint timeout,
							[MarshalAs (UnmanagedType.U4)] OciTransactionFlags flags);

		public void AttachToServiceContext ()
		{
			int status = 0;
			status = OciGlue.OCIAttrSet (Service,
							OciHandleType.Service,
							this,
							0,
							OciAttributeType.Transaction,
							ErrorHandle);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public void Begin ()
		{
			int status = 0;

			AttachToServiceContext ();

			status = OCITransStart (Service,
						ErrorHandle,
						60,
						OciTransactionFlags.New);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public void Commit ()
		{
			int status = 0;
			AttachToServiceContext ();
			status = OCITransCommit (Service, ErrorHandle, 0);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				base.Dispose (disposing);
			}
		}

		public void Rollback ()
		{
			int status = 0;
			AttachToServiceContext ();
			status = OCITransRollback (Service, ErrorHandle, 0);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		#endregion // Methods
	}
}
