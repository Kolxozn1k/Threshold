using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct Wave
    {
        public string WaveName;
        public GameObject enemyPrefab; // префаб врага
        public int count;
        public float rate; // врагов в секунду

        public Wave(string waveName, GameObject enemyPrefab, int count, float rate)
        {
            this.WaveName = waveName;
            this.enemyPrefab = enemyPrefab;
            this.count = count;
            this.rate = rate;
        }
    }

    [Header("Настройки волн")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenWaves;

    [Header("Состояние")]
    private int currentWaveIndex = 0;
    private float WaveCountDown;
    private int aliveEnemiesCount;
    private bool isSpawning;

    public int currentWaveNumber => currentWaveIndex + 1;
    public int aliveEnemyCount => aliveEnemiesCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WaveCountDown = timeBetweenWaves;
    }

    // Update is called once per frame
    void Update()
    {
        if(isSpawning) return;

        if(aliveEnemiesCount > 0) return;

        if(WaveCountDown <= 0f)
        {
            if (currentWaveIndex < waves.Length)
            {
                StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            }
            else
            {
                // Здесь в будущем можно вызвать окно победы в UI
            }
        }
        else
        {
            WaveCountDown -= Time.deltaTime;
        }

        
    }

    IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;

        for(int i = 0; i < wave.count; i++) 
        {
            SpawnEnemy(wave.enemyPrefab);

            yield return new WaitForSeconds(1f / wave.rate);
        }

        isSpawning = false;
        currentWaveIndex++;
        WaveCountDown = timeBetweenWaves;

    }


    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Точки спавна (SpawnPoints) не назначены в WaveSpawner!");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogWarning("Префаб врага не назначен для этой волны!");
            return;
        }

        Transform RandomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newEnemy = Instantiate(enemyPrefab, RandomPoint.position, RandomPoint.rotation);

        aliveEnemiesCount++;

        StartCoroutine(TrackEnemyDeath(newEnemy));
    }

    IEnumerator TrackEnemyDeath(GameObject enemy)
    {
        enemyHealth health = enemy.GetComponent<enemyHealth>();

        while(health != null && (health == null || !health.isDead))
        {
            yield return new WaitForSeconds(0.5f);
        }

        aliveEnemiesCount--;
    }
}
