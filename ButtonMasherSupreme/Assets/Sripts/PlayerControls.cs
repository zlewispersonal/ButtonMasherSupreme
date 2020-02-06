using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float JumpSpeed = 5.0f;
    public float Speed = 1.0f;

    private Vector2 MoveVector;
    private Rigidbody2D M_RigidBody;

    void Awake()
    {
        M_RigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ControlInput();
        Motor();
    }

    void ControlInput()
    {
        float deadZone = 0.1f;
        MoveVector = Vector2.zero;

        if (Input.GetAxis("Horizontal") > deadZone || Input.GetAxis("Horizontal") < -deadZone)
            MoveVector += new Vector2(Input.GetAxis("Horizontal"), 0);
    }

    void Motor()
    {
        if (MoveVector.magnitude > 1)
            MoveVector = Vector3.Normalize(MoveVector);

        MoveVector = MoveVector * Speed * Time.deltaTime;

        M_RigidBody.MovePosition(M_RigidBody.position + MoveVector);
    }
}
