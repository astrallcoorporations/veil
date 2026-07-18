using System.IO;
using UnityEditor;
using UnityEngine;
using Veil.Camera;
using Veil.Input;
using Veil.Interaction;
using Veil.Movement;

namespace Veil.LevelGen.Editor
{
    /// <summary>Code-driven builder for `Player.prefab` so the wiring between every M1 system is reproducible, not hand-clicked.</summary>
    public static class PlayerPrefabBuilder
    {
        private const string SettingsFolder = "Assets/_Project/Settings";
        private const string PrefabPath = "Assets/_Project/Prefabs/Player.prefab";

        [MenuItem("VEIL/Build Player Prefab")]
        public static void Build()
        {
            var movementSettings = LoadOrCreate<MovementSettings>($"{SettingsFolder}/DefaultMovementSettings.asset");
            var cameraSettings = LoadOrCreate<CameraSettings>($"{SettingsFolder}/DefaultCameraSettings.asset");
            var inputReader = LoadOrCreate<InputReader>($"{SettingsFolder}/DefaultInputReader.asset");

            var root = new GameObject("Player");
            root.AddComponent<CapsuleCollider>();
            var motor = root.AddComponent<CharacterMotor>();
            motor.Settings = movementSettings;

            var cameraRig = new GameObject("CameraRig");
            cameraRig.transform.SetParent(root.transform);
            cameraRig.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var cam = cameraRig.AddComponent<UnityEngine.Camera>();

            var cameraController = cameraRig.AddComponent<CameraController>();
            var camControllerSo = new SerializedObject(cameraController);
            camControllerSo.FindProperty("targetCamera").objectReferenceValue = cam;
            camControllerSo.FindProperty("settings").objectReferenceValue = cameraSettings;
            camControllerSo.FindProperty("motor").objectReferenceValue = motor;
            camControllerSo.ApplyModifiedPropertiesWithoutUndo();

            var interactionCaster = cameraRig.AddComponent<InteractionCaster>();
            var casterSo = new SerializedObject(interactionCaster);
            casterSo.FindProperty("eye").objectReferenceValue = cam;
            casterSo.FindProperty("input").objectReferenceValue = inputReader;
            casterSo.ApplyModifiedPropertiesWithoutUndo();

            var holdPoint = new GameObject("HoldPoint").transform;
            holdPoint.SetParent(cameraRig.transform);
            holdPoint.localPosition = new Vector3(0f, 0f, 1.2f);

            var grabController = root.AddComponent<GrabController>();
            var grabSo = new SerializedObject(grabController);
            grabSo.FindProperty("input").objectReferenceValue = inputReader;
            grabSo.FindProperty("holdPoint").objectReferenceValue = holdPoint;
            grabSo.ApplyModifiedPropertiesWithoutUndo();

            var playerController = root.AddComponent<PlayerController>();
            var playerSo = new SerializedObject(playerController);
            playerSo.FindProperty("motor").objectReferenceValue = motor;
            playerSo.FindProperty("input").objectReferenceValue = inputReader;
            playerSo.FindProperty("movementSettings").objectReferenceValue = movementSettings;
            playerSo.FindProperty("cameraController").objectReferenceValue = cameraController;
            playerSo.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory("Assets/_Project/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            Directory.CreateDirectory(SettingsFolder);
            var instance = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }
    }
}
