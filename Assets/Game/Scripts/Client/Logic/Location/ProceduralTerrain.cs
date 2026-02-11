using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player;
using SaintsField.Playa;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.Client.Logic.Location
{
    public class ProceduralTerrain : MonoBehaviour
    {
    
        [SerializeField] private GridElement _blockPrefab;
        [Range(0, 1f)][SerializeField] private float _hilliness = 0.5f;
        [SerializeField] private int _mapSize = 20;
        [SerializeField] private int _blockSpacingHorizontal = 20;
        [SerializeField] private int _blockSpacingVertical = 20;
        [SerializeField] private int _maxBlocks = 300;
        [SerializeField] private int _maxHeight = 5;
        [SerializeField] private List<GridElement> _allBlocks = new List<GridElement>();
        private float _minDistanceToPlayer = 60f;
        private int _checksPerFrame = 10;
        private bool _isSearchingPosition;

        private GridElement[,] _gridElements;
       
        private Vector2Int lockedDirection = Vector2Int.zero;

        public void GetPositionForEvent(Action<Vector3> onResult)
        {
            if (_isSearchingPosition)
            {
                // fallback — чтобы ивент не завис
                onResult?.Invoke(Vector3.zero);
                return;
            }
            StartCoroutine(FindPositionCoroutine(onResult));
        }
        private IEnumerator FindPositionCoroutine(Action<Vector3> onResult)
        {
            _isSearchingPosition = true;

            // 1️⃣ Копируем и перемешиваем
            List<GridElement> shuffledElements = new List<GridElement>(_allBlocks);
            Shuffle(shuffledElements);
            int checks = 0;

            foreach (var element in shuffledElements)
            {
                if (++checks >= _checksPerFrame)
                {
                    checks = 0;
                    yield return null;
                }
                if (element.IsSlope || element.HaveEvent)
                    continue;
                if (IsTooCloseToAnyPlayer(element.transform.position))
                    continue;
                Vector3 spawnPos = element.GetSpawnEventPosition();
                _isSearchingPosition = false;
                onResult?.Invoke(spawnPos);
                yield break;
           
            }

            // 3️⃣ fallback
            _isSearchingPosition = false;
            onResult?.Invoke(Vector3.zero);
        }
        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private bool IsTooCloseToAnyPlayer(Vector3 position)
        {
            float minSqr = _minDistanceToPlayer * _minDistanceToPlayer;

            foreach (var player in PlayerHealth.AllPlayers)
            {
                if (!player.Value)
                    continue;

                if ((player.Value.transform.position - position).sqrMagnitude < minSqr)
                    return true;
            }

            return false;
        }
#if UNITY_EDITOR
        [Button]
        private void DestroyTerrain()
        {
            var allBlocks = transform.GetComponentsInChildren<GridElement>();
            foreach (var block in allBlocks)
            {
                DestroyImmediate(block.gameObject);
            }
            _gridElements = null;
            _allBlocks.Clear();
            lockedDirection = Vector2Int.zero;
        }

        [Button]
        private void Generate()
        {
            DestroyTerrain();
            _gridElements = new GridElement[_mapSize, _mapSize];

            // 1. Старт с рандомной точки
            int startX = Random.Range(0, _mapSize);
            int startZ = Random.Range(0, _mapSize);
            GridElement current = CreateElement(startX, 0, startZ);
            _allBlocks.Add(current);

            int spawnedCount = 1;

            while (spawnedCount < _maxBlocks)
            {
                Vector2Int direction = Vector2Int.zero;

                // Пробуем продолжить в lockedDirection
                if (lockedDirection != Vector2Int.zero)
                {
                    int nx = current.Coordinates.x + lockedDirection.x;
                    int nz = current.Coordinates.z + lockedDirection.y;
                    if (IsValidAndEmpty(nx, nz))
                    {
                        direction = lockedDirection;
                    }
                    else
                    {
                        lockedDirection = Vector2Int.zero;
                    }
                }

                // Или выбираем случайное доступное направление
                if (direction == Vector2Int.zero)
                {
                    var available = GetAvailableDirections(current);
                    if (available.Count > 0)
                    {
                        direction = available[Random.Range(0, available.Count)];
                    }
                }

                // Если из текущего блока некуда идти — ищем любой блок с выходом
                if (direction == Vector2Int.zero)
                {
                    bool found = false;
                    foreach (var block in _allBlocks)
                    {
                        var dirs = GetAvailableDirections(block);
                        if (dirs.Count > 0)
                        {
                            current = block;
                            direction = dirs[Random.Range(0, dirs.Count)];
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        break; // карта заполнена
                    }
                }

                // Координаты следующего блока
                int newX = current.Coordinates.x + direction.x;
                int newZ = current.Coordinates.z + direction.y;
                int newY = current.Coordinates.y;

                // Проверка: можно ли сделать подъём
                bool canRaise = 
                    CanRaiseElevationInDirection(current, direction) &&
                    IsSlopeSafeHere(current, direction); // ← без !current.IsSlope

                bool withinHeightLimit = current.Coordinates.y < _maxHeight;
                bool shouldRaise = canRaise && withinHeightLimit && Random.value < _hilliness / 2f;

                if (shouldRaise)
                {
                    newY++;
                    lockedDirection = direction;
                }
                else
                {
                    lockedDirection = Vector2Int.zero;
                }

                GridElement next = CreateElement(newX, newY, newZ);
                if (shouldRaise)
                {
                    current.MakeSlope(direction); // применяется к текущему (нижнему) блоку
                }
                next.ConfigureBottomPart(_blockSpacingVertical);
                _allBlocks.Add(next);
                current.SpawnGrass();
                current = next;
                spawnedCount++;
            }
        }

        private GridElement CreateElement(int x, int y, int z)
        {
            if (!IsValidAndEmpty(x, z)) return null;

            Vector3 position = new Vector3(
                x * _blockSpacingHorizontal,
                y * _blockSpacingVertical,
                z * _blockSpacingHorizontal
            );

            // GridElement element = Instantiate(_blockPrefab, position, Quaternion.identity, transform);
            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(_blockPrefab.gameObject) as GameObject;
    
            if (prefabInstance == null)
            {
                Debug.LogError("Failed to instantiate prefab");
                return null;
            }

            prefabInstance.transform.SetParent(transform, false);
            prefabInstance.transform.position = position;
            prefabInstance.transform.rotation = Quaternion.identity;

            GridElement element = prefabInstance.GetComponent<GridElement>();
            element.SetCoordinates(new Vector3Int(x, y, z));
            _gridElements[x, z] = element;
            return element;
        }

        private bool IsValidAndEmpty(int x, int z)
        {
            return x >= 0 && x < _mapSize && z >= 0 && z < _mapSize && _gridElements[x, z] == null;
        }

        private List<Vector2Int> GetAvailableDirections(GridElement element)
        {
            List<Vector2Int> dirs = new List<Vector2Int>();
            foreach (var dir in new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down })
            {
                int nx = element.Coordinates.x + dir.x;
                int nz = element.Coordinates.z + dir.y;
                if (IsValidAndEmpty(nx, nz))
                {
                    dirs.Add(dir);
                }
            }
            return dirs;
        }

        private bool CanRaiseElevationInDirection(GridElement element, Vector2Int direction)
        {
            int nx = element.Coordinates.x + direction.x;
            int nz = element.Coordinates.z + direction.y;
            return IsValidAndEmpty(nx, nz);
        }

        /// <summary>
        /// Проверяет, что перед склоном (A - D) и после следующего блока (A + 2*D) есть место на карте.
        /// Гарантирует отсутствие обрывов до и после склона.
        /// </summary>
        private bool IsSlopeSafeHere(GridElement element, Vector2Int direction)
        {
            // Prev = A - D
            int prevX = element.Coordinates.x - direction.x;
            int prevZ = element.Coordinates.z - direction.y;

            // Next = A + 2*D
            int nextX = element.Coordinates.x + 2 * direction.x;
            int nextZ = element.Coordinates.z + 2 * direction.y;

            bool prevInBounds = prevX >= 0 && prevX < _mapSize && prevZ >= 0 && prevZ < _mapSize;
            bool nextInBounds = nextX >= 0 && nextX < _mapSize && nextZ >= 0 && nextZ < _mapSize;

            return prevInBounds && nextInBounds;
        }
        #endif
    }
}