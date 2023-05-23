using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomMap.AStar
{
    using RandomMap.DS;

    public class AStarPathfinder
    {
        const int DIAGONAL_COST = 14;
        const int STRAIGHT_COST = 10;

        CGrid grid;

        MinHeap<AStarNode> openedHeap;
        List<AStarNode> closedList;

        AStarNode[,] aStarGrid;

        public void Prepare()
        {
            grid = CGrid.instance;

            if (openedHeap == null)
                openedHeap = new MinHeap<AStarNode>(grid.GridXSize * grid.GridYSize);
            else
                openedHeap.Clear();

            if (closedList == null)
                closedList = new List<AStarNode>();
            else
                closedList.Clear();

            if (aStarGrid == null)
            {
                aStarGrid = new AStarNode[grid.GridYSize, grid.GridXSize];
            }
            else
            {
                for (int i = 0; i < grid.GridYSize; ++i)
                {
                    for (int j = 0; j < grid.GridXSize; ++j)
                    {
                        aStarGrid[i, j] = null;
                    }
                }
            }
        }

        public List<AStarNode> FindPath(CNode start, CNode end)
        {
            // 초기화 진행
            Prepare();

            // 시작 노드를 닫힌 리스트에 추가하고 주변 노드 탐색
            SearchNearNodes(start, end);

            AStarNode minCostNode;

            // F Cost가 가장 낮은 노드를 닫힌 리스트에 추가하고 주변 노드 탐색하기를 반복
            while ((minCostNode = openedHeap.Pop()) != null)
            {
                // H Cost가 0이면 목적지까지 가는 경로를 찾은 것이므로 반복 종료
                if (minCostNode.HCost == 0)
                {
                    break;
                }
                
                SearchNearNodes(minCostNode, end);
            }

            // 최단 경로 추출 후 반환
            List<AStarNode> paths = null;

            if (minCostNode != null)
            {
                paths = new List<AStarNode>();
                AStarNode next = minCostNode;

                // 도착지부터 역으로 거슬러 경로 추출
                while (next.Parent != null)
                {
                    paths.Insert(0, next);
                    next = next.Parent;
                }

                // 출발지 노드 추가
                paths.Insert(0, next);

                // 경로를 Hallway로 표시
                foreach (AStarNode n in paths)
                {
                    n.Node.Hallway = true;
                }
            }
            else
            {
                Debug.Log($"cannot find path from ({start.GridX},{start.GridY}) to ({end.GridX},{end.GridY})");
            }

            return paths;
        }

        void SearchNearNodes(CNode target, CNode end)
        {
            AStarNode start = new AStarNode(target);

            SearchNearNodes(start, end);
        }

        void SearchNearNodes(AStarNode target, CNode end)
        {
            CGrid grid = CGrid.instance;

            closedList.Add(target);

            int topLeftX = target.Node.GridX - 1 < 0 ? 0 : target.Node.GridX - 1;
            int topLeftY = target.Node.GridY - 1 < 0 ? 0 : target.Node.GridY - 1;

            // target 노드 주변의 8개 노드에 대해 F Cost 조사
            for (int i = topLeftY; i < topLeftY + 3; ++i)
            {
                for (int j = topLeftX; j < topLeftX + 3; ++j)
                {
                    // 그리드를 벗어난 경우 처리
                    if (i >= grid.GridYSize || j >= grid.GridXSize)
                        continue;

                    // 타겟 노드인 경우 처리
                    if (i == target.Node.GridY && j == target.Node.GridX)
                        continue;

                    CNode node = grid.Grid[i, j];

                    // 이동이 불가능한 노드일 경우 처리
                    if (!node.Walkable)
                        continue;

                    int gCost = target.GCost + (target.Node.isDiagonal(node) ? DIAGONAL_COST : STRAIGHT_COST);
                    int hCost = Mathf.Abs(end.GridX - node.GridX) * STRAIGHT_COST + Mathf.Abs(end.GridY - node.GridY) * STRAIGHT_COST;
                    int fCost = gCost + hCost;

                    AStarNode aStarNode = new AStarNode(target, node, gCost, hCost);

                    if (aStarGrid[i, j] != null)
                    {
                        // 기존 A* 노드의 F Cost가 더 높다면 갱신
                        if (aStarGrid[i, j].FCost > aStarNode.FCost)
                        {
                            aStarGrid[i, j] = aStarNode;
                        }
                        // 아니라면 이미 openedHeap에 데이터가 있으므로 다음으로 이동
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        aStarGrid[i, j] = aStarNode;
                    }

                    openedHeap.Insert(fCost, aStarNode);
                }
            }
        }
    }
}
