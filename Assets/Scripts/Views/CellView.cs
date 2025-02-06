using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTapMatch
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _renderer;

        public int X { get; private set; }
        public int Y { get; private set; }
        public bool Dirty { get; set; }
        public Vector3 WorldPosition { get => _worldPosition; private set => _worldPosition = value; }
        public SpriteRenderer Renderer { get => _renderer; private set => _renderer = value; }

        Vector3 _worldPosition;
        int _gameWidth;
        int _gameHeight;
        int _size;
        float _spacing;

        public virtual void Initialize(Cell cell, int gameWidth, int gameHeight, int size, float spacing, float yOffset)
        {
            _gameWidth = gameWidth;
            _gameHeight = gameHeight;
            _spacing = spacing;
            _size = size;

            X = cell.X;
            Y = cell.Y;

            var vec3 = cell.Vector3();
            
            var center = new Vector2(gameWidth / 2, gameHeight / 2);
            vec3.x -= center.x;
            vec3.y -= center.y;
            vec3.x *= size + spacing;
            vec3.y *= size + spacing;

            transform.localScale = Vector3.one * size;
            transform.position = vec3 + (Vector3.up * yOffset);
            
            WorldPosition = vec3;
        }

        public virtual void UpdatePosition(Cell cell)
        {
            X = cell.X;
            Y = cell.Y;

            var vec3 = cell.Vector3();
            
            var center = new Vector2(_gameWidth / 2, _gameHeight / 2);
            vec3.x -= center.x;
            vec3.y -= center.y;
            vec3.x *= _size + _spacing;
            vec3.y *= _size + _spacing;

            transform.localScale = Vector3.one * _size;
            WorldPosition = vec3;
        }

        public IEnumerator Lerp(Transform transform, Vector3 a, Vector3 b, EasingFunction.Ease easing, float speed = 1f)
        {
            var t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                var easeFunction = EasingFunction.GetEasingFunction(easing);
                var eased = easeFunction(0, 1, t);
                transform.position = Vector3.Lerp(a, b, eased);
                yield return null;
            }

            transform.position = b;
        }

        public static HashSet<T> CreateViews<T>(GameClient _client, Grid gameGrid, T prefab, Transform parent = null) where T : CellView
        {   
            var created = new HashSet<T>();
            foreach (var cell in gameGrid)
            {
                var view = GameObject.Instantiate(prefab, parent);

                var width = _client.GameWidth;
                var height = _client.GameHeight;
                var size = _client.CellSize;
                var spacing = _client.GridSpacing;
                var yOffset = _client.GridCellStartOffsetY;
                
                view.Initialize(cell, width, height, size, spacing, yOffset);
                view.Dirty = true;

                created.Add(view);
            }

            return created;
        }
    }
}


