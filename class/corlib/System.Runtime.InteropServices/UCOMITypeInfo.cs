
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

// System.Runtime.InteropServices.UCOMITypeInfo.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;

namespace System.Runtime.InteropServices
{

	[Guid("00020401-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMITypeInfo {
		void AddressOfMember (int memid, INVOKEKIND invKind, out IntPtr ppv); 
		void CreateInstance (object pUnkOuter, ref Guid riid, out object ppvObj);
		void GetContainingTypeLib (out UCOMITypeLib ppTLB, out int pIndex);
		void GetDllEntry (int memid, INVOKEKIND invKind, out string pBstrDllName, out string pBstrName, out short pwOrdinal);
		void GetDocumentation (int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
		void GetFuncDesc (int index, out IntPtr ppFuncDesc);
		void GetIDsOfNames ([In] string[] rgszNames, int cNames, [Out] int[] pMemId);
		void GetImplTypeFlags (int index, out int pImplTypeFlags);
		void GetMops (int memid, out string pBstrMops);
		void GetNames (int memid, [Out] string[] rgBstrNames, int cMaxNames, out int pcNames);
		void GetRefTypeInfo (int hRef, out UCOMITypeInfo ppTI);
		void GetRefTypeOfImplType (int index, out int href);
		void GetTypeAttr (out IntPtr ppTypeAttr);
		void GetTypeComp (out UCOMITypeComp ppTComp);
		void GetVarDesc (int index, out IntPtr ppVarDesc);
		void Invoke (object pvInstance, int memid, short wFlags, ref DISPPARAMS pDispParams, out object pVarResult, out EXCEPINFO pExcepInfo, out int puArgErr);
		void ReleaseFuncDesc (IntPtr pFuncDesc);
		void ReleaseTypeAttr (IntPtr pTypeAttr);
		void ReleaseVarDesc (IntPtr pVarDesc);
	}
}

