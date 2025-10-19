using Assembly.Projecto.Final.Data.EntityFramework.Context;
using Assembly.Projecto.Final.Domain.Common;
using Assembly.Projecto.Final.Domain.Enums;
using Assembly.Projecto.Final.Domain.Models;
using Assembly.Projecto.Final.Services.Dtos.IServiceDtos.OtherModelsDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Assembly.Projecto.Final.WebAPI.Extensions
{
    public static class WebApplicationExtensions
    {
        public static void EnsureDatabaseMigration(this WebApplication app)
        {
            var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                //context.Database.EnsureDeleted();

                context.Database.Migrate();

                //SeedData(context);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao migrar ou semear o banco de dados: {ex.Message}");
            }
        }

        private static void SeedData(ApplicationDbContext context)
        {
            if (context.Employees.Any())
            {
                return;
            }

            var name = Name.Create("Ana", "Paula", "Dias");
            var agent = Agent.Create(name,DateTime.Parse("09-07-1989"),"Feminino","",true,
                DateTime.Parse("07-03-2013"),DateTime.Parse("12-05-2017"),RoleType.Admin);

            context.Employees.Add(agent);

            context.SaveChanges();

            var entityLink = EntityLink.Create(EntityType.Employee, agent.Id);
            agent.SetEntityLink(entityLink);

            byte[] passwordHash;
            byte[] passwordSalt;

            using (var hmac = new HMACSHA512())
            {
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("admin123"));
                passwordSalt = hmac.Key;
            }

            var account = Account.Create(passwordHash, passwordSalt,"admin@gmail.com");

            agent.EntityLink.SetAccount(account);

            context.Employees.Update(agent);

            context.SaveChanges();
        }
    }
}
