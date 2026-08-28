using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineVoting.Api.Documentation.Filters
{
    public class DeviceLocationHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            string httpMethod = context.ApiDescription.HttpMethod ?? string.Empty;

            if (httpMethod != "POST"
                && httpMethod != "PUT"
                && httpMethod != "PATCH"
                && httpMethod != "DELETE")
            {
                return;
            }

            operation.Parameters ??= new List<IOpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-Latitude",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Optional device latitude.",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Number,
                    Format = "double"
                }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-Longitude",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Optional device longitude.",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Number,
                    Format = "double"
                }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-Accuracy",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Optional device location accuracy in metres.",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Number,
                    Format = "double"
                }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-Location-Captured-At",
                In = ParameterLocation.Header,
                Required = false,
                Description = "UTC time when the device location was captured.",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "date-time"
                }
            });
        }
    }
}