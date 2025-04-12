using System;
using System.Collections.Generic;
using System.Linq;

namespace Grid_Generator
{
    public class Triangle
    {
        // 三个顶点
        public readonly VertexHex a;
        public readonly VertexHex b;
        public readonly VertexHex c;

        private readonly VertexHex[] vertices;

        // 三条边(其实可以去掉)
        private readonly Edge ab;
        private readonly Edge bc;
        private readonly Edge ac;
        private readonly Edge[] edges;

        public readonly VertexTriangleCenter center;

        public Triangle(VertexHex a, VertexHex b, VertexHex c, ICollection<VertexMid> mids,
            ICollection<VertexCenter> centers, ICollection<Edge> edges,
            ICollection<Triangle> triangles
        )
        {
            this.a = a;
            this.b = b;
            this.c = c;
            vertices = new VertexHex[] { a, b, c };

            // 查找三条边是否存在
            ab = Edge.FindEdge(a, b, edges);
            bc = Edge.FindEdge(c, b, edges);
            ac = Edge.FindEdge(a, c, edges);
            // 如果不存在这条边就先创建出来
            ab ??= new Edge(a, b, edges, mids);
            bc ??= new Edge(c, b, edges, mids);
            ac ??= new Edge(a, c, edges, mids);
            // 将边放入数组
            this.edges = new Edge[] { ab, bc, ac };
            // 计算中心点
            center = new VertexTriangleCenter(this);
            centers.Add(center);

            triangles.Add(this);
        }

        /// <summary>
        /// 创建一个环上的三角形
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        private static void TrianglesRing(int radius, List<VertexHex> vertices, ICollection<VertexMid> mids,
            ICollection<VertexCenter> centers,
            ICollection<Edge> edges,
            ICollection<Triangle> triangles)
        {
            // 首先获取对应的内圈外圈点
            var inner = VertexHex.GrabRing(radius - 1, vertices);
            var outer = VertexHex.GrabRing(radius, vertices);
            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < radius; j++)
                {
                    // 创建两个外圈顶点加一个内圈顶点组成的三角形（黄色三角形）
                    var a = outer[i * radius + j];
                    var b = outer[(i * radius + j + 1) % outer.Count]; // 转一圈之后需要回到最初的点所以要取余
                    var c = inner[(i * (radius - 1) + j) % inner.Count];
                    new Triangle(a, b, c, mids, centers, edges, triangles);
                    // 创建一个外圈顶点加两个内圈顶点组成的三角形（蓝色三角形）
                    if (j > 0) // 蓝色三角形第一圈是没有的
                    {
                        var d = inner[i * (radius - 1) + j - 1];
                        new Triangle(a, c, d, mids, centers, edges, triangles);
                    }
                }
            }
        }

        /// <summary>
        /// 创建所有的三角形
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        public static void TriangleHex(List<VertexHex> vertices, ICollection<VertexMid> mids,
            ICollection<VertexCenter> centers, List<Edge> edges,
            List<Triangle> triangles)
        {
            for (int i = 1; i <= Grid.radius; i++)
            {
                TrianglesRing(i, vertices, mids, centers, edges, triangles);
            }
        }

        /// <summary>
        /// 判断两个三角形是否相邻
        /// 将三角形的edges转化为哈希集合与边表求交集
        /// 有一条边重合就是相邻三角形
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsNeighbor(Triangle target)
        {
            var intersection = new HashSet<Edge>(edges);
            intersection.IntersectWith(target.edges);
            return intersection.Count == 1;
        }

        /// <summary>
        /// 查找所有相邻三角形
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public List<Triangle> FindAllNeighborTriangles(List<Triangle> triangles)
        {
            return triangles.Where(IsNeighbor).ToList();
        }

        /// <summary>
        /// 找相邻三角形的重合边
        /// </summary>
        /// <returns></returns>
        public Edge NeighborEdge(Triangle neighbor)
        {
            var intersection = new HashSet<Edge>(edges);
            intersection.IntersectWith(neighbor.edges);
            return intersection.Single();
        }

        /// <summary>
        /// 找自身三角形中和相邻三角形不共有的顶点
        /// </summary>
        /// <param name="neighbor"></param>
        /// <returns></returns>
        public VertexHex IsolatedVertexSelf(Triangle neighbor)
        {
            var exception = new HashSet<VertexHex>(vertices);
            exception.ExceptWith(NeighborEdge(neighbor).hexes);
            return exception.Single();
        }

        /// <summary>
        /// 找邻居三角形中和相邻三角形不共有的顶点
        /// </summary>
        /// <param name="neighbor"></param>
        /// <returns></returns>
        public VertexHex IsolatedVertexNeighbor(Triangle neighbor)
        {
            var exception = new HashSet<VertexHex>(neighbor.vertices);
            exception.ExceptWith(NeighborEdge(neighbor).hexes);
            return exception.Single();
        }

        /// <summary>
        /// 合并三角形
        /// </summary>
        /// <param name="neighbor"></param>
        /// <param name="edges"></param>
        /// <param name="triangles"></param>
        /// <param name="quads"></param>
        public void MergeNeighborTriangles(Triangle neighbor, List<VertexMid> mids, ICollection<VertexCenter> centers,
            List<Edge> edges,
            List<Triangle> triangles, List<Quad> quads)
        {
            // 点的顺序为顺时针
            var a = IsolatedVertexSelf(neighbor);
            var b = vertices[(Array.IndexOf(vertices, a) + 1) % 3];
            var c = IsolatedVertexNeighbor(neighbor);
            var d = neighbor.vertices[(Array.IndexOf(neighbor.vertices, c) + 1) % 3];
            var quad = new Quad(a, b, c, d, centers, edges, quads);
            // 将重合边与它的中点去掉
            var edge = NeighborEdge(neighbor);
            edges.Remove(edge);
            mids.Remove(edge.mid);
            // 移除两个合并三角形的中心点
            centers.Remove(this.center);
            centers.Remove(neighbor.center);
            // 去掉两个三角形
            triangles.Remove(this);
            triangles.Remove(neighbor);
        }

        /// <summary>
        ///  判断网格中是否还有相邻三角形
        /// </summary>
        /// <returns></returns>
        public static bool HasNeighborTriangles(List<Triangle> triangles)
        {
            return triangles.Any(a => triangles.Any(a.IsNeighbor));
        }

        /// <summary>
        /// 随即抓取相邻三角形合并
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="triangles"></param>
        /// <param name="quads"></param>
        public static void RandomlyMergeTriangles(List<VertexMid> mids, ICollection<VertexCenter> centers,
            List<Edge> edges, List<Triangle> triangles,
            List<Quad> quads)
        {
            // 随机抓取一个三角形查看是否有相邻三角形
            var randomIndex = UnityEngine.Random.Range(0, triangles.Count);
            var neighbors = triangles[randomIndex].FindAllNeighborTriangles(triangles);
            if (neighbors.Count != 0)
            {
                var randomNeighborIndex = UnityEngine.Random.Range(0, neighbors.Count);
                triangles[randomIndex]
                    .MergeNeighborTriangles(neighbors[randomNeighborIndex], mids, centers, edges, triangles, quads);
            }
        }

        /// <summary>
        /// 网格细分
        /// </summary>
        public void Subdivide(List<SubQuad> subQuads)
        {
            var quadA = new SubQuad(a, ab.mid, center, ac.mid, subQuads);
            var quadB = new SubQuad(b, bc.mid, center, ab.mid, subQuads);
            var quadC = new SubQuad(c, ac.mid, center, bc.mid, subQuads);
        }
    }
}