using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyTapMatch
{
    [Serializable]
    public struct Cell : IEqualityComparer<Cell>, IEquatable<Cell>
    {
        public int X;
        public int Y;
        public float r, g, b, a;
        public bool Unoccupied;

        public Cell(int x, int y)
        {
            X = x;
            Y = y;

            r = 0;
            g = 0;
            b = 0;
            a = 0;

            Unoccupied = true;
        }

        public bool Equals(Cell a, Cell b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public bool Equals(Cell other)
        {
            return other.X == X && Y == other.Y;
        }

        public int GetHashCode(Cell obj)
        {
            return obj.X.GetHashCode() ^ obj.Y.GetHashCode();
        }

        public Vector3 Vector3()
        {
            return new Vector3(X, Y, 0);
        }

        public override bool Equals(object obj)
        {
            if (obj is Cell)
            {
                return Equals((Cell)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static bool operator ==(Cell a, Cell b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Cell a, Cell b)
        {
            return !a.Equals(b);
        }
    }
}


