using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]

public class PlayerInput : MonoBehaviour
{
    public KeyCode forward = KeyCode.W;
    public KeyCode back = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode turnLeft = KeyCode.Q;
    public KeyCode turnRight = KeyCode.E;
    public KeyCode shift = KeyCode.LeftShift;

    PlayerController controller;

    // Start is called before the first frame update
    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKey(forward) && Input.GetKey(shift))
        {
            if (Input.GetKey(right)) controller.MoveDiagonalRightForward();
            else if (Input.GetKey(left)) controller.MoveDiagonalLeftForward();
            else controller.MoveForward();
        }
        if (Input.GetKeyUp(back))
        {
            if (Input.GetKey(shift)) controller.MoveBackward();
            else controller.RotateBack();
        }
        if (Input.GetKeyUp(forward) && !Input.GetKey(shift)) controller.MoveForward();
        if (Input.GetKeyUp(left) && Input.GetKey(shift)) controller.MoveLeft();
        if (Input.GetKeyUp(right) && Input.GetKey(shift)) controller.MoveRight();
        if (Input.GetKeyUp(left)) controller.RotateLeft();
        if (Input.GetKeyUp(right)) controller.RotateRight();




    }
}
