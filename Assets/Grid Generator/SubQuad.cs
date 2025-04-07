namespace Grid_Generator
{
    /// <summary>
    /// 细分后得到的小四边形
    /// </summary>
    public class SubQuad
    {
        public readonly VertexHex a;
        public readonly VertexMid b;
        public readonly VertexCenter c;
        public readonly VertexMid d;

        public SubQuad(VertexHex a, VertexMid b, VertexCenter c, VertexMid d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
    }
}