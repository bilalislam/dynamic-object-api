using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DynamicObjectApi.Models;

public class DynamicObject
{
    [Key] public int Id { get; set; }

    [Required] public string ObjectType { get; set; }

    [Required] public JsonDocument Data { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}