using System.Collections.Generic;
using UnityEngine;

namespace Grid_Generator
{
    public class Grid
    {
        public static int radius;

        public static float cellSize; // 相邻点之间的距离值

        public readonly List<VertexHex> hexes = new List<VertexHex>(); // 六边形点阵

        public readonly List<VertexMid> mids = new List<VertexMid>(); // 边的中点

        public readonly List<VertexCenter> centers = new List<VertexCenter>(); // 三角形和四边形的中心点

        public readonly List<Edge> edges = new List<Edge>(); // 边

        public readonly List<Triangle> triangles = new List<Triangle>(); // 三角形列表

        public readonly List<Quad> quads = new List<Quad>(); // 四边形列表

        public readonly List<SubQuad> subQuads = new List<SubQuad>();// 细分四边形列表

        public Grid(int radius, float cellSize)
        {
            Grid.radius = radius;
            Grid.cellSize = cellSize;
            VertexHex.Hex(hexes); // 创建六边形点阵
            Triangle.TriangleHex(hexes, mids, centers, edges, triangles); // 创建三角形

            var cnt = 1000;// 最大迭代次数防止死循环
            while (Triangle.HasNeighborTriangles(triangles) && cnt > 0)// 合并三角形
            {
                Triangle.RandomlyMergeTriangles(mids, centers, edges, triangles, quads);
                cnt--;
            }
            
            // 细分三角形和四边形
            foreach (var triangle in triangles) { triangle.Subdivide(subQuads); }
            foreach (var quad in quads) { quad.Subdivide(subQuads); }
        }
    }
}