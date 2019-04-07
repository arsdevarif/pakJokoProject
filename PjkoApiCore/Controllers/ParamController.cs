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
    public class ParamController : Controller
    {
        Koneksi konn = new Koneksi();
        DataParameter getdb = new DataParameter();

        //[HttpGet]
        //[Route("jnsrek/{tahun?}")]
        //public IActionResult GetRekJenis(string tahun)
        //{
            
        //    getdb.GetDb(tahun);
        //    string nmdb = getdb.Nmdbsimda.TrimEnd();
        //    IEnumerable<ParamRekJenisPen> dataVM = new List<ParamRekJenisPen>();
        //    if (!String.IsNullOrEmpty(nmdb))
        //    {
        //        try
        //        {
        //            SqlConnection conn = konn.GetConn();
        //            conn.Open();
        //            SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
        //                                            " SELECT (substring(ltrim(rTRIM(A.Kd_Rek_1) + " +
        //                                            " rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3)), 1, 3)) kd_rek ," +
        //                                            " NM_REK_3 NM_REK  FROM Ref_Rek_3 A "+ 
        //                                            " WHERE A.Kd_Rek_1 = 4 "+
        //                                            "", conn);
        //            cmd.ExecuteNonQuery();
        //            SqlDataAdapter da = new SqlDataAdapter(cmd);
        //            DataTable dt = new DataTable();
        //            da.Fill(dt);
        //            var table = (from DataRow dr in dt.Rows

        //                         select new ParamRekJenisPen()
        //                         {
        //                             //Kd_rek_all = (dr["Kd_rek_all"]).ToString(),
        //                             Kd_rek = (dr["Kd_rek"]).ToString().TrimEnd(),
        //                             Nmrek = (dr["nm_rek"]).ToString().TrimEnd(),
                                     


        //                         }).OrderBy(c => c.Kd_rek).ToList();

        //            dataVM = table.ToList();
        //            conn.Close();


        //        }
        //        catch (SqlException ex)
        //        {
        //            return BadRequest(ex.Message);
        //        }
        //    }
        //    else
        //    {
        //        return Ok("Data untuk Tahun " + tahun + " tidak ada");
        //    }
        //    return Ok(dataVM);
        //}

        [HttpGet]
        [Route("objekrek/{tahun?}")]
        public IActionResult GetRekObject(string tahun)
        {
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<ParamRekObjekPen> dataVM = new List<ParamRekObjekPen>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " SELECT (substring(ltrim(rTRIM(A.Kd_Rek_1) + " +
                                                    " rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4)), 1, 6)) kd_rek, " +
                                                    " NM_REK_4 NM_REK FROM Ref_Rek_4 A " +
                                                    " WHERE A.Kd_Rek_1 = 4 and A.KD_REK_2=1 AND A.KD_REK_3=1" +
                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new ParamRekObjekPen()
                                 {
                                     //Kd_rek_all = (dr["Kd_rek_all"]).ToString(),
                                     Kd_rek = (dr["Kd_rek"]).ToString().TrimEnd(),
                                     Nmrek = (dr["nm_rek"]).ToString().TrimEnd(),



                                 }).OrderBy(c => c.Kd_rek).ToList();

                    dataVM = table.ToList();
                    conn.Close();


                }
                catch (SqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Ok("Data untuk Tahun " + tahun + " tidak ada");
            }
            return Ok(dataVM);
        }
        [HttpGet]
        [Route("dtopd/{tahun?}")]
        public IActionResult Getdtopd(string tahun)
        {
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<ParamDataOpd> dataVM = new List<ParamDataOpd>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " SELECT (substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD, + " +                                                    
                                                    " NM_SUB_UNIT FROM Ref_SUB_UNIT A " +
                                                    " Order by Kd_urusan,kd_bidang,kd_unit,kd_sub"+                                                  
                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new ParamDataOpd()
                                 {
                                     //Kd_rek_all = (dr["Kd_rek_all"]).ToString(),
                                     Kdopd = (dr["kdopd"]).ToString().TrimEnd(),
                                     Nmsubunit = (dr["nm_sub_unit"]).ToString().TrimEnd(),



                                 }).OrderBy(c => c.Kdopd).ToList();

                    dataVM = table.ToList();
                    conn.Close();


                }
                catch (SqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Ok("Data untuk Tahun " + tahun + " tidak ada");
            }
            return Ok(dataVM);
        }

        [HttpGet]
        [Route("dthibah/{tahun?}")]
        public IActionResult Getdthibah(string tahun)
        {
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<ParamDataHibah> dataVM = new List<ParamDataHibah>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " SELECT (substring(ltrim(rTRIM(A.Kd_Rek_1) + " +
                                                    " rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4)), 1, 3)) kd_rek, " +
                                                    " NM_REK_4 NM_REK FROM Ref_Rek_4 A " +
                                                    " WHERE A.Kd_Rek_1 = 5 and KD_REK_2= 1 AND KD_REK_3= 4 " +
                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new ParamDataHibah()
                                 {
                                     //Kd_rek_all = (dr["Kd_rek_all"]).ToString(),
                                     Kd_rek = (dr["Kd_rek"]).ToString().TrimEnd(),
                                     Nmrek = (dr["nm_rek"]).ToString().TrimEnd(),



                                 }).OrderBy(c => c.Kd_rek).ToList();

                    dataVM = table.ToList();
                    conn.Close();


                }
                catch (SqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Ok("Data untuk Tahun " + tahun + " tidak ada");
            }
            return Ok(dataVM);
        }

        [HttpGet]
        [Route("dtdndesa/{tahun?}")]
        public IActionResult Getdtdndesa(string tahun)
        {
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<ParamDataHibah> dataVM = new List<ParamDataHibah>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " SELECT (substring(ltrim(rTRIM(A.Kd_Rek_1) + " +
                                                    " rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4)), 1, 3)) kd_rek, " +
                                                    " NM_REK_4 NM_REK FROM Ref_Rek_4 A " +
                                                    " WHERE A.Kd_Rek_1 = 5 and KD_REK_2= 1 AND KD_REK_3= 4 " +
                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new ParamDataHibah()
                                 {
                                     //Kd_rek_all = (dr["Kd_rek_all"]).ToString(),
                                     Kd_rek = (dr["Kd_rek"]).ToString().TrimEnd(),
                                     Nmrek = (dr["nm_rek"]).ToString().TrimEnd(),



                                 }).OrderBy(c => c.Kd_rek).ToList();

                    dataVM = table.ToList();
                    conn.Close();


                }
                catch (SqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Ok("Data untuk Tahun " + tahun + " tidak ada");
            }
            return Ok(dataVM);
        }
    }
}