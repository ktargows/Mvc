// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ApiControllerApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ILoggerFactory _loggerFactory;

        public ApiControllerApplicationModelProvider(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <remarks>
        /// Order is set to execute after the <see cref="DefaultApplicationModelProvider"/>.
        /// </remarks>
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            var autoValidateModelFilter = new AutoValidateModelFilter(_loggerFactory.CreateLogger<AutoValidateModelFilter>());

            foreach (var controllerModel in context.Result.Controllers)
            {
                if (!controllerModel.Attributes.Any(f => f is ApiControllerAttribute))
                {
                    continue;
                }

                foreach (var actionModel in controllerModel.Actions)
                {
                    actionModel.Filters.Add(autoValidateModelFilter);
                }
            }
        }
    }
}
