using System;
using Umbraco.Core.Models;

namespace UmbMapper
{
    public static class IMapperConfigExtensions
    {
        /// <summary>
        /// Loads the <see cref="ClassMap{T}" /> for a particualre model and run the mapper agains it
        /// </summary>
        /// <typeparam name="T">The type of the class to map</typeparam>
        /// <param name="config">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <returns>
        /// This <paramref name="config"/>
        /// </returns>
        public static IMapperConfig Model<T>(this IMapperConfig config, Action<ClassMap<T>> mapper)
            where T : class, new()
        {
            ClassMap<T> m = config.Model<T>();
            mapper?.Invoke(m);
            return config;
        }

        /// <summary>
        /// Loads the <see cref="ClassMap{T}" /> for a particualre model and run the mapper agains it
        /// </summary>
        /// <typeparam name="T">The type of the class to map</typeparam>
        /// <param name="config">The configuration.</param>
        /// <param name="content">The content.</param>
        /// <returns>
        /// This <paramref name="config" />
        /// </returns>
        public static T Map<T>(this IMapperConfig config, IPublishedContent content)
            where T : class, new()
        {
            return (T)config.Map(typeof(T), content);
        }
    }
}