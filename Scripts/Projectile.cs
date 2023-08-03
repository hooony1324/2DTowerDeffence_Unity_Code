using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected Movement2D movement;
    protected Transform target;
    protected Vector2 launchDirection;

    protected float projectilespeed;
    protected float lifeTime = 3.0f;

    protected virtual void Awake()
    {
        movement = GetComponent<Movement2D>();

        
    }
    // float speed -> modifier stat �ϰ� ����
    public virtual void SetUp(Transform target, Vector2 direction)
    {
        this.target = target;
        this.launchDirection = direction;
        Invoke(nameof(DeactiveDelay), lifeTime);
    }

    private void DeactiveDelay() => gameObject.SetActive(false);

    // Object Pool Manage
    private void OnDisable()
    {
        ObjectPooler.ReturnToPool(gameObject);
        CancelInvoke();
    }


    protected virtual void Update()
    {
        // Ÿ���� ���߿� ������ ����
        if (false == target.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }


    public virtual void Launch()
    {
        gameObject.SetActive(true);
    }


}
