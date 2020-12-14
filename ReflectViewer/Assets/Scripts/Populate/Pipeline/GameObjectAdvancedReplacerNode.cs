using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Pipeline
{
    [Serializable]
    public class GameObjectAdvancedReplacerNodeSettings
    {
        [Serializable]
        public class ReplacementEntry
        {
            public string category;
            public string family;
            public GameObject prefab;
            public float minSize = 1.0f;
            public bool matchHeight = true;
        }

        public bool enableReplacements = true;
        public List<ReplacementEntry> entries;
    }

    public class GameObjectAdvancedReplacerNode : ReflectNode<GameObjectAdvancedReplacer>
    {
        public GameObjectInput input = new GameObjectInput();

        [SerializeField]
        ExposedReference<Transform> m_Root;

        public GameObjectAdvancedReplacerNodeSettings settings;

        public void SetRoot(Transform root, IExposedPropertyTable resolver)
        {
            resolver.SetReferenceValue(m_Root.exposedName, root);
        }

        protected override GameObjectAdvancedReplacer Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var root = m_Root.Resolve(resolver);
            if (root == null)
            {
                root = new GameObject("replacement root").transform;
            }

            var node = new GameObjectAdvancedReplacer(root, settings);
            input.streamEvent = node.OnGameObjectEvent;
            return node;
        }
    }

    public class GameObjectAdvancedReplacer : IReflectNodeProcessor
    {
        readonly GameObjectAdvancedReplacerNodeSettings m_Settings;
        readonly Transform m_Root;

        Dictionary<GameObject, GameObject> replacements = new Dictionary<GameObject, GameObject>();

        public GameObjectAdvancedReplacer(Transform root, GameObjectAdvancedReplacerNodeSettings settings)
        {
            m_Root = root;
            m_Settings = settings;
        }

        public void OnGameObjectEvent(SyncedData<GameObject> stream, StreamEvent streamEvent)
        {
            if (streamEvent == StreamEvent.Added)
            {
                var gameObject = stream.data;

                if (replacements.ContainsKey(gameObject))
                    return;

                if (!gameObject.TryGetComponent(out Metadata metadata))
                    return;

                if (!metadata.parameters.dictionary.TryGetValue("Category", out var category))
                    return;

                if (!metadata.parameters.dictionary.TryGetValue("Family", out var family))
                    return;

                float heightValue = 0;
                var hasHeight = metadata.parameters.dictionary.TryGetValue("Height", out var height);
                var hasValidHeight = hasHeight ? float.TryParse(height.value, out heightValue) : false;
                heightValue *= 0.001f;

                foreach (var entry in m_Settings.entries)
                {
                    if (category.value.Contains(entry.category) && family.value.Contains(entry.family))
                    {
                        if (entry.minSize > 0 && hasValidHeight && entry.minSize > heightValue)
                            return;

                        var obj = Object.Instantiate(entry.prefab, gameObject.transform.position, gameObject.transform.rotation, m_Root);
                        
                        replacements.Add(gameObject, obj);

                        if (entry.matchHeight)
                        {
                            Bounds prefabBounds = new Bounds();
                            foreach (MeshFilter m in entry.prefab.GetComponentsInChildren<MeshFilter>())
                                prefabBounds.Encapsulate(m.sharedMesh.bounds);

                            obj.transform.localScale = Vector3.one * (heightValue / prefabBounds.size.y);
                        }
                        return;
                    }
                }
            }
            else if (streamEvent == StreamEvent.Removed)
            {
                GameObject.Destroy(replacements[stream.data]);
                replacements.Remove(stream.data);
            }
            else if (streamEvent == StreamEvent.Changed)
            {
                replacements[stream.data].transform.SetPositionAndRotation(stream.data.transform.position, stream.data.transform.rotation);
            }
        }

        public void RefreshObjects()
        {
            m_Root.gameObject.SetActive(m_Settings.enableReplacements);
        }

        public void OnPipelineInitialized()
        {
            m_Root.gameObject.SetActive(m_Settings.enableReplacements);
        }

        public void OnPipelineShutdown()
        {
            replacements.Clear();
        }
    }
}
