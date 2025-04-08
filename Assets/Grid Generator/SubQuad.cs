using System.Collections.Generic;
using UnityEngine;

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

        public SubQuad(VertexHex a, VertexMid b, VertexCenter c, VertexMid d, List<SubQuad> subQuads)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;

            subQuads.Add(this);
        }

        /// <summary>
        /// 计算网格平滑偏移值
        /// </summary>
        public void CalculateRelaxOffset()
        {
            var center = (a.currentPosition + b.currentPosition + c.currentPosition + d.currentPosition) / 4;
            // 先计算细分四边形的顶点a平滑成正方形的坐标值，
            // 即顶点a的当前坐标加顶点b绕中心点逆时针转90度得到的坐标，
            // 加顶点c绕中心点逆时针转180度得到的坐标,
            // 加顶点d绕中心点逆时针转270度得到的坐标的平均值
            var vectorA = (a.currentPosition
                           + Quaternion.AngleAxis(-90, Vector3.up) * (b.currentPosition - center) + center
                           + Quaternion.AngleAxis(-180, Vector3.up) * (c.currentPosition - center) + center
                           + Quaternion.AngleAxis(-270, Vector3.up) * (d.currentPosition - center) + center) / 4;
            // A的平滑坐标值确定之后,只需要把a的平滑坐标依次绕中心点顺时针旋转90°、180°、270°回去得到新的bcd
            var vectorB = Quaternion.AngleAxis(90, Vector3.up) * (vectorA - center) + center;
            var vectorC = Quaternion.AngleAxis(180, Vector3.up) * (vectorA - center) + center;
            var vectorD = Quaternion.AngleAxis(270, Vector3.up) * (vectorA - center) + center;
            // 计算平滑成完美的正方形需要的向量，0.1的系数是一个magic数字
            a.offset += (vectorA - a.currentPosition) * 0.1f;
            b.offset += (vectorB - b.currentPosition) * 0.1f;
            c.offset += (vectorC - c.currentPosition) * 0.1f;
            d.offset += (vectorD - d.currentPosition) * 0.1f;
        }
    }
}