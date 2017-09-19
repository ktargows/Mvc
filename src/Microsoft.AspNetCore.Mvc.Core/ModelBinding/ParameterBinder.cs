// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// Binds and validates models specified by a <see cref="ParameterDescriptor"/>.
    /// </summary>
    public class ParameterBinder
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly IModelBinderFactory _modelBinderFactory;
        private readonly ValidatorCache _validatorCache;
        private readonly IModelValidatorProvider _validatorProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="ParameterDescriptor"/>.
        /// </summary>
        /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="modelBinderFactory">The <see cref="IModelBinderFactory"/>.</param>
        /// <param name="validatorProviders">The <see cref="IModelValidatorProvider"/> instances.</param>
        public ParameterBinder(
            IModelMetadataProvider modelMetadataProvider,
            IModelBinderFactory modelBinderFactory,
            IList<IModelValidatorProvider> validatorProviders)
        {
            if (modelMetadataProvider == null)
            {
                throw new ArgumentNullException(nameof(modelMetadataProvider));
            }

            if (modelBinderFactory == null)
            {
                throw new ArgumentNullException(nameof(modelBinderFactory));
            }

            _modelMetadataProvider = modelMetadataProvider;
            _modelBinderFactory = modelBinderFactory;
            _validatorCache = new ValidatorCache();
            _validatorProvider = new CompositeModelValidatorProvider(validatorProviders);
        }

        /// <summary>
        /// Initializes and binds a model specified by <paramref name="parameter"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/>.</param>
        /// <param name="parameter">The <see cref="ParameterDescriptor"/></param>
        /// <returns>The result of model binding.</returns>
        public Task<ModelBindingResult> BindModelAsync(
            ActionContext actionContext,
            IValueProvider valueProvider,
            ParameterDescriptor parameter)
        {
            return BindModelAsync(actionContext, valueProvider, parameter, value: null);
        }

        /// <summary>
        /// Binds a model specified by <paramref name="parameter"/> using <paramref name="value"/> as the initial value.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/>.</param>
        /// <param name="parameter">The <see cref="ParameterDescriptor"/></param>
        /// <param name="value">The initial model value.</param>
        /// <returns>The result of model binding.</returns>
        public virtual Task<ModelBindingResult> BindModelAsync(
            ActionContext actionContext,
            IValueProvider valueProvider,
            ParameterDescriptor parameter,
            object value)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (valueProvider == null)
            {
                throw new ArgumentNullException(nameof(valueProvider));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var metadata = _modelMetadataProvider.GetMetadataForType(parameter.ParameterType);
            var binder = _modelBinderFactory.CreateBinder(new ModelBinderFactoryContext
            {
                BindingInfo = parameter.BindingInfo,
                Metadata = metadata,
                CacheToken = parameter,
            });

            return BindModelAsync(
                actionContext,
                binder,
                valueProvider,
                parameter,
                metadata,
                value);
        }

        /// <summary>
        /// Binds a model specified by <paramref name="parameter"/> using <paramref name="value"/> as the initial value.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="modelBinder">The <see cref="IModelBinder"/>.</param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/>.</param>
        /// <param name="parameter">The <see cref="ParameterDescriptor"/></param>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <param name="value">The initial model value.</param>
        /// <returns>The result of model binding.</returns>
        public virtual async Task<ModelBindingResult> BindModelAsync(
            ActionContext actionContext,
            IModelBinder modelBinder,
            IValueProvider valueProvider,
            ParameterDescriptor parameter,
            ModelMetadata metadata,
            object value)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (modelBinder == null)
            {
                throw new ArgumentNullException(nameof(modelBinder));
            }

            if (valueProvider == null)
            {
                throw new ArgumentNullException(nameof(valueProvider));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (parameter.BindingInfo?.RequestPredicate?.Invoke(actionContext) == false)
            {
                return ModelBindingResult.Failed();
            }

            var modelBindingContext = DefaultModelBindingContext.CreateBindingContext(
                actionContext,
                valueProvider,
                metadata,
                parameter.BindingInfo,
                parameter.Name);
            modelBindingContext.Model = value;

            var parameterModelName = parameter.BindingInfo?.BinderModelName ?? metadata.BinderModelName;
            if (parameterModelName != null)
            {
                // The name was set explicitly, always use that as the prefix.
                modelBindingContext.ModelName = parameterModelName;
            }
            else if (modelBindingContext.ValueProvider.ContainsPrefix(parameter.Name))
            {
                // We have a match for the parameter name, use that as that prefix.
                modelBindingContext.ModelName = parameter.Name;
            }
            else
            {
                // No match, fallback to empty string as the prefix.
                modelBindingContext.ModelName = string.Empty;
            }

            await modelBinder.BindModelAsync(modelBindingContext);

            var modelBindingResult = modelBindingContext.Result;

            Validate(
                actionContext,
                parameter,
                metadata,
                modelBindingContext,
                modelBindingResult);

            return modelBindingResult;
        }

        private void Validate(
            ActionContext actionContext,
            ParameterDescriptor parameterDescriptor,
            ModelMetadata modelMetadata,
            ModelBindingContext bindingContext,
            ModelBindingResult modelBindingResult)
        {
            if (modelBindingResult.IsModelSet || modelMetadata.IsRequired)
            {
                var visitor = new ValidationVisitor(
                    actionContext,
                    _validatorProvider,
                    _validatorCache,
                    _modelMetadataProvider,
                    bindingContext.ValidationState);

                var isValid = visitor.Validate(modelMetadata, bindingContext.ModelName, modelBindingResult.Model, skipNullAtTopLevel: !modelMetadata.IsRequired);
                if (!isValid)
                {
                    // Only add one validation error in the case where both [Required] and
                    // [BindRequired] are specified
                    return;
                }
            }

            if (!modelBindingResult.IsModelSet && modelMetadata.IsBindingRequired)
            {
                var fieldName = bindingContext.FieldName ?? modelMetadata.BinderModelName;
                var message = modelMetadata.ModelBindingMessageProvider.MissingBindRequiredValueAccessor(fieldName);
                actionContext.ModelState.TryAddModelError(bindingContext.ModelName, message);
            }
        }
    }
}
