using Hypocrites.Defines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hypocrites.Player
{
    [RequireComponent(typeof(PlayerController))]

    public class PlayerInput : MonoBehaviour
    {

        public KeyCode forward = KeyCode.W;
        public KeyCode back = KeyCode.S;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        //public KeyCode turnLeft = KeyCode.Q;
        //public KeyCode turnRight = KeyCode.E;
        public KeyCode shift = KeyCode.LeftShift;

        PlayerController controller;
        DialogueManager dialogueManager;

        // Start is called before the first frame update
        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            dialogueManager = DialogueManager.Instance;
        }

        private void OnForward() 
        {
            controller.Move(Directions.UP); 
        }
        private void OnBackward() 
        {
            if (Input.GetKey(shift)) controller.Move(Directions.DOWN);
            else controller.Rotate(Directions.DOWN);
        }
        private void OnLeft() 
        { 
            if (Input.GetKey(shift)) controller.Move(Directions.LEFT);
            else controller.Rotate(Directions.LEFT);
        }
        private void OnRight()
        {
            if (Input.GetKey(shift)) controller.Move(Directions.RIGHT);
            else controller.Rotate(Directions.RIGHT);
        }

        // Update is called once per frame
        private void Update()
        {
            if (dialogueManager.IsTyping)
                return;
            
            
            if (Input.GetKey(forward) && Input.GetKey(shift))
            {
                if (Input.GetKey(right)) controller.Move(Directions.RIGHT | Directions.UP);
                else if (Input.GetKey(left)) controller.Move(Directions.LEFT | Directions.UP);
                else controller.Move(Directions.UP);
            }
        }
    }
}
