using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RepositoryLayer.Entity
{
    [Table("Greetings")]
    public class GreetingEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Message { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [JsonIgnore]
        public UserEntity User { get; set; }
    }
}
