using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AGD.Service.DTOs.Request
{
    public class UpdateUserProfileRequest
    {
        [StringLength(250)]
        public string? Address { get; set; }

        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }

        [Phone, StringLength(30)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string? Username { get; set; }

        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        public DateOnly? DateOfBirth { get; set; }
    }
}