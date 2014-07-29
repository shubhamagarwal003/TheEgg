using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    /// <summary>
    /// Some reflection utilities that can be AOT compiled (and are therefore available at runtime).
    /// </summary>
    public class RuntimeReflectionUtilities {
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        /// <summary>
        /// Invokes the given static method on the given type.
        /// </summary>
        /// <param name="type">The type to search for the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="parameters">The parameters to invoke the method with.</param>
        public static void InvokeStaticMethod(Type type, string methodName, object[] parameters) {
            try {
                type.GetMethod(methodName, StaticFlags).Invoke(null, parameters);
            }
            catch { }
        }
        public static void InvokeStaticMethod(string typeName, string methodName, object[] parameters) {
            InvokeStaticMethod(TypeCache.FindType(typeName), methodName, parameters);
        }

        /// <summary>
        /// Attempts to query the current value for the given static method. If the type does not
        /// have the given field/property or it is not castable to type T, then the given default
        /// value is returned.
        /// </summary>
        public static T QueryStaticMethod<T>(Type type, string methodName, T defaultValue, object[] parameters) {
            try {
                return (T)type.GetMethod(methodName, StaticFlags).Invoke(null, parameters);
            }
            catch {
                return defaultValue;
            }
        }
        public static T QueryStaticMethod<T>(string typeName, string methodName, T defaultValue, object[] parameters) {
            return QueryStaticMethod(TypeCache.FindType(typeName), methodName, defaultValue, parameters);
        }

        /// <summary>
        /// Attempts to query the current value for the given static field or property value. If the
        /// type does not have the given field/property or it is not castable to type T, then the
        /// given default value is returned.
        /// </summary>
        public static T QueryStaticFieldProperty<T>(Type type, string name, T defaultValue) {
            try {
                return (T)type.GetField(name, StaticFlags).GetValue(null);
            }
            catch { }

            try {
                return (T)type.GetProperty(name, StaticFlags).GetValue(null, null);
            }
            catch { }

            return defaultValue;
        }
        public static T QueryStaticFieldProperty<T>(string typeName, string name, T defaultValue) {
            return QueryStaticFieldProperty(TypeCache.FindType(typeName), name, defaultValue);
        }

        /// <summary>
        /// Returns a list of object instances from types in the assembly that implement the given
        /// type. This only constructs objects which have default constructors.
        /// </summary>
        public static IEnumerable<TInterface> GetAssemblyInstances<TInterface>() {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                   from type in assembly.GetTypes()

                   where typeof(TInterface).IsAssignableFrom(type)
                   where type.GetConstructor(Type.EmptyTypes) != null

                   select (TInterface)Activator.CreateInstance(type);
        }

        /// <summary>
        /// Returns all types that derive from UnityEngine.Object that are usable during runtime.
        /// </summary>
        public static IEnumerable<Type> GetUnityObjectTypes() {
            return from assembly in GetRuntimeAssemblies()

                   // GetExportedTypes() doesn't work for dynamic modules, so we jut use GetTypes()
                   // instead and manually filter for public
                   from type in assembly.GetTypes()
                   where type.IsVisible

                   // Cannot be an open generic type
                   where type.IsGenericTypeDefinition == false

                   where typeof(UnityObject).IsAssignableFrom(type)

                   select type;
        }

        /// <summary>
        /// Return a guess of all assemblies that can be used at runtime.
        /// </summary>
        public static IEnumerable<Assembly> GetRuntimeAssemblies() {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()

                   where assembly.FullName.Contains("UnityEditor") == false
                   where assembly.FullName.Contains("AssetStoreTools") == false
                   where assembly.FullName.Contains("-Editor") == false

                   select assembly;
        }

        /// <summary>
        /// Returns all types in the current AppDomain that derive from the given baseType and are a
        /// class that is not an open generic type.
        /// </summary>
        public static IEnumerable<Type> AllSimpleTypesDerivingFrom(Type baseType) {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                   from type in assembly.GetTypes()
                   where baseType.IsAssignableFrom(type)
                   where type.IsClass
                   where type.IsGenericTypeDefinition == false
                   select type;
        }
    }
}