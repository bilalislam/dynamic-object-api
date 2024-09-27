using Newtonsoft.Json.Linq;

namespace DynamicObjectApi.Domain;

public interface IRuleEngine{
    Task ValidateRule(JObject data, JToken? ruleSet);
}