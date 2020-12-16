using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Reflect.Data;
using Unity.Reflect.IO;
using Unity.Reflect.Model;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.StandAloneViewer
{
    public class StandAloneBuiltInModelProvider : ISyncModelProvider
    {
        string m_DataFolder, m_manifestFolder;

        public StandAloneBuiltInModelProvider()
        {
            m_manifestFolder = Directory.EnumerateDirectories(Path.Combine(Application.streamingAssetsPath, "ReflectStandAloneBuiltInModels"), "*", SearchOption.AllDirectories).FirstOrDefault();

            if (m_manifestFolder == null)
            {
                Debug.LogError("Unable to find Reflect imported model data. Reflect Stand Alone Template requires local Reflect Model data in 'Reflect'.");
            }
        }

        public async Task<IEnumerable<SyncManifest>> GetSyncManifestsAsync()
        {
            var syncManifestPath = Directory.EnumerateFiles(m_manifestFolder, "*.manifest", SearchOption.AllDirectories).FirstOrDefault();
            m_DataFolder = Path.Combine(Path.GetDirectoryName(syncManifestPath), Path.GetFileNameWithoutExtension(syncManifestPath));
            var syncManifest = await PlayerFile.LoadManifestAsync(syncManifestPath);

            return new [] { syncManifest };
        }

        public async Task<ISyncModel> GetSyncModelAsync(StreamKey streamKey, string hash)
        {
            return await PlayerFile.LoadSyncModelAsync(m_DataFolder, streamKey.key, hash);
        }
    }
}
