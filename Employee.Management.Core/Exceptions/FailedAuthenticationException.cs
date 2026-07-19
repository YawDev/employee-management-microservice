namespace Employee.Management.Core.Exceptions
{
    public class FailedAuthenticationException : BadRequestException
    {
        public FailedAuthenticationException()
        {
        }
        public FailedAuthenticationException(string message)
            : base(message)
        {
        }
    }
}
