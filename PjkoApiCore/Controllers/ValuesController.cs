using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PjkoApiCore.AppConfig;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using System.Data;
using PjkoApiCore.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Linq;
namespace PjkoApiCore.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    //[ApiController]
    //[Route("api/Values")]
    public class ValuesController : ControllerBase
    {
        Koneksi konn = new Koneksi();
        
        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            //string[] data = {  };
            string data = null;

            try
            {
                SqlConnection conn = konn.GetConn();
                conn.Open();
                SqlCommand cmd = new SqlCommand("select @@servername namaserver", conn);

                SqlDataReader da = cmd.ExecuteReader();
                while (da.Read())
                {
                    data = (da["namaserver"]).ToString();
                }
                da.Close();
                conn.Close();
                 
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message); 
            }
            string []dtarray  =  {  data };
            return Ok(dtarray);
        }

        [HttpGet]
        [Route("lrappkd/{tahun?}/{d1?}")]
        public IActionResult GetData(string tahun,string d1)
        {
            var xDoc = XDocument.Parse((System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/StoreProcedure/test.xml")));

            // with Descendents
            //var customer = xDoc.Root.Element("Sp");
            //var optinStatus = customer.Element("query");
            //Console.WriteLine(status.Value);
            var status = xDoc.Descendants("query2").Single();

            IEnumerable<DataLRAPPKD> dataVM = new List<DataLRAPPKD>();
            try
            {
                SqlConnection conn = konn.GetConn();
                conn.Open();
                SqlCommand cmd = new SqlCommand("RptLRAPPKD", conn);
                cmd.Parameters.AddWithValue("@tahun",tahun);
                cmd.Parameters.AddWithValue("@d1", Convert.ToDateTime(d1));
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                var table = (from DataRow dr in dt.Rows

                               select new DataLRAPPKD()
                               {
                                   Realisasi = Convert.ToDecimal(dr["realisasi"]),
                                   Kdlra1 = Convert.ToInt16(dr["kd_rek_1"])
                                   
                               }).OrderBy(c => c.Kdlra1).ToList();
                //var peg = table.ToList();
                //var daftunitVm = Mapper.Map<IEnumerable<Daftunit>, IEnumerable<DaftunitViewModel>>(daftunit);
                dataVM = table.ToList();
                conn.Close();
               
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(dataVM);
        }

        [HttpGet]
        [Route("lragabungan/{tahun?}/{d1?}/{d2?}")]
        public IActionResult LraGabungan(string tahun, string d1, string d2)
        {
            //string[] data = {  };

            IEnumerable<DataLRA_Gabungan> dataVM = new List<DataLRA_Gabungan>();
            try
            {
                SqlConnection conn = konn.GetConn();
                conn.Open();
                SqlCommand cmd = new SqlCommand("RptLRA_Gabungan", conn);
                cmd.Parameters.AddWithValue("@tahun", tahun);
                cmd.Parameters.AddWithValue("@level", "5");
                cmd.Parameters.AddWithValue("@kd_urusan", "");
                cmd.Parameters.AddWithValue("@kd_bidang", "");
                cmd.Parameters.AddWithValue("@kd_unit", "");
                cmd.Parameters.AddWithValue("@kd_sub", "");
                cmd.Parameters.AddWithValue("@kd_prog", "");
                cmd.Parameters.AddWithValue("@id_prog", "");
                cmd.Parameters.AddWithValue("@kd_keg", "");
                cmd.Parameters.AddWithValue("@d1", Convert.ToDateTime(d1));
                cmd.Parameters.AddWithValue("@d2", Convert.ToDateTime(d1));
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                var table = (from DataRow dr in dt.Rows

                             select new DataLRA_Gabungan()
                             {
                                 Kd_rek = (dr["Kd_rek"]).ToString(),
                                 Nm_rek = (dr["nm_rek"]).ToString().TrimEnd(),
                                 Anggaran = Convert.ToDecimal(dr["Anggaran"]),
                                 Realisasi_Ini = Convert.ToDecimal(dr["Realisasi_Ini"]),
                                 Realisasi_Lalu = Convert.ToDecimal(dr["Realisasi_Lalu"]),
                                 Realisasi_Total = Convert.ToDecimal(dr["Realiasi_Total"]),
                                 Sisa = Convert.ToDecimal(dr["sisa"])
                                 

                             }).OrderBy(c => c.Kd_rek).ToList();
                //var peg = table.ToList();
                //var daftunitVm = Mapper.Map<IEnumerable<Daftunit>, IEnumerable<DaftunitViewModel>>(daftunit);
                dataVM = table.ToList();
                conn.Close();

            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(dataVM);
        }


        // GET api/values/5
        [HttpGet("datatahun")]
        public ActionResult  GetTahun()
        {
            var articles = new object();
            try
            {
                var file = (JObject)JsonConvert.DeserializeObject(System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/AppConfig/simdapropertis.json"));
               

                    try
                    {
                        articles = file["DataTahun"].Children();                        
                        return Ok(articles);
                    }
                    catch (Exception )
                {
                    string error = "Data Value Json Tidak Ada";
                    return StatusCode(401, error);
                    }
                
            }
            catch (Exception)
            {
                string error = "File Tidak Ada";
                //return StatusCode(401);
                return StatusCode(401,error);
            }
            
             

        }
        [HttpGet("thanggaran")]
        public ActionResult GetTahunAnggaran()
        {
            var articles = new object();
            try
            {
                var file = (JObject)JsonConvert.DeserializeObject(System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/AppConfig/simdapropertis.json"));


                try
                {
                    articles = file["TahunAnggaran"];
                    return Ok(articles);
                }
                catch (Exception)
                {
                    string error = "Data Value Json Tidak Ada";
                    return StatusCode(401, error);
                }

            }
            catch (Exception)
            {
                string error = "File Tidak Ada";
                //return StatusCode(401);
                return StatusCode(401, error);
            }



        }
        [HttpGet("dtbulan")]
        public IActionResult GetBulan()
        {
            var file = (JObject)JsonConvert.DeserializeObject(System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/AppConfig/simdapropertis.json"));
            var articles = file["DataTahun"].Children();
            return Ok(articles);
        }

        [HttpGet("xml")]
        public IActionResult GetXml()
        {

           
           var xDoc = XDocument.Parse((System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/StoreProcedure/test.xml")) );

            // with Descendents
            var sp = xDoc.Root.Element("Sp");
            var query = sp.Element("query1");
            //Console.WriteLine(status.Value);
            //var status = xDoc.Descendants("query1").Single();
            string data = null;

            try
            {
                SqlConnection conn = konn.GetConn();
                conn.Open();
                SqlCommand cmd = new SqlCommand(query.Value, conn);

                SqlDataReader da = cmd.ExecuteReader();
                while (da.Read())
                {
                    data = (da["namaserver"]).ToString();
                }
                da.Close();
                conn.Close();

            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
            string[] dtarray = { data };
            return Ok(dtarray);

            //return Ok(status.Value);
        }
        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
