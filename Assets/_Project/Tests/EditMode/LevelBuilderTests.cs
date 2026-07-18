using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Veil.Interaction;
using Veil.LevelGen.Editor;

namespace Veil.Tests.EditMode
{
    public class LevelBuilderTests
    {
        [Test]
        public void Build_CreatesRequiredGreyboxElements()
        {
            // UnityEngine.SceneManagement.SceneManager.CreateScene is play-mode-only; EditMode
            // tests run outside play mode. EditorSceneManager.NewScene(..., Additive) also fails
            // here because the batchmode test runner's active scene is an untitled unsaved scene.
            // EditorSceneManager.NewPreviewScene() creates an isolated in-memory scene that is not
            // part of the editor's open-scene set, which is the standard pattern for Editor tests
            // that need a throwaway Scene without touching editor state.
            var scene = EditorSceneManager.NewPreviewScene();
            LevelBuilder.Build(scene);

            var roots = scene.GetRootGameObjects();
            Assert.IsTrue(roots.Any(g => g.name == "SpawnPlatform"), "Missing SpawnPlatform");
            Assert.IsTrue(roots.Any(g => g.name == "SlideGap"), "Missing SlideGap");
            Assert.IsTrue(roots.Any(g => g.name == "VaultObstacle"), "Missing VaultObstacle");
            Assert.IsTrue(roots.Any(g => g.name == "JumpGapCatchNet"), "Missing JumpGapCatchNet");
            Assert.IsTrue(roots.Any(g => g.name == "MantleLedge"), "Missing MantleLedge");
            Assert.IsTrue(roots.Any(g => g.name == "CoverPillar_Stealth1"), "Missing CoverPillar_Stealth1");
            Assert.IsTrue(roots.Any(g => g.name == "CoverCrate_Stealth1"), "Missing CoverCrate_Stealth1");
            Assert.IsTrue(roots.Any(g => g.name == "CoverPillar_Stealth2"), "Missing CoverPillar_Stealth2");
            Assert.IsTrue(roots.Any(g => g.name == "CoverCrate_Stealth2"), "Missing CoverCrate_Stealth2");
            Assert.IsTrue(roots.Any(g => g.GetComponentInChildren<Door>() != null), "Missing Door");
            Assert.IsTrue(roots.Any(g => g.GetComponentInChildren<Lever>() != null), "Missing Lever");
            Assert.IsTrue(roots.Any(g => g.GetComponentInChildren<Pickup>() != null), "Missing Pickup");
            Assert.IsTrue(roots.Any(g => g.name == "TraversalShelf"), "Missing TraversalShelf");
            Assert.IsTrue(roots.Any(g => g.name == "GrabbableCrate" && g.GetComponentInChildren<GrabbableObject>() != null), "Missing GrabbableCrate");
            Assert.IsTrue(roots.Any(g => g.name == "FinalOverlook"), "Missing FinalOverlook");

            EditorSceneManager.ClosePreviewScene(scene);
        }

        [Test]
        public void Build_WiresLeverToDoor_PullingLeverTogglesDoorOpenState()
        {
            var scene = EditorSceneManager.NewPreviewScene();
            LevelBuilder.Build(scene);

            var roots = scene.GetRootGameObjects();
            var door = roots.Select(g => g.GetComponentInChildren<Door>()).First(d => d != null);
            var lever = roots.Select(g => g.GetComponentInChildren<Lever>()).First(l => l != null);
            var link = roots.Select(g => g.GetComponentInChildren<DoorLeverLink>()).FirstOrDefault(l => l != null);

            Assert.IsNotNull(link, "Missing DoorLeverLink wiring the Lever to the Door");

            // EditorSceneManager.NewPreviewScene() (used above, per the class-level comment on
            // the other test) is an isolated in-memory scene that Unity does not tick: neither
            // Awake nor OnEnable fire automatically for MonoBehaviours placed in it, even across
            // an inactive->active transition, which was confirmed empirically while debugging
            // this test. In a real loaded scene (or Play mode) OnEnable fires normally the moment
            // LevelBuilder activates the link's GameObject, so this reflection call simply does,
            // by hand, what Unity's own engine loop would otherwise do automatically -- it is not
            // a workaround for a bug in DoorLeverLink, it's a stand-in for the scene ticking that
            // this isolated preview scene doesn't provide.
            var onEnable = typeof(DoorLeverLink).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
            onEnable.Invoke(link, null);

            bool openBefore = door.IsOpen;
            lever.Interact(null);

            Assert.AreNotEqual(openBefore, door.IsOpen, "Pulling the lever did not flip the door's open state -- the wiring isn't real");

            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
