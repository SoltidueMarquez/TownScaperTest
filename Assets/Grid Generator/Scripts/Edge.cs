using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Grid_Generator
{
    /// <summary>
    /// 三角形的一条边
    /// </summary>
    public class Edge
    {
        public readonly HashSet<VertexHex> hexes;// 两个端点
        public readonly VertexMid mid;// 中点
        public Edge(VertexHex a, VertexHex b, ICollection<Edge> edges, ICollection<VertexMid> mids)
        {
            hexes = new HashSet<VertexHex> { a, b };
            edges.Add(this);
            mid = new VertexMid(this, mids);
        }

        /// <summary>
        /// 在列表中查找对应边
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="edges"></param>
        /// <returns></returns>
        public static Edge FindEdge(VertexHex a, VertexHex b, IEnumerable<Edge> edges)
        {
            return edges.FirstOrDefault(edge => edge.hexes.Contains(a) && edge.hexes.Contains(b));
        }
    }
}
