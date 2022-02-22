#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

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