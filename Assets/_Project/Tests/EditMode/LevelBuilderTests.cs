using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
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
            Assert.IsTrue(roots.Any(g => g.name == "VaultGap"), "Missing VaultGap");
            Assert.IsTrue(roots.Any(g => g.name == "MantleLedge"), "Missing MantleLedge");
            Assert.IsTrue(roots.Any(g => g.name == "SlideGap"), "Missing SlideGap");
            Assert.IsTrue(roots.Any(g => g.GetComponentInChildren<Veil.Interaction.Door>() != null), "Missing Door");
            Assert.IsTrue(roots.Any(g => g.GetComponentInChildren<Veil.Interaction.Lever>() != null), "Missing Lever");
            Assert.IsTrue(roots.Any(g => g.GetComponentInChildren<Veil.Interaction.GrabbableObject>() != null), "Missing GrabbableObject");

            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
