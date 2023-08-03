using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower02 : Tower
{
    protected override void Awake()
    {
        base.Awake();

    }

    protected override void Update()
    {
        base.Update();

    }

    protected override void Fire()
    {
        base.Fire();

        Vector3 spawnPoint = spawnPoints[0].transform.position;
        Projectile proj = ObjectPooler.SpawnFromPool<Projectile>("Projectile", spawnPoint);
        Vector3 dir = (attackTarget.transform.position - spawnPoint).normalized;
        proj.SetUp(attackTarget, dir);
        proj.Launch();
    }
}
