namespace DynamicObjectApi.Models;

public class ValidationException : Exception{
    internal ValidationException(string message) : base(message){
    }
}