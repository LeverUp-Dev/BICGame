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
                    GameObject fogInstance = Instantiate(fogPrefab, node.WorldPosition + Vector3.up * fogHeight, Quaternion.identity);//���� ��ġ
                    fogInstance.transform.localScale *= CGrid.Instance.GridNodeDiameter; //�Ȱ��� ũ�⸦ �׸����� ũ�⿡ ����
                    fogInstance.transform.SetParent(miniMapFogParent); //miniMapFogParent ���̾��Ű �ȿ��ٰ� ����
                    fogGrid[i, j] = fogInstance;
                }
            }

        }

        void RemoveFog(Vector3 playerGrid)
        {
            
        }
    }
}