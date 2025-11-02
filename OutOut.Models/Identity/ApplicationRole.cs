using Microsoft.AspNetCore.Identity;

namespace OutOut.Models.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }

        public override string ToString()
        {
            return Name;
        }
    }
}
