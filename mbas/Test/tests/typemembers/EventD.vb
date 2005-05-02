Imports System

Class C
	Delegate Sub EH(ByVal i as integer)
	Public Event E as EH

	Public Sub S()
		RaiseEvent E(10)
	End Sub
End Class

Class C1
	dim WithEvents x as C = new C()

	Sub call_S()
		x.S()
	End Sub

	Sub EH(i as Integer) Handles x.E
		if i<>10
			throw new System.Exception("#A1 Event call FAils")
		end if
	End Sub
End Class

Module M
	Sub Main()
		dim y as new C1
		y.call_S()
	End Sub
End Module


