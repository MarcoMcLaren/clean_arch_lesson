namespace Domain.Exceptions;

public class InvalidRentalOperationException : Exception
{
    public InvalidRentalOperationException(string message) : base(message) { }
}
