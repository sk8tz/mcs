// cs0619.cs: 'ObsoleteException' is obsolete: ''
// Line: 15

using System;

[System.Obsolete("", true)]
class ObsoleteException: Exception {
}

class MainClass {
        public void Method ()
        {
                try {
                }
                catch (ObsoleteException) {
                }
        }
}