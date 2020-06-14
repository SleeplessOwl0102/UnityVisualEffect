using UnityEngine;
using System.Collections.Generic;

namespace Smrvfx
{
    [ExecuteAlways]
    public sealed class SkinnedMeshBaker : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] SkinnedMeshRenderer[] _source = null;
        [SerializeField] ComputeShader _compute = null;

        #endregion

        #region Public properties

        public Texture PositionMap => _positionMap;
        public Texture VelocityMap => _velocityMap;
        public Texture NormalMap => _normalMap;

        public int VertexCount => _mesh != null ? AllpositionList.Count: 0;
        #endregion

        #region Temporary objects

        List<Mesh> _mesh;
        Matrix4x4 _previousTransform = Matrix4x4.identity;

        List<Vector3> _positionList = new List<Vector3>();
        List<Vector3> AllpositionList = new List<Vector3>();

        ComputeBuffer _positionBuffer1;
        ComputeBuffer _positionBuffer2;
        ComputeBuffer _normalBuffer;

        RenderTexture _positionMap;
        RenderTexture _velocityMap;
        RenderTexture _normalMap;

        #endregion

        #region MonoBehaviour implementation

        void OnEnable()
        {
            _mesh = new List<Mesh>();
            int vcount = 0;
            for (int i = 0; i< _source.Length;i++)
            {
                _mesh.Add(new Mesh());
                _source[i].BakeMesh(_mesh[i]);
                vcount += _mesh[i].vertexCount;
            }

            var vcount_x3 = vcount * 3;

            _positionBuffer1 = new ComputeBuffer(vcount_x3, sizeof(float));
            _positionBuffer2 = new ComputeBuffer(vcount_x3, sizeof(float));
            _normalBuffer = new ComputeBuffer(vcount_x3, sizeof(float));

            var width = 256;
            var height = (((vcount + width - 1) / width + 7) / 8) * 8;

            _positionMap = NewFloatRenderTexture(width, height);
            _velocityMap = NewHalfRenderTexture(width, height);
            _normalMap = NewHalfRenderTexture(width, height);
        }

        void OnDestroy()
        {
            for (int i = 0; i < _mesh.Count; i++)
            {
                if (Application.isPlaying)
                    Destroy(_mesh[i]);
                else
                    DestroyImmediate(_mesh[i]);
            }
            

            _positionBuffer1.Dispose();
            _positionBuffer2.Dispose();
            _normalBuffer.Dispose();

            if (Application.isPlaying)
                Destroy(_positionMap);
            else
                DestroyImmediate(_positionMap);
            
        }

        void Update()
        {
            int vcount = 0;
            for (int i = 0; i < _source.Length; i++)
            {
                //_mesh.Add(new Mesh());
                _source[i].BakeMesh(_mesh[i]);
                vcount += _mesh[i].vertexCount;
            }
            AllpositionList.Clear();
            for (int i = 0; i < _source.Length; i++)
            {
                _mesh[i].GetVertices(_positionList);
                AllpositionList.AddRange(_positionList);
            }
            

            TransferData();
            SwapPositionBuffers();

            _previousTransform = _source[0].transform.localToWorldMatrix;
        }

        #endregion

        #region Render texture utilities

        static RenderTexture NewRenderTexture
          (int width, int height, RenderTextureFormat format)
        {
            var rt = new RenderTexture(width, height, 0, format);
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        static RenderTexture NewHalfRenderTexture(int width, int height)
          => NewRenderTexture(width, height, RenderTextureFormat.ARGBHalf);

        static RenderTexture NewFloatRenderTexture(int width, int height)
          => NewRenderTexture(width, height, RenderTextureFormat.ARGBFloat);

        #endregion

        #region Buffer operations

        void TransferData()
        {
            var width = _positionMap.width;
            var height = _positionMap.height;

            var vcount = AllpositionList.Count;
            var vcount_x3 = vcount * 3;

            var l2w = _source[0].transform.localToWorldMatrix;

            _compute.SetInt("VertexCount", vcount);
            _compute.SetMatrix("Transform", l2w);
            _compute.SetFloat("FrameRate", 1 / Time.deltaTime);

            _positionBuffer1.SetData(AllpositionList);

            _compute.SetBuffer(0, "PositionBuffer", _positionBuffer1);

            _compute.SetTexture(0, "PositionMap", _positionMap);
            _compute.Dispatch(0, width / 8, height / 8, 1);
        }

        void SwapPositionBuffers()
        {
            var temp = _positionBuffer1;
            _positionBuffer1 = _positionBuffer2;
            _positionBuffer2 = temp;
        }

        #endregion
    }
}
