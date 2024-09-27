using DynamicObjectApi.Domain;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace DynamicObjectApi.Infrastructure;

public class OrderEngine(ApplicationDbContext context) : IRuleEngine{
    public async Task ValidateRule(JObject data, JToken? ruleSet){
        if (ruleSet?["mustContainAtLeastOneProduct"]?.Value<bool>() == true){
            var products = data["products"] as JArray;
            if (products == null || !products.Any()){
                throw new ValidationException("Order must contain at least one product.");
            }

            foreach (var product in products){
                var productId = product.Value<int>();
                var isExists = await context.Objects.FirstOrDefaultAsync(
                    x => x.Id == productId && x.ObjectType == "product");

                if (isExists == null){
                    throw new ValidationException("Product does not exist.");
                }
            }
        }

        if (ruleSet?["totalPriceMustBePositive"]?.Value<bool>() == true){
            var totalPrice = data["total_price"]?.Value<decimal>() ?? 0;
            if (totalPrice <= 0){
                throw new ValidationException("Total price must be positive.");
            }
        }

        if (ruleSet?["customerMustBeValid"]?.Value<bool>() == true){
            var customerId = data["customer_id"]?.Value<int>() ?? 0;
            var isExists = await context.Objects.FirstOrDefaultAsync(
                x => x.Id == customerId && x.ObjectType == "customer");

            if (isExists == null){
                throw new ValidationException("Customer does not exist.");
            }
        }
    }
}