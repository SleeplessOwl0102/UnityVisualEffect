namespace Lyx.VisualEffect
{
    using System;
    using UnityEngine;
    using UnityEngine.Assertions;

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PlexusEffect : MonoBehaviour
    {
        public readonly string kernelName = "Main";
        public readonly int id_time = Shader.PropertyToID("_Time");
        public readonly int id_speed = Shader.PropertyToID("_Speed");
        public readonly int id_maxOffset = Shader.PropertyToID("_MaxOffset");
        public readonly int id_positions = Shader.PropertyToID("_Positions");
        public readonly int id_preOffset = Shader.PropertyToID("_PreOffset");
        public readonly int id_dimensions = Shader.PropertyToID("_Dimensions");

        public Vector3Int dimensions = Vector3Int.zero;
        public ComputeShader compute = null;
        public Material material = null;

        public float speed = .01f;
        public float maxOffset = 4;

        private ComputeBuffer positionsBuffer;
        private ComputeBuffer preOffsetBuffer;
        private int kernel;
        private int[] groups;

        #region Lifetime

        private void Start()
        {
            PrepareCompute();
            CreateMesh();
            StoreThreadGroupCount();
        }

        private void Update()
        {
            UpdateComputeParameters();
            compute.Dispatch(kernel, groups[0], groups[1], groups[2]);
        }

        private void UpdateComputeParameters()
        {
            compute.SetFloat(id_time, Time.time);
            compute.SetFloat(id_speed, speed);
            compute.SetFloat(id_maxOffset, maxOffset);
        }

        private void OnDestroy()
        {
            positionsBuffer.Dispose();
            preOffsetBuffer.Dispose();
        }

        #endregion

        public int GridCellCount(Vector3Int grid)
        {
            return grid.x * grid.y * grid.z;
        }
        #region Initialization
        private void InitializeBuffers()
        {
            int count = GridCellCount(dimensions);
            positionsBuffer = new ComputeBuffer(count, 3 * sizeof(float));
            preOffsetBuffer = new ComputeBuffer(count, 3 * sizeof(float));
            compute.SetBuffer(kernel, id_positions, positionsBuffer);
            compute.SetBuffer(kernel, id_preOffset, preOffsetBuffer);
        }

        private void SetShaderConstants()
        {
            compute.SetInts(id_dimensions, dimensions.x, dimensions.y, dimensions.z);
            material.SetBuffer(id_positions, positionsBuffer);
            material.SetVector(id_dimensions, new Vector4(dimensions.x, dimensions.y, dimensions.z, 0));
        }

        private void CreateMesh()
        {
            int count = GridCellCount(dimensions);
            var vertices = new Vector3[count];
            var indices = new int[count];
            PopulateMeshData(vertices, indices);
            StoreMeshData(vertices, indices);
        }

        private void PopulateMeshData(Vector3[] vertices, int[] indices)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int z = 0; z < dimensions.z; z++)
                    {
                        int index = x * dimensions.z * dimensions.y + y * dimensions.z + z;
                        vertices[index] = new Vector3(x, y, z);
                        indices[index] = index;
                    }
                }
            }
        }

        private void StoreMeshData(Vector3[] vertices, int[] indices)
        {
            var mesh = new Mesh();
            mesh.name = "Grid";
            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Points, submesh: 0);
            Bounds bounds = mesh.bounds;
            bounds.size *= 2f;
            mesh.bounds = bounds;
            var filter = GetComponent<MeshFilter>();
            filter.mesh = mesh;
        }

        private void PrepareCompute()
        {
            kernel = compute.FindKernel(kernelName);
            InitializeBuffers();
            SetShaderConstants();
        }

        private void CheckForValidDimensions(int[] sizes)
        {
            var remainders = new Vector3Int(
                dimensions.x % sizes[0],
                dimensions.y % sizes[1],
                dimensions.z % sizes[2]
            );
            bool isValid = remainders.x == 0 && remainders.y == 0 && remainders.z == 0;
            Assert.IsTrue(isValid, "Dimensions must be divisible by the compute shader thread group size.");
        }

        private void StoreThreadGroupCount()
        {
            var uSizes = new uint[3];
            compute.GetKernelThreadGroupSizes(kernel, out uSizes[0], out uSizes[1], out uSizes[2]);
            int[] sizes = Array.ConvertAll(uSizes, size => (int)size);
            CheckForValidDimensions(sizes);
            groups = new int[]
            {
                dimensions.x / sizes[0],
                dimensions.y / sizes[1],
                dimensions.z / sizes[2]
            };
        }

        #endregion
    }
}