﻿// <copyright file="ClassMap{T}.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using UmbMapper.Extensions;
using UmbMapper.Invocations;
using UmbMapper.PropertyMappers;
using UmbMapper.Proxy;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace UmbMapper
{
    /// <summary>
    /// Configures mapping of type properties to Umbraco properties
    /// </summary>
    /// <typeparam name="T">The type of object to map</typeparam>
    public class ClassMap<T> : IClassMap
        where T : class, new()
    {
        private readonly FastPropertyAccessor propertyAccessor;
        private readonly List<PropertyMap<T>> maps;
        private readonly MapperConfig config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassMap{T}" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public ClassMap(MapperConfig config)
        {
            this.config = config;
            Type type = typeof(T);
            this.MappedType = type;
            this.propertyAccessor = new FastPropertyAccessor(type);
            this.maps = new List<PropertyMap<T>>();
            this.AllProperties();
        }

        /// <inheritdoc/>
        public Type MappedType { get; }

        /// <summary>
        /// Gets the collection of mappings registered with the mapper
        /// </summary>
        public IReadOnlyCollection<PropertyMap<T>> Mappings => this.maps;

        /// <summary>
        /// Get the mapping for the property.
        /// </summary>
        /// <param name="propertyExpression">The property to map</param>
        /// <returns>The <see cref="PropertyMap{T}"/></returns>
        public PropertyMap<T> Property(Expression<Func<T, object>> propertyExpression)
        {
            // The property access might be getting converted to object to match the func
            // If so, get the operand and see if that's a member expression
            MemberExpression member = propertyExpression.Body as MemberExpression
                ?? (propertyExpression.Body as UnaryExpression)?.Operand as MemberExpression;

            if (member == null)
            {
                throw new ArgumentException("Action must be a member expression.");
            }

            var map = new PropertyMap<T>(member.Member as PropertyInfo);

            PropertyMap<T> finalMapping = this.maps.SingleOrDefault(x => x.Info.Property.Name == map.Info.Property.Name);

            if (finalMapping == null)
            {
                finalMapping = map;
                this.maps.Add(map);
            }

            return finalMapping;
        }

        /// <summary>
        /// Adds the map from each property to an equivalent Umbraco property
        /// </summary>
        /// <param name="propertyExpressions">The properties to map</param>
        /// <returns>The <see cref="IEnumerable{T}"/></returns>
        public IEnumerable<PropertyMap<T>> Properties(params Expression<Func<T, object>>[] propertyExpressions)
        {
            if (propertyExpressions == null)
            {
                return Enumerable.Empty<PropertyMap<T>>();
            }

            var mapsTemp = new List<PropertyMap<T>>();
            foreach (Expression<Func<T, object>> property in propertyExpressions)
            {
                mapsTemp.Add(this.Property(property));
            }

            return mapsTemp;
        }

        /// <summary>
        /// Adds a map from each property in the class to an equivalent Umbraco property
        /// </summary>
        /// <returns>The <see cref="IEnumerable{T}"/></returns>
        public IEnumerable<PropertyMap<T>> AllProperties()
        {
            // no longet n
            foreach (PropertyInfo property in typeof(T).GetProperties(UmbMapperConstants.MappableFlags))
            {
                this.AddOrGetPropertyMapping(property);
            }

            return this.maps;
        }

        /// <inheritdoc/>
        object IClassMap.Map(IPublishedContent content)
        {
            return this.Map(content);
        }

        /// <summary>
        /// Performs the mapping operation
        /// </summary>
        /// <param name="content">The published content</param>
        /// <returns>
        /// The value
        /// </returns>
        public T Map(IPublishedContent content)
        {
            object result = this.MappedType.GetInstance();
            var maps = this.maps.Where(x => !x.Info.Ignore);

            // Gather any lazily mapped properties and assign the lazy mapping
            IEnumerable<PropertyMap<T>> lazyMaps = maps.Where(m => m.Info.Lazy).ToArray();
            if (lazyMaps.Any())
            {
                // A dictionary to store lazily invoked values.
                var lazyProperties = new Dictionary<string, Lazy<object>>();
                foreach (PropertyMap<T> map in lazyMaps)
                {
                    object localResult = result;
                    lazyProperties.Add(map.Info.Property.Name, new Lazy<object>(() => MapProperty(this.config, map, content, this.propertyAccessor, localResult)));
                }

                // Create a proxy instance to replace our object.
                var interceptor = new LazyInterceptor(result, lazyProperties);
                var factory = new ProxyFactory();
                result = factory.CreateProxy(this.MappedType, interceptor);
            }

            // Now map the non-lazy properties
            foreach (PropertyMap<T> map in maps.Where(m => !m.Info.Lazy))
            {
                MapProperty(this.config, map, content, this.propertyAccessor, result);
            }

            return (T)result;
        }

        private static object MapProperty(MapperConfig config, PropertyMap<T> map, IPublishedContent content, FastPropertyAccessor propertyAccessor, object result)
        {
            // Users might want to use lazy loading with API controllers that do not inherit from UmBracoAPIController.
            // Certain mappers like Archtype require the context so we want to ensure it exists.
            EnsureUmbracoContext();

            // We don't have to explictly set a mapper.
            if (map.PropertyMapper == null)
            {
                map.PropertyMapper = new UmbracoPropertyMapper(map.Info);
            }

            // Ensure the property mapper is always invoked first
            object value = null;
            if (!(map.PropertyMapper is UmbracoPropertyMapper))
            {
                value = new UmbracoPropertyMapper(map.Info).Map(content, null);
            }

            // Other mappers
            value = map.PropertyMapper.Map(content, value);
            if (value != null)
            {
                PropertyMapInfo info = map.Info;
                value = SantizeValue(value, info);
                value = RecursivelyMap(config, value, info);

                if (value != null)
                {
                    propertyAccessor.SetValue(map.PropertyMapper.Property.Name, result, value);
                }
            }

            return value;
        }

        private static object RecursivelyMap(MapperConfig config, object value, PropertyMapInfo info)
        {
            if (!info.PropertyType.IsInstanceOfType(value))
            {
                // If the property value is an IPublishedContent, then we can map it to the target type.
                var content = value as IPublishedContent;
                if (content != null && info.PropertyType.IsClass)
                {
                    return config.Map(info.PropertyType, content);
                }

                // If the property value is an IEnumerable<IPublishedContent>, then we can map it to the target type.
                if (value != null && value.GetType().IsEnumerableOfType(typeof(IPublishedContent)) && info.IsEnumerableType)
                {
                    Type genericType = info.PropertyType.GetEnumerableType();
                    if (genericType != null && genericType.IsClass)
                    {
                        return config.Map(genericType, (IEnumerable<IPublishedContent>)value);
                    }
                }
            }

            return value;
        }

        private static object SantizeValue(object value, PropertyMapInfo info)
        {
            bool propertyIsCastableEnumerable = info.IsCastableEnumerableType;
            bool propertyIsConvertableEnumerable = info.IsConvertableEnumerableType;

            if (value != null)
            {
                bool valueIsConvertableEnumerable = value.GetType().IsConvertableEnumerableType();

                // You cannot set an enumerable of type from an empty object array.
                // This should allow the casting back of IEnumerable<T> to an empty List<T> Collection<T> etc.
                // I cant think of any that don't have an empty constructor
                if (value.Equals(Enumerable.Empty<object>()) && propertyIsCastableEnumerable)
                {
                    Type typeArg = info.PropertyType.GetTypeInfo().GenericTypeArguments.First();
                    return info.PropertyType.IsInterface ? EnumerableInvocations.Cast(typeArg, (IEnumerable)value) : info.PropertyType.GetInstance();
                }

                // Ensure only a single item is returned when requested.
                if (valueIsConvertableEnumerable && !propertyIsConvertableEnumerable)
                {
                    // Property is not enumerable, but value is, so grab first item
                    IEnumerator enumerator = ((IEnumerable)value).GetEnumerator();
                    return enumerator.MoveNext() ? enumerator.Current : null;
                }

                // And now check for the reverse situation.
                if (!valueIsConvertableEnumerable && propertyIsConvertableEnumerable)
                {
                    var array = Array.CreateInstance(value.GetType(), 1);
                    array.SetValue(value, 0);
                    return array;
                }
            }
            else
            {
                if (propertyIsCastableEnumerable)
                {
                    if (info.PropertyType.IsInterface && !info.IsEnumerableOfKeyValueType)
                    {
                        // Value is null, but property is enumerable interface, so return empty enumerable
                        return EnumerableInvocations.Empty(info.PropertyType.GenericTypeArguments.First());
                    }

                    // Concrete enumerables cannot be cast from Array so we create an instance and return it
                    // if we know it has an empty constructor.
                    ParameterInfo[] constructorParams = info.PropertyType.GetConstructorParameters();
                    if (constructorParams != null && constructorParams.Length == 0)
                    {
                        // Internally this uses Activator.CreateInstance which is heavily optimized.
                        return info.PropertyType.GetInstance();
                    }
                }
            }

            return value;
        }

        private static void EnsureUmbracoContext()
        {
            if (UmbracoContext.Current == null)
            {
                var dummyHttpContext = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("/", string.Empty, new StringWriter())));
                UmbracoContext.EnsureContext(
                    dummyHttpContext,
                    ApplicationContext.Current,
                    new WebSecurity(dummyHttpContext, ApplicationContext.Current),
                    UmbracoConfig.For.UmbracoSettings(),
                    UrlProviderResolver.Current.Providers,
                    false);
            }
        }

        private PropertyMap<T> AddOrGetPropertyMapping(PropertyInfo info)
        {
            PropertyMap<T> found = this.maps.SingleOrDefault(x => x.Info.Property.Name == info.Name);
            if (found == null)
            {
                found = new PropertyMap<T>(info);
                this.maps.Add(found);
            }

            return found;
        }
    }
}