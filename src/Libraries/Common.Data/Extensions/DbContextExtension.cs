using System;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Extensions
{
    public static class DbContextExtension
    {
        /// <summary>
        /// Check if data table is exist in application
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="table">Table name</param>
        public static bool IsTableExists(this DbContext context, string table)
        {
            bool result = false;
            try
            {
                using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = $"SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table}'";
                    context.Database.OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            result = reader.GetInt32(0) >= 1;
                        }
                    }
                }
            }
            catch (Exception)
            {}

            return result;
        }
    }
}
