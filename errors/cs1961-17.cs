// CS1961: The contravariant type parameter `T' must be covariantly valid on `B<T>'
// Line: 8

interface A<out T>
{
}

interface B<in T> : A<T>
{
}
