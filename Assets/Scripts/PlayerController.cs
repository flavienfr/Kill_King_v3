//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Threading;
using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public Rigidbody2D rb2d;
    public Animator animator;

    private Vector2 moveVelocity;

    /*void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }*/

    void Update()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(moveHorizontal, moveVertical);
        moveVelocity = moveInput.normalized * speed;

        //Animation
        if (moveVelocity != Vector2.zero)
        {
            animator.SetFloat("Horizontal", moveVelocity.x);
            animator.SetFloat("Vertical", moveVelocity.y);
        }
        animator.SetFloat("Speed", moveVelocity.sqrMagnitude);
    }

    void FixedUpdate()
    {
        rb2d.MovePosition(rb2d.position + moveVelocity * Time.fixedDeltaTime);
    }
}
