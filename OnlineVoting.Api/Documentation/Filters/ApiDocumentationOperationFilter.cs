using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions;
using OnlineVoting.Api.Documentation.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineVoting.Api.Documentation.Filters
{
    public sealed class ApiDocumentationOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ApiDocumentationAttribute? attribute = context.MethodInfo
                .GetCustomAttributes(typeof(ApiDocumentationAttribute), false)
                .Cast<ApiDocumentationAttribute>()
                .FirstOrDefault();

            if (attribute is null)
                return;

            if (!ApiDocumentationRegistry.Operations.TryGetValue(attribute.Key, out ApiOperationDocumentation? documentation))
                return;

            operation.Summary = documentation.Summary;
            operation.Description = documentation.Description;

            foreach (KeyValuePair<string, ApiResponseDocumentation> response in documentation.Responses)
            {
                ApiResponseDocumentation responseDocumentation = response.Value;

                OpenApiResponse openApiResponse = new()
                {
                    Description = responseDocumentation.Description
                };

                if (responseDocumentation.ResponseType is not null)
                {
                    string contentType = IsProblemDetailsType(responseDocumentation.ResponseType)
                        ? "application/problem+json"
                        : "application/json";

                    openApiResponse.Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [contentType] = new OpenApiMediaType
                        {
                            Schema = context.SchemaGenerator.GenerateSchema(responseDocumentation.ResponseType, context.SchemaRepository)
                        }
                    };
                }

                operation.Responses[response.Key] = openApiResponse;
            }

            const string internalServerErrorStatusCode = "500";

            if (!operation.Responses.ContainsKey(internalServerErrorStatusCode))
            {
                operation.Responses[internalServerErrorStatusCode] = new OpenApiResponse
                {
                    Description = "An unexpected server error occurred.",

                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new OpenApiMediaType
                        {
                            Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
                        }
                    }
                };
            }
        }

        private static bool IsProblemDetailsType(Type responseType)
        {
            return typeof(ProblemDetails).IsAssignableFrom(responseType);
        }
    }
}