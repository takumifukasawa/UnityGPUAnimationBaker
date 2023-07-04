using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace GPUAnimationBaker
{
    public class VertexAttributesBaker
    {

        private struct VertexAttributes
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Tangent;
        }

        private RenderTexture _bakedPositionRenderTexture;
        private RenderTexture _bakedNormalRenderTexture;
        private RenderTexture _bakedTangentRenderTexture;

        private Texture2D _bakedPositionMap;
        private Texture2D _bakedNormalMap;
        private Texture2D _bakedTangentMap;

        private List<VertexAttributes> _vertexAttributesList;

        private List<GPUAnimationFrame> _gpuAnimationFrames;

        private Mesh _runtimeMesh;

        private SkinnedMeshRenderer _skinnedMeshRenderer;

        private Mesh _memoryMesh;


        public VertexAttributesBaker(
            SkinnedMeshRenderer skinnedMeshRenderer
        )
        {
            // foreach (RenderTexture renderTexture in new[] { _bakedPositionRenderTexture, _bakedNormalRenderTexture, _bakedTangentRenderTexture })
            // {
            //     renderTexture.enableRandomWrite = true;
            //     renderTexture.Create();
            //     RenderTexture.active = renderTexture;
            //     GL.Clear(true, true, Color.clear);
            // }

            _vertexAttributesList = new List<VertexAttributes>();
            _gpuAnimationFrames = new List<GPUAnimationFrame>();

            _skinnedMeshRenderer = skinnedMeshRenderer;

            _memoryMesh = new Mesh();
        }

        RenderTexture CreateRenderTexture(int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
            rt.enableRandomWrite = true;
            rt.Create();

            RenderTexture tmp = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = tmp;

            return rt;
        }


        public void MemoryVertexAttributes(int vertexIndex)
        {
            _skinnedMeshRenderer.BakeMesh(_memoryMesh);

            VertexAttributes vertexAttributes = new VertexAttributes()
            {
                Position = _memoryMesh.vertices[vertexIndex],
                Normal = _memoryMesh.normals[vertexIndex],
                Tangent = _memoryMesh.tangents[vertexIndex],
            };
            _vertexAttributesList.Add(vertexAttributes);
        }

        public void MemoryAnimationFrame(string name, int frames)
        {
            GPUAnimationFrame gpuAnimationFrame = new GPUAnimationFrame(name, frames);
            _gpuAnimationFrames.Add(gpuAnimationFrame);
        }

#if UNITY_EDITOR

        public void Bake(
            ComputeShader bakerComputeShader,
            int frames,
            int uvChannel)
        {
            int vertexCount = _skinnedMeshRenderer.sharedMesh.vertexCount;

            int pixels = vertexCount * frames;

            Vector2Int textureSize = GetTexturePOTRect(pixels);

            int textureWidth = textureSize.x;
            int textureHeight = textureSize.y;

            _bakedPositionRenderTexture = CreateRenderTexture(textureWidth, textureHeight);
            _bakedNormalRenderTexture = CreateRenderTexture(textureWidth, textureHeight);
            _bakedTangentRenderTexture = CreateRenderTexture(textureWidth, textureHeight);

            // for (int i = 0; i < textureSize.x * textureSize.y - pixels; i++)
            // {
            //     _vertexAttributesList.Add(new VertexAttributes()
            //     {
            //         Position = Vector3.zero,
            //         Normal = Vector3.zero,
            //         Tangent = Vector3.zero
            //     });
            // }

            GraphicsBuffer graphicsBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                _vertexAttributesList.Count * 3,
                Marshal.SizeOf(new Vector3())
            );
            graphicsBuffer.SetData(_vertexAttributesList.ToArray());

            int kernel = bakerComputeShader.FindKernel("CSMain");
            // uint x, y, z;
            // bakerComputeShader.GetKernelThreadGrouprects(kernel, out x, out y, out z);

            bakerComputeShader.SetInt("VertexCount", vertexCount);
            bakerComputeShader.SetInt("TextureWidth", textureWidth);
            bakerComputeShader.SetBuffer(kernel, "InputData", graphicsBuffer);
            bakerComputeShader.SetTexture(kernel, "OutPosition", _bakedPositionRenderTexture);
            bakerComputeShader.SetTexture(kernel, "OutNormal", _bakedNormalRenderTexture);
            bakerComputeShader.SetTexture(kernel, "OutTangent", _bakedTangentRenderTexture);

            bakerComputeShader.Dispatch(
                kernel,
                textureWidth,
                // (int)Mathf.Floor((float)pixels / (float)textureSize.x),
                textureHeight,
                1
            );

            Debug.Log(string.Format(
                "[VertexAttributesBaker] Bake - width: {0}, height: {1}, vertexCount: {2}, frames: {3}, vertexAttributesList count: {4}",
                textureWidth,
                textureHeight,
                vertexCount,
                frames,
                _vertexAttributesList.Count
            ));

            graphicsBuffer.Release();

            _bakedPositionMap = ConvertRenderTextureToTexture2D(_bakedPositionRenderTexture);
            _bakedNormalMap = ConvertRenderTextureToTexture2D(_bakedNormalRenderTexture);
            _bakedTangentMap = ConvertRenderTextureToTexture2D(_bakedTangentRenderTexture);

            // Graphics.CopyTexture(_bakedPositionRenderTexture, _bakedPositionMap);
            // Graphics.CopyTexture(_bakedNormalRenderTexture, _bakedNormalMap);
            // Graphics.CopyTexture(_bakedTangentRenderTexture, _bakedTangentMap);

            _runtimeMesh = CreateMeshForGPUAnimation(_skinnedMeshRenderer.sharedMesh, uvChannel);
        }

        public void SaveAssets(string name, Shader runtimeShader, float fps, float totalDuration, float totalFrames)
        {
            string folderName = "GPUAnimationBakerExported";
            string folderPath = Path.Combine("Assets", folderName);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", folderName);
            }
            string subFolder = name;
            string subFolderPath = Path.Combine(folderPath, subFolder);
            if (!AssetDatabase.IsValidFolder(subFolderPath))
            {
                AssetDatabase.CreateFolder(folderPath, subFolder);
            }

            // staticMeshGameObject.AddComponent<MeshFilter>().sharedMesh = skinnedMeshRenderer.sharedMesh;
            Material runtimeMaterial = new Material(runtimeShader);
            runtimeMaterial.SetTexture("_BakedPositionMap", _bakedPositionMap);
            runtimeMaterial.SetTexture("_BakedNormalMap", _bakedNormalMap);
            runtimeMaterial.SetTexture("_BakedTangentMap", _bakedTangentMap);
            // TODO
            // runtimeMaterial.SetFloat("_BakedAnimationDuration", 0);

            AssetDatabase.CreateAsset(
                _bakedPositionMap,
                Path.Combine(
                    subFolderPath,
                    string.Format("{0}.BakedPositionMap.asset", name)
                )
            );
            AssetDatabase.CreateAsset(
                _bakedNormalMap,
                Path.Combine(
                    subFolderPath,
                    string.Format("{0}.BakedNormalMap.asset", name)
                )
            );
            AssetDatabase.CreateAsset(
                _bakedTangentMap,
                Path.Combine(
                    subFolderPath,
                    string.Format("{0}.BakedTangentMap.asset", name)
                )
            );
            AssetDatabase.CreateAsset(
                _runtimeMesh,
                Path.Combine(
                    subFolderPath,
                    string.Format("{0}.Mesh.asset", name)
                )
            );
            AssetDatabase.CreateAsset(
                runtimeMaterial,
                Path.Combine(
                    subFolderPath,
                    string.Format("{0}.Material.asset", name)
                )
            );

            // ----

            GPUAnimationDataScriptableObject gpuAnimationData = ScriptableObject.CreateInstance<GPUAnimationDataScriptableObject>();
            gpuAnimationData.FPS = fps;
            gpuAnimationData.TotalDuration = totalDuration;
            gpuAnimationData.TotalFrames = totalFrames;
            gpuAnimationData.GPUAnimationFrames = _gpuAnimationFrames;
            // 全部サイズ一緒なのでrendertextureから引っ張ってきちゃう
            gpuAnimationData.TextureWidth = _bakedPositionRenderTexture.width;
            gpuAnimationData.TextureHeight = _bakedPositionRenderTexture.height;
            gpuAnimationData.VertexCount = _skinnedMeshRenderer.sharedMesh.vertexCount;

            AssetDatabase.CreateAsset(
                gpuAnimationData,
                Path.Combine(
                    subFolderPath,
                    string.Format("{0}.GPUAnimationData.asset", name)
                )
            );

            GameObject staticMeshGameObject = new GameObject(name);

            staticMeshGameObject.AddComponent<MeshRenderer>().sharedMaterial = runtimeMaterial;

            GPUAnimationController gpuAnimationController = staticMeshGameObject.AddComponent<GPUAnimationController>();
            gpuAnimationController.SetAnimationData(gpuAnimationData);
            gpuAnimationController.SetIsRuntime(false);

            staticMeshGameObject.AddComponent<MeshFilter>().sharedMesh = _runtimeMesh;

            PrefabUtility.SaveAsPrefabAsset(
                staticMeshGameObject,
                Path.Combine(folderPath, name + ".prefab").Replace("\\", "/")
            // Path.Combine(subFolderPath, name + ".prefab").Replace("\\", "/")
            );

            // ----

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

