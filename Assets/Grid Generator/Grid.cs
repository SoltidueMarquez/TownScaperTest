using System.Collections.Generic;

namespace Grid_Generator
{
    public class Grid
    {
        public static int radius;

        public static float cellSize;// 相邻点之间的距离值
        
        public readonly List<VertexHex> hexes = new List<VertexHex>();

        public Grid(int radius, float cellSize)
        {
            Grid.radius = radius;
            Grid.cellSize = cellSize;
            VertexHex.Hex(hexes);
        }
    }
}