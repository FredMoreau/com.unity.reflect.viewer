using SharpFlux;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class PopulateOptionsUIController : MonoBehaviour
    {
        [SerializeField] Button m_DialogButton = default;
        [SerializeField] SlideToggle m_TreesToggle = default;

        DialogWindow m_DialogWindow;
        Image m_DialogButtonImage;
        SceneOptionData m_CurrentsSceneOptionData;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
            m_DialogWindow = GetComponent<DialogWindow>();
        }

        void Start()
        {
            m_DialogButton.onClick.AddListener(OnDialogButtonClicked);

            m_TreesToggle.onValueChanged.AddListener(OnTreesToggleChanged);
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_DialogButtonImage.enabled = data.activeDialog == DialogType.PopulateOptions;

            if (m_CurrentsSceneOptionData == data.sceneOptionData)
                return;

            if (m_CurrentsSceneOptionData.enableTrees != data.sceneOptionData.enableTrees)
                m_TreesToggle.on = data.sceneOptionData.enableTrees;

            m_CurrentsSceneOptionData = data.sceneOptionData;
        }

        void OnTreesToggleChanged(bool on)
        {
            var data = UIStateManager.current.stateData.sceneOptionData;
            data.enableTrees = on;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetViewOption, data));
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.PopulateOptions;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }
    }
}
