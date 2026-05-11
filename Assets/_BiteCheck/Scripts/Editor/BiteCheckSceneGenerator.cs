using BiteCheck.Core;
using BiteCheck.Input;
using BiteCheck.Systems;
using BiteCheck.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BiteCheck.Editor
{
    public static class BiteCheckSceneGenerator
    {
        private const string ScenePath = "Assets/_BiteCheck/Scenes/MainScene.unity";
        private const string MaterialsFolder = "Assets/_BiteCheck/Materials";

        [MenuItem("Tools/Bite Check/Create Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            EnsureProjectFolders();

            Material groundMaterial = CreateOrUpdateMaterial("Ground_Mat", new Color(0.22f, 0.24f, 0.22f));
            Material quarantineMaterial = CreateOrUpdateMaterial("QuarantineZone_Mat", new Color(0.62f, 0.18f, 0.16f));
            Material shelterMaterial = CreateOrUpdateMaterial("ShelterGate_Mat", new Color(0.18f, 0.44f, 0.7f));
            CreateOrUpdateMaterial("HumanDebug_Mat", new Color(0.35f, 0.75f, 0.45f));
            CreateOrUpdateMaterial("InfectedDebug_Mat", new Color(0.65f, 0.45f, 0.75f));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainScene";

            Camera mainCamera = CreateCamera();
            CreateLight();
            CreateGround(groundMaterial);
            Transform spawnPoint = CreatePoint("SpawnPoint", new Vector3(0f, 0f, 6f));
            Transform decisionPoint = CreatePoint("DecisionPoint", new Vector3(0f, 0f, -1.5f));
            Transform quarantineZone = CreateZone("LeftQuarantineZone", new Vector3(-3.5f, 0.05f, -1.5f), new Vector3(2f, 0.1f, 3f), quarantineMaterial);
            Transform shelterGate = CreateZone("RightShelterGate", new Vector3(3.5f, 1f, -1.5f), new Vector3(2f, 2f, 3f), shelterMaterial);

            StatsManager statsManager = new GameObject("StatsManager").AddComponent<StatsManager>();
            UpgradeSystem upgradeSystem = new GameObject("UpgradeSystem").AddComponent<UpgradeSystem>();
            SwipeInputController swipeInput = new GameObject("SwipeInputController").AddComponent<SwipeInputController>();
            RoundManager roundManager = new GameObject("RoundManager").AddComponent<RoundManager>();
            GameManager gameManager = new GameObject("GameManager").AddComponent<GameManager>();

            GameObject canvasObject = CreateCanvas();
            UIManager uiManager = canvasObject.AddComponent<UIManager>();
            AudioSource feedbackAudioSource = canvasObject.AddComponent<AudioSource>();
            FeedbackEffectsController feedbackEffects = canvasObject.AddComponent<FeedbackEffectsController>();
            CreateEventSystemIfNeeded();

            SetObjectReference(roundManager, "swipeInput", swipeInput);
            SetObjectReference(roundManager, "spawnPoint", spawnPoint);
            SetObjectReference(roundManager, "decisionPoint", decisionPoint);
            SetObjectReference(roundManager, "quarantineZone", quarantineZone);
            SetObjectReference(roundManager, "shelterGate", shelterGate);
            SetObjectReference(roundManager, "upgradeSystem", upgradeSystem);
            SetObjectReference(upgradeSystem, "statsManager", statsManager);
            SetObjectReference(gameManager, "statsManager", statsManager);
            SetObjectReference(gameManager, "roundManager", roundManager);
            SetObjectReference(gameManager, "upgradeSystem", upgradeSystem);
            SetBool(gameManager, "startOnAwake", false);
            SetObjectReference(uiManager, "statsManager", statsManager);
            SetObjectReference(uiManager, "roundManager", roundManager);
            SetObjectReference(uiManager, "gameManager", gameManager);
            SetObjectReference(uiManager, "upgradeSystem", upgradeSystem);
            SetObjectReference(feedbackEffects, "roundManager", roundManager);
            SetObjectReference(feedbackEffects, "statsManager", statsManager);
            SetObjectReference(feedbackEffects, "targetCamera", mainCamera);
            SetObjectReference(feedbackEffects, "audioSource", feedbackAudioSource);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = gameManager;
            Debug.Log($"Bite Check prototype scene generated at {ScenePath}");
        }

        private static void EnsureProjectFolders()
        {
            EnsureFolder("Assets", "_BiteCheck");
            EnsureFolder("Assets/_BiteCheck", "Materials");
            EnsureFolder("Assets/_BiteCheck", "Scenes");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Material CreateOrUpdateMaterial(string materialName, Color color)
        {
            string path = $"{MaterialsFolder}/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(FindDefaultShader());
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader FindDefaultShader()
        {
            return Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Sprites/Default");
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.orthographic = true;
            camera.orthographicSize = 4.4f;
            cameraObject.transform.position = new Vector3(0f, 3.2f, -5.5f);
            cameraObject.transform.rotation = Quaternion.Euler(24f, 0f, 0f);
            return camera;
        }

        private static void CreateLight()
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateGround(Material material)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(1.4f, 1f, 1.6f);
            ground.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Transform CreatePoint(string name, Vector3 position)
        {
            GameObject point = new GameObject(name);
            point.transform.position = position;

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Marker";
            marker.transform.SetParent(point.transform, false);
            marker.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            marker.transform.localScale = new Vector3(0.25f, 0.03f, 0.25f);

            return point.transform;
        }

        private static Transform CreateZone(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zone.name = name;
            zone.transform.position = position;
            zone.transform.localScale = scale;
            zone.GetComponent<Renderer>().sharedMaterial = material;

            Collider zoneCollider = zone.GetComponent<Collider>();
            if (zoneCollider != null)
            {
                Object.DestroyImmediate(zoneCollider);
            }

            return zone.transform;
        }

        private static GameObject CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<SafeAreaFitter>();

            Component canvasScaler = AddOptionalComponent(canvasObject, "UnityEngine.UI.CanvasScaler, UnityEngine.UI");
            ConfigureCanvasScaler(canvasScaler);
            AddOptionalComponent(canvasObject, "UnityEngine.UI.GraphicRaycaster, UnityEngine.UI");
            return canvasObject;
        }

        private static void CreateEventSystemIfNeeded()
        {
            System.Type eventSystemType = FindType(
                "UnityEngine.EventSystems.EventSystem, UnityEngine.UI",
                "UnityEngine.EventSystems.EventSystem, UnityEngine.EventSystems");
            System.Type inputModuleType = FindType(
                "UnityEngine.EventSystems.StandaloneInputModule, UnityEngine.UI",
                "UnityEngine.EventSystems.StandaloneInputModule, UnityEngine.EventSystems");

            if (eventSystemType == null || HasObjectOfType(eventSystemType))
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent(eventSystemType);

            if (inputModuleType != null)
            {
                eventSystemObject.AddComponent(inputModuleType);
            }
        }

        private static Component AddOptionalComponent(GameObject target, string typeName)
        {
            System.Type type = System.Type.GetType(typeName);

            if (type != null)
            {
                return target.AddComponent(type);
            }

            return null;
        }

        private static void ConfigureCanvasScaler(Component canvasScaler)
        {
            if (canvasScaler == null)
            {
                return;
            }

            SetEnumProperty(canvasScaler, "uiScaleMode", "ScaleWithScreenSize");
            SetProperty(canvasScaler, "referenceResolution", new Vector2(1080f, 1920f));
            SetEnumProperty(canvasScaler, "screenMatchMode", "MatchWidthOrHeight");
            SetProperty(canvasScaler, "matchWidthOrHeight", 0.5f);
        }

        private static void SetProperty(Component target, string propertyName, object value)
        {
            System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);

            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value);
            }
        }

        private static void SetEnumProperty(Component target, string propertyName, string enumValue)
        {
            System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);

            if (property == null || !property.CanWrite || !property.PropertyType.IsEnum)
            {
                return;
            }

            object value = System.Enum.Parse(property.PropertyType, enumValue);
            property.SetValue(target, value);
        }

        private static bool HasObjectOfType(System.Type type)
        {
            Object[] sceneObjects = Object.FindObjectsByType(
                typeof(Object),
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < sceneObjects.Length; i++)
            {
                if (sceneObjects[i] != null && type.IsInstanceOfType(sceneObjects[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static System.Type FindType(params string[] typeNames)
        {
            for (int i = 0; i < typeNames.Length; i++)
            {
                System.Type type = System.Type.GetType(typeNames[i]);

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"Could not find serialized property '{propertyName}' on {target.name}.", target);
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"Could not find serialized property '{propertyName}' on {target.name}.", target);
                return;
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
