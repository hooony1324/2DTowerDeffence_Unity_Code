using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetProjectile : Projectile
{
    protected override void Awake()
    {
        base.Awake();


    }

    protected override void Update()
    {
        base.Update();
    }

    public override void SetUp(Transform target, Vector2 direction)
    {
        base.SetUp(target, direction);

        projectilespeed = 20.0f;
        movement.MoveSpeed = projectilespeed;
    }

    public override void Launch()
    {
        base.Launch();

        movement.MoveSpeed = projectilespeed;
        movement.MoveTo(launchDirection);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;
        if (collision.transform != target) return;

        // enemy¿¡°Ô damage


        this.gameObject.SetActive(false);
    }
}
