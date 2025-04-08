using System;
using UnityEngine;

namespace Grid_Generator
{
    public class GridGenerator : MonoBehaviour
    {
        [SerializeField, Tooltip("网格半径")] private int radius;
        [SerializeField, Tooltip("网格点单位间距")] private float cellSize;
        [SerializeField, Tooltip("网格平滑次数")] private int relaxTimes;
        private Grid grid; // 网格

        private void Awake()
        {
            grid = new Grid(radius, cellSize, relaxTimes); // 创建网格
        }

        // 调试测试
        private void OnDrawGizmos()
        {
            if (grid == null) return;
            Gizmos.color = Color.yellow;
            foreach (var vertex in grid.hexes)
            {
                Gizmos.DrawSphere(vertex.currentPosition, 0.1f);
            }

            // Gizmos.color = Color.yellow;
            // foreach (var triangle in grid.triangles)
            // {
            //     Gizmos.DrawLine(triangle.a.currentPosition, triangle.b.currentPosition);
            //     Gizmos.DrawLine(triangle.a.currentPosition, triangle.c.currentPosition);
            //     Gizmos.DrawLine(triangle.b.currentPosition, triangle.c.currentPosition);
            //     //Gizmos.DrawSphere((triangle.a.currentPosition + triangle.b.currentPosition + triangle.c.currentPosition) /3, 0.05f);
            // }
            //
            // Gizmos.color = Color.green;
            // foreach (var quad in grid.quads)
            // {
            //     Gizmos.DrawLine(quad.a.currentPosition, quad.b.currentPosition);
            //     Gizmos.DrawLine(quad.b.currentPosition, quad.c.currentPosition);
            //     Gizmos.DrawLine(quad.c.currentPosition, quad.d.currentPosition);
            //     Gizmos.DrawLine(quad.a.currentPosition, quad.d.currentPosition);
            // }

            Gizmos.color = Color.red;
            foreach (var mid in grid.mids)
            {
                Gizmos.DrawSphere(mid.currentPosition, 0.1f);
            }
            
            Gizmos.color = Color.cyan;
            foreach (var center in grid.centers)
            {
                Gizmos.DrawSphere(center.currentPosition, 0.1f);
            }
            
            Gizmos.color = Color.white;
            foreach (var subQuad in grid.subQuads)
            {
                Gizmos.DrawLine(subQuad.a.currentPosition, subQuad.b.currentPosition);
                Gizmos.DrawLine(subQuad.b.currentPosition, subQuad.c.currentPosition);
                Gizmos.DrawLine(subQuad.c.currentPosition, subQuad.d.currentPosition);
                Gizmos.DrawLine(subQuad.a.currentPosition, subQuad.d.currentPosition);
            }
        }

        #region 动态可视化网格平滑的过程
        // private void Update()
        // {
        //     VisualMeshRelax();
        // }
        private void VisualMeshRelax()
        {
            if(relaxTimes>0)
            {
                // 网格平滑
                foreach (var subQuad in grid.subQuads)
                {
                    subQuad.CalculateRelaxOffset();
                }

                // 遍历每个点计算当前坐标
                foreach (var vertex in grid.vertices)
                {
                    vertex.Relax();
                }

                relaxTimes--;
            }
        }
        #endregion
    }
}