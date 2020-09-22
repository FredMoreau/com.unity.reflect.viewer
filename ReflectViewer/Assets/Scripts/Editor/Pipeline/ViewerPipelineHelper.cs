﻿using System;
using UnityEditor;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    static class ViewerPipelineHelper
    {
        [UnityEditor.MenuItem("Create Viewer Pipeline Asset", menuItem = "Assets/Reflect/Pipeline/Create Viewer Pipeline Asset")]
        static void CreateBasicSampleAsset()
        {
            var pipelineAsset = ScriptableObject.CreateInstance<PipelineAsset>();

            // Nodes

            var projectStreamer = pipelineAsset.CreateNode<ProjectStreamerNode>();
            var instanceProvider = pipelineAsset.CreateNode<SyncObjectInstanceProviderNode>();
            var spatialFilter = pipelineAsset.CreateNode<SpatialFilterNode>();
            var streamLimiter = pipelineAsset.CreateNode<StreamInstanceLimiterNode>();
            var dataProvider = pipelineAsset.CreateNode<DataProviderNode>();
            var meshConverter = pipelineAsset.CreateNode<MeshConverterNode>();
            var materialConverter = pipelineAsset.CreateNode<URPMaterialConverterNode>();
            var textureConverter = pipelineAsset.CreateNode<TextureConverterNode>();
            var instanceConverter = pipelineAsset.CreateNode<InstanceConverterNode>();
            var metadataFilter = pipelineAsset.CreateNode<MetadataFilterNode>();
            var streamIndicator = pipelineAsset.CreateNode<StreamIndicatorNode>();
            var lightFilter = pipelineAsset.CreateNode<LightFilterNode>();

            // Inputs / Outputs

            pipelineAsset.CreateConnection(projectStreamer.assetOutput, dataProvider.assetInput);
            pipelineAsset.CreateConnection(projectStreamer.assetOutput, spatialFilter.assetInput);
            pipelineAsset.CreateConnection(projectStreamer.assetOutput, streamIndicator.streamAssetInput);
            pipelineAsset.CreateConnection(spatialFilter.assetOutput, instanceProvider.input);
            pipelineAsset.CreateConnection(instanceProvider.output, metadataFilter.instanceInput);
            pipelineAsset.CreateConnection(instanceProvider.output, streamLimiter.instanceInput);
            pipelineAsset.CreateConnection(instanceProvider.output, streamIndicator.streamInstanceInput);
            pipelineAsset.CreateConnection(streamLimiter.instanceOutput, dataProvider.instanceInput);
            pipelineAsset.CreateConnection(dataProvider.syncMeshOutput, meshConverter.input);
            pipelineAsset.CreateConnection(dataProvider.syncMaterialOutput, materialConverter.input);
            pipelineAsset.CreateConnection(dataProvider.syncTextureOutput, textureConverter.input);
            pipelineAsset.CreateConnection(dataProvider.instanceDataOutput, instanceConverter.input);
            pipelineAsset.CreateConnection(dataProvider.instanceDataOutput, streamIndicator.streamInstanceDataInput);
            pipelineAsset.CreateConnection(instanceConverter.output, metadataFilter.gameObjectInput);
            pipelineAsset.CreateConnection(instanceConverter.output, streamIndicator.gameObjectInput);
            pipelineAsset.CreateConnection(instanceConverter.output, spatialFilter.gameObjectInput); // loop back into spatial filter to control bounding box display
            pipelineAsset.CreateConnection(instanceConverter.output, lightFilter.gameObjectInput);

            // Params

            pipelineAsset.SetParam(dataProvider.hashCacheParam, projectStreamer);
            pipelineAsset.SetParam(materialConverter.textureCacheParam, textureConverter);
            pipelineAsset.SetParam(instanceConverter.materialCacheParam, materialConverter);
            pipelineAsset.SetParam(instanceConverter.meshCacheParam, meshConverter);

            // Save Asset

            AssetDatabase.CreateAsset(pipelineAsset, "Assets/Pipelines/ViewerPipeline.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = pipelineAsset;
        }
    }

}
