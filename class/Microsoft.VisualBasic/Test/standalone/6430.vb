  '
  ' Copyright (c) 2002-2003 Mainsoft Corporation.
  '
  ' Permission is hereby granted, free of charge, to any person obtaining a
  ' copy of this software and associated documentation files (the "Software"),
  ' to deal in the Software without restriction, including without limitation
  ' the rights to use, copy, modify, merge, publish, distribute, sublicense,
  ' and/or sell copies of the Software, and to permit persons to whom the
  ' Software is furnished to do so, subject to the following conditions:
  ' 
  ' The above copyright notice and this permission notice shall be included in
  ' all copies or substantial portions of the Software.
  ' 
  ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  ' FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  ' DEALINGS IN THE SOFTWARE.
  '


Imports Microsoft.VisualBasic.Collection
Imports System.Collections

Public Class TestClass
    Public Function Test() As Integer
        'BeginCode
        Dim oDT1 As Integer = 10
        Dim oDT2 As Double = 1.1
        Dim oDT3 As String = "abc"
        Dim oDT4 As String = "def"

        Dim col As New Microsoft.VisualBasic.Collection()

        col.Add(oDT1, Nothing, Nothing, Nothing)
        col.Add(oDT2, Nothing, Nothing, Nothing)
        col.Add(oDT3, Nothing, Nothing, Nothing)
        col.Add(oDT4, Nothing, Nothing, Nothing)
        If col.count <> 4 Then Return 2        col.remove(3)        If col.count <> 3 Then Return 4        '// Collection class is 1-based        If col(3).tostring <> "def" Then Return 8        col.Remove(1)        If col.count <> 2 Then Return 16        If col(1).tostring <> "1.1" Then Return 32        If col(2).tostring <> "def" Then Return 64        col.Remove(2)        If col.count <> 1 Then Return 128        If col(1).tostring <> "1.1" Then Return 256        col.Remove(1)        If col.count <> 0 Then Return 512
        Return 1
    End Function
End Class
