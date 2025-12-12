using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using Fusion.Addons.KCC;

/// <summary>
/// RequireComponent agrega al objeto donde estas añadiendo este script, los componentes que escribas dentro
/// </summary>

[RequireComponent(typeof(Rigidbody), typeof(GroundCheck), typeof(KCC))]
public class MovementController : NetworkBehaviour
{
    
    private Rigidbody rbPlayer;

    [SerializeField] private Animator _animator;

    [SerializeField] private KCC kcc;
   
    
    private void Start()
    {
        kcc = GetComponent<KCC>();
        rbPlayer = GetComponent<Rigidbody>();
    }
    
    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority)
        {
            if (GetInput(out NetworkInputData input))
            {
                Movement(input);
                UpdateAnimator(input);
            }
        }
        
        
        
    }

    private void UpdateAnimator(NetworkInputData input)
    {
        _animator.SetBool("IsWalking", input.move != Vector2.zero);
        _animator.SetBool("IsRunning", input.isRunning);
        _animator.SetFloat("WalkingZ", input.move.y);
        _animator.SetFloat("WalkingX", input.move.x);
    }

    #region Movimiento

    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runSpeed = 7.7f;
    [SerializeField] private float crouchSpeed = 3.9f;

    private void Movement(NetworkInputData input)
    {
        Quaternion realRotation = Quaternion.Euler(0, input.yrotation, 0);
        Vector3 worldDirection = realRotation * (new Vector3(input.move.x, 0, input.move.y));


        //rbPlayer.linearVelocity = worldDirection.normalized * (Runner.DeltaTime * Speed(input));

        kcc.SetKinematicVelocity(worldDirection.normalized * (Runner.DeltaTime * Speed(input)));
                            
    }

    private float Speed(NetworkInputData input)
    {
        return input.move.y < 0 || input.move.x != 0 ? walkSpeed :
            input.isRunning ? runSpeed : walkSpeed;
    }


    #endregion

  
}