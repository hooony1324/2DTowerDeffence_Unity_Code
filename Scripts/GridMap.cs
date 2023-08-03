using UnityEngine;
using Unity.Mathematics;
using KSH.Utils;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;

// Grid를 이용하여 TileMap을 시각화
public class GridMap : MonoBehaviour
{
    // Grid Section
    [SerializeField]
    private int2 gridSize;
    public int2 GridSize => gridSize;
    private Grid grid;
    public Grid GetGrid()
    {
        return grid;
    }

    // Tile Setting Section
    [SerializeField]
    private GameObject tileStrongWall;
    [SerializeField]
    private GameObject tileEnemySpawn;
    [SerializeField]
    private GameObject tileTowerWall;
    [SerializeField]
    private List<GameObject> enemySpawnerObjects;
    private List<EnemySpawner> enemySpawners;
    public List<EnemySpawner> EnemySpawners => enemySpawners;

    private Dictionary<string, GameObject> tileDictionary;

    private void Awake()
    {
        grid = new Grid(gridSize.x, gridSize.y, 1, new Vector3(0, 0));
        enemySpawners = new List<EnemySpawner>();

        // 타일맵의 StrongWall및 EnemyEntry설정
        tileDictionary = new Dictionary<string, GameObject>();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                if (x == 0 || y == 0 || x == gridSize.x - 1 || y == gridSize.y - 1)
                {
                    SpawnTile(tileStrongWall, x, y);
                    
                }
            }
        }

        foreach (GameObject enemySpawnerObject in enemySpawnerObjects)
        {
            ChangeTile(tileEnemySpawn, enemySpawnerObject.transform.position);
            EnemySpawner enemySpawner = enemySpawnerObject.GetComponent<EnemySpawner>();
            enemySpawners.Add(enemySpawner);

            // debug binding
            GameManager.Instance.AddDebugHandler(enemySpawner.RenderPath);
        }


    }

    public void SpawnTowerBaseTile(Vector3 gridPosition)
    {
        bool posValid = grid.SetValue(gridPosition, 1);

        if (false == posValid)
        {
            return;
        }

        // 모든 길을 막는지 확인
        bool hasPath = false;
        foreach (EnemySpawner enemySpawner in enemySpawners)
        {
            hasPath |= enemySpawner.FindPath();
        }

        if (hasPath)
        {
            ChangeTile(tileTowerWall, gridPosition);
        }
        else
        {
            grid.SetValue(gridPosition, 0);
        }

        foreach (EnemySpawner enemySpawner in enemySpawners)
        {
            // 자신의 경로에 새로운 타일이 생긴 enemy만 추격
            enemySpawner.RecalculateEnemiesPath(gridPosition);
        }

    }

    public void DestroyTowerBaseTile(Vector3 gridPosition)
    {
        DestroyTile(gridPosition);

        foreach (EnemySpawner enemySpawner in enemySpawners)
        {
            //enemySpawner.FindPath();
            enemySpawner.RecalculateEnemiesPath(gridPosition);
        }
    }

    private void SpawnTile(GameObject tile, Vector3 position)
    {
        int x, y;
        grid.GetIndex(position, out x, out y);
        SpawnTile(tile, x, y);
    }

    private void SpawnTile(GameObject tile, int x, int y)
    {
        if (grid.GetValue(x, y) != 0)
        {
            return;
        }

        grid.SetValue(x, y, 1);
        GameObject tileObject = Instantiate<GameObject>(tile);
        tileObject.transform.position = grid.GetWorldPosition(x, y);
        tileObject.transform.parent = this.transform;
        string tileKey = $"tile_{x:D2}{y:D2}";
        tileObject.name = tileKey;
        tileDictionary.Add(tileKey, tileObject);
    }

    private void DestroyTile(Vector3 position)
    {
        int x, y;
        grid.GetIndex(position, out x, out y);
        DestroyTile(x, y);
    }

    private void DestroyTile(int x, int y)
    {
        string tileKey = $"tile_{x:D2}{y:D2}";

        GameObject tileObject;
        tileDictionary.TryGetValue(tileKey, out tileObject);
        if (tileObject != null)
        {
            Destroy(tileObject);
        }
        grid.SetValue(x, y, 0);
        tileDictionary.Remove(tileKey);
    }

    private void ChangeTile(GameObject tile, Vector3 position)
    {
        int x, y;
        grid.GetIndex(position, out x, out y);
        ChangeTile(tile, x, y);
    }

    private void ChangeTile(GameObject tile, int x, int y)
    {
        if (GetTileType(x, y) != TileType.StrongWall)
        {
            DestroyTile(x, y);
            SpawnTile(tile, x, y);
            return;
        }
        
        // EnemySpawn타일이면 StrongWallTile변경 가능
        Tile tileComponent = tile.GetComponent<Tile>();
        if (tileComponent.type == TileType.StartPoint)
        {
            DestroyTile(x, y);
            SpawnTile(tile, x, y);
        }

    }

    private TileType GetTileType(Vector3 position)
    {
        int x, y;
        grid.GetIndex(position, out x, out y);
        return GetTileType(x, y);
    }

    private TileType GetTileType(int x, int y)
    {
        GameObject tileObject;
        string tileKey = $"tile_{x:D2}{y:D2}";
        tileDictionary.TryGetValue(tileKey, out tileObject);

        if (tileObject == null)
        {
            return TileType.None;
        }

        Tile tile = tileObject.GetComponent<Tile>();
        return tile.type;
    }

    public Tile GetTile(Vector3 worldPosition)
    {
        int x, y;
        grid.GetIndex(worldPosition, out x, out y);

        GameObject tileObject;
        string tileKey = $"tile_{x:D2}{y:D2}";
        tileDictionary.TryGetValue(tileKey, out tileObject);

        if (tileObject == null)
        {
            return null;
        }

        Tile tile = tileObject.GetComponent<Tile>();

        return tile;
    }
 
}
