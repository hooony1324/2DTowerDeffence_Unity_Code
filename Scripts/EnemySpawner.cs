using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


// EnemyType, ObjectPooler의 Tag와 동일하게

public class EnemySpawner : MonoBehaviour
{
    private Vector3 spawnPosition;

    private Grid grid;

    [SerializeField]
    private GameObject EnemyGoal;
    private int2[] goalPositions; 

    private Pathfinding pathFinding;
    private PathRenderer pathRenderer;
    private int2 startPosition;
    private int[] pathPositions;
    private List<Vector3> pathRenderPositions;

    // Enemy Spawn
    [SerializeField]
    private List<GameObject> enemyTypeObjects;
    private Enemy[] enemyTypes;

    //private List<Enemy> spawnedEnemies;
    //public List<Enemy> SpawnedEnemies { get { return spawnedEnemies; } }


    // Grid는 Awake에서 초기화되서 Start시점 이용
    private void Start()
    {
        spawnPosition = transform.position;
        pathFinding = GetComponent<Pathfinding>();
        pathRenderer = GetComponent<PathRenderer>();

        grid = GameManager.Instance.GetGrid();
        grid.GetIndex(spawnPosition, out startPosition.x, out startPosition.y);

        // Enemy Goal
        goalPositions = new int2[4];
        Tile[] goaltiles = EnemyGoal.GetComponentsInChildren<Tile>();
        for (int i = 0; i < 4; i++)
        {
            int x, y;
            Vector3 goalPosition = goaltiles[i].transform.position;
            grid.GetIndex(goalPosition, out x, out y);
            goalPositions[i] = new int2(x, y);
        }

        pathRenderPositions = new List<Vector3>();

        // Enemy Spawn
        //spawnedEnemies = new List<Enemy>();
        enemyTypes = new Enemy[enemyTypeObjects.Count];
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            enemyTypes[i] = enemyTypeObjects[i].GetComponent<Enemy>();
        }

        // Debugging
        GameManager.Instance.AddDebugHandler(SetEnemiesDebug);

        FindPath();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnEnemey(EnemyType.Enemy01);
        }

    }


    public bool FindPath()
    {
        float startTime = Time.realtimeSinceStartup;
        pathPositions = pathFinding.FindPath(startPosition, goalPositions);

        if (pathPositions.Length == 0)
        {
            return false;
        }

        float time = ((Time.realtimeSinceStartup - startTime) * 1000f);
        Debug.Log($"Time: {time,4:f2}ms");

        SetLineRenderPath(pathPositions);

        return true;
    }

    public void RecalculateEnemiesPath(Vector3 newTilePosition)
    {
        List<Enemy> spawnedEnemies = ObjectPooler.GetAllPools<Enemy>("Enemy");

        foreach (Enemy enemy in spawnedEnemies)
        {
            if (enemy.IsDestroyed() == false)
            {
                if (enemy.IsPositionInPath(newTilePosition))
                {
                    enemy.ChangeState(EnemyState.RecalculatePath);
                }
            }
        }
    }

    public void RenderPath(bool isPlay)
    {
        if (isPlay)
        {
            pathRenderer.PlayLine();
        }
        else
        {
            pathRenderer.StopLine();
        }
    }

    private void SetLineRenderPath(int[] pathPositions)
    {
        pathRenderPositions.Clear();

        for (int i = 0; i < pathPositions.Length; ++i)
        {
            int width = grid.Width;
            int x = pathPositions[i] % width;
            int y = pathPositions[i] / width;
            Vector3 worldPosition = grid.GetWorldPosition(x, y);
            pathRenderPositions.Add(new Vector3(worldPosition.x, worldPosition.y, 0f));
        }

        pathRenderer.SetPath(pathRenderPositions);
    }

    private Enemy SpawnEnemey(EnemyType type)
    {
        Enemy spawnedEnemy = ObjectPooler.SpawnFromPool<Enemy>("Enemy", transform.position);
        spawnedEnemy.transform.position = spawnPosition;
        spawnedEnemy.Setup(type, goalPositions);
        spawnedEnemy.gameObject.SetActive(true);
        spawnedEnemy.ChangeState(EnemyState.RecalculatePath);
        return spawnedEnemy;
    }

    private void SetEnemiesDebug(bool value)
    {
        List<Enemy> spawnedEnemies = ObjectPooler.GetAllPools<Enemy>("Enemy");

        foreach (Enemy enemy in spawnedEnemies)
        {
            if (enemy.IsDestroyed() == false)
            {
                enemy.SetDebugText(value);
            }
        }
    }


}
