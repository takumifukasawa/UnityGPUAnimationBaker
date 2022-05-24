using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace GPUAnimationBaker
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorBaker : MonoBehaviour
    {
        [SerializeField]
        private ComputeShader _bakerComputeShader;

        [SerializeField]
        private Shader _runtimeShader;

        [SerializeField]
        private int _animationFps = 20;

        [SerializeField]
        private int _uvChannel = 1;


        // private struct VertexAttributes
        // {
        //     public Vector3 position;
        //     public Vector3 normal;
        //     public Vector3 tangent;
        // }

        public IEnumerator Generate()
        {
            Debug.Log("===== AnimatorBaker Begin Generate ... =====");
            Animator animator = GetComponent<Animator>();
            AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
            SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

            int vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;
            int textureWidth = Mathf.NextPowerOfTwo(vertexCount);

            // Mesh mesh = new Mesh();

            animator.speed = 0;

            int totalFrames = 0;
            float totalDuration = 0;

            VertexAttributesBaker _baker = new VertexAttributesBaker(skinnedMeshRenderer);

            foreach (AnimationClip animationClip in animationClips)
            {
                // int frames = Mathf.NextPowerOfTwo((int)(animationClip.length / (1f / (float)_animationFps)));
                // int currentAnimationClipFrames = Mathf.NextPowerOfTwo((int)(animationClip.length / (1f / (float)_animationFps)));
                int currentAnimationClipFrames = (int)(animationClip.length / (1f / (float)_animationFps));
                // List<VertexAttributes> vertexAttributesList = new List<VertexAttributes>();

                totalFrames += currentAnimationClipFrames;
                totalDuration += animationClip.length;

                // totalFrames = Mathf.NextPowerOfTwo(currentAnimationClipFrames);

                Debug.Log(string.Format(
                    "[AnimatorBaker] Generate - animationClip - name: {0}, duration: {1}, frames: {2}",
                    animationClip.name,
                    animationClip.length,
                    currentAnimationClipFrames
                ));

                _baker.MemoryAnimationFrame(animationClip.name, currentAnimationClipFrames);

                // VertexAttributesBaker _baker = new VertexAttributesBaker(textureWidth, frames, skinnedMeshRenderer);

                // RenderTexture bakedPositionRenderTexture = new RenderTexture(textureWidth, frames, 0, RenderTextureFormat.ARGBHalf);
                // RenderTexture bakedNormalRenderTexture = new RenderTexture(textureWidth, frames, 0, RenderTextureFormat.ARGBHalf);
                // RenderTexture bakedTangentRenderTexture = new RenderTexture(textureWidth, frames, 0, RenderTextureFormat.ARGBHalf);

                // foreach (RenderTexture renderTexture in new[] { bakedPositionRenderTexture, bakedNormalRenderTexture, bakedTangentRenderTexture })
                // {
                //     renderTexture.enableRandomWrite = true;
                //     renderTexture.Create();
                //     RenderTexture.active = renderTexture;
                //     GL.Clear(true, true, Color.clear);
                // }

                animator.Play(animationClip.name);

                yield return 0;

                for (int i = 0; i < currentAnimationClipFrames; i++)
                {
                    animator.Play(animationClip.name, 0, (float)i / currentAnimationClipFrames);
                    yield return 0;
                    // skinnedMeshRenderer.BakeMesh(mesh);
                    for (int j = 0; j < vertexCount; j++)
                    {
                        int vertexIndex = j;
                        _baker.MemoryVertexAttributes(vertexIndex);
                        // VertexAttributes vertexAttributes = new VertexAttributes()
                        // {
                        //     position = mesh.vertices[vertexIndex],
                        //     normal = mesh.normals[vertexIndex],
                        //     tangent = mesh.tangents[vertexIndex]
                        // };
                        // // vertexAttributesList.Add(vertexAttributes);
                        // _baker.AddVertexAttributes(vertexAttributes);
                    }
                }

                // #if UNITY_EDITOR
                //                 _baker.Bake(_bakerComputeShader, frames, _uvChannel);
                //                 _baker.SaveAssets(gameObject.name, _runtimeShader, animationClip.length);
                //                 _baker.Dispose();
                // #endif

                // GraphicsBuffer graphicsBuffer = new GraphicsBuffer(
                //     GraphicsBuffer.Target.Structured,
                //     vertexAttributesList.Count * 3,
                //     Marshal.SizeOf(new Vector3())
                // );
                // graphicsBuffer.SetData(vertexAttributesList.ToArray());

                // int kernel = _bakerComputeShader.FindKernel("CSMain");
                // uint x, y, z;
                // _bakerComputeShader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);

                // _bakerComputeShader.SetInt("VertexCount", vertexCount);
                // _bakerComputeShader.SetBuffer(kernel, "InputData", graphicsBuffer);
                // _bakerComputeShader.SetTexture(kernel, "OutPosition", bakedPositionRenderTexture);
                // _bakerComputeShader.SetTexture(kernel, "OutNormal", bakedNormalRenderTexture);
                // _bakerComputeShader.SetTexture(kernel, "OutTangent", bakedTangentRenderTexture);
                // _bakerComputeShader.Dispatch(
                //     kernel,
                //     vertexCount / (int)x + 1,
                //     frames / (int)y + 1,
                //     1
                // );

                // graphicsBuffer.Release();

                // string folderName = "BakedAnimationTex";
                // string folderPath = Path.Combine("Assets", folderName);
                // if (!AssetDatabase.IsValidFolder(folderPath))
                // {
                //     AssetDatabase.CreateFolder("Assets", folderName);
                // }
                // string subFolder = name;
                // string subFolderPath = Path.Combine(folderPath, subFolder);
                // if (!AssetDatabase.IsValidFolder(subFolderPath))
                // {
                //     AssetDatabase.CreateFolder(folderPath, subFolder);
                // }

                // Texture2D bakedPositionMap = ConvertRenderTextureToTexture2D(bakedPositionRenderTexture);
                // Texture2D bakedNormalMap = ConvertRenderTextureToTexture2D(bakedNormalRenderTexture);
                // Texture2D bakedTangentMap = ConvertRenderTextureToTexture2D(bakedTangentRenderTexture);

                // Graphics.CopyTexture(bakedPositionRenderTexture, bakedPositionMap);
                // Graphics.CopyTexture(bakedNormalRenderTexture, bakedNormalMap);
                // Graphics.CopyTexture(bakedTangentRenderTexture, bakedTangentMap);

                // GameObject staticMeshGameObject = new GameObject(name + "." + animationClip.name);
                // // staticMeshGameObject.AddComponent<MeshFilter>().sharedMesh = skinnedMeshRenderer.sharedMesh;
                // Mesh newMesh = CreateMeshForGPUAnimation(skinnedMeshRenderer.sharedMesh);
                // staticMeshGameObject.AddComponent<MeshFilter>().sharedMesh = newMesh;

                // Material runtimeMat = new Material(_runtimeShader);
                // runtimeMat.SetTexture("_BakedPositionMap", bakedPositionMap);
                // runtimeMat.SetTexture("_BakedNormalMap", bakedNormalMap);
                // runtimeMat.SetTexture("_BakedTangentMap", bakedTangentMap);
                // runtimeMat.SetFloat("_BakedAnimationDuration", animationClip.length);

                // staticMeshGameObject.AddComponent<MeshRenderer>().sharedMaterial = runtimeMat;

                // GPUAnimationController gpuAnimationController = staticMeshGameObject.AddComponent<GPUAnimationController>();
                // GPUAnimationDataScriptableObject gpuAnimationData = ScriptableObject.CreateInstance<GPUAnimationDataScriptableObject>();
                // gpuAnimationData.TotalDuration = animationClip.length;
                // gpuAnimationController.Init(gpuAnimationData);

                // AssetDatabase.CreateAsset(
                //     bakedPositionMap,
                //     Path.Combine(
                //         subFolderPath,
                //         string.Format("{0}.{1}.BakedPositionMap.asset", gameObject.name, animationClip.name)
                //     )
                // );
                // AssetDatabase.CreateAsset(
                //     bakedNormalMap,
                //     Path.Combine(
                //         subFolderPath,
                //         string.Format("{0}.{1}.BakedNormalMap.asset", gameObject.name, animationClip.name)
                //     )
                // );
                // AssetDatabase.CreateAsset(
                //     bakedTangentMap,
                //     Path.Combine(
                //         subFolderPath,
                //         string.Format("{0}.{1}.BakedTangentMap.asset", gameObject.name, animationClip.name)
                //     )
                // );
                // AssetDatabase.CreateAsset(
                //     newMesh,
                //     Path.Combine(
                //         subFolderPath,
                //         string.Format("{0}.{1}.Mesh.asset", gameObject.name, animationClip.name)
                //     )
                // );
                // AssetDatabase.CreateAsset(
                //     runtimeMat,
                //     Path.Combine(
                //         subFolderPath,
                //         string.Format("{0}.{1}.Material.asset", gameObject.name, animationClip.name)
                //     )
                // );
                // AssetDatabase.CreateAsset(
                //     gpuAnimationData,
                //     Path.Combine(
                //         subFolderPath,
                //         string.Format("{0}.{1}.GPUAnimationData.asset", gameObject.name, animationClip.name)
                //     )
                // );
                // PrefabUtility.SaveAsPrefabAsset(
                //     staticMeshGameObject,
                //     Path.Combine(folderPath, staticMeshGameObject.name + ".prefab").Replace("\\", "/")
                // );

                // AssetDatabase.SaveAssets();
                // AssetDatabase.Refresh();

            } // end foreach

#if UNITY_EDITOR
            _baker.Bake(
                // textureWidth,
                // Mathf.NextPowerOfTwo(totalFrames),
                // Mathf.NextPowerOfTwo(totalFrames),
                _bakerComputeShader,
                // Mathf.NextPowerOfTwo(totalFrames),
                totalFrames,
                _uvChannel
            );
            _baker.SaveAssets(gameObject.name, _runtimeShader, _animationFps, totalDuration, totalFrames);
            _baker.Dispose();
#endif

            Debug.Log("===== AnimatorBaker End Generate !! =====");

        }

        // private static Texture2D ConvertRenderTextureToTexture2D(RenderTexture rt)
        // {
        //     TextureFormat format = TextureFormat.RGBAHalf;
        //     Texture2D texture = new Texture2D(rt.width, rt.height, format, false);
        //     Rect rect = Rect.MinMaxRect(0f, 0f, texture.width, texture.height);
        //     RenderTexture tmp = RenderTexture.active;
        //     RenderTexture.active = rt;
        //     texture.ReadPixels(rect, 0, 0);
        //     RenderTexture.active = tmp;
        //     return texture;
        // }

        // private Mesh CreateMeshForGPUAnimation(Mesh sourceMesh)
        // {
        //     Mesh mesh = new Mesh();
        //     mesh.vertices = sourceMesh.vertices;
        //     mesh.uv = sourceMesh.uv;

        //     List<Vector2> refUV = new List<Vector2>();
        //     for (int i = 0; i < sourceMesh.vertexCount; i++)
        //     {
        //         refUV.Add(new Vector2(i + 0.5f, 0) / Mathf.NextPowerOfTwo(sourceMesh.vertexCount));
        //     }
        //     mesh.SetUVs(_uvChannel, refUV);

        //     mesh.normals = sourceMesh.normals;
        //     mesh.tangents = sourceMesh.tangents;
        //     mesh.triangles = sourceMesh.triangles;

        //     return mesh;
        // }
    }
}