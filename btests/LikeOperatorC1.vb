' BC30201: Expression expected 
' BC30512: Option Strict On disallows implicit conversions from 'Integer' to 'String'

Option Strict On

Imports System

Module LikeOperatorC1
    Sub main()

        Dim a As Boolean

        a = "HELLO" Like 
        If a <> True Then
            Console.WriteLine("#A1-LikeOperator:Failed")
        End If

        a =  Like "H*O"
        If a <> True Then
            Console.WriteLine("#A2-LikeOperator:Failed")
        End If

        a = 123 Like 123

    End Sub

End Module
