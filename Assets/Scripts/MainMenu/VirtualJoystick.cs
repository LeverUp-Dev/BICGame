using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private RectTransform lever;
    private RectTransform rectTransform;

    [SerializeField, Range(10, 150)]
    float leverRange;

    private Vector2 inputDirection;
    private bool isInput;

    [SerializeField]
    private TPSCharacterController controller;

    public float sensitivity = 1.0f;

    public enum JoystickType { Move, Rotate }
    public JoystickType joystickType;

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    //오브젝트를 클릭해서 드래그하는 도중에 호출되는 함수
    //하지만 드래그 도중 마우스를 멈추면 값이 호출 되지 않음
    public void OnBeginDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
        isInput = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
    }

    
    public void OnEndDrag(PointerEventData eventData)
    {
        lever.anchoredPosition = Vector2.zero;
        isInput = false;
        switch(joystickType)
        {
            case JoystickType.Move:
                controller.Move(Vector2.zero);
                break; 

                case JoystickType .Rotate:
                break;

        }
    }

    private void InputControlVector()
    {
        //캐릭터에게 입력벡터를 전달
        switch(joystickType)
        {
            case JoystickType.Move:
                controller.Move(inputDirection * sensitivity);
                break;

                case JoystickType.Rotate:
                controller.LookAround(inputDirection * sensitivity);
                break;
        }
        //Debug.Log(inputDirection.x + "/" + inputDirection.y);
    }

    private void ControlJoystickLever(PointerEventData eventData)
    {
        var inputPos = eventData.position - rectTransform.anchoredPosition;
        var inputVector = inputPos.magnitude < leverRange ? inputPos :
            inputPos.normalized * leverRange;
        lever.anchoredPosition = inputVector;
        inputDirection = inputVector / leverRange;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isInput)
        {
            InputControlVector();
        }
    }
}
