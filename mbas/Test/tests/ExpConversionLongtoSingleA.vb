'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionLongtoSingleA
	Sub Main()
		Dim a as Long = 124
		Dim b as Single = CSng(a)
		if b<>124 then 
			Throw New System.Exception("Explicit Conversion of Long to Single has Failed. Expected 124 but got " &b)
		End if		
	End Sub
End Module
