'BC30012: '#If' must end with a matching '#End If'

Imports System
Module ConditionalCompilation

#If True
	Sub Main()
		Console.WriteLine("Hello World 1")
	End Sub
#Else If False
	Sub R()
		Console.WriteLine("Hello World 2")
	End Sub
End Module
