using Common.Logic.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using BFT.AlgoService.Data.Models;
using BFT.AlgoService.Logic.Models;

namespace BFT.AlgoService.Logic.Factories
{
    public class DataFactory
    {
        public static Data.Models.Data Create(string fileName, long lifetime, string userId) => new Data.Models.Data
        {
        };

        public static Data.Models.Data Create(IFormFile file, long lifetime, string userId) => new Data.Models.Data
        {
        };

        public static DataDto CreateDto(Data.Models.Data model) => new DataDto
        {
        };


        public static Data.Models.Data Create(DataDto dto) => new Data.Models.Data
        {
        };
    }
}
