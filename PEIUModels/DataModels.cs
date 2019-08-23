using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class UShortRange
    {
        public ushort Max;
        public ushort Min;
        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }


    public class UInt32Range
    {
        public uint Max;
        public uint Min;
        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }

    public class FloatingRange
    {
        public static readonly FloatingRange Empty = new FloatingRange { Max = 0, Min = 0 };
        public float Max;
        public float Min;
        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }

    public class PairPower
    {
        public static readonly PairPower Empty = new PairPower(0, 0);
        public float P;
        public float Q;

        public PairPower() { }
        public PairPower(float p, float q)
        {
            P = p;
            Q = q;
        }
    }

    public class RST
    {
        public static readonly RST Empty = new RST(0, 0, 0);
        public float R;
        public float S;
        public float T;

        public RST() { }
        public RST(float r, float s, float t)
        {
            R = r;
            S = s;
            T = t;
        }

        public override string ToString()
        {
            return $"{R},{S},{T}";
        }
    }
}
