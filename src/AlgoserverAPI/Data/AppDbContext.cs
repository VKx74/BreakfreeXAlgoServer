using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Algoserver.API.Models;


namespace Algoserver.API.Data
{
    public class AppDbContext: DbContext
    {
        public DbSet<Statistics> Statistics { get; set; }
    }
}
