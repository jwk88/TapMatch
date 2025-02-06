using System;
using System.Collections.Generic;

namespace MyTapMatch
{
    public struct Cell : IEqualityComparer<Cell>, IEquatable<Cell>
    {
        public int X;
        public int Y;

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
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
    }
}


