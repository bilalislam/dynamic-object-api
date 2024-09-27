using Newtonsoft.Json.Linq;

namespace DynamicObjectApi.Domain;

public interface IBusinessLogicService{
    Task ApplyBusinessRules(string objectType, JObject data);
}