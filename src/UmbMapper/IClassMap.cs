﻿// <copyright file="IMapperConfig.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using System;
using Umbraco.Core.Models;

namespace UmbMapper
{
    /// <summary>
    /// Defines the base properties required for a mapper configuration
    /// </summary>
    public interface IClassMap
    {
        /// <summary>
        /// Gets the mapped type
        /// </summary>
        Type MappedType { get; }

        /// <summary>
        /// Performs the mapping operation
        /// </summary>
        /// <param name="content">The published content</param>
        /// <returns>The <see cref="object"/></returns>
        object Map(IPublishedContent content);
    }
}