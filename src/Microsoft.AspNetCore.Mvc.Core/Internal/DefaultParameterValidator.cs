using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class DefaultParameterValidator : IParameterValidator
    {
        private readonly ValidatorCache _validatorCache;
        private readonly IModelValidatorProvider _validatorProvider;
        private readonly IExtendedModelMetadataProvider _modelMetadataProvider;

        public DefaultParameterValidator(
            IModelMetadataProvider modelMetadataProvider,
            IList<IModelValidatorProvider> validatorProviders)
        {
            if (modelMetadataProvider == null)
            {
                throw new ArgumentNullException(nameof(modelMetadataProvider));
            }

            if (validatorProviders == null)
            {
                throw new ArgumentNullException(nameof(validatorProviders));
            }

            _validatorCache = new ValidatorCache();
            _validatorProvider = new CompositeModelValidatorProvider(validatorProviders);

            // Only the extended metadata provider interface provides information about properties,
            // so if we don't get that type, this validator will do nothing.
            _modelMetadataProvider = modelMetadataProvider as IExtendedModelMetadataProvider;
        }

        public void Validate(ActionContext actionContext, ParameterDescriptor parameterDescriptor, ModelMetadata modelMetadata, bool isValueSet, object parameterValue, ModelBindingContext bindingContext)
        {
            if (_modelMetadataProvider == null)
            {
                return;
            }
            
            if (isValueSet || modelMetadata.IsRequired)
            {
                var visitor = new ValidationVisitor(
                    actionContext,
                    _validatorProvider,
                    _validatorCache,
                    _modelMetadataProvider,
                    bindingContext.ValidationState);

                var isValid = visitor.Validate(modelMetadata, bindingContext.ModelName, parameterValue, skipNullAtTopLevel: !modelMetadata.IsRequired);
                if (!isValid)
                {
                    // Only add one validation error in the case where both [Required] and
                    // [BindRequired] are specified
                    return;
                }
            }

            if (!isValueSet && modelMetadata.IsBindingRequired)
            {
                var fieldName = bindingContext.FieldName ?? modelMetadata.BinderModelName;
                var message = modelMetadata.ModelBindingMessageProvider.MissingBindRequiredValueAccessor(fieldName);
                actionContext.ModelState.TryAddModelError(bindingContext.ModelName, message);
            }
        }
    }
}
