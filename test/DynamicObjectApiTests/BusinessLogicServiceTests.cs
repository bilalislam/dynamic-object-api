// using System.Linq.Expressions;
// using DynamicObjectAPI.Data;
// using DynamicObjectApi.Models;
// using DynamicObjectApi.Services;
// using Microsoft.EntityFrameworkCore;
// using Moq;
// using Newtonsoft.Json.Linq;
// using Assert = Xunit.Assert;
// using ValidationException = DynamicObjectApi.Models.ValidationException;
//
// namespace DynamicObjectApiTests;
//
// public class BusinessLogicServiceTests{
//     private readonly BusinessLogicService _businessLogicService;
//     private readonly Mock<ApplicationDbContext> _contextMock;
//
//     public BusinessLogicServiceTests(){
//         _contextMock = new Mock<ApplicationDbContext>();
//         _businessLogicService = new BusinessLogicService(_contextMock.Object);
//     }
//
//     [Fact]
//     public async Task ApplyBusinessRules_OrderWithoutProducts_ThrowsValidationException(){
//         // Arrange
//         var jsonData = JObject.Parse("{ 'products': [] }");
//         var rules = JObject.Parse("{ 'rules': { 'mustContainAtLeastOneProduct': true } }");
//         File.WriteAllText("Rules/order.json", rules.ToString());
//
//         // Act & Assert
//         var exception = await Assert.ThrowsAsync<ValidationException>(
//             () => _businessLogicService.ApplyBusinessRules("order", jsonData)
//         );
//
//         Assert.Equal("Order must contain at least one product.", exception.Message);
//     }
//
//     [Fact]
//     public async Task ApplyBusinessRules_ValidProduct_Success(){
//         // Arrange
//         var product = new DynamicObject{ Id = 1, ObjectType = "product" };
//         _contextMock.Setup(x => x.Objects.FirstOrDefaultAsync(
//                 It.IsAny<Expression<Func<DynamicObject, bool>>>(), default))
//             .ReturnsAsync(product);
//
//         var jsonData = JObject.Parse("{ 'products': [1], 'total_price': 100 }");
//         var rules = JObject.Parse(
//             "{ 'rules': { 'mustContainAtLeastOneProduct': true, 'totalPriceMustBePositive': true } }");
//         await File.WriteAllTextAsync("Rules/order.json", rules.ToString());
//
//         // Act
//         await _businessLogicService.ApplyBusinessRules("order", jsonData);
//     }
//
//     [Fact]
//     public async Task ApplyBusinessRules_InvalidTotalPrice_ThrowsValidationException(){
//         // Arrange
//         var jsonData = JObject.Parse("{ 'products': [1], 'total_price': 0 }");
//         var rules = JObject.Parse("{ 'rules': { 'totalPriceMustBePositive': true } }");
//         await File.WriteAllTextAsync("Rules/order.json", rules.ToString());
//         
//
//         // Act & Assert
//         var exception = await Assert.ThrowsAsync<ValidationException>(
//             () => _businessLogicService.ApplyBusinessRules("order", jsonData)
//         );
//
//         Assert.Equal("Total price must be positive.", exception.Message);
//     }
// }