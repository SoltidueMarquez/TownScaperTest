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
            Gizmos.color = Color.yellow;
            foreach (var vertex in grid.hexes)
            {
                Gizmos.DrawSphere(vertex.coord.worldPosition, 0.1f);
            }

            Gizmos.color = Color.yellow;
            foreach (var triangle in grid.triangles)
            {
                Gizmos.DrawLine(triangle.a.coord.worldPosition, triangle.b.coord.worldPosition);
                Gizmos.DrawLine(triangle.a.coord.worldPosition, triangle.c.coord.worldPosition);
                Gizmos.DrawLine(triangle.b.coord.worldPosition, triangle.c.coord.worldPosition);
                //Gizmos.DrawSphere((triangle.a.coord.worldPosition + triangle.b.coord.worldPosition + triangle.c.coord.worldPosition) /3, 0.05f);
            }

            Gizmos.color = Color.green;
            foreach (var quad in grid.quads)
            {
                Gizmos.DrawLine(quad.a.coord.worldPosition, quad.b.coord.worldPosition);
                Gizmos.DrawLine(quad.b.coord.worldPosition, quad.c.coord.worldPosition);
                Gizmos.DrawLine(quad.c.coord.worldPosition, quad.d.coord.worldPosition);
                Gizmos.DrawLine(quad.a.coord.worldPosition, quad.d.coord.worldPosition);
            }

            Gizmos.color = Color.red;
            foreach (var mid in grid.mids)
            {
                Gizmos.DrawSphere(mid.InitialPosition, 0.1f);
            }
            
            Gizmos.color = Color.cyan;
            foreach (var center in grid.centers)
            {
                Gizmos.DrawSphere(center.InitialPosition, 0.1f);
            }
            
            Gizmos.color = Color.white;
            foreach (var subQuad in grid.subQuads)
            {
                Gizmos.DrawLine(subQuad.a.InitialPosition, subQuad.b.InitialPosition);
                Gizmos.DrawLine(subQuad.b.InitialPosition, subQuad.c.InitialPosition);
                Gizmos.DrawLine(subQuad.c.InitialPosition, subQuad.d.InitialPosition);
                Gizmos.DrawLine(subQuad.a.InitialPosition, subQuad.d.InitialPosition);
            }
        }
    }
}