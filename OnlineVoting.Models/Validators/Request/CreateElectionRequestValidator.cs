using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using System.Globalization;

namespace OnlineVoting.Models.Validators.Request
{
    public class CreateElectionRequestValidator : AbstractValidator<CreateElectionRequest>
    {
        public CreateElectionRequestValidator()
        {
            RuleFor(x => x.Id)
                .Must(BeValidGuid)
                .When(x => !string.IsNullOrWhiteSpace(x.Id))
                .WithMessage("Election id must be a valid GUID.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.YearId)
                .GreaterThan(0);

            RuleFor(x => x.ElectionTypeId)
                .GreaterThan(0);

            RuleFor(x => x.ElectionStatusId)
                .GreaterThan(0);

            RuleFor(x => x.FacultyId)
                .GreaterThan(0)
                .When(x => x.FacultyId.HasValue);

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .When(x => x.DepartmentId.HasValue);

            RuleFor(x => x)
                .Must(x => !(x.FacultyId.HasValue && x.DepartmentId.HasValue))
                .WithMessage("FacultyId and DepartmentId cannot both be provided.");

            RuleFor(x => x.ApplicationStartAt)
                .Must(BeValidDate)
                .When(x => !string.IsNullOrWhiteSpace(x.ApplicationStartAt))
                .WithMessage("Application start date is invalid.");

            RuleFor(x => x.ApplicationEndAt)
                .Must(BeValidDate)
                .When(x => !string.IsNullOrWhiteSpace(x.ApplicationEndAt))
                .WithMessage("Application end date is invalid.");

            RuleFor(x => x)
                .Must(x => HaveMatchingDatePair(x.ApplicationStartAt, x.ApplicationEndAt))
                .WithMessage("Application start and end dates must either both be provided or both be empty.");

            RuleFor(x => x)
                .Must(x => EndIsAfterStart(x.ApplicationStartAt, x.ApplicationEndAt))
                .WithMessage("Application end date must be after the application start date.");

            RuleFor(x => x.VoterRegistrationStartAt)
                .Must(BeValidDate)
                .When(x => !string.IsNullOrWhiteSpace(x.VoterRegistrationStartAt))
                .WithMessage("Voter registration start date is invalid.");

            RuleFor(x => x.VoterRegistrationEndAt)
                .Must(BeValidDate)
                .When(x => !string.IsNullOrWhiteSpace(x.VoterRegistrationEndAt))
                .WithMessage("Voter registration end date is invalid.");

            RuleFor(x => x)
                .Must(x => HaveMatchingDatePair(x.VoterRegistrationStartAt, x.VoterRegistrationEndAt))
                .WithMessage("Voter registration start and end dates must either both be provided or both be empty.");

            RuleFor(x => x)
                .Must(x => EndIsAfterStart(x.VoterRegistrationStartAt, x.VoterRegistrationEndAt))
                .WithMessage("Voter registration end date must be after the voter registration start date.");

            RuleFor(x => x.VotingStartAt)
                .Must(BeValidDate)
                .When(x => !string.IsNullOrWhiteSpace(x.VotingStartAt))
                .WithMessage("Voting start date is invalid.");

            RuleFor(x => x.VotingEndAt)
                .Must(BeValidDate)
                .When(x => !string.IsNullOrWhiteSpace(x.VotingEndAt))
                .WithMessage("Voting end date is invalid.");

            RuleFor(x => x)
                .Must(x => HaveMatchingDatePair(x.VotingStartAt, x.VotingEndAt))
                .WithMessage("Voting start and end dates must either both be provided or both be empty.");

            RuleFor(x => x)
                .Must(x => EndIsAfterStart(x.VotingStartAt, x.VotingEndAt))
                .WithMessage("Voting end date must be after the voting start date.");
        }

        private static bool BeValidGuid(string? value)
        {
            return Guid.TryParse(value, out Guid _);
        }

        private static bool BeValidDate(string? value)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime _);
        }

        private static bool HaveMatchingDatePair(string? start, string? end)
        {
            bool hasStart = !string.IsNullOrWhiteSpace(start);
            bool hasEnd = !string.IsNullOrWhiteSpace(end);

            return hasStart == hasEnd;
        }

        private static bool EndIsAfterStart(string? start, string? end)
        {
            if (string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end))
                return true;

            bool validStart = DateTime.TryParse(start, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime startDate);
            bool validEnd = DateTime.TryParse(end, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime endDate);

            if (!validStart || !validEnd)
                return true;

            return endDate > startDate;
        }
    }
}