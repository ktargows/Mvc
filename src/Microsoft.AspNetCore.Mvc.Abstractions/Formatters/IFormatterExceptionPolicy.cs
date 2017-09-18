// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.Formatters
{
    /// <summary>
    /// A policy which <see cref="IInputFormatter"/>s can use to indicate if they 
    /// </summary>
    public interface IFormatterExceptionPolicy
    {
        bool SendBadRequestForExceptionsDuringDeserialization { get; }
    }
}
