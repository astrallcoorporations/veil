using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Veil.Interaction;

namespace Veil.LevelGen.Editor
{
    /// <summary>
    /// Procedurally builds the M1 mini stealth sandbox: greybox verticality (vault gap,
    /// mantle ledge, slide gap) plus sightline-blocking cover and the three required
    /// interactables. Code-driven so the level is reproducible and reviewable as a diff
    /// rather than hand-placed in the Editor.
    /// </summary>
    public static class LevelBuilder
    {
        private const float ArenaSize = 40f;

        /// <summary>Menu entry point that builds the layout into the currently active scene.</summary>
        [MenuItem("VEIL/Build M1 Test Level")]
        public static void BuildActiveScene() => Build(SceneManager.GetActiveScene());

        /// <summary>Populates the given scene with the M1 greybox layout.</summary>
        public static void Build(Scene scene)
        {
            var floor = CreateBlock("Floor", new Vector3(ArenaSize, 1f, ArenaSize), new Vector3(0f, -0.5f, 0f));
            SceneManager.MoveGameObjectToScene(floor, scene);

            var vaultGap = CreateBlock("VaultGap", new Vector3(2f, 0.8f, 0.5f), new Vector3(-8f, 0.4f, 0f));
            SceneManager.MoveGameObjectToScene(vaultGap, scene);

            var mantleLedge = CreateBlock("MantleLedge", new Vector3(4f, 1.8f, 1f), new Vector3(-2f, 0.9f, 6f));
            SceneManager.MoveGameObjectToScene(mantleLedge, scene);

            var slideGapTop = CreateBlock("SlideGap", new Vector3(3f, 0.6f, 1.5f), new Vector3(4f, 1.3f, -6f));
            SceneManager.MoveGameObjectToScene(slideGapTop, scene);

            var cover1 = CreateBlock("CoverCrate_1", new Vector3(1f, 1f, 1f), new Vector3(3f, 0.5f, 3f));
            SceneManager.MoveGameObjectToScene(cover1, scene);
            var cover2 = CreateBlock("CoverPillar_1", new Vector3(0.8f, 3f, 0.8f), new Vector3(-5f, 1.5f, -4f));
            SceneManager.MoveGameObjectToScene(cover2, scene);

            var doorGo = CreateBlock("Door", new Vector3(1.2f, 2f, 0.1f), new Vector3(10f, 1f, 0f));
            doorGo.AddComponent<Door>();
            SceneManager.MoveGameObjectToScene(doorGo, scene);

            var leverGo = CreateBlock("Lever", new Vector3(0.2f, 0.5f, 0.2f), new Vector3(9f, 0.75f, 1.5f));
            leverGo.AddComponent<Lever>();
            SceneManager.MoveGameObjectToScene(leverGo, scene);

            var pickupGo = CreateBlock("Pickup", new Vector3(0.3f, 0.3f, 0.3f), new Vector3(0f, 0.15f, -2f));
            pickupGo.AddComponent<Pickup>();
            SceneManager.MoveGameObjectToScene(pickupGo, scene);

            var grabbableGo = CreateBlock("GrabbableCrate", new Vector3(0.5f, 0.5f, 0.5f), new Vector3(2f, 0.25f, 2f));
            var rb = grabbableGo.AddComponent<Rigidbody>();
            rb.mass = 5f;
            grabbableGo.AddComponent<GrabbableObject>();
            SceneManager.MoveGameObjectToScene(grabbableGo, scene);

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player.prefab");
            if (playerPrefab != null)
            {
                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = new Vector3(0f, 1f, -10f);
                SceneManager.MoveGameObjectToScene(player, scene);
            }
        }

        /// <summary>
        /// Batchmode entry point: creates a fresh scene, builds the M1 greybox layout
        /// (including the Player prefab when present), and saves it as
        /// <c>Assets/_Project/Levels/M1_StealthSandbox.unity</c>.
        /// </summary>
        public static void BuildAndSaveM1Scene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            Build(scene);
            System.IO.Directory.CreateDirectory("Assets/_Project/Levels");
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/_Project/Levels/M1_StealthSandbox.unity");
        }

        private static GameObject CreateBlock(string name, Vector3 scale, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.localScale = scale;
            go.transform.position = position;
            return go;
        }
    }
}
