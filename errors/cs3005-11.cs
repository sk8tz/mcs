// cs3005-11.cs: Identifier `CLSInterface' differing only in case is not CLS-compliant
// Line: 10

using System;
[assembly:CLSCompliant (true)]

public interface CLSInterface {
}

public class clsInterface: CLSInterface {
}