// cs3005-5.cs: Identifier `CLSClass.Event_A' differing only in case is not CLS-compliant
// Compiler options: -warnaserror
// Line: 13

[assembly:System.CLSCompliant(true)]

public delegate void MyDelegate(int i);

public class Base {
        protected event System.ResolveEventHandler Event_a;
}

public class CLSClass: Base {
        public event MyDelegate Event_A;
}
