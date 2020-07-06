using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Filters
{
    public class SetThreadCurrentPrincipalFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User != null)
            {
                Thread.CurrentPrincipal = context.HttpContext.User;
            }
            base.OnActionExecuting(context);
        }
    }
}
