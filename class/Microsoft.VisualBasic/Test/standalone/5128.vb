Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Dim result As Integer = StrComp (Nothing,Nothing,CompareMethod.Text)
			If result <> 0 Then
				Throw New Exception ("#StrComp01: Expected 0 but got " + result.ToString ())
			End If
			Return result
		'End Code
	End Function
End Class
