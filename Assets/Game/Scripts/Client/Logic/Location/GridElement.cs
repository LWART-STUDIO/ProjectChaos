using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Location
{
    public class GridElement : MonoBehaviour
    {
        [SerializeField] private GameObject _straightElement;
        [SerializeField] private GameObject _slopeElement;
        [SerializeField] private List<GameObject> _grassPrefabs;
        [SerializeField] private List<GameObject> _treesPrefabs;
        [SerializeField] private List<GameObject> _rocksPrefabs;
        private Vector3 _startPosition;
        private Vector3 _startScale;
        private Vector3Int coordinates;
        private bool isSlope = false;
        private Vector2Int slopeDirection = Vector2Int.zero;

        public Vector3Int Coordinates => coordinates;
        private List<GameObject> _currentGrass = new List<GameObject>();
        private List<GameObject> _currentTrees = new List<GameObject>();
        private List<GameObject> _currentRocks = new List<GameObject>();

        public void SetCoordinates(Vector3Int coords)
        {
            coordinates = coords;
        }

        public void SpawnGrass()
        {
            foreach (GameObject grass in _currentGrass)
            {
                DestroyImmediate(grass);
            }

            _currentGrass.Clear();
            GameObject activeElement = isSlope ? _slopeElement : _straightElement;
            int randomCount = Random.Range(3, 7);

            Renderer blockRenderer = activeElement.GetComponent<Renderer>();
            if (blockRenderer == null) return;

            Bounds bounds = blockRenderer.bounds;
            float sizeX = bounds.size.x;
            float sizeZ = bounds.size.z;
            float blockHeight = bounds.size.y;

            for (int i = 0; i < randomCount; i++)
            {
                // Генерируем случайное смещение в пределах верхней части блока
                float offsetX = Random.Range(-sizeX * 0.45f, sizeX * 0.45f);
                float offsetZ = Random.Range(-sizeZ * 0.45f, sizeZ * 0.45f);
                Vector3 candidatePos = new Vector3(
                    bounds.center.x + offsetX,
                    0f, // будет пересчитано
                    bounds.center.z + offsetZ
                );

                Vector3 spawnPosition;
                Quaternion spawnRotation;

                if (isSlope)
                {
                    // === 1. Определяем направление склона в мировых координатах ===
                    Vector3 worldSlopeDir = Vector3.zero;
                    float run = 0f; // длина склона в этом направлении

                    if (slopeDirection == Vector2Int.right)
                    {
                        worldSlopeDir = Vector3.right;
                        run = sizeX;
                    }
                    else if (slopeDirection == Vector2Int.left)
                    {
                        worldSlopeDir = Vector3.left;
                        run = sizeX;
                    }
                    else if (slopeDirection == Vector2Int.up)
                    {
                        worldSlopeDir = Vector3.forward;
                        run = sizeZ;
                    }
                    else if (slopeDirection == Vector2Int.down)
                    {
                        worldSlopeDir = Vector3.back;
                        run = sizeZ;
                    }

                    // === 2. Вычисляем высоту точки на склоне ===
                    Vector3 fromCenter = candidatePos - bounds.center;
                    float projection = Vector3.Dot(fromCenter, worldSlopeDir);
                    float maxProjection = run * 0.5f; // половина длины в направлении склона

                    float t = (maxProjection > 0f)
                        ? Mathf.Clamp(projection / maxProjection, -1f, 1f)
                        : 0f;

                    // Линейная интерполяция по высоте: от min.y (t=-1) до max.y (t=+1)
                    float yOnSlope = Mathf.Lerp(bounds.min.y, bounds.max.y, (t + 1f) * 0.5f);
                    spawnPosition = new Vector3(candidatePos.x, yOnSlope, candidatePos.z);

                    // === 3. Вычисляем поворот по нормали склона ===
                    float k = (run > 0f) ? blockHeight / run : 0f;
                    Vector3 normal = new Vector3(
                        -k * worldSlopeDir.x,
                        1f,
                        -k * worldSlopeDir.z
                    ).normalized;

                    // Вектор "вперёд" для травы — горизонтальный, перпендикулярный направлению склона
                    Vector3 forward = Vector3.Cross(Vector3.up, normal);
                    if (forward.magnitude < 0.01f)
                        forward = Vector3.forward; // fallback, если нормаль вертикальна

                    spawnRotation = Quaternion.LookRotation(forward, normal);
                }
                else
                {
                    // Ровный блок — просто на верхней границе
                    spawnPosition = new Vector3(candidatePos.x, bounds.max.y, candidatePos.z);
                    spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                }

                // === 4. Создаём экземпляр травы ===
                GameObject grassPrefab = _grassPrefabs[Random.Range(0, _grassPrefabs.Count)];
                GameObject grassInstance = Instantiate(grassPrefab, spawnPosition, spawnRotation, transform);
                grassInstance.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
                _currentGrass.Add(grassInstance);
            }

            SpawnTree();
            SpawnRocks();
        }

        public void SpawnTree()
        {
            foreach (GameObject tree in _currentTrees)
            {
                DestroyImmediate(tree);
            }
            _currentTrees.Clear();
            GameObject activeElement = isSlope ? _slopeElement : _straightElement;
            int randomCount = Random.Range(0, 3);

            Renderer blockRenderer = activeElement.GetComponent<Renderer>();
            if (blockRenderer == null) return;

            Bounds bounds = blockRenderer.bounds;
            float sizeX = bounds.size.x;
            float sizeZ = bounds.size.z;

            for (int i = 0; i < randomCount; i++)
            {
                bool spawnTree = Random.Range(0, 3) == 0 ? true : false;
                if (!spawnTree)
                    continue;
                float offsetX = Random.Range(-sizeX * 0.45f, sizeX * 0.45f);
                float offsetZ = Random.Range(-sizeZ * 0.45f, sizeZ * 0.45f);
                Vector3 horizontalOffset = new Vector3(offsetX, 0, offsetZ);
                Vector3 candidatePos = bounds.center + horizontalOffset;

                Vector3 spawnPosition;

                if (isSlope)
                {
                    // 1. Определяем мировое направление, в котором идёт подъём склона
                    Vector3 worldSlopeDirection = Vector3.zero;
                    if (slopeDirection == Vector2Int.right) worldSlopeDirection = Vector3.right;
                    else if (slopeDirection == Vector2Int.left) worldSlopeDirection = Vector3.left;
                    else if (slopeDirection == Vector2Int.up)
                        worldSlopeDirection = Vector3.forward; // предполагаем, что "up" = +Z
                    else if (slopeDirection == Vector2Int.down) worldSlopeDirection = Vector3.back; // "down" = -Z


                    // 2. Находим, насколько candidatePos смещён от ЦЕНТРА блока вдоль направления склона
                    Vector3 fromCenter = candidatePos - bounds.center;
                    float projection = Vector3.Dot(fromCenter, worldSlopeDirection);

                    // 3. Максимальное возможное смещение в этом направлении = половина размера блока в этом направлении
                    float maxProjection = 0.5f * (
                        Mathf.Abs(worldSlopeDirection.x) * sizeX +
                        Mathf.Abs(worldSlopeDirection.z) * sizeZ
                    );

                    // 4. Нормализуем проекцию от -1 до +1 (−1 = низ склона, +1 = верх склона)
                    float t = maxProjection > 0 ? Mathf.Clamp(projection / maxProjection, -1f, 1f) : 0f;

                    // 5. Высота: низ склона = bounds.min.y, верх = bounds.max.y
                    // Когда t = -1 → Y = min, t = +1 → Y = max
                    float yOnSlope = Mathf.Lerp(bounds.min.y, bounds.max.y, (t + 1f) * 0.5f);

                    spawnPosition = new Vector3(candidatePos.x, yOnSlope, candidatePos.z);
                }
                else
                    spawnPosition = new Vector3(candidatePos.x, bounds.max.y, candidatePos.z);

                GameObject treePrefab = _treesPrefabs[Random.Range(0, _treesPrefabs.Count)];
                GameObject treeInstance = Instantiate(treePrefab, spawnPosition, Quaternion.identity, transform);
                treeInstance.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
                _currentTrees.Add(treeInstance);
            }
        }

        public void SpawnRocks()
        {
            foreach (GameObject rock in _currentRocks)
            {
                DestroyImmediate(rock);
            }

            _currentRocks.Clear();
            GameObject activeElement = isSlope ? _slopeElement : _straightElement;
            int randomCount = Random.Range(0, 3);

            Renderer blockRenderer = activeElement.GetComponent<Renderer>();
            if (blockRenderer == null) return;

            Bounds bounds = blockRenderer.bounds;
            float sizeX = bounds.size.x;
            float sizeZ = bounds.size.z;
            float blockHeight = bounds.size.y;

            for (int i = 0; i < randomCount; i++)
            {
                bool spawnRock = Random.Range(0, 3) == 0 ? true : false;
                if (!spawnRock)
                    continue;
                // Генерируем случайное смещение в пределах верхней части блока
                float offsetX = Random.Range(-sizeX * 0.45f, sizeX * 0.45f);
                float offsetZ = Random.Range(-sizeZ * 0.45f, sizeZ * 0.45f);
                Vector3 candidatePos = new Vector3(
                    bounds.center.x + offsetX,
                    0f, // будет пересчитано
                    bounds.center.z + offsetZ
                );

                Vector3 spawnPosition;
                Quaternion spawnRotation;

                if (isSlope)
                {
                    // === 1. Определяем направление склона в мировых координатах ===
                    Vector3 worldSlopeDir = Vector3.zero;
                    float run = 0f; // длина склона в этом направлении

                    if (slopeDirection == Vector2Int.right)
                    {
                        worldSlopeDir = Vector3.right;
                        run = sizeX;
                    }
                    else if (slopeDirection == Vector2Int.left)
                    {
                        worldSlopeDir = Vector3.left;
                        run = sizeX;
                    }
                    else if (slopeDirection == Vector2Int.up)
                    {
                        worldSlopeDir = Vector3.forward;
                        run = sizeZ;
                    }
                    else if (slopeDirection == Vector2Int.down)
                    {
                        worldSlopeDir = Vector3.back;
                        run = sizeZ;
                    }

                    // === 2. Вычисляем высоту точки на склоне ===
                    Vector3 fromCenter = candidatePos - bounds.center;
                    float projection = Vector3.Dot(fromCenter, worldSlopeDir);
                    float maxProjection = run * 0.5f; // половина длины в направлении склона

                    float t = (maxProjection > 0f)
                        ? Mathf.Clamp(projection / maxProjection, -1f, 1f)
                        : 0f;

                    // Линейная интерполяция по высоте: от min.y (t=-1) до max.y (t=+1)
                    float yOnSlope = Mathf.Lerp(bounds.min.y, bounds.max.y, (t + 1f) * 0.5f);
                    spawnPosition = new Vector3(candidatePos.x, yOnSlope, candidatePos.z);

                    // === 3. Вычисляем поворот по нормали склона ===
                    float k = (run > 0f) ? blockHeight / run : 0f;
                    Vector3 normal = new Vector3(
                        -k * worldSlopeDir.x,
                        1f,
                        -k * worldSlopeDir.z
                    ).normalized;

                    // Вектор "вперёд" для травы — горизонтальный, перпендикулярный направлению склона
                    Vector3 forward = Vector3.Cross(Vector3.up, normal);
                    if (forward.magnitude < 0.01f)
                        forward = Vector3.forward; // fallback, если нормаль вертикальна

                    spawnRotation = Quaternion.LookRotation(forward, normal);
                }
                else
                {
                    // Ровный блок — просто на верхней границе
                    spawnPosition = new Vector3(candidatePos.x, bounds.max.y, candidatePos.z);
                    spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                }

                // === 4. Создаём экземпляр травы ===
                GameObject rockPrefab = _rocksPrefabs[Random.Range(0, _rocksPrefabs.Count)];
                GameObject rockInstance = Instantiate(rockPrefab, spawnPosition, spawnRotation, transform);
                rockInstance.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
                _currentRocks.Add(rockInstance);
            }
        }

        public void MakeSlope(Vector2Int direction)
        {
            _straightElement.SetActive(false);
            _slopeElement.SetActive(true);
            isSlope = true;
            slopeDirection = direction;

            transform.position = _startPosition;
            transform.localScale = _startScale;

            if (direction == Vector2Int.right)
            {
                transform.rotation = Quaternion.Euler(0, -90f, 0);
            }
            else if (direction == Vector2Int.left)
            {
                transform.rotation = Quaternion.Euler(0, 90f, 0);
            }
            else if (direction == Vector2Int.up)
            {
                transform.rotation = Quaternion.Euler(0, 180f, 0);
            }
            else if (direction == Vector2Int.down)
            {
            }
        }

        public void ConfigureBottomPart(float blockSpacingVertical)
        {
            _startPosition = transform.position;
            _startScale = transform.localScale;

            // Целевая ВЕРХНЯЯ позиция блока (как задумано логикой)
            float targetTopY = Coordinates.y * blockSpacingVertical;

            // Высота одного "этажа" в мировых единицах
            float unitHeight = blockSpacingVertical;

            // Сколько "этажей" занимает этот блок от нуля
            int heightInUnits = Coordinates.y; // если от Y=0

            // Если блок на уровне 0 — делаем минимальную высоту (1 юнит)
            if (heightInUnits == 0) heightInUnits = 1;

            // Новая высота меша в мировых единицах
            float newWorldHeight = heightInUnits * unitHeight;

            // Получаем исходную (локальную) высоту модели при scale = 1
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null) return;

            float originalLocalHeight = renderer.bounds.size.y / transform.localScale.y;

            // Новый localScale.y, чтобы достичь нужной мировой высоты
            float newLocalScaleY = newWorldHeight / originalLocalHeight;

            // Применяем масштаб
            Vector3 newScale = transform.localScale;
            newScale.y = newLocalScaleY;
            transform.localScale = newScale;

            // Теперь сдвигаем блок так, чтобы его ВЕРХ остался на targetTopY
            // При масштабировании pivot остаётся на месте, но меш растягивается вниз и вверх
            // Чтобы верх был на targetTopY, центр должен быть на:
            //   centerY = targetTopY - newWorldHeight / 2

            float centerY = targetTopY - newWorldHeight * 0.5f;
            transform.position = new Vector3(transform.position.x, centerY, transform.position.z);
        }
    }
}