// CS8035: Covariant type parameters can only be used as return types or in interface inheritance
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

delegate void B<out T> (A<A<T>> a);
