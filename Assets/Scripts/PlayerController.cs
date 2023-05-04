using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed;

    [SerializeField]
    private float lookSenesitivity;

    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0;

    [SerializeField]
    private Camera theCamera;
    private Rigidbody myRigid;


    // Start is called before the first frame update
    void Start()
    {
        myRigid = GetComponent<Rigidbody>();
    }

    public bool testFlag = true;
    // Update is called once per frame
    void Update()
    {
        Move();
        CameraAngle();
        //CameraRotation();
        //CharacterRotation();

        /*
        if (Input.GetKeyDown(KeyCode.UpArrow) && move == false)
        {
            if (testFlag) StartCoroutine(moveBlockTranslate(Vector3.left));
            else StartCoroutine(moveBlockTime(Vector3.left));
        }*/

    }

    //이동
    private void Move()
    {
        //float _moveDirx = Input.GetAxisRaw("Horizontal");
        float _moveDirz = Input.GetAxisRaw("Vertical");

        //Vector3 _moveHorizontal = transform.right * _moveDirx;
        Vector3 _moveVertical = transform.forward * _moveDirz;

        Vector3 _velocity = (/*_moveHorizontal +*/ _moveVertical).normalized * walkSpeed;
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    //마우스로 회전
    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation * lookSenesitivity;
        currentCameraRotationX += _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    //카메라 직각 회전
    bool rotating;
    Vector3 CLOCKWISE = Vector3.up;
    Vector3 ANTI_CLOCKWISE = Vector3.down;

    float rotateTime = 0.1f;

    float Round90(float f)
    {
        float r = f % 90;
        return (r < 45) ? f - r : f - r + 90;
    }
    private void CameraAngle()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            StartCoroutine(MoveBlockTime(CLOCKWISE));
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            StartCoroutine(MoveBlockTime(ANTI_CLOCKWISE));
    }


    private IEnumerator MoveBlockTime(Vector3 wise)
    {
        rotating = true;

        this.transform.Rotate(new Vector3(0, wise.y, 0));

        float elapsedTime = 0.0f;

        Quaternion currentRotation = this.transform.rotation;
        Vector3 targetEulerAngles = this.transform.rotation.eulerAngles;
        targetEulerAngles.y += (88.0f) * wise.y;

        Quaternion targetRotation = Quaternion.Euler(targetEulerAngles);

        while (elapsedTime < rotateTime)
        {
            transform.rotation = Quaternion.Euler(Vector3.Lerp(currentRotation.eulerAngles, targetRotation.eulerAngles, elapsedTime / rotateTime));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetEulerAngles.y = Round90(targetEulerAngles.y);
        this.transform.rotation = Quaternion.Euler(targetEulerAngles);

        rotating = false;
    }


    //한 칸씩 이동하는 코드 
    public bool move = false;
    private float blockMoveTime;
    private float blockMoveSpeed;
    
    private IEnumerator moveBlockTime(Vector3 dir)
    {
        move = true;

        float elapsedTime = 0.0f;

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = currentPosition + dir;

        while (elapsedTime < blockMoveTime)
        {
            transform.position = Vector3.Lerp(currentPosition, targetPosition, elapsedTime / blockMoveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        move = false;
    }

    private IEnumerator moveBlockTranslate(Vector3 dir)
    {
        move = true;

        Vector3 targetPosition = transform.position + dir;

        while (Vector3.Magnitude(targetPosition - transform.position) >= 0.01f)
        {
            transform.Translate(dir * Time.deltaTime * blockMoveSpeed);
            yield return null;
        }

        transform.position = targetPosition;

        move = false;
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
