using System.Linq;
using Grid_Generator.Modules;
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

        [SerializeField, Tooltip("")] private ModuleLibrary moduleLibrary;
        [SerializeField, Tooltip("")] private Material moduleMaterial;

        [SerializeField, Tooltip("测试小球")] private Transform addSphere;
        [SerializeField, Tooltip("测试小球")] private Transform deleteSphere;
        private Grid grid; // 网格

        private void Awake()
        {
            grid = new Grid(radius, height, cellSize, cellHeight, relaxTimes); // 创建网格
            moduleLibrary = Instantiate(moduleLibrary);
        }

        private void Update()
        {
            // 测试用，性能消耗很恐怖
            foreach (var vertexY in grid.vertices.SelectMany(vertex => vertex.vertexYs))
            {
                vertexY.isActive = vertexY.isActive switch
                {
                    false when (Vector3.Distance(vertexY.worldPosition, addSphere.position) < 1.5f) && (!vertexY.isBoundary) => true,
                    true when Vector3.Distance(vertexY.worldPosition, deleteSphere.position) < 1.5f => false,
                    _ => vertexY.isActive
                };
            }

            foreach (var subQuadCube in grid.subQuads.SelectMany(subQuad => subQuad.subQuadCubes))
            {
                subQuadCube.UpdateBit();
                if (subQuadCube.preBit != subQuadCube.bit)
                {
                    UpdateSlot(subQuadCube);
                }
            }
        }

        private void UpdateSlot(SubQuadCube subQuadCube)
        {
            var slotName = $"Slot_{grid.subQuads.IndexOf(subQuadCube.subQuad)}_{subQuadCube.y}";

            var slotGameObject = transform.Find(slotName) ? transform.Find(slotName).gameObject : null;

            if (slotGameObject == null) // 如果没找到slot，当bit值变化就创建新的slot
            {
                if (subQuadCube.bit != "00000000" && subQuadCube.bit != "11111111")
                {
                    slotGameObject = new GameObject(slotName, typeof(Slot));
                    slotGameObject.transform.SetParent(transform);
                    slotGameObject.transform.localPosition = subQuadCube.centerPosition;
                    var slot = slotGameObject.GetComponent<Slot>();
                    slot.Initialize(moduleLibrary, subQuadCube, moduleMaterial);
                    slot.UpdateModule(slot.possibleModules[0]);
                }
            }
            else
            {
                var slot = slotGameObject.GetComponent<Slot>();
                if (subQuadCube.bit is "00000000" or "11111111")
                {
                    Destroy(slotGameObject);
                    Resources.UnloadUnusedAssets();
                }
                else // 更新slot的module
                {
                    slot.ResetPossibleModules(moduleLibrary);
                    slot.UpdateModule(slot.possibleModules[0]);
                }
            }
        }

        public void ToggleSlot(VertexY vertexY)
        {
            vertexY.isActive = !vertexY.isActive;
            foreach (var subQuadCube in vertexY.subQuadCubes)
            {
                subQuadCube.UpdateBit();
                UpdateSlot(subQuadCube);
            }
        }

        public Grid GetGrid()
        {
            return grid;
        }

        // 调试测试
        private void OnDrawGizmos()
        {
            // if (grid == null) return;
            // foreach (var vertexY in grid.vertices.SelectMany(vertex => vertex.vertexYs))
            // {
            //     Gizmos.color = (vertexY.isActive) ? Color.red : Color.gray;
            //     Gizmos.DrawSphere(vertexY.worldPosition, (vertexY.isActive) ? 0.3f : 0.1f);
            // }
            //
            // foreach (var subQuad in grid.subQuads)
            // {
            //     foreach (var subQuadCube in subQuad.subQuadCubes)
            //     {
            //         Gizmos.color = Color.gray;
            //         Gizmos.DrawLine(subQuadCube.vertexYs[0].worldPosition, subQuadCube.vertexYs[1].worldPosition);
            //         Gizmos.DrawLine(subQuadCube.vertexYs[1].worldPosition, subQuadCube.vertexYs[2].worldPosition);
            //         Gizmos.DrawLine(subQuadCube.vertexYs[2].worldPosition, subQuadCube.vertexYs[3].worldPosition);
            //         Gizmos.DrawLine(subQuadCube.vertexYs[3].worldPosition, subQuadCube.vertexYs[0].worldPosition);
            //         
            //         Gizmos.DrawLine(subQuadCube.vertexYs[4].worldPosition, subQuadCube.vertexYs[5].worldPosition);
            //         Gizmos.DrawLine(subQuadCube.vertexYs[5].worldPosition, subQuadCube.vertexYs[6].worldPosition);
            //         Gizmos.DrawLine(subQuadCube.vertexYs[6].worldPosition, subQuadCube.vertexYs[7].worldPosition);
            //         Gizmos.DrawLine(subQuadCube.vertexYs[7].worldPosition, subQuadCube.vertexYs[4].worldPosition);
            //         
            //         Gizmos.DrawLine(subQuadCube.vertexYs[0].worldPosition, subQuadCube.vertexYs[4].worldPosition);
            //         Gizmos.DrawLine(subQuadCube.vertexYs[1].worldPosition, subQuadCube.vertexYs[5].worldPosition);
            //         Gizmos.DrawLine(subQuadCube.vertexYs[2].worldPosition, subQuadCube.vertexYs[6].worldPosition);
            //         Gizmos.DrawLine(subQuadCube.vertexYs[3].worldPosition, subQuadCube.vertexYs[7].worldPosition);
            //         
            //         GUI.color = Color.blue;
            //         Handles.Label(subQuadCube.centerPosition, subQuadCube.bit);
            //     }
            // }
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