//------------------------------------------------------------------------------
// 
// System.UIntPtr.cs 
//
// Copyright (C) 2001 Michael Lambert, All Rights Reserved
// 
// Author:          Michael Lambert, michaellambert@email.com
// Created:         Thu 07/31/2001 
//
// Remarks:         Requires '/unsafe' compiler option.  This class uses void*,
//                  ulong, and uint in overloaded constructors, conversion, and 
//                  cast members in the public interface.  Using pointers is not 
//                  valid CLS and the methods in question have been marked with  
//                  the CLSCompliant attribute that avoid compiler warnings.
//
//------------------------------------------------------------------------------

using System.Runtime.Serialization;
using System.Runtime.InteropServices;

[
    assembly: System.CLSCompliant(true)
] 

namespace System
{

[
    StructLayout(LayoutKind.Auto),
    CLSCompliant(true)
]
public unsafe struct UIntPtr : ISerializable
{
    public static readonly UIntPtr Zero = new UIntPtr(0);
    private void* _pointer;

    [
        CLSCompliant(false)
    ]
    public UIntPtr(ulong value)
    {
        _pointer = (void*) value;
    }
    
    [
        CLSCompliant(false)
    ]
    public UIntPtr(uint value)
    {
        _pointer = (void*)value;
    }

    [
        CLSCompliant(false)
    ]
    public unsafe UIntPtr(void* value)
    {
        _pointer = value;
    }

    public override bool Equals(object obj)
    {
        if( obj is UIntPtr )
        {
            UIntPtr obj2 = (UIntPtr)obj;
            return this._pointer == obj2._pointer;
        }   

        return false;
    }
    public override int GetHashCode()
    {
        return (int)_pointer;
    }
    
    [
        CLSCompliant(false)
    ]
    public uint ToUInt32()
    {
        return (uint) _pointer;
    }

    [
        CLSCompliant(false)
    ]
    public ulong ToUInt64()
    {
        return (ulong) _pointer;
    }

    [
        CLSCompliant(false)
    ]
    public unsafe void* ToPointer()
    {
        return _pointer;
    }
    public override string ToString()
    {
        return ((uint) _pointer).ToString();
    }

    // Interface ISerializable
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if( info == null )
            throw new ArgumentNullException( "info" );
        // if( context == null ) -- context is struct can't be null.
        //    throw new ArgumentNullException( "context" );
        
        info.AddValue("pointer", (ulong)_pointer);
    }

    public static bool operator ==(UIntPtr value1, UIntPtr value2)
    {
        return value1._pointer == value2._pointer;
    }
    public static bool operator !=(UIntPtr value1, UIntPtr value2)
    {
        return value1._pointer != value2._pointer;
    }

    [
        CLSCompliant(false)
    ]
    public static explicit operator ulong(UIntPtr value)
    {
        return (ulong)value._pointer;
    }

    [
        CLSCompliant(false)
    ]
    public static explicit operator uint(UIntPtr value)
    {
        return (uint)value._pointer;
    }
    
    [
        CLSCompliant(false)
    ]
    public static explicit operator UIntPtr(ulong value)
    {
        return new UIntPtr(value);
    }

    [
        CLSCompliant(false)
    ]
    public unsafe static explicit operator UIntPtr(void* value)
    {
        return new UIntPtr(value);
    }

    [
        CLSCompliant(false)
    ]
    public unsafe static explicit operator void*(UIntPtr value)
    {
        return value.ToPointer();
    }
    
    [
        CLSCompliant(false)
    ]
    public static explicit operator UIntPtr(uint value)
    {
        return new UIntPtr(value);
    }

    public static int Size
    {
        get
        {   
            return sizeof(void*); 
        }
    }
}

} // Namespace

