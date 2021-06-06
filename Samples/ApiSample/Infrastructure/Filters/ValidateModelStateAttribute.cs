using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiSample.Infrastructure.Filters
{
    /// <summary>
    /// Model state validation filter attribute
    /// </summary>
    /// <seealso cref="ActionFilterAttribute" />
    public sealed class ValidateModelStateAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateModelStateAttribute"/> class.
        /// </summary>
        public ValidateModelStateAttribute()
        {
            Order = -2001;
        }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}