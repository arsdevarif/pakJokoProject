using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using PjkoApiCore.Model;

namespace PjkoApiCore.AppConfig
{
    public class DataParameter
    {
        Koneksi konn = new Koneksi();
        public string Nmdbsimda { get; set; }
        public void GetDb(string Tahun)
        {
            try
            {
                //DataTahun datatahun = null;
                if (!String.IsNullOrEmpty(Tahun))
                {
                    //try
                    //{
                    //    SqlConnection conn = konn.GetConn();
                    //    conn.Open();
                    //    SqlCommand cmd = new SqlCommand(" SELECT isnull((NMDBSIMDA),NULL)NMDBSIMDA  FROM SETDBSIMDA WHERE " +
                    //                                    " TAHUN=@TAHUN  " +
                    //                                    " ", conn);
                    //    cmd.Parameters.AddWithValue("@TAHUN", Tahun);

                    //    SqlDataReader dtGet = cmd.ExecuteReader();
                    //    while (dtGet.Read())
                    //    {

                    //        datatahun = new DataTahun();
                    //        datatahun.Nmdatabase = (dtGet["NMDBSIMDA"]).ToString().TrimEnd();
                    //        Nmdbsimda = datatahun.Nmdatabase;
                    //    }

                    //    dtGet.Close();
                    //    conn.Close();
                    //    if (String.IsNullOrEmpty(Nmdbsimda)) { Nmdbsimda = " "; }
                    var file = JObject.Parse(System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/AppConfig/simdapropertis.json"));
                    
                    try
                    {
                        //articles = file["DataTahun"].Children();
                        var data = (from JObject dt in file["DataTahun"]
                                    select new DataTahun()
                                    {
                                        Id = Convert.ToInt32(dt["Id"]),
                                        Tahun = dt["Tahun"].ToString(),
                                        Nmdatabase = dt["Nmdatabase"].ToString(),

                                    }).Where(c => c.Tahun == Tahun).ToList();
                        Nmdbsimda = data.Select(c=>c.Nmdatabase).FirstOrDefault();
                        if (String.IsNullOrEmpty(Nmdbsimda)) { Nmdbsimda = " "; }
                    }
                    catch(Exception)
                    {
                        Nmdbsimda = " ";
                    }


                }
            }
            catch (Exception ex)
            {
                string error = "Data parameter tidak ada, hubungi administrator";
                Nmdbsimda = error;
            }
        }
    }
}
