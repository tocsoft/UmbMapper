using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace UmbMapper
{
    /// <summary>
    /// Encapulates the root mapper config
    /// </summary>
    public interface IMapperConfig
    {
        /// <summary>
        /// Maps the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="content">The content.</param>
        /// <returns>The mapped value</returns>
        object Map(Type type, IPublishedContent content);

        /// <summary>
        /// Maps the content to a collection of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="content">The content.</param>
        /// <returns>The mapped values</returns>
        IEnumerable<object> Map(Type type, IEnumerable<IPublishedContent> content);

        /// <summary>
        /// Loads the <see cref="ClassMap{T}"/> for a particualre model
        /// </summary>
        /// <typeparam name="T">The type of the class to map</typeparam>
        /// <returns>The class map ready to update.</returns>
        ClassMap<T> Model<T>()
            where T : class, new();
    }
}