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

        public List<SubQuadCube> subQuadCubes = new List<SubQuadCube>(); // 在此细分四边形平面基础上的三维四边形方块列表

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

        /// <summary>
        /// 计算中心点
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCenterPosition()
        {
            return (a.currentPosition + b.currentPosition + c.currentPosition + d.currentPosition) / 4;
        }

        public Vector3 GetMid_ab()
        {
            return (a.currentPosition + b.currentPosition) / 2;
        }

        public Vector3 GetMid_bc()
        {
            return (b.currentPosition + c.currentPosition) / 2;
        }

        public Vector3 GetMid_cd()
        {
            return (c.currentPosition + d.currentPosition) / 2;
        }

        public Vector3 GetMid_ad()
        {
            return (a.currentPosition + d.currentPosition) / 2;
        }
    }

    public class SubQuadCube
    {
        public readonly SubQuad subQuad;
        public readonly int y;
        public readonly Vector3 centerPosition;

        /// <summary>
        /// 网格的八个顶点
        /// </summary>
        public readonly VertexY[] vertexYs = new VertexY[8];

        /// <summary>
        /// bit值
        /// </summary>
        public string bit = "00000000";

        /// <summary>
        /// 上一次状态的bit值
        /// </summary>
        public string preBit = "00000000";

        public SubQuadCube(SubQuad subQuad, int y)
        {
            this.subQuad = subQuad;
            this.y = y;
            centerPosition = this.subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight * (y + 0.5f);

            // 上层顶点
            vertexYs[0] = subQuad.a.vertexYs[y + 1];
            vertexYs[1] = subQuad.b.vertexYs[y + 1];
            vertexYs[2] = subQuad.c.vertexYs[y + 1];
            vertexYs[3] = subQuad.d.vertexYs[y + 1];
            // 下层顶点
            vertexYs[4] = subQuad.a.vertexYs[y];
            vertexYs[5] = subQuad.b.vertexYs[y];
            vertexYs[6] = subQuad.c.vertexYs[y];
            vertexYs[7] = subQuad.d.vertexYs[y];

            foreach (var vertexY in vertexYs)
            {
                vertexY.subQuadCubes.Add(this);
            }
        }

        /// <summary>
        /// 根据顶点激活状态计算bit的值
        /// </summary>
        public void UpdateBit()
        {
            preBit = bit;
            bit = string.Empty;
            for (var i = 0; i < 8; i++)
            {
                bit += (vertexYs[i].isActive) ? "1" : "0";
            }
        }
    }
}