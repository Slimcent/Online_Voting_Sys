using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class StaffDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [StaffDocumentationKeys.GetAllActiveStaff] = new ApiOperationDocumentation
            {
                Summary = "Gets all active staff.",
                Description = "Returns all staff accounts that are currently active.",

                Responses = CreateStaffListResponses("The active staff members were returned successfully.")
            },

            [StaffDocumentationKeys.GetAllDeletedStaff] = new ApiOperationDocumentation
            {
                Summary = "Gets all deleted staff.",
                Description = "Returns all staff accounts that have been deleted.",

                Responses = CreateStaffListResponses("The deleted staff members were returned successfully.")
            },

            [StaffDocumentationKeys.GetAllPagedStaff] = new ApiOperationDocumentation
            {
                Summary = "Gets paginated staff.",
                Description = "Returns a paginated, searchable and sortable list of staff members.",

                Responses = CreatePagedStaffResponses("The paginated staff list was returned successfully.")
            },

            [StaffDocumentationKeys.GetAllPagedActiveStaff] = new ApiOperationDocumentation
            {
                Summary = "Gets paginated active staff.",
                Description = "Returns a paginated, searchable and sortable list of active staff members.",

                Responses = CreatePagedStaffResponses("The paginated active staff list was returned successfully.")
            },

            [StaffDocumentationKeys.GetAllPagedDeletedStaff] = new ApiOperationDocumentation
            {
                Summary = "Gets paginated deleted staff.",
                Description = "Returns a paginated, searchable and sortable list of deleted staff members.",

                Responses = CreatePagedStaffResponses("The paginated deleted staff list was returned successfully.")
            },

            [StaffDocumentationKeys.GetStaffById] = new ApiOperationDocumentation
            {
                Summary = "Gets a staff member by ID.",
                Description = "Returns the staff member identified by the supplied ID.",
                Responses = CreateSingleStaffResponses()
            },

            [StaffDocumentationKeys.GetStaffByEmail] = new ApiOperationDocumentation
            {
                Summary = "Gets a staff member by email.",
                Description = "Returns the staff member associated with the supplied email address.",

                Responses = CreateSingleStaffResponses()
            },

            [StaffDocumentationKeys.CreateStaff] = new ApiOperationDocumentation
            {
                Summary = "Creates a staff member.",
                Description = "Creates a new staff account using the supplied user information.",

                Responses = CreateStringResponse("The staff member was created successfully.", includeNotFound: true)
            },

            [StaffDocumentationKeys.UpdateStaff] = new ApiOperationDocumentation
            {
                Summary = "Partially updates a staff member.",
                Description = "Applies the supplied JSON Patch operations to the staff member identified by the ID.",

                Responses = CreateStringResponse("The staff member was updated successfully.", includeNotFound: true)
            },

            [StaffDocumentationKeys.EditStaff] = new ApiOperationDocumentation
            {
                Summary = "Updates a staff member.",
                Description = "Replaces the editable information of the staff member identified by the ID.",

                Responses = CreateStringResponse("The staff member was updated successfully.", includeNotFound: true)
            },

            [StaffDocumentationKeys.PatchStaffAddress] = new ApiOperationDocumentation
            {
                Summary = "Partially updates a staff address.",
                Description = "Applies the supplied JSON Patch operations to the address of the staff member identified by the ID.",

                Responses = CreateStringResponse("The staff address was updated successfully.", includeNotFound: true)
            },

            [StaffDocumentationKeys.ToggleStaffStatus] = new ApiOperationDocumentation
             {
                 Summary = "Toggles a staff member's status.",
                 Description = "Changes the active status of the staff member " +   "identified by the supplied ID.",

                 Responses = CreateStringResponse("The staff status was changed successfully.", includeNotFound: true)
             },

            [StaffDocumentationKeys.UpdateStaffAddress] = new ApiOperationDocumentation
            {
                Summary = "Updates a staff address.",
                Description = "Updates the complete address of the staff member identified by the supplied ID.",

                Responses = CreateStringResponse("The staff address was updated successfully.", includeNotFound: true)
            },

            [StaffDocumentationKeys.GetTotalNumberOfStaff] = new ApiOperationDocumentation
            {
                Summary = "Gets the total number of staff members.",
                Description = "Returns the total number of staff records.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The total number of staff members was returned successfully.",

                        ResponseType = typeof(string)
                    },

                    ["400"] = CommonApiResponses.BadRequest("No staff records were found."),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [StaffDocumentationKeys.DeleteStaff] = new ApiOperationDocumentation
            {
                Summary = "Deletes a staff member.",
                Description = "Deletes the staff member identified by the supplied ID.",

                Responses = CreateStringResponse("The staff member was deleted successfully.", includeNotFound: true)
            }
        };

        private static IReadOnlyDictionary<string, ApiResponseDocumentation> CreateStaffListResponses(string successDescription)
        {
            return new Dictionary<string, ApiResponseDocumentation>
            {
                ["200"] = new ApiResponseDocumentation
                {
                    Description = successDescription,
                    ResponseType = typeof(IEnumerable<StaffResponse>)
                },

                ["400"] = CommonApiResponses.BadRequest("No staff records were found."),

                ["401"] = CommonApiResponses.Unauthorized(),

                ["403"] = CommonApiResponses.Forbidden()
            };
        }

        private static IReadOnlyDictionary<string, ApiResponseDocumentation> CreatePagedStaffResponses(string successDescription)
        {
            return new Dictionary<string, ApiResponseDocumentation>
            {
                ["200"] = new ApiResponseDocumentation
                {
                    Description = successDescription,
                    ResponseType = typeof(PagedResponse<StaffResponse>)
                },

                ["400"] = CommonApiResponses.BadRequest(),

                ["401"] = CommonApiResponses.Unauthorized(),

                ["403"] = CommonApiResponses.Forbidden()
            };
        }

        private static IReadOnlyDictionary<string, ApiResponseDocumentation> CreateSingleStaffResponses()
        {
            return new Dictionary<string, ApiResponseDocumentation>
            {
                ["200"] = new ApiResponseDocumentation
                {
                    Description = "The staff member was returned successfully.",
                    ResponseType = typeof(StaffResponse)
                },

                ["400"] = CommonApiResponses.BadRequest(),

                ["401"] = CommonApiResponses.Unauthorized(),

                ["403"] = CommonApiResponses.Forbidden(),

                ["404"] = CommonApiResponses.NotFound("The staff member could not be found.")
            };
        }

        private static IReadOnlyDictionary<string, ApiResponseDocumentation> CreateStringResponse(string successDescription, bool includeNotFound)
        {
            Dictionary<string, ApiResponseDocumentation> responses = new()
            {
                ["200"] = new ApiResponseDocumentation
                {
                    Description = successDescription,
                    ResponseType = typeof(string)
                },

                ["400"] = CommonApiResponses.BadRequest(),

                ["401"] = CommonApiResponses.Unauthorized(),

                ["403"] = CommonApiResponses.Forbidden()
            };

            if (includeNotFound)
            {
                responses["404"] = CommonApiResponses.NotFound("The staff member could not be found.");
            }

            return responses;
        }
    }
}