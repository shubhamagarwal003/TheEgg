﻿// The MIT License (MIT)
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
using System.Reflection;

namespace FullInspector.Internal {
    /// <summary>
    /// Caches type name to type lookups. Type lookups occur in all loaded assemblies.
    /// </summary>
    public static class TypeCache {
        /// <summary>
        /// Cache from fully qualified type name to type instances.
        /// </summary>
        // TODO: verify that type names will be unique
        private static Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Assemblies indexed by their name.
        /// </summary>
        private static Dictionary<string, Assembly> _assembliesByName;

        /// <summary>
        /// A list of assemblies, by index.
        /// </summary>
        private static List<Assembly> _assembliesByIndex;

        static TypeCache() {
            // Setup assembly references so searching and the like resolves correctly.
            _assembliesByName = new Dictionary<string, Assembly>();
            _assembliesByIndex = new List<Assembly>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                _assembliesByName[assembly.FullName] = assembly;
                _assembliesByIndex.Add(assembly);
            }

            _cachedTypes = new Dictionary<string, Type>();

            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
        }

        private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args) {
            _assembliesByName[args.LoadedAssembly.FullName] = args.LoadedAssembly;
            _assembliesByIndex.Add(args.LoadedAssembly);
        }

        /// <summary>
        /// Does a direct lookup for the given type, ie, goes directly to the assembly identified by
        /// assembly name and finds it there.
        /// </summary>
        /// <param name="assemblyName">The assembly to find the type in.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="type">The found type.</param>
        /// <returns>True if the type was found, false otherwise.</returns>
        private static bool TryDirectTypeLookup(string assemblyName, string typeName, out Type type) {
            if (assemblyName != null) {
                Assembly assembly;
                if (_assembliesByName.TryGetValue(assemblyName, out assembly)) {
                    type = assembly.GetType(typeName);
                    return type != null;
                }
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Tries to do an indirect type lookup by scanning through every loaded assembly until the
        /// type is found in one of them.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="type">The found type.</param>
        /// <returns>True if the type was found, false otherwise.</returns>
        private static bool TryIndirectTypeLookup(string typeName, out Type type) {
            // There used to be a foreach loop through the value keys of the _assembliesByName
            // dictionary. However, during that loop assembly loads could occur, causing an
            // OutOfSync exception. To resolve that, we just iterate through the assemblies by
            // index.

            int i = 0;
            while (i < _assembliesByIndex.Count) {
                Assembly assembly = _assembliesByIndex[i];

                // try GetType; should be fast
                type = assembly.GetType(typeName);
                if (type != null) {
                    return true;
                }

                // private type or similar; go through the slow path and check every type's full
                // name
                foreach (var foundType in assembly.GetTypes()) {
                    if (foundType.FullName == typeName) {
                        type = foundType;
                        return true;
                    }
                }

                ++i;
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Find a type with the given name. An exception is thrown if no type with the given name
        /// can be found. This method searches all currently loaded assemblies for the given type.
        /// </summary>
        /// <param name="name">The fully qualified name of the type.</param>
        /// <param name="assemblyHint">A hint for the assembly to start the search with</param>
        /// <param name="willThrow">Will an exception be thrown if the type could not be found? If
        /// this is false, then null will be returned if the type was not located.</param>
        public static Type FindType(string name, string assemblyHint = null, bool willThrow = true) {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }

            Type type;
            if (_cachedTypes.TryGetValue(name, out type) == false) {
                // if both the direct and indirect type lookups fail, then throw an exception
                if (TryDirectTypeLookup(assemblyHint, name, out type) == false &&
                    TryIndirectTypeLookup(name, out type) == false) {

                    if (willThrow) {
                        throw new InvalidOperationException(
                            "Cannot locate type = {0}, {1}".F(name, assemblyHint));

                    }
                }

                _cachedTypes[name] = type;
            }

            return type;
        }
    }
}