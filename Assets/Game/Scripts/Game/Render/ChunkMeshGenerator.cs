using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Island.Game.Data.Blocks;
using Island.Game.System;
using Island.Game.World;
using UnityEngine;

namespace Island.Game.Render
{
    /// <summary>
    /// 区块网格生成器
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(ChunkContainer))]
    public class ChunkMeshGenerator : MonoBehaviour
    {
        public bool physicsReady;

        private static int _generatingMeshTaskCount;

        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        private readonly List<int> _triangles = new List<int>();

        private ChunkContainer _chunkContainer;
        private MeshFilter _meshFilter;
        private Renderer _meshRenderer;
        private MeshCollider _meshCollider;

        private Mesh _mesh;
        private bool _isGeneratingMesh;

        void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _chunkContainer = GetComponent<ChunkContainer>();

            _mesh = new Mesh();
        }

        void Start()
        {
            _meshRenderer.material = GameManager.BlockTextureManager.BlocksMaterial;
        }

        void OnEnable()
        {
            _meshRenderer.enabled = true;
        }

        void OnDisable()
        {
            _meshRenderer.enabled = false;
        }

        public void Unload()
        {
            StopAllCoroutines();
            if (_isGeneratingMesh)
                --_generatingMeshTaskCount;
            physicsReady = false;
            _mesh.Clear();
            if (_meshCollider == null)
                _meshCollider = GetComponent<MeshCollider>();

            if (_meshCollider != null)
                _meshCollider.sharedMesh = _mesh;
        }

        private Vector3 ScaleByBlockSize(Vector3 vector)
        {
            vector.x *= _chunkContainer.BlockSize.x;
            vector.y *= _chunkContainer.BlockSize.y;
            vector.z *= _chunkContainer.BlockSize.z;

            return vector;
        }

        IEnumerator MeshRefreshCoroutine()
        {
            _isGeneratingMesh = true;
            ++_generatingMeshTaskCount;
            _meshRenderer.enabled = false;
            physicsReady = false;

            _vertices.Clear();
            _normals.Clear();
            _uvs.Clear();
            _triangles.Clear();

            for (var x = 0; x < _chunkContainer.ChunkSize.x; ++x)
            {
                for (var y = 0; y < _chunkContainer.ChunkSize.y; ++y)
                    for (var z = 0; z < _chunkContainer.ChunkSize.z; ++z)
                    {

                        var blockData = _chunkContainer.GetBlockData(x, y, z);

                        var needDraw = !blockData.IsAlpha && (
                                           _chunkContainer.GetBlockData(x, y + 1, z).IsAlpha ||
                                           _chunkContainer.GetBlockData(x, y - 1, z).IsAlpha ||
                                           _chunkContainer.GetBlockData(x + 1, y, z).IsAlpha ||
                                           _chunkContainer.GetBlockData(x - 1, y, z).IsAlpha ||
                                           _chunkContainer.GetBlockData(x, y, z + 1).IsAlpha ||
                                           _chunkContainer.GetBlockData(x, y, z - 1).IsAlpha);

                        if (!needDraw)
                            continue;

                        var triangleStartPos = _vertices.Count;

                        if (_chunkContainer.GetBlockData(x, y + 1, z).IsAlpha)
                        {
                            //上面
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y, z + 1)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y, z + 1)));

                            _normals.Add(Vector3.up);
                            _normals.Add(Vector3.up);
                            _normals.Add(Vector3.up);
                            _normals.Add(Vector3.up);

                            var textureUp = blockData.GetFaceTexture(Face.Up);

                            _uvs.Add(new Vector2(textureUp.right, textureUp.top));
                            _uvs.Add(new Vector2(textureUp.left, textureUp.top));
                            _uvs.Add(new Vector2(textureUp.left, textureUp.bottom));
                            _uvs.Add(new Vector2(textureUp.right, textureUp.bottom));

                            _triangles.Add(triangleStartPos + 0);
                            _triangles.Add(triangleStartPos + 1);
                            _triangles.Add(triangleStartPos + 2);
                            _triangles.Add(triangleStartPos + 2);
                            _triangles.Add(triangleStartPos + 3);
                            _triangles.Add(triangleStartPos + 0);
                            triangleStartPos += 4;
                        }

                        if (_chunkContainer.GetBlockData(x, y - 1, z).IsAlpha)
                        {
                            //下面

                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y - 1, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y - 1, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y - 1, z + 1)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y - 1, z + 1)));

                            _normals.Add(Vector3.down);
                            _normals.Add(Vector3.down);
                            _normals.Add(Vector3.down);
                            _normals.Add(Vector3.down);

                            var textureDown = blockData.GetFaceTexture(Face.Down);

                            _uvs.Add(new Vector2(textureDown.right, textureDown.top));
                            _uvs.Add(new Vector2(textureDown.left, textureDown.top));
                            _uvs.Add(new Vector2(textureDown.left, textureDown.bottom));
                            _uvs.Add(new Vector2(textureDown.right, textureDown.bottom));

                            _triangles.Add(triangleStartPos + 2);
                            _triangles.Add(triangleStartPos + 1);
                            _triangles.Add(triangleStartPos + 0);
                            _triangles.Add(triangleStartPos + 0);
                            _triangles.Add(triangleStartPos + 3);
                            _triangles.Add(triangleStartPos + 2);

                            triangleStartPos += 4;
                        }

                        if (_chunkContainer.GetBlockData(x, y, z + 1).IsAlpha)
                        {
                            //前面
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y - 1, z + 1)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y - 1, z + 1)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y, z + 1)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y, z + 1)));

                            _normals.Add(Vector3.forward);
                            _normals.Add(Vector3.forward);
                            _normals.Add(Vector3.forward);
                            _normals.Add(Vector3.forward);

                            var textureBack = blockData.GetFaceTexture(Face.Forward);

                            _uvs.Add(new Vector2(textureBack.right, textureBack.top));
                            _uvs.Add(new Vector2(textureBack.left, textureBack.top));
                            _uvs.Add(new Vector2(textureBack.left, textureBack.bottom));
                            _uvs.Add(new Vector2(textureBack.right, textureBack.bottom));

                            _triangles.Add(triangleStartPos + 2);
                            _triangles.Add(triangleStartPos + 1);
                            _triangles.Add(triangleStartPos + 0);
                            _triangles.Add(triangleStartPos + 0);
                            _triangles.Add(triangleStartPos + 3);
                            _triangles.Add(triangleStartPos + 2);

                            triangleStartPos += 4;
                        }

                        if (_chunkContainer.GetBlockData(x, y, z - 1).IsAlpha)
                        {
                            //后面
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y - 1, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y - 1, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y, z)));

                            _normals.Add(Vector3.back);
                            _normals.Add(Vector3.back);
                            _normals.Add(Vector3.back);
                            _normals.Add(Vector3.back);

                            var textureForward = blockData.GetFaceTexture(Face.Back);

                            _uvs.Add(new Vector2(textureForward.right, textureForward.top));
                            _uvs.Add(new Vector2(textureForward.left, textureForward.top));
                            _uvs.Add(new Vector2(textureForward.left, textureForward.bottom));
                            _uvs.Add(new Vector2(textureForward.right, textureForward.bottom));

                            _triangles.Add(triangleStartPos + 0);
                            _triangles.Add(triangleStartPos + 1);
                            _triangles.Add(triangleStartPos + 2);
                            _triangles.Add(triangleStartPos + 2);
                            _triangles.Add(triangleStartPos + 3);
                            _triangles.Add(triangleStartPos + 0);

                            triangleStartPos += 4;
                        }

                        if (_chunkContainer.GetBlockData(x - 1, y, z).IsAlpha)
                        {
                            //左面
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y - 1, z + 1)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y - 1, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x, y, z + 1)));

                            _normals.Add(Vector3.left);
                            _normals.Add(Vector3.left);
                            _normals.Add(Vector3.left);
                            _normals.Add(Vector3.left);

                            var textureLeft = blockData.GetFaceTexture(Face.Left);

                            _uvs.Add(new Vector2(textureLeft.right, textureLeft.top));
                            _uvs.Add(new Vector2(textureLeft.left, textureLeft.top));
                            _uvs.Add(new Vector2(textureLeft.left, textureLeft.bottom));
                            _uvs.Add(new Vector2(textureLeft.right, textureLeft.bottom));

                            _triangles.Add(triangleStartPos + 2);
                            _triangles.Add(triangleStartPos + 1);
                            _triangles.Add(triangleStartPos + 0);
                            _triangles.Add(triangleStartPos + 0);
                            _triangles.Add(triangleStartPos + 3);
                            _triangles.Add(triangleStartPos + 2);

                            triangleStartPos += 4;
                        }

                        if (_chunkContainer.GetBlockData(x + 1, y, z).IsAlpha)
                        {
                            //右面
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y - 1, z + 1)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y - 1, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y, z)));
                            _vertices.Add(ScaleByBlockSize(new Vector3(x + 1, y, z + 1)));

                            _normals.Add(Vector3.right);
                            _normals.Add(Vector3.right);
                            _normals.Add(Vector3.right);
                            _normals.Add(Vector3.right);

                            var textureRight = blockData.GetFaceTexture(Face.Right);

                            _uvs.Add(new Vector2(textureRight.right, textureRight.top));
                            _uvs.Add(new Vector2(textureRight.left, textureRight.top));
                            _uvs.Add(new Vector2(textureRight.left, textureRight.bottom));
                            _uvs.Add(new Vector2(textureRight.right, textureRight.bottom));

                            _triangles.Add(triangleStartPos + 0);
                            _triangles.Add(triangleStartPos + 1);
                            _triangles.Add(triangleStartPos + 2);
                            _triangles.Add(triangleStartPos + 2);
                            _triangles.Add(triangleStartPos + 3);
                            _triangles.Add(triangleStartPos + 0);
                        }
                    }

                yield return 1;
            }

            _mesh.SetVertices(_vertices);
            yield return 1;
            _mesh.SetNormals(_normals);
            yield return 1;
            _mesh.SetUVs(0, _uvs);
            yield return 1;
            _mesh.SetTriangles(_triangles, 0);
            yield return 1;
            /*_mesh.vertices = _vertices.ToArray();
            _mesh.normals = _normals.ToArray();
            _mesh.uv = _uvs.ToArray();
            _mesh.triangles = _triangles.ToArray();*/

            _meshFilter.mesh = _mesh;

            yield return 1;

            if (_meshCollider == null)
                _meshCollider = GetComponent<MeshCollider>();

            _meshCollider.cookingOptions = MeshColliderCookingOptions.None;

            if (_meshCollider != null)
                _meshCollider.sharedMesh = _mesh;

            _meshRenderer.enabled = true;
            --_generatingMeshTaskCount;
            _isGeneratingMesh = false;
            physicsReady = true;
        }

        public bool TryRefresh()
        {
            // 没有mesh生成任务或者游戏初始化时可以进行区块刷新
            if (_generatingMeshTaskCount < 1 || 
                (GameManager.IsInitializing && _generatingMeshTaskCount < 5))
            {
                StartCoroutine(MeshRefreshCoroutine());
                return true;
            }

            return false;
        }
    }
}
