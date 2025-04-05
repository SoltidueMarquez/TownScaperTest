using System.Collections.Generic;

namespace Grid_Generator
{
    public class Grid
    {
        public static int radius;

        public static float cellSize;// 相邻点之间的距离值
        
        public readonly List<VertexHex> hexes = new List<VertexHex>();// 六边形点阵
        public readonly List<Edge> edges = new List<Edge>();// 边
        public readonly List<Triangle> triangles = new List<Triangle>();// 三角形列表

        public Grid(int radius, float cellSize)
        {
            Grid.radius = radius;
            Grid.cellSize = cellSize;
            VertexHex.Hex(hexes);// 创建六边形点阵
            Triangle.TriangleHex(hexes, edges, triangles); // 创建三角形
        }
    }
}