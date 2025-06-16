using System;
using DefaultNamespace;
using Grid_Generator;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Collider_System
{
    public class Clicker : MonoBehaviour
    {
        private GridGenerator m_GridGenerator;
        private ColliderSystem m_ColliderSystem;
        private PlayerInputActions m_InputActions;

        private RaycastHit m_RaycastHit;
        [SerializeField] private float raycastRange;
        [SerializeField] private LayerMask clickerLayerMask;

        private VertexY vertexYSelected;
        private VertexY vertexYTarget;

        private void Awake()
        {
            var wordMaster = GetComponentInParent<WorldMaster>();
            m_GridGenerator = wordMaster.gridGenerator;
            m_ColliderSystem = wordMaster.colliderSystem;

            m_InputActions = new PlayerInputActions();
            m_InputActions.Build.Enable();
            m_InputActions.Build.Add.performed += Add;
            m_InputActions.Build.Delete.performed += Delete;
        }

        private void Update()
        {
            FindTarget();
        }

        private void FindTarget()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out m_RaycastHit, raycastRange, clickerLayerMask))
            {
                if (m_RaycastHit.transform.GetComponent<GroundColliderQuad>())
                {
                    // 没有选择快（因为与空地相交）
                    vertexYSelected = null;

                    // 计算目标快，通过向量茶积判断焦点在一个对边中点连线的哪一侧，再判断焦点在另一个对边中点连线的哪侧，就可以知道焦点具体在哪个象限了
                    Vector3 aim = m_RaycastHit.point;
                    SubQuad subQuad = m_RaycastHit.transform.GetComponent<GroundColliderQuad>().subQuad;

                    Vector3 a = subQuad.a.currentPosition;
                    Vector3 b = subQuad.b.currentPosition;
                    Vector3 c = subQuad.c.currentPosition;
                    Vector3 d = subQuad.d.currentPosition;

                    Vector3 ab = (a + b) / 2;
                    Vector3 cd = (c + d) / 2;
                    Vector3 bc = (b + c) / 2;
                    Vector3 da = (d + a) / 2;

                    float ab_cd = (aim.z - ab.z) * (aim.x - cd.x) - (aim.z - cd.z) * (aim.x - ab.x);
                    float bc_da = (aim.z - bc.z) * (aim.x - da.x) - (aim.z - da.z) * (aim.x - bc.x);

                    float a_ab_cd = (a.z - ab.z) * (a.x - cd.x) - (a.z - cd.z) * (a.x - ab.x);
                    float a_bc_da = (a.z - bc.z) * (a.x - da.x) - (a.z - da.z) * (a.x - bc.x);

                    bool on_ad_side = ab_cd * a_ab_cd >= 0;
                    bool on_ab_side = bc_da * a_bc_da >= 0;

                    if (on_ad_side && on_ab_side)
                        vertexYTarget = subQuad.a.vertexYs[1];
                    else if (on_ad_side && !on_ab_side)
                        vertexYTarget = subQuad.d.vertexYs[1];
                    else if (!on_ad_side && on_ab_side)
                        vertexYTarget = subQuad.b.vertexYs[1];
                    else
                        vertexYTarget = subQuad.c.vertexYs[1];

                    if (vertexYTarget.vertex.isBoundary)
                    {
                        vertexYTarget = null;
                    }
                }
            }
        }

        private void Add(InputAction.CallbackContext ctx)
        {
            if(vertexYTarget is { isActive: false })
                m_GridGenerator.ToggleSlot(vertexYTarget);
        }

        private void Delete(InputAction.CallbackContext ctx)
        {
            if(vertexYSelected is { isActive: false })
                m_GridGenerator.ToggleSlot(vertexYSelected);
        }

        private void OnDrawGizmos()
        {
            if (vertexYTarget != null)
                Gizmos.DrawSphere(vertexYTarget.worldPosition, 0.2f);
        }
    }
}