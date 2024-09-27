using DynamicObjectApi.Domain;
using DynamicObjectApi.Infrastructure;
using Newtonsoft.Json.Linq;

namespace DynamicObjectApi.Application;

public class BusinessLogicService(IRuleEngineStrategy engineStrategy, ApplicationDbContext context)
    : IBusinessLogicService{
    private readonly Dictionary<string, JObject> _rules = new(){
        { "order", JObject.Parse(File.ReadAllText("Infrastructure/Rules/order.json")) },
        { "product", JObject.Parse(File.ReadAllText("Infrastructure/Rules/product.json")) }
    };

    public async Task ApplyBusinessRules(string objectType, JObject data){
        var rules = GetRulesForObjectType(objectType);
        if (rules != null){
            var ruleSet = rules["rules"];
            switch (objectType){
                case "order":
                    engineStrategy.SetRuleStrategy(new OrderEngine(context));
                    break;
                case "product":
                    engineStrategy.SetRuleStrategy(new ProductEngine());
                    break;
                default:
                    throw new InvalidOperationException("Invalid object type.");
            }

            await engineStrategy.ValidateRule(data, ruleSet);
        }
    }

    private JObject GetRulesForObjectType(string objectType){
        return _rules.TryGetValue(objectType, out var rule) ? rule : null!;
    }
}