using DynamicObjectApi.Application;
using DynamicObjectApi.Domain;
using DynamicObjectApi.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json.Linq;

namespace DynamicObjectApiTests;

public class BusinessLogicServiceTests
{
    private readonly Mock<IRuleEngineStrategy> _mockEngineStrategy;
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly IBusinessLogicService _service;

    public BusinessLogicServiceTests()
    {
        // Mock DbContext setup
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _mockContext = new Mock<ApplicationDbContext>(options);

        // Mock Rule Engine Strategy
        _mockEngineStrategy = new Mock<IRuleEngineStrategy>();

        // Create BusinessLogicService with mocks
        _service = new BusinessLogicService(_mockEngineStrategy.Object, _mockContext.Object);
    }

    [Fact]
    public async Task ApplyBusinessRules_Order_WithValidRules_ShouldInvokeOrderEngine()
    {
        // Arrange
        var data = JObject.Parse("{ 'products': [1, 2], 'total_price': 100, 'customer_id': 1 }");
        var ruleSet = JObject.Parse("{ 'rules': { 'mustContainAtLeastOneProduct': true, 'totalPriceMustBePositive': true, 'customerMustBeValid': true } }");
        _mockEngineStrategy.Setup(x => x.SetRuleStrategy(It.IsAny<OrderEngine>()));
        _mockEngineStrategy.Setup(x => x.ValidateRule(data, It.IsAny<JToken?>())).Returns(Task.CompletedTask);

        // Act
        await _service.ApplyBusinessRules("order", data);

        // Assert
        _mockEngineStrategy.Verify(x => x.SetRuleStrategy(It.IsAny<OrderEngine>()), Times.Once);
        _mockEngineStrategy.Verify(x => x.ValidateRule(It.IsAny<JObject>(), It.IsAny<JToken?>()), Times.Once);
    }

    [Fact]
    public async Task ApplyBusinessRules_Product_WithValidRules_ShouldInvokeProductEngine()
    {
        // Arrange
        var data = JObject.Parse("{ 'name': 'Sample Product', 'price': 50 }");
        var ruleSet = JObject.Parse("{ 'rules': { 'nameCannotBeEmpty': true, 'priceMustBePositive': true } }");
        _mockEngineStrategy.Setup(x => x.SetRuleStrategy(It.IsAny<ProductEngine>()));
        _mockEngineStrategy.Setup(x => x.ValidateRule(data, It.IsAny<JToken?>())).Returns(Task.CompletedTask);

        // Act
        await _service.ApplyBusinessRules("product", data);

        // Assert
        _mockEngineStrategy.Verify(x => x.SetRuleStrategy(It.IsAny<ProductEngine>()), Times.Once);
        _mockEngineStrategy.Verify(x => x.ValidateRule(It.IsAny<JObject>(), It.IsAny<JToken?>()), Times.Once);
    }

    [Fact]
    public async Task ApplyBusinessRules_InvalidObjectType_ShouldThrowException()
    {
        // Arrange
        var data = JObject.Parse("{ 'someField': 'someValue' }");

        // Act
        var act = async () => await _service.ApplyBusinessRules("invalidType", data);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid object type.");
    }
}
