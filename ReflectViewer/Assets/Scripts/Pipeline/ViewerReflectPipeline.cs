using System;
using System.Collections.Generic;
using Unity.Reflect;
using Unity.Reflect.IO;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.StandAloneViewer;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    public class ViewerReflectPipeline : MonoBehaviour, IUpdateDelegate
    {
#pragma warning disable CS0649
        [SerializeField]
        ReflectPipeline m_ReflectPipeline;
#pragma warning restore CS0649

        [SerializeField] bool m_isStandAloneViewer = default;

        public event Action<float> update;

        bool m_SyncEnabled = false;
        bool m_IsManifestDirty = false;

        Project m_SelectedProject;

        ReflectClient m_Client;
        AuthClient m_AuthClient;

        StandAloneBuiltInModelProvider m_modelProvider;

        public bool IsStandAloneViewer { get => m_isStandAloneViewer; }

        public bool TryGetNode<T>(out T node) where T : class, IReflectNode
        {
            return m_ReflectPipeline.TryGetNode(out node);
        }

        // Streaming Events
        public void OpenProject(Project project)
        {
            if (m_AuthClient == null)
            {
                Debug.LogError("Unable to open project without a Authentication Client.");
                return;
            }

            if (m_SelectedProject != null)
            {
                Debug.LogError("Only one project can be opened at a time.");
                return;
            }

            m_SelectedProject = project;

            m_Client = new ReflectClient(this, m_AuthClient.user, m_AuthClient.storage, m_SelectedProject);
            m_Client.manifestUpdated += OnManifestUpdated;
            m_IsManifestDirty = false;

            m_ReflectPipeline.InitializeAndRefreshPipeline(m_Client);

            // TODO : SaveProjectData(project) saves "project.data" for the offline project.
            // Maybe we need to move/remove this code depends on the design.
            // "project.data" file is using to get "Offline Project List" and "Enable Delete Button" in the project option dialog now
            var storage = new PlayerStorage();
            storage.SetEnvironment(ProjectServer.ProjectDataPath, true, false);
            storage.SaveProjectData(project);
        }

        public void OpenStandAloneProject()
        {
            m_modelProvider = new StandAloneBuiltInModelProvider();

            m_ReflectPipeline.InitializeAndRefreshPipeline(m_modelProvider);
        }

        public void SetSync(bool enabled)
        {
            m_SyncEnabled = enabled;
            if (enabled && m_IsManifestDirty)
            {
                m_ReflectPipeline.RefreshPipeline();
                m_IsManifestDirty = false;
            }
        }

        void OnManifestUpdated()
        {
            if (m_SyncEnabled)
            {
                m_ReflectPipeline.RefreshPipeline();
                m_IsManifestDirty = false;
            }
            else
            {
                m_IsManifestDirty = true;
            }
        }

        public void CloseProject()
        {
            if (m_ReflectPipeline == null)
            {
                return;
            }

            m_ReflectPipeline.ShutdownPipeline();

            if (m_Client != null)
            {
                m_Client.manifestUpdated -= OnManifestUpdated;
                m_Client.Dispose();
            }

            m_SelectedProject = null;
        }

        public void SetUser(UnityUser user)
        {
            if (user == null || string.IsNullOrEmpty(user.UserId))
            {
                Debug.LogError("Invalid User");
            }

            // Storage
            var storage = new PlayerStorage();
            storage.SetEnvironment(ProjectServer.ProjectDataPath, true, false);

            // Client
            m_AuthClient = new AuthClient(user, storage);

            ReflectPipelineFactory.SetUser(user, this, m_AuthClient, storage);
        }

        public void ClearUser()
        {
            ReflectPipelineFactory.ClearUser();
        }

        void OnDisable()
        {
            ClearUser();
            CloseProject();
            m_ReflectPipeline.ShutdownPipeline();
            update = null;
        }

        void Update()
        {
            update?.Invoke(Time.unscaledDeltaTime);
        }
    }
}
