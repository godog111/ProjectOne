using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    private float vertical;//玩家垂直输入

    private float stairClimbSpeed = 3f;//爬楼梯速度
    private bool isLadder;//是否在梯子上
    [SerializeField] private bool isCliming = false;//是否在爬梯
    [SerializeField] private Rigidbody2D rb;//刚体组件
    [SerializeField] Animator animator;//动画控制器


    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Stairs") && (Input.GetAxisRaw("Vertical") > 0))
        {
            isLadder = true;
            isCliming = true;
            rb.velocity = Vector2.zero;
            animator.SetBool("Climbing", true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Stairs"))
        {
            isLadder = false;
            isCliming = false;
            animator.SetBool("Climbing", false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        vertical = Input.GetAxisRaw("Vertical");

        InputOnLadder();
        IsPlayerCliming();
    }

    private void InputOnLadder()
    {
        if (isLadder && Mathf.Abs(vertical) > 0)
        {
            isCliming = true;
        }
        else if (isLadder && Mathf.Abs(vertical) == 0)
        {
            isCliming = false;
        }

        if (isLadder)
        {
            rb.gravityScale = 0;
            rb.velocity = new Vector2(rb.velocity.x * 0.3f, vertical * stairClimbSpeed);
        }
        else
        {
            rb.gravityScale = 3;//玩家没在梯子上，重力恢复
        }

    }

    private void IsPlayerCliming()
    {
        if (isCliming && isLadder)
        {
            Debug.Log("停楼梯"+isLadder+isCliming);
            animator.speed = 1f;
        }
        else if (!isCliming && isLadder)
        {
            Debug.Log("爬楼梯");
            animator.speed = 0f;
        }
    }
}
