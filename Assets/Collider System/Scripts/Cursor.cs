using Grid_Generator;
using UnityEngine;
using Grid = Grid_Generator.Grid;

namespace Collider_System
{
    public class Cursor : MonoBehaviour
    {
        public void UpdateCursor(RaycastHit raycastHit,RaycastHitType raycastHitType, VertexY selected, VertexY target)
        {
            GetComponent<MeshFilter>().mesh.Clear();
            if (raycastHitType == RaycastHitType.none) return;
            Vertex vertex = target.vertex;
            GetComponent<MeshFilter>().mesh = vertex.CreateMesh();
            transform.position = target.worldPosition + Vector3.down * (Grid.cellHeight * (0.5f));
        }
    }
}