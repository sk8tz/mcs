// cs1540-2.cs : Cannot access protected member `A.X()' via a qualifier of type `B'; the qualifier must be of type `C' (or derived from it)
// line: 21

class A
{
        protected virtual void X ()
        {
        }
}
 
class B : A
{
}
 
class C : A
{
        static B b = new B ();
 
        static void M ()
        {
                b.X ();
        }
}