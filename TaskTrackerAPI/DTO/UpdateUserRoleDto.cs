using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO
{
    public class UpdateUserRoleDto
    {
        [Required]
        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role.")]
        public UserRole Role { get; set; }
    }
}
