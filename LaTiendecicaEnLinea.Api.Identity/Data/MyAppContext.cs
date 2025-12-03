using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LaTiendecicaEnLinea.Api.Identity.Data
{
    public class MyAppContext : IdentityDbContext<IdentityUser>
    {
        public MyAppContext(DbContextOptions<MyAppContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
