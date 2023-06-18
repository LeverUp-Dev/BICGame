using UnityEngine;

namespace Hypocrites.MiniMap
{
    using Hypocrites.Grid;
    using System;
    using System.Collections.Generic;

    public class Minimap : MonoBehaviour
    {
        public GameObject fogPrefab;
        public Transform miniMapFogParent;
        public int fogHeight;
        public int howRemoveFog;
        private static GameObject[,] fogGrid;

        void Awake()
        {
            fogGrid = new GameObject[CGrid.Instance.GridYSize, CGrid.Instance.GridXSize];

            for (int i = 0; i < CGrid.Instance.GridXSize; i++)
            {
                for (int j = 0; j < CGrid.Instance.GridYSize; j++)
                {
                    CNode node = CGrid.Instance.Grid[j, i];
                    GameObject fogInstance = Instantiate(fogPrefab, node.WorldPosition + Vector3.up * fogHeight, Quaternion.identity);//���� ��ġ
                    fogInstance.transform.localScale *= CGrid.Instance.GridNodeDiameter; //�Ȱ��� ũ�⸦ �׸����� ũ�⿡ ����
                    fogInstance.transform.SetParent(miniMapFogParent); //miniMapFogParent ���̾��Ű �ȿ��ٰ� ����
                    fogGrid[i, j] = fogInstance;
                }
            }
        }
        
        public void RemoveFog(CNode L)
        {
            Destroy(fogGrid[L.GridX, L.GridY]);
            for (int i = 1; i <= howRemoveFog; i++)
            {
                Destroy(fogGrid[L.GridX + i, L.GridY + i]);
                for (int j = 1; j <= i * 2 - 1; j++)
                {
                    Destroy(fogGrid[(L.GridX - i) + j, L.GridY + i]);
                }
                Destroy(fogGrid[L.GridX - i, L.GridY + i]);
                for (int j = 1; j <= i * 2 - 1; j++)
                {
                    Destroy(fogGrid[L.GridX - i, (L.GridY - i) + j]);
                }
                Destroy(fogGrid[L.GridX - i, L.GridY - i]);
                for (int j = 1; j <= i * 2 - 1; j++)
                {
                    Destroy(fogGrid[(L.GridX - i) + j, L.GridY - i]);
                }
                Destroy(fogGrid[L.GridX + i, L.GridY - i]);
                for (int j = 1; j <= i * 2 - 1; j++)
                {
                    Destroy(fogGrid[L.GridX + i, (L.GridY - i) + j]);
                }
            }
        }
    }
}