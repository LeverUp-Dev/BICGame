using UnityEngine;

namespace Hypocrites.MiniMap
{
    using Hypocrites.Grid;
    using System.Collections.Generic;

    public class Minimap : MonoBehaviour
    {
        public GameObject fogPrefab;
        public Transform miniMapFogParent;
        public int fogHeight;
        private GameObject[,] fogGrid;

        void Awake()
        {
            for (int i = 0; i < CGrid.Instance.GridYSize; i++)
            {
                for (int j = 0; j < CGrid.Instance.GridXSize; j++)
                {
                    CNode node = CGrid.Instance.Grid[i, j];
                    GameObject fogInstance = Instantiate(fogPrefab, node.WorldPosition + Vector3.up * fogHeight, Quaternion.identity);//생성 위치
                    fogInstance.transform.localScale *= CGrid.Instance.GridNodeDiameter; //안개를 크기를 그리드의 크기에 맞춤
                    fogInstance.transform.SetParent(miniMapFogParent); //miniMapFogParent 하이어라키 안에다가 생성
                    fogGrid[i, j] = fogInstance;
                }
            }

        }

        void RemoveFog(Vector3 playerGrid)
        {
            
        }
    }
}