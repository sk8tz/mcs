// cs3003-2.cs: Type of `CLSClass.Index' is not CLS-compliant
// Line: 10

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        public ulong Index {
            set
            {
            }
        }
}
