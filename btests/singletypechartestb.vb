Module SingleTypeCharTest
    Sub Main()
        Try
            Dim m As Integer
            m = f(20)
            If m <> 20 Then
                Throw New Exception("IntegerTypeChar: failed")
            End If
            Exit Sub
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub

    Function f!(ByVal param!)
        f! = param
    End Function
End Module
