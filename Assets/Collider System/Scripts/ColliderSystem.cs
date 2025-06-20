using System;
using DefaultNamespace;
using UnityEngine;

namespace Collider_System
{
    public class ColliderSystem : MonoBehaviour
    {
        private WorldMaster worldMaster;

        [SerializeField] private GroundCollider groundCollider;

        [field: SerializeField] public SlotColliderSystem slotColliderSystem { get; private set; }

        private void Awake()
        {
            worldMaster = GetComponentInParent<WorldMaster>();
        }

        private void Start()
        {
            groundCollider.CreateCollider(worldMaster.gridGenerator.GetGrid());
        }
    }
}