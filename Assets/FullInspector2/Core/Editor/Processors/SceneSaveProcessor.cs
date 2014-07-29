﻿using UnityEngine;

namespace FullInspector.Internal {
    /// <summary>
    /// This processor saves the state of any unsaved BaseBehavior instances when the scene has been
    /// saved. This type isn't necessary as the BaseBehaviorEditor manages saving everything, but it
    /// provides some extra data protection.
    /// </summary>
    internal class SceneSaveProcessor : UnityEditor.AssetModificationProcessor {
        protected static void OnWillSaveAssets(string[] assets) {
            if (FullInspectorSettings.ForceSaveAllAssetsOnSceneSave) {
                FullInspectorSaveManager.SaveAll();
            }
        }
    }
}