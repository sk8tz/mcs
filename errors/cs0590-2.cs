// cs0590.cs: User-defined operators cannot return void
// Line: 5

class SampleClass {
        public static void operator << (SampleClass value, int value2) {
                return null;
        }
}
