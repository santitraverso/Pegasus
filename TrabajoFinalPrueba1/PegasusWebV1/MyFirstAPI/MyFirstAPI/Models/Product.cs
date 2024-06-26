using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MyFirstAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Sku {  get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }  
        public bool IsAvailable { get; set; }

        //We make Category required by making this required but Category can still be null
        [Required]
        public int CategoryId { get; set; }
        //We have to tell the serializer that this should not be serialized or it will create an infinite loop in the JSON
        [JsonIgnore]
        public virtual Category? Category { get; set; }
    }
}
