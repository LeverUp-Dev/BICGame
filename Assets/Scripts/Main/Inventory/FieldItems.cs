using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldItems : MonoBehaviour
{
    public Item item;
    public SpriteRenderer image;

    public void SetItem(Item _item)
    {
        item.itemType = _item.itemType;
        item.itemName = _item.itemName;
        item.itemImage = _item.itemImage;
        item.effs = _item.effs;

        image.sprite = _item.itemImage;
    }

    public Item GetItem()
    {
        return item;
    }

    public void DestoryItem()
    {
        Destroy(gameObject);
    }
}
