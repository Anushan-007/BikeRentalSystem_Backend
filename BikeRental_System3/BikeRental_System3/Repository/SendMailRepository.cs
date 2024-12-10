using BikeRental_System3.Data;
using BikeRental_System3.Enus;
using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Repository
{
    public class SendMailRepository(AppDbContext _Context)
    {


        public async Task<EmailTemplate> GetTemplate(EmailTypes emailTypes)
        {
            var template = _Context.EmailTemplates.Where(x => x.emailTypes == emailTypes).FirstOrDefault();
            return template;
        }

    }
}
