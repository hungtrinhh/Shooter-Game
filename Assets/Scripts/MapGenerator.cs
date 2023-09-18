using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator: MonoBehaviour {
    //Variables
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform underground;
    public Transform navmeshMaskPrefab;

    public Vector2 maxMapSize;
    [Range (0, 1)]
    public float borderThickness;
    public float tileSize;

    public Map[] maps;
    public int mapIndex;


    Map currentMap;
    Transform mapHolder;
    Transform[,] tileMap;
    List<Coord> allOpenTileCoordinates;
    Queue<Coord> shuffledTileCoordinates;
    Queue<Coord> shuffledOpenTileCoordinates;

    void Awake () {
        GenerateMap ();
    }
    void Start () {
        FindObjectOfType<Spawner> ().OnNewWave += OnNewWave;
    }

    void OnNewWave (int waveNumber) {
        mapIndex = waveNumber - 1;
        GenerateMap ();
    }

    public void GenerateMap () {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        CreateCoordinates ();
        GenerateMapHolderObject ();
        CreateTileMap ();
        AddObstacles ();
        CreateNavMesh ();

    }

    void CreateCoordinates () {
        List<Coord> tileCoordinates = new List<Coord> ();
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                tileCoordinates.Add (new Coord (x, y));
            }
        }
        allOpenTileCoordinates = new List<Coord> (tileCoordinates);
        shuffledTileCoordinates = new Queue<Coord> (Utility.ShuffleArray (tileCoordinates.ToArray (), currentMap.seed));
    }

    void GenerateMapHolderObject () {
        string holderName = "Generated Map";
        if (transform.Find (holderName)) {
            DestroyImmediate (transform.Find (holderName).gameObject);
        }
        mapHolder = new GameObject (holderName).transform;
        mapHolder.parent = transform;
    }

    void CreateTileMap () {
        for (int x = 0; x < currentMap.mapSize.x; x++) {
            for (int y = 0; y < currentMap.mapSize.y; y++) {
                Vector3 tilePosition = CoordToPosition (x, y);
                Transform newTile = Instantiate (tilePrefab, tilePosition, Quaternion.Euler (Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - borderThickness) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }
    }

    void AddObstacles () {
        System.Random prng = new System.Random (currentMap.seed);
        bool[,] obstacleMap = new bool[(int) currentMap.mapSize.x, (int) currentMap.mapSize.y];
        int obstacleCount = (int) (currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        for (int i = 0; i < obstacleCount; i++) {
            Coord randomCoord = GetRandomCoord ();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;
            if (randomCoord != currentMap.mapCentre && MapIsTraversable (obstacleMap, currentObstacleCount)) {
                float obstacleHeight = Mathf.Lerp (currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float) prng.NextDouble ());
                float obstacleRadius = (1 - borderThickness) * tileSize;
                Vector3 obstaclePosition = CoordToPosition (randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate (obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
                newObstacle.localScale = new Vector3 (obstacleRadius, obstacleHeight, obstacleRadius);
                newObstacle.parent = mapHolder;

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer> ();
                Material obstacleMaterial = new Material (obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float) currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp (currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenTileCoordinates.Remove (randomCoord);
            } else {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
        shuffledOpenTileCoordinates = new Queue<Coord> (Utility.ShuffleArray (allOpenTileCoordinates.ToArray (), currentMap.seed));
    }

    void CreateNavMesh () {
        Transform maskLeft = Instantiate (navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        Transform maskRight = Instantiate (navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        Transform maskFwd = Instantiate (navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        Transform maskBck = Instantiate (navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;

        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        maskFwd.parent = mapHolder;
        maskFwd.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
        maskBck.parent = mapHolder;
        maskBck.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        navmeshFloor.localScale = new Vector3 (maxMapSize.x, maxMapSize.y) * tileSize;
        underground.localScale = new Vector3 (currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);
    }

    //Compares the number of tiles accessible from the center to the number that should be accessible
    //flood fill algorithm - start at center and expand radius
    bool MapIsTraversable (bool[,] obstacleMap, int currentObstacleCount) {
        bool[,] mapFlags = new bool[obstacleMap.GetLength (0), obstacleMap.GetLength (1)];
        Queue<Coord> queue = new Queue<Coord> ();
        queue.Enqueue (currentMap.mapCentre);
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0) {
            Coord tile = queue.Dequeue ();

            //add accessible neighbours
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0) { //ignore diagonals
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength (0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength (1)) {//check if neighbour is inside map
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY]) { //check if that neighbour is an obstacle
                                mapFlags[neighbourX, neighbourY] = true; //set tile to accessible
                                queue.Enqueue (new Coord (neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTileCount = (int) (currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }





    /*--------- COORD STRUCT ---------*/
    [System.Serializable]
    public struct Coord {
        public int x;
        public int y;

        public Coord (int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator == (Coord c1, Coord c2) {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator != (Coord c1, Coord c2) {
            return !(c1 == c2);
        }
    }

    //returns the next element of the shuffledCoordinates queue to make sure you don't
    //receive the same tile twice
    public Coord GetRandomCoord () {
        Coord randomCoord = shuffledTileCoordinates.Dequeue ();
        shuffledTileCoordinates.Enqueue (randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile () {
        Coord randomCoord = shuffledOpenTileCoordinates.Dequeue ();
        shuffledTileCoordinates.Enqueue (randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    Vector3 CoordToPosition (int x, int y) {
        return new Vector3 (-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition (Vector3 position) {
        int x = Mathf.RoundToInt (position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt (position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp (x, 0, tileMap.GetLength (0) - 1);
        y = Mathf.Clamp (y, 0, tileMap.GetLength (1) - 1);
        return tileMap[x, y];
    }

    /*--------- MAP CLASS ---------*/
    [System.Serializable]
    public class Map {
        public Coord mapSize;
        [Range (0, 1)]
        public float obstaclePercent;
        public int seed;

        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCentre {
            get {
                return new Coord (mapSize.x / 2, mapSize.y / 2);
            }
        }

    }
}
