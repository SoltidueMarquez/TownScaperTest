using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Grid_Generator
{
    public class Vertex
    {
        /// <summary>
        /// 每个点在世界中的实际初始位置，为后续网格平滑做准备
        /// </summary>
        public Vector3 InitialPosition;

        /// <summary>
        /// 当前坐标
        /// </summary>
        public Vector3 currentPosition;

        /// <summary>
        /// 网格平滑偏移值
        /// </summary>
        public Vector3 offset = Vector3.zero;

        /// <summary>
        /// 细分四边形
        /// </summary>
        public List<SubQuad> subQuads = new List<SubQuad>();

        /// <summary>
        /// y坐标列表
        /// </summary>
        public List<VertexY> vertexYs = new List<VertexY>();

        public bool isBoundary;
        public int index;

        /// <summary>
        /// 松弛函数，根据偏移值与初始坐标计算当前坐标
        /// </summary>
        public void Relax()
        {
            currentPosition = InitialPosition + offset;
        }

        public void BoundaryCheck()
        {
            // 判断是否为边缘Hex
            bool isBoundaryHex = this is VertexHex && ((VertexHex)this).coord.radius == Grid.radius;
            // 判断是否为边缘Mid
            bool isBoundaryMid =
                this is VertexMid && ((VertexMid)this).edge.hexes.ToArray()[0].coord.radius == Grid.radius
                                  && ((VertexMid)this).edge.hexes.ToArray()[1].coord.radius == Grid.radius;
            isBoundary = isBoundaryHex || isBoundaryMid;
        }

        /// <summary>
        /// 创建cursor的mesh
        /// </summary>
        /// <returns></returns>
        public Mesh CreateMesh()
        {
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();

            foreach (var subQuad in subQuads)
            {
                if (this is VertexCenter)
                {
                    meshVertices.Add(currentPosition);
                    meshVertices.Add(subQuad.GetMid_cd());
                    meshVertices.Add(subQuad.GetCenterPosition());
                    meshVertices.Add(subQuad.GetMid_bc());
                }
                else if (this is VertexMid)
                {
                    if (subQuad.b == this)
                    {
                        meshVertices.Add(currentPosition);
                        meshVertices.Add(subQuad.GetMid_bc());
                        meshVertices.Add(subQuad.GetCenterPosition());
                        meshVertices.Add(subQuad.GetMid_ab());
                    }
                    else
                    {
                        meshVertices.Add(currentPosition);
                        meshVertices.Add(subQuad.GetMid_ad());
                        meshVertices.Add(subQuad.GetCenterPosition());
                        meshVertices.Add(subQuad.GetMid_cd());
                    }
                }
                else
                {
                    meshVertices.Add(currentPosition);
                    meshVertices.Add(subQuad.GetMid_ab());
                    meshVertices.Add(subQuad.GetCenterPosition());
                    meshVertices.Add(subQuad.GetMid_ad());
                }
            }

            for (int i = 0; i < meshVertices.Count; i++)
            {
                meshVertices[i] -= currentPosition;
            }

            for (int i = 0; i < subQuads.Count; i++)
            {
                meshTriangles.Add(i * 4);
                meshTriangles.Add(i * 4 + 1);
                meshTriangles.Add(i * 4 + 2);
                meshTriangles.Add(i * 4);
                meshTriangles.Add(i * 4 + 2);
                meshTriangles.Add(i * 4 + 3);
            }

            Mesh mesh = new Mesh
            {
                vertices = meshVertices.ToArray(),
                triangles = meshTriangles.ToArray()
            };
            return mesh;
        }
    }

    /// <summary>
    /// cube坐标系
    /// </summary>
    public class Coord
    {
        public readonly int q;
        public readonly int r;
        public readonly int s;
        public readonly int radius;
        public readonly Vector3 worldPosition; // 世界坐标系下的真实坐标

        public Coord(int q, int r, int s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
            this.radius = Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(s));
            worldPosition = WorldPosition();
        }

        /// <summary>
        /// 计算真实世界坐标
        /// </summary>
        /// <returns></returns>
        public Vector3 WorldPosition()
        {
            return new Vector3(q * Mathf.Sqrt(3) / 2, 0, -(float)r - ((float)q / 2)) * 2 * Grid.cellSize;
        }

        /// <summary>
        /// 顺时针的扩散方向
        /// </summary>
        public static Coord[] directions = new Coord[]
        {
            new Coord(0, 1, -1),
            new Coord(-1, 1, 0),
            new Coord(-1, 0, 1),
            new Coord(0, -1, 1),
            new Coord(1, -1, 0),
            new Coord(1, 0, -1),
        };

        public static Coord Direction(int direction)
        {
            return Coord.directions[direction];
        }

        /// <summary>
        /// 相加函数
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public Coord Add(Coord coord)
        {
            return new Coord(q + coord.q, r + coord.r, s + coord.s);
        }

        /// <summary>
        /// 根据k放大坐标,用于从中心扩散出一层坐标
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public Coord Scale(int k)
        {
            return new Coord(q * k, r * k, s * k);
        }

        /// <summary>
        /// 返回给定方向的邻居格子坐标
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Coord Neighbor(int direction)
        {
            return Add(Direction(direction));
        }

        /// <summary>
        /// 根据半径生成外环点
        /// 找到环的起始点（使用方向4缩放得到）
        /// 按照六个方向，每个方向走 radius 步；
        /// 将每一个经过的坐标加入列表中。
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static List<Coord> CoordRing(int radius)
        {
            var res = new List<Coord>();
            if (radius == 0)
            {
                res.Add(new Coord(0, 0, 0));
            }
            else // 根据Scale和Ring函数得到single ring的初始点
            {
                // 从 Direction(4).Scale(radius) 得到环的起点，然后顺时针地在每个方向走 radius 步，就会绕完整个一圈。
                var coord = Coord.Direction(4).Scale(radius);
                for (var i = 0; i < 6; i++)
                {
                    for (var j = 0; j < radius; j++)
                    {
                        res.Add(coord);
                        coord = coord.Neighbor(i);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// 创建六边形点阵坐标
        /// </summary>
        /// <returns></returns>
        public static List<Coord> CreateHex()
        {
            var res = new List<Coord>();
            for (var i = 0; i <= Grid.radius; i++)
            {
                res.AddRange(CoordRing(i));
            }

            return res;
        }
    }

    /// <summary>
    /// Vertex的子类
    /// 由于最后得到的网格除了最初的顶点
    /// 后续还会有细分得到的线段中点和四边形和三角形的中心点
    /// 因此这些点可以是 vertex 类的其他子类
    /// 便于编写后续的程序
    /// </summary>
    public class VertexHex : Vertex
    {
        public readonly Coord coord;

        public VertexHex(Coord coord)
        {
            this.coord = coord;
            InitialPosition = coord.worldPosition;
            currentPosition = InitialPosition;
        }

        /// <summary>
        /// 根据六边形点阵坐标创建六边形点阵
        /// </summary>
        /// <param name="vertices"></param>
        public static void Hex(List<VertexHex> vertices)
        {
            vertices.AddRange(Coord.CreateHex().Select(coord => new VertexHex(coord)));
        }

        /// <summary>
        /// 或许对应半径的环上的坐标点
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static List<VertexHex> GrabRing(int radius, List<VertexHex> vertices)
        {
            return radius == 0 ? vertices.GetRange(0, 1) : vertices.GetRange(radius * (radius - 1) * 3 + 1, radius * 6);
        }
        
        public List<Mesh> CreateSideMesh()
        {
            int n = this.subQuads.Count;
            List<Mesh> meshes = new List<Mesh>();
            for (int i = 0; i < n; i++)
            {
                List<Vector3> meshVertices = new List<Vector3>();
                List<int> meshTriangles = new List<int>();

                meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_ab() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_ab() + Vector3.down * Grid.cellHeight / 2);
                foreach (var subQuad in subQuads.Where(subQuad => subQuad.d == subQuads[i].b))
                {
                    meshVertices.Add(subQuad.GetCenterPosition()+Vector3.up *Grid.cellHeight / 2);
                    meshVertices.Add(subQuad.GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                    break;
                }
            

                for (int j = 0; j < meshVertices.Count; j++)
                { meshVertices[j] -= currentPosition; }

                meshTriangles.Add(0);
                meshTriangles.Add(2);
                meshTriangles.Add(1);
                meshTriangles.Add(2);
                meshTriangles.Add(3);
                meshTriangles.Add(1);
                meshTriangles.Add(2);
                meshTriangles.Add(4);
                meshTriangles.Add(5);
                meshTriangles.Add(2);
                meshTriangles.Add(5);
                meshTriangles.Add(3);

                Mesh mesh = new Mesh
                {
                    vertices = meshVertices.ToArray(),
                    triangles = meshTriangles.ToArray()
                };
                meshes.Add(mesh);
            }

            return meshes;
        }
    }

    /// <summary>
    /// Edge类的中点
    /// </summary>
    public class VertexMid : Vertex
    {
        public readonly Edge edge;

        public VertexMid(Edge edge, ICollection<VertexMid> middles)
        {
            this.edge = edge;
            var a = edge.hexes.ToArray()[0];
            var b = edge.hexes.ToArray()[1];
            middles.Add(this);
            InitialPosition = (a.InitialPosition + b.InitialPosition) / 2;
            currentPosition = InitialPosition;
        }
        
        public List<Mesh> CreateSideMesh()
        {
            List<Mesh> meshes = new List<Mesh>();
            for (int i = 0; i < 4; i++)
            {
                List<Vector3> meshVertices = new List<Vector3>();
                List<int> meshTriangles = new List<int>();

                meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                if (subQuads[i].b == this)
                {
                    meshVertices.Add(subQuads[i].GetMid_bc() + Vector3.up * Grid.cellHeight / 2);
                    meshVertices.Add(subQuads[i].GetMid_bc() + Vector3.down * Grid.cellHeight / 2);
                    foreach (var subQuad in subQuads.Where(subQuad => subQuad.c == subQuads[i].c && subQuad != subQuads[i]))
                    {
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                        break;
                    }
                }
                else
                {
                    meshVertices.Add(subQuads[i].GetMid_ad() + Vector3.up * Grid.cellHeight / 2);
                    meshVertices.Add(subQuads[i].GetMid_ad() + Vector3.down * Grid.cellHeight / 2);
                    foreach (var subQuad in subQuads.Where(subQuad => subQuad.a == subQuads[i].a && subQuad != subQuads[i]))
                    {
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                        break;
                    }
                }

                for (int j = 0; j < meshVertices.Count; j++)
                { meshVertices[j] -= currentPosition; }

                meshTriangles.Add(0);
                meshTriangles.Add(2);
                meshTriangles.Add(1);
                meshTriangles.Add(2);
                meshTriangles.Add(3);
                meshTriangles.Add(1);
                meshTriangles.Add(2);
                meshTriangles.Add(4);
                meshTriangles.Add(5);
                meshTriangles.Add(2);
                meshTriangles.Add(5);
                meshTriangles.Add(3);

                Mesh mesh = new Mesh
                {
                    vertices = meshVertices.ToArray(),
                    triangles = meshTriangles.ToArray()
                };
                meshes.Add(mesh);
            }

            return meshes;
        }
    }

    /// <summary>
    /// 多边形的中心点基类
    /// </summary>
    public class VertexCenter : Vertex
    {
        public List<Mesh> CreateSideMesh()
        {
            int n = this.subQuads.Count;
            List<Mesh> meshes = new List<Mesh>();
            for (int i = 0; i < n; i++)
            {
                List<Vector3> meshVertices = new List<Vector3>();
                List<int> meshTriangles = new List<int>();

                meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_cd() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[(i + n - 1) % n].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_cd() + Vector3.down * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[(i + n - 1) % n].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);

                for (int j = 0; j < meshVertices.Count; j++)
                { meshVertices[j] -= currentPosition; }

                meshTriangles.Add(0);
                meshTriangles.Add(1);
                meshTriangles.Add(3);
                meshTriangles.Add(1);
                meshTriangles.Add(4);
                meshTriangles.Add(3);
                meshTriangles.Add(1);
                meshTriangles.Add(2);
                meshTriangles.Add(5);
                meshTriangles.Add(1);
                meshTriangles.Add(5);
                meshTriangles.Add(4);

                Mesh mesh = new Mesh
                {
                    vertices = meshVertices.ToArray(),
                    triangles = meshTriangles.ToArray()
                };
                meshes.Add(mesh);
            }

            return meshes;
        }
    }

    /// <summary>
    /// 三角形中心点
    /// </summary>
    public class VertexTriangleCenter : VertexCenter
    {
        public VertexTriangleCenter(Triangle triangle)
        {
            InitialPosition = (triangle.a.InitialPosition + triangle.b.InitialPosition + triangle.c.InitialPosition) /
                              3;
            currentPosition = InitialPosition;
        }
    }

    /// <summary>
    /// 四边形中心点
    /// </summary>
    public class VertexQuadCenter : VertexCenter
    {
        public VertexQuadCenter(Quad quad)
        {
            InitialPosition = (quad.a.InitialPosition + quad.b.InitialPosition + quad.c.InitialPosition +
                               quad.d.InitialPosition) / 4;
            currentPosition = InitialPosition;
        }
    }

    /// <summary>
    /// 带有纵坐标的顶点
    /// </summary>
    public class VertexY
    {
        public readonly Vertex vertex;
        public readonly int y;
        public readonly string name;
        public readonly Vector3 worldPosition;
        public readonly bool isBoundary;
        public bool isActive;

        public List<SubQuadCube> subQuadCubes = new List<SubQuadCube>();

        public VertexY(Vertex vertex, int y)
        {
            this.vertex = vertex;
            this.y = y;
            this.name = $"Vertex_{vertex.index}_{y}";
            isBoundary = vertex.isBoundary || Math.Abs(y - Grid.height) < 0.01f || y == 0;
            worldPosition = vertex.currentPosition + Vector3.up * (y * Grid.cellHeight);
        }
    }
}