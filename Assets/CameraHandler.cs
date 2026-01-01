using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[Serializable]
public class CameraEvent
{
    public CinemachineCamera Camera;
    public CinemachineOrbitalFollow c2;
    public Transform c3;
    public InputValue pan;
    public InputValue tilt;
    public UnityEvent OnEnableaction;
    public UnityEvent OnDisableaction;
    public void Initialize()
    {
        //pan.value = Mathf.Clamp(pan.value, pan.Range.x, pan.Range.y);

        tilt.Range.x = Mathf.Clamp(tilt.Range.x, -90, 90);
        tilt.Range.y = Mathf.Clamp(tilt.Range.y, -90, 90);
        //tilt.value= Mathf.Clamp(tilt.value, tilt.Range.x, tilt.Range.y);   
    }
}

[Serializable]
public class InputValue
{
    [SerializeField] private float value;
    public Vector2 Range;
    public void SetValue(float v)
    {
        value = Mathf.Clamp(v, Range.x, Range.y);
    }
    public float GetValue() => Mathf.Clamp(value, Range.x, Range.y);
}
public class CameraHandler : MonoBehaviour
{
    public CharacterMover character;
    private string runButton = "Camera";
    //public CinemachineCamera[] allCameras;
    public List<CameraEvent> Cameras;
    public int currentCameraIndex = 0;

    public CinemachineCamera CCcamera;

    [Header("Sensitivity")]
    public Vector2 mouseSensitivity;
    public float touchSensitivity = 0.1f;
    public float smoothTime = 0.05f;

    public Vector2 targetDelta;
    private bool android;

    private void Start()
    {
        //Cursor.visible = false;
        CCcamera = Cameras[0].Camera;
        Cameras[0].Camera.Prioritize();
        foreach (var cam in Cameras)
        {
            cam.c2 = cam.Camera.gameObject.GetComponent<CinemachineOrbitalFollow>();
        }
    }
    void Update()
    {
        if (!character.enabled)
            return;
        if (SimpleInput.GetButtonDown(runButton) || Input.GetKeyDown(KeyCode.C))
        {
            Cameras[currentCameraIndex].OnDisableaction?.Invoke();
            //currentCameraIndex++;
            currentCameraIndex = (currentCameraIndex+1 >= Cameras.Count) ? 0 : currentCameraIndex+1;

            //if (currentCameraIndex >= Cameras.Count) { currentCameraIndex = 0; }
            ChangeCamera(currentCameraIndex);

        }

        targetDelta = GetInput();

        if (Cameras[currentCameraIndex].c2 != null)
        {
            targetDelta.y = Mathf.Clamp(targetDelta.y, Cameras[currentCameraIndex].c2.VerticalAxis.Range.x,
             Cameras[currentCameraIndex].c2.VerticalAxis.Range.y);

            Cameras[currentCameraIndex].c2.HorizontalAxis.Value = targetDelta.x;
            Cameras[currentCameraIndex].c2.VerticalAxis.Value = targetDelta.y;
        }
        else if (Cameras[currentCameraIndex].c3 != null)
        {
            var cc = Cameras[currentCameraIndex];
            cc.pan.SetValue(cc.pan.GetValue() + targetDelta.x);

            float f = -1 * Input.GetAxis("Mouse Y") * mouseSensitivity.y;
            cc.tilt.SetValue(cc.tilt.GetValue() + f);
            var rot = Quaternion.Euler(cc.tilt.GetValue(), cc.pan.GetValue(), 0);
            cc.c3.rotation = rot;
        }
    }

    public void ChangeCamera(int index)
    {
        //for (int i = 0; i < Cameras.Count; i++)
        //    if (i == index)
        //    {
        Cameras[index].Camera.Prioritize();
        Cameras[index].OnEnableaction?.Invoke();
        CCcamera = Cameras[index].Camera;
        //}
    }

    Vector2 Delta;
    private Vector2 GetInput()
    {
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount > 0)
            {
                UnityEngine.Touch touch = Input.GetTouch(0);
                if (touch.phase == UnityEngine.TouchPhase.Moved && !EventSystem.current.IsPointerOverGameObject())
                {
                    Delta.x = touch.deltaPosition.x * touchSensitivity;
                    Delta.y -= touch.deltaPosition.y * touchSensitivity;
                }
            }
            else
            {
                Delta.x = 0;
            }
        }
        else
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Delta.x = Input.GetAxis("Mouse X") * mouseSensitivity.x;
                Delta.y -= Input.GetAxis("Mouse Y") * mouseSensitivity.y;
            }
            else
                Delta.x = 0;
        }
        return Delta;
    }
}
