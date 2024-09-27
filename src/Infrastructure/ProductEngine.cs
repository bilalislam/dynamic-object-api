using DynamicObjectApi.Domain;
using Newtonsoft.Json.Linq;

namespace DynamicObjectApi.Infrastructure;

public class ProductEngine : IRuleEngine{
    public Task ValidateRule(JObject data, JToken? ruleSet){
        if (ruleSet?["nameCannotBeEmpty"]?.Value<bool>() == true){
            var name = data["name"]?.Value<string>();
            if (string.IsNullOrWhiteSpace(name)){
                throw new ValidationException("Product name cannot be empty.");
            }
        }

        if (ruleSet?["priceMustBePositive"]?.Value<bool>() == true){
            var price = data["price"]?.Value<decimal>() ?? 0;
            if (price <= 0){
                throw new ValidationException("Product price must be positive.");
            }
        }

        return Task.CompletedTask;
    }
}