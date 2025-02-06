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
        Queue<IEnumerator> _batch;
        List<Coroutine> _actives;

        List<CellView> _allViews;
        HashSet<PlayableView> _playables;
        Queue<PlayableView> _pool;

        Transform _gridParent;
        Transform _playableParent;
        IInputController _inputController;

        public Runtime(GameClient client, IInputController inputController)
        {
            _client = client;
            _allViews = new List<CellView>();
            _pool = new Queue<PlayableView>();
            _playables = new HashSet<PlayableView>();
            _grid = new Grid(_client.GameWidth, client.GameHeight);
            _inputController = inputController;

            _batch = new Queue<IEnumerator>();
            _actives = new List<Coroutine>();

            _gridParent = new GameObject("Grid").transform;
            _playableParent = new GameObject("Playables").transform;

            var gridViews = CellView.CreateViews(_client, _grid, _client.CellPrefab, _gridParent);
            var colorViews = CellView.CreateViews(_client, _grid, _client.PlayablePrefab, _playableParent);

            _allViews.AddRange(gridViews);
            _allViews.AddRange(colorViews);

            FormatGame(gridViews, colorViews);
            _gameRoutine = client.StartCoroutine(GameLoop());
        }

        ~Runtime()
        {
            Flush();   
        }

        public void Flush()
        {
            foreach (var active in _actives) { _client.StopCoroutine(active); }
            foreach (var view in _allViews) { GameObject.Destroy(view.gameObject); }
            
            _allViews.Clear();
            _grid = null;
            
            GameObject.Destroy(_gridParent.gameObject);
            GameObject.Destroy(_playableParent.gameObject);

            if (_gameRoutine != null) _client.StopCoroutine(_gameRoutine);
            
            _gameRoutine = null;
        }

        void FormatGame(HashSet<CellView> gridViews, HashSet<PlayableView> playables)
        {
            foreach (var view in gridViews)
            {
                view.gameObject.SetActive(true);
                view.transform.position = view.WorldPosition;
            }

            foreach (var playable in playables)
            {
                _playables.Add(playable);
                PoolIn(playable);
            }

            var colors = _client.PlayableColors;
            foreach (var cell in _grid)
            {
                var rng = UnityEngine.Random.Range(0, colors.Length);
                var color = colors[rng];
                _grid.AssignColor(cell.X, cell.Y, color.r, color.g, color.b, color.a);
                
                PoolOut(cell.X, cell.Y);
            }
        }

        public IEnumerator GameLoop()
        {
            while (true)
            {
                foreach (var playable in _playables)
                {
                    if (playable.Dirty)
                    {
                        PlayIn(playable);
                    }
                }

                while (_batch.Count > 0)
                {
                    var next = _batch.Dequeue();
                    _client.StartCoroutine(RunSubroutine(next));
                    yield return null;
                }

                foreach (var playable in _playables)
                {
                    playable.Dirty = false;
                }

                _inputController.Refresh();
                if (_inputController.MouseClickThisFrame)
                {
                    UpdateMouseInput();
                }

                yield return null;
            }
        }

        void UpdateMouseInput()
        {
            var ray = Camera.main.ScreenPointToRay(_inputController.MouseScreenPos);
            var hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                var view = hit.collider.GetComponent<PlayableView>();
                if (view != null)
                {
                    ProcessClick(view);
                }
            }
        }

        // TOOD: remove this, unnecessary
        IEnumerator RunSubroutine(IEnumerator subroutine, Action done = null)
        {
            var execution = _client.StartCoroutine(subroutine);
            _actives.Add(execution);
            yield return execution;
            _actives.Remove(execution);
            done?.Invoke();
        }

        public void PlayIn(PlayableView playable)
        {
            var curve = _client.GridAnimationCurve;
            var speed = _client.GridAnimationSpeed;

            playable.gameObject.SetActive(true);
            var lerp = playable.Lerp(playable.transform, playable.transform.position, playable.WorldPosition, curve, speed);
            _batch.Enqueue(lerp);
        }

        public void ProcessClick(PlayableView view)
        {
            if (view.Dirty) return;
            PoolIn(view, poolOutInstantly: true);
        }

        public void PoolOut(int x, int y, bool newColor = false)
        {
            var next = _pool.Dequeue();

            var width = _client.GameWidth;
            var height = _client.GameHeight;
            var size = _client.CellSize;
            var spacing = _client.GridSpacing;
            var yOffset = _client.GridCellStartOffsetY;

            if (newColor)
            {
                var colors = _client.PlayableColors;
                var rng = UnityEngine.Random.Range(0, colors.Length);
                var color = colors[rng];
                _grid.AssignColor(x, y, color.r, color.g, color.b, color.a);
            }
            
            var cell = _grid[x, y];
            next.Initialize(cell, width, height, size, spacing, yOffset);
            next.SetColor(cell);
            next.Dirty = true;
        }

        public void PoolIn(PlayableView playable, bool poolOutInstantly = false)
        {
            playable.gameObject.SetActive(false);
            _pool.Enqueue(playable);

            var yMax = _client.GameHeight;
            var currentX = playable.X;
            var currentY = playable.Y + 1;

            for (int y = currentY; y < yMax; y++)
            {
                foreach (var entry in _playables)
                {
                    if (_pool.Contains(entry)) continue;
                    if (entry.X != currentX) continue;
                    if (entry.Y == y)
                    {
                        if (y - 1 >= 0)
                        {
                            var current = _grid[entry.X, entry.Y];
                            var below = _grid[entry.X, entry.Y - 1];
                            entry.UpdatePosition(below);
                            entry.Dirty = true;

                            var c = entry.Renderer.color;
                            _grid.AssignColor(below.X, below.Y, c.r, c.g, c.b, c.a);
                            _grid.FreeUpCell(current.X, current.Y);
                        }
                    }
                }
            }

            if (poolOutInstantly)
            {
                foreach (var cell in _grid)
                {
                    if (cell.Unoccupied)
                    {
                        if (_pool.Count > 0)
                        {
                            PoolOut(cell.X, cell.Y, newColor: true);
                        }
                    }
                }
            }
        }
    }
}