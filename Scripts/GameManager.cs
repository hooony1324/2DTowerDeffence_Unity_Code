using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSH.Utils;
using UnityEngine.EventSystems;

public enum GameState { GameReady, WaveStart, GameOver}

public delegate void DebugEventHandler(bool value);

public class DebugObjects
{
    public event DebugEventHandler debugEventHandler;

    public void OnDebugEvents(bool isDebug)
    {
        debugEventHandler(isDebug);
    }

}

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private GridMap gridMap;
    [SerializeField]
    private TowerBuilder towerBuilder;

    private GameState gameState;
    private GameObject GameStateTextObj;
    private TextMesh GameStateText;

    private bool debugMode = false;
    public bool DebugMode => debugMode;

    public GridMap GridMap => gridMap;
    public Grid GetGrid()
    {
        return gridMap.GetGrid();
    }

    // for debugging(use delegate)
    DebugObjects debugObjects = new DebugObjects();

    public void AddDebugHandler(DebugEventHandler debugEventHandler)
    {
        debugObjects.debugEventHandler += debugEventHandler;
    }

    public void RemoveDebugHandler(DebugEventHandler debugEventHandler)
    {
        debugObjects.debugEventHandler += debugEventHandler;
    }

    

    private void Awake()
    {
        GameStateTextObj = transform.Find("GameStateText").gameObject;
        GameStateText = GameStateTextObj.GetComponent<TextMesh>();
        GameStateTextObj.SetActive(false);
        AddDebugHandler(GameStateTextObj.SetActive);
        GameStateText.text = "GameState : " + gameState.ToString();

        ChangeState(GameState.GameReady);
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            debugMode = !debugMode;
            debugObjects.OnDebugEvents(debugMode);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            towerBuilder.AllTowersPlay(true);
        }
    }

    private void ChangeState(GameState state)
    {
        StopCoroutine(gameState.ToString());

        gameState = state;
        GameStateText.text = "GameState : " + gameState.ToString();

        StartCoroutine(gameState.ToString());
    }

    private IEnumerator GameReady()
    {
        // Start


        // Update
        while (true)
        {
            PlayerMapInput();

            yield return null;
        }

        // End

    }

    private IEnumerator WaveStart()
    {
        // Start
        towerBuilder.AllTowersPlay(true);
        // spawnenemies();

        // Update
        while (true)
        {
            PlayerMapInput();

            // 웨이브 종료
            if (true)
            {
                break;
            }

            yield return null;
        }

        // End
        towerBuilder.AllTowersPlay(false);
    }

    private IEnumerator GameOver()
    {
        // Start

        // Update
        while (true)
        {
            

            yield return null;
        }

        // End

    }

    private void PlayerMapInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // UI위를 클릭했으면
            if (EventSystem.current.IsPointerOverGameObject() == true)
            {
                return;
            }

            Vector3 gridPosition = UtilsClass.GetMouseWorldPosition();

            // TowerBase생성할지 체크
            if (towerBuilder.CurIndicatedIndex == 0)
            {
                if (GetGrid().GetValue(gridPosition) == 0)
                {
                    gridMap.SpawnTowerBaseTile(gridPosition);
                }
            }
            // 타워생성할지 체크
            else
            {
                // - grid 1체크, 타일에 타워 설치 되어 있는지 체크
                if (GetGrid().GetValue(gridPosition) == 1)
                {
                    // 타일에 타워 설치 되어 있는지 체크
                    Tile baseTile = gridMap.GetTile(gridPosition);
                    if (baseTile.isTowerAttached)
                    {
                        return;
                    }

                    // 타워 생성
                    towerBuilder.SpawnTower(gridPosition, baseTile);
                }
            }

        }

        if (Input.GetMouseButtonDown(1))
        {
            // 타워 철거 > 타일 제거
            
            // UI위를 클릭했으면
            if (EventSystem.current.IsPointerOverGameObject() == true)
            {
                return;
            }

            Vector3 gridPosition = UtilsClass.GetMouseWorldPosition();

            Tile baseTile = gridMap.GetTile(gridPosition);

            if (baseTile == null)
            {
                return;
            }

            if (true == baseTile.isTowerAttached)
            {
                towerBuilder.DestroyTower(baseTile);
            }
            else
            {
                gridMap.DestroyTowerBaseTile(gridPosition);
            }

        }
    }
}
