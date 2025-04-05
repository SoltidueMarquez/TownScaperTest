using System.Collections.Generic;

namespace Grid_Generator
{
    public class Triangle
    {
        // 三个顶点
        public readonly VertexHex a;
        public readonly VertexHex b;
        public readonly VertexHex c;

        // 三条边
        public readonly Edge ab;
        public readonly Edge bc;
        public readonly Edge ac;

        public readonly Edge[] edges;

        public Triangle(VertexHex a, VertexHex b, VertexHex c, List<Edge> edges, List<Triangle> triangles)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            
            // 查找三条边是否存在
            ab = Edge.FindEdge(a, b, edges);
            bc = Edge.FindEdge(c, b, edges);
            ac = Edge.FindEdge(a, c, edges);
            // 如果不存在这条边就先创建出来
            ab ??= new Edge(a, b, edges);
            bc ??= new Edge(c, b, edges);
            ac ??= new Edge(a, c, edges);
            // 将边放入数组
            this.edges = new Edge[] { ab, bc, ac };
            
            triangles.Add(this);
        }

        /// <summary>
        /// 创建一个环上的三角形
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        private static void TrianglesRing(int radius, List<VertexHex> vertices, List<Edge> edges, List<Triangle> triangles)
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
                    new Triangle(a, b, c, edges, triangles);
                    // 创建一个外圈顶点加两个内圈顶点组成的三角形（蓝色三角形）
                    if (j > 0) // 蓝色三角形第一圈是没有的
                    {
                        var d = inner[i * (radius - 1) + j - 1];
                        new Triangle(a, c, d, edges, triangles);
                    }
                }
            }
        }

        /// <summary>
        /// 创建所有的三角形
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        public static void TriangleHex(List<VertexHex> vertices, List<Edge> edges, List<Triangle> triangles)
        {
            for (int i = 1; i <= Grid.radius; i++)
            {
                TrianglesRing(i, vertices, edges, triangles);
            }
        }
        
        
    }
}