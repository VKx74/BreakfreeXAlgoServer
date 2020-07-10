using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using BFT.AlgoService.Data.Models;
using BFT.AlgoService.Logic.Factories;
using BFT.AlgoService.Logic.Models;

namespace BFT.AlgoService.Logic.Services
{
    public class AlgoService
    {
        private readonly ApplicationDbContext _db;
       
        public AlgoService(ApplicationDbContext db)
        {
            _db = db;
        }

        public string SayHallo()
        {
            return "Hello";
        }
    }
}
