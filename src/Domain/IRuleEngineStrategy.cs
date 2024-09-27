using Newtonsoft.Json.Linq;

namespace DynamicObjectApi.Domain;

public interface IRuleEngineStrategy{
    Task ValidateRule(JObject data, JToken? ruleSet);
    void SetRuleStrategy(IRuleEngine ruleEngine);
}