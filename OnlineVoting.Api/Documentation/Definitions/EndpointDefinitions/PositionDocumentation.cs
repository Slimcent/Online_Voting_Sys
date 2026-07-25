using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class PositionDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
            {
                [PositionDocumentationKeys.GetAllPagedPositions] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets paginated positions.",

                        Description =
                            "Returns a paginated, searchable and sortable list " +
                            "of positions.",

                        Responses =
                            CreatePagedPositionResponses(
                                "The paginated positions were returned successfully.")
                    },

                [PositionDocumentationKeys.GetAllPagedActivePositions] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets paginated active positions.",

                        Description =
                            "Returns a paginated, searchable and sortable list " +
                            "of active positions.",

                        Responses =
                            CreatePagedPositionResponses(
                                "The paginated active positions were returned successfully.")
                    },

                [PositionDocumentationKeys.GetAllPagedDeletedPositions] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets paginated deleted positions.",

                        Description =
                            "Returns a paginated, searchable and sortable list " +
                            "of deleted positions.",

                        Responses =
                            CreatePagedPositionResponses(
                                "The paginated deleted positions were returned successfully.")
                    },

                [PositionDocumentationKeys.GetAllPositions] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets all positions.",

                        Description =
                            "Returns all positions regardless of their current status.",

                        Responses =
                            CreatePositionListResponses(
                                "The positions were returned successfully.")
                    },

                [PositionDocumentationKeys.GetAllActivePositions] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets all active positions.",

                        Description =
                            "Returns all positions that are currently active.",

                        Responses =
                            CreatePositionListResponses(
                                "The active positions were returned successfully.")
                    },

                [PositionDocumentationKeys.GetAllDeletedPositions] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets all deleted positions.",

                        Description =
                            "Returns all positions that have been deleted.",

                        Responses =
                            CreatePositionListResponses(
                                "The deleted positions were returned successfully.")
                    },

                [PositionDocumentationKeys.GetPositionById] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Gets a position by ID.",

                        Description =
                            "Returns the position identified by the supplied ID.",

                        Responses =
                            new Dictionary<string, ApiResponseDocumentation>
                            {
                                ["200"] =
                                    new ApiResponseDocumentation
                                    {
                                        Description =
                                            "The position was returned successfully.",

                                        ResponseType =
                                            typeof(PositionResponse)
                                    },

                                ["400"] =
                                    CommonApiResponses.BadRequest(),

                                ["401"] =
                                    CommonApiResponses.Unauthorized(),

                                ["403"] =
                                    CommonApiResponses.Forbidden(),

                                ["404"] =
                                    CommonApiResponses.NotFound(
                                        "The position could not be found.")
                            }
                    },

                [PositionDocumentationKeys.CreatePosition] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Creates a position.",

                        Description =
                            "Creates a new position using the supplied name.",

                        Responses =
                            CreateStringResponses(
                                "The position was created successfully.",
                                includeConflict: true)
                    },

                [PositionDocumentationKeys.PatchPosition] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Partially updates a position.",

                        Description =
                            "Applies the supplied JSON Patch operations to the " +
                            "position identified by the ID.",

                        Responses =
                            CreateStringResponses(
                                "The position was updated successfully.",
                                includeNotFound: true)
                    },

                [PositionDocumentationKeys.UpdatePosition] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Updates a position.",

                        Description =
                            "Updates the position identified by the supplied ID.",

                        Responses =
                            CreateStringResponses(
                                "The position was updated successfully.",
                                includeNotFound: true)
                    },

                [PositionDocumentationKeys.DeletePosition] =
                    new ApiOperationDocumentation
                    {
                        Summary = "Deletes a position.",

                        Description =
                            "Deletes the position identified by the supplied ID.",

                        Responses =
                            CreateStringResponses(
                                "The position was deleted successfully.",
                                includeNotFound: true)
                    }
            };

        private static IReadOnlyDictionary<
            string,
            ApiResponseDocumentation> CreatePagedPositionResponses(
                string successDescription)
        {
            return new Dictionary<string, ApiResponseDocumentation>
            {
                ["200"] =
                    new ApiResponseDocumentation
                    {
                        Description = successDescription,

                        ResponseType =
                            typeof(PagedResponse<PositionResponse>)
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
            ApiResponseDocumentation> CreatePositionListResponses(
                string successDescription)
        {
            return new Dictionary<string, ApiResponseDocumentation>
            {
                ["200"] =
                    new ApiResponseDocumentation
                    {
                        Description = successDescription,

                        ResponseType =
                            typeof(IEnumerable<PositionResponse>)
                    },

                ["401"] =
                    CommonApiResponses.Unauthorized(),

                ["403"] =
                    CommonApiResponses.Forbidden()
            };
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

            if (includeNotFound)
            {
                responses["404"] =
                    CommonApiResponses.NotFound(
                        "The position could not be found.");
            }

            if (includeConflict)
            {
                responses["409"] =
                    CommonApiResponses.Conflict(
                        "A position with the supplied name already exists.");
            }

            return responses;
        }
    }
}