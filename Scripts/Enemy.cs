using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public enum EnemyState { RecalculatePath, OnMove /*Stun, Slow, ...*/ }
public enum EnemyType { Enemy01, Enemy02, Enemy03 };

public class Enemy : MonoBehaviour
{
    private Movement2D movement;

    private int2[] goalPositions;
    private List<Vector3> pathPositions;
    private int pathindex = 0;
    private EnemyState state;
    private GameObject stateTextObj;
    private Pathfinding pathfinding;
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private List<Sprite> spritePrefabs;

    private void Awake()
    {
        pathPositions = new List<Vector3>();

        movement = GetComponent<Movement2D>();
        pathfinding = GetComponent<Pathfinding>();

        state = EnemyState.RecalculatePath;
        stateTextObj = transform.Find("StateText").gameObject;
        stateTextObj.SetActive(GameManager.Instance.DebugMode);

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Object Pool Manage
    public void Setup(EnemyType type, int2[] goalPositions)
    {
        spriteRenderer.sprite = spritePrefabs[(int)type];
        this.goalPositions = new int2[goalPositions.Length];
        goalPositions.CopyTo(this.goalPositions, 0);
    }

    public void OnEnable()
    {

    }

    public void OnDisable()
    {
        StopCoroutine(state.ToString());

        ObjectPooler.ReturnToPool(gameObject);
    }
    // ~Object Pool Manage



    public void ChangeState(EnemyState newState)
    {
        StopCoroutine(state.ToString());

        state = newState;
        stateTextObj.GetComponent<TextMesh>().text = state.ToString();
        
        StartCoroutine(state.ToString());
    }

    public void SetWorldPathPositions(List<Vector3> worldPathPositions)
    {
        pathPositions = worldPathPositions;
    }

    private IEnumerator RecalculatePath()
    {
        CalculateFirstPoint();

        while (true)
        {
            if (Vector3.Distance(transform.position, pathPositions[pathindex]) < 0.02f * movement.MoveSpeed)
            {
                break;
            }

            yield return null;
        }

        ChangeState(EnemyState.OnMove);
    }

    private IEnumerator OnMove()
    {
        NextMoveTo();

        while (true)
        {
            if (Vector3.Distance(transform.position, pathPositions[pathindex]) < 0.02f * movement.MoveSpeed)
            {
                NextMoveTo();
            }

            yield return null;
        }
        
    }


    // path가 업데이트 되면 다른 경로로 변경되는 과정
    private void CalculateFirstPoint()
    {
        if (false == this.gameObject.activeInHierarchy)
        {
            return;
        }

        // 현재위치 기준, Pathfinding
        int startX, startY;
        GameManager.Instance.GetGrid().GetIndex(transform.position, out startX, out startY);

        int2 startPoint = new int2(startX, startY);
        int[] path = pathfinding.FindPath(startPoint, goalPositions);

        Grid grid = GameManager.Instance.GetGrid();
        pathPositions.Clear();
        for (int i = 0; i < path.Length; ++i)
        {
            int width = grid.Width;
            int x = path[i] % width;
            int y = path[i] / width;
            Vector3 worldPosition = grid.GetWorldPosition(x, y);
            pathPositions.Add(new Vector3(worldPosition.x, worldPosition.y, 0f));
        }


        // pathfind한 노드들중 가장 가까운 노드로 일단 움직임
        Vector3 curPosition = transform.position;
        Vector3 targetPosition = new Vector3();
        float distance = float.MaxValue;
        for (int i = 0; i < pathPositions.Count; i++)
        {
            float newDistance = Vector3.Distance(curPosition, pathPositions[i]);
            if (newDistance < distance)
            {
                distance = newDistance;
                targetPosition = pathPositions[i];
                pathindex = i;
            }
        }

        Vector3 targetDir = (targetPosition - curPosition).normalized;
        movement.MoveTo(targetDir);
    }

    // 계산이 완료된 Path위에서 다음 경로로 진행
    private void NextMoveTo()
    {
        if (pathindex > 0)
        {
            transform.position = pathPositions[pathindex];

            pathindex--;
            Vector3 targetDir = (pathPositions[pathindex] - transform.position).normalized;
            movement.MoveTo(targetDir);
        }
        // **목적지 도달**
        else if (pathindex == 0)
        {

            
            gameObject.SetActive(false);
        }
    }

    public void SetDebugText(bool value)
    {
        stateTextObj.SetActive(value);
    }

    public bool IsPositionInPath(Vector3 position)
    {
        if (pathPositions.Count == 0)
        {
            return false;
        }

        int x, y;
        Grid grid = GameManager.Instance.GetGrid();
        grid.GetIndex(position, out x, out y);

        for (int i = 0; i < pathPositions.Count; i++)
        {
            if (pathPositions[i].x == x && pathPositions[i].y == y)
            {
                return true;
            }
        }
        return false;
    }

}
