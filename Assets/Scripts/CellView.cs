using UnityEngine;

namespace MyTapMatch
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _renderer;

        public Cell Cell { get => _cell; private set => _cell = value; }
        public Vector3 GamePosition { get => _gamePosition; private set => _gamePosition = value; }

        Cell _cell;
        Vector3 _gamePosition;

        public void Initialize(Cell cell, int gameWidth, int gameHeight, int size, float spacing)
        {
            Cell = cell;
            var cellPos = cell.Vector3();
            var center = new Vector2(gameWidth / 2, gameHeight / 2);
            cellPos.x -= center.x;
            cellPos.y -= center.y;
            cellPos.x *= size + spacing;
            cellPos.y *= size + spacing;
            transform.localScale = Vector3.one * size;
            GamePosition = cellPos;
        }

        public void Render()
        {
            transform.position = GamePosition;
        }
    }
}


