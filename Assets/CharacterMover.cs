using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows;
using Input = UnityEngine.Input;


public enum CharacterAnimationState { IDLE, WALK, RUN, JUMP }

public class CharacterMover : MonoBehaviour
{
    public Camera Camera;
    public CharacterController controller;
    public Vector3 moveDirection;
    public float rotation;
    public float currentSpeed = 1;
    //public float rotationSpeed1 = 1;
    public float walkSpeed = 1;
    public float runSpeed = 1;
    public float inputHorizontal;
    public float inputVertical;
    private string horizontalAxis = "Horizontal2";
    private string verticalAxis = "Vertical2";
    //private string horizontalAxisMouse = "Mouse X";
    //private string verticalAxisMouse = "Mouse Y";
    private string jumpButton = "Jump2";
    private string runButton = "Run";
    private string Action = "Action";

    [SerializeField] private Animator animator;
    public CharacterAnimationState animstate;
    [SerializeField] private float _camerRotationSpeed = 3;


    [Header("Movement Settings")]
    public float playerSpeed = 5.0f;
    public float jumpHeight = 1.5f;
    public float gravityValue = -9.81f;

    [Header("Ground Check Settings")]
    public float groundRayDistance = 0.3f;
    public LayerMask groundLayer;
    public Vector3 playerVelocity;
    public bool groundedPlayer;



    public void SetAnimstate(CharacterAnimationState state)
    {
        if (state == animstate) return;
        animstate = state;
        animator.SetBool("isWalking", state == CharacterAnimationState.WALK);
        animator.SetBool("isRuning", state == CharacterAnimationState.RUN);
        Debug.Log("SetAnimstate");
    }



    //float _targetRotation;
    //float RotationSmoothTime = 0.12f;
    //public float _speed = 1;
    //float _rotationVelocity=1;
    void Update()
    {
        //{
        //    Vector3 inputDirection = new Vector3(inputHorizontal, 0.0f, inputVertical).normalized;

        //    // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        //    // if there is a move input rotate player when the player is moving
        //   // if (_input.move != Vector2.zero)
        //    {
        //        _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
        //                          Camera.transform.eulerAngles.y;
        //        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, 
        //            ref _rotationVelocity,
        //            RotationSmoothTime);

        //        // rotate to face input direction relative to camera position
        //        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        //    }


        //    Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        //    // move the player
        //    controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
        //                     new Vector3(0.0f, 0, 0.0f) * Time.deltaTime);
        //}

        HandleInput();
        HandleMovement();
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        if (moveDirection.magnitude != 0)
        {
            //if (currentSpeed == runSpeed)
            if ((SimpleInput.GetButton(runButton) || Input.GetKey(KeyCode.LeftShift)))
                SetAnimstate(CharacterAnimationState.RUN);
            else
            if (currentSpeed != 0)
                SetAnimstate(CharacterAnimationState.WALK);
        }
        else SetAnimstate(CharacterAnimationState.IDLE);


    }

    private void HandleMovement()
    {
        //transform.eulerAngles += new Vector3(0, rotation * rotationSpeed, 0);
        //transform.Rotate(Camera.gameObject.transform.forward,1);


        //if ((moveDirection.magnitude != 0))
        {

            //var cameraface = Camera.gameObject.transform.right * inputHorizontal
            //   ;// + Camera.gameObject.transform.forward* ((inputVertical!=0)? 1 : 0);
            //cameraface.y = 0;
            //cameraface.Normalize();
            //Quaternion targetRotation = Quaternion.LookRotation(cameraface);
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _camerRotationSpeed * Time.deltaTime);


            if (inputHorizontal != 0)
            {
                var cameraface = Camera.gameObject.transform.right * inputHorizontal;
                cameraface.y = 0;
                cameraface.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(cameraface);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    targetRotation, _camerRotationSpeed * Time.deltaTime);

            }
            if (inputVertical != 0)
            {
                var cameraface = Camera.gameObject.transform.forward * inputVertical;
                cameraface.y = 0;
                cameraface.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(cameraface);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    targetRotation, _camerRotationSpeed * Time.deltaTime);
            }
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        }


        //stick to ground
        groundedPlayer = Physics.Raycast(transform.position, Vector3.down, groundRayDistance, groundLayer);
        //Debug.DrawRay(transform.position, Vector3.down * groundRayDistance, Color.red);

        if (groundedPlayer && playerVelocity.y < 0)
            playerVelocity.y = -2f;


        if ((SimpleInput.GetButtonDown(jumpButton) || Input.GetKeyDown(KeyCode.Space)) && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);


    }


    public float SpeedChangeRate = 5;
    private void HandleInput()
    {
        inputHorizontal = GetSignWithZero(SimpleInput.GetAxis(horizontalAxis)) +
            BoolToFloat(Input.GetKey(KeyCode.D)) - BoolToFloat(Input.GetKey(KeyCode.A));
        inputVertical = GetSignWithZero(SimpleInput.GetAxis(verticalAxis)) +
             BoolToFloat(Input.GetKey(KeyCode.W)) - BoolToFloat(Input.GetKey(KeyCode.S));

        moveDirection = Camera.gameObject.transform.forward * inputVertical
        + Camera.gameObject.transform.right * inputHorizontal;
        moveDirection.y = 0;
        //rotation = inputHorizontal;
        float targetSpeed = 0;
        if (moveDirection.magnitude != 0)
            targetSpeed = (SimpleInput.GetButton(runButton) || Input.GetKey(KeyCode.LeftShift))
            ? runSpeed : walkSpeed;



        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed,
                   Time.deltaTime * SpeedChangeRate);
        currentSpeed = Mathf.Round(currentSpeed * 1000f) / 1000f;


        if (SimpleInput.GetButtonDown(jumpButton) || Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger("Jump");

        if (SimpleInput.GetButtonDown(Action) || Input.GetKeyDown(KeyCode.KeypadEnter))
            animator.SetTrigger("Wave");
    }

    public float BoolToFloat(bool value)
    { return value ? 1 : 0; }

    public float GetSignWithZero(float v)
    {
        v = Mathf.Round(v * 10f) / 10f;
        if (Mathf.Abs(v) < 0.4f)
            return 0;
        return v;
        if (v == 0) return 0;
        return Mathf.Sign(v);
    }
}
