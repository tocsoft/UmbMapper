// <copyright file="HomeMap.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using UmbMapper.PropertyMappers;
using UmbMapper.PropertyMappers.Archetype;
using UmbMapper.PropertyMappers.NuPickers;
using UmbMapper.Sample.Models.Pages;

namespace UmbMapper.Sample.ComponentModel.Mappers
{
    /// <summary>
    /// Configures mapping for the home page
    /// </summary>
    public static class HomeMap
    {
        /// <summary>
        /// Configures mapping for the home page
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void Configure(MapperConfig config)
        {
            config.Model<Home>(m =>
            {
                m.Property(x => x.BrowserPageTitle)
                 .SetAlias(x => x.BrowserPageTitle, x => x.Name);

                m.Property(x => x.OpenGraphType)
                     .SetMapper<NuPickerEnumPropertyMapper>();

                m.Property(x => x.OpenGraphImage)
                    .SetMapper<UmbracoPickerPropertyMapper>();

                m.Property(x => x.Gallery)
                    .SetMapper<ArchetypeFactoryPropertyMapper>(); // could also read attribute to find the default mapper
            });
        }
    }
}