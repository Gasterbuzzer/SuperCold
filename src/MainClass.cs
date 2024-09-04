using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;
using HarmonyLib;
using UnityEngine;
using SuperhotAssets;
using System.Collections;
using static RootMotion.FinalIK.IKSolver;
using static System.Net.Mime.MediaTypeNames;

namespace SuperCold
{

    public static class BuildInfo
    {
        public const string Name = "SuperCold";
        public const string Description = "Mod for randomizing weapons on player and enemies.";
        public const string Author = "Gasterbuzzer";
        public const string Company = null;
        public const string Version = "1.0.0";
        public const string DownloadLink = "https://github.com/Gasterbuzzer/SuperCold/releases/";
    }

    public class MainClass : MelonMod
    {
        public static Material greyWall;
        public static Material purpleGun;
        public static Material blueEnemy;

        // Create Materials for later replacing.
        public override void OnLateInitializeMelon()
        {
            greyWall = new Material(Shader.Find("Standard"));
            greyWall.color = new Color(0.176f, 0.188f, 0.212f, 1f);

            purpleGun = new Material(Shader.Find("Standard"));
            purpleGun.color = new Color(0.82f, 0.118f, 0.761f, 1f);

            blueEnemy = new Material(Shader.Find("Standard"));

            // Set the main color of the material to blue
            blueEnemy.color = new Color(0.1f, 0.1f, 1f, 1f);  // _Color set to blue

            // Set the emission color to blue to simulate a subtle glow
            blueEnemy.EnableKeyword("_EMISSION");
            blueEnemy.SetColor("_EmissionColor", new Color(0.3f, 0.3f, 0.6f, 1f));  // _EmissionColor set to blue

            // Set glossiness (similar to _Shininess and _Gloss)
            blueEnemy.SetFloat("_Glossiness", 0.7f);  // A combination of _Shininess and _Gloss values

            // Specular highlights color approximation (using Standard Shader's PBR model)
            blueEnemy.SetColor("_SpecColor", new Color(0.5f, 0.5f, 0.5f, 1f));  // _SpecColor / _SuperSpecColor equivalent
            blueEnemy.SetFloat("_Metallic", 0.5f); // Approximating the specular effect

            // Simulate rim color with emission if needed (not directly supported by Standard Shader)
            Color rimColor = new Color(0.26f, 0.19f, 0.16f, 0f);
            blueEnemy.SetColor("_EmissionColor", blueEnemy.GetColor("_EmissionColor") + rimColor * 0.3f);  // Simulate a subtle rim effect
        }

