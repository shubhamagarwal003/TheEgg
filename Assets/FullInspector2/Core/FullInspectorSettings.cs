
namespace FullInspector {
    /// <summary>
    /// This class contains some settings that can be used to customize the behavior of the Full
    /// Inspector.
    /// </summary>
    public static class FullInspectorSettings {
        /// <summary>
        /// A scene has just been saved. Should all IScriptableObjects be checked to see if they
        /// need to be saved? This is disabled by default because it causes a performance hit when
        /// saving and unless you have an extremely strange user scenario where you are not using
        /// the inspector to edit a BaseBehavior, everything will save correctly.
        /// </summary>
        public static bool ForceSaveAllAssetsOnSceneSave = false;

        /// <summary>
        /// A recompilation has been detected. Should all IScriptableObjects be checked to see if
        /// they need to be saved? This is disabled by default because it causes a performance hit
        /// when saving and unless you have an extremely strange user scenario where you are not
        /// using the inspector to edit a BaseBehavior, everything will save correctly.
        /// </summary>
        public static bool ForceSaveAllAssetsOnRecompilation = false;

        /// <summary>
        /// A recompilation has been detected. Should all IScriptableObjects be checked to see if
        /// they need to be restored? This is disabled by default because it causes a performance
        /// hit.
        /// </summary>
        public static bool ForceRestoreAllAssetsOnRecompilation = false;

        /// <summary>
        /// If this is set to true, then Full Inspector will attempt to automatically instantiate
        /// all reference fields/properties in an object. This will negatively impact the
        /// performance for creating objects (lots of reflection is used).
        /// </summary>
        public static bool AutomaticReferenceInstantation = false;

        /// <summary>
        /// If this is set to true, then when the reflected inspector encounters a property that is
        /// null it will attempt to create an instance of that property. This is most similar to how
        /// Unity operates. Please note that this will not instantiate fields/properties that are
        /// hidden from the inspector. Additionally, this will not instantiate fields which do not
        /// have a default constructor.
        /// </summary>
        public static bool InspectorAutomaticReferenceInstantation = true;

        /// <summary>
        /// Should public properties/fields automatically be shown in the inspector? If this is
        /// false, then only properties annotated with [ShowInInspector] will be shown.
        /// [HideInInspector] will never be necessary.
        /// </summary>
        public static bool InspectorAutomaticallyShowPublicProperties = true;

        /// <summary>
        /// Should the "Open Script" button above every property editor be shown?
        /// </summary>
        public static bool ShowOpenScriptButton = true;

        /// <summary>
        /// Should Full Inspector emit warnings when it detects a possible data loss (such as a
        /// renamed or removed variable) or general serialization issue?
        /// </summary>
        public static bool EmitWarnings = false;
    }
}