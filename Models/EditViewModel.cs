namespace FileDigitilizationSystem.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }  // Needed to find the user
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
