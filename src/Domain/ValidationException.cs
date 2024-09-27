namespace DynamicObjectApi.Domain;

public class ValidationException : Exception{
    internal ValidationException(string message) : base(message){
    }
}