// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.Formatters
{
    /// <summary>
    /// Exception thrown by <see cref="IInputFormatter"/> when the input is not in an expected format.
    /// </summary>
    public class InputFormatException : Exception
    {
        public InputFormatException()
        {
        }

        public InputFormatException(string message)
            : base(message)
        {
        }

        public InputFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
