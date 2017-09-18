using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Validation
{
    public interface IParameterValidator
    {
        void Validate(ActionContext actionContext, ParameterDescriptor parameterDescriptor, ModelMetadata modelMetadata, bool isValueSet, object parameterValue, ModelBindingContext bindingContext);
    }
}
