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
            // �ʱ�ȭ ����
            Prepare();

            // ���� ��带 ���� ����Ʈ�� �߰��ϰ� �ֺ� ��� Ž��
            SearchNearNodes(start, end);

            AStarNode minCostNode;

            // F Cost�� ���� ���� ��带 ���� ����Ʈ�� �߰��ϰ� �ֺ� ��� Ž���ϱ⸦ �ݺ�
            while ((minCostNode = openedHeap.Pop()) != null)
            {
                // H Cost�� 0�̸� ���������� ���� ��θ� ã�� ���̹Ƿ� �ݺ� ����
                if (minCostNode.HCost == 0)
                {
                    break;
                }
                
                SearchNearNodes(minCostNode, end);
            }

            // �ִ� ��� ���� �� ��ȯ
            List<AStarNode> paths = null;

            if (minCostNode != null)
            {
                paths = new List<AStarNode>();
                AStarNode next = minCostNode;

                // ���������� ������ �Ž��� ��� ����
                while (next.Parent != null)
                {
                    paths.Insert(0, next);
                    next = next.Parent;
                }

                // ����� ��� �߰�
                paths.Insert(0, next);

                // ��θ� Hallway�� ǥ��
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

            // target ��� �ֺ��� 8�� ��忡 ���� F Cost ����
            for (int i = topLeftY; i < topLeftY + 3; ++i)
            {
                for (int j = topLeftX; j < topLeftX + 3; ++j)
                {
                    // �׸��带 ��� ��� ó��
                    if (i >= grid.GridYSize || j >= grid.GridXSize)
                        continue;

                    // Ÿ�� ����� ��� ó��
                    if (i == target.Node.GridY && j == target.Node.GridX)
                        continue;

                    CNode node = grid.Grid[i, j];

                    // �̵��� �Ұ����� ����� ��� ó��
                    if (!node.Walkable)
                        continue;

                    int gCost = target.GCost + (target.Node.isDiagonal(node) ? DIAGONAL_COST : STRAIGHT_COST);
                    int hCost = Mathf.Abs(end.GridX - node.GridX) * STRAIGHT_COST + Mathf.Abs(end.GridY - node.GridY) * STRAIGHT_COST;
                    int fCost = gCost + hCost;

                    AStarNode aStarNode = new AStarNode(target, node, gCost, hCost);

                    if (aStarGrid[i, j] != null)
                    {
                        // ���� A* ����� F Cost�� �� ���ٸ� ����
                        if (aStarGrid[i, j].FCost > aStarNode.FCost)
                        {
                            aStarGrid[i, j] = aStarNode;
                        }
                        // �ƴ϶�� �̹� openedHeap�� �����Ͱ� �����Ƿ� �������� �̵�
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
