using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

enum RenderState { Rendering, Pausing }

public class PathRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private GameObject trailObj;
    private Vector3[] pathPositions;
    private RenderState state = RenderState.Rendering;
    private float trailDuration = 1.5f;
    private float waitTime = 0f;
    private float trailTime = 1.5f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        trailObj = transform.Find("TrailObj").gameObject;
        trailObj.GetComponent<TrailRenderer>().time = 0.2f;
    }

    private void Start()
    {
        PlayTrail();
    }

    public void SetPath(List<Vector3> path)
    {
        pathPositions = path.ToArray();
        Array.Reverse(pathPositions);

        lineRenderer.positionCount = pathPositions.Length;
        lineRenderer.SetPositions(pathPositions);
        
    }


    public void PlayLine()
    {
        lineRenderer.enabled = true;

    }

    public void StopLine()
    {
        lineRenderer.enabled = false;


    }

    public void PlayTrail()
    {
        StartCoroutine(state.ToString());
    }

    public void StopTrail()
    {
        StopCoroutine(state.ToString());
        trailObj.transform.position = pathPositions[0];
        trailObj.SetActive(false);
    }

    private IEnumerator Rendering()
    {
        if (pathPositions.Length == 0)
        {
            yield return null;
        }

        int index = 0;
        trailObj.transform.position = pathPositions[index];
        trailObj.SetActive(true);
        state = RenderState.Rendering;

        // 1Ä­ ÁøÇà, Lerp
        float trailOneDelta = trailTime / pathPositions.Length;
        float current = 0;

        while (true)
        {
            current += Time.deltaTime;
            float percent = current / trailOneDelta;
            Vector3 nextPosition = Vector3.Lerp(trailObj.transform.position, pathPositions[index], percent);
            trailObj.transform.position = nextPosition;

            if (current >= trailOneDelta)
            {
                current = 0;
                index++;
            }

            if (index >= pathPositions.Length)
            {
                StartCoroutine(nameof(Pausing));
                break;
            }

            yield return null;
        }
    }

    private IEnumerator Pausing()
    {
        if (pathPositions.Length == 0)
        {
            yield return null;
        }

        state = RenderState.Pausing;
        
        while (true)
        {
            waitTime += Time.deltaTime;

            if (waitTime > trailDuration)
            {
                waitTime = 0f;
                trailObj.SetActive(false);
                StartCoroutine(nameof(Rendering));
                break;
            }

            yield return null;
        }

    }
}
