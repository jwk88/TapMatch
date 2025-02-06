using System.Collections;
using System.Collections.Generic;

namespace MyTapMatch
{
    public class Grid : IEnumerable<Cell>
    {
        Cell[] _grid;
        int _width;
        int _height;
        
        public Grid(int width, int height)
        {
            _width = width;
            _height = height;

            _grid = new Cell[width * height];
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


