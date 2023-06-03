namespace Hypocrites.Map.AStar
{
    using Hypocrites.Grid;

    public class AStarNode
    {
        public AStarNode Parent { get; set; }

        public CNode Node { get; set; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost { get; set; }

        public AStarNode(CNode node)
        {
            Node = node;
        }

        public AStarNode(AStarNode parent, CNode node, int gCost, int hCost)
        {
            Parent = parent;
            Node = node;
            GCost = gCost;
            HCost = hCost;
            FCost = GCost + HCost;
        }
    }
}
