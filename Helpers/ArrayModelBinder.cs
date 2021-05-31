using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CourseLibrary.API.Helpers
{
    /// <summary>
    /// Method which converting string format Ids from route value,
    /// to the Enumerable of Guid.
    /// </summary>
    public class ArrayModelBinder: IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Our binder works only on enumerable types
            if (bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
            
            // Get the inputted value through the value provider (for us example : string with guid ids)
            // bindingContext.ModelName - It is a key, which looking up the value, which us send
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();
            
            // If that value is null or whitespace, we return null
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }
            
            // The value is not null or whitespace,
            // and the type of the model is enumerable.
            // Get enumerable`s type, and a converter.
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);
            
            // Convert each item in the value list to the enumerable type (Guid)
            // StringSplitOptions.RemoveEmptyEntries method returning the value does not include array elements that contain an empty string.
            var values = value.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => converter.ConvertFromString(id.Trim()))
                .ToArray();
            
            // Create an array of that type, and set is as the Model value
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;
            
            // return a successful result, passing in the Model
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}