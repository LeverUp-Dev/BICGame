using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public bool smoothTransition = false;
    public float transitionSpeed = 10f;
    public float transitionRotationSpeed = 500f;
    public bool IsBorder;

    Vector3 targetGridPos;
    Vector3 prevTargetGridPos;
    Vector3 targetRotation;

    private void Start()
    {
        targetGridPos = Vector3Int.RoundToInt(transform.position);
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        if(true)
        {
            prevTargetGridPos = targetGridPos;

            Vector3 targetPosition = targetGridPos;

            if (targetRotation.y > 270f) targetRotation.y = 0f;
            if (targetRotation.y < 0f) targetRotation.y = 270f;

            if(!smoothTransition)
            {
                transform.position = targetPosition;
                transform.rotation = Quaternion.Euler(targetRotation);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * transitionRotationSpeed);
            }
        }
        else
        {
            targetGridPos = prevTargetGridPos;
        }
    }

    public void RotateLeft() { if (AtRest) targetRotation -= Vector3.up * 90f; }
    public void RotateRight() { if (AtRest) targetRotation += Vector3.up * 90f; }
    public void RotateBack() { if (AtRest) targetRotation += Vector3.up * 180f; }
    public void MoveForward() { if (AtRest) targetGridPos += transform.forward*2;  }
    public void MoveBackward() { if (AtRest) targetGridPos -= transform.forward*2; }
    public void MoveLeft() { if (AtRest) targetGridPos -= transform.right*2; }
    public void MoveRight() { if (AtRest) targetGridPos += transform.right*2; }
    public void MoveDiagonalRightForward() {if (AtRest) targetGridPos += (transform.forward + transform.right)*2; }
    public void MoveDiagonalLeftForward() { if (AtRest) targetGridPos += (transform.forward - transform.right)*2; }

    bool AtRest
    {
        get
        {
            if((Vector3.Distance(transform.position, targetGridPos) <0.05f)&& 
                Vector3.Distance(transform.eulerAngles, targetRotation) < 0.05f)
                return true;
            else
                return false;
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
