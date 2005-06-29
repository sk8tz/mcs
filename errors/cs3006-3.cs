// cs3006-3.cs: Overloaded method `CLSInterface.Test(int)' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 9

using System;
[assembly: CLSCompliant(true)]

public interface CLSInterface {
        void Test(int a);
        void Test(ref int b);
}
