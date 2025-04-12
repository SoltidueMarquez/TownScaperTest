using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Grid_Generator
{
    public class GridGenerator : MonoBehaviour
    {
        [SerializeField, Tooltip("网格半径")] private int radius;
        [SerializeField, Tooltip("网格高度")] private int height;
        [SerializeField, Tooltip("网格点单位间距")] private float cellSize;
        [SerializeField, Tooltip("网格点单位高度")] private float cellHeight;
        [SerializeField, Tooltip("网格平滑次数")] private int relaxTimes;
        private Grid grid; // 网格

        [SerializeField, Tooltip("测试小球")] private Transform addSphere;
        [SerializeField, Tooltip("测试小球")] private Transform deleteSphere;

        private void Awake()
        {
            grid = new Grid(radius, height, cellSize, cellHeight, relaxTimes); // 创建网格
        }

        private void Update()
        {
            // 测试用，性能消耗很恐怖
            foreach (var vertexY in grid.vertices.SelectMany(vertex => vertex.vertexYs))
            {
                vertexY.isActive = vertexY.isActive switch
                {
                    false when Vector3.Distance(vertexY.worldPosition, addSphere.position) < 2f => true,
                    true when Vector3.Distance(vertexY.worldPosition, deleteSphere.position) < 2f => false,
                    _ => vertexY.isActive
                };
            }

            foreach (var subQuadCube in grid.subQuads.SelectMany(subQuad => subQuad.subQuadCubes))
            {
                subQuadCube.UpdateBit();
            }
        }

        // 调试测试
        private void OnDrawGizmos()
        {
            if (grid == null) return;
            foreach (var vertexY in grid.vertices.SelectMany(vertex => vertex.vertexYs))
            {
                Gizmos.color = (vertexY.isActive) ? Color.red : Color.gray;
                Gizmos.DrawSphere(vertexY.worldPosition, (vertexY.isActive) ? 0.3f : 0.1f);
            }

            foreach (var subQuad in grid.subQuads)
            {
                foreach (var subQuadCube in subQuad.subQuadCubes)
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(subQuadCube.vertexYs[0].worldPosition, subQuadCube.vertexYs[1].worldPosition);
                    Gizmos.DrawLine(subQuadCube.vertexYs[1].worldPosition, subQuadCube.vertexYs[2].worldPosition);
                    Gizmos.DrawLine(subQuadCube.vertexYs[2].worldPosition, subQuadCube.vertexYs[3].worldPosition);
                    Gizmos.DrawLine(subQuadCube.vertexYs[3].worldPosition, subQuadCube.vertexYs[0].worldPosition);
                    
                    Gizmos.DrawLine(subQuadCube.vertexYs[4].worldPosition, subQuadCube.vertexYs[5].worldPosition);
                    Gizmos.DrawLine(subQuadCube.vertexYs[5].worldPosition, subQuadCube.vertexYs[6].worldPosition);
                    Gizmos.DrawLine(subQuadCube.vertexYs[6].worldPosition, subQuadCube.vertexYs[7].worldPosition);
                    Gizmos.DrawLine(subQuadCube.vertexYs[7].worldPosition, subQuadCube.vertexYs[4].worldPosition);
                    
                    Gizmos.DrawLine(subQuadCube.vertexYs[0].worldPosition, subQuadCube.vertexYs[4].worldPosition);
                    Gizmos.DrawLine(subQuadCube.vertexYs[1].worldPosition, subQuadCube.vertexYs[5].worldPosition);
                    Gizmos.DrawLine(subQuadCube.vertexYs[2].worldPosition, subQuadCube.vertexYs[6].worldPosition);
                    Gizmos.DrawLine(subQuadCube.vertexYs[3].worldPosition, subQuadCube.vertexYs[7].worldPosition);
                    
                    GUI.color = Color.blue;
                    Handles.Label(subQuadCube.centerPosition, subQuadCube.bit);
                }
            }
        }

        #region 动态可视化网格平滑的过程

        // private void Update()
        // {
        //     VisualMeshRelax();
        // }
        private void VisualMeshRelax()
        {
            if (relaxTimes > 0)
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