using System;
using System.Collections;
using System.Collections.Generic;

namespace MyTapMatch
{
    [Serializable]
    public class Grid : IEnumerable<Cell>
    {
        protected Cell[] _grid;
        protected int _width;
        protected int _height;
        
        public Grid(int width, int height)
        {
            _width = width;
            _height = height;

            _grid = new Cell[width * height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _grid[y * _width + x] = new Cell(x, y);
                }
            }
        }

        public HashSet<Cell> GetNeighbours(Cell cell)
        {
            var set = new HashSet<Cell>();
            for (int x1 = -1; x1 <= 1; x1++)
            {
                for (int y1 = -1; y1 <= 1; y1++)
                {
                    if (x1 == 0 && y1 == 0 || Math.Abs(x1) == Math.Abs(y1)) continue;
                    int x = cell.X + x1; 
                    int y = cell.Y + y1;
                    if (x >= 0 && x < _width && y >= 0 && y < _height)
                    {
                        set.Add(_grid[y * _width + x]);
                    }
                }
            }
            return set;
        }

        public void AssignColor(int x, int y, float r, float g, float b, float a)
        {
            var cell = _grid[y * _width + x];
            cell.r = r;
            cell.g = g;
            cell.b = b;
            cell.a = a;
            cell.Unoccupied = false;
            _grid[y * _width + x] = cell;
        }

        public void FreeUpCell(int x, int y)
        {
            var cell = _grid[y * _width + x];
            cell.r = 0;
            cell.g = 0;
            cell.b = 0;
            cell.a = 0;
            cell.Unoccupied = true;
            _grid[y * _width + x] = cell;
        }

        public HashSet<(float, float, float, float)> GetColors()
        {
            var set = new HashSet<(float, float, float, float)>();
            foreach (var cell in _grid)
            {
                set.Add((cell.r, cell.g, cell.b, cell.a));
            }
            return set;
        }

        public Cell this[int x, int y]
        {
            get => _grid[y * _width + x];
            set => _grid[y * _width + x] = value;
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    yield return _grid[y * _width + x];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}


