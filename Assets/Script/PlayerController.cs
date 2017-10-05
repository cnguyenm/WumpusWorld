using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float velocity;

    private Animator animator;
    private Rigidbody rb;

    private const string IS_WALKING = "isWalking";
    private const string TRIGGER_ATTACK = "triggerAttack";

	// Use this for initialization
	void Start () {

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

        // get input
        int x = (int)Input.GetAxisRaw("Horizontal");
        int z = (int)Input.GetAxisRaw("Vertical");

        // generate move direction
        Vector3 moveDir = new Vector3(x, 0, z);

        // player look at new direction
        transform.LookAt(transform.position + moveDir);

        // player move
        rb.MovePosition(transform.position + moveDir * velocity * Time.deltaTime);

        // set animator
        bool isWalking = Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0;
        animator.SetBool(IS_WALKING, isWalking);

        // if attack
        if (Input.GetKeyDown(KeyCode.J))
        {
            print("Attack");
            animator.SetTrigger(TRIGGER_ATTACK);
        }
	}

    
}