#endif

        public void Dispose()
        {
            _bakedPositionRenderTexture.Release();
            _bakedPositionRenderTexture = null;
            _bakedNormalRenderTexture.Release();
            _bakedNormalRenderTexture = null;
            _bakedTangentRenderTexture.Release();
            _bakedTangentRenderTexture = null;
        }

        private static Texture2D ConvertRenderTextureToTexture2D(RenderTexture rt)
        {
            TextureFormat format = TextureFormat.RGBAHalf;
            Texture2D texture = new Texture2D(rt.width, rt.height, format, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            texture.filterMode = FilterMode.Point;
            // texture.filterMode = FilterMode.Bilinear;

            Rect rect = Rect.MinMaxRect(0f, 0f, texture.width, texture.height);
            RenderTexture tmp = RenderTexture.active;
            RenderTexture.active = rt;
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();
            RenderTexture.active = tmp;
            return texture;
        }

        private static Mesh CreateMeshForGPUAnimation(Mesh sourceMesh, int uvChannel = 1)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = sourceMesh.vertices;
            mesh.uv = sourceMesh.uv;

            List<Vector2> refUV = new List<Vector2>();
            for (int i = 0; i < sourceMesh.vertexCount; i++)
            {
                // refUV.Add(new Vector2(i + 0.5f, 0) / Mathf.NextPowerOfTwo(sourceMesh.vertexCount));
                // refUV.Add(new Vector2(i, 0) / Mathf.NextPowerOfTwo(sourceMesh.vertexCount));
                refUV.Add(new Vector2(i, 0));
            }
            mesh.SetUVs(uvChannel, refUV);

            mesh.normals = sourceMesh.normals;
            mesh.tangents = sourceMesh.tangents;
            mesh.triangles = sourceMesh.triangles;

            return mesh;
        }

        private static Vector2Int GetTexturePOTRect(int pixels)
        {
            // Vector2Int rect = new Vector2Int(128, 128);
            Vector2Int rect = new Vector2Int(1, 1);
            bool isCheckW = true;
            while (true)
            {
                int sum = rect.x * rect.y;
                if (pixels <= sum)
                {
                    break;
                }
                if (isCheckW)
                {
                    rect.x = Mathf.NextPowerOfTwo(rect.x + 1);
                }
                else
                {
                    rect.y = Mathf.NextPowerOfTwo(rect.y + 1);
                }
                isCheckW = !isCheckW;
            }
            Debug.Log(string.Format("[VertexAttributesBaker] pixels: {0}, width: {1}, height: {2}", pixels, rect.x, rect.y));
            return rect;
        }
    }

}