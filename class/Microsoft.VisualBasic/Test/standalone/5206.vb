Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5206.txt", OpenMode.Output)
			Dim a As Boolean = True
			WriteLine (1, a)
			FileClose (1)

		'End Code
	End Function
End Class
