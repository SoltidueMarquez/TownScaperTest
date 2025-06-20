using System.Collections.Generic;
using System.Linq;
using Grid_Generator;
using UnityEngine;
using Grid = Grid_Generator.Grid;

namespace Collider_System
{
    public class SlotColliderSystem : MonoBehaviour
    {
        private string GetSlotColliderName(VertexY vertexY)
        {
            return $"SlotCollider_{vertexY.name}";
        }

        public void CreateCollider(VertexY vertexY)
        {
            // 创建Slot碰撞体父对象
            GameObject slotCollider = new GameObject(GetSlotColliderName(vertexY), typeof(SlotCollider));
            slotCollider.GetComponent<SlotCollider>().vertexY = vertexY;
            slotCollider.transform.SetParent(transform);
            slotCollider.transform.localPosition = vertexY.worldPosition;

            // 创建顶部碰撞面-与cursor相同
            GameObject top = new GameObject("top_to_" + (vertexY.y + 1), typeof(MeshCollider), typeof(SlotColliderTop));
            top.GetComponent<MeshCollider>().sharedMesh = vertexY.vertex.CreateMesh();
            top.layer = LayerMask.NameToLayer("SlotCollider");
            top.transform.SetParent(slotCollider.transform);
            top.transform.localPosition = Vector3.up * Grid.cellHeight / 2;

            // 创建底部碰撞面-将顶部面翻转
            GameObject bottom = new GameObject("bottom_to_" + (vertexY.y - 1), typeof(MeshCollider), typeof(SlotColliderBottom));
            bottom.GetComponent<MeshCollider>().sharedMesh = vertexY.vertex.CreateMesh();
            bottom.GetComponent<MeshCollider>().sharedMesh.triangles = bottom.GetComponent<MeshCollider>().sharedMesh.triangles.Reverse().ToArray();
            bottom.layer = LayerMask.NameToLayer("SlotCollider");
            bottom.transform.SetParent(slotCollider.transform);
            bottom.transform.localPosition = Vector3.down * Grid.cellHeight / 2;

            //创建侧向碰撞面，并计算相邻块
            if (vertexY.vertex is VertexCenter center)
            {
                List<Mesh> meshes = center.CreateSideMesh();
                for (int i = 0; i < center.subQuads.Count; i++)
                {
                    VertexY neighbor = center.subQuads[i].d.vertexYs[vertexY.y];
                    GameObject side = new GameObject("side_to_" + neighbor.name, typeof(MeshCollider), typeof(SlotColliderSide));
                    side.GetComponent<SlotColliderSide>().neighbor = neighbor;
                    side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                    side.layer = LayerMask.NameToLayer("SlotCollider");
                    side.transform.SetParent(slotCollider.transform);
                    side.transform.localPosition = Vector3.zero;
                }
            }
            else if (vertexY.vertex is VertexHex hex)
            {
                List<Mesh> meshes = hex.CreateSideMesh();
                for (int i = 0; i < hex.subQuads.Count; i++)
                {
                    VertexY neighbor = hex.subQuads[i].b.vertexYs[vertexY.y];
                    GameObject side = new GameObject("side_to_" + neighbor.name, typeof(MeshCollider), typeof(SlotColliderSide));
                    side.GetComponent<SlotColliderSide>().neighbor = neighbor;
                    side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                    side.layer = LayerMask.NameToLayer("SlotCollider");
                    side.transform.SetParent(slotCollider.transform);
                    side.transform.localPosition = Vector3.zero;
                }
            }
            else
            {
                List<Mesh> meshes =((VertexMid)vertexY.vertex).CreateSideMesh();
                for (int i = 0; i < 4; i++)
                {
                    VertexY neighbor;
                    if (vertexY.vertex == vertexY.vertex.subQuads[i].b)
                        neighbor = vertexY.vertex.subQuads[i].c.vertexYs[vertexY.y]; 
                    else neighbor =vertexY.vertex.subQuads[i].a.vertexYs[vertexY.y];
                    GameObject side = new GameObject("side_to_" + neighbor.name, typeof(MeshCollider), typeof(SlotColliderSide));
                    side.GetComponent<SlotColliderSide>().neighbor = neighbor;
                    side.GetComponent<MeshCollider>().sharedMesh =meshes[i];
                    side.layer = LayerMask.NameToLayer("SlotCollider");
                    side.transform.SetParent(slotCollider.transform);
                    side.transform.localPosition =Vector3.zero;
                }
            }
        }

        public void DestroyCollider(VertexY vertexY)
        {
            Destroy(transform.Find(GetSlotColliderName(vertexY)).gameObject);
            Resources.UnloadUnusedAssets();
        }
    }

    public class SlotCollider : MonoBehaviour { public VertexY vertexY; }

    public class SlotColliderTop : MonoBehaviour { }

    public class SlotColliderBottom : MonoBehaviour { }

    public class SlotColliderSide : MonoBehaviour { public VertexY neighbor; }
}