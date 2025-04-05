using System.Collections.Generic;
using UnityEngine;

namespace Grid_Generator
{
    public class Grid
    {
        public static int radius;

        public static float cellSize;// 相邻点之间的距离值
        
        public readonly List<VertexHex> hexes = new List<VertexHex>();// 六边形点阵
        public readonly List<Edge> edges = new List<Edge>();// 边
        public readonly List<Triangle> triangles = new List<Triangle>();// 三角形列表
        public readonly List<Quad> quads = new List<Quad>();// 四边形列表
        
        public Grid(int radius, float cellSize)
        {
            Grid.radius = radius;
            Grid.cellSize = cellSize;
            VertexHex.Hex(hexes);// 创建六边形点阵
            Triangle.TriangleHex(hexes, edges, triangles); // 创建三角形

            var cnt = 1000;
            while (Triangle.HasNeighborTriangles(triangles) && cnt > 0)
            {
                Triangle.RandomlyMergeTriangles(edges,triangles,quads);
                cnt--;
            }
        }
    }
}