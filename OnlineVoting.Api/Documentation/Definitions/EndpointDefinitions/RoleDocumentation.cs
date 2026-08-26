using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class RoleDocumentation
    {
        public static readonly IReadOnlyDictionary<
            string,
            ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
            {
                [RoleDocumentationKeys.GetAllRoles] = new ApiOperationDocumentation
                    {
                        Summary = "Gets all roles.",

                        Description = "Returns all roles, regardless of their current status.",

                        Responses = CreateRoleListResponses("The roles were returned successfully.")
                    },

                [RoleDocumentationKeys.GetAllActiveRoles] = new ApiOperationDocumentation
                    {
                        Summary = "Gets all active roles.",

                        Description = "Returns all roles that are currently active.",

                        Responses = CreateRoleListResponses("The active roles were returned successfully.")
                    },

                [RoleDocumentationKeys.GetAllDeactivatedRoles] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets all deactivated roles.",

                        Description =
                            "Returns all roles that are currently deactivated.",

                        Responses =
                            CreateRoleListResponses(
                                "The deactivated roles were returned successfully.")
                    },

                [RoleDocumentationKeys.GetAllPagedRoles] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets paginated roles.",

                        Description =
                            "Returns a paginated, searchable and sortable list " +
                            "of roles.",

                        Responses =
                            CreatePagedRoleResponses(
                                "The paginated roles were returned successfully.")
                    },

                [RoleDocumentationKeys.GetAllPagedActiveRoles] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets paginated active roles.",

                        Description =
                            "Returns a paginated, searchable and sortable list " +
                            "of active roles.",

                        Responses =
                            CreatePagedRoleResponses(
                                "The paginated active roles were returned successfully.")
                    },

                [RoleDocumentationKeys.GetAllPagedDeactivatedRoles] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets paginated deactivated roles.",

                        Description =
                            "Returns a paginated, searchable and sortable list " +
                            "of deactivated roles.",

                        Responses =
                            CreatePagedRoleResponses(
                                "The paginated deactivated roles were returned successfully.")
                    },

                [RoleDocumentationKeys.GetUserRoles] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets the roles assigned to a user.",

                        Description =
                            "Returns the roles assigned to the user identified " +
                            "by the supplied username.",

                        Responses =
                            CreateOperationResponses(
                                "The user's roles were returned successfully.",
                                includeNotFound: true)
                    },

                [RoleDocumentationKeys.CreateRole] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Creates a role.",

                        Description =
                            "Creates a new role using the supplied role name.",

                        Responses =
                            CreateOperationResponses(
                                "The role was created successfully.",
                                includeConflict: true)
                    },

                [RoleDocumentationKeys.EditRole] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Updates a role.",

                        Description =
                            "Updates the role identified by the supplied ID.",

                        Responses =
                            CreateStringResponses(
                                "The role was updated successfully.",
                                includeNotFound: true)
                    },

                [RoleDocumentationKeys.AddUserToRole] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Adds a user to a role.",

                        Description =
                            "Assigns the specified role to the user identified " +
                            "by the supplied email address.",

                        Responses =
                            CreateOperationResponses(
                                "The user was added to the role successfully.",
                                includeNotFound: true,
                                includeConflict: true)
                    },

                [RoleDocumentationKeys.RemoveUserFromRole] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Removes a user from a role.",

                        Description =
                            "Removes the specified role from the user identified " +
                            "by the supplied email address.",

                        Responses =
                            CreateOperationResponses(
                                "The user was removed from the role successfully.",
                                includeNotFound: true)
                    },

                [RoleDocumentationKeys.ToggleRoleStatus] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Toggles a role's status.",

                        Description =
                            "Activates or deactivates the role identified by " +
                            "the supplied ID.",

                        Responses =
                            CreateStringResponses(
                                "The role status was changed successfully.",
                                includeNotFound: true)
                    },

                [RoleDocumentationKeys.DeleteRoleById] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Deletes a role by ID.",

                        Description =
                            "Deletes the role identified by the supplied ID.",

                        Responses =
                            CreateStringResponses(
                                "The role was deleted successfully.",
                                includeNotFound: true)
                    },

                [RoleDocumentationKeys.DeleteRoleByName] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Deletes a role by name.",

                        Description =
                            "Deletes the role matching the supplied role name.",

                        Responses =
                            CreateStringResponses(
                                "The role was deleted successfully.",
                                includeNotFound: true)
                    }
            };

        private static IReadOnlyDictionary<
            string,
            ApiResponseDocumentation> CreateRoleListResponses(
                string successDescription)
        {
            return new Dictionary<string, ApiResponseDocumentation>
            {
                ["201"] =
                    new ApiResponseDocumentation
                    {
                        Description = successDescription,
                        ResponseType = typeof(IEnumerable<RoleResponse>)
                    },

                ["401"] =
                    CommonApiResponses.Unauthorized(),

                ["403"] =
                    CommonApiResponses.Forbidden()
            };
        }

        private static IReadOnlyDictionary<
            string,
            ApiResponseDocumentation> CreatePagedRoleResponses(
                string successDescription)
        {
            return new Dictionary<string, ApiResponseDocumentation>
            {
                ["200"] =
                    new ApiResponseDocumentation
                    {
                        Description = successDescription,
                        ResponseType =
                            typeof(PagedResponse<RoleResponse>)
                    },

                ["400"] =
                    CommonApiResponses.BadRequest(),

                ["401"] =
                    CommonApiResponses.Unauthorized(),

                ["403"] =
                    CommonApiResponses.Forbidden()
            };
        }

        private static IReadOnlyDictionary<
            string,
            ApiResponseDocumentation> CreateOperationResponses(
                string successDescription,
                bool includeNotFound = false,
                bool includeConflict = false)
        {
            Dictionary<string, ApiResponseDocumentation> responses =
                new()
                {
                    ["200"] =
                        new ApiResponseDocumentation
                        {
                            Description = successDescription
                        },

                    ["400"] =
                        CommonApiResponses.BadRequest(),

                    ["401"] =
                        CommonApiResponses.Unauthorized(),

                    ["403"] =
                        CommonApiResponses.Forbidden()
                };

            AddOptionalResponses(
                responses,
                includeNotFound,
                includeConflict);

            return responses;
        }

        private static IReadOnlyDictionary<
            string,
            ApiResponseDocumentation> CreateStringResponses(
                string successDescription,
                bool includeNotFound = false,
                bool includeConflict = false)
        {
            Dictionary<string, ApiResponseDocumentation> responses =
                new()
                {
                    ["200"] =
                        new ApiResponseDocumentation
                        {
                            Description = successDescription,
                            ResponseType = typeof(string)
                        },

                    ["400"] =
                        CommonApiResponses.BadRequest(),

                    ["401"] =
                        CommonApiResponses.Unauthorized(),

                    ["403"] =
                        CommonApiResponses.Forbidden()
                };

            AddOptionalResponses(
                responses,
                includeNotFound,
                includeConflict);

            return responses;
        }

        private static void AddOptionalResponses(
            IDictionary<string, ApiResponseDocumentation> responses,
            bool includeNotFound,
            bool includeConflict)
        {
            if (includeNotFound)
            {
                responses["404"] =
                    CommonApiResponses.NotFound(
                        "The requested user or role could not be found.");
            }

            if (includeConflict)
            {
                responses["409"] =
                    CommonApiResponses.Conflict(
                        "The requested role assignment already exists.");
            }
        }
    }
}