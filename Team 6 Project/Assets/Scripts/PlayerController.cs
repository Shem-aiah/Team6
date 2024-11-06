using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController pController;

    [SerializeField] int speed;
    [SerializeField] int sprint;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;

    private int jumpCount;

    bool isSprinting;
    bool isJumping;

    Vector3 moveDirection;
    Vector3 playerVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Sprint();
    }

    // Setter for jumpCount private variable
    void SetJumpCount(int jump)
    {
        jumpCount = jump;
    }

    // Getter for jumpCount private variable
    int GetJumpCount()
    { 
        return jumpCount; 
    }

    void Movement()
    {
        if(pController.isGrounded)
        {
            SetJumpCount(0);
            playerVelocity = Vector3.zero;
        }

        moveDirection = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));

        pController.Move(moveDirection * speed * Time.deltaTime);


        Jump();

        pController.Move(playerVelocity * Time.deltaTime);
        playerVelocity.y -= gravity * Time.deltaTime;
    }
    
    void Jump()
    {
        if(Input.GetButtonDown("Jump") && GetJumpCount() < jumpMax)
        {
            SetJumpCount(GetJumpCount() + 1);
            playerVelocity.y = jumpSpeed;
        }
    }

    void Sprint()
    {
        if(Input.GetButtonDown("Sprint"))
        {
            speed *= sprint;
            isSprinting = true;
        }
        else if(Input.GetButtonUp("Sprint"))
        {
            speed /= sprint;
            isSprinting = false;
        }
    }
}
