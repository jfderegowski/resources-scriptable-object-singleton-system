using UnityEngine;

namespace Runtime
{
    /// <summary>
    /// Base class for all ScriptableObjects that are loaded from the Resources folder.
    /// </summary>
    public class ResourcesScriptableObject : ScriptableObject
    {
        /// <summary>
        /// Array of all instances of ResourcesScriptableObject in the Resources folder.
        /// </summary>
        private static ResourcesScriptableObject[] Instances
        {
            get
            {
                if (_instances == null)
                    RefreshInstances();

                return _instances;
            }
        }

        /// <summary>
        /// Array of all instances of ResourcesScriptableObject in the Resources folder.
        /// </summary>
        private static ResourcesScriptableObject[] _instances;

        /// <summary>
        /// Refreshes the array of instances of ResourcesScriptableObject in the Resources folder.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RefreshInstances() =>
            _instances = Resources.LoadAll<ResourcesScriptableObject>("");

        /// <summary>
        /// Tries to get the instance of the specified type from the array of instances.
        /// </summary>
        /// <param name="instance">The instance of the specified type.</param>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <returns>True if the instance was found, false otherwise.</returns>
        private protected static bool TryGetFromInstances<T>(out T instance) where T : ResourcesScriptableObject<T>
        {
            instance = GetFromInstances<T>();

            if (instance) return true;

            RefreshInstances();

            instance = GetFromInstances<T>();

            return instance;
        }

        /// <summary>
        /// Gets the instance of the specified type from the array of instances.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <returns>The instance of the specified type.</returns>
        private static T GetFromInstances<T>() where T : ResourcesScriptableObject<T>
        {
#if UNITY_EDITOR
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
#endif

            foreach (var instance in Instances)
            {
                if (instance is not T tInstance) continue;

                return tInstance;
            }

            RefreshInstances();

            foreach (var instance in Instances)
            {
                if (instance is not T tInstance) continue;

                return tInstance;
            }

            return null;
        }

        /// <summary>
        /// Call base to validate the ScriptableObject and check if it is in the Resources folder.
        /// </summary>
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (!UnityEditor.AssetDatabase.GetAssetPath(this).Contains("Resources"))
                Debug.LogError($"ScriptableObject {name} is not in Resources folder. " +
                               "Please move it to the Resources folder.", this);
#endif
        }
    }

    /// <summary>
    /// Generic version of ResourcesScriptableObject.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    public class ResourcesScriptableObject<T> : ResourcesScriptableObject where T : ResourcesScriptableObject<T>
    {
        /// <summary>
        /// Gets the instance of the specified type.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance) return _instance;

                if (TryGetFromInstances(out T instance))
                    return _instance = instance;
                
#if UNITY_EDITOR
                _instance = CreateScriptableObject();
#endif
                
                return _instance;
            }
        }

        /// <summary>
        /// The instance of the specified type.
        /// </summary>
        private static T _instance;
        
#if UNITY_EDITOR
        /// <summary>
        /// Creates a new ScriptableObject of the specified type and saves it in the Resources folder.
        /// </summary>
        /// <returns>The created ScriptableObject instance.</returns>
        private static T CreateScriptableObject()
        {
            var instance = CreateInstance<T>();

            instance.name = typeof(T).Name;

            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");

            UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/Resources/" + typeof(T).Name + ".asset");

            UnityEditor.AssetDatabase.SaveAssets();

            return instance;
        }
#endif
    }
}
