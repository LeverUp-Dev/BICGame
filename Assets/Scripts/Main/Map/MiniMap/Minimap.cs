using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject fogPrefab;

    void Awake()
    {
        for (int i = 0; i < CGrid.instance.GridYSize; i++)
        {
            for (int j = 0; j < CGrid.instance.GridXSize; j++)
            {
                CNode node = CGrid.instance.Grid[i, j];
                GameObject fogInstance = Instantiate(fogPrefab, node.WorldPosition, Quaternion.identity);
                fogInstance.transform.localScale *= CGrid.instance.gridNodeDiameter;
            }
        }

    }
}
