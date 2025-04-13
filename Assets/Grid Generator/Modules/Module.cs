using System;
using UnityEngine;

namespace Grid_Generator.Modules
{
    [Serializable]
    public class Module
    {
        [Tooltip("名称")] public string moduleName;
        [Tooltip("网格")] public Mesh mesh;
        [Tooltip("旋转90°的次数")] public int rotation;
        [Tooltip("镜像")] public bool flip;

        public Module(string name, Mesh mesh, int rotation,bool flip)
        {
            moduleName = name;
            this.mesh = mesh;
            this.rotation = rotation;
            this.flip = flip;
        }
    }
}