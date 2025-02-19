using UnityEngine;

namespace MyTapMatch
{
    public class GameClient : MonoBehaviour
    {
        [SerializeField] InputListener _inputListener;
        [SerializeField] int _gameWidth = 8;
        [SerializeField] int _gameHeight = 8;
        [SerializeField] int _cellSize = 1;
        [SerializeField] float _gridSpacing = 0.25f;
        [SerializeField] int _gridCellStartOffsetY = 10;
        [SerializeField] Transform _viewParent;
        [SerializeField] CellView _cellPrefab;
        [SerializeField] PlayableView _playablePrefab;
        [SerializeField] EasingFunction.Ease _gridAnimationCurve = EasingFunction.Ease.EaseOutBounce;
        [SerializeField] float _gridAnimationSpeed = 1f;
        [SerializeField] Vector2 _viewOffset = new Vector2(4f, 0.5f);
        [SerializeField] Color[] _playableColors;

        public int GameWidth { get => _gameWidth; set => _gameWidth = value; }
        public int GameHeight { get => _gameHeight; set => _gameHeight = value; }
        public int CellSize { get => _cellSize; set => _cellSize = value; }
        public float GridSpacing { get => _gridSpacing; set => _gridSpacing = value; }
        public int GridCellStartOffsetY { get => _gridCellStartOffsetY; set => _gridCellStartOffsetY = value; }
        public CellView CellPrefab { get => _cellPrefab; set => _cellPrefab = value; }
        public EasingFunction.Ease GridAnimationCurve { get => _gridAnimationCurve; set => _gridAnimationCurve = value; }
        public float GridAnimationSpeed { get => _gridAnimationSpeed; set => _gridAnimationSpeed = value; }
        public Color[] PlayableColors { get => _playableColors; set => _playableColors = value; }
        public PlayableView PlayablePrefab { get => _playablePrefab; set => _playablePrefab = value; }
        public Transform ViewParent { get => _viewParent; set => _viewParent = value; }
        public Vector2 ViewOffset { get => _viewOffset; set => _viewOffset = value; }

        Runtime _runtime;

        public void NewGame()
        {
            if (_runtime != null)
            {
                _runtime.Flush();
            }

            _runtime = new Runtime(this, _inputListener);
        }
    }
}


