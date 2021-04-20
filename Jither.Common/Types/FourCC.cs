using System;
using System.Collections.Generic;
using System.Text;

namespace Jither.IO.Types
{
    public struct FourCC
    {
        private static readonly string VALID_CHARS = "_ 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public uint NumericValue { get; }
        public string Name { get; }

        public bool IsValid
        {
            get
            {
                string name = Name;
                for (int i = 0; i < 4; i++)
                {
                    if (VALID_CHARS.IndexOf(name[i]) < 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public FourCC(uint value)
        {
            this.Name = Encoding.ASCII.GetString(BitConverter.GetBytes(value));
            this.NumericValue = value;
        }

        public FourCC(string value)
        {
            this.Name = value;
            this.NumericValue = BitConverter.ToUInt32(Encoding.ASCII.GetBytes(value), 0);
        }

        public static bool operator ==(FourCC a, FourCC b)
        {
            return a.NumericValue == b.NumericValue;
        }

        public static bool operator !=(FourCC a, FourCC b)
        {
            return a.NumericValue != b.NumericValue;
        }

        public static implicit operator FourCC(string a)
        {
            if (a.Length != 4)
            {
                throw new ArgumentException("FourCC string must be 4 characters long");
            }
            return new FourCC(a);
        }

        public override int GetHashCode()
        {
            return NumericValue.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (!(obj is FourCC))
            {
                return false;
            }

            return ((FourCC)obj).NumericValue == this.NumericValue;
        }
    }
}
