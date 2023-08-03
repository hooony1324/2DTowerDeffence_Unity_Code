using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public enum TowerState { SearchTarget, Attack, Pause };

public class Tower : MonoBehaviour
{
    [SerializeField]
    protected GameObject projectileObj;
    [SerializeField]
    protected List<GameObject> spawnPoints;
    [SerializeField]
    protected float attackRange = 5f;
    [SerializeField]
    protected float attackRate = 0.5f;


    protected Transform attackTarget = null;
    protected TowerState towerState;

    protected virtual void Awake()
    {
        SetPlay(false);
    }

    protected virtual void Fire()
    {
    }

    public void ChangeState(TowerState state)
    {
        StopCoroutine(towerState.ToString());

        towerState = state;

        StartCoroutine(towerState.ToString());
    }

    protected virtual void Update()
    {
        if (attackTarget != null)
        {
            RotateToTarget();
        }
    }

    protected virtual IEnumerator SearchTarget()
    {
        

        while (true)
        {
            if (attackTarget != null)
            {
                ChangeState(TowerState.Attack);
            }

            yield return null;

            FindNearestEnemy();
        }
    }

    protected virtual IEnumerator Attack()
    {

        while (true)
        {
            // 타겟 잃었으면
            if (attackTarget == null)
            {
                ChangeState(TowerState.SearchTarget);
                break;
            }

            // 공격 범위 밖이면
            float distance = Vector3.Distance(attackTarget.position, transform.position);
            if (distance > attackRange)
            {
                attackTarget = null;
                ChangeState(TowerState.SearchTarget);
                break;
            }

            yield return new WaitForSeconds(attackRate);

            // 공격
            Fire();
        }
    }

    protected virtual IEnumerator Pause()
    {
        attackTarget = null;

        while (true)
        {
            // 외부에서 Pause풀어줌


            yield return null;
        }

    }

    protected void FindNearestEnemy()
    {
        List<Enemy> spawnedEnemies = ObjectPooler.GetAllPools<Enemy>("Enemy");
        int count = spawnedEnemies.Count;
        if (count == 0)
        {
            return;
        }

        NativeList<float2> enemyPositions = new NativeList<float2>(count, Allocator.TempJob);
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            enemyPositions.Add(new float2(spawnedEnemies[i].transform.position.x, spawnedEnemies[i].transform.position.y));
        }

        // Target을 찾아준다
        NativeArray<int> findEnemyIndex = new NativeArray<int>(1, Allocator.TempJob);
        FindEnemyJob findEnemyJob = new FindEnemyJob()
        {
            result = findEnemyIndex,
            enemyPositions = enemyPositions,
            towerPosition = new float2(transform.position.x, transform.position.y)
        };

        JobHandle handle = findEnemyJob.Schedule();
        handle.Complete();

        int targetEnemyIndex = findEnemyIndex[0];
        findEnemyIndex.Dispose();

        if (targetEnemyIndex == -1)
        {
            attackTarget = null;
        }
        else
        {
            attackTarget = spawnedEnemies[targetEnemyIndex].transform;
        }

        enemyPositions.Dispose();
    }

    [BurstCompile]
    public struct FindEnemyJob : IJob
    {
        public NativeArray<int> result;
        public NativeList<float2> enemyPositions;
        public float2 towerPosition;

        public void Execute()
        {
            result[0] = -1;
            float minDistance = float.MaxValue;
            for (int i = 0; i < enemyPositions.Length; i++)
            {
                float newDistance = Vector2.Distance(towerPosition, enemyPositions[i]);

                if (newDistance < minDistance)
                {
                    result[0] = i;
                    minDistance = newDistance;
                }
            }

        }
    }

    protected void RotateToTarget()
    {
        Vector3 targetDir = (attackTarget.transform.position - transform.position).normalized;

        float dx = targetDir.x;
        float dy = targetDir.y;

        float degree = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, degree);
    }

    public void SetPlay(bool isPlay)
    {
        if (!isPlay)
        {
            ChangeState(TowerState.Pause);
        }
        else
        {
            ChangeState(TowerState.SearchTarget);
        }
    }

}
