Imports System
Module ConditionalCompilation
	Sub Main()
		Dim value As Integer
		Try
			'Testing #If and #End If Block
			
			#If True
			       value=10 
			#End If
			If value<>10 Then
				Throw New Exception("#A1-Conditional Compilation:Failed ")
			End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
		Try
			#If False
				Throw New Exception("#A2-Conditional Compilation:Failed")")
                	#End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try

	End Sub
End Module

