Class A
    Public Overridable Sub F1()
    End Sub
End Class

Class B
    Inherits A
    Public Overrides NotOverridable Sub F1()
    End Sub
End Class

Class D
    Inherits B

    Public Overrides Sub F1()
    End Sub
End Class


Module OverrideC1
    Sub Main()
    End Sub
End Module

