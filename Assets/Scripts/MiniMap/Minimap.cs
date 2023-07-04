using UnityEngine;

namespace Hypocrites.MiniMap
{
    using Hypocrites.Grid;
    using Hypocrites.Map;
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using UnityEditor.Experimental.GraphView;

    public class Minimap : MonoBehaviour
    {
        public GameObject fogPrefab;
        public Transform miniMapFogParent;
        public int fogHeight;
        public int sightDistance;
        private static GameObject[,] fogGrid;

        public Renderer rend;
        Material material;
        public float transparentSpeed;
        float opacity = 1f;

        public GameObject floorPrefab;
        public Transform miniMapFloorParent;

        Vector3 wall;
        void Awake()
        {
            fogGrid = new GameObject[CGrid.Instance.GridYSize, CGrid.Instance.GridXSize];

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

        void Start()
        {
            GameObject floorInstance = Instantiate(floorPrefab);
            floorInstance.transform.SetParent(miniMapFloorParent);

            material = rend.sharedMaterial;
        }

        public void CheckGrid(CNode PlayerNode)
        {
            //int playerX = PlayerNode.GridX, playerY = PlayerNode.GridY;
            //List<GameObject> InRangeList = new List<GameObject>();
            //List<GameObject> ObstacleList = new List<GameObject>();

            //int sX = PlayerNode.GridX - sightDistance, sY = PlayerNode.GridY - sightDistance;
            //for (int i = 0; i < sightDistance * 2 + 1; i++)
            //{
            //    for(int j = 0; j < sightDistance * 2 + 1; j++)
            //    {
            //        if (CGrid.Instance.Grid[sX + j, sY + i].Walkable == false)
            //            InRangeList.Add(fogGrid[sX + j, sY + i]);
            //        else if(CGrid.Instance.Grid[sX + j, sY + i].Walkable)
            //            ObstacleList.Add(fogGrid[sX + j, sY + i]);
            //    }
            //}
        }

        double Dist(CNode startNode, GameObject target)
        {
            Vector3 targetLocation = target.transform.position;
            CNode targetNode = CGrid.Instance.GetNodeFromWorldPosition(targetLocation);

            int x = targetNode.GridX - startNode.GridX;
            int y = targetNode.GridY - startNode.GridY;
            double distance = Math.Sqrt(x * x + y * y);
            return distance;
        }

        public void StartRemoveFog(CNode PlayerNode)
        {
            int horizontal = Mathf.RoundToInt(Camera.main.transform.forward.z);
            int vertical = Mathf.RoundToInt(Camera.main.transform.forward.x);

            for (int i = 0; i < sightDistance; i++)
            {
                for (int j = 0; j < i * 2 + 1; j++)
                {
                    Color old = material.color;
                    opacity -= transparentSpeed * Time.deltaTime;
                    material.color = new Color(old.r, old.g, old.b, opacity);

                    int startX = PlayerNode.GridX;
                    int startY = PlayerNode.GridY;

                    if (horizontal == -1)
                    {
                        startX = startX + i;
                        if (CGrid.Instance.Grid[startY + i, startX - j].Walkable)
                        {
                            Destroy(fogGrid[startY + i, startX - j]);
                            continue;
                        }
                        return;
                    }
                    else if(horizontal == 1)
                    {
                        startX = startX - i;
                        if (CGrid.Instance.Grid[startY - i, startX + j].Walkable)
                        {
                            Destroy(fogGrid[startY - i, startX + j]);
                            continue;
                        }
                        return;
                    }
                    if (vertical == -1)
                    {   
                        startY = startY - i;
                        if (CGrid.Instance.Grid[startY + j, startX - i].Walkable)
                        {
                            Destroy(fogGrid[startY + j, startX - i]);
                            continue;
                        }
                        return;
                    }
                    else if (vertical == 1)
                    {
                        startY = startY + i;
                        if (CGrid.Instance.Grid[startY - j, startX + i].Walkable)
                        {
                            Destroy(fogGrid[startY - j, startX + i]);
                            continue;
                        }
                        return;
                    }
                }
            }
        }
    }
}