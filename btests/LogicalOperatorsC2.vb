'BC30201: Expression expected

Imports System

Module LogicalOperatorsC2
    Sub main()

        Dim a As Boolean = True
        Dim b As Boolean = False
        Dim c As Boolean
        c = a And
        c =  And b
        If a And b Then
            a = False
        End If
    End Sub
End Module