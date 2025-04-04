using UnityEngine;

namespace Grid_Generator
{
    public class GridGenerator : MonoBehaviour
    {
        [SerializeField, Tooltip("网格半径")] private int radius;
        [SerializeField, Tooltip("网格点单位间距")] private float cellSize;

        private Grid grid; // 网格

        private void Awake()
        {
            grid = new Grid(radius, cellSize); // 创建网格
        }

        // 调试测试
        private void OnDrawGizmos()
        {
            if (grid == null) return;
            foreach (var vertex in grid.hexes)
            {
                Gizmos.DrawSphere(vertex.coord.worldPosition, 0.3f);
            }
        }
    }
}