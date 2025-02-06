namespace MyTapMatch
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Runtime
    {
        Grid _grid;
        GameClient _client;

        Coroutine _gameRoutine;
        Queue<IEnumerator> _queue;
        Queue<IEnumerator> _batch;
        List<Coroutine> _actives;

        HashSet<CellView> _views;
        HashSet<PlayableView> _inPlay;
        HashSet<PlayableView> _pool;

        Transform _gridParent;
        Transform _playableParent;

        public Runtime(GameClient client)
        {
            _client = client;
            _views = new HashSet<CellView>();
            _inPlay = new HashSet<PlayableView>();
            _pool = new HashSet<PlayableView>();
            _grid = new Grid(_client.GameWidth, client.GameHeight);

            _queue = new Queue<IEnumerator>();
            _batch = new Queue<IEnumerator>();
            _actives = new List<Coroutine>();

            _gridParent = new GameObject("Grid").transform;
            _playableParent = new GameObject("Playables").transform;

            var gridViews = CreateViews(_client.CellPrefab, _gridParent);
            var colorViews = CreateViews(_client.PlayablePrefab, _playableParent);
            
            FormatColors(colorViews);
            
            _gameRoutine = client.StartCoroutine(GameStart(gridViews, colorViews));
        }

        ~Runtime()
        {
            Debug.Log("Flushing runtime");
            Flush();   
        }

        public void Flush()
        {
            _queue.Clear();

            foreach (var active in _actives)
            {
                _client.StopCoroutine(active);
            }

            foreach (var view in _views)
            {
                GameObject.Destroy(view.gameObject);
            }
            
            _views.Clear();
            _grid = null;
            
            GameObject.Destroy(_gridParent.gameObject);
            GameObject.Destroy(_playableParent.gameObject);

            if (_gameRoutine != null)
            {
                _client.StopCoroutine(_gameRoutine);
            }
            
            _gameRoutine = null;
        }

        public IEnumerator GameStart(HashSet<CellView> gameGrid, HashSet<PlayableView> playables)
        {
            TransitionIn(gameGrid, playables);

            while (_batch.Count > 0)
            {
                var next = _batch.Dequeue();
                _client.StartCoroutine(RunSubroutine(next));
                yield return null;
            }

            _inPlay = new HashSet<PlayableView>(playables);
            yield return GameLoop();   
        }

        public IEnumerator GameLoop()
        {
            while (true)
            {
                while (_queue.Count > 0)
                {
                    var next = _queue.Dequeue();
                    yield return RunSubroutine(next);
                }

                while (_batch.Count > 0)
                {
                    var next = _batch.Dequeue();
                    _client.StartCoroutine(RunSubroutine(next));
                    yield return null;
                }

                yield return null;
            }
        }

        IEnumerator RunSubroutine(IEnumerator subroutine, Action done = null)
        {
            var execution = _client.StartCoroutine(subroutine);
            _actives.Add(execution);
            yield return execution;
            _actives.Remove(execution);
            done?.Invoke();
        }

        HashSet<T> CreateViews<T>(T prefab, Transform parent = null) where T : CellView
        {   
            var set = new HashSet<T>();
            foreach (var cell in _grid)
            {
                var cellView = GameObject.Instantiate(prefab, parent);
                cellView.Initialize(cell, _client.GameWidth, _client.GameHeight, _client.CellSize, _client.GridSpacing);

                var gamePos = cellView.GamePosition;
                cellView.transform.position = gamePos + (Vector3.up * _client.GridCellStartOffsetY);
                cellView.gameObject.SetActive(false);
                set.Add(cellView);
                _views.Add(cellView);
            }

            return set;
        }

        void FormatColors<T>(HashSet<T> cells) where T : CellView
        {
            var colors = _client.PlayableColors;
            foreach (var entry in cells)
            {
                var rng = UnityEngine.Random.Range(0, colors.Length);
                var color = colors[rng];
                entry.Renderer.color = color;
            }
        }

        void TransitionIn(HashSet<CellView> grid, HashSet<PlayableView> playables)
        {
            foreach (var view in grid)
            {
                view.gameObject.SetActive(true);

                var curve = _client.GridAnimationCurve;
                var speed = _client.GridAnimationSpeed;

                _batch.Enqueue(Lerp(view.transform, view.transform.position, view.GamePosition, curve, speed));
            }

            foreach (var view in playables)
            {
                view.gameObject.SetActive(true);

                var curve = _client.GridAnimationCurve;
                var speed = _client.GridAnimationSpeed;

                _batch.Enqueue(Lerp(view.transform, view.transform.position, view.GamePosition, curve, speed));
            }
        }

        IEnumerator Lerp(Transform transform, Vector3 a, Vector3 b, EasingFunction.Ease easing, float speed = 1f)
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
        }

        public void ProcessClick(PlayableView view)
        {
            if (!_inPlay.Contains(view)) return;

            var neighbours = _grid.GetNeighbours(view.Cell);
            var clickedColor = view.Renderer.color;
            foreach (var entry in _views)
            {
                if (entry is PlayableView)
                {
                    var neighbourPlayable = entry as PlayableView;
                    if (neighbourPlayable.Renderer.color != clickedColor) continue;
                    foreach (var neighbour in neighbours)
                    {
                        if (neighbourPlayable.Cell == neighbour)
                        {
                            PoolPlayable(neighbourPlayable);
                        }
                    }
                }
            }
            
            PoolPlayable(view);
        }

        public void PoolPlayable(PlayableView playable)
        {
            _inPlay.Remove(playable);
            _pool.Add(playable);
            playable.gameObject.SetActive(false);
        }
    }
}