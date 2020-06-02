using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DevService.Models
{
    public class MainContext : DbContext
    {
        public MainContext(DbContextOptions<MainContext> options) : base(options)
        { }

        public DbSet<Commands> Commands { get; set; }
        public DbSet<CommandParams> CommandParams { get; set; }
        public DbSet<Users> Users { get; set; }
    }
}
