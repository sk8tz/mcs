
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
using System.Data;
using System.Runtime.InteropServices;
using System.Text;

namespace IBM.Data.DB2
{
	/// <summary>
	/// DB2Prototypes class is a wrapper for the DB2.lib, IBM's Call Level Interface to DB2
	/// </summary>
	/// 
	#region comented
	/*internal class DB2CLIWrapper
	{
		
		#if (OS_LINUX)
			private const string libname = "db2_36";
		#endif
		
		#if (OS_WINDOWS)
			private const string libname = "db2cli";
		#endif
		
		[DllImport("db2cli", EntryPoint = "SQLAllocHandle")]
			internal static extern short SQLAllocHandle(short handleType, IntPtr inputHandle,  ref IntPtr outputHandle);

		[DllImport("db2cli", EntryPoint = "SQLFreeHandle")]
		internal static extern short SQLFreeHandle(short handleType, IntPtr inputHandle);

		[DllImport("db2cli", EntryPoint = "SQLGetConnectAttrW")]
		internal static extern short SQLGetConnectAttr(IntPtr ConnectionHandle, int Attribute, [Out] IntPtr ValuePtr, int BufferLength, out int StringLengthPtr);

		[DllImport("db2cli", EntryPoint = "SQLGetConnectAttrW")]
		internal static extern short SQLGetConnectAttr(IntPtr ConnectionHandle, int Attribute, out int Value, int BufferLength, IntPtr Zero);

		[DllImport("db2cli", EntryPoint = "SQLFreeStmt")]
		internal static extern short SQLFreeStmt(IntPtr StatementHandle, short option);

		[DllImport("db2cli", EntryPoint = "SQLConnect")]
			internal static extern short SQLConnect(IntPtr sqlHdbc, string serverName, short serverNameLength, string userName, short userNameLength, string authentication, short authenticationLength);
		[DllImport("db2cli", EntryPoint = "SQLColAttribute")]
			internal static extern short SQLColAttribute(IntPtr StatementHandle, short ColumnNumber, short FieldIdentifier, IntPtr CharacterAttribute, short BufferLength,  ref short StringLength,  ref int NumericAttribute);
		//[DllImport("db2cli", EntryPoint = "SQLGetData")]
		//	internal static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, IntPtr TargetPtr, IntPtr BufferLength, ref IntPtr StrLen_or_Ind);
		[DllImport("db2cli", EntryPoint="SQLMoreResults")]
			internal static extern short SQLMoreResults(IntPtr StatementHandle);

		//[DllImport("db2cli", EntryPoint = "SQLGetData")]
		//internal static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, IntPtr TargetPtr, IntPtr BufferLength, out int StrLen_or_Ind);
		[DllImport("db2cli", EntryPoint = "SQLGetData")]
		internal static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, IntPtr TargetPtr, int BufferLength, out int StrLen_or_Ind);
		
			
		//[DllImport("db2cli", EntryPoint = "SQLGetData")]
		//internal static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, byte[] TargetPtr, IntPtr BufferLength, ref IntPtr StrLen_or_Ind);
		
		[DllImport("db2cli", EntryPoint = "SQLGetData")]
		internal static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, byte[] TargetPtr, int BufferLength, out int StrLen_or_Ind);

		[DllImport("db2cli", EntryPoint = "SQLGetData")]
		internal static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, [Out] StringBuilder sb, int BufferLength, out int StrLen_or_Ind);

		[DllImport("db2cli", EntryPoint = "SQLColAttributeW")]
		internal static extern short SQLColAttribute(IntPtr StatementHandle, short ColumnNumber, short FieldIdentifier, [Out] StringBuilder CharacterAttribute, short BufferLength,  out short StringLength, out int NumericAttribute);
	
		[DllImport("db2cli", CharSet = CharSet.Auto, EntryPoint = "SQLDisconnect")]
			internal static extern short SQLDisconnect(IntPtr sqlHdbc);
		[DllImport("db2cli", EntryPoint = "SQLGetDiagRec")]
			internal static extern short SQLGetDiagRec( short handleType, IntPtr handle, short recNum, [Out] StringBuilder sqlState, ref IntPtr nativeErrorPtr, [Out] StringBuilder errorMessage, short bufferLength, ref IntPtr shortTextLengthPtr);
		[DllImport("db2cli", EntryPoint = "SQLSetConnectAttr")]
			internal static extern short SQLSetConnectAttr(IntPtr sqlHdbc, long sqlAttr, [In] IntPtr sqlValuePtr, long sqlValueLength);
		[DllImport("db2cli", EntryPoint = "SQLSetStmtAttr")]
			internal static extern short SQLSetStmtAttr(IntPtr sqlHstmt, long sqlAttr, [In] IntPtr sqlValuePtr, long sqlValueLength);
		[DllImport("db2cli", EntryPoint = "SQLEndTran")]
			internal static extern short SQLEndTran (short handleType, IntPtr handle, short fType);
		[DllImport("db2cli", EntryPoint = "SQLCancel")]
			internal static extern short SQLCancel(IntPtr handle);
		[DllImport("db2cli", EntryPoint = "SQLNumResultCols")]
			internal static extern short SQLNumResultCols(IntPtr handle, ref int numCols);
		[DllImport("db2cli", EntryPoint = "SQLFetch")]
			internal static extern short SQLFetch(IntPtr handle);
		[DllImport("db2cli", EntryPoint = "SQLRowCount")]
			internal static extern short SQLRowCount(IntPtr stmtHandle, ref int numRows);
		[DllImport("db2cli", EntryPoint = "SQLExecute")]
			internal static extern short SQLExecute(IntPtr handle);
		[DllImport ("db2cli", EntryPoint = "SQLExecDirect")]
			internal static extern short SQLExecDirect(IntPtr stmtHandle, string stmt, int length);
		[DllImport("db2cli", EntryPoint = "SQLDescribeCol")]
			internal static extern short SQLDescribeCol(IntPtr stmtHandle, ushort colNum, StringBuilder colName, short colNameMaxLength, IntPtr colNameLength, ref IntPtr dataType, ref IntPtr colSizePtr, ref IntPtr scalePtr, ref IntPtr nullablePtr );
		[DllImport("db2cli", EntryPoint = "SQLBindCol")]
			internal static extern short SQLBindCol(IntPtr StatementHandle, short ColumnNumber, short TargetType, IntPtr TargetValue, IntPtr BufferLength, ref IntPtr StrLen_or_Ind); 
		[DllImport("db2cli", EntryPoint = "SQLDriverConnect")]
			internal static extern short SQLDriverConnect(IntPtr hdbc, int centered, [In] string inConnectStr, [In] int inStrLength, [Out] StringBuilder outConnectStr, [Out] int outStrCapacity, [Out] IntPtr outStrLengthReturned, [In] int completion);
		[DllImport("db2cli", EntryPoint = "SQLPrepare")]
			internal static extern short SQLPrepare(IntPtr stmtHandle, string stmt, int length);
		[DllImport("db2cli", EntryPoint = "SQLDescribeParam")]
			internal static extern short SQLDescribeParam(IntPtr stmtHandle, short paramNumber,ref IntPtr dataType, ref IntPtr paramSize, ref IntPtr decimalDigits, ref IntPtr nullable);
		[DllImport("db2cli", EntryPoint = "SQLNumParams")]
			internal static extern short SQLNumParams(IntPtr stmtHandle, ref IntPtr numParams);
		
		[DllImport("db2cli")]
			internal static extern short SQLBindParameter(IntPtr stmtHandle, ushort paramNumber, 
			short dataType, short valueType, short paramType, uint colSize, short decDigits, 
			 IntPtr dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);
			
		[DllImport("db2cli")]
			internal static extern short SQLBindParameter(IntPtr stmtHandle, ushort paramNumber, 
			short dataType, short valueType, short paramType, uint colSize, short decDigits, 
			byte[] dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);
						
		[DllImport("db2cli", EntryPoint = "SQLBindParameter")]
			internal static extern short SQLBindParameter(IntPtr stmtHandle, ushort paramNumber, 
			short dataType, short valueType, short paramType, uint colSize, short decDigits,
			ref int dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);
	
		[DllImport("db2cli", EntryPoint = "SQLBindParameter")]
			internal static extern short SQLBindParameter(IntPtr stmtHandle, ushort paramNumber,
			short dataType, short valueType, short paramType, uint colSize, short decDigits,
			ref double dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);
				
		[DllImport("db2cli", EntryPoint = "SQLDescribeParam")]
			internal static extern short SQLDescribeParam(IntPtr stmtHandle, short ParameterNumber, IntPtr DataTypePtr, IntPtr ParameterSizePtr, IntPtr DecimalDigitsPtr, IntPtr NullablePtr); 
		
		[DllImport("db2cli", EntryPoint = "SQLGetLength")]
			internal static extern short SQLGetLength( IntPtr stmtHandle, short locatorCType, int Locator,
			IntPtr stringLength, IntPtr indicatorValue);
		[DllImport("db2cli", EntryPoint = "SQLGetPosition")]
			internal static extern short SQLGetPosition(IntPtr stmtHandle, short locatorCType, int sourceLocator, int searchLocator, 
			string searchLiteral, int searchLiteralLength, uint fromPosition, IntPtr locatedAt, IntPtr indicatorValue);
		[DllImport("db2cli", EntryPoint = "SQLGetPosition")]
		    internal static extern short SQLBindFileToCol (IntPtr stmtHandle, ushort colNum, string fileName, IntPtr fileNameLength, 
			IntPtr fileOptions, short maxFileNameLength, IntPtr stringLength, IntPtr indicatorValue);
		[DllImport("db2cli", EntryPoint = "SQLGetPosition")]
		    internal static extern short SQLBindFileToParam (IntPtr stmtHandle, ushort targetType, short dataType, string fileName,
			IntPtr fileNameLength, short maxFileNameLength, IntPtr indicatorValue);	
			

	}*/
	#endregion

