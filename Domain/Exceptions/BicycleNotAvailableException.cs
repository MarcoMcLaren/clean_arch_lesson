namespace Domain.Exceptions;

public class BicycleNotAvailableException : Exception
{
    public BicycleNotAvailableException(Guid id)
        : base($"Bicycle with ID '{id}' is not currently available for rental.") { }
}
