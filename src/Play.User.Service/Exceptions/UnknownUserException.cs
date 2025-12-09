namespace Play.User.Service.Exceptions
{
    [Serializable]
    internal class UnknownUserException(Guid UserId) : Exception($"Unknown user '{UserId}'")
    {
        public Guid UserId { get; } = UserId;
    }
}