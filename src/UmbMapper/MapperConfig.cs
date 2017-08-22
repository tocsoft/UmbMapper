// <copyright file="MapperConfigRegistry.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UmbMapper.Invocations;
using Umbraco.Core.Models;

namespace UmbMapper
{
    /// <summary>
    /// The registry for mapper configurations
    /// </summary>
    public class MapperConfig : IMapperConfig
    {
        private ConcurrentDictionary<Type, IClassMap> mappers = new ConcurrentDictionary<Type, IClassMap>();

        /// <summary>
        /// Gets or sets the default instance that is used by the mapping extension methods.
        /// </summary>
        public static MapperConfig Default { get; set; } = new MapperConfig();

        /// <inheritdoc/>
        public ClassMap<T> Model<T>()
            where T : class, new()
        {
            if (this.mappers.TryGetValue(typeof(T), out IClassMap mapper))
            {
                return (ClassMap<T>)mapper;
            }
            else
            {
                var config = new ClassMap<T>(this);
                this.mappers.AddOrUpdate(typeof(T), config, (t, o) => o);
                return config;
            }
        }

        private IClassMap Model(Type type)
        {
            if (this.mappers.TryGetValue(type, out IClassMap mapper))
            {
                return mapper;
            }
            else
            {
                Type mappedClass = typeof(ClassMap<>).MakeGenericType(type);
                var map = (IClassMap)Activator.CreateInstance(mappedClass, new[] { this });
                this.mappers.AddOrUpdate(type, map, (t, o) => o);
                return map;
            }
        }

        /// <inheritdoc/>
        public object Map(Type type, IPublishedContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            IClassMap mapper = this.Model(type);
            return mapper.Map(content);
        }

        /// <inheritdoc/>
        public IEnumerable<object> Map(Type type, IEnumerable<IPublishedContent> content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            IClassMap mapper = this.Model(type);

            IEnumerable<object> typedItems = content.Select(x => mapper.Map(x));

            // We need to cast back here as nothing is strong typed anymore.
            return (IEnumerable<object>)EnumerableInvocations.Cast(type, typedItems);
        }
    }
}