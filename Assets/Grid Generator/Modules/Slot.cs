using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Grid_Generator.Modules
{
    public class Slot : MonoBehaviour
    {
        public List<Module> possibleModules;
        public SubQuadCube subQuadCube;
        public GameObject module;
        public Material material;

        private void Awake()
        {
            module = new GameObject("Module", typeof(MeshFilter), typeof(MeshRenderer));
            module.transform.SetParent(transform);
            module.transform.localPosition = Vector3.zero;
        }

        public void Initialize(ModuleLibrary moduleLibrary, SubQuadCube subQuadCube,Material material)
        {
            this.subQuadCube = subQuadCube;
            ResetPossibleModules(moduleLibrary);
            this.material = material;
        }

        public void ResetPossibleModules(ModuleLibrary moduleLibrary)
        {
            this.possibleModules = moduleLibrary.GetModules(subQuadCube.bit);
        }

        // 旋转mesh
        private void RotateModule(Mesh mesh, int rotation)
        {
            if (rotation == 0) return; // 遍历网格的所有点，以原点纵轴旋转90°
            var vertices = mesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Quaternion.AngleAxis(90 * rotation, Vector3.up) * vertices[i];
            }

            mesh.vertices = vertices;
        }

        // 镜像mesh，遍历所有的点沿x轴镜像
        private void FlipModule(Mesh mesh, bool flip)
        {
            if (!flip) return;
            var vertices = mesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(-vertices[i].x, vertices[i].y, vertices[i].z);
            }

            mesh.vertices = vertices;
            mesh.triangles = mesh.triangles.Reverse().ToArray();
        }

        // 双层插值计算对应点的位置,把正方形的模块变形到有机网格
        private void DeformModule(Mesh mesh, SubQuadCube subQuadCube)
        {
            var vertices = mesh.vertices;
            var subQuad = subQuadCube.subQuad;
            // 因为建模的时候在blender中将模块的大小设置成了1m，所以这里lerp的比例只需要用点的坐标值加上0.5
            for (var i = 0; i < vertices.Length; i++)
            {
                var adX = Vector3.Lerp(subQuad.a.currentPosition, subQuad.b.currentPosition, (vertices[i].x + 0.5f));
                var bcX = Vector3.Lerp(subQuad.b.currentPosition, subQuad.c.currentPosition, (vertices[i].x + 0.5f));
                // 最后得到的结果是以subQuadCube的中心为中心的
                vertices[i] = Vector3.Lerp(adX, bcX, (vertices[i].z + 0.5f))
                    + Vector3.up * vertices[i].y * Grid.cellHeight - subQuad.GetCenterPosition();
            }

            mesh.vertices = vertices;
        }

        // 根据我们之前全部导入到library的module来写出更新模块的mesh的函数
        public void UpdateModule(Module module)
        {
            var mesh = this.module.GetComponent<MeshFilter>().mesh;
            mesh = module.mesh;
            FlipModule(mesh, module.flip);
            RotateModule(mesh, module.rotation);
            DeformModule(mesh, subQuadCube);
            this.module.GetComponent<MeshRenderer>().material = material;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}