using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using AuditTrialUsingMvcPlusIdentity.Models;

namespace AuditTrialUsingMvcPlusIdentity.Data
{
    public class ApplicationDbContext : AuditableIdentityContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<AuditTrialUsingMvcPlusIdentity.Models.Product> Product { get; set; }
    }
}
