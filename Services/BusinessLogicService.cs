using Newtonsoft.Json.Linq;

namespace DynamicObjectApi.Services;

public class BusinessLogicService
{
    private readonly Dictionary<string, JObject> _rules = new()
    {
        { "order", JObject.Parse(File.ReadAllText("Rules/order.json")) },
        { "product", JObject.Parse(File.ReadAllText("Rules/product.json")) }
    };

    public void ApplyBusinessRules(string objectType, JObject data)
    {
        var rules = GetRulesForObjectType(objectType);
        if (rules == null)
        {
            throw new InvalidOperationException("operation not supported");
        }

        var ruleSet = rules["rules"];
        switch (objectType)
        {
            case "order":
            {
                if (ruleSet?["mustContainAtLeastOneProduct"]?.Value<bool>() == true)
                {
                    var products = data["products"] as JArray;
                    if (products == null || !products.Any())
                    {
                        throw new InvalidOperationException("Order must contain at least one product.");
                    }
                }

                if (ruleSet?["totalPriceMustBePositive"]?.Value<bool>() == true)
                {
                    var totalPrice = data["total_price"]?.Value<decimal>() ?? 0;
                    if (totalPrice <= 0)
                    {
                        throw new InvalidOperationException("Total price must be positive.");
                    }
                }

                break;
            }
            case "product":
            {
                if (ruleSet?["nameCannotBeEmpty"]?.Value<bool>() == true)
                {
                    var name = data["name"]?.Value<string>();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        throw new InvalidOperationException("Product name cannot be empty.");
                    }
                }

                if (ruleSet?["priceMustBePositive"]?.Value<bool>() == true)
                {
                    var price = data["price"]?.Value<decimal>() ?? 0;
                    if (price <= 0)
                    {
                        throw new InvalidOperationException("Product price must be positive.");
                    }
                }

                break;
            }
            default:
                throw new InvalidOperationException("valid rules not found");
        }
    }

    private JObject GetRulesForObjectType(string objectType)
    {
        return _rules.TryGetValue(objectType, out var rule) ? rule : null!;
    }
}