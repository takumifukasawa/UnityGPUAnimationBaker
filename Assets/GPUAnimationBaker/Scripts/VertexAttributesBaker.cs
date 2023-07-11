using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using System;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
#endif

namespace GPUAnimationBaker
{
    /// <summary>
    /// 
    /// </summary>
    public class VertexAttributesBaker
    {
        // ----------------------------------------------------------------------------------
        // public
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skinnedMeshRenderer"></param>
        public VertexAttributesBaker(
            SkinnedMeshRenderer skinnedMeshRenderer
        )
        {
            _boneAttributesList = new List<BoneAttributes>();
            _gpuAnimationFrames = new List<GPUAnimationFrame>();

            _skinnedMeshRenderer = skinnedMeshRenderer;

            _memoryMesh = new Mesh();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexIndex"></param>
        // tmp
        // public void MemoryVertexAttributes(int vertexIndex)
        public void MemoryAllBoneAttributes()
        {
            _skinnedMeshRenderer.BakeMesh(_memoryMesh);

            var boneOffsetMatrices = GetBoneOffsetMatrices(_skinnedMeshRenderer);
            var bones = _skinnedMeshRenderer.bones;
            var poseMatrices = CalculateBonePoseMatrices(bones, boneOffsetMatrices);
            // var m = _skinnedMeshRenderer.bones[boneIndex].localToWorldMatrix;

            for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
            {
                MemoryBoneAttributes(poseMatrices[i]);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexIndex"></param>
        // tmp
        // public void MemoryVertexAttributes(int vertexIndex)
        void MemoryBoneAttributes(Matrix4x4 m)
        {
            BoneAttributes vertexAttributes = new BoneAttributes(
                new Vector4(m.m00, m.m01, m.m02, m.m03),
                new Vector4(m.m10, m.m11, m.m12, m.m13),
                new Vector4(m.m20, m.m21, m.m22, m.m23)
                // new Vector4(m.m30, m.m31, m.m32, m.m33)
                // new Vector4(0, 0, 0, 1)
            );
            _boneAttributesList.Add(vertexAttributes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="frames"></param>
        public void MemoryAnimationFrame(string name, int frames)
        {
            GPUAnimationFrame gpuAnimationFrame = new GPUAnimationFrame(name, frames);
            _gpuAnimationFrames.Add(gpuAnimationFrame);
        }

#if UNITY_EDITOR

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bakerComputeShader"></param>
        /// <param name="frames"></param>
        /// <param name="uvChannel"></param>
        public void Bake(
            ComputeShader bakerComputeShader,
            int frames,
            int uvChannel)
        {
            int bakeRowNum = 4;
            
            int boneCount = _skinnedMeshRenderer.bones.Length;

            // for bake skinning
            int pixels = boneCount * bakeRowNum * frames;

            Vector2Int textureSize = GetTexturePOTRect(pixels);

            int textureWidth = textureSize.x;
            int textureHeight = textureSize.y;

            _bakedBonesRenderTexture = CreateRenderTexture(textureWidth, textureHeight);

            GraphicsBuffer graphicsBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                _boneAttributesList.Count,
                Marshal.SizeOf(new Vector4()) * bakeRowNum
            );
            graphicsBuffer.SetData(_boneAttributesList.ToArray());

            int kernel = bakerComputeShader.FindKernel("CSMain");
            // uint x, y, z;
            // bakerComputeShader.GetKernelThreadGrouprects(kernel, out x, out y, out z);

            int div = Mathf.FloorToInt(textureWidth / bakeRowNum);

            bakerComputeShader.SetInt("BoneCount", boneCount);
            bakerComputeShader.SetInt("TextureWidth", textureWidth);
            bakerComputeShader.SetInt("TextureHeight", textureHeight);
            bakerComputeShader.SetBuffer(kernel, "InputData", graphicsBuffer);
            bakerComputeShader.SetTexture(kernel, "OutBones", _bakedBonesRenderTexture);
            bakerComputeShader.Dispatch(
                kernel,
                textureWidth / bakeRowNum, // w
                textureHeight, // h
                1
            );

            Debug.Log($"[VertexAttributesBaker] Bake - width: {textureWidth}, height: {textureHeight}, boneCount: {boneCount}, frames: {frames}, vertexAttributesList count: {_boneAttributesList.Count}");

            graphicsBuffer.Release();

            _bakedBonesMap = ConvertRenderTextureToTexture2D(_bakedBonesRenderTexture);

            _runtimeMesh = CreateMeshForGPUAnimation(
                _skinnedMeshRenderer.sharedMesh,
                uvChannel
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="runtimeShader"></param>
        /// <param name="fps"></param>
        /// <param name="totalDuration"></param>
        /// <param name="totalFrames"></param>
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

            Material runtimeMaterial = new Material(runtimeShader);
            runtimeMaterial.SetTexture("_BakedBonesMap", _bakedBonesMap);

            AssetDatabase.CreateAsset(
                _bakedBonesMap,
                Path.Combine(
                    subFolderPath,
                    string.Format("{0}.BakedBonesMap.asset", name)
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

            GPUAnimationDataScriptableObject gpuAnimationData = GPUAnimationDataScriptableObject.Create(
                fps,
                totalDuration,
                totalFrames,
                _bakedBonesRenderTexture.width,
                _bakedBonesRenderTexture.height,
                _skinnedMeshRenderer.sharedMesh.vertexCount,
                _skinnedMeshRenderer.bones.Length,
                _gpuAnimationFrames,
                GetBoneOffsetMatrices(_skinnedMeshRenderer).ToList()
            );

            AssetDatabase.CreateAsset(
                gpuAnimationData,
                Path.Combine(
                    subFolderPath,
                    string.Format("{0}.GPUAnimationData.asset", name)
                )
            );

            GameObject staticMeshGameObject = new GameObject(name);

            staticMeshGameObject.AddComponent<MeshRenderer>().sharedMaterial = runtimeMaterial;
            // staticMeshGameObject.AddComponent<MeshRenderer>().materials[0] = runtimeMaterial;

            staticMeshGameObject.AddComponent<MeshFilter>().sharedMesh = _runtimeMesh;

            Debug.Log($"[VertexAttributesBaker.SaveAssets] static mesh go: {staticMeshGameObject}");
            GPUAnimationController gpuAnimationController = staticMeshGameObject.AddComponent<GPUAnimationController>();
            gpuAnimationController.SetAnimationData(gpuAnimationData);
            gpuAnimationController.SetIsRuntime(false);

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

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _bakedBonesRenderTexture.Release();
            _bakedBonesRenderTexture = null;
        }

        // ----------------------------------------------------------------------------------
        // private
        // ----------------------------------------------------------------------------------

        private struct BoneAttributes
        {
            public Vector4 BoneRow0;
            public Vector4 BoneRow1;
            public Vector4 BoneRow2;
            public Vector4 BoneRow3;

            public BoneAttributes(Vector4 r0, Vector4 r1, Vector4 r2)
            {
                BoneRow0 = r0;
                BoneRow1 = r1;
                BoneRow2 = r2;
                BoneRow3 = new Vector4(0, 0, 0, 1);
                // BoneRow3 = r3;
            }
        }

        private RenderTexture _bakedBonesRenderTexture;

        private Texture2D _bakedBonesMap;

        private List<BoneAttributes> _boneAttributesList;

        private List<GPUAnimationFrame> _gpuAnimationFrames;

        private Mesh _runtimeMesh;

        private SkinnedMeshRenderer _skinnedMeshRenderer;

        private Mesh _memoryMesh;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
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


        // /// <summary>
        // /// とある時点での全てのボーンにおける、ボーンごとの姿勢行列
        // /// </summary>
        // /// <returns></returns>
        // static Matrix4x4 GetBonePoseMatrices(SkinnedMeshRenderer skinnedMeshRenderer)
        // {
        //     int vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;
        //     var boneWeights = skinnedMeshRenderer.sharedMesh.GetAllBoneWeights();
        //     var bonesPerVertex = skinnedMeshRenderer.sharedMesh.GetBonesPerVertex();

        //     Debug.Log("-------------------");
        //     Debug.Log(skinnedMeshRenderer.bones);
        //     Debug.Log($"bone count: {skinnedMeshRenderer.bones.Length}");
        //     Debug.Log(skinnedMeshRenderer.sharedMesh.bindposes);
        //     Debug.Log($"bind pose count: {skinnedMeshRenderer.sharedMesh.bindposes.Length}");

        //     var bones = skinnedMeshRenderer.bones;

        //     var boneWeightIndex = 0;
        //     for (var vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
        //     {
        //         var totalWeight = 0f;
        //         var numberOfBonesForThisVertex = bonesPerVertex[vertexIndex];
        //         Debug.Log($"vertex index: {vertexIndex}, influence bone num: {numberOfBonesForThisVertex}");
        //         for (var i = 0; i < numberOfBonesForThisVertex; i++)
        //         {
        //             var currentBoneWeight = boneWeights[boneWeightIndex];
        //             totalWeight += currentBoneWeight.weight;
        //             Debug.Log($"vertex index: {vertexIndex}, influence bone index: {currentBoneWeight.boneIndex}, bone weight: {currentBoneWeight.weight}");
        //             if (i > 0)
        //             {
        //                 Debug.Assert(boneWeights[boneWeightIndex - 1].weight != currentBoneWeight.weight);
        //             }

        //             boneWeightIndex++;
        //         }

        //         Debug.Assert(Mathf.Approximately(1f, totalWeight));
        //     }

        //     Debug.Log("-------------------");
        // }

        /// <summary>
        /// とある頂点のboneweightを取得
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <returns></returns>
        // List<BoneWeight1> GetVertexBoneWeight(int vertexIndex)
        static List<List<BoneWeight1>> GetVerticesBoneWeights(Mesh sourceMesh)
        {
            // int vertexCount = _skinnedMeshRenderer.sharedMesh.vertexCount;
            // var boneWeights = _skinnedMeshRenderer.sharedMesh.GetAllBoneWeights();
            // var bonesPerVertex = _skinnedMeshRenderer.sharedMesh.GetBonesPerVertex();

            // var vertexBoneWeights = new List<BoneWeight1>();

            // var boneWeightIndex = 0;
            // var totalWeight = 0f;
            // var numberOfBonesForThisVertex = bonesPerVertex[vertexIndex];
            // for (var i = 0; i < numberOfBonesForThisVertex; i++)
            // {
            //     var currentBoneWeight = boneWeights[boneWeightIndex];
            //     Debug.Log($"vertex index: {vertexIndex}, influence bone index: {currentBoneWeight.boneIndex}, bone weight: {currentBoneWeight.weight}");
            //     vertexBoneWeights.Add(currentBoneWeight);
            //     boneWeightIndex++;
            // }

            // return vertexBoneWeights;

            var verticesBoneWeights = new List<List<BoneWeight1>>();

            int vertexCount = sourceMesh.vertexCount;
            var boneWeights = sourceMesh.GetAllBoneWeights();
            var bonesPerVertex = sourceMesh.GetBonesPerVertex();

            // Debug.Log("-------------------");
            // Debug.Log(_skinnedMeshRenderer.bones);
            // Debug.Log($"bone count: {_skinnedMeshRenderer.bones.Length}");
            // Debug.Log(_skinnedMeshRenderer.sharedMesh.bindposes);
            // Debug.Log($"bind pose count: {_skinnedMeshRenderer.sharedMesh.bindposes.Length}");
            // var bones = _skinnedMeshRenderer.bones;

            var boneWeightIndex = 0;
            for (var vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
            {
                var vertexBoneWeight = new List<BoneWeight1>();
                var totalWeight = 0f;
                var numberOfBonesForThisVertex = bonesPerVertex[vertexIndex];
                // Debug.Log($"vertex index: {vertexIndex}, influence bone num: {numberOfBonesForThisVertex}");
                for (var i = 0; i < numberOfBonesForThisVertex; i++)
                {
                    var currentBoneWeight = boneWeights[boneWeightIndex];
                    totalWeight += currentBoneWeight.weight;
                    // Debug.Log($"vertex index: {vertexIndex}, influence bone index: {currentBoneWeight.boneIndex}, bone weight: {currentBoneWeight.weight}");
                    if (i > 0)
                    {
                        Debug.Assert(boneWeights[boneWeightIndex - 1].weight != currentBoneWeight.weight);
                    }

                    vertexBoneWeight.Add(currentBoneWeight);

                    boneWeightIndex++;
                }

                verticesBoneWeights.Add(vertexBoneWeight);

                Debug.Assert(Mathf.Approximately(1f, totalWeight));
            }

            Debug.Log("-------------------");

            return verticesBoneWeights;
        }

        // /// <summary>
        // /// 
        // /// </summary>
        // void GetBoneWeights()
        // {
        //     int vertexCount = _skinnedMeshRenderer.sharedMesh.vertexCount;
        //     var boneWeights = _skinnedMeshRenderer.sharedMesh.GetAllBoneWeights();
        //     var bonesPerVertex = _skinnedMeshRenderer.sharedMesh.GetBonesPerVertex();

        //     Debug.Log("-------------------");
        //     Debug.Log(_skinnedMeshRenderer.bones);
        //     Debug.Log($"bone count: {_skinnedMeshRenderer.bones.Length}");
        //     Debug.Log(_skinnedMeshRenderer.sharedMesh.bindposes);
        //     Debug.Log($"bind pose count: {_skinnedMeshRenderer.sharedMesh.bindposes.Length}");

        //     var boneWeightIndex = 0;
        //     for (var vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
        //     {
        //         var totalWeight = 0f;
        //         var numberOfBonesForThisVertex = bonesPerVertex[vertexIndex];
        //         Debug.Log($"vertex index: {vertexIndex}, influence bone num: {numberOfBonesForThisVertex}");
        //         for (var i = 0; i < numberOfBonesForThisVertex; i++)
        //         {
        //             var currentBoneWeight = boneWeights[boneWeightIndex];
        //             totalWeight += currentBoneWeight.weight;
        //             Debug.Log($"vertex index: {vertexIndex}, influence bone index: {currentBoneWeight.boneIndex}, bone weight: {currentBoneWeight.weight}");
        //             if (i > 0)
        //             {
        //                 Debug.Assert(boneWeights[boneWeightIndex - 1].weight != currentBoneWeight.weight);
        //             }

        //             boneWeightIndex++;
        //         }

        //         Debug.Assert(Mathf.Approximately(1f, totalWeight));
        //     }

        //     Debug.Log("-------------------");
        // }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rt"></param>
        /// <returns></returns>
        private static Texture2D ConvertRenderTextureToTexture2D(RenderTexture rt)
        {
            TextureFormat format = TextureFormat.RGBAHalf;
            Texture2D texture = new Texture2D(rt.width, rt.height, format, false);
            
            // st wrap mode
            texture.wrapMode = TextureWrapMode.Clamp;

            // sets filter mode
            texture.filterMode = FilterMode.Point;

            // set anisotropy
            texture.anisoLevel = 0;

            Rect rect = Rect.MinMaxRect(0f, 0f, texture.width, texture.height);
            RenderTexture tmp = RenderTexture.active;
            RenderTexture.active = rt;
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();
            RenderTexture.active = tmp;
            return texture;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceMesh"></param>
        /// <param name="uvChannel"></param>
        /// <returns></returns>
        private static Mesh CreateMeshForGPUAnimation(
            Mesh sourceMesh,
            int uvChannel = 1
        )
        {
            Mesh mesh = new Mesh();
            mesh.vertices = sourceMesh.vertices;
            mesh.uv = sourceMesh.uv;

            int animationFramesUVChannel = 1;
            int boneWeightsUVChannel = 2;

            // create uv
            List<Vector2> refUV = new List<Vector2>();
            for (int i = 0; i < sourceMesh.vertexCount; i++)
            {
                // refUV.Add(new Vector2(i + 0.5f, 0) / Mathf.NextPowerOfTwo(sourceMesh.vertexCount));
                // refUV.Add(new Vector2(i, 0) / Mathf.NextPowerOfTwo(sourceMesh.vertexCount));
                refUV.Add(new Vector2(i, 0));
            }


            // create vertex bone weights
            var verticesBoneWeights = GetVerticesBoneWeights(sourceMesh);
            List<Vector4> packedVerticesBoneWeights = new List<Vector4>();
            for (int vertexIndex = 0; vertexIndex < verticesBoneWeights.Count; vertexIndex++)
            {
                var packedBoneWeight = Vector4.zero;
                // 一旦影響するボーンは2つまで
                // TODO: 4つまで影響させたい
                // index: 0
                packedBoneWeight.x = verticesBoneWeights[vertexIndex][0].boneIndex;
                packedBoneWeight.y = verticesBoneWeights[vertexIndex][0].weight;
                // Debug.Log($"bone -  vertex index: {vertexIndex}, bone index: {packedBoneWeight.x}, weight: {packedBoneWeight.y}");
                // var accWeight = 0f;
                // accWeight += packedBoneWeight.y;
                // index: 1
                if (verticesBoneWeights[vertexIndex].Count >= 2)
                {
                    packedBoneWeight.z = verticesBoneWeights[vertexIndex][1].boneIndex;
                    packedBoneWeight.w = verticesBoneWeights[vertexIndex][1].weight;
                    // Debug.Log($"bone -  vertex index: {vertexIndex}, bone index: {packedBoneWeight.z}, weight: {packedBoneWeight.w}");
                    // accWeight += packedBoneWeight.w;
                }
                // Debug.Log($"acc weight: {accWeight}");
                // if (accWeight < 0.95f)
                // {
                //     Debug.LogWarning("invalid weight");
                // }

                packedVerticesBoneWeights.Add(packedBoneWeight);
            }

            // for debug
            // Debug.Log("==========================");
            // Debug.Log(verticesBoneWeights.Count);
            // for (int i = 0; i < verticesBoneWeights.Count; i++)
            // {
            //     var vertexBoneWeight = verticesBoneWeights[i];
            //     Debug.Log($"index: {i}, bone weight num: {vertexBoneWeight.Count}");
            // }
            // Debug.Log("==========================");

            // default
            // mesh.SetUVs(uvChannel, refUV);
            mesh.SetUVs(animationFramesUVChannel, refUV);

            mesh.SetUVs(boneWeightsUVChannel, packedVerticesBoneWeights);

            mesh.normals = sourceMesh.normals;
            mesh.tangents = sourceMesh.tangents;
            mesh.triangles = sourceMesh.triangles;

            mesh.bindposes = sourceMesh.bindposes;

            return mesh;
        }

        /// <summary>
        /// 全てのボーンにおける、ボーンごとの初期姿勢行列の逆行列
        /// 初期姿勢において、ローカル座標の原点に合わせる役割
        /// </summary>
        /// <returns></returns>
        static Matrix4x4[] GetBoneOffsetMatrices(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            return skinnedMeshRenderer.sharedMesh.bindposes;
        }

        /// <summary>
        /// すべてのボーンにおける、ボーンオフセット行列を踏まえたボーンの姿勢行列
        /// </summary>
        /// <param name="bones"></param>
        /// <returns></returns>
        private static List<Matrix4x4> CalculateBonePoseMatrices(Transform[] bones, Matrix4x4[] boneOffsetMatrices)
        {
            var bonePoseMatrices = new List<Matrix4x4>();
            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];

                // 1. boneの localmatrix をかけてくパターン
                // Matrix4x4 resultMatrix = Matrix4x4.TRS(bone.localPosition, bone.localRotation, bone.localScale);
                // InternalCalculateBonePoseMatrix(bone, ref resultMatrix);
                // // Debug.Log("result mat");
                // // Debug.Log(resultMatrix);
                // resultMatrix = resultMatrix * boneOffsetMatrices[i];
                // // Debug.Log("result mat * bone offset");
                // // Debug.Log(resultMatrix);
                // // var mat = InternalCalculateBonePoseMatrix(bone, ref resultMatrix);
                // // var mat = bone.localToWorldMatrix;
                // // var mat = Matrix4x4.TRS(bone.position, bone.rotation, bone.lossyScale);

                // 2. bone world matrix * bone offset matrix
                var resultMatrix = bone.localToWorldMatrix * boneOffsetMatrices[i];

                bonePoseMatrices.Add(resultMatrix);
            }

            return bonePoseMatrices;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bone"></param>
        /// <returns></returns>
        private static void InternalCalculateBonePoseMatrix(Transform targetBone, ref Matrix4x4 resultMatrix)
        {
            // Debug.Log($"target bone: {targetBone}, has parent bone: {!!targetBone.parent}");
            // Debug.Log(resultMatrix);
            if (!targetBone.parent)
            {
                return;
            }

            resultMatrix = resultMatrix * Matrix4x4.TRS(
                               targetBone.parent.localPosition,
                               targetBone.parent.localRotation,
                               targetBone.parent.localScale
                           );
            // Debug.Log(resultMatrix);
            InternalCalculateBonePoseMatrix(targetBone.parent, ref resultMatrix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
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