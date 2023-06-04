using Hypocrites.Enumerations;
using Hypocrites.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace Hypocrites.Player
{
    public class PlayerController : MonoBehaviour
    {
        public bool smoothTransition = false;
        public float transitionSpeed = 10f;
        public float transitionRotationSpeed = 500f;
        public LayerMask layermask;
        public float transitionMegnification = 2;

        private bool isBlock;
        Vector3 targetGridPos;
        Vector3 prevTargetGridPos;
        Vector3 targetRotation;

        private float length = 0.99f;
        GameObject nearObject;

        private void Start()
        {
            targetGridPos = Vector3Int.RoundToInt(transform.position);
        }

        private void FixedUpdate()
        {
            isBlock = !Physics.Raycast(targetGridPos, -transform.right, length, layermask)
                && !Physics.Raycast(targetGridPos, transform.right, length, layermask)
                && !Physics.Raycast(targetGridPos, -transform.forward, length, layermask)
                && !Physics.Raycast(targetGridPos, transform.forward, length, layermask);

            if (Physics.Raycast(targetGridPos, -transform.right, out RaycastHit hit1, length, layermask))
            {
                Debug.Log(hit1.transform.name);
            }

            MovePlayer();

            Debug.DrawRay(targetGridPos, transform.forward * length, Color.red);
        }

        void MovePlayer()
        {
            if (targetRotation.y > 270f)
            {
                if (targetRotation.y > 360f) targetRotation.y = 90f;
                else targetRotation.y = 0f;
            }
            else if (targetRotation.y < 0f) targetRotation.y = 270f;

            if (!smoothTransition) transform.rotation = Quaternion.Euler(targetRotation);
            else transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * transitionRotationSpeed);

            if (isBlock)
            {
                Vector3 targetPosition = targetGridPos;
                prevTargetGridPos = targetGridPos;

                if (!smoothTransition) transform.position = targetPosition;
                else transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            }
            else targetGridPos = prevTargetGridPos;
        }

        bool AtRest
        {
            get
            {
                return Vector3.Distance(transform.position, targetGridPos) < 0.05f &&
                       Vector3.Distance(transform.eulerAngles, targetRotation) < 0.05f;
            }
        }

        public void Rotate(Directions dir)
        {
            Vector3 rotation = Vector3.zero;

            if (AtRest)
            {
                if (dir.Contains(Directions.RIGHT))
                    rotation += Vector3.up * 90f;

                if (dir.Contains(Directions.LEFT))
                    rotation -= Vector3.up * 90f;

                if (dir.Contains(Directions.DOWN))
                    rotation += Vector3.up * 180f;

                targetRotation += rotation;
            }
        }

        public void Move(Directions dir)
        {
            Vector3 movement = Vector3.zero;

            if (AtRest)
            {
                if (dir.Contains(Directions.UP))
                    movement += transform.forward * transitionMegnification;

                if (dir.Contains(Directions.RIGHT))
                    movement += transform.right * transitionMegnification;

                if (dir.Contains(Directions.DOWN))
                    movement -= transform.forward * transitionMegnification;

                if (dir.Contains(Directions.LEFT))
                    movement -= transform.right * transitionMegnification;

                targetGridPos += movement;
                
                EventManager.Instance.Roll(PlayerConstants.TEMP_STAT_LUCK);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (other.tag == "Fog")
                nearObject = other.gameObject;
            Destroy(nearObject);
        }
    }
}

        /*
        private void CharacterRotation()
        {
            float elapsedTime = 0.0f;
            float _yRotation = 0; //= Input.GetAxisRaw("Horizontal");
            if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                Quaternion currentRotation = this.transform.rotation;
                _yRotation += 90.0f;
                Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f);
                myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));

                while (elapsedTime < rotateTime)
                {
                    myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(Vector3.Lerp(currentRotation, _yRotation, elapsedTime / rotateTime)));

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

            }
            else if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Vector3 _characterRotationY = new Vector3(0f, -1f, 0f);
                myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
            }*/
