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
        public readonly HashSet<VertexHex> hexes;// 两个断点

        public Edge(VertexHex a, VertexHex b, List<Edge> edges)
        {
            hexes = new HashSet<VertexHex> { a, b };
            edges.Add(this);
        }

        /// <summary>
        /// 在列表中查找对应边
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="edges"></param>
        /// <returns></returns>
        public static Edge FindEdge(VertexHex a, VertexHex b, List<Edge> edges)
        {
            return edges.FirstOrDefault(edge => edge.hexes.Contains(a) && edge.hexes.Contains(b));
        }
    }
}
