// cs3005-18.cs: Identifier `B.TEST()' differing only in case is not CLS-compliant
// Compiler options: -warnaserror
// Line: 15

using System;

[assembly: CLSCompliantAttribute (true)]

public class A
{
    [CLSCompliant (false)]
    public void Test () {}
        
    public void test () {}
}

public class B: A
{
    public void TEST () {} 
}
