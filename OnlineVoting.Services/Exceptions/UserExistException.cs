namespace OnlineVoting.Services.Exceptions
{
    public class UserExistException : NotFoundException
    {
        public UserExistException(string email)
            : base($"The user with email: {email} already exist in the database.")
        {
        }
    }
}
