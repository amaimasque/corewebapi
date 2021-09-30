using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWebAPI2.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CoreWebAPI2.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<NoteModel> NoteModel { get; set; }
        public DbSet<ProductModel> ProductModel { get; set; }
    }
}
