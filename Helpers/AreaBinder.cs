using ElPrisApi.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ElPrisApi.Helpers
{
    public class AreaBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

            if (Enum.TryParse(typeof(Area), value, true, out var result) && Enum.IsDefined(typeof(Area), result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            else
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid value '{value}' for Area. Valid values are SE1, SE2, SE3, SE4.");
            }

            return Task.CompletedTask;
        }
    }

}
