using UnityEngine;

namespace Hypocrites.MiniMap
{
    using Hypocrites.Grid;

    public class Minimap : MonoBehaviour
    {
        public GameObject fogPrefab;
        public Transform miniMapFogParent;
        public int fogHeight;

        void Awake()
        {
            for (int i = 0; i < CGrid.Instance.GridYSize; i++)
            {
                for (int j = 0; j < CGrid.Instance.GridXSize; j++)
                {
                    CNode node = CGrid.Instance.Grid[i, j];
                    GameObject fogInstance = Instantiate(fogPrefab, node.WorldPosition + Vector3.up * fogHeight, Quaternion.identity);
                    fogInstance.transform.localScale *= CGrid.Instance.GridNodeDiameter;
                    fogInstance.transform.SetParent(miniMapFogParent);
                }
            }
        }
    }
}