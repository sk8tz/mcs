Module IntegerLiteralTestC2
    Sub Main()
        Try
            Dim i As Integer
            i = &O9
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
