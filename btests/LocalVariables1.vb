' In all ambiguous situations, the name resolves to the function rather than the local
' StackOverFlow Exception occurs

Imports System

Module LocalVariables1

    Function f1() As Integer()
        f1(0) = 1
        f1(1) = 2
        Dim x As Integer = f1(0)
    End Function

    Sub main()
        Dim b() As Integer = f1()
        Console.WriteLine("{0}  {1}", b(0), b(1))
    End Sub

End Module