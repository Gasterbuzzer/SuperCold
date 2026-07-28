using System.Reflection;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

namespace SuperCold
{
    public class MainClass : MelonMod
    {
        private static Material greyWall;
        private static Material purpleGun;
        private static Material blueEnemy;

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Metallic = Shader.PropertyToID("_Metallic");
        private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
        private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");

        // Create Materials for later replacing.
        public override void OnLateInitializeMelon()
        {
            greyWall = new Material(Shader.Find("Standard"));
            greyWall.color = new Color(0.176f, 0.188f, 0.212f, 1f);

            purpleGun = new Material(Shader.Find("Standard"));
            purpleGun.color = new Color(0.82f, 0.118f, 0.761f, 1f);

            blueEnemy = new Material(Shader.Find("Standard"));

            // Set the main color of the material to blue
            blueEnemy.color = new Color(0.1f, 0.1f, 1f, 1f); // _Color set to blue

            // Set the emission color to blue to simulate a subtle glow
            blueEnemy.EnableKeyword("_EMISSION");

            // _EmissionColor set to blue
            blueEnemy.SetColor(EmissionColor, new Color(0.3f, 0.3f, 0.6f, 1f));

            // Set glossiness (similar to _Shininess and _Gloss)
            blueEnemy.SetFloat(Glossiness, 0.7f); // A combination of _Shininess and _Gloss values

            // Specular highlights color approximation (using Standard Shader's PBR model)
            // _SpecColor / _SuperSpecColor equivalent
            blueEnemy.SetColor(SpecColor, new Color(0.5f, 0.5f, 0.5f, 1f));
            blueEnemy.SetFloat(Metallic, 0.5f); // Approximating the specular effect

            // Simulate rim color with emission if needed (not directly supported by Standard Shader)
            Color rimColor = new Color(0.26f, 0.19f, 0.16f, 0f);

            // Simulate a subtle rim effect
            blueEnemy.SetColor(EmissionColor, blueEnemy.GetColor(EmissionColor) + rimColor * 0.3f);
        }

        // On Game Scene Change, replace material of walls and objects with the following names:
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            ProcessObjectsWithName("pb", greyWall);
            ProcessObjectsWithName("prefab", greyWall);
            ProcessObjectsWithName("model", greyWall);

            ProcessObjectsWithName("Glock", purpleGun);
            ProcessObjectsWithName("glock", purpleGun);

            SetSpawnerColorBlue();
            SetBloodBlue();
            SetTrailBlue();
            SetEndTowerBlue();

            // Some Spawner Lights may appear later, so we may use this pointless coroutine for now.
            // Should be fixed by the SetSpawnerColorBlue function.
            MelonCoroutines.Start(ChangeSpawnerLights());
        }

