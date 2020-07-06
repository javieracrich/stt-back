using Common;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Api
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            // Policy names map to scopes
            var requiredScopes = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy)
                .Distinct();

            if (requiredScopes.Any())
            {
                operation.Security = new List<IDictionary<string, IEnumerable<string>>>
                {
                    new Dictionary<string, IEnumerable<string>> {{"Bearer", new string[] { }}}
                };
            }
        }
    }

    public class SetRightContentTypes : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {

            var list = new List<string>()
            {
                "GetAllSchools",
                "PostSchool",
                "PostTest"
            };

            if (!list.Contains(operation.OperationId))
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                operation.Parameters.Insert(0, new NonBodyParameter
                {
                    Name = Constants.SchoolCodeHeader,
                    In = "header",
                    Type = "string",
                    Format = "uuid",
                    Required = true
                });
            }

            OnlyJson(operation.Produces);
            OnlyJson(operation.Consumes);

            void OnlyJson(IList<string> types)
            {
                if (types?.Any() == true)
                {
                    types.Clear();
                    types.Add("application/json");
                }
            }
        }
    }
}