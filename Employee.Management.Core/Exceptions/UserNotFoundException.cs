namespace Employee.Management.Core.Exceptions
{
    public class UserNotFoundException : BadRequestException
    {
        public UserNotFoundException()
        {
        }
        public UserNotFoundException(string message)
            : base(message)
        {
        }
    }
}
