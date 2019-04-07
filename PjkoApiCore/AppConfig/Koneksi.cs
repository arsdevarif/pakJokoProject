using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PjkoApiCore.AppConfig
{
    public class Koneksi
    {

        public System.Data.SqlClient.SqlConnection GetConn()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            //.AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            var connectionString = configuration.GetSection("ConnectionStrings").Value;
            SqlConnection conn = new SqlConnection(connectionString);

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("connection configuration in problem");

            return conn;

        }

        public static System.Data.SqlClient.SqlConnection Conn { get; set; }
    }
}
