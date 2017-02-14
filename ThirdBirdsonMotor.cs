using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdBirdsonMotor : MonoBehaviour
{
    // Game Object Components
    private ThirdBirdsonController controller;
    private ThirdBirdsonAnimator animator;

    // Movement & Turning
    private Vector3 moveVector = Vector3.zero;
    private float turnSpeed = 1.0f;
    private float groundSpeed = 0.5f;
    private float flightSpeed = 5.0f;
    private float maxFlightSpeed = 5.0f;
    private float minFlightSpeed = 0.75f;

    // Gravity
    private float gravityStrength = 5.0f;
    private float terminalVelocity = 10.0f;
    private float verticalVelocity = 0.0f;

    private float launchStrength = 0.5f;
    private float flapStrength = 0.25f;

    // Sliding
    private Vector3 slideDirection = Vector3.zero;
    private float slideSpeed = 3.0f;
    private float slideLimit = 0.7f;
    private float maxControllableSlideMagnitude = 0.55f;

    private void Awake()
    {
        controller = GetComponent<ThirdBirdsonController>();
        animator = GetComponent<ThirdBirdsonAnimator>();

        StartCoroutine(AccelerateFlightSpeed());
    }

    private IEnumerator AccelerateFlightSpeed()
    {
        while(controller != null)
        {
            if(animator.GetIsFlying())
            {
                flightSpeed = flightSpeed + (Time.deltaTime);
                flightSpeed = Mathf.Clamp(flightSpeed, minFlightSpeed, maxFlightSpeed);
            }
            else
            {
                flightSpeed = minFlightSpeed;
            }

            yield return 0;
        }

        yield break;
    }

    private void ApplySlide()
    {
        if(controller.GetCharacterController().isGrounded)
        {
            slideDirection = Vector3.zero;

            RaycastHit hit;
            if(Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.down, out hit))
            {
                if(hit.normal.y < slideLimit)
                {
                    slideDirection = new Vector3(hit.normal.x, -hit.normal.y, hit.normal.z);
                }
            }

            if(slideDirection.magnitude < maxControllableSlideMagnitude)
            {
                moveVector += slideDirection;
            }
            else
            {
                moveVector = slideDirection;
            }
        }
    }

    private float DetermineSpeed()
    {
        float speed = 0.0f;

        switch(animator.GetCurrentDirection())
        {
            case ThirdBirdsonAnimator.Direction.Stationary:
                speed = 0.0f;
                break;
            case ThirdBirdsonAnimator.Direction.Forward:
                speed = groundSpeed;
                break;
            case ThirdBirdsonAnimator.Direction.ForwardLeft:
                speed = groundSpeed * 0.75f;
                break;
            case ThirdBirdsonAnimator.Direction.ForwardRight:
                speed = groundSpeed * 0.75f;
                break;
            default:
                speed = 0.0f;
                break;
        }

        if(slideDirection.magnitude > 0)
        {
            speed = slideSpeed;
        }

        if(animator.GetIsFlying())
        {
            speed = flightSpeed;
        }

        return speed;
    }

    private void ApplyGravity()
    {
        if(animator.GetIsFlying())
        {
            moveVector = new Vector3(moveVector.x, moveVector.y - (gravityStrength / 10 * Time.deltaTime), moveVector.z);
        }
        else if(moveVector.y > -terminalVelocity)
        {
            moveVector = new Vector3(moveVector.x, moveVector.y - (gravityStrength * Time.deltaTime), moveVector.z);
        }
        
        if(controller.GetCharacterController().isGrounded && moveVector.y < -1)
        {
            moveVector = new Vector3(moveVector.x, -1, moveVector.z);
        }

        /*
        if(moveVector.y > -terminalVelocity)
        {
            if(animator.GetIsFlying())
            {
                moveVector = new Vector3(moveVector.x, moveVector.y - (gravityStrength / 10 * Time.deltaTime), moveVector.z);
            }
            else
            {
                moveVector = new Vector3(moveVector.x, moveVector.y - (gravityStrength * Time.deltaTime), moveVector.z);
            }
        }

        if(controller.GetCharacterController().isGrounded && moveVector.y < -1)
        {
            moveVector = new Vector3(moveVector.x, -1, moveVector.z);
        }
        */
    }

    private void AlignCharacterWithCamera()
    {
        if(moveVector.x != 0 || moveVector.z != 0)
        {
            transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x,
                                    Camera.main.transform.eulerAngles.y, transform.eulerAngles.z));
        }
    }

    public void UpdateMotor()
    {
        // Convert moveVector to world space. 
        moveVector = transform.TransformDirection(moveVector);

        if(moveVector.magnitude > 1)
        {
            moveVector = Vector3.Normalize(moveVector);
        }

        //ApplySlide();

        moveVector *= DetermineSpeed();
        moveVector = new Vector3(moveVector.x, verticalVelocity, moveVector.z);

        ApplyGravity();

        controller.GetCharacterController().Move(moveVector * Time.deltaTime);
        transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * turnSpeed * 100.0f);
    }

    public void AddMoveVector(Vector3 addMoveVector)
    {
        moveVector += addMoveVector;
    }

    public void Launch()
    {
        if(controller.GetCharacterController().isGrounded)
        {
            animator.Launch();
            verticalVelocity = launchStrength;
        }
    }

    public void Flap()
    {
        if(animator.GetCurrentState() == ThirdBirdsonAnimator.State.Flapping)
        {
            animator.SetIsFlying(true);
            verticalVelocity = flapStrength;
        }
    }

    
    #region Setters & Getters

    public void SetMoveVector(Vector3 newMoveVector)
    {
        moveVector = newMoveVector;
    }

    public Vector3 GetMoveVector()
    {
        return moveVector;
    }

    public void SetTurnSpeed(float newTurnSpeed)
    {
        turnSpeed = newTurnSpeed;
    }

    public float GetTurnSpeed()
    {
        return turnSpeed;
    }

    public void SetVerticalVelocity(float newVerticalVelocity)
    {
        verticalVelocity = newVerticalVelocity;
    }

    #endregion
}