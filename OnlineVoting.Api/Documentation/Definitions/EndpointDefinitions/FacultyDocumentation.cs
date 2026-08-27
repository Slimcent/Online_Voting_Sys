using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class FacultyDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [FacultyDocumentationKeys.CreateFaculty] = new ApiOperationDocumentation
            {
                Summary = "Creates a faculty.",

                Description = "Creates one or more faculties using the supplied name or names.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The faculty was created successfully.",

                        ResponseType = typeof(string)
                    },

                    ["400"] = CommonApiResponses.BadRequest("The faculty information is invalid."),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["409"] = CommonApiResponses.Conflict("A faculty with the supplied name already exists.")
                }
            },

            [FacultyDocumentationKeys.GetFaculties] = new ApiOperationDocumentation
            {
                Summary = "Gets faculties.",

                Description = "Returns a paginated list of faculties.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The faculties were retrieved successfully.",

                        ResponseType = typeof(PagedResponse<FacultyResponse>)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [FacultyDocumentationKeys.GetFaculty] = new ApiOperationDocumentation
            {
                Summary = "Gets a faculty.",

                Description = "Returns the faculty with the specified id.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The faculty was retrieved successfully.",

                        ResponseType = typeof(FacultyResponse)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified faculty could not be found.")
                }
            },

            [FacultyDocumentationKeys.UpdateFaculty] = new ApiOperationDocumentation
            {
                Summary = "Updates a faculty.",

                Description = "Updates the faculty with the specified id using the supplied name.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The faculty was updated successfully.",

                        ResponseType = typeof(string)
                    },

                    ["400"] = CommonApiResponses.BadRequest("The faculty information is invalid."),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified faculty could not be found."),

                    ["409"] = CommonApiResponses.Conflict("A faculty with the supplied name already exists.")
                }
            },

            [FacultyDocumentationKeys.ToggleFacultyActivation] = new ApiOperationDocumentation
            {
                Summary = "Toggles faculty activation.",

                Description = "Toggles the active status of the faculty with the specified id.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The faculty activation status was updated successfully.",

                        ResponseType = typeof(string)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified faculty could not be found.")
                }
            },

            [FacultyDocumentationKeys.DeleteFaculty] = new ApiOperationDocumentation
            {
                Summary = "Deletes a faculty.",

                Description = "Deletes the faculty with the specified id.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The faculty was deleted successfully.",

                        ResponseType = typeof(string)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified faculty could not be found.")
                }
            },

            [FacultyDocumentationKeys.GetFacultiesWithDepartments] = new ApiOperationDocumentation
            {
                Summary = "Gets faculties with departments.",

                Description = "Returns a paginated list of faculties including their departments.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The faculties and their departments were retrieved successfully.",

                        ResponseType = typeof(PagedResponse<FacultyResponse>)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [FacultyDocumentationKeys.GetFacultyWithDepartments] = new ApiOperationDocumentation
            {
                Summary = "Gets a faculty with departments.",

                Description = "Returns the faculty with the specified id including its departments.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The faculty and its departments were retrieved successfully.",

                        ResponseType = typeof(FacultyResponse)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified faculty could not be found.")
                }
            }
        };
    }
}