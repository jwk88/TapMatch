namespace MyTapMatch
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Runtime
    {
        Grid _grid;
        GameClient _client;

        Coroutine _gameRoutine;
        Queue<IEnumerator> _batch;
        List<Coroutine> _activeSubroutines;

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
            _activeSubroutines = new List<Coroutine>();

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
            foreach (var active in _activeSubroutines) { _client.StopCoroutine(active); }
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
            _playables = playables;

            foreach (var view in gridViews)
            {
                view.gameObject.SetActive(true);
                view.transform.position = view.WorldPosition;
            }

            foreach (var playable in _playables)
            {
                Spawn(playable);
            }
        }

        public IEnumerator GameLoop()
        {
            while (true)
            {
                while (_batch.Count > 0)
                {
                    var next = _batch.Dequeue();
                    _client.StartCoroutine(RunSubroutine(next));
                    yield return null;
                }

                _inputController.Refresh();
                if (_inputController.MouseClickThisFrame)
                {
                    UpdateMouseInput();
                }

                if (_activeSubroutines.Count > 0)
                {
                    yield return null;
                    continue;
                }

                while (_pool.Count > 0)
                {
                    var next = _pool.Dequeue();
                    Spawn(next);
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

        IEnumerator RunSubroutine(IEnumerator subroutine)
        {
            var execution = _client.StartCoroutine(subroutine);
            
            _activeSubroutines.Add(execution);
            yield return execution;
            _activeSubroutines.Remove(execution);
        }

        public void MovePlayable(PlayableView playable)
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
            
            var origin = _grid[view.X, view.Y];
            PoolNeighboursRecursively(origin, view);
            UpdateColumns();
        }

        void UpdateColumns()
        {
            var emptyColumns = new HashSet<int>();
            foreach (var cell in _grid)
            {
                if (cell.Unoccupied)
                {
                    emptyColumns.Add(cell.X);
                }
            }

            var moved = new HashSet<PlayableView>();
            foreach (var emptyColumn in emptyColumns)
            {
                var x = emptyColumn;
                for (int y = 0; y < _client.GameHeight; y++)
                {
                    var cell = _grid[x, y];
                    if (cell.Unoccupied)
                    {
                        var y1 = y;
                        for (;y1 < _client.GameHeight; y1++)
                        {
                            if (!_grid[x, y1].Unoccupied)
                            {
                                break;
                            }
                        }

                        if (y1 >= _client.GameHeight) continue;

                        var playable = GetPlayableView(x, y1);
                        var color = playable.Renderer.color;

                        playable.UpdatePosition(cell);
                        _grid.AssignColor(cell.X, cell.Y, color.r, color.g, color.b, color.a);
                        _grid.FreeUpCell(x, y1);
                        moved.Add(playable);
                    }
                }

                foreach (var move in moved)
                {
                    MovePlayable(move);
                }

                //_updatingPlayables = true;
            }
        }

        PlayableView GetPlayableView(int x, int y)
        {
            foreach (var entry in _playables)
            {
                if (_pool.Contains(entry)) continue;
                if (entry.X == x && entry.Y == y)
                {
                    return entry;
                }
            }
            return null;
        }

        public void Spawn(PlayableView next)
        {
            if (!_grid.TryGetUnoccupied(out var cell))
            {
                Debug.Log("no unoccupied cells...");
                return;
            }

            var width = _client.GameWidth;
            var height = _client.GameHeight;
            var size = _client.CellSize;
            var spacing = _client.GridSpacing;
            var yOffset = _client.GridCellStartOffsetY;

            var colors = _client.PlayableColors;
            var rng = UnityEngine.Random.Range(0, colors.Length);
            var color = colors[rng];

            _grid.AssignColor(cell.X, cell.Y, color.r, color.g, color.b, color.a);
            cell = _grid[cell.X, cell.Y];

            next.Initialize(cell, width, height, size, spacing, yOffset);
            next.SetColor(cell);

            MovePlayable(next);
        }

        public void PoolNeighboursRecursively(Cell origin, PlayableView originView)
        {
            originView.gameObject.SetActive(false);

            _pool.Enqueue(originView);
            _grid.FreeUpCell(origin.X, origin.Y);
            
            var neighbours = _grid.GetNeighbours(origin);
            foreach (var playable in _playables)
            {
                if (playable == originView) continue;
                if (_pool.Contains(playable)) continue;
                if (playable.Renderer.color != originView.Renderer.color) continue;
                foreach (var neighbour in neighbours)
                {
                    var playableCell = _grid[playable.X, playable.Y];
                    if (playableCell == neighbour)
                    {
                        PoolNeighboursRecursively(playableCell, playable);
                    }
                }
            }
        }
    }
}