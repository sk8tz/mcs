//
// System.Random.cs
//
// Author:
//   Bob Smith (bob@thestuff.net)
//
// (C) 2001 Bob Smith.  http://www.thestuff.net
//

using System;

namespace System
{
        public class Random
        {
                private int S = 1;
                private const int A = 16807;
                private const int M = 2147483647;
                private const int Q = 127773;
                private const int R = 2836;
                public const byte MaxValue = 0xFF;
                public const byte MinValue = 0x00;
                public Random()
                {
                        S = (int)DateTime.Now;
                }
                public Random(int Seed)
                {
                        S = Seed;
                }
                public virtual int Next()
                {
                        return (int)(Random.Sample()*Random.MaxValue);
                }
                public virtual int Next(int maxValue)
                {
                        if (maxValue < Random.MinValue)
                                throw new ArgumentOutOfRangeException("Max value is less then min value.");
                        else if (maxValue == Random.MinValue)
                                return Random.MinValue;
                        return (int)(Random.Sample()*maxValue);
                }
                public virtual int Next(int minValue, int maxValue)
                        if (minValue > maxValue)
                                throw new ArgumentOutOfRangeException("Min value is greater then max value.");
                        else if (minValue == maxValue)
                                return minValue;
                        return (int)(Random.Sample()*maxValue)+minValue;
                }
                public virtual void NextBytes(byte[] buffer)
                {
                        int i, l;
                        if (buffer == NULL)
                                throw ArgumentNullException();
                        l = buffer.GetUpperBound(0);
                        for (i = buffer.GetLowerBound(0); i < l; i++)
                        {
                                buffer[i] = (byte)(Random.Sample()*Random.MaxValue);
                        }
                }
                public virtual double NextDouble()
                {
                        return Random.Sample();
                }
                protected virtual double Sample(){
                        S=A*(S%Q)-R*(S/Q);
                        if(S<0) S+=M;
                        return S/(double)Int32.MaxValue;
                }
        }
}

