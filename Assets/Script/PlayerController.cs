using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float velocity;

    private Animator animator;
    private Rigidbody rb;

    private const string IS_WALKING = "isWalking";

	// Use this for initialization
	void Start () {

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

        int x = (int)Input.GetAxisRaw("Horizontal");
        int z = (int)Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(x, 0, z);

        transform.LookAt(transform.position + moveDir);

        rb.MovePosition(transform.position + moveDir * velocity * Time.deltaTime);

        bool isWalking = Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0;
        animator.SetBool(IS_WALKING, isWalking);
	}

    
}
