using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdBirdsonAnimator : MonoBehaviour
{
    public enum Direction { Stationary, Forward, Left, Right, ForwardLeft, ForwardRight }
    public enum State { Idle, WalkingForward, WalkingLeft, WalkingRight, Sliding, Launching, Flapping, Gliding, Landing, Dead, ActionLocked };

    private Animation birdimation;
    private ThirdBirdsonController controller;
    private ThirdBirdsonMotor motor;

    private Direction currentDirection;
    private State currentState;
    private State previousState;
    private bool isFlying = false;

    private void Awake()
    {
        birdimation = GetComponent<Animation>();
        controller = GetComponent<ThirdBirdsonController>();
        motor = GetComponent<ThirdBirdsonMotor>();
    }
	
	private void Update()
    {
        DetermineCurrentState();
        ProcessCurrentState();

        //Debug.Log("Current State: " + currentState + "      ---      Flying: " + isFlying);
	}

    private void DetermineCurrentState()
    {
        if(currentState == State.Dead || currentState == State.ActionLocked)
        {
            return;
        }

        // Idle, WalkingForward, WalkingLeft, WalkingRight, Landing, Sliding
        if(controller.GetCharacterController().isGrounded)
        {
            if(currentState != State.Landing)
            {
                switch(currentDirection)
                {
                    case Direction.Stationary:
                        currentState = State.Idle;
                        break;
                    case Direction.Forward:
                        currentState = State.WalkingForward;
                        break;
                    case Direction.Left:
                        currentState = State.WalkingLeft;
                        break;
                    case Direction.Right:
                        currentState = State.WalkingRight;
                        break;
                    case Direction.ForwardLeft:
                        currentState = State.WalkingLeft;
                        break;
                    case Direction.ForwardRight:
                        currentState = State.WalkingRight;
                        break;
                    default:
                        break;
                }
            }

            if(motor.GetMoveVector().y < -1)
            {
                currentState = State.Sliding;
            }

            if(isFlying)
            {
                currentState = State.Landing;
            }
        } // Flapping, Gliding
        else if(!controller.GetCharacterController().isGrounded)
        {
            if(currentState != State.Launching)
            {
                if(Input.GetButton("Jump"))
                {
                    currentState = State.Flapping;
                }
                else
                {
                    currentState = State.Gliding;
                }
            }

            /*
            if(!isFlying)
            {
                currentState = State.Flapping;
            }
            */
        }

        if(currentState != previousState)
        {
            EnterState(currentState);
            previousState = currentState;
        }
    }

    private void ProcessCurrentState()
    {
        switch(currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.WalkingForward:
                WalkingForward();
                break;
            case State.WalkingLeft:
                WalkingLeft();
                break;
            case State.WalkingRight:
                WalkingRight();
                break;
            case State.Sliding:
                Sliding();
                break;
            case State.Launching:
                Launching();
                break;
            case State.Flapping:
                Flapping();
                break;
            case State.Gliding:
                Gliding();
                break;
            case State.Landing:
                Landing();
                break;
            case State.Dead:
                break;
            case State.ActionLocked:
                break;
            default:
                break;
        }
    }

    public void DetermineCurrentDirection()
    {
        bool forward = false;
        bool left = false;
        bool right = false;

        if(motor.GetMoveVector().z > 0)
        {
            forward = true;
        }

        if(motor.GetTurnSpeed() > 0)
        {
            right = true;
        }

        if(motor.GetTurnSpeed() < 0)
        {
            left = true;
        }

        if(forward)
        {
            if(left)
            {
                currentDirection = Direction.ForwardLeft;
            }
            else if(right)
            {
                currentDirection = Direction.ForwardRight;
            }
            else
            {
                currentDirection = Direction.Forward;
            }
        }
        else if(left)
        {
            currentDirection = Direction.Left;
        }
        else if(right)
        {
            currentDirection = Direction.Right;
        }
        else
        {
            currentDirection = Direction.Stationary;
        }
    }

    public void Launch()
    {
        EnterLaunching();
    }

    public void Attack()
    {
        StartCoroutine(ActionLockTimer(birdimation["LandAttack"].length));
        birdimation.CrossFade("LandAttack");
    }

    private IEnumerator ActionLockTimer(float delay)
    {
        currentState = State.ActionLocked;

        yield return new WaitForSeconds(delay);

        currentState = State.Idle;

        yield break;
    }


    #region Character State Entry Methods

    private void EnterState(State newState)
    {
        switch(newState)
        {
            case State.Launching:
                EnterLaunching();
                break;
            case State.Landing:
                EnterLanding();
                break;
            case State.Dead:
                break;
            default:
                break;
        }
    }

    private void EnterLaunching()
    {
        currentState = State.Launching;
        isFlying = true;
        StartCoroutine(LaunchTimer(birdimation["Launch"].length * 0.5f));
    }

    private IEnumerator LaunchTimer(float delay)
    {
        yield return new WaitForSeconds(delay);

        if(!controller.GetCharacterController().isGrounded)
        {
            currentState = State.Flapping;
        }
        
        yield break;
    }

    private void EnterLanding()
    {
        isFlying = false;
        StartCoroutine(LandTimer(birdimation["Land"].length * 0.9f));
    }

    private IEnumerator LandTimer(float delay)
    {
        yield return new WaitForSeconds(delay);

        if(controller.GetCharacterController().isGrounded)
        {
            currentState = State.Idle;
        }

        yield break;
    }

    private void EnterDead()
    {

    }

    #endregion


    #region Character State Action Methods

    private void Idle()
    {
        birdimation.CrossFade("Idle");
    }

    private void WalkingForward()
    {
        birdimation.CrossFade("WalkForward");
    }

    private void WalkingLeft()
    {
        birdimation.CrossFade("WalkForward");
    }

    private void WalkingRight()
    {
        birdimation.CrossFade("WalkForward");
    }

    private void Sliding()
    {
        birdimation.CrossFade("Slide");
    }

    private void Launching()
    {
        birdimation["Launch"].speed = 1.5f;
        birdimation.CrossFade("Launch");
    }

    private void Flapping()
    {
        if(!isFlying)
        {
            birdimation["Flap_01"].speed = 2.0f;
            birdimation.CrossFade("Flap_01");
        }
        else
        {
            switch(currentDirection)
            {
                case Direction.Forward:
                    birdimation.CrossFade("Flap_02");
                    break;
                case Direction.ForwardLeft:
                    birdimation.CrossFade("TurnLFlap");
                    break;
                case Direction.ForwardRight:
                    birdimation.CrossFade("TurnRFlap");
                    break;
                default:
                    birdimation.CrossFade("Flap_01");
                    break;
            }
        }
    }

    private void Gliding()
    {
        switch(currentDirection)
        {
            case Direction.Forward:
                birdimation.CrossFade("Glide");
                break;
            case Direction.ForwardLeft:
                birdimation.CrossFade("TurnLGlide");
                break;
            case Direction.ForwardRight:
                birdimation.CrossFade("TurnRGlide");
                break;
            default:
                birdimation.CrossFade("Glide");
                break;
        }
    }

    private void Landing()
    {
        birdimation.CrossFade("Land");
    }

    #endregion


    #region Setters & Getters

    public Direction GetCurrentDirection()
    {
        return currentDirection;
    }

    public State GetCurrentState()
    {
        return currentState;
    }

    public void SetIsFlying(bool flying)
    {
        isFlying = flying;
    }

    public bool GetIsFlying()
    {
        return isFlying;
    }

    #endregion
}