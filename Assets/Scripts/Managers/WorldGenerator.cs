// Assets/Scripts/Managers/WorldGenerator.cs
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("Player & World Settings")]
    public Transform player;
    [Tooltip("Размер чанка по X и Y в тайлах")]
    public int chunkSize = 124;
    [Tooltip("Радиус чанков вокруг игрока (в чанках)")]
    public int loadRadius = 2;
    [Tooltip("Минимальное смещение игрока (в мировых юнитах) для пересчёта чанков")]
    public float updateDistanceThreshold = 1.24f * 4f;

    [Header("Grid / Tilemaps")]
    public Grid worldGrid;
    public Tilemap sandLayer;
    public Tilemap grassLayer;
    public Tilemap mountainLayer;

    [Header("Tiles")]
    public TileBase sandTile;
    public TileBase grassTile;
    public TileBase mountainTile;

    [Header("Object prefabs (spawn only on grass)")]
    public GameObject[] objectPrefabs;
    [Range(0f, 1f)]
    public float objectSpawnChance = 0.10f; // вероятность спавна объекта на клетке с травой

    [Header("Noise Settings")]
    public int seed = 12345;
    public float globalScale = 0.01f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    // internal
    private readonly HashSet<(int, int)> loadedChunks = new();     // реально применённые чанки
    private readonly HashSet<(int, int)> enqueuedChunks = new();   // чанки, которые запланированы/в очереди
    private readonly Queue<(int, int)> generateQueue = new();     // очередь (используется только в main thread)
    private readonly Dictionary<(int, int), List<GameObject>> chunkObjects = new();
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> objectPools = new();
    private Vector3 lastPlayerPosition;
    private System.Random mainRng;
    private bool queueWorkerRunning = false;

    private class PooledInstance : MonoBehaviour { public GameObject originalPrefab; }

    private enum TileType : byte { Sand = 0, Grass = 1, Mountain = 2 }

    private enum Biome
    {
        Sand,
        Grass,
        Mountain,
        Forest
    }

    void Awake()
    {
        worldGrid.cellSize = new Vector3(1.24f, 1.24f, 0f);

        lastPlayerPosition = player != null ? player.position : Vector3.zero;
        mainRng = new System.Random(seed);

        // Создаём пулы для префабов
        foreach (var prefab in objectPrefabs)
        {
            if (prefab == null) continue;
            objectPools[prefab] = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var go = Instantiate(prefab);
                    var tag = go.GetComponent<PooledInstance>();
                    if (tag == null) tag = go.AddComponent<PooledInstance>();
                    tag.originalPrefab = prefab;
                    go.SetActive(false);
                    return go;
                },
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go => go.SetActive(false),
                actionOnDestroy: go => Destroy(go),
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 1000
            );
        }
    }

    void Start()
    {
        if (player == null) Debug.LogError("[WorldGenerator] player not assigned!");
        if (sandLayer == null || grassLayer == null || mountainLayer == null)
            Debug.LogError("[WorldGenerator] Tilemap layers not assigned!");

        // Не блокируем Start! Просто ставим в очередь начальные чанки.
        EnqueueChunksAroundPlayer();
    }

    void Update()
    {
        if (player == null) return;

        // применяем рабочую очередь (запускает воркер если нужно)
        if (!queueWorkerRunning && generateQueue.Count > 0)
            _ = ProcessQueueWorker(); // запустить без await

        if (Vector3.Distance(player.position, lastPlayerPosition) > updateDistanceThreshold)
        {
            lastPlayerPosition = player.position;
            EnqueueChunksAroundPlayer();
            UnloadFarChunks();
        }
    }

    // Ставим в очередь чанки вокруг игрока (не генерируем синхронно)
    private void EnqueueChunksAroundPlayer()
    {
        var playerChunk = WorldPosToChunkCoord(player.position);
        int cx0 = playerChunk.Item1 - loadRadius;
        int cx1 = playerChunk.Item1 + loadRadius;
        int cy0 = playerChunk.Item2 - loadRadius;
        int cy1 = playerChunk.Item2 + loadRadius;

        for (int cx = cx0; cx <= cx1; cx++)
        {
            for (int cy = cy0; cy <= cy1; cy++)
            {
                var key = (cx, cy);
                if (loadedChunks.Contains(key)) continue;
                if (enqueuedChunks.Contains(key)) continue;

                enqueuedChunks.Add(key);
                generateQueue.Enqueue(key);
            }
        }
    }

    // Удаляем чанки, которые вышли за радиус
    private void UnloadFarChunks()
    {
        var playerChunk = WorldPosToChunkCoord(player.position);
        int playerChunkX = playerChunk.Item1;
        int playerChunkY = playerChunk.Item2;

        var toRemove = new List<(int, int)>();

        foreach (var c in loadedChunks)
        {
            if (Mathf.Abs(c.Item1 - playerChunkX) > loadRadius || Mathf.Abs(c.Item2 - playerChunkY) > loadRadius)
                toRemove.Add(c);
        }

        foreach (var r in toRemove)
        {
            ClearChunk(r.Item1, r.Item2);
            loadedChunks.Remove(r);
        }

        // Если были запланированные чанки вне радиуса — помечаем, что они не нужны
        var enqRemove = new List<(int, int)>();
        foreach (var c in enqueuedChunks)
        {
            if (Mathf.Abs(c.Item1 - playerChunkX) > loadRadius || Mathf.Abs(c.Item2 - playerChunkY) > loadRadius)
                enqRemove.Add(c);
        }
        foreach (var r in enqRemove) enqueuedChunks.Remove(r);
        // генерующая очередь всё ещё может содержать старые записи — воркер пропустит их, если их нет в enqueuedChunks
    }

    private (int, int) WorldPosToChunkCoord(Vector3 worldPos)
    {
        int cellX = Mathf.FloorToInt(worldPos.x / worldGrid.cellSize.x);
        int cellY = Mathf.FloorToInt(worldPos.y / worldGrid.cellSize.y);
        int chunkX = Mathf.FloorToInt((float)cellX / chunkSize);
        int chunkY = Mathf.FloorToInt((float)cellY / chunkSize);
        return (chunkX, chunkY);
    }

    // Рабочий воркер, выполняет по одному чанку за кадр (можно менять)
    private async UniTaskVoid ProcessQueueWorker()
    {
        if (queueWorkerRunning) return;
        queueWorkerRunning = true;

        try
        {
            // Пока в очереди есть элементы
            while (generateQueue.Count > 0)
            {
                (int, int) key;
                lock (generateQueue)
                {
                    if (generateQueue.Count == 0) break;
                    key = generateQueue.Dequeue();
                }

                // Если этот чанк уже отменён (removed из enqueuedChunks), пропускаем
                if (!enqueuedChunks.Contains(key))
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    continue;
                }

                // Убираем из enqueued — он сейчас будет обрабатываться
                enqueuedChunks.Remove(key);

                // Генерируем (фон + main-thread apply внутри)
                await GenerateChunkAsync(key.Item1, key.Item2);

                // отметим как загруженный
                loadedChunks.Add(key);

                // После каждого чанка отдаём кадр движку — предотвращаем зависание
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        finally
        {
            queueWorkerRunning = false;
        }
    }

    // Генерация чанка: heavy work на пуле потоков, применение в main thread
    private async UniTask GenerateChunkAsync(int chunkX, int chunkY)
    {
        // Генерируем типы тайлов на пуле потоков
        TileType[] tileTypes = await UniTask.RunOnThreadPool(() => GenerateTileTypesArray(chunkX, chunkY));

        // Затем применяем на главном потоке (ставим тайлы и спавним объекты)
        ApplyChunk(chunkX, chunkY, tileTypes);
    }

    // Возвращает одномерный массив TileType длиной chunkSize*chunkSize, индекс: y*chunkSize + x
    private TileType[] GenerateTileTypesArray(int chunkX, int chunkY)
    {
        int area = chunkSize * chunkSize;
        var types = new TileType[area];

        // Уникальный сид для чанка
        int localSeed = seed ^ (chunkX * 73856093) ^ (chunkY * 19349663);
        System.Random localRng = new System.Random(localSeed);

        // Смещения для разных карт шума
        float offsetX = (float)localRng.NextDouble() * 10000f;
        float offsetY = (float)localRng.NextDouble() * 10000f;
        float moistureOffsetX = (float)localRng.NextDouble() * 20000f;
        float moistureOffsetY = (float)localRng.NextDouble() * 20000f;
        float tempOffsetX = (float)localRng.NextDouble() * 30000f;
        float tempOffsetY = (float)localRng.NextDouble() * 30000f;

        int idx = 0;
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int globalX = chunkX * chunkSize + x;
                int globalY = chunkY * chunkSize + y;

                // Карта высот
                float height = GetFractalNoise(globalX + offsetX, globalY + offsetY);

                // Карта влажности
                float moisture = GetFractalNoise(globalX + moistureOffsetX, globalY + moistureOffsetY);

                // Карта температуры (градиент сверху вниз + шум)
                float latitudeFactor = 1f - (globalY / 2000f); // карта по вертикали
                float temperature = Mathf.Clamp01(latitudeFactor + GetFractalNoise(globalX + tempOffsetX, globalY + tempOffsetY) * 0.25f);

                // Выбираем биом
                Biome biome;
                if (height < 0.25f)
                {
                    biome = Biome.Sand; // низины → пустыня
                }
                else if (height < 0.6f)
                {
                    if (moisture > 0.55f)
                        biome = Biome.Forest; // влажность + средняя высота → лес
                    else
                        biome = Biome.Grass;  // иначе → трава
                }
                else if (height < 0.8f)
                {
                    biome = Biome.Grass; // холмы без леса
                }
                else
                {
                    biome = Biome.Mountain; // вершины → горы
                }

                // Маппинг биома в TileType
                switch (biome)
                {
                    case Biome.Sand: types[idx++] = TileType.Sand; break;
                    case Biome.Grass: types[idx++] = TileType.Grass; break;
                    case Biome.Forest: types[idx++] = TileType.Grass; break; // лес = трава + объекты
                    case Biome.Mountain: types[idx++] = TileType.Mountain; break;
                }
            }
        }

        return types;
    }

    // Применение чанка: создаём массивы точного размера и передаём в SetTilesBlock, затем спавним объекты (на траве)
    private void ApplyChunk(int chunkX, int chunkY, TileType[] tileTypes)
    {
        int area = chunkSize * chunkSize;
        var sandArr = new TileBase[area];
        var grassArr = new TileBase[area];
        var mountainArr = new TileBase[area];

        // Локальный RNG для детерминированного спавна объектов в чанке
        int localSeed = seed ^ (chunkX * 73856093) ^ (chunkY * 19349663);
        System.Random localRng = new System.Random(localSeed + 1234567);

        var spawnedObjects = new List<GameObject>();

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int idx = y * chunkSize + x;
                int globalX = chunkX * chunkSize + x;
                int globalY = chunkY * chunkSize + y;

                var type = tileTypes[idx];
                switch (type)
                {
                    case TileType.Sand:
                        sandArr[idx] = sandTile;
                        grassArr[idx] = null;
                        mountainArr[idx] = null;
                        break;
                    case TileType.Grass:
                        sandArr[idx] = null;
                        grassArr[idx] = grassTile;
                        mountainArr[idx] = null;

                        // Спавн объекта на траве
                        if (objectPrefabs.Length > 0 && localRng.NextDouble() < objectSpawnChance)
                        {
                            var prefab = objectPrefabs[localRng.Next(objectPrefabs.Length)];
                            if (prefab != null && objectPools.TryGetValue(prefab, out var pool))
                            {
                                var go = pool.Get();
                                var cell = new Vector3Int(globalX, globalY, 0);
                                var worldPos = worldGrid.CellToWorld(cell);
                                Vector3 halfOffset = new Vector3(worldGrid.cellSize.x, worldGrid.cellSize.y, 0f) * 0.5f;
                                go.transform.position = worldPos + halfOffset;
                                go.transform.SetParent(transform, true);
                                spawnedObjects.Add(go);
                            }
                        }
                        break;
                    case TileType.Mountain:
                        sandArr[idx] = null;
                        grassArr[idx] = null;
                        mountainArr[idx] = mountainTile;
                        break;
                }
            }
        }

        var origin = new Vector3Int(chunkX * chunkSize, chunkY * chunkSize, 0);
        var bounds = new BoundsInt(origin.x, origin.y, 0, chunkSize, chunkSize, 1);

        // Устанавливаем блоки для каждого слоя
        sandLayer.SetTilesBlock(bounds, sandArr);
        grassLayer.SetTilesBlock(bounds, grassArr);
        mountainLayer.SetTilesBlock(bounds, mountainArr);

        // Сохраняем объекты для возврата в пул при выгрузке
        chunkObjects[(chunkX, chunkY)] = spawnedObjects;
    }

    private void ClearChunk(int chunkX, int chunkY)
    {
        int area = chunkSize * chunkSize;
        var clear = new TileBase[area];
        for (int i = 0; i < area; i++) clear[i] = null;

        var origin = new Vector3Int(chunkX * chunkSize, chunkY * chunkSize, 0);
        var bounds = new BoundsInt(origin.x, origin.y, 0, chunkSize, chunkSize, 1);

        sandLayer.SetTilesBlock(bounds, clear);
        grassLayer.SetTilesBlock(bounds, clear);
        mountainLayer.SetTilesBlock(bounds, clear);

        if (chunkObjects.TryGetValue((chunkX, chunkY), out var objs))
        {
            foreach (var o in objs)
            {
                if (o == null) continue;
                var tag = o.GetComponent<PooledInstance>();
                if (tag != null && tag.originalPrefab != null && objectPools.TryGetValue(tag.originalPrefab, out var pool))
                {
                    pool.Release(o);
                }
                else
                {
                    Destroy(o);
                }
            }
            chunkObjects.Remove((chunkX, chunkY));
        }
    }

    private float GetFractalNoise(float x, float y)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float maxAmp = 0f;
        float total = 0f;

        for (int o = 0; o < octaves; o++)
        {
            float sampleX = (x * frequency) * globalScale;
            float sampleY = (y * frequency) * globalScale;
            float per = Mathf.PerlinNoise(sampleX, sampleY);
            total += per * amplitude;
            maxAmp += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return Mathf.Clamp01(total / maxAmp);
    }

    // Возвращает модификатор скорости для мира (grass=1.0, sand=0.75, mountain=0.25).
    public float GetSpeedModifierAtWorldPos(Vector3 worldPos)
    {
        int cellX = Mathf.FloorToInt(worldPos.x / worldGrid.cellSize.x);
        int cellY = Mathf.FloorToInt(worldPos.y / worldGrid.cellSize.y);
        var cell = new Vector3Int(cellX, cellY, 0);

        if (grassLayer.HasTile(cell)) return 1.0f;
        if (sandLayer.HasTile(cell)) return 0.75f;
        if (mountainLayer.HasTile(cell)) return 0.25f;
        return 1.0f;
    }

    [ContextMenu("ForceReloadAllChunks")]
    public void ForceReloadAllChunks()
    {
        var copyLoaded = new List<(int, int)>(loadedChunks);
        foreach (var c in copyLoaded)
        {
            ClearChunk(c.Item1, c.Item2);
            loadedChunks.Remove(c);
        }

        var copyEnq = new List<(int, int)>(enqueuedChunks);
        foreach (var c in copyEnq) enqueuedChunks.Remove(c);

        lock (generateQueue)
        {
            generateQueue.Clear();
        }

        EnqueueChunksAroundPlayer();
    }
}
