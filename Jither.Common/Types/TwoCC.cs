using System;
using System.Collections.Generic;
using System.Text;

namespace Jither.IO.Types
{
    public struct TwoCC
    {
        private static readonly string VALID_CHARS = "_ 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public ushort NumericValue { get; }
        public string Name { get; }

        public bool IsValid
        {
            get
            {
                string name = Name;
                for (int i = 0; i < 2; i++)
                {
                    if (VALID_CHARS.IndexOf(name[i]) < 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public TwoCC(ushort value)
        {
            this.Name = Encoding.ASCII.GetString(BitConverter.GetBytes(value));
            this.NumericValue = value;
        }

        public TwoCC(string value)
        {
            this.Name = value;
            this.NumericValue = BitConverter.ToUInt16(Encoding.ASCII.GetBytes(value), 0);
        }

        public static bool operator ==(TwoCC a, TwoCC b)
        {
            return a.NumericValue == b.NumericValue;
        }

        public static bool operator !=(TwoCC a, TwoCC b)
        {
            return a.NumericValue != b.NumericValue;
        }

        public static implicit operator TwoCC(string a)
        {
            if (a.Length != 2)
            {
                throw new ArgumentException("TwoCC string must be 2 characters long");
            }
            return new TwoCC(a);
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

            if (!(obj is TwoCC))
            {
                return false;
            }

            return ((TwoCC)obj).NumericValue == this.NumericValue;
        }
    }
}
