using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class DepartmentDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [DepartmentDocumentationKeys.CreateDepartment] = new ApiOperationDocumentation
            {
                Summary = "Creates a department.",

                Description = "Creates one or more departments under the specified faculty.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The department was created successfully.",

                        ResponseType = typeof(string)
                    },

                    ["400"] = CommonApiResponses.BadRequest("The department information is invalid."),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified faculty could not be found."),

                    ["409"] = CommonApiResponses.Conflict("A department with the supplied name already exists.")
                }
            },

            [DepartmentDocumentationKeys.GetDepartments] = new ApiOperationDocumentation
            {
                Summary = "Gets departments.",

                Description = "Returns a paginated list of departments.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The departments were retrieved successfully.",

                        ResponseType = typeof(PagedResponse<DepartmentResponse>)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [DepartmentDocumentationKeys.GetDepartment] = new ApiOperationDocumentation
            {
                Summary = "Gets a department.",

                Description = "Returns the department with the specified id.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The department was retrieved successfully.",

                        ResponseType = typeof(DepartmentResponse)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified department could not be found.")
                }
            },

            [DepartmentDocumentationKeys.GetDepartmentsByFacultyId] = new ApiOperationDocumentation
            {
                Summary = "Gets departments by faculty.",

                Description = "Returns all departments belonging to the specified faculty.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The departments were retrieved successfully.",

                        ResponseType = typeof(IEnumerable<DepartmentResponse>)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified faculty could not be found.")
                }
            },

            [DepartmentDocumentationKeys.GetPagedDepartmentsByFacultyId] = new ApiOperationDocumentation
            {
                Summary = "Gets paged departments by faculty.",

                Description = "Returns a paginated list of departments belonging to the specified faculty.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The departments were retrieved successfully.",

                        ResponseType = typeof(PagedResponse<DepartmentResponse>)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified faculty could not be found.")
                }
            },

            [DepartmentDocumentationKeys.UpdateDepartment] = new ApiOperationDocumentation
            {
                Summary = "Updates a department.",

                Description = "Updates the department with the specified id using the supplied information.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The department was updated successfully.",

                        ResponseType = typeof(string)
                    },

                    ["400"] = CommonApiResponses.BadRequest("The department information is invalid."),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified department or faculty could not be found."),

                    ["409"] = CommonApiResponses.Conflict("A department with the supplied name already exists.")
                }
            },

            [DepartmentDocumentationKeys.ToggleDepartmentActivation] = new ApiOperationDocumentation
            {
                Summary = "Toggles department activation.",

                Description = "Toggles the active status of the department with the specified id.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The department activation status was updated successfully.",

                        ResponseType = typeof(string)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified department could not be found.")
                }
            },

            [DepartmentDocumentationKeys.DeleteDepartment] = new ApiOperationDocumentation
            {
                Summary = "Deletes a department.",

                Description = "Deletes the department with the specified id.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The department was deleted successfully.",

                        ResponseType = typeof(string)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified department could not be found.")
                }
            }
        };
    }
}