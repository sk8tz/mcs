Module DecimalTypeCharTest
    Sub Main()
        Try
            Dim m As Decimal
            m = f(20.2)
            If m <> 20.2 Then
                Throw New Exception("DecimalTypeCharTest: failed")
            End If
            Exit Sub
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub

    Function f@(ByVal param@)
        f = param
    End Function
End Module