        /// <summary>
        /// Assigns Material to all GameObjects containing the name,
        /// </summary>
        /// <param name="namePart"> Name to be contained by the GameObject </param>
        /// <param name="assignedMaterial"> Material to be assigned </param>
        private static void ProcessObjectsWithName(string namePart, Material assignedMaterial)
        {
            // Find all objects in the scene with the name.
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains(namePart))
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = assignedMaterial;
                    }
                }
            }
        }

        /// <summary>
        /// Sets color of the enemy spawner to blue.
        /// </summary>
        private static void SetSpawnerColorBlue()
        {
            // Find all objects in the scene, including inactive ones.
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("FrontSpotZcieniem") || obj.name.Contains("PointLightBezcieniowy") ||
                    obj.name.Contains("Point light na przeciwnika"))
                {
                    Light lightComponent = obj.GetComponent<Light>();
                    if (lightComponent != null)
                    {
                        obj.GetComponent<Light>().color = Color.blue;
                    }
                    else
                    {
                        MelonLogger.Warning("Could not find light component in enemy spawner.");
                    }
                }
            }
        }

        /// <summary>
        /// Sets color of the enemy blood to blue.
        /// </summary>
        private static void SetBloodBlue()
        {
            // Find all objects in the scene, including inactive ones.
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Particles - geometry") || obj.name.Contains("Particles - enemyballs"))
                {
                    ParticleSystem particleComponent = obj.GetComponent<ParticleSystem>();
                    ParticleSystemRenderer particleRendererComponent = obj.GetComponent<ParticleSystemRenderer>();
                    if (particleComponent != null)
                    {
                        // For now main doesn't work, so we directly use this deprecated way.

                        ParticleSystem.MainModule mainModule = particleComponent.main;

                        mainModule.startColor = Color.blue;
                        particleRendererComponent.material = blueEnemy;
                    }
                    else
                    {
                        MelonLogger.Warning("Could not find particle system component in enemy blood particle.");
                    }
                }
            }
        }

        /// <summary>
        /// Sets color of the bullet trail to blue.
        /// </summary>
        private static void SetTrailBlue()
        {
            // Find all objects in the scene, including inactive ones.
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Trail(Clone)"))
                {
                    MeshRenderer bulletTrail = obj.GetComponent<MeshRenderer>();
                    if (bulletTrail != null)
                    {
                        bulletTrail.material.color = Color.blue;
                    }
                    else
                    {
                        MelonLogger.Warning("WARNING: Could not find mesh renderer in bullet trail.");
                    }
                }
            }
        }

        /// <summary>
        /// Sets final tower of the last level to blue.
        /// </summary>
        private static void SetEndTowerBlue()
        {
            // Find all objects in the scene, including inactive ones.
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("piramida-krysztalowa-02"))
                {
                    MeshRenderer bulletTrail = obj.GetComponent<MeshRenderer>();
                    if (bulletTrail != null)
                    {
                        bulletTrail.material = blueEnemy;
                    }
                    else
                    {
                        MelonLogger.Warning("WARNING: Could not find mesh renderer in bullet trail.");
                    }
                }
            }
        }

        /// <summary>
        /// (Deprecated) Changes the enemy spawner lights when they spawn for the first time.
        /// Not useful as we can do it beforehand.
        /// </summary>
        private static IEnumerator ChangeSpawnerLights()
        {
            GameObject frontSpotZcieniem = null;

            while (frontSpotZcieniem == null)
            {
                yield return null; // Wait for the next frame
                frontSpotZcieniem = GameObject.Find("FrontSpotZcieniem");
            }

            SetSpawnerColorBlue();
        }
    }

    [HarmonyPatch(typeof(MaterialDefinition), "InitDictionaries")]
    public static class PatchMaterialSet
    {
        private static readonly FieldInfo Mat = typeof(MaterialDefinition).GetField("Material",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
        private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
        private static readonly int Metallic = Shader.PropertyToID("_Metallic");

        /// <summary>
        /// Patches Material definition to change the enemy material to blue.
        /// </summary>
        /// <param name="__instance"> Caller of function. </param>
        // ReSharper disable once UnusedMember.Local
        private static void Postfix(MaterialDefinition __instance)
        {
            Material currentMat = (Material)Mat.GetValue(__instance);

            if (currentMat.name == "MAIN RedCrystalNuEnemyMaterial")
            {
                // Create a new material with a default shader
                Material blueMaterial = new Material(Shader.Find("Standard"));

                blueMaterial.shaderKeywords = currentMat.shaderKeywords;
                blueMaterial.globalIlluminationFlags = currentMat.globalIlluminationFlags;

                // Set the main color of the material to blue
                blueMaterial.color = new Color(0.1f, 0.1f, 1f, 1f); // _Color set to blue

                // Set the emission color to blue to simulate a subtle glow
                blueMaterial.EnableKeyword("_EMISSION");

                // _EmissionColor set to blue
                blueMaterial.SetColor(EmissionColor, new Color(0.3f, 0.3f, 0.6f, 1f));

                // Set glossiness (similar to _Shininess and _Gloss)
                blueMaterial.SetFloat(Glossiness, 0.7f); // A combination of _Shininess and _Gloss values

                // Specular highlights color approximation (using Standard Shader's PBR model)
                // _SpecColor / _SuperSpecColor equivalent
                blueMaterial.SetColor(SpecColor, new Color(0.5f, 0.5f, 0.5f, 1f));
                blueMaterial.SetFloat(Metallic, 0.5f); // Approximating the specular effect

                // Simulate rim color with emission if needed (not directly supported by Standard Shader)
                Color rimColor = new Color(0.26f, 0.19f, 0.16f, 0f);

                // Simulate a subtle rim effect
                blueMaterial.SetColor(EmissionColor, blueMaterial.GetColor(EmissionColor) + rimColor * 0.3f);

                Mat.SetValue(__instance, blueMaterial);
            }
        }
    }

    [HarmonyPatch(typeof(StaticParticles), "Awake")]
    public static class PatchEnemySpawnEffectDefault
    {
        /// <summary>
        /// Patches the Static Particle class to replace the light colors of Enemy Spawner Particles.
        /// Note: I believe this is can be removed.
        /// </summary>
        /// <param name="__instance"> Caller of function. </param>
        // ReSharper disable once UnusedMember.Local
        private static void Postfix(StaticParticles __instance)
        {
            GameObject spawnEffectGameObject = __instance.gameObject;

            spawnEffectGameObject.transform.Find("FrontSpotZcieniem").GetComponent<Light>().color = Color.blue;
            spawnEffectGameObject.transform.Find("PointLightBezcieniowy").GetComponent<Light>().color = Color.blue;
        }
    }


    [HarmonyPatch(typeof(TextManager), "DisplaySingleWord", typeof(OverlayWord))]
    public static class PatchSuperhotTitle
    {
        /// <summary>
        /// Patches the super hot text after ending a level to supercold.
        /// </summary>
        /// <param name="word"> Word to be displayed. </param>
        // ReSharper disable once UnusedMember.Local
        private static void Prefix(ref OverlayWord word)
        {
            if (word != null)
            {
                if (word.txt.ToLower() == "hot")
                {
                    word.txt = "COLD";
                }
            }
        }
    }

    [HarmonyPatch(typeof(SHGUI), "SetPixelFront", typeof(char), typeof(int), typeof(int), typeof(char))]
    public static class PatchCursorBlue
    {
        /// <summary>
        /// Patches the cursor to be blue on front.
        /// </summary>
        /// <param name="col"> Color </param>
        // ReSharper disable once UnusedMember.Local
        private static void Prefix(ref char col)
        {
            if (col == 'r')
            {
                col = 'b';
            }
        }
    }

    [HarmonyPatch(typeof(SHGUI), "SetPixelBack", typeof(char), typeof(int), typeof(int), typeof(char))]
    public static class PatchSelectGUIBlue
    {
        /// <summary>
        /// Patches the cursor to be blue on back.
        /// </summary>
        /// <param name="col"> Color </param>
        // ReSharper disable once UnusedMember.Local
        private static void Prefix(ref char col)
        {
            if (col == 'r')
            {
                col = 'b';
            }
        }
    }

    [HarmonyPatch(typeof(SHGUI), "SetColorFront", typeof(char), typeof(int), typeof(int))]
    public static class PatchCursorBlue2
    {
        /// <summary>
        /// Patches set color.
        /// </summary>
        /// <param name="c"> Color </param>
        // ReSharper disable once UnusedMember.Local
        private static void Prefix(ref char c)
        {
            if (c == 'r')
            {
                c = 'b';
            }
        }
    }
}