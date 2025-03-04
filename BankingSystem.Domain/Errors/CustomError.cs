namespace BankingSystem.Domain.Errors
{
    public sealed record CustomError(string Code, string Message,ErrorType ErrorType = ErrorType.Failure)
    {
        private const string RecordNotFoundCode = "RecordNotFound";
        private const string ValidationErrorCode = "ValidationError";
        private const string ServerErrorCode = "ServerError";
        private const string ConflictErrorCode = "ConflictError";
        private const string AccessDeniedErrorCode = "AccessDenied";
        private const string AccessForbiddenErrorCode = "AccessForbidden";
        
        public static readonly CustomError None = new(string.Empty, string.Empty);
        public static readonly CustomError NullValue = new("Error.NullValue","Null value was provided");
        
        public static CustomError Failure(string description) =>
            new(ServerErrorCode, description);

        public static CustomError NotFound(string description) =>
            new(RecordNotFoundCode, description, ErrorType.NotFound);

        public static CustomError Validation(string description) =>
            new(ValidationErrorCode, description, ErrorType.Validation);

        public static CustomError Conflict(string code, string description) =>
            new(ConflictErrorCode, description, ErrorType.Conflict);

        public static CustomError AccessUnAuthorized(string code, string description) =>
            new(AccessDeniedErrorCode, description, ErrorType.AccessUnAuthorized);

        public static CustomError AccessForbidden(string code, string description) =>
            new(AccessForbiddenErrorCode, description, ErrorType.AccessForbidden);
    }
}