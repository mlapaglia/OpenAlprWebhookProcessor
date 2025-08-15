namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers
{
    public class UserDto
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool TwoFactorEnabled { get; set; }
    }
}