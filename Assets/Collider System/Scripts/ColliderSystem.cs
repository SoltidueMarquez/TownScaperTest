using System;
using DefaultNamespace;
using UnityEngine;

namespace Collider_System
{
    public class ColliderSystem : MonoBehaviour
    {
        private WorldMaster worldMaster;

        [SerializeField] private GroundCollider groundCollider;

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