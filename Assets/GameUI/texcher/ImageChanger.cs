using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
    private Image image;

    [SerializeField]
    private Sprite[] sprites;

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            image.sprite = sprites[index];
            index++;
            if(sprites.Length == index)
            {
                index = 0;
            }
        }
    }
}
