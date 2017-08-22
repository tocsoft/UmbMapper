// <copyright file="PropertyMapExtensions.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using System;
using System.Collections.Generic;
using UmbMapper.PropertyMappers;

namespace UmbMapper
{
    /// <summary>
    /// Extension methods for the <see cref="PropertyMap{T}"/> class
    /// </summary>
    public static class PropertyMapExtensions
    {
        /// <summary>
        /// Instructs the mapper to map the property recursively up the tree
        /// </summary>
        /// <typeparam name="T">The type of object to map</typeparam>
        /// <param name="collection">The collection of property mappings</param>
        /// <param name="recursive">with the properties should be recursive or not</param>
        /// <returns>The collection.</returns>
        public static IEnumerable<PropertyMap<T>> AsRecursive<T>(this IEnumerable<PropertyMap<T>> collection, bool recursive)
            where T : class, new()
        {
            foreach (PropertyMap<T> p in collection)
            {
                p.AsRecursive(recursive);
            }

            return collection;
        }

        /// <summary>
        /// Instructs the mapper to lazily map the property
        /// </summary>
        /// <typeparam name="T">The type of object to map</typeparam>
        /// <param name="collection">The collection of property mappings</param>
        /// <returns>The collection.</returns>
        public static IEnumerable<PropertyMap<T>> AsLazy<T>(this IEnumerable<PropertyMap<T>> collection)
            where T : class, new()
        {
            foreach (PropertyMap<T> p in collection)
            {
                p.AsLazy();
            }

            return collection;
        }

        /// <summary>
        /// Instructs the mapper to lazily map the property
        /// </summary>
        /// <typeparam name="T">The type of object to map</typeparam>
        /// <param name="collection">The collection of property mappings</param>
        /// <param name="value">The value</param>
        /// <returns>The collection.</returns>
        public static IEnumerable<PropertyMap<T>> DefaultValue<T>(this IEnumerable<PropertyMap<T>> collection, object value)
            where T : class, new()
        {
            foreach (PropertyMap<T> p in collection)
            {
                p.DefaultValue(value);
            }

            return collection;
        }

        /// <summary>
        /// Immediately executes the given action on each map in the source sequence.
        /// </summary>
        /// <typeparam name="T">The type of object to map</typeparam>
        /// <param name="source">The sequence of property mappings</param>
        /// <param name="action">The action to execute on each map.</param>
        public static void ForEach<T>(this IEnumerable<PropertyMap<T>> source, Action<PropertyMap<T>> action)
            where T : class, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (PropertyMap<T> element in source)
            {
                action(element);
            }
        }

        /// <summary>
        /// Immediately executes the given action on each map in the source sequence.
        /// Each element's index is used in the logic of the action.
        /// </summary>
        /// <typeparam name="T">The type of object to map</typeparam>>
        /// <param name="source">The sequence of property mappings</param>
        /// <param name="action">The action to execute on each map; the second parameter
        /// of the action represents the index of the source map.</param>
        public static void ForEach<T>(this IEnumerable<PropertyMap<T>> source, Action<PropertyMap<T>, int> action)
            where T : class, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            int index = 0;
            foreach (PropertyMap<T> element in source)
            {
                action(element, index++);
            }
        }
    }
}