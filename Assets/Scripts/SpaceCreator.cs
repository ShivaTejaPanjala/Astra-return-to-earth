using System.Collections.Generic;
using UnityEngine;

public class SpaceCreator : MonoBehaviour
{
    [System.Serializable]
    public class StageData
    {
        public int obstacleCount = 250;
        public int powerCoreCount = 2;
        public float spawnInterval = 1f;
        public float spawnDistanceAhead = 200f;
        public int maxObstaclesPerSpawn = 10;
        public int minObstaclesPerSpawn = 5;
    }

    [SerializeField] private ParticleSystem travelParticles;
    [SerializeField] private float particleSpeedMin = 10f;
    [SerializeField] private float particleSpeedMax = 60f;

    public Transform player;
    public GameObject[] obstaclePrefabs;
    public GameObject boostCorePrefab;
    public GameObject bonusCorePrefab;
    public GameObject specialCorePrefab;

    [Range(0f, 1f)] public float coreChancePerObstacle = 0.08f;
    [Range(0f, 1f)] public float boostCoreChance = 0.3f;
    [Range(0f, 1f)] public float bonusCoreChance = 0.6f;

    public float coreSpawnZOffsetMin = 5f;
    public float coreSpawnZOffsetMax = 18f;

    public StageData[] stages;

    public float laneX = 8f;
    public float laneY = 4f;

    public float destroyBehindDistance = 40f;

    public bool enableClusterSpawning = true;
    public int clusterSizeMin = 3;
    public int clusterSizeMax = 7;
    public float clusterSpread = 3f;

    public bool enableSmartTargeting = true;
    public float predictionStrength = 0.5f;
    [SerializeField] private float predictionDistance = 5f;

    public float difficultyMultiplier = 0.002f;
    public float maxDifficultyBoost = 3f;

    public Camera mainCamera;
    public float baseFOV = 70f;
    public float maxFOV = 100f;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private int currentStageIndex;
    private int spawnedObstacleCount;
    private int spawnedCoreCount;

    private float nextSpawnZ;
    private float nextSpawnTime;

    private float intensity;
    private float distanceTravelled;

    private void Start()
    {
        if (player == null || stages == null || stages.Length == 0)
            return;

        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main;

        currentStageIndex = 0;
        nextSpawnZ = player.position.z + stages[currentStageIndex].spawnDistanceAhead;
        nextSpawnTime = Time.time + stages[currentStageIndex].spawnInterval;
    }

    private void Update()
    {
        if (player == null || stages == null || stages.Length == 0)
            return;

        distanceTravelled += Time.deltaTime * 10f;
        intensity = Mathf.Clamp01(distanceTravelled * difficultyMultiplier);

        StageData stage = stages[currentStageIndex];
        float difficulty = Mathf.Lerp(1f, maxDifficultyBoost, intensity);

        if (Time.time >= nextSpawnTime && !IsStageComplete(stage))
        {
            nextSpawnZ = player.position.z + stage.spawnDistanceAhead;
            SpawnStageChunk(nextSpawnZ, stage);
            nextSpawnTime = Time.time + (stage.spawnInterval / difficulty);

            if (IsStageComplete(stage))
                AdvanceStage();
        }

        UpdateTravelParticles();
        UpdateCameraFOV();
        CleanupOldObjects();
    }

    private void UpdateTravelParticles()
    {
        if (travelParticles == null) return;

        var main = travelParticles.main;
        float speed = Mathf.Lerp(particleSpeedMin, particleSpeedMax, intensity);
        main.startSpeed = speed;

        var emission = travelParticles.emission;
        emission.rateOverTime = Mathf.Lerp(20f, 150f, intensity);
    }

    private void UpdateCameraFOV()
    {
        if (mainCamera == null) return;

        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, intensity);
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * 3f);
    }

    private void SpawnStageChunk(float zPos, StageData stage)
    {
        int obstaclesToSpawn = Mathf.RoundToInt(
            Random.Range(stage.minObstaclesPerSpawn, stage.maxObstaclesPerSpawn + 1) * (1f + intensity)
        );

        for (int i = 0; i < obstaclesToSpawn && spawnedObstacleCount < stage.obstacleCount; i++)
        {
            if (enableClusterSpawning && Random.value > Mathf.Lerp(0.8f, 0.3f, intensity))
            {
                int cluster = SpawnCluster(zPos);
                spawnedObstacleCount += Mathf.Min(cluster, stage.obstacleCount - spawnedObstacleCount);
            }
            else
            {
                SpawnRandomObstacle(zPos);
                spawnedObstacleCount++;
            }
        }

        if (spawnedCoreCount < stage.powerCoreCount)
        {
            TrySpawnCore(zPos);
        }
    }

    private int SpawnCluster(float zPos)
    {
        int clusterSize = Mathf.RoundToInt(
            Random.Range(clusterSizeMin, clusterSizeMax) * (1f + intensity)
        );

        Vector3 center = new Vector3(
            Random.Range(-laneX, laneX),
            Random.Range(-laneY, laneY),
            zPos
        );

        for (int i = 0; i < clusterSize; i++)
        {
            Vector3 offset = Random.insideUnitSphere * clusterSpread;
            offset.z = 0f;
            SpawnObstacleAt(center + offset);
        }

        return clusterSize;
    }

    private void SpawnRandomObstacle(float zPos)
    {
        Vector3 pos = new Vector3(
            Random.Range(-laneX, laneX),
            Random.Range(-laneY, laneY),
            zPos
        );

        SpawnObstacleAt(pos);
    }

    private void SpawnObstacleAt(Vector3 pos)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return;

        int index = Random.Range(0, obstaclePrefabs.Length);
        GameObject obj = Instantiate(obstaclePrefabs[index], pos, Quaternion.identity);

        if (enableSmartTargeting && player != null)
        {
            Vector3 predictedPos = player.position + player.forward * predictionStrength * predictionDistance;
            Vector3 dir = (predictedPos - pos).normalized;
            obj.transform.forward = dir;
        }

        spawnedObjects.Add(obj);
    }

    private void TrySpawnCore(float zPos)
    {
        if (Random.value > coreChancePerObstacle)
            return;

        GameObject prefab = SelectCorePrefab();
        if (prefab == null)
            return;

        Vector3 pos = new Vector3(
            Random.Range(-laneX, laneX),
            Random.Range(-laneY, laneY),
            zPos + Random.Range(coreSpawnZOffsetMin, coreSpawnZOffsetMax)
        );

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        spawnedObjects.Add(obj);
        spawnedCoreCount++;
    }

    private GameObject SelectCorePrefab()
    {
        float roll = Random.value;
        if (roll < boostCoreChance) return boostCorePrefab;
        if (roll < boostCoreChance + bonusCoreChance) return bonusCorePrefab;
        return specialCorePrefab;
    }

    private bool IsStageComplete(StageData stage)
    {
        return spawnedObstacleCount >= stage.obstacleCount;
    }

    private void AdvanceStage()
    {
        if (currentStageIndex >= stages.Length - 1)
            return;

        currentStageIndex++;
        spawnedObstacleCount = 0;
        spawnedCoreCount = 0;

        nextSpawnZ = player.position.z + stages[currentStageIndex].spawnDistanceAhead;
        nextSpawnTime = Time.time + stages[currentStageIndex].spawnInterval;
    }

    private void CleanupOldObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            if (spawnedObjects[i].transform.position.z < player.position.z - destroyBehindDistance)
            {
                Destroy(spawnedObjects[i]);
                spawnedObjects.RemoveAt(i);
            }
        }
    }
}