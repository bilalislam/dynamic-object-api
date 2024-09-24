using DynamicObjectAPI.Data;
using DynamicObjectApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace DynamicObjectApi.Services;

public class BusinessLogicService{
    private readonly ApplicationDbContext _context;

    private readonly Dictionary<string, JObject> _rules = new(){
        { "order", JObject.Parse(File.ReadAllText("Rules/order.json")) },
        { "product", JObject.Parse(File.ReadAllText("Rules/product.json")) }
    };

    public BusinessLogicService(ApplicationDbContext context){
        _context = context;
    }

    public async Task ApplyBusinessRules(string objectType, JObject data){
        var rules = GetRulesForObjectType(objectType);
        if (rules != null){
            var ruleSet = rules["rules"];
            switch (objectType){
                case "order":{
                    if (ruleSet?["mustContainAtLeastOneProduct"]?.Value<bool>() == true){
                        var products = data["products"] as JArray;
                        if (products == null || !products.Any()){
                            throw new ValidationException("Order must contain at least one product.");
                        }

                        foreach (var product in products){
                            var productId = product.Value<int>();
                            var isExists = await _context.Objects.FirstOrDefaultAsync(
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
                        var isExists = await _context.Objects.FirstOrDefaultAsync(
                            x => x.Id == customerId && x.ObjectType == "customer");

                        if (isExists == null){
                            throw new ValidationException("Customer does not exist.");
                        }
                    }


                    break;
                }
                case "product":{
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

                    break;
                }
            }
        }
    }

    private JObject GetRulesForObjectType(string objectType){
        return _rules.TryGetValue(objectType, out var rule) ? rule : null!;
    }
}