namespace Domain.Exceptions;

public class BicycleNotFoundException : Exception
{
    public BicycleNotFoundException(Guid id)
        : base($"Bicycle with ID '{id}' was not found.") { }
}
