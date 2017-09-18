// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    public interface IModelMetadataProvider
    {
        ModelMetadata GetMetadataForType(Type modelType);

        IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType);
    }

    // TODO: Could we take a breaking change and add this to the IModelMetadataProvider
    // interface? If not, it means that anyone who's built a custom IModelMetadataProvider
    // will not get the new parameter validation feature.
    public interface IExtendedModelMetadataProvider : IModelMetadataProvider
    {
        ModelMetadata GetMetadataForParameter(ActionDescriptor actionDescriptor, ParameterDescriptor parameter);
    }
}