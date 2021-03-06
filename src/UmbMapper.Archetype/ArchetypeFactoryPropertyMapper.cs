﻿// <copyright file="ArchetypeFactoryPropertyMapper.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using Archetype.Extensions;
using Archetype.Models;
using Umbraco.Core.Models;

namespace UmbMapper.PropertyMappers.Archetype
{
    /// <summary>
    /// Maps Archetype properties in the backoffice
    /// </summary>
    public class ArchetypeFactoryPropertyMapper : DocTypeFactoryPropertyMapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchetypeFactoryPropertyMapper"/> class.
        /// </summary>
        /// <param name="info">The property map information</param>
        public ArchetypeFactoryPropertyMapper(PropertyMapInfo info)
            : base(info)
        {
        }

        /// <inheritdoc/>
        public override object Map(IPublishedContent content, object value)
        {
            if (value is IPublishedContent)
            {
                return base.Map(content, value);
            }

            var archetypeModel = value as ArchetypeModel;
            if (archetypeModel != null)
            {
                return base.Map(content, archetypeModel.ToPublishedContentSet());
            }

            var archetypeFieldsetModel = value as ArchetypeFieldsetModel;
            if (archetypeFieldsetModel != null)
            {
                return base.Map(content, archetypeFieldsetModel.ToPublishedContent());
            }

            return value;
        }
    }
}