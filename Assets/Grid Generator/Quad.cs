using System.Collections.Generic;

namespace Grid_Generator
{
    /// <summary>
    /// 四边形类，用于合并三角形
    /// </summary>
    public class Quad
    {
        public readonly VertexHex a;
        public readonly VertexHex b;
        public readonly VertexHex c;
        public readonly VertexHex d;

        public readonly Edge ab;
        public readonly Edge bc;
        public readonly Edge cd;
        public readonly Edge ad;

        /// <summary>
        /// 构造函数
        /// 这边的四边形所有的边有来源于三角形
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="edges"></param>
        /// <param name="quads"></param>
        public Quad(VertexHex a, VertexHex b, VertexHex c, VertexHex d, List<Edge> edges, List<Quad> quads)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;

            ab = Edge.FindEdge(a, b, edges);
            bc = Edge.FindEdge(b, c, edges);
            cd = Edge.FindEdge(c, d, edges);
            ad = Edge.FindEdge(a, d, edges);

            quads.Add(this);
        }
    }
}