        // On Game Scene Change, replace material of walls and objects with the following names:
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {

            MelonLogger.Msg($"New Scene Loaded: {sceneName}.");

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
        /// Assings Material to all GameObjects containing the name,
        /// </summary>
        /// <param name="namePart"> Name to be contained by the GameObject </param>
        /// <param name="assignedMaterial"> Material to be assigned </param>
        public static void ProcessObjectsWithName(string namePart, Material assignedMaterial)
        {
            // Find all objects in the scene with the name,.
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

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
        public static void SetSpawnerColorBlue()
        {
            // Find all objects in the scene, including inactive ones.
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("FrontSpotZcieniem") || obj.name.Contains("PointLightBezcieniowy") || obj.name.Contains("Point light na przeciwnika"))
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
        public static void SetBloodBlue()
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
                        particleComponent.startColor = Color.blue;
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
        public static void SetTrailBlue()
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
                        MelonLogger.Warning("Could not find mesh renderer in bullet trail.");
                    }
                }
            }
        }

        /// <summary>
        /// Sets final tower of the last level to blue.
        /// </summary>
        public static void SetEndTowerBlue()
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
                        MelonLogger.Warning("Could not find mesh renderer in bullet trail.");
                    }
                }
            }
        }

        /// <summary>
        /// (Deprecated) Changes the enemy spawner lights when they spawn for the first time. Not usefull as we can do it beforehand.
        /// </summary>
        public static IEnumerator ChangeSpawnerLights()
        {
            GameObject frontSpotZcieniem = null;

            while (frontSpotZcieniem == null)
            {
                yield return null; // Wait for the next frame
                frontSpotZcieniem = GameObject.Find("FrontSpotZcieniem");
            }

            MelonLogger.Msg("Found Spawner Lights Available.");

            SetSpawnerColorBlue();
        }
    }

    [HarmonyPatch(typeof(MaterialDefinition), "InitDictionaries", new Type[] { })]
    public static class PatchMaterialSet
    {

        /// <summary>
        /// Patches Material Defintion to change the enemy material to blue.
        /// </summary>
        /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
        /// <param name="__instance"> Caller of function. </param>
        private static void Postfix(MethodBase __originalMethod, object __instance)
        {
            Type classType = __originalMethod.DeclaringType;
            FieldInfo mat = classType.GetField("Material", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Material currentMat = (Material)mat.GetValue(__instance);

            if (currentMat.name == "MAIN RedCrystalNuEnemyMaterial")
            {

                // Create a new material with a default shader
                Material blueMaterial = new Material(Shader.Find("Standard"));

                blueMaterial.shaderKeywords = currentMat.shaderKeywords;
                blueMaterial.globalIlluminationFlags = currentMat.globalIlluminationFlags;

                // Set the main color of the material to blue
                blueMaterial.color = new Color(0.1f, 0.1f, 1f, 1f);  // _Color set to blue

                // Set the emission color to blue to simulate a subtle glow
                blueMaterial.EnableKeyword("_EMISSION");
                blueMaterial.SetColor("_EmissionColor", new Color(0.3f, 0.3f, 0.6f, 1f));  // _EmissionColor set to blue

                // Set glossiness (similar to _Shininess and _Gloss)
                blueMaterial.SetFloat("_Glossiness", 0.7f);  // A combination of _Shininess and _Gloss values

                // Specular highlights color approximation (using Standard Shader's PBR model)
                blueMaterial.SetColor("_SpecColor", new Color(0.5f, 0.5f, 0.5f, 1f));  // _SpecColor / _SuperSpecColor equivalent
                blueMaterial.SetFloat("_Metallic", 0.5f); // Approximating the specular effect

                // Simulate rim color with emission if needed (not directly supported by Standard Shader)
                Color rimColor = new Color(0.26f, 0.19f, 0.16f, 0f);
                blueMaterial.SetColor("_EmissionColor", blueMaterial.GetColor("_EmissionColor") + rimColor * 0.3f);  // Simulate a subtle rim effect

                mat.SetValue(__instance, blueMaterial);
            }

        }
    }

    [HarmonyPatch(typeof(StaticParticles), "Awake", new Type[] { })]
    public static class PatchEnemySpawnEffectDefault
    {

        /// <summary>
        /// Patches the Static Particle class to replace the light colors of Enemy Spawner Particles. Note: I believe this is can be removed.
        /// </summary>
        /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
        /// <param name="__instance"> Caller of function. </param>
        private static void Postfix(MethodBase __originalMethod, object __instance)
        {
            Type classType = __originalMethod.DeclaringType;

            GameObject spawnEffectGameObject = ((MonoBehaviour)__instance).gameObject;

            spawnEffectGameObject.transform.Find("FrontSpotZcieniem").GetComponent<Light>().color = Color.blue;
            spawnEffectGameObject.transform.Find("PointLightBezcieniowy").GetComponent<Light>().color = Color.blue;
        }
    }


    [HarmonyPatch(typeof(TextManager), "DisplaySingleWord", new Type[] {typeof(OverlayWord) })]
    public static class PatchSuperhotTitle
    {

        /// <summary>
        /// Patches the super hot text after ending a level to supercold.
        /// </summary>
        /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
        /// <param name="__instance"> Caller of function. </param>
        /// <param name="word"> Word to be displayed. </param>
        private static void Prefix(MethodBase __originalMethod, object __instance, ref OverlayWord word)
        {
            if (word != null)
            {
                if (word.txt.ToLower() == "hot")
                {
                    word.txt = "COLD";
                }
            }
            else
            {
                MelonLogger.Warning($"Empty Word displayed. Can safely be ignored.");
            }

            
        }
    }

    [HarmonyPatch(typeof(SHGUI), "SetPixelFront", new Type[] {typeof(char), typeof(int), typeof(int), typeof(char) })]
    public static class PatchCursorBlue
    {
        /// <summary>
        /// Patches the the cursor to be blue on front.
        /// </summary>
        /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
        /// <param name="__instance"> Caller of function. </param>
        /// <param name="C"> Character </param>
        /// <param name="x"> X Postion </param>
        /// <param name="y"> Y Position </param>
        /// <param name="col"> Color </param>
        private static void Prefix(MethodBase __originalMethod, object __instance, ref char C, ref int x, ref int y, ref char col)
        {
            if (col == 'r')
            {
                col = 'b';
            }
        }
    }

    [HarmonyPatch(typeof(SHGUI), "SetPixelBack", new Type[] { typeof(char), typeof(int), typeof(int), typeof(char) })]
    public static class PatchSelectGUIBlue
    {
        /// <summary>
        /// Patches the the cursor to be blue on back.
        /// </summary>
        /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
        /// <param name="__instance"> Caller of function. </param>
        /// <param name="C"> Character </param>
        /// <param name="x"> X Postion </param>
        /// <param name="y"> Y Position </param>
        /// <param name="col"> Color </param>
        private static void Prefix(MethodBase __originalMethod, object __instance, ref char C, ref int x, ref int y, ref char col)
        {
            if (col == 'r')
            {
                col = 'b';
            }
        }
    }

    [HarmonyPatch(typeof(SHGUI), "SetColorFront", new Type[] { typeof(char), typeof(int), typeof(int)})]
    public static class PatchCursorBlue2
    {
        /// <summary>
        /// Patches set color.
        /// </summary>
        /// <param name="__originalMethod"> Method which was called (Used to get class type.) </param>
        /// <param name="__instance"> Caller of function. </param>
        /// <param name="c"> Color </param>
        /// <param name="x"> X Postion </param>
        /// <param name="y"> Y Position </param>
        private static void Prefix(MethodBase __originalMethod, object __instance, ref char c, ref int x, ref int y)
        {
            if (c == 'r')
            {
                c = 'b';
            }
        }
    }
}
