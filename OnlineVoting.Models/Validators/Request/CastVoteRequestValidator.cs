using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators.Request
{
    public class CastVoteRequestValidator : AbstractValidator<CastVoteRequest>
    {
        public CastVoteRequestValidator()
        {
            RuleFor(x => x.RegisteredVoterId)
                .NotEmpty();

            RuleFor(x => x.VotingCode)
                .NotEmpty();

            RuleFor(x => x.ElectionPositionId)
                .NotEmpty();

            RuleFor(x => x.ContestantId)
                .NotEmpty();
        }
    }
}