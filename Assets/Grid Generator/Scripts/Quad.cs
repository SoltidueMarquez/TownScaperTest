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

        public readonly VertexQuadCenter center;

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
        public Quad(VertexHex a, VertexHex b, VertexHex c, VertexHex d, ICollection<VertexCenter> centers,
            IReadOnlyCollection<Edge> edges, ICollection<Quad> quads)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;

            ab = Edge.FindEdge(a, b, edges);
            bc = Edge.FindEdge(b, c, edges);
            cd = Edge.FindEdge(c, d, edges);
            ad = Edge.FindEdge(a, d, edges);

            center = new VertexQuadCenter(this);
            centers.Add(center);

            quads.Add(this);
        }

        /// <summary>
        /// 网格细分
        /// </summary>
        public void Subdivide(List<SubQuad> subQuads)
        {
            var quadA = new SubQuad(a, ab.mid, center, ad.mid, subQuads);
            var quadB = new SubQuad(b, bc.mid, center, ab.mid, subQuads);
            var quadC = new SubQuad(c, cd.mid, center, bc.mid, subQuads);
            var quadD = new SubQuad(d, ad.mid, center, cd.mid, subQuads);
            
            a.subQuads.Add(quadA);
            b.subQuads.Add(quadB);
            c.subQuads.Add(quadC);
            d.subQuads.Add(quadD);
            center.subQuads.Add(quadA);
            center.subQuads.Add(quadB);
            center.subQuads.Add(quadC);
            center.subQuads.Add(quadD);
            ab.mid.subQuads.Add(quadA);
            ab.mid.subQuads.Add(quadB);
            bc.mid.subQuads.Add(quadB);
            bc.mid.subQuads.Add(quadC);
            cd.mid.subQuads.Add(quadC);
            cd.mid.subQuads.Add(quadD);
            ad.mid.subQuads.Add(quadD);
            ad.mid.subQuads.Add(quadA);
            
        }
    }
}