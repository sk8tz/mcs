// CS3014: `I' cannot be marked as CLS-compliant because the assembly is not marked as CLS-compliant
// Line: 8
// Compiler options: -warnaserror -warn:1

using System;

[CLSCompliant (true)]
public interface I {
}
