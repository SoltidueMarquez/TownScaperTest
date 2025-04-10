using System;
using System.Linq;
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
            foreach (var vertexY in grid.vertices.SelectMany(vertex => vertex.VertexYs))
            {
                vertexY.isActive = vertexY.isActive switch
                {
                    false when Vector3.Distance(vertexY.worldPosition, addSphere.position) < 2f => true,
                    true when Vector3.Distance(vertexY.worldPosition, deleteSphere.position) < 2f => false,
                    _ => vertexY.isActive
                };
            }
        }

        // 调试测试
        private void OnDrawGizmos()
        {
            if (grid == null) return;
            foreach (var vertex in grid.vertices)
            {
                foreach (var vertexY in vertex.VertexYs)
                {
                    Gizmos.color = (vertexY.isActive) ? Color.red : Color.gray;
                    Gizmos.DrawSphere(vertexY.worldPosition, (vertexY.isActive) ? 0.3f : 0.1f);
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