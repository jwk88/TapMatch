using UnityEngine;

namespace MyTapMatch
{
    public class GameClient : MonoBehaviour
    {
        [SerializeField] int _gameWidth = 8;
        [SerializeField] int _gameHeight = 8;
        [SerializeField] int _cellSize = 1;
        [SerializeField] float _gridSpacing = 0.25f;
        [SerializeField] int _gridCellStartOffsetY = 10;
        [SerializeField] CellView _cellPrefab;
        [SerializeField] EasingFunction.Ease _gridAnimationCurve = EasingFunction.Ease.EaseOutBounce;
        [SerializeField] float _gridAnimationSpeed = 1f;

        public int GameWidth { get => _gameWidth; set => _gameWidth = value; }
        public int GameHeight { get => _gameHeight; set => _gameHeight = value; }
        public int CellSize { get => _cellSize; set => _cellSize = value; }
        public float GridSpacing { get => _gridSpacing; set => _gridSpacing = value; }
        public int GridCellStartOffsetY { get => _gridCellStartOffsetY; set => _gridCellStartOffsetY = value; }
        public CellView CellPrefab { get => _cellPrefab; set => _cellPrefab = value; }
        public EasingFunction.Ease GridAnimationCurve { get => _gridAnimationCurve; set => _gridAnimationCurve = value; }
        public float GridAnimationSpeed { get => _gridAnimationSpeed; set => _gridAnimationSpeed = value; }

        Runtime _runtime;

        public void NewGame()
        {
            if (_runtime != null)
            {
                _runtime.Flush();
            }

            _runtime = new Runtime(this);
        }   
    }
}


