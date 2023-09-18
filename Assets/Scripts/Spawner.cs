using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner: MonoBehaviour {

    public Wave[] waves;
    public Enemy enemy;
    public float spawnDelay = 1.5f;
    public float flashPerSecond = 4;
    public Color flashColor = Color.red;

    public bool developperMode;
    bool isEnabled = true;
    int waveIndex;
    Wave currentWave;
    int remainingEnemiesToSpawn;
    int remainingEnemiesAlive;
    float nextSpawnTime;

    MapGenerator map;
    LivingEntity player;
    Transform playerTransform;
    float allowedCampingTime = 2;
    float nextCampingCheckTime;
    float campingRadius = 1.5f; //how far the player has to move from his position to not be considered camping
    Vector3 lastPlayerPosition;
    bool isCamping;


    public event System.Action<int> OnNewWave;

    void Start () {
        player = FindObjectOfType<Player> ();
        player.OnDeath += OnPlayerDeath;
        playerTransform = player.transform;
        lastPlayerPosition = playerTransform.position;
        nextCampingCheckTime = Time.time + allowedCampingTime;

        map = FindObjectOfType<MapGenerator> ();
        NextWave ();
    }

    void Update () {
        if (isEnabled) {
            if (Time.time > nextCampingCheckTime) {
                nextCampingCheckTime = Time.time + allowedCampingTime;
                isCamping = (Vector3.Distance (playerTransform.position, lastPlayerPosition) < campingRadius);
                lastPlayerPosition = playerTransform.position;
            }

            if ((remainingEnemiesToSpawn > 0 || currentWave.infiniteWave) && Time.time > nextSpawnTime) {
                remainingEnemiesToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
                StartCoroutine ("SpawnEnemy");
            }

            if (developperMode) {
                if (Input.GetKeyDown (KeyCode.Return)) {
                    StopCoroutine ("SpawnEnemy");
                    foreach (Enemy e in FindObjectsOfType<Enemy> ()) {
                        GameObject.Destroy (e.gameObject);
                    }
                    NextWave ();
                }
            }
        }
    }

    IEnumerator SpawnEnemy () {

        Transform spawnTile = map.GetRandomOpenTile ();
        if (isCamping) {
            spawnTile = map.GetTileFromPosition (playerTransform.position);
        }
        Material tileMat = spawnTile.GetComponent<Renderer> ().material;
        Color originalTileColor = Color.white;
        float spawnTimer = 0;
        while (spawnTimer < spawnDelay) {
            spawnTimer += Time.deltaTime;
            tileMat.color = Color.Lerp (originalTileColor, flashColor, Mathf.PingPong (spawnTimer * flashPerSecond, 1));
            yield return null;
        }
        Enemy spawnedEnemy = Instantiate (enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics (currentWave.enemyMoveSpeed, currentWave.enemyHealth, currentWave.enemyStrength, currentWave.enemyColor);

    }

    void OnEnemyDeath () {
        remainingEnemiesAlive--;

        if (remainingEnemiesAlive == 0) {
            NextWave ();
        }
    }

    void OnPlayerDeath () {
        isEnabled = false;
    }

    void NextWave () {
        if (waveIndex > 0) {
            AudioManager.instance.PlaySoundEffect2D ("Level Complete");
        }
        waveIndex++;
        if (waveIndex - 1 < waves.Length) {
            currentWave = waves[waveIndex - 1];
            remainingEnemiesToSpawn = currentWave.enemyCount;
            remainingEnemiesAlive = remainingEnemiesToSpawn;
            OnNewWave?. Invoke(waveIndex);
        }
        nextCampingCheckTime = Time.time + allowedCampingTime;
        resetPlayerPosition ();
    }

    void resetPlayerPosition () {
        playerTransform.position = map.GetTileFromPosition (Vector3.zero).position + Vector3.up;
    }

    [System.Serializable]
    public class Wave {
        public bool infiniteWave;
        public int enemyCount;
        public float timeBetweenSpawns;

        public float enemyMoveSpeed;
        public int enemyStrength;
        public float enemyHealth;
        public Color enemyColor;
    }
}
