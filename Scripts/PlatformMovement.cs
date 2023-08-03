using System.Collections;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    [SerializeField]
    private Transform startPoint;
    [SerializeField]
    private Transform endPoint;
    [SerializeField]
    private float moveTime = 1.0f;
    [SerializeField]
    private AnimationCurve animationCurve;
    [SerializeField]
    private float waitTime = 1.0f;

    private IEnumerator MoveStart()
    {
        float percent = 0;
        float current = 0;

        while (true) 
        {
            current += Time.deltaTime;
            percent = current / moveTime;

            Vector3 newPos = Vector3.Lerp(startPoint.position, endPoint.position, animationCurve.Evaluate(percent));
            transform.position = newPos;

            // 목적지 도착 & 출발지와 역전
            float distance = Vector3.Distance(transform.position, endPoint.position);
            if (distance < 0.02f)
            {
                yield return new WaitForSeconds(waitTime);

                Transform temp = startPoint;
                startPoint = endPoint;
                endPoint = temp;
                current = 0.0f;
            }

            yield return null;
        }
    }

    private void Start()
    {
        StartCoroutine(nameof(MoveStart));

    }
}
