using System.Collections.Generic;
using UnityEngine;

namespace Grid_Generator
{
    public class Grid
    {
        public static int radius;

        public static float height;// 总高度

        public static float cellSize; // 相邻点之间的距离值

        public static float cellHeight;

        public readonly List<VertexHex> hexes = new List<VertexHex>(); // 六边形点阵

        public readonly List<VertexMid> mids = new List<VertexMid>(); // 边的中点

        public readonly List<VertexCenter> centers = new List<VertexCenter>(); // 三角形和四边形的中心点

        public readonly List<Vertex> vertices = new List<Vertex>(); // 所有点的集合

        public readonly List<Edge> edges = new List<Edge>(); // 边

        public readonly List<Triangle> triangles = new List<Triangle>(); // 三角形列表

        public readonly List<Quad> quads = new List<Quad>(); // 四边形列表

        public readonly List<SubQuad> subQuads = new List<SubQuad>(); // 细分四边形列表

        public Grid(int radius, int height, float cellSize, float cellHeight, int relaxTimes)
        {
            Grid.radius = radius;
            Grid.height = height;
            Grid.cellSize = cellSize;
            Grid.cellHeight = cellHeight;
            
            VertexHex.Hex(hexes); // 创建六边形点阵
            Triangle.TriangleHex(hexes, mids, centers, edges, triangles); // 创建三角形

            var cnt = 1000; // 最大迭代次数防止死循环
            while (Triangle.HasNeighborTriangles(triangles) && cnt > 0) // 合并三角形
            {
                Triangle.RandomlyMergeTriangles(mids, centers, edges, triangles, quads);
                cnt--;
            }

            // 获取所有点的列表
            vertices.AddRange(hexes);
            vertices.AddRange(mids);
            vertices.AddRange(centers);

            // 细分三角形和四边形
            foreach (var triangle in triangles)
            {
                triangle.Subdivide(subQuads);
            }
            foreach (var quad in quads)
            {
                quad.Subdivide(subQuads);
            }
            
            for (var i = 0; i < relaxTimes; i++)
            {
                // 网格平滑
                foreach (var subQuad in subQuads) { subQuad.CalculateRelaxOffset(); }
                // 遍历每个点计算当前坐标
                foreach (var vertex in vertices) { vertex.Relax(); }
            }

            // 创建三维网格
            foreach (var vertex in vertices)
            {
                vertex.index = vertices.IndexOf(vertex);
                vertex.BoundaryCheck();
                for (var i = 0; i < Grid.height + 1; i++)
                {
                    vertex.vertexYs.Add(new VertexY(vertex, i));
                }
            }
            foreach (var subQuad in subQuads)// 注意这里subQuad_cube的纵向个数要比vertex_Y少一个
            {
                for (var i = 0; i < Grid.height; i++)
                {
                    subQuad.subQuadCubes.Add(new SubQuadCube(subQuad, i));
                }
            }
            
            
        }
    }
}