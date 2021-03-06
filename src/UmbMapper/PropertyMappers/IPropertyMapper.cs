﻿// <copyright file="IPropertyMapper.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using System;
using System.Globalization;
using System.Reflection;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace UmbMapper.PropertyMappers
{
    /// <summary>
    /// Defines properties for mapping a property
    /// </summary>
    public interface IPropertyMapper
    {
        /// <summary>
        /// Gets the property
        /// </summary>
        PropertyInfo Property { get; }

        /// <summary>
        /// Gets the property type
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// Gets the property aliases
        /// </summary>
        string[] Aliases { get; }

        /// <summary>
        /// Gets a value indicating whether to map the property recursively up the tree
        /// </summary>
        bool Recursive { get; }

        /// <summary>
        /// Gets the default value
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Gets the culture
        /// </summary>
        CultureInfo Culture { get; }

        /// <summary>
        /// Gets the Umbraco context
        /// </summary>
        UmbracoContext UmbracoContext { get; }

        /// <summary>
        /// Gets the MembershipHelper instance
        /// </summary>
        MembershipHelper Members { get; }

        /// <summary>
        /// Gets the UmbracoHelper instance
        /// </summary>
        UmbracoHelper Umbraco { get; }

        /// <summary>
        /// Maps the property from the given content
        /// </summary>
        /// <param name="content">The published content</param>
        /// <param name="value">The current value</param>
        /// <returns>The <see cref="object"/></returns>
        object Map(IPublishedContent content, object value);
    }
}