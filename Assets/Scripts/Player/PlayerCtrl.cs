using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerCtrl : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Components")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool canMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (animator != null)
        {
            animator.SetBool("IsFishing", false);
        }
    }

    private void Update()
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            SetAnimationSpeed(0f);
            return;
        }

        // Move

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(moveX, moveY).normalized;

        SetAnimationSpeed(moveInput.sqrMagnitude);

        if (spriteRenderer != null)
        {
            if (moveInput.x > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
            else if (moveInput.x < -0.01f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = moveInput * moveSpeed;
    }

    private void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            moveInput = Vector2.zero;
            rb.velocity = Vector2.zero;
            SetAnimationSpeed(0f);
        }
    }

    //Fishig Movement

    public void SetFishingAnimation(bool value)
    {
        if (animator != null)
        {
            animator.SetBool("IsFishing", value);
        }
    }

    public void PlayFishingSuccess()
    {
        if (animator != null)
        {
            animator.SetTrigger("FishingSuccess");
        }
    }

    public void PlayFishingFail()
    {
        if (animator != null)
        {
            animator.SetTrigger("FishingFail");
        }
    }

    public bool CanMove()
    {
        return canMove;
    }
}
