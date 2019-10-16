using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    
    //Rigidbody rb;

    public float velocity = 9;

    [Space]
    public float InputX;
    public float InputZ;
    public Vector3 desiredMoveDirection;
    public bool blockRotationPlayer;
    public float desiredRotaionSpeed = 0.1f;
    public Animator anim;
    public float Speed;
    public float allowPlayerRotation = 0.1f;
    public Camera cam;
    public CharacterController controller;
    public bool isGround;

    [Header("AnimationSmoothing")]
    [Range(0, 1f)]
    public float HorizontalAniSmoothTime = 0.2f; 
    [Range(0, 1f)]
    public float VerticalAniSmoothTime = 0.2f;
    [Range(0, 1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;


    private float verticalVel;
    private Vector3 moveVector;
    public bool canMove;



    // Initialize
    void Awake()
    {
        
        //rb = GetComponent<Rigidbody>();
        //anim = AnimationController.Instance.getAnim();
        cam = Camera.main;
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return;

        InputMagnitude();

        isGround = controller.isGrounded;
        if (isGround)
        {
            verticalVel = 0;
        }
        else
        {
            verticalVel -= 0.05f * Time.deltaTime;
        }
        moveVector = new Vector3(0, verticalVel, 0);
        controller.Move(moveVector);

        //Updater

    }

    void PlayerMoveAndRotation()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        var camera = Camera.main;
        var forward = cam.transform.forward;
        var right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = forward * InputZ + right * InputX;

        if(blockRotationPlayer == false)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotaionSpeed);
            controller.Move(desiredMoveDirection * Time.deltaTime * velocity);
        }
        
    }

    public void RotateToCamera(Transform t)
    {
        var camera = Camera.main;
        var forward = cam.transform.forward;
        var right = cam.transform.right;

        desiredMoveDirection = forward;

        t.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotaionSpeed);
    }

    public void RotateTowards(Transform t)
    {
        transform.rotation = Quaternion.LookRotation(t.position - transform.position);
    }

    void InputMagnitude()
    {
        //Caculate Input Vectors
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        //anim.SetFloat("InputZ", InputZ, VerticalAniSmoothTime, Time.deltaTime * 2f);
        //anim.SetFloat("InputX", InputX, HorizontalAniSmoothTime, Time.deltaTime * 2f);

        //Caculate the Input Magnitude
        Speed = new Vector2(InputX, InputZ).sqrMagnitude;

        //Physically move player
        if (Speed > allowPlayerRotation)
        {
            //anim.SetFloat("InputMagnitude", Speed, StartAnimTime, Time.deltaTime);
            PlayerMoveAndRotation();
        }
        else if (Speed < allowPlayerRotation)
        {
            //anim.SetFloat("InputMagnitude", Speed, StopAnimTime, Time.deltaTime);

        }
        anim.SetFloat("Blend",Speed);

    }

}
