namespace BankingSystem.Domain.Errors
{
    public sealed record CustomError(string Code, string Message)
    {
        private const string RecordNotFoundCode = "RecordNotFound";
        private const string ValidationErrorCode = "ValidationError";
        private const string ServerErrorCode = "ServerError";

        public static readonly CustomError None = new(string.Empty, string.Empty);
        
        public static CustomError RecordNotFound(string message)
        {
            return new CustomError(RecordNotFoundCode, message);
        }

        public static CustomError ServerError(string message)
        {
            return new CustomError(ServerErrorCode, message);
        }

        public static CustomError ValidationError(string message)
        {
            return new CustomError(ValidationErrorCode, message);
        }
    }
}