	internal class DB2CLIWrapper
	{
		static bool useLibCli;

		static public short Initialize(ref IntPtr pEnvHandle)
		{
			string OSVersion = Environment.OSVersion.ToString();
			useLibCli = true;
			if(OSVersion.Substring(0,4)=="Unix"){
				useLibCli = false;
			}
		 	return DB2CLIWrapper.SQLAllocHandle(DB2Constants.SQL_HANDLE_ENV, IntPtr.Zero, out pEnvHandle);
		}

		static public short SQLAllocHandle(short handleType, IntPtr inputHandle, out IntPtr outputHandle)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLAllocHandle(handleType, inputHandle, out outputHandle);
			return StaticWrapper36.SQLAllocHandle(handleType, inputHandle, out outputHandle);
		}
		static public short SQLFreeHandle(short handleType, IntPtr inputHandle)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLFreeHandle(handleType, inputHandle);
			return StaticWrapper36.SQLFreeHandle(handleType, inputHandle);
		}
		static public short SQLFreeStmt(IntPtr StatementHandle, short option)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLFreeStmt(StatementHandle, option);
			return StaticWrapper36.SQLFreeStmt(StatementHandle, option);
		}
		static public short SQLConnect(IntPtr sqlHdbc, string serverName, short serverNameLength, string userName, short userNameLength, string authentication, short authenticationLength)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLConnect(sqlHdbc, serverName, serverNameLength, userName, userNameLength, authentication, authenticationLength);
			return StaticWrapper36.SQLConnect(sqlHdbc, serverName, serverNameLength, userName, userNameLength, authentication, authenticationLength);
		}
		static public short SQLColAttribute(IntPtr StatementHandle, short ColumnNumber, short FieldIdentifier, StringBuilder CharacterAttribute, short BufferLength, out short StringLength, out int NumericAttribute)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLColAttribute(StatementHandle, ColumnNumber, FieldIdentifier, CharacterAttribute, BufferLength, out StringLength, out NumericAttribute);
			return StaticWrapper36.SQLColAttribute(StatementHandle, ColumnNumber, FieldIdentifier, CharacterAttribute, BufferLength, out StringLength, out NumericAttribute);
		}
		static public short SQLGetConnectAttr(IntPtr ConnectionHandle, int Attribute, IntPtr ValuePtr, int BufferLength, out int StringLengthPtr)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLGetConnectAttr(ConnectionHandle, Attribute, ValuePtr, BufferLength, out StringLengthPtr);
			return StaticWrapper36.SQLGetConnectAttr(ConnectionHandle, Attribute, ValuePtr, BufferLength, out StringLengthPtr);
		}
		static public short SQLGetConnectAttr(IntPtr ConnectionHandle, int Attribute, out int Value, int BufferLength, IntPtr Zero)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLGetConnectAttr(ConnectionHandle, Attribute, out Value, BufferLength, Zero);
			return StaticWrapper36.SQLGetConnectAttr(ConnectionHandle, Attribute, out Value, BufferLength, Zero);
		}
		static public short SQLColAttribute(IntPtr StatementHandle, short ColumnNumber, short FieldIdentifier, IntPtr CharacterAttribute, short BufferLength, ref short StringLength, ref int NumericAttribute)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLColAttribute(StatementHandle, ColumnNumber, FieldIdentifier, CharacterAttribute, BufferLength, ref StringLength, ref NumericAttribute);
			return StaticWrapper36.SQLColAttribute(StatementHandle, ColumnNumber, FieldIdentifier, CharacterAttribute, BufferLength, ref StringLength, ref NumericAttribute);
		}
		static public short SQLMoreResults(IntPtr StatementHandle)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLMoreResults(StatementHandle);
			return StaticWrapper36.SQLMoreResults(StatementHandle);
		}
		static public short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, StringBuilder sb, int BufferLength, out int StrLen_or_Ind)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLGetData(StatementHandle, ColumnNumber, TargetType, sb, BufferLength, out StrLen_or_Ind);
			return StaticWrapper36.SQLGetData(StatementHandle, ColumnNumber, TargetType, sb, BufferLength, out StrLen_or_Ind);
		}
		static public short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, IntPtr TargetPtr, int BufferLength, out int StrLen_or_Ind)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLGetData(StatementHandle, ColumnNumber, TargetType, TargetPtr, BufferLength, out StrLen_or_Ind);
			return StaticWrapper36.SQLGetData(StatementHandle, ColumnNumber, TargetType, TargetPtr, BufferLength, out StrLen_or_Ind);
		}
		static public short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, byte[] TargetPtr, int BufferLength, out int StrLen_or_Ind)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLGetData(StatementHandle, ColumnNumber, TargetType, TargetPtr, BufferLength, out StrLen_or_Ind);
			return StaticWrapper36.SQLGetData(StatementHandle, ColumnNumber, TargetType, TargetPtr, BufferLength, out StrLen_or_Ind);
		}
		static public short SQLDisconnect(IntPtr sqlHdbc)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLDisconnect(sqlHdbc);
			return StaticWrapper36.SQLDisconnect(sqlHdbc);
		}
		static public short SQLGetDiagRec( short handleType, IntPtr handle, short recNum, StringBuilder sqlState, out int nativeError, StringBuilder errorMessage, int bufferLength, out short textLengthPtr)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLGetDiagRec( handleType, handle, recNum, sqlState, out nativeError, errorMessage, bufferLength, out textLengthPtr);
			return StaticWrapper36.SQLGetDiagRec( handleType, handle, recNum, sqlState, out nativeError, errorMessage, bufferLength, out textLengthPtr);
		}
		static public short SQLSetConnectAttr(IntPtr sqlHdbc, int sqlAttr, IntPtr sqlValuePtr, int sqlValueLength)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLSetConnectAttr(sqlHdbc, sqlAttr, sqlValuePtr, sqlValueLength);
			return StaticWrapper36.SQLSetConnectAttr(sqlHdbc, sqlAttr, sqlValuePtr, sqlValueLength);
		}
		static public short SQLSetStmtAttr(IntPtr sqlHdbc, int sqlAttr, IntPtr sqlValuePtr, int sqlValueLength)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLSetStmtAttr(sqlHdbc, sqlAttr, sqlValuePtr, sqlValueLength);
			return StaticWrapper36.SQLSetStmtAttr(sqlHdbc, sqlAttr, sqlValuePtr, sqlValueLength);
		}

		//for bulk operations
		static public short SQLSetStmtAttr(IntPtr sqlHdbc, int sqlAttr, ushort[] sqlValuePtr, int sqlValueLength)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLSetStmtAttr(sqlHdbc, sqlAttr, sqlValuePtr, sqlValueLength);
			return StaticWrapper36.SQLSetStmtAttr(sqlHdbc, sqlAttr, sqlValuePtr, sqlValueLength);
		}

		static public short SQLEndTran (short handleType, IntPtr handle, short fType)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLEndTran (handleType, handle, fType);
			return StaticWrapper36.SQLEndTran (handleType, handle, fType);
		}
		static public short SQLCancel(IntPtr handle)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLCancel(handle);
			return StaticWrapper36.SQLCancel(handle);
		}
		static public short SQLNumResultCols(IntPtr handle, out short numCols)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLNumResultCols(handle, out numCols);
			return StaticWrapper36.SQLNumResultCols(handle, out numCols);
		}
		static public short SQLFetch(IntPtr handle)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLFetch(handle);
			return StaticWrapper36.SQLFetch(handle);
		}
		static public short SQLRowCount(IntPtr stmtHandle, out int numRows)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLRowCount(stmtHandle, out numRows);
			return StaticWrapper36.SQLRowCount(stmtHandle, out numRows);
		}
		static public short SQLExecute(IntPtr handle)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLExecute(handle);
			return StaticWrapper36.SQLExecute(handle);
		}
		static public short SQLExecDirect(IntPtr stmtHandle, string stmt, int length)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLExecDirect(stmtHandle, stmt, length);
			return StaticWrapper36.SQLExecDirect(stmtHandle, stmt, length);
		}
		static public short SQLDriverConnect(IntPtr hdbc, IntPtr windowHandle, string inConnectStr, short inStrLength, StringBuilder outConnectStr, short outStrCapacity, out short outStrLengthReturned, int completion)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLDriverConnect(hdbc, windowHandle, inConnectStr, inStrLength, outConnectStr, outStrCapacity, out outStrLengthReturned, completion);
			return StaticWrapper36.SQLDriverConnect(hdbc, windowHandle, inConnectStr, inStrLength, outConnectStr, outStrCapacity, out outStrLengthReturned, completion);
		}
		static public short SQLPrepare(IntPtr stmtHandle, string stmt, int length)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLPrepare(stmtHandle, stmt, length);
			return StaticWrapper36.SQLPrepare(stmtHandle, stmt, length);
		}
		static public short SQLBindParameter(IntPtr stmtHandle, short paramNumber, short dataType, short valueType, short paramType, int colSize, short decDigits, IntPtr dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLBindParameter(stmtHandle, paramNumber, dataType, valueType, paramType, colSize, decDigits, dataBufferPtr, dataBufferLength, StrLen_or_IndPtr);
			return StaticWrapper36.SQLBindParameter(stmtHandle, paramNumber, dataType, valueType, paramType, colSize, decDigits, dataBufferPtr, dataBufferLength, StrLen_or_IndPtr);
		}

		static public short SQLBindParameter(IntPtr stmtHandle, short paramNumber, short dataType, short valueType, short paramType, int colSize, short decDigits, int[] dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLBindParameter(stmtHandle, paramNumber, dataType, valueType, paramType, colSize, decDigits, dataBufferPtr, dataBufferLength, StrLen_or_IndPtr);
			return StaticWrapper36.SQLBindParameter(stmtHandle, paramNumber, dataType, valueType, paramType, colSize, decDigits, dataBufferPtr, dataBufferLength, StrLen_or_IndPtr);
		}
		public static short SQLGetInfo(IntPtr sqlHdbc, short fInfoType, [Out] StringBuilder rgbInfoValue, short cbInfoValueMax, out short pcbInfoValue)
		{
			if(useLibCli)
				return StaticWrapperCli.SQLGetInfo(sqlHdbc, fInfoType, rgbInfoValue, cbInfoValueMax, out pcbInfoValue);
			return StaticWrapper36.SQLGetInfo(sqlHdbc, fInfoType, rgbInfoValue, cbInfoValueMax, out pcbInfoValue);
		}

		/// <summary>
		/// db2Prototypes class is a wrapper for the db2.lib, IBM's Call Level Interface to Db2
		/// </summary>
		public class StaticWrapperCli
		{
			private const string libname = "db2cli";
		
			[DllImport(libname, EntryPoint = "SQLAllocHandle")]
			public static extern short SQLAllocHandle(short handleType, IntPtr inputHandle,  out IntPtr outputHandle);
			[DllImport(libname, EntryPoint = "SQLFreeHandle")]
			public static extern short SQLFreeHandle(short handleType, IntPtr inputHandle);

			[DllImport(libname, EntryPoint = "SQLFreeStmt")]
			public static extern short SQLFreeStmt(IntPtr StatementHandle, short option);

			[DllImport(libname, EntryPoint = "SQLConnectW", CharSet=CharSet.Unicode)]

			public static extern short SQLConnect(IntPtr sqlHdbc, string serverName, short serverNameLength, string userName, short userNameLength, string authentication, short authenticationLength);
			[DllImport(libname, EntryPoint = "SQLColAttributeW", CharSet=CharSet.Unicode)]
			public static extern short SQLColAttribute(IntPtr StatementHandle, short ColumnNumber, short FieldIdentifier, [Out] StringBuilder CharacterAttribute, short BufferLength,  out short StringLength, out int NumericAttribute);

			[DllImport(libname, EntryPoint = "SQLGetConnectAttrW", CharSet=CharSet.Unicode)]
			public static extern short SQLGetConnectAttr(IntPtr ConnectionHandle, int Attribute, [Out] IntPtr ValuePtr, int BufferLength, out int StringLengthPtr);

			[DllImport(libname, EntryPoint = "SQLGetConnectAttrW", CharSet=CharSet.Unicode)]
			public static extern short SQLGetConnectAttr(IntPtr ConnectionHandle, int Attribute, out int Value, int BufferLength, IntPtr Zero);

			[DllImport(libname, EntryPoint = "SQLColAttributeW", CharSet=CharSet.Unicode)]
			public static extern short SQLColAttribute(IntPtr StatementHandle, short ColumnNumber, short FieldIdentifier, IntPtr CharacterAttribute, short BufferLength,  ref short StringLength,  ref int NumericAttribute);
			[DllImport(libname, EntryPoint="SQLMoreResults")]
			public static extern short SQLMoreResults(IntPtr StatementHandle);

			[DllImport(libname, EntryPoint = "SQLGetData")]
			public static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, [Out] StringBuilder sb, int BufferLength, out int StrLen_or_Ind);

			[DllImport(libname, EntryPoint = "SQLGetData")]
			public static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, IntPtr TargetPtr, int BufferLength, out int StrLen_or_Ind);
		
			[DllImport(libname, EntryPoint = "SQLGetData")]
			public static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, [Out] byte[] TargetPtr, int BufferLength, out int StrLen_or_Ind);

			[DllImport(libname,  EntryPoint = "SQLDisconnect")]
			public static extern short SQLDisconnect(IntPtr sqlHdbc);

			[DllImport(libname, EntryPoint = "SQLGetDiagRec")]
			public static extern short SQLGetDiagRec( short handleType, IntPtr handle, short recNum, [Out] StringBuilder sqlState, out int nativeError, [Out] StringBuilder errorMessage, int bufferLength, out short textLengthPtr);

			[DllImport(libname, EntryPoint = "SQLSetConnectAttr")]
			public static extern short SQLSetConnectAttr(IntPtr sqlHdbc, int sqlAttr, [In] IntPtr sqlValuePtr, int sqlValueLength);
			
			[DllImport(libname, EntryPoint = "SQLSetStmtAttr")]
			public static extern short SQLSetStmtAttr(IntPtr sqlHdbc, int sqlAttr, [In] IntPtr sqlValuePtr, int sqlValueLength);
			
			//For bulk operations
			[DllImport(libname, EntryPoint = "SQLSetStmtAttr")]
			public static extern short SQLSetStmtAttr(IntPtr sqlHdbc, int sqlAttr, ushort[] sqlValuePtr, int sqlValueLength);

			[DllImport(libname, EntryPoint = "SQLEndTran")]
			public static extern short SQLEndTran (short handleType, IntPtr handle, short fType);
			[DllImport(libname, EntryPoint = "SQLCancel")]
			public static extern short SQLCancel(IntPtr handle);
			[DllImport(libname, EntryPoint = "SQLNumResultCols")]
			public static extern short SQLNumResultCols(IntPtr handle, out short numCols);
			[DllImport(libname, EntryPoint = "SQLFetch")]
			public static extern short SQLFetch(IntPtr handle);
			[DllImport(libname, EntryPoint = "SQLRowCount")]
			public static extern short SQLRowCount(IntPtr stmtHandle, out int numRows);
			[DllImport(libname, EntryPoint = "SQLExecute")]
			public static extern short SQLExecute(IntPtr handle);
			[DllImport (libname, EntryPoint = "SQLExecDirectW", CharSet=CharSet.Unicode)]
			public static extern short SQLExecDirect(IntPtr stmtHandle, string stmt, int length);
			[DllImport(libname, EntryPoint = "SQLDriverConnectW", CharSet=CharSet.Unicode)]
			public static extern short SQLDriverConnect(IntPtr hdbc, IntPtr windowHandle, [In] string inConnectStr, [In] short inStrLength, [Out] StringBuilder outConnectStr, [Out] short outStrCapacity, out short outStrLengthReturned, [In] int completion);
			[DllImport(libname, EntryPoint = "SQLPrepareW", CharSet=CharSet.Unicode)]
			public static extern short SQLPrepare(IntPtr stmtHandle, string stmt, int length);
			[DllImport(libname)]
			public static extern short SQLBindParameter(IntPtr stmtHandle, short paramNumber, 
				short dataType, short valueType, short paramType, int colSize, short decDigits, 
				IntPtr dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);

			[DllImport(libname)]
			public static extern short SQLBindParameter(IntPtr stmtHandle, short paramNumber, 
				short dataType, short valueType, short paramType, int colSize, short decDigits, 
				int[] dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);

			[DllImport(libname, EntryPoint = "SQLGetInfoW", CharSet=CharSet.Unicode)]
			public static extern short SQLGetInfo(IntPtr sqlHdbc, short fInfoType, [Out] StringBuilder rgbInfoValue, short cbInfoValueMax, out short pcbInfoValue);
		}

		public class StaticWrapper36
		{
			private const string libname = "db2_36";
		
			[DllImport(libname, EntryPoint = "SQLAllocHandle")]
			public static extern short SQLAllocHandle(short handleType, IntPtr inputHandle,  out IntPtr outputHandle);
			[DllImport(libname, EntryPoint = "SQLFreeHandle")]
			public static extern short SQLFreeHandle(short handleType, IntPtr inputHandle);

			[DllImport(libname, EntryPoint = "SQLFreeStmt")]
			public static extern short SQLFreeStmt(IntPtr StatementHandle, short option);

			[DllImport(libname, EntryPoint = "SQLConnect", CharSet=CharSet.Ansi)]

			public static extern short SQLConnect(IntPtr sqlHdbc, string serverName, short serverNameLength, string userName, short userNameLength, string authentication, short authenticationLength);
			[DllImport(libname, EntryPoint = "SQLColAttributeW", CharSet=CharSet.Unicode)]
			public static extern short SQLColAttribute(IntPtr StatementHandle, short ColumnNumber, short FieldIdentifier, [Out] StringBuilder CharacterAttribute, short BufferLength,  out short StringLength, out int NumericAttribute);

			[DllImport(libname, EntryPoint = "SQLGetConnectAttrW", CharSet=CharSet.Unicode)]
			public static extern short SQLGetConnectAttr(IntPtr ConnectionHandle, int Attribute, [Out] IntPtr ValuePtr, int BufferLength, out int StringLengthPtr);

			[DllImport(libname, EntryPoint = "SQLGetConnectAttrW", CharSet=CharSet.Unicode)]
			public static extern short SQLGetConnectAttr(IntPtr ConnectionHandle, int Attribute, out int Value, int BufferLength, IntPtr Zero);

			[DllImport(libname, EntryPoint = "SQLColAttributeW", CharSet=CharSet.Unicode)]
			public static extern short SQLColAttribute(IntPtr StatementHandle, short ColumnNumber, short FieldIdentifier, IntPtr CharacterAttribute, short BufferLength,  ref short StringLength,  ref int NumericAttribute);
			[DllImport(libname, EntryPoint="SQLMoreResults")]
			public static extern short SQLMoreResults(IntPtr StatementHandle);

			[DllImport(libname, EntryPoint = "SQLGetData")]
			public static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, [Out] StringBuilder sb, int BufferLength, out int StrLen_or_Ind);

			[DllImport(libname, EntryPoint = "SQLGetData")]
			public static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, IntPtr TargetPtr, int BufferLength, out int StrLen_or_Ind);
		
			[DllImport(libname, EntryPoint = "SQLGetData")]
			public static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, [Out] byte[] TargetPtr, int BufferLength, out int StrLen_or_Ind);

			[DllImport(libname,  EntryPoint = "SQLDisconnect")]
			public static extern short SQLDisconnect(IntPtr sqlHdbc);

			[DllImport(libname, EntryPoint = "SQLGetDiagRec")]
			public static extern short SQLGetDiagRec( short handleType, IntPtr handle, short recNum, [Out] StringBuilder sqlState, out int nativeError, [Out] StringBuilder errorMessage, int bufferLength, out short textLengthPtr);

			[DllImport(libname, EntryPoint = "SQLSetConnectAttr")]
			public static extern short SQLSetConnectAttr(IntPtr sqlHdbc, int sqlAttr, [In] IntPtr sqlValuePtr, int sqlValueLength);
			[DllImport(libname, EntryPoint = "SQLSetStmtAttr")]
			public static extern short SQLSetStmtAttr(IntPtr sqlHdbc, int sqlAttr, [In] IntPtr sqlValuePtr, int sqlValueLength);
	
			//for bulk operations
			[DllImport(libname, EntryPoint = "SQLSetStmtAttr")]
			public static extern short SQLSetStmtAttr(IntPtr sqlHdbc, int sqlAttr, ushort[] sqlValuePtr, int sqlValueLength);

			[DllImport(libname, EntryPoint = "SQLEndTran")]
			public static extern short SQLEndTran (short handleType, IntPtr handle, short fType);
			[DllImport(libname, EntryPoint = "SQLCancel")]
			public static extern short SQLCancel(IntPtr handle);
			[DllImport(libname, EntryPoint = "SQLNumResultCols")]
			public static extern short SQLNumResultCols(IntPtr handle, out short numCols);
			[DllImport(libname, EntryPoint = "SQLFetch")]
			public static extern short SQLFetch(IntPtr handle);
			[DllImport(libname, EntryPoint = "SQLRowCount")]
			public static extern short SQLRowCount(IntPtr stmtHandle, out int numRows);
			[DllImport(libname, EntryPoint = "SQLExecute")]
			public static extern short SQLExecute(IntPtr handle);
			[DllImport (libname, EntryPoint = "SQLExecDirectW", CharSet=CharSet.Unicode)]
			public static extern short SQLExecDirect(IntPtr stmtHandle, string stmt, int length);
			[DllImport(libname, EntryPoint = "SQLDriverConnectW", CharSet=CharSet.Unicode)]
			public static extern short SQLDriverConnect(IntPtr hdbc, IntPtr windowHandle, [In] string inConnectStr, [In] short inStrLength, [Out] StringBuilder outConnectStr, [Out] short outStrCapacity, out short outStrLengthReturned, [In] int completion);
			[DllImport(libname, EntryPoint = "SQLPrepareW", CharSet=CharSet.Unicode)]
			public static extern short SQLPrepare(IntPtr stmtHandle, string stmt, int length);
			[DllImport(libname)]
			public static extern short SQLBindParameter(IntPtr stmtHandle, short paramNumber, 
				short dataType, short valueType, short paramType, int colSize, short decDigits, 
				IntPtr dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);

			[DllImport(libname)]
			public static extern short SQLBindParameter(IntPtr stmtHandle, short paramNumber, 
				short dataType, short valueType, short paramType, int colSize, short decDigits, 
				int[] dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);

			[DllImport(libname, EntryPoint = "SQLGetInfoW", CharSet=CharSet.Unicode)]
			public static extern short SQLGetInfo(IntPtr sqlHdbc, short fInfoType, [Out] StringBuilder rgbInfoValue, short cbInfoValueMax, out short pcbInfoValue);

		}
	}
}
