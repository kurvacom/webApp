using LegalTranslation.Data;
using LegalTranslation.Interfaces;
using LegalTranslation.Models;
using LegalTranslation.ViewModels;
using LegalTranslation.Helpers;
using Microsoft.AspNetCore.Identity;

namespace LegalTranslation.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountRepository(AppDbContext context,
            UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }



        public bool CheckEmailCategory(string email)
        {
            bool result = false;

            List<string> userEmails = _context.Users.Select(u => u.Email).ToList();

            if (userEmails.Contains(email))
            {
                var adminEmail = _userManager.GetUsersInRoleAsync("admin").Result.FirstOrDefault().Email;

                if (email == adminEmail)
                {
                    result = SendAdmin(email);
                }
                else
                {
                    result = SendWorker(email);
                }
            }
            else
            {
                result = SendUnidentified(email);
            }
            return result;
        }        

        public bool SendAdmin(string email)
        {
            AdminRepository ar = new AdminRepository(_context, _userManager, _roleManager);
            string id = ar.GetAdminData().Id;

            string password = RandomPasswordGenerator.GenerateRandomPassword();

            ar.ChangeAdminForgottenPassword(id, password);

            string body = $"Your new password is: {password}\nChange it as soon as you log in!";
            string subject = "Admin Password Reset!!!";

            EmailSender sender = new EmailSender(_context);
            sender.ForgottenPassword(body, subject);

            return true;
        }

        public bool SendUnidentified(string email)
        {
            string body = $"Unidentified user with email: {email} tried to Log in!";
            string subject = "Unidentified user tried to Log in!";

            EmailSender sender = new EmailSender(_context);
            sender.NotAdmin(body, subject);

            return true;
        }

        public bool SendWorker(string email)
        {
            string body = $"Your worker with email: {email} could not log in!\nAdmin must log in and RECREATE this user and tell them their new Email and Password!";
            string subject = "Worker Password Forgotten!";

            EmailSender sender = new EmailSender(_context);
            sender.NotAdmin(body, subject);

            return true;
        }
    }
}
