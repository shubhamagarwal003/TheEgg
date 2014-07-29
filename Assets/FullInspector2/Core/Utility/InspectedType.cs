// The MIT License (MIT)
//
// Copyright (c) 2013-2014 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    /// <summary>
    /// Provides a view of an arbitrary type that unifies a number of discrete concepts in the CLR.
    /// Arrays and Collection types have special support, but their APIs are unified by the
    /// TypeMetadata so that they can be treated as if they were a regular type.
    /// </summary>
    public sealed partial class InspectedType {
        static InspectedType() {
            InitializePropertyRemoval();
        }

        /// <summary>
        /// Returns true if the type represented by this metadata contains a default constructor.
        /// </summary>
        public bool HasDefaultConstructor {
            get {
                if (_hasDefaultConstructorCache.HasValue == false) {
                    // arrays are considered to have a default constructor
                    if (_isArray) {
                        _hasDefaultConstructorCache = true;
                    }

                    // value types (ie, structs) always have a default constructor
                    else if (ReflectedType.IsValueType) {
                        _hasDefaultConstructorCache = true;
                    }

                    else {
                        // consider private constructors as well
                        var ctor = ReflectedType.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null, Type.EmptyTypes, null);
                        _hasDefaultConstructorCache = ctor != null;
                    }
                }

                return _hasDefaultConstructorCache.Value;
            }
        }
        private bool? _hasDefaultConstructorCache;

        /// <summary>
        /// Creates a new instance of the type that this metadata points back to. If this type has a
        /// default constructor, then Activator.CreateInstance will be used to construct the type
        /// (or Array.CreateInstance if it an array). Otherwise, an uninitialized object created via
        /// FormatterServices.GetSafeUninitializedObject is used to construct the instance.
        /// </summary>
        public object CreateInstance() {
            // Unity requires special construction logic for types that derive from
            // ScriptableObject. The normal inspector reflection logic will create ScriptableObject
            // instances if FullInspectorSettings.AutomaticReferenceInstantation has been set to
            // true.
            if (typeof(ScriptableObject).IsAssignableFrom(ReflectedType)) {
                return ScriptableObject.CreateInstance(ReflectedType);
            }

            if (HasDefaultConstructor == false) {
                return FormatterServices.GetSafeUninitializedObject(ReflectedType);
            }

            if (_isArray) {
                // we have to start with a size zero array otherwise it will have invalid data
                // inside of it
                return Array.CreateInstance(ElementType, 0);
            }

            try {
                return Activator.CreateInstance(ReflectedType, /*nonPublic:*/ true);
            }
            catch (MissingMethodException e) {
                throw new InvalidOperationException("Unable to create instance of " + ReflectedType + "; there is no default constructor", e);
            }
            catch (TargetInvocationException e) {
                throw new InvalidOperationException("Constructor of " + ReflectedType + " threw an exception when creating an instance", e);
            }
            catch (MemberAccessException e) {
                throw new InvalidOperationException("Unable to access constructor of " + ReflectedType, e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the TypeMetadata class from a type. Use TypeCache to get
        /// instances of TypeMetadata; do not use this constructor directly.
        /// </summary>
        internal InspectedType(Type type) {
            ReflectedType = type;

            // determine if we are a collection or array; recall that arrays implement the
            // ICollection interface, however

            _isArray = type.IsArray;
            IsCollection = _isArray || type.IsImplementationOf(typeof(ICollection<>));

            // If we're a collection or array, get the generic type definition so that client code
            // can determine how to deserialize child elements
            if (_isArray == false && IsCollection) {
                Type collectionType = type.GetInterface(typeof(ICollection<>));
                _elementType = collectionType.GetGenericArguments()[0];
            }
            else if (_isArray) {
                _elementType = type.GetElementType();
            }

            // If we're not one of those three types, then we will be using Properties to assign
            // data to ourselves, so we want to lookup said information
            else {
                // Lookup the local properties
                _localInspectedProperties = CollectLocalProperties(type).OrderBy(property =>
                    InspectorOrderAttribute.GetInspectorOrder(property.MemberInfo)
                ).ToList();

                // Find all of the properties on this object (and on parent types)
                _inspectedProperties = new List<InspectedProperty>(_localInspectedProperties);
                if (ReflectedType.BaseType != null) {
                    // We go through InspectedType for parent types so that removing a property from
                    // a parent type will correctly propagate down.
                    var inspectedParentType = InspectedType.Get(ReflectedType.BaseType);
                    _inspectedProperties.AddRange(inspectedParentType._inspectedProperties);
                }

                // Sort the properties by their order. We use OrderBy instead of Sort because
                // OrderBy is guaranteed to be stable.
                _inspectedProperties = _inspectedProperties.OrderBy(property =>
                    InspectorOrderAttribute.GetInspectorOrder(property.MemberInfo)
                ).ToList();


                // Get the list of properties that we serialize.
                _serializedProperties = (from property in _inspectedProperties
                                         where IsSerializedByFullInspector(property)
                                         where IsSerializedByUnity(property) == false
                                         select property).ToList();

                // Add our property names in
                _nameToProperty = new Dictionary<string, InspectedProperty>();
                foreach (var property in _inspectedProperties) {
                    _nameToProperty[property.Name] = property;
                }

                // Collect all of the methods that act as buttons for this type.
                BindingFlags allMethodBindingFlags =
                        BindingFlags.Instance |
                        BindingFlags.Static |
                        BindingFlags.FlattenHierarchy |
                        BindingFlags.Public |
                        BindingFlags.NonPublic;

                BindingFlags localMethodBindingFlags =
                        BindingFlags.Instance |
                        BindingFlags.Static |
                        BindingFlags.DeclaredOnly |
                        BindingFlags.Public |
                        BindingFlags.NonPublic;

                // Find and order all of the buttons on the type.
                _buttons = (from method in ReflectedType.GetMethods(allMethodBindingFlags)
                            where Attribute.IsDefined(method, typeof(InspectorButtonAttribute))
                            orderby InspectorOrderAttribute.GetInspectorOrder(method)
                            select new InspectedMethod(method)).ToArray();

                _localButtons = (from method in ReflectedType.GetMethods(localMethodBindingFlags)
                                 where Attribute.IsDefined(method, typeof(InspectorButtonAttribute))
                                 orderby InspectorOrderAttribute.GetInspectorOrder(method)
                                 select new InspectedMethod(method)).ToArray();

                // Collect all of the methods on the type.
                _methods = (from method in ReflectedType.GetMethods(allMethodBindingFlags)
                            where Attribute.IsDefined(method, typeof(HideInInspector)) == false
                            select new InspectedMethod(method)).ToArray();
            }
        }

        /// <summary>
        /// Returns true if the given property can be displayed in the inspector. A property is
        /// not inspectable if it's a delegate, it cannot be both read and written to, or if it is
        /// an indexer.
        /// </summary>
        private static bool IsInspectableProperty(PropertyInfo property) {
            // We don't serialize delegates
            if (typeof(Delegate).IsAssignableFrom(property.PropertyType)) {
                return false;
            }

            // If the property cannot be both read and written to, we don't serialize it
            if (property.CanRead == false || property.CanWrite == false) {
                return false;
            }

            // If the property is named "Item", it might be the this[int] indexer, which in
            // that case we don't serialize it We cannot just compare with "Item" because of
            // explicit interfaces, where the name of the property will be the full method
            // name.
            if (property.Name.EndsWith("Item")) {
                ParameterInfo[] parameters = property.GetIndexParameters();
                if (parameters.Length > 0) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if the given field is inspectable. A field is not inspectable if it's a
        /// delegate or a compiler generated field.
        /// </summary>
        private static bool IsInspectableField(FieldInfo field) {
            // We don't serialize delegates
            if (typeof(Delegate).IsAssignableFrom(field.FieldType)) {
                return false;
            }

            // We don't serialize compiler generated fields (an example would be a backing
            // field for an automatically generated property).
            if (field.IsDefined(typeof(CompilerGeneratedAttribute), false)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns all of the static properties on the given type.
        /// </summary>
        public IEnumerable<InspectedProperty> StaticProperties {
            get {
                if (IsCollection) {
                    yield break;
                }

                var flags =
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.Static;

                foreach (MemberInfo member in ReflectedType.GetMembers(flags)) {
                    FieldInfo field = member as FieldInfo;
                    PropertyInfo prop = member as PropertyInfo;

                    if (field != null && InspectedType.IsInspectableField(field)) {
                        yield return new InspectedProperty(field);
                    }

                    if (prop != null && InspectedType.IsInspectableProperty(prop)) {
                        yield return new InspectedProperty(prop);
                    }
                }
            }
        }

        /// <summary>
        /// Recursive method that adds all of the properties and fields from the given type into the
        /// given list. This method does not collect properties for parent types.
        /// </summary>
        /// <param name="reflectedType">The type to process to collect properties from.</param>
        private static List<InspectedProperty> CollectLocalProperties(Type reflectedType) {
            var properties = new List<InspectedProperty>();

            var flags =
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly;

            foreach (MemberInfo member in reflectedType.GetMembers(flags)) {
                PropertyInfo property = member as PropertyInfo;
                FieldInfo field = member as FieldInfo;

                //-
                //-
                //-
                //-
                // Properties
                if (property != null) {
                    if (IsInspectableProperty(property)) {
                        properties.Add(new InspectedProperty(property));
                    }
                }

                //-
                //-
                //-
                //-
                // Fields
                else if (field != null) {
                    if (IsInspectableField(field)) {
                        properties.Add(new InspectedProperty(field));
                    }
                }
            }

            return properties;
        }

        /// <summary>
        /// The type that this metadata is modeling, ie, the type that the metadata was constructed
        /// off of.
        /// </summary>
        public Type ReflectedType {
            get;
            private set;
        }

        /// <summary>
        /// Returns the parent inspected type, or null if there is none.
        /// </summary>
        public InspectedType Parent {
            get {
                if (ReflectedType.BaseType == null) {
                    return null;
                }

                return InspectedType.Get(ReflectedType.BaseType);
            }
        }

        /// <summary>
        /// Iff this metadata maps back to a Collection or an Array type, then this is the type of
        /// element stored inside the array or collection. Otherwise, this method throws an
        /// exception.
        /// </summary>
        public Type ElementType {
            get {
                if (IsCollection == false) {
                    throw new InvalidOperationException("Unable to get the ElementType of a " +
                        "type metadata object that is not a collection");
                }

                return _elementType;
            }
        }
        private Type _elementType;

        /// <summary>
        /// True if the base type is a collection. If true, accessing Properties will throw an
        /// exception.
        /// </summary>
        public bool IsCollection {
            get;
            private set;
        }

        /// <summary>
        /// True if the base type is an array. If true, accessing Properties will throw an
        /// exception. IsCollection is also true if _isArray is true.
        /// </summary>
        private bool _isArray;

        private InspectedMethod[] _methods;
        public InspectedMethod[] Methods {
            get {
                if (IsCollection) {
                    throw new InvalidOperationException("A type that is a collection or an array " +
                        "does not have properties (for the metadata on type " + ReflectedType + ")");
                }

                return _methods;
            }
        }

        /// <summary>
        /// A cached list of the buttons on the inspected type.
        /// </summary>
        private InspectedMethod[] _buttons;

        /// <summary>
        /// Returns all buttons within the type.
        /// </summary>
        public IEnumerable<InspectedMethod> Buttons {
            get {
                if (IsCollection) {
                    throw new InvalidOperationException("A type that is a collection or an array " +
                        "does not have buttons (for the metadata on type " + ReflectedType + ")");
                }

                return _buttons;
            }
        }

        /// <summary>
        /// A cached list of the buttons on the inspected type.
        /// </summary>
        private InspectedMethod[] _localButtons;

        /// <summary>
        /// Returns all buttons on the given type that are local to this inheritance level.
        /// </summary>
        public IEnumerable<InspectedMethod> LocalButtons {
            get {
                if (IsCollection) {
                    throw new InvalidOperationException("A type that is a collection or an array " +
                        "does not have buttons (for the metadata on type " + ReflectedType + ")");
                }

                return _localButtons;
            }
        }

        private Dictionary<string, InspectedProperty> _nameToProperty;
        public InspectedProperty GetPropertyByName(string name) {
            if (IsCollection) {
                throw new InvalidOperationException("A type that is a collection or an array " +
                    "does not have properties (for the metadata on type " + ReflectedType + ")");
            }

            InspectedProperty metadata;

            // if lookup fails, metadata will be null
            _nameToProperty.TryGetValue(name, out metadata);

            return metadata;
        }

        /// <summary>
        /// The properties on the type that should be serialized by Full Inspector.
        /// </summary>
        public List<InspectedProperty> SerializedProperties {
            get {
                if (IsCollection) {
                    throw new InvalidOperationException("A type that is a collection or an array " +
                        "does not have properties (for the metadata on type " + ReflectedType + ")");
                }

                return _serializedProperties;
            }
        }
        private List<InspectedProperty> _serializedProperties;

        private List<InspectedProperty> _localInspectedProperties;

        /// <summary>
        /// The set of inspected properties on the collection that only exist on this level of
        /// the inheritance hierarchy.
        /// </summary>
        public IEnumerable<InspectedProperty> LocalInspectedProperties {
            get {
                if (IsCollection) {
                    throw new InvalidOperationException("A type that is a collection or an array " +
                        "does not have properties (for the metadata on type " + ReflectedType + ")");
                }

                return _localInspectedProperties;
            }
        }

        /// <summary>
        /// The properties on the type that should be shown in the inspector.
        /// </summary>
        public IEnumerable<InspectedProperty> InspectedProperties {
            get {
                if (IsCollection) {
                    throw new InvalidOperationException("A type that is a collection or an array " +
                        "does not have properties (for the metadata on type " + ReflectedType + ")");
                }

                return _inspectedProperties;
            }
        }
        private List<InspectedProperty> _inspectedProperties;

    }
}