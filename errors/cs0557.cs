// cs0557.cs: Duplicate user-defined conversion in class 'SampleClass'
// Line: 5

class SampleClass {
        public static implicit operator bool (SampleClass value) {
                return true;
        }
        
        public static implicit operator bool (SampleClass value) {
                return true;
        }
}
