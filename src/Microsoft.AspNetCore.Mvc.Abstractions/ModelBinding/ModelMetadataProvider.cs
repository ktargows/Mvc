// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// A provider that can supply instances of <see cref="ModelMetadata"/>.
    /// </summary>
    public abstract class ModelMetadataProvider : IModelMetadataProvider
    {
        /// <summary>
        /// Supplies metadata describing the properties of a <see cref="Type"/>.
        /// </summary>
        /// <param name="modelType">The <see cref="Type"/>.</param>
        /// <returns>A set of <see cref="ModelMetadata"/> instances describing properties of the <see cref="Type"/>.</returns>
        public abstract IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType);

        /// <summary>
        /// Supplies metadata describing a <see cref="Type"/>.
        /// </summary>
        /// <param name="modelType">The <see cref="Type"/>.</param>
        /// <returns>A <see cref="ModelMetadata"/> instance describing the <see cref="Type"/>.</returns>
        public abstract ModelMetadata GetMetadataForType(Type modelType);

        /// <summary>
        /// Supplies metadata describing an action parameter.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="ActionDescriptor"/>.</param>
        /// <param name="parameter">The <see cref="ParameterDescriptor"/>.</param>
        /// <returns>A <see cref="ModelMetadata"/> instance describing properties of the <see cref="ActionDescriptor"/>.</returns>
        public abstract ModelMetadata GetMetadataForParameter(ActionDescriptor actionDescriptor, ParameterDescriptor parameter);
    }
}
