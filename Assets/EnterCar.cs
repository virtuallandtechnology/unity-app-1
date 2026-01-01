//using StarterAssets;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class EnterCar : MonoBehaviour
{
    public CharacterController characterController;
    public CarController carController;
    public Animator animator;
    public Transform EnterCarInitPos;
    public Transform DriverSeat;
    public float duration;
    public float durationbeforecloseDoor;
    public DoorHandler DoorHandler;
    public bool allowToEnter;
    public bool IsinCar;
    [Space]
    public bool HasNPCDriver;
    public Animator NPCdriver;
    public float durationBeforePullOutTheDriver = 0;
    public float durationBeforeEnterShort = 0;
    public bool StartTimer;
    public float timer;
    public float distanceToEnter;
    public float MinimumdistanceToEnter = 1.1f;
    public List<CinemachineCamera> cameras;
    public GameObject EnterExitIcon;
    public float Speed;
    private void Start()
    {
        NPCdriver.speed = 0;
    }

    private void Update()
    {
        Speed = body.linearVelocity.magnitude;

        EnterExitIcon.SetActive((!IsinCar && allowToEnter) ^ (IsinCar && Speed < 0.5f));


        if (Input.GetKeyDown(KeyCode.F) && !IsinCar && allowToEnter && (Speed < 0.5f))
        {
            StartCoroutine(EnterCarRoutine());
        }

        distanceToEnter = Vector3.Distance(characterController.gameObject.transform.position,
            EnterCarInitPos.position);


        allowToEnter = Vector3.Distance(characterController.gameObject.transform.position,
            EnterCarInitPos.position) < MinimumdistanceToEnter;

        if (IsinCar)
        {
            characterController.gameObject.transform.position = DriverSeat.position;
            characterController.gameObject.transform.rotation = DriverSeat.rotation;
            if (Input.GetKeyUp(KeyCode.C))
                ChangeCamera();

            if (Input.GetKeyUp(KeyCode.F) && (Speed < 0.5f))
            {
                StartCoroutine(ExitCarRoutine());
            }
        }

        if (StartTimer)
            timer += Time.deltaTime;
    }

    private int currentcamera = 0;

    private void ChangeCamera()
    {
        currentcamera++;
        if (currentcamera >= cameras.Count)
            currentcamera = 0;
        cameras[currentcamera].Prioritize();
    }
    public Rigidbody body;
    IEnumerator EnterCarRoutine()
    {
        yield return null;
        _ = carController.HandBrakToStop();
        while (body.linearVelocity.magnitude > 1)
            yield return null;
        //Debug.Break();
        characterController.enabled = false;
        characterController.GetComponent<CharacterMover>().enabled = false;

        yield return StartCoroutine(MoveRoutine(characterController.gameObject.transform,
            characterController.gameObject.transform.position, EnterCarInitPos.position, duration));

        yield return StartCoroutine(RotateRoutine(characterController.gameObject,
            characterController.gameObject.transform
            .eulerAngles, carController.gameObject.transform.eulerAngles, duration / 2));


        //if (HasNPCDriver)
        //{
        //    //Debug.Log("start1");
        //    //animator.SetTrigger("EnterCarWithDriver");
        //    //StartTimer = true;
        //    //Debug.Log("EnterCarWithDriver");
        //    //yield return new WaitForSeconds(durationBeforePullOutTheDriver);
        //    //Debug.Log("speed = 1");
        //    //NPCdriver.speed = 1;
        //    ////Debug.Log("            yield return new WaitForSeconds(durationBeforeEnterShort);\r\n");
        //    ////yield return new WaitForSeconds(durationBeforeEnterShort);
        //    ////Debug.Log("EnterCarShort");
        //    ////animator.SetTrigger("EnterCarShort");

        //}
        //else
        //{
        animator.SetTrigger("EnterCar");
        DoorHandler.StartOpen();
        yield return StartCoroutine(MoveRoutine(characterController.gameObject.transform,
            characterController.gameObject.transform.position, DriverSeat.position, duration));
        yield return new WaitForSeconds(durationbeforecloseDoor);
        //}



        yield return new WaitForSeconds(2.0f);
        carController.enabled = true;
        IsinCar = true;
        ChangeCamera();
    }

    float exitcar_waitbeforeOpenDoor = 0.1f;
    public float BetweenDelay_OpenFromInside = 1.8f;

    public IEnumerator ExitCarRoutine()
    {
        //_ = carController.HandBrakToStop();
        //while (body.linearVelocity.magnitude > 1)
        //    yield return null;

        IsinCar = false;
        carController.enabled = false;

        yield return null;
        animator.SetTrigger("exitCar");
        //yield return new WaitForSeconds(exitcar_waitbeforeOpenDoor);
        DoorHandler.StartOpenFromInside();
        //StartTimer = true;
        //Debug.Break();

        yield return new WaitForSeconds(BetweenDelay_OpenFromInside);
        animator.SetTrigger("exitCarFinished");
        yield return StartCoroutine(MoveRoutine(characterController.gameObject.transform,
           characterController.gameObject.transform.position,
           EnterCarInitPos.position, 0.25f));

        //StartCoroutine(RotateRoutine(characterController.gameObject,
        //    characterController.gameObject.transform
        //    .eulerAngles, carController.gameObject.transform.eulerAngles, duration / 2));
        characterController.enabled = true;
        characterController.GetComponent<CharacterMover>().enabled = true;
        characterController.GetComponent<CameraHandler>().ChangeCamera(0);

    }
    IEnumerator MoveRoutine(Transform obj, Vector3 pos1, Vector3 pos2, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float percentageComplete = elapsedTime / duration;

            obj.position = Vector3.Lerp(pos1, pos2, percentageComplete);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        obj.position = pos2;
    }

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
}
