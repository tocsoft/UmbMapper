// <copyright file="PublishedImageMap.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using UmbMapper.Sample.Models.Media;
using Umbraco.Core;

namespace UmbMapper.Sample.ComponentModel.Mappers
{
    /// <summary>
    /// Configures mapping of published images
    /// </summary>
    public static class PublishedImageMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedImageMap"/> class.
        /// </summary>
        public static void Configure(MapperConfig config)
        {
            config.Model<PublishedImage>(m =>
            {
                m.Property(x => x.FileName).SetAlias(Constants.Conventions.Media.File);
                m.Property(x => x.Bytes).SetAlias(Constants.Conventions.Media.Bytes);
                m.Property(x => x.Extension).SetAlias(Constants.Conventions.Media.Extension);
                m.Property(x => x.Width).SetAlias(Constants.Conventions.Media.Width);
                m.Property(x => x.Height).SetAlias(Constants.Conventions.Media.Height);
                m.Property(x => x.Crops).SetAlias(Constants.Conventions.Media.File);
            });
        }
    }
}