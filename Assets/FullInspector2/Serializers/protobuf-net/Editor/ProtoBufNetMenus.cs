using ProtoBuf.Meta;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Serializers.ProtoBufNet {
    public class ProtoBufNetMenus {

        [MenuItem("Window/Full Inspector/Compile protobuf-net Serialization DLL (beta)")]
        public static void CompileSerializationDLL() {
            string TypeName = ProtoBufNetSettings.PrecompiledSerializerTypeName;
            string DllPath = TypeName + ".dll";
            string AssetsDllPath = "Assets/" + DllPath;

            var options = new RuntimeTypeModel.CompilerOptions {
                TypeName = TypeName,
                OutputPath = DllPath,
                ImageRuntimeVersion = Assembly.GetExecutingAssembly().ImageRuntimeVersion,
                MetaDataVersion = 0x20000, // use .NET 2 onwards
                Accessibility = RuntimeTypeModel.Accessibility.Public
            };

            var model = TypeModelCreator.CreateModel();
            try {
                model.Compile(options);
            }
            catch (Exception) {
                Debug.LogError("Make sure to compile to protobuf-net DLL while the editor is not " +
                    "in AOT mode");
                throw;
            }

            // put the delete in a try/catch because the AssetsDllPath might not exist
            try {
                File.Delete(AssetsDllPath);
            }
            catch (Exception) { }
            try {
                File.Move(DllPath, AssetsDllPath);
            }
            catch (Exception) {
                Debug.LogWarning(string.Format("Unable to move {0} to {1}", DllPath, AssetsDllPath));
            }

            // attempt to notify Unity that an asset refresh needs to occur
            AssetDatabase.ImportAsset(AssetsDllPath, ImportAssetOptions.ForceUpdate);

            var typeList = new StringBuilder();
            foreach (var modelType in model.GetTypes()) {
                typeList.AppendLine(modelType.ToString());
            }
            Debug.Log("Finished compiling protobuf-net serialization DLL (at " + AssetsDllPath +
                "). It contains serialization data for the following types:\n\n" +
                typeList.ToString());
        }
    }
}