using System.Collections;
using UnityEngine;

public class DoorHandler : MonoBehaviour
{
    public GameObject Door;
    public Vector3 OpenAngles;
    public Vector3 CloseAngles;
    public float OpenDuration;
    public float CloseDuration;
    public float BetweenDelay;
    public float StartDelay;
    public float timer;

    public bool StartTimer { get; private set; }

    IEnumerator RotateRoutine(GameObject obj, Vector3 startRotationEuler, Vector3 endRotationEuler, float duration)
    {
        float elapsedTime = 0;
        Quaternion startRot = Quaternion.Euler(startRotationEuler);
        Quaternion endRot = Quaternion.Euler(endRotationEuler);

        while (elapsedTime < duration)
        {
            float percentageComplete = elapsedTime / duration;
            obj.transform.localRotation = Quaternion.Slerp(startRot, endRot, percentageComplete);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.localRotation = endRot;
    }

    IEnumerator StartOpenClose()
    {

        yield return new WaitForSeconds(StartDelay);
        yield return StartCoroutine(RotateRoutine(Door,CloseAngles, OpenAngles, OpenDuration));
        //StartTimer = true;
        yield return new WaitForSeconds(BetweenDelay);
        yield return StartCoroutine(RotateRoutine(Door, OpenAngles, CloseAngles, OpenDuration));
    }
    public void StartOpen()
    {
        StartCoroutine(StartOpenClose());
    }   
    
    public void StartOpenFromInside()
    {
        StartCoroutine(StartOpenFromInsideIE());
    }

    public float StartDelay_OpenFromInside;
    public float BetweenDelay_OpenFromInside;
    IEnumerator StartOpenFromInsideIE()
    {

        yield return new WaitForSeconds(StartDelay_OpenFromInside);
        yield return StartCoroutine(RotateRoutine(Door, CloseAngles, OpenAngles, OpenDuration));
        StartTimer = true;
        yield return new WaitForSeconds(BetweenDelay_OpenFromInside);
        yield return StartCoroutine(RotateRoutine(Door, OpenAngles, CloseAngles, CloseDuration));
    }


    private void Update()
    {
        if (StartTimer)
            timer += Time.deltaTime;
    }
}
