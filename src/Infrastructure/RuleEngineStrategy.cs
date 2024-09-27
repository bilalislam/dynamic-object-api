using DynamicObjectApi.Domain;
using Newtonsoft.Json.Linq;

namespace DynamicObjectApi.Infrastructure;

public class RuleEngineStrategy : IRuleEngineStrategy{
    private IRuleEngine ruleEngine;

    public void SetRuleStrategy(IRuleEngine ruleEngine){
        this.ruleEngine = ruleEngine;
    }

    public async Task ValidateRule(JObject data, JToken? ruleSet){
        if (ruleEngine == null){
            throw new InvalidOperationException("Rule engine is not set.");
        }

        await ruleEngine.ValidateRule(data, ruleSet);
    }
}