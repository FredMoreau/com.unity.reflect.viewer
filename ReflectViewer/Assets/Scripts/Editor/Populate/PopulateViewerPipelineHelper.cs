using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Reflect.Pipeline;

namespace Demo
{
    static class PopulateViewerPipelineHelper
    {
        [UnityEditor.MenuItem("Create Populate Viewer Pipeline Asset", menuItem = "Assets/Reflect/Pipeline/Create Populate Viewer Pipeline Asset")]
        static void CreateBasicSampleAsset()
        {
            var existingPipelineAsset = AssetDatabase.LoadAssetAtPath<PipelineAsset>("Assets/Pipelines/ViewerPipeline.asset");
            if (existingPipelineAsset == null)
            {
                Debug.LogWarning("Assets / Pipelines / ViewerPipeline.asset not found.");
                return;
            }

            var pipelineAsset = PipelineAsset.Instantiate<PipelineAsset>(existingPipelineAsset);

            if (pipelineAsset.TryGetNode<InstanceConverterNode>(out var instanceConverter))
            {
                var gameObjectReplacerNode = pipelineAsset.CreateNode<GameObjectAdvancedReplacerNode>();

                pipelineAsset.CreateConnection(instanceConverter.output, gameObjectReplacerNode.input);

                // Save Asset

                AssetDatabase.CreateAsset(pipelineAsset, "Assets/Populate/Pipelines/Populate Viewer Pipeline.asset");
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();

                Selection.activeObject = pipelineAsset;
            }
            else
            {
                Debug.LogWarning("No InstanceConverterNode found in Assets/Pipelines/ViewerPipeline.asset.");
            }
        }
    }
}
