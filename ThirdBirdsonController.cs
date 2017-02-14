using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(ThirdBirdsonMotor))]
[RequireComponent(typeof(ThirdBirdsonAnimator))]
public class ThirdBirdsonController : MonoBehaviour
{
    // Drag & Drop
    [SerializeField]
    private GameObject ragdoll;

    // Components
    private CharacterController characterController;
    private ThirdBirdsonMotor motor;
    private ThirdBirdsonAnimator animator;

    // Input
    private float deadZone = 0.1f;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        motor = GetComponent<ThirdBirdsonMotor>();
        animator = GetComponent<ThirdBirdsonAnimator>();
    }
	
	private void Update()
    {
        motor.SetVerticalVelocity(motor.GetMoveVector().y);
        motor.SetMoveVector(Vector3.zero);
        motor.SetTurnSpeed(0.0f);

        GetMotorInput();
        GetActionInput();

        motor.UpdateMotor();
    }

    private void GetMotorInput()
    {
        float vertInput = Input.GetAxis("Vertical");
        if(!animator.GetIsFlying())
        {
            if(vertInput > deadZone || vertInput < -deadZone)
            {
                motor.AddMoveVector(new Vector3(0, 0, vertInput));
            }
        }
        else
        {
           motor.AddMoveVector(new Vector3(0, 0, 1.0f));
        }

        float horizInput = Input.GetAxis("Horizontal");
        if(horizInput > deadZone || horizInput < -deadZone)
        {
            motor.SetTurnSpeed(horizInput);
        }

        animator.DetermineCurrentDirection();
    }

    private void GetActionInput()
    {
        if(Input.GetButtonDown("Jump"))
        {
            Launch();
        }

        if(Input.GetButton("Jump"))
        {
            Flap();
        }
        
        //if(Input.GetButtonDown("Fire1"))
        if(Input.GetKeyDown(KeyCode.E))
        {
            // If mouse is hovering over enemy -> attack. 
            // If mouse is hovering over item -> grab. 
            // If mouse is hovering over self and hold item -> use. 
            // etc.
            Attack();
        }

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            Die();
        }
    }

    private void Launch()
    {
        motor.Launch();
    }

    private void Flap()
    {
        motor.Flap();
    }

    private void Die()
    {
        GameObject deadPigeon = GameObject.Instantiate(ragdoll, transform.position, transform.rotation);
        Camera.main.GetComponent<ThirdPersonCamera>().SetTargetLookAt(deadPigeon.transform.FindChild("Pigeon"));
        
        Rigidbody[] rigidBodies = deadPigeon.GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rb in rigidBodies)
        {
            rb.AddForce(transform.TransformVector(motor.GetMoveVector()) / 4, ForceMode.Impulse);
            
        }

        GameObject.Destroy(this.gameObject);
    }

    private void Attack()
    {
        if(characterController.isGrounded)
        {
            animator.Attack();
        }
    }


    #region Setters & Getters

    public CharacterController GetCharacterController()
    {
        return characterController;
    }

    #endregion
}