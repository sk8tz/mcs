REM LineNo: 17
REM ExpectedError: BC30302
REM ErrorMessage: Type character '@' cannot be used in a declaration with an explicit type.

Imports System
Module DecimalTypeCharTest
    Sub Main()
	Dim a As Decimal
	a@=10.25

	Dim b@
	b=20.24

	Dim c As Decimal
	c@=23.45

	Dim d@ As Decimal
	d=45.56
    End Sub
End Module
