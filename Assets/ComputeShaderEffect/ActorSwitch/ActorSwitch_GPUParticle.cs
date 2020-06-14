using DG.Tweening;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace MaidChan
{
    [ExecuteAlways]
    public class ActorSwitch_GPUParticle : MonoBehaviour
    {
        [SerializeField]
        private Mesh instanceMesh = null;

        private SkinnedMeshRenderer[] skinMeshs;

        [SerializeField]
        private Volume postProcessVolume = null;

        [SerializeField]
        private Material material = null;

        [SerializeField]
        private ComputeShader computeShader = null;

        public float thresholdY1 = 0;
        public float thresholdY2 = 0;
        public float thresholdY3 = 0;

        [ColorUsage(true, true)]
        public Color color;

        private int subMeshIndex = 0;
        private const int WARP_SIZE = 1024;

        private int instanceCount;
        private int computeUpdate_KernelIndex;
        private int computeShaderExecuteTimes;

        private float maxY = float.MinValue;
        private float minY = float.MaxValue;

        private ComputeBuffer computeBuffer;
        private ComputeBuffer argsBuffer;

        public Animator animator;
        public Transform actor;
        public Texture2D _colorTex;

        public struct Particle
        {
            public Vector3 position;
            public float scale;
            public float height;
            public Color color;
            public Color color2;
            public float translate;
        }


        public Transform obj;
        private void Update()
        {
            Shader.SetGlobalFloat("_threshold", obj.position.y);
            Shader.SetGlobalInt("_cull", 1);
        }

        private void Start()
        {
            InitGPUParticle();
            Shader.SetGlobalInt("_cull", 0);
        }

        public void InitGPUParticle()
        {
            skinMeshs = actor.GetComponentsInChildren<SkinnedMeshRenderer>();

            instanceCount = 0;
            foreach (var item in skinMeshs)
            {
                instanceCount += item.sharedMesh.vertices.Length;
            }

            InitArgsBuffer();
        }

        public void ShowActor()
        {
            if (computeBuffer != null)
                return;

            StartCoroutine(ShowMaid());
        }

        public void HideActor()
        {
            if (computeBuffer != null)
                return;

            StartCoroutine(HideMaid());
        }

        private IEnumerator HideMaid()
        {
            animator.speed = 0;
            InitComputeBuffer();

            Shader.SetGlobalInt("_cull", 1);
            DOTween.To(() => postProcessVolume.weight, x => postProcessVolume.weight = x, 1, 0.2f);

            actor.gameObject.SetActive(true);
            bool complete = false;
            computeShader.SetVector("fcolor", new Vector4(color.r, color.g, color.b, color.a));
            thresholdY1 = maxY;
            thresholdY2 = maxY;
            thresholdY3 = maxY;
            DOTween.To(() => thresholdY1, x => thresholdY1 = x, minY, 1.0f).OnComplete(() =>
            {
                actor.gameObject.SetActive(false);
            });
            DOTween.To(() => thresholdY2, x => thresholdY2 = x, minY, 1.0f).SetDelay(0.2f);
            DOTween.To(() => thresholdY3, x => thresholdY3 = x, minY, 2).SetDelay(0.3f).OnComplete(() =>
            {
                complete = true;
            });
            while (complete == false)
            {
                Shader.SetGlobalFloat("_threshold", thresholdY1);
                computeShader.SetFloat("deltaTime", Time.deltaTime);
                computeShader.SetFloat("threshold1", thresholdY1);
                computeShader.SetFloat("threshold2", thresholdY2);
                computeShader.SetFloat("threshold3", thresholdY3);

                computeShader.Dispatch(computeUpdate_KernelIndex, computeShaderExecuteTimes, 1, 1);
                Graphics.DrawMeshInstancedIndirect(
                    instanceMesh,
                    subMeshIndex,
                    material,
                    new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
                    argsBuffer);

                yield return null;
            }

            DOTween.To(() => postProcessVolume.weight, x => postProcessVolume.weight = x, 0, 0.3f);
            Shader.SetGlobalInt("_cull", 0);

            computeBuffer.Release();
            computeBuffer = null;
            Debug.LogWarning("Compelete");
        }

        private IEnumerator ShowMaid()
        {
            animator.speed = 0;
            InitComputeBuffer();

            Shader.SetGlobalInt("_cull", 1);
            DOTween.To(() => postProcessVolume.weight, x => postProcessVolume.weight = x, 1, 0.2f);

            bool complete = false;
            computeShader.SetVector("fcolor", new Vector4(color.r, color.g, color.b, color.a));
            thresholdY1 = minY;
            thresholdY2 = minY;
            thresholdY3 = minY;

            DOTween.To(() => thresholdY3, x => thresholdY3 = x, maxY, 2);
            DOTween.To(() => thresholdY2, x => thresholdY2 = x, maxY, 1.0f).SetDelay(0.7f).OnStart(() =>
            {
                actor.gameObject.SetActive(true);
            });
            DOTween.To(() => thresholdY1, x => thresholdY1 = x, maxY, 1.0f).SetDelay(0.9f).OnComplete(() =>
            {
                complete = true;
            });

            while (complete == false)
            {
                Shader.SetGlobalFloat("_threshold", thresholdY1);
                computeShader.SetFloat("deltaTime", Time.deltaTime);
                computeShader.SetFloat("threshold1", thresholdY1);
                computeShader.SetFloat("threshold2", thresholdY2);
                computeShader.SetFloat("threshold3", thresholdY3);

                computeShader.Dispatch(computeUpdate_KernelIndex, computeShaderExecuteTimes, 1, 1);
                Graphics.DrawMeshInstancedIndirect(
                    instanceMesh,
                    subMeshIndex,
                    material,
                    new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
                    argsBuffer);

                yield return null;
            }

            DOTween.To(() => postProcessVolume.weight, x => postProcessVolume.weight = x, 0, 0.3f);
            Shader.SetGlobalInt("_cull", 0);

            animator.speed = 1;
            computeBuffer.Release();
            computeBuffer = null;
        }

        //argsBuffer 用來突破GPU Instance 1024個的限制 運作原理還不清楚?????
        private void InitArgsBuffer()
        {
            if (argsBuffer != null)
            {
                argsBuffer.Release();
            }

            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            // Indirect args
            if (instanceMesh != null)
            {
                args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex); ;
                args[1] = (uint)instanceCount;
                args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
                args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
            }
            else
            {
                args[0] = args[1] = args[2] = args[3] = 0;
            }
            argsBuffer.SetData(args);
        }

        private void InitComputeBuffer()
        {
            maxY = float.MinValue;
            minY = float.MaxValue;

            Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
            {
                Vector3 dir = point - pivot; // get point direction relative to pivot
                dir = Quaternion.Euler(angles) * dir; // rotate it
                point = dir + pivot; // calculate rotated point
                return point; // return it
            }

            computeShaderExecuteTimes = Mathf.CeilToInt((float)instanceCount / WARP_SIZE);
            int stride = Marshal.SizeOf(typeof(Particle));

            if (computeBuffer != null)
            {
                computeBuffer.Release();
            }
            computeBuffer = new ComputeBuffer(instanceCount, stride);

            //初始化頂點資料
            Particle[] positions = new Particle[instanceCount];
            int index = 0;
            foreach (var item in skinMeshs)
            {
                var bakedMesh = new Mesh();
                item.BakeMesh(bakedMesh);

                var translate = transform.TransformPoint(item.transform.position);
                var rota = item.transform.rotation.eulerAngles;
                for (int i = 0; i < bakedMesh.vertices.Length; i++)
                {
                    var pointColor = _colorTex.GetPixelBilinear(bakedMesh.uv[i].x, bakedMesh.uv[i].y);

                    positions[index].color = pointColor;
                    positions[index].color2 = pointColor;
                    positions[index].position = RotatePointAroundPivot(bakedMesh.vertices[i], Vector3.zero, rota) + translate;
                    positions[index].scale = UnityEngine.Random.Range(0.04f, 0.08f);
                    positions[index].height = positions[index].scale;
                    positions[index].translate = 0;

                    var curY = positions[index].position.y;
                    if (curY > maxY)
                    {
                        maxY = curY;
                    }
                    if (curY < minY)
                    {
                        minY = curY;
                    }
                    index++;
                }
            }
            minY -= 1;
            maxY += 1;

            computeBuffer.SetData(positions);
            computeUpdate_KernelIndex = computeShader.FindKernel("Update");

            //let comuteshader can use buffer data
            computeShader.SetBuffer(computeUpdate_KernelIndex, "Particles", computeBuffer);
            //let vertex shader can use buffer data
            material.SetBuffer("Particles", computeBuffer);
        }

        private void OnDisable()
        {
            if (computeBuffer != null)
            {
                computeBuffer.Release();
            }
            computeBuffer = null;

            if (argsBuffer != null)
            {
                argsBuffer.Release();
            }
            argsBuffer = null;
        }
    }
}