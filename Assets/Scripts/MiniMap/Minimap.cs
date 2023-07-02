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
        public int sightDistance;
        private static GameObject[,] fogGrid;

        public GameObject floorPrefab;
        public Transform miniMapFloorParent;

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

        void Start()
        {
            GameObject floorInstance = Instantiate(floorPrefab);
            floorInstance.transform.SetParent(miniMapFloorParent);
        }

        public void RemoveFog(CNode PlayerNode)
        {
            int startX = PlayerNode.GridX - sightDistance;
            int startY = PlayerNode.GridY - sightDistance;

            for (int i = 0; i < sightDistance * 2 + 1; i++)
            {
                for (int j = 0; j < sightDistance * 2 + 1; j++)
                {
                    Destroy(fogGrid[startX + j, startY + i]);
                }
            }
        }
    }
}