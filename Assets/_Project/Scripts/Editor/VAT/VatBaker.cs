#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace EnemyEcs.Editor
{
    public static class VatBaker
    {
        private const string VatOutputDir = "Assets/_Project/Resources/VAT";
        private const float SampleFps = 24f;

        private struct ArchetypeInfo
        {
            public string Name;
            public string CompanionPrefabPath;
            public string BaseMaterialPath;
        }

        private static readonly ArchetypeInfo[] Archetypes = new[]
        {
            new ArchetypeInfo
            {
                Name = "Minion",
                CompanionPrefabPath = "Assets/_Project/Prefabs/Entities/MinionCompanion.prefab",
                BaseMaterialPath = "Assets/_Project/Materials/Minion.mat",
            },
            new ArchetypeInfo
            {
                Name = "Mutant",
                CompanionPrefabPath = "Assets/_Project/Prefabs/Entities/MutantCompanion.prefab",
                BaseMaterialPath = "Assets/_Project/Materials/Mutant.mat",
            },
            new ArchetypeInfo
            {
                Name = "Ogr",
                CompanionPrefabPath = "Assets/_Project/Prefabs/Entities/OgrCompanion.prefab",
                BaseMaterialPath = "Assets/_Project/Materials/Ogr.mat",
            },
            new ArchetypeInfo
            {
                Name = "Devil",
                CompanionPrefabPath = "Assets/_Project/Prefabs/Entities/DevilCompanion.prefab",
                BaseMaterialPath = "Assets/_Project/Materials/Devil.mat",
            },
            new ArchetypeInfo
            {
                Name = "Eye",
                CompanionPrefabPath = "Assets/_Project/Prefabs/Entities/EyeCompanion.prefab",
                BaseMaterialPath = "Assets/_Project/Materials/Eye.mat",
            },
        };

        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            if (!File.Exists("Assets/_Project/Resources/VAT/Minion_PosTex.asset"))
            {
                Debug.Log("[VatBaker] First-time setup: automatically baking VAT assets...");
                EditorApplication.delayCall += BakeAll;
            }
        }

        [MenuItem("Tools/VAT/Bake All Enemy Animations")]
        public static void BakeAll()
        {
            if (!Directory.Exists(VatOutputDir))
                Directory.CreateDirectory(VatOutputDir);

            var shader = Shader.Find("Universal Render Pipeline/VAT_UniversalLit");
            if (shader == null)
            {
                Debug.LogError("VAT_UniversalLit shader not found!");
                return;
            }

            var configs = new Dictionary<string, VatAnimationConfig>();

            foreach (ArchetypeInfo info in Archetypes)
            {
                Debug.Log($"[VatBaker] Baking archetype: {info.Name}...");
                VatAnimationConfig config = BakeArchetype(info, shader);
                if (config != null)
                    configs[info.Name] = config;
            }

            // Create variant configs for OldMutant, HotDevil, BigEye
            CreateVariants(configs, shader);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VatBaker] All VAT assets successfully baked and saved!");
        }

        private static VatAnimationConfig BakeArchetype(ArchetypeInfo info, Shader vatShader)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(info.CompanionPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at {info.CompanionPrefabPath}");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.name = "VAT_Temp_" + info.Name;
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            try
            {
                var smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(false);
                if (smrs.Length == 0)
                {
                    Debug.LogError($"No active SkinnedMeshRenderer found in {info.Name}");
                    return null;
                }

                var animator = smrs[0].GetComponentInParent<Animator>();
                if (animator == null)
                    animator = instance.GetComponentInChildren<Animator>(false);

                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    Debug.LogError($"No Animator found in {info.Name}");
                    return null;
                }

                var controller = animator.runtimeAnimatorController as AnimatorController;
                AnimationClip walkClip = FindClip(controller, "Walking");
                AnimationClip attackClip = FindClip(controller, "Attack");
                AnimationClip deathClip = FindClip(controller, "Death");

                if (walkClip == null)
                    Debug.LogWarning($"Walk clip not found for {info.Name}");

                int vertexCount = 0;
                foreach (SkinnedMeshRenderer part in smrs)
                    vertexCount += part.sharedMesh.vertexCount;

                int walkFrames = walkClip != null ? Mathf.Max(1, Mathf.CeilToInt(walkClip.length * SampleFps)) : 1;
                int attackFrames = attackClip != null ? Mathf.Max(1, Mathf.CeilToInt(attackClip.length * SampleFps)) : 1;
                int deathFrames = deathClip != null ? Mathf.Max(1, Mathf.CeilToInt(deathClip.length * SampleFps)) : 1;

                int totalFrames = walkFrames + attackFrames + deathFrames;
                int texWidth = vertexCount;
                int texHeight = totalFrames;

                var posColors = new Color[texWidth * texHeight];
                var normColors = new Color[texWidth * texHeight];

                var tempMesh = new Mesh();
                Transform root = instance.transform;

                int currentRow = 0;

                // SampleAnimation needs the GameObject the clip's curves are authored against -
                // that's whatever holds the Animator, not necessarily the instantiated root (the
                // companion prefab wraps the animated FBX model one level down).
                GameObject animationRoot = animator.gameObject;

                // Sample Walk
                BakeClipFrames(animationRoot, smrs, root, walkClip, walkFrames, tempMesh, posColors, normColors, texWidth, ref currentRow);

                // Sample Attack
                BakeClipFrames(animationRoot, smrs, root, attackClip, attackFrames, tempMesh, posColors, normColors, texWidth, ref currentRow);

                // Sample Death
                BakeClipFrames(animationRoot, smrs, root, deathClip, deathFrames, tempMesh, posColors, normColors, texWidth, ref currentRow);

                Object.DestroyImmediate(tempMesh);

                // Create Position Texture
                var posTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAHalf, false, true);
                posTex.filterMode = FilterMode.Point;
                posTex.wrapMode = TextureWrapMode.Clamp;
                posTex.SetPixels(posColors);
                posTex.Apply(false, false);

                // Create Normal Texture
                var normTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAHalf, false, true);
                normTex.filterMode = FilterMode.Point;
                normTex.wrapMode = TextureWrapMode.Clamp;
                normTex.SetPixels(normColors);
                normTex.Apply(false, false);

                string posPath = $"{VatOutputDir}/{info.Name}_PosTex.asset";
                string normPath = $"{VatOutputDir}/{info.Name}_NormTex.asset";

                AssetDatabase.CreateAsset(posTex, posPath);
                AssetDatabase.CreateAsset(normTex, normPath);

                // Create Static Mesh - combine every renderer's mesh into one, in the same order
                // used for the VAT bake above, so SV_VertexID lines up with the texture rows.
                var combineInstances = new CombineInstance[smrs.Length];
                for (var i = 0; i < smrs.Length; i++)
                    combineInstances[i] = new CombineInstance { mesh = smrs[i].sharedMesh };

                var staticMesh = new Mesh();
                staticMesh.CombineMeshes(combineInstances, mergeSubMeshes: true, useMatrices: false);
                staticMesh.name = $"{info.Name}_VAT_Mesh";
                string meshPath = $"{VatOutputDir}/{info.Name}_Mesh.asset";
                AssetDatabase.CreateAsset(staticMesh, meshPath);

                // Create Material
                var baseMat = AssetDatabase.LoadAssetAtPath<Material>(info.BaseMaterialPath);
                var vatMat = new Material(vatShader);
                vatMat.name = $"{info.Name}_VAT_Mat";
                // Without this, Unity never compiles/selects the DOTS_INSTANCING_ON variant for
                // this material, so the per-instance _AnimParams override (walk/attack frame,
                // playback time) is never read - every entity just shows the material's static
                // default (frame 0), which looks like a frozen pose.
                vatMat.enableInstancing = true;
                if (baseMat != null)
                {
                    if (baseMat.HasProperty("_BaseMap"))
                        vatMat.SetTexture("_BaseMap", baseMat.GetTexture("_BaseMap"));
                    else if (baseMat.HasProperty("_MainTex"))
                        vatMat.SetTexture("_BaseMap", baseMat.GetTexture("_MainTex"));

                    if (baseMat.HasProperty("_BaseColor"))
                        vatMat.SetColor("_BaseColor", baseMat.GetColor("_BaseColor"));
                }

                vatMat.SetTexture("_VATPositions", posTex);
                vatMat.SetTexture("_VATNormals", normTex);
                vatMat.SetVector("_VATParams", new Vector4(texWidth, texHeight, 0, 0));
                vatMat.SetVector("_AnimParams", new Vector4(0, walkFrames, 0, SampleFps));

                string matPath = $"{VatOutputDir}/{info.Name}_VatMat.mat";
                AssetDatabase.CreateAsset(vatMat, matPath);

                // Create Config ScriptableObject
                var config = ScriptableObject.CreateInstance<VatAnimationConfig>();
                config.Mesh = staticMesh;
                config.Material = vatMat;
                config.PositionTexture = posTex;
                config.NormalTexture = normTex;
                config.TextureSize = new Vector2(texWidth, texHeight);
                config.WalkStartFrame = 0;
                config.WalkFrameCount = walkFrames;
                config.AttackStartFrame = walkFrames;
                config.AttackFrameCount = attackFrames;
                config.DeathStartFrame = walkFrames + attackFrames;
                config.DeathFrameCount = deathFrames;
                config.Fps = SampleFps;

                string configPath = $"{VatOutputDir}/{info.Name}_VatConfig.asset";
                AssetDatabase.CreateAsset(config, configPath);

                return config;
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void BakeClipFrames(
            GameObject rootGo,
            SkinnedMeshRenderer[] smrs,
            Transform root,
            AnimationClip clip,
            int frameCount,
            Mesh tempMesh,
            Color[] posColors,
            Color[] normColors,
            int texWidth,
            ref int currentRow)
        {
            for (var f = 0; f < frameCount; f++)
            {
                if (clip != null)
                {
                    float time = Mathf.Min((float)f / SampleFps, clip.length);
                    clip.SampleAnimation(rootGo, time);
                }

                int rowOffset = currentRow * texWidth;
                int vertexOffset = 0;

                foreach (SkinnedMeshRenderer smr in smrs)
                {
                    // useScale:true so BakeMesh's output excludes the renderer's own scale - the
                    // TransformPoint/TransformDirection calls below already fold that scale back
                    // in via the full local-to-world matrix, and without this flag it gets
                    // applied twice.
                    smr.BakeMesh(tempMesh, true);
                    Vector3[] vertices = tempMesh.vertices;
                    Vector3[] normals = tempMesh.normals;

                    for (var v = 0; v < vertices.Length; v++)
                    {
                        Vector3 worldPos = smr.transform.TransformPoint(vertices[v]);
                        Vector3 localPos = root.InverseTransformPoint(worldPos);

                        Vector3 worldNorm = smr.transform.TransformDirection(normals[v]);
                        Vector3 localNorm = root.InverseTransformDirection(worldNorm).normalized;

                        posColors[rowOffset + vertexOffset + v] = new Color(localPos.x, localPos.y, localPos.z, 1f);
                        normColors[rowOffset + vertexOffset + v] = new Color(localNorm.x, localNorm.y, localNorm.z, 1f);
                    }

                    vertexOffset += vertices.Length;
                }

                currentRow++;
            }
        }

        private static AnimationClip FindClip(AnimatorController controller, string stateName)
        {
            if (controller == null)
                return null;

            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                foreach (ChildAnimatorState state in layer.stateMachine.states)
                {
                    if (state.state.name == stateName)
                        return state.state.motion as AnimationClip;
                }
            }

            return null;
        }

        private static void CreateVariants(Dictionary<string, VatAnimationConfig> baseConfigs, Shader vatShader)
        {
            // OldMutant: uses Mutant mesh & textures with OldMutant material
            if (baseConfigs.TryGetValue("Mutant", out VatAnimationConfig mutantConfig))
            {
                var mat = new Material(vatShader);
                mat.name = "OldMutant_VAT_Mat";
                mat.enableInstancing = true;
                mat.SetTexture("_VATPositions", mutantConfig.PositionTexture);
                mat.SetTexture("_VATNormals", mutantConfig.NormalTexture);
                mat.SetVector("_VATParams", new Vector4(mutantConfig.TextureSize.x, mutantConfig.TextureSize.y, 0, 0));
                mat.SetVector("_AnimParams", new Vector4(0, mutantConfig.WalkFrameCount, 0, mutantConfig.Fps));

                var oldMutantMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mutant.mat");
                if (oldMutantMat != null && oldMutantMat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", oldMutantMat.GetTexture("_BaseMap"));
                mat.SetColor("_BaseColor", new Color(0.7f, 0.4f, 0.4f, 1f)); // Reddish tint for OldMutant

                AssetDatabase.CreateAsset(mat, $"{VatOutputDir}/OldMutant_VatMat.mat");

                var cfg = ScriptableObject.CreateInstance<VatAnimationConfig>();
                cfg.Mesh = mutantConfig.Mesh;
                cfg.Material = mat;
                cfg.PositionTexture = mutantConfig.PositionTexture;
                cfg.NormalTexture = mutantConfig.NormalTexture;
                cfg.TextureSize = mutantConfig.TextureSize;
                cfg.WalkStartFrame = mutantConfig.WalkStartFrame;
                cfg.WalkFrameCount = mutantConfig.WalkFrameCount;
                cfg.AttackStartFrame = mutantConfig.AttackStartFrame;
                cfg.AttackFrameCount = mutantConfig.AttackFrameCount;
                cfg.DeathStartFrame = mutantConfig.DeathStartFrame;
                cfg.DeathFrameCount = mutantConfig.DeathFrameCount;
                cfg.Fps = mutantConfig.Fps;
                AssetDatabase.CreateAsset(cfg, $"{VatOutputDir}/OldMutant_VatConfig.asset");
            }

            // HotDevil: uses Devil mesh & textures with HotDevil tint
            if (baseConfigs.TryGetValue("Devil", out VatAnimationConfig devilConfig))
            {
                var mat = new Material(vatShader);
                mat.name = "HotDevil_VAT_Mat";
                mat.enableInstancing = true;
                mat.SetTexture("_VATPositions", devilConfig.PositionTexture);
                mat.SetTexture("_VATNormals", devilConfig.NormalTexture);
                mat.SetVector("_VATParams", new Vector4(devilConfig.TextureSize.x, devilConfig.TextureSize.y, 0, 0));
                mat.SetVector("_AnimParams", new Vector4(0, devilConfig.WalkFrameCount, 0, devilConfig.Fps));

                var devilMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Devil.mat");
                if (devilMat != null && devilMat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", devilMat.GetTexture("_BaseMap"));
                mat.SetColor("_BaseColor", new Color(1f, 0.5f, 0.2f, 1f)); // Fiery orange tint for HotDevil

                AssetDatabase.CreateAsset(mat, $"{VatOutputDir}/HotDevil_VatMat.mat");

                var cfg = ScriptableObject.CreateInstance<VatAnimationConfig>();
                cfg.Mesh = devilConfig.Mesh;
                cfg.Material = mat;
                cfg.PositionTexture = devilConfig.PositionTexture;
                cfg.NormalTexture = devilConfig.NormalTexture;
                cfg.TextureSize = devilConfig.TextureSize;
                cfg.WalkStartFrame = devilConfig.WalkStartFrame;
                cfg.WalkFrameCount = devilConfig.WalkFrameCount;
                cfg.AttackStartFrame = devilConfig.AttackStartFrame;
                cfg.AttackFrameCount = devilConfig.AttackFrameCount;
                cfg.DeathStartFrame = devilConfig.DeathStartFrame;
                cfg.DeathFrameCount = devilConfig.DeathFrameCount;
                cfg.Fps = devilConfig.Fps;
                AssetDatabase.CreateAsset(cfg, $"{VatOutputDir}/HotDevil_VatConfig.asset");
            }

            // BigEye: uses Eye mesh & textures with BigEye material
            if (baseConfigs.TryGetValue("Eye", out VatAnimationConfig eyeConfig))
            {
                var mat = new Material(vatShader);
                mat.name = "BigEye_VAT_Mat";
                mat.enableInstancing = true;
                mat.SetTexture("_VATPositions", eyeConfig.PositionTexture);
                mat.SetTexture("_VATNormals", eyeConfig.NormalTexture);
                mat.SetVector("_VATParams", new Vector4(eyeConfig.TextureSize.x, eyeConfig.TextureSize.y, 0, 0));
                mat.SetVector("_AnimParams", new Vector4(0, eyeConfig.WalkFrameCount, 0, eyeConfig.Fps));

                var eyeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Eye.mat");
                if (eyeMat != null && eyeMat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", eyeMat.GetTexture("_BaseMap"));
                mat.SetColor("_BaseColor", new Color(0.9f, 0.2f, 0.2f, 1f)); // Dark red tint for BigEye

                AssetDatabase.CreateAsset(mat, $"{VatOutputDir}/BigEye_VatMat.mat");

                var cfg = ScriptableObject.CreateInstance<VatAnimationConfig>();
                cfg.Mesh = eyeConfig.Mesh;
                cfg.Material = mat;
                cfg.PositionTexture = eyeConfig.PositionTexture;
                cfg.NormalTexture = eyeConfig.NormalTexture;
                cfg.TextureSize = eyeConfig.TextureSize;
                cfg.WalkStartFrame = eyeConfig.WalkStartFrame;
                cfg.WalkFrameCount = eyeConfig.WalkFrameCount;
                cfg.AttackStartFrame = eyeConfig.AttackStartFrame;
                cfg.AttackFrameCount = eyeConfig.AttackFrameCount;
                cfg.DeathStartFrame = eyeConfig.DeathStartFrame;
                cfg.DeathFrameCount = eyeConfig.DeathFrameCount;
                cfg.Fps = eyeConfig.Fps;
                AssetDatabase.CreateAsset(cfg, $"{VatOutputDir}/BigEye_VatConfig.asset");
            }
        }
    }
}
#endif
