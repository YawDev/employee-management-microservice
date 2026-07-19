namespace Employee.Management.Core.Exceptions
{
    // Requested resource does not exist — maps to 404 in ExceptionHandlingMiddleware.
    // Subclasses BadRequestException so it is treated as an expected (Warning-level) failure,
    // mirroring UserNotFoundException.
    public class NotFoundException : BadRequestException
    {
        public NotFoundException()
        {
        }
        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}
