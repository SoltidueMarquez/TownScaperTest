using UnityEngine;
using Grid_Generator;
using Grid = Grid_Generator.Grid;

namespace Collider_System
{
    public class GroundCollider : MonoBehaviour
    {
        // 简单的遍历输入的grid里所有的subquad,根据subquad四个顶点创建碰撞面
        public void CreateCollider(Grid grid)
        {
            foreach (var subQuad in grid.subQuads)
            {
                var meshVertices = new Vector3[]
                {
                    subQuad.a.currentPosition,
                    subQuad.b.currentPosition,
                    subQuad.c.currentPosition,
                    subQuad.d.currentPosition
                };
                var meshTriangles = new int[]
                {
                    0, 1, 2,
                    0, 2, 3
                };
                var mesh = new Mesh
                {
                    vertices = meshVertices,
                    triangles = meshTriangles
                };
                var groundQuadColliderQuad = new GameObject(
                    "QuadCollider_" + grid.subQuads.IndexOf(subQuad), typeof(MeshCollider), typeof(GroundColliderQuad));
                groundQuadColliderQuad.transform.SetParent(transform);
                groundQuadColliderQuad.GetComponent<MeshCollider>().sharedMesh = mesh;
                groundQuadColliderQuad.GetComponent<GroundColliderQuad>().subQuad = subQuad;
                groundQuadColliderQuad.layer = LayerMask.NameToLayer("GroundCollider");
            }
        }
    }

    public class GroundColliderQuad : MonoBehaviour
    {
        public SubQuad subQuad;
    }
}