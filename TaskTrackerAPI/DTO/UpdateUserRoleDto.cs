using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO
{
    public class UpdateUserRoleDto
    {
        [Required]
        [RegularExpression("^(User|Admin)$", ErrorMessage = "Invalid role. Please specify either 'User' or 'Admin'.")]
        public string Role { get; set; } = string.Empty;
    }
}
