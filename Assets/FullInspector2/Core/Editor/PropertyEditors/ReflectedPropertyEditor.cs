using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    /// <summary>
    /// The general property editor that takes over when there is no specific override. This uses
    /// reflection to discover what values to edit.
    /// </summary>
    /// <remarks>This class has grown to be much too large. It needs to be refactored into a set of
    /// smaller components.</remarks>
    internal class ReflectedPropertyEditor : IPropertyEditor, IPropertyEditorEditAPI {
        /// <summary>
        /// The maximum depth that the reflected editor will go to for automatic object reference
        /// instantiation.
        /// </summary>
        private const int MaximumNestingDepth = 15;

        // These three variables contain the current nesting levels for the Edit, GetElementHeight,
        // and OnSceneGUI methods, respectively.
        private static int NestingDepthEdit;
        private static int NestingDepthHeight;
        private static int NestingDepthScene;

        /// <summary>
        /// This returns true if automatic instantiation should be enabled. Automatic instantiation
        /// gets disabled after the reflected editor has gone a x calls deep into itself in an
        /// attempt to prevent infinite recursion.
        /// </summary>
        private static bool ShouldAutoInstantiate() {
            return
                NestingDepthEdit < MaximumNestingDepth &&
                NestingDepthHeight < MaximumNestingDepth &&
                NestingDepthScene < MaximumNestingDepth;
        }

        // HashSets containing all of the inspected objects that have already been seen. These are
        // used to detect cycles when inspecting objects, so that a special "<cycle>" GUI can be
        // shown instead of causing a StackOverflowException.
        private static HashSet<object> AlreadyInspectedObjectsEdit = new HashSet<object>();
        private static HashSet<object> AlreadyInspectedObjectsHeight = new HashSet<object>();
        private static HashSet<object> AlreadyInspectedObjectsScene = new HashSet<object>();


        private InspectedType _metadata;

        public PropertyEditorChain EditorChain {
            get;
            set;
        }

        public ReflectedPropertyEditor(InspectedType metadata) {
            _metadata = metadata;
        }

        /// <summary>
        /// How tall buttons should be.
        /// </summary>
        private static float ButtonHeight = 18;

        /// <summary>
        /// How tall the label element should be.
        /// </summary>
        private const int TitleHeight = 17;

        /// <summary>
        /// How much space is between each element.
        /// </summary>
        private const float DividerHeight = 2f;

        /// <summary>
        /// The minimum height a child property editor has to be before a foldout is displayed
        /// </summary>
        private const float MinimumFoldoutHeight = 80;

        /// <summary>
        /// The height of "Open Script" button
        /// </summary>
        private static readonly float ScriptButtonHeight =
            EditorStyles.objectField.CalcHeight(GUIContent.none, 100);

        /// <summary>
        /// Metadata we store on edited objects to determine if we should show a foldout. This
        /// object instance is fetched using ObjectMetadata{StateObject}.
        /// </summary>
        private class StateObject {
            public bool Foldout = true;
        }

        /// <summary>
        /// Returns true if the given GUIContent element contains any content.
        /// </summary>
        private static bool HasLabel(GUIContent label) {
            return label.text != GUIContent.none.text ||
                label.image != GUIContent.none.image ||
                label.tooltip != GUIContent.none.tooltip;
        }

        /// <summary>
        /// Draws a label at the given region. Returns an indented rectangle that can be used for
        /// drawing properties directly under the label.
        /// </summary>
        private static Rect DrawLabel(Rect region, GUIContent label) {
            Rect titleRect = new Rect(region);
            titleRect.height = TitleHeight;
            region.y += TitleHeight;
            region.height -= TitleHeight;

            EditorGUI.LabelField(titleRect, label);
            return RectTools.IndentedRect(region);
        }

        /// <summary>
        /// Returns true if the given property should be displayed in the inspector.
        /// </summary>
        private static bool ShowProperty(InspectedProperty property) {
            if (property.MemberInfo.GetAttribute<HideInInspector>() != null) {
                return false;
            }

            if (property.MemberInfo.GetAttribute<ShowInInspectorAttribute>() != null) {
                return true;
            }

            if (FullInspectorSettings.InspectorAutomaticallyShowPublicProperties) {
                return property.IsPublic;
            }

            return false;
        }

        /// <summary>
        /// Currently, until the metadata storage system is rewritten, we only show foldouts for
        /// classes and interfaces.
        /// </summary>
        /// <remarks>
        /// Structs are not supported because the metadata storage solution does not currently
        /// support them.
        /// </remarks>
        private static bool CanShowFoldout(Type type) {
            return type.IsClass || type.IsInterface;
        }

        /// <summary>
        /// Returns true if the period separate property path contains the given property name.
        /// </summary>
        private static bool ContainsPropertyName(string propertyPath, string propertyName) {
            string[] paths = propertyPath.Split('.');
            for (int i = 0; i < paths.Length; ++i) {
                if (paths[i] == propertyName) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to extract the name of serialized key for the given property modification.
        /// </summary>
        /// <param name="obj">The object that that modification is applied to.</param>
        /// <param name="mod">The modification.</param>
        /// <param name="keyName">An output parameter containing the name of the key that the
        /// modification maps to.</param>
        /// <returns>True if the key was found, false otherwise.</returns>
        private static bool TryExtractPropertyName(ISerializedObject obj, PropertyModification mod, out string keyName) {
            //-
            // We want to extract 2 from _serializedStateValues.Array.data[2]
            // We could probably use a regular expression, but this is fine for now
            if (mod.propertyPath.StartsWith("_serializedStateValues.Array.data[")) {
                string front = mod.propertyPath.Remove(0, "_serializedStateValues.Array.data".Length + 1);
                string num = front.Substring(0, front.Length - 1);

                int index;
                if (int.TryParse(num, out index) &&
                    index >= 0 && index < obj.SerializedStateKeys.Count) {

                    keyName = obj.SerializedStateKeys[index];
                    return true;
                }
            }

            keyName = string.Empty;
            return false;
        }

        private static bool HasPrefabDiff(object instance, InspectedProperty property) {
            // For prefab differences, we rely upon the internal Unity mechanisms for identifying
            // when an object has a prefab diff. We are able to do this because we only support
            // top-level prefab differences.
            //
            // One of the current issues with this mechanism is when an array is serialized by
            // Unity, and only part of the array tracks the prefab, then the inspector will show the
            // entire array in bold (when only the one part should be).

            // We only want top-level components
            if (instance is MonoBehaviour == false) {
                return false;
            }

            // If there is no prefab, then we don't show anything in bold.
            var prefabGameObject = (GameObject)PrefabUtility.GetPrefabParent(((MonoBehaviour)instance).gameObject);
            if (prefabGameObject == null) {
                return false;
            }

            // If the prefab doesn't have this component, then the entire component should be in
            // bold.
            var prefab = prefabGameObject.GetComponent(instance.GetType());
            if (prefab == null) {
                return true;
            }

            // Get all of the property modifications on the object. If there are no property
            // modifications, then nothing should be in bold.
            PropertyModification[] mods = PrefabUtility.GetPropertyModifications((UnityObject)instance);
            if (mods == null) {
                return false;
            }

            ISerializedObject serializedInstance = (ISerializedObject)instance;

            foreach (PropertyModification mod in mods) {
                // A property modification can take one of two forms. It can either be modifying a
                // Unity serialized value or a Full Inspector serialized value.

                // Check to see if it's a Full Inspector serialized value. If it is, then we lookup
                // the key that the modification is associated with and, if we find said key, and
                // that the key is equal to the property we are checking for, then we return true.
                string serializedPropertyName;
                if (TryExtractPropertyName(serializedInstance, mod, out serializedPropertyName) &&
                    serializedPropertyName == property.Name) {
                    return true;
                }

                // Check to see if it is a Unity serialized value. We have to do a dotted comparison
                // because the propertyPath may be associated with, ie, an array, which in that case
                // the path is something like "values.Array._items[0]" while property.Name is just
                // "values".
                if (ContainsPropertyName(mod.propertyPath, property.Name)) {
                    return true;
                }
            }

            return false;
        }

        public object OnSceneGUI(object element) {
            try {
                ++NestingDepthScene;
                if (_metadata.ReflectedType.IsClass && element != null && AlreadyInspectedObjectsScene.Add(element) == false) {
                    return element;
                }

                // Not showing a scene GUI for the object for this frame should be fine
                if (element == null) {
                    return element;
                }

                foreach (var property in _metadata.InspectedProperties) {
                    if (ShowProperty(property) == false) {
                        continue;
                    }

                    var editorChain = PropertyEditor.Get(property.StorageType, property.MemberInfo);
                    IPropertyEditor editor = editorChain.FirstEditor;

                    object currentValue = property.Read(element);
                    currentValue = editor.OnSceneGUI(currentValue);
                    property.Write(element, currentValue);
                }

                return element;
            }
            finally {
                --NestingDepthScene;
                if (NestingDepthScene == 0) AlreadyInspectedObjectsScene = new HashSet<object>();
            }
        }

        public bool CanEdit(Type dataType) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// A helper method that draws the inspector for a field/property at the given location.
        /// </summary>
        private void EditProperty(ref Rect region, object element, InspectedProperty property) {
            // skip the property if it shouldn't be shown
            if (ShowProperty(property) == false) {
                return;
            }

            bool hasPrefabDiff = HasPrefabDiff(element, property);
            if (hasPrefabDiff) UnityInternalReflection.SetBoldDefaultFont(true);

            // get the label / tooltip
            TooltipAttribute tooltip = property.MemberInfo.GetAttribute<TooltipAttribute>();
            GUIContent label = new GUIContent(DisplayNameMapper.Map(property.Name),
                tooltip != null ? tooltip.Tooltip : "");

            // edit the property
            {
                var editorChain = PropertyEditor.Get(property.StorageType, property.MemberInfo);
                IPropertyEditor editor = editorChain.FirstEditor;

                object currentValue = property.Read(element);

                float propertyHeight = editor.GetElementHeight(label, currentValue);
                Rect propertyRegion = new Rect(region);
                propertyRegion.height = propertyHeight;

                float usedHeight = 0;

                // Show a foldout if the subitem is above a certain height
                StateObject stateObject = ObjectMetadata<StateObject>.Get(currentValue);
                if (propertyHeight > MinimumFoldoutHeight &&
                    CanShowFoldout(property.StorageType)) {

                    GUIContent foldoutLabel = stateObject.Foldout ? GUIContent.none :
                        editor.GetFoldoutHeader(label, currentValue);
                    Rect foldoutRegion = propertyRegion;
                    foldoutRegion.height = EditorStyles.foldout.CalcHeight(foldoutLabel, 100);
                    foldoutRegion.width = EditorStyles.foldout.CalcSize(foldoutLabel).x;

                    stateObject.Foldout = EditorGUI.Foldout(foldoutRegion, stateObject.Foldout, foldoutLabel);
                    usedHeight = EditorStyles.foldout.CalcHeight(foldoutLabel, 100);
                }

                // If the foldout is active, then draw the inspector for the subitem
                if (stateObject.Foldout) {
                    object updatedValue = editor.Edit(propertyRegion, label, currentValue);
                    property.Write(element, updatedValue);
                    usedHeight = propertyHeight;
                }

                region.y += usedHeight;
            }

            region.y += DividerHeight;

            if (hasPrefabDiff) UnityInternalReflection.SetBoldDefaultFont(false);
        }

        /// <summary>
        /// A helper method that draws a button at the given region.
        /// </summary>
        private void EditButton(ref Rect region, object element, InspectedMethod method) {
            Rect buttonRect = region;
            buttonRect.height = ButtonHeight;

            string buttonName = method.DisplayName;

            // Disable the button if invoking it will cause an exception
            if (method.HasArguments) buttonName += " (Remove method parameters to enable this button)";

            EditorGUI.BeginDisabledGroup(method.HasArguments);
            if (GUI.Button(buttonRect, buttonName)) {
                method.Invoke(element);
            }
            EditorGUI.EndDisabledGroup();

            region.y += ButtonHeight + DividerHeight;
        }

        /// <summary>
        /// Draws the actual property editors.
        /// </summary>
        private object EditPropertiesButtons(Rect region, object element) {
            InspectedType inspectedType = _metadata;

            while (inspectedType != null) {
                var buttons = new Stack<InspectedMethod>(inspectedType.LocalButtons.Reverse());
                var properties = new Stack<InspectedProperty>(inspectedType.LocalInspectedProperties.Reverse());

                while (buttons.Count > 0 && properties.Count > 0) {
                    double propertyOrder = InspectorOrderAttribute.GetInspectorOrder(properties.Peek().MemberInfo);
                    double buttonOrder = InspectorOrderAttribute.GetInspectorOrder(buttons.Peek().Method);

                    if (buttonOrder < propertyOrder) {
                        EditButton(ref region, element, buttons.Pop());
                    }

                    else {
                        EditProperty(ref region, element, properties.Pop());
                    }
                }

                while (properties.Count > 0) {
                    EditProperty(ref region, element, properties.Pop());
                }

                while (buttons.Count > 0) {
                    EditButton(ref region, element, buttons.Pop());
                }

                inspectedType = inspectedType.Parent;
            }

            return element;
        }

        /// <summary>
        /// Returns the height of the element that opens up the script. If the element is not
        /// displayed for any reason, then this method returns 0.
        /// </summary>
        private static float GetOpenScriptButtonHeight(object element) {
            if (FullInspectorSettings.ShowOpenScriptButton == false) {
                return 0;
            }

            if (fiEditorUtility.HasMonoScript(element)) {
                return ScriptButtonHeight + DividerHeight;
            }

            return 0;
        }

        /// <summary>
        /// Draws the element that opens up the script associated with the given element. If the
        /// editor element should not be shown, then this method does not do anything.
        /// </summary>
        /// <returns>A rect containing the still usable parts of region</returns>
        private static Rect ShowOpenScriptButton(Rect region, object element) {
            if (FullInspectorSettings.ShowOpenScriptButton == false) {
                return region;
            }

            // Show the "Open Script" button
            MonoScript monoScript;
            if (fiEditorUtility.TryGetMonoScript(element, out monoScript)) {
                Rect rect = region;
                rect.height = ScriptButtonHeight;

                region.height -= ScriptButtonHeight + DividerHeight;
                region.y += ScriptButtonHeight + DividerHeight;

                EditorGUI.ObjectField(rect, "Script", monoScript, typeof(MonoScript), false);
            }

            return region;
        }

        public object Edit(Rect region, GUIContent label, object element) {
            try {
                ++NestingDepthEdit;
                if (_metadata.ReflectedType.IsClass && element != null && AlreadyInspectedObjectsEdit.Add(element) == false) {
                    EditorGUI.LabelField(region, label, new GUIContent("<cycle>"));
                    return element;
                }


                region = ShowOpenScriptButton(region, element);

                if (HasLabel(label)) {
                    region = DrawLabel(region, label);
                }

                if (element == null) {
                    // if the user want's an instance, we'll create one right away
                    // We also check to make sure we should automatically instantiate references, as
                    // if we're pretty far down in the nesting level there may be an infinite recursion
                    // going on
                    if (FullInspectorSettings.InspectorAutomaticReferenceInstantation &&
                        _metadata.HasDefaultConstructor &&
                        ShouldAutoInstantiate()) {

                        element = _metadata.CreateInstance();
                    }

                    // otherwise we show a button to create an instance
                    else {
                        string buttonMessage = "{0} reference is null; create instance with default constructor?";
                        if (_metadata.HasDefaultConstructor == false) {
                            buttonMessage = "{0} reference is null and there is no default constructor; create unformatted instance?";
                        }
                        if (GUI.Button(region, string.Format(buttonMessage, _metadata.ReflectedType.Name))) {
                            element = _metadata.CreateInstance();
                        }

                        return element;
                    }
                }

                return EditPropertiesButtons(region, element);

            }
            finally {
                --NestingDepthEdit;
                if (NestingDepthEdit == 0) AlreadyInspectedObjectsEdit = new HashSet<object>();
            }
        }

        public float GetElementHeight(GUIContent label, object element) {
            try {
                ++NestingDepthHeight;
                if (_metadata.ReflectedType.IsClass && element != null && AlreadyInspectedObjectsHeight.Add(element) == false) {
                    return EditorStyles.label.CalcHeight(GUIContent.none, 100) + 5;
                }

                float height = HasLabel(label) ? TitleHeight : 0;

                height += GetOpenScriptButtonHeight(element);

                if (element == null) {
                    // if the user want's an instance, we'll create one right away
                    // We also check to make sure we should automatically instantiate references, as
                    // if we're pretty far down in the nesting level there may be an infinite recursion
                    // going on
                    if (FullInspectorSettings.InspectorAutomaticReferenceInstantation &&
                        _metadata.HasDefaultConstructor &&
                        ShouldAutoInstantiate()) {

                        element = _metadata.CreateInstance();
                    }

                    // otherwise we show a button to create an instance
                    else {
                        height += EditorStyles.largeLabel.CalcHeight(label, 100);
                    }
                }

                if (element != null) {
                    height += (ButtonHeight + DividerHeight) * _metadata.Buttons.Count();

                    foreach (var property in _metadata.InspectedProperties) {
                        // skip the property if it shouldn't be shown
                        if (ShowProperty(property) == false) {
                            continue;
                        }

                        // property itself
                        var editorChain = PropertyEditor.Get(property.StorageType, property.MemberInfo);
                        IPropertyEditor editor = editorChain.FirstEditor;

                        object currentValue = property.Read(element);
                        GUIContent propertyLabel = new GUIContent(DisplayNameMapper.Map(property.Name));

                        StateObject stateObject = ObjectMetadata<StateObject>.Get(currentValue);
                        if (stateObject.Foldout) {
                            float propertyHeight = editor.GetElementHeight(propertyLabel, currentValue);
                            height += propertyHeight;
                        }
                        else {
                            height += EditorStyles.foldout.CalcHeight(propertyLabel, 100);
                        }

                        // divider
                        height += DividerHeight;
                    }
                }

                return height;
            }
            finally {
                --NestingDepthHeight;
                if (NestingDepthHeight == 0) AlreadyInspectedObjectsHeight = new HashSet<object>();
            }
        }

        public GUIContent GetFoldoutHeader(GUIContent label, object element) {
            return label;
        }

        public static IPropertyEditor TryCreate(Type dataType) {
            // The reflected property editor is applicable to *every* type except collections,
            // where it is expected that the ICollectionPropertyEditor will take over (or something
            // more specific than that, such as the IListPropertyEditor).

            var metadata = InspectedType.Get(dataType);
            if (metadata.IsCollection) {
                return null;
            }

            return new ReflectedPropertyEditor(metadata);
        }
    }
}