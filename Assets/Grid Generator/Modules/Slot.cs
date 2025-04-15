using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Grid_Generator.Modules
{
    public class Slot : MonoBehaviour
    {
        [SerializeField] public List<Module> possibleModules = new List<Module>();
        public GameObject module;
        private SubQuadCube SubQuadCube;
        public Material material;

        public void Awake()
        {
            module = new GameObject("Module", typeof(MeshFilter), typeof(MeshRenderer));
            module.transform.SetParent(transform);
            module.transform.localPosition = Vector3.zero;
        }


        public void Initialize(ModuleLibrary ml, SubQuadCube cq, Material m)
        {
            SubQuadCube = cq;

            ResetPossibleModules(ml);
            material = m;
        }

        public void ResetPossibleModules(ModuleLibrary ml)
        {
            possibleModules = ml.GetModules(SubQuadCube.bit).ConvertAll(x => x);
        }
        
        private void RotateModule(Mesh mesh, int rotateTimes)
        {
            if (rotateTimes == 0) return;
            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = Quaternion.AngleAxis(90 * rotateTimes, Vector3.up) * vertices[i];
            }

            mesh.vertices = vertices;
        }

        private void FlipModule(Mesh mesh, bool flip)
        {
            if (!flip) return;
            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = new Vector3(-vertices[i].x, vertices[i].y, vertices[i].z);
            }

            mesh.vertices = vertices;
            mesh.triangles = mesh.triangles.Reverse().ToArray();
        }

        private void ReShapeModule(Mesh mesh, SubQuadCube cubeQuad)
        {
            var vertices = mesh.vertices;
            var quad = cubeQuad.subQuad;
            for (int i = 0; i < vertices.Length; ++i)
            {
                var adX = Vector3.Lerp(quad.a.currentPosition, quad.d.currentPosition, (vertices[i].x + 0.5f));
                var bcX = Vector3.Lerp(quad.b.currentPosition, quad.c.currentPosition, (vertices[i].x + 0.5f));
                vertices[i] = Vector3.Lerp(adX, bcX, (vertices[i].z + 0.5f))
                    + Vector3.up * vertices[i].y * Grid.cellHeight - quad.GetCenterPosition();
            }

            mesh.vertices = vertices;
        }

        public void UpdateModule(Module module)
        {
            var moduleFilter = this.module.GetComponent<MeshFilter>();
            moduleFilter.mesh = module.mesh;
            RotateModule(moduleFilter.mesh, module.rotation);
            FlipModule(moduleFilter.mesh, module.flip);
            ReShapeModule(moduleFilter.mesh, SubQuadCube);
            moduleFilter.mesh.RecalculateNormals();
            moduleFilter.mesh.RecalculateBounds();

            this.module.GetComponent<MeshRenderer>().material = material;
        }
    }
}