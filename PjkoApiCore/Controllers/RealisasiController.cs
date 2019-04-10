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
    public class RealisasiController : Controller
    {
        Koneksi konn = new Koneksi();
        DataParameter getdb = new DataParameter();

        // GET api/values
        [HttpGet]
        //[Route("lrapenjns/{tahun?}/{d1?}/{d2?}/{jnsrek?}")]
        [Route("lrapenjns/{tahun?}/{d1?}/{d2?}")]                                //realisasi
        public IActionResult GetLRAjns(string tahun,DateTime d1,DateTime d2,int? jnsrek)
        {
             
            getdb.GetDb(tahun);
            string  nmdb =getdb.Nmdbsimda.TrimEnd();
            IEnumerable<DataLRA_Gabungan> dataVM = new List<DataLRA_Gabungan>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " +nmdb+ "  DECLARE @tmpTable TABLE(kd_rek_all INT,Kd_Rek_1 tinyint, Kd_Rek_2 tinyint, Kd_Rek_3 tinyint, Kd_Rek_4 tinyint, Kd_Rek_5 tinyint, Anggaran money, Realisasi_Lalu money, Realisasi_Ini money) " +
                                                    " DECLARE @tmpHasil TABLE(KD_REK_ALL INT, Tingkat tinyint, Kd_Rek_1 tinyint, Kd_Rek_2 tinyint, Kd_Rek_3 tinyint, Kd_Rek_4 tinyint, Kd_Rek_5 tinyint, Kd_Rek varchar(20), Nm_Rek varchar(255), Anggaran money, Realisasi_Lalu money, Realisasi_Ini money) " +
                                                    " DECLARE @Kd_Perubahan tinyint, @level int SET @level = 4 " +
                                                    " SELECT @Kd_Perubahan = MAX(Kd_Perubahan) FROM DBO.Ta_RASK_Arsip_Perubahan  WHERE Tahun = '"+tahun+"' AND Kd_Perubahan IN(4, 6, 8) AND Tgl_Perda <=  '" + d2 + "'" +
                                                    " INSERT INTO @tmpTable " +
                                                    " SELECT kd_rek_all, A.Kd_Rek_1, A.Kd_Rek_2, A.Kd_Rek_3, A.Kd_Rek_4, A.Kd_Rek_5, SUM(A.Anggaran) AS Anggaran, SUM(A.Realisasi_Lalu) AS Realisasi_Lalu, SUM(A.Realisasi_Ini) AS Realisasi_Ini" +
                                                    " FROM ( SELECT(substring(ltrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3)), 1, 3))KD_REK_ALL, A.Kd_Rek_1, A.Kd_Rek_2, A.Kd_Rek_3, A.Kd_Rek_4, A.Kd_Rek_5, A.Total AS Anggaran, 0 AS Realisasi_Lalu, 0 AS Realisasi_Ini " +
                                                    " FROM   DBO.Ta_RASK_Arsip A WHERE(A.Kd_Perubahan = @Kd_Perubahan) AND(A.Tahun = '"+tahun+"')      AND A.Kd_Rek_1 = 4" +
                                                    " UNION ALL" +
                                                    " SELECT substring(ltrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3)), 1, 3) KD_REK_ALL," +
                                                    " A.Kd_Rek_1, A.Kd_Rek_2, A.Kd_Rek_3, A.Kd_Rek_4, A.Kd_Rek_5, 0 AS Anggaran," +
                                                    " CASE C.SaldoNorm WHEN 'D' THEN A.Debet - A.Kredit ELSE A.Kredit - A.Debet END AS Realisasi_Lalu, 0 AS Realisasi_Ini " +
                                                    " FROM DBO.Ta_JurnalSemua_Rinc A INNER JOIN DBO.Ta_JurnalSemua B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND A.No_Bukti = B.No_Bukti " +
                                                    " INNER JOIN          DBO.Ref_Rek_3 C ON A.Kd_Rek_1 = C.Kd_Rek_1 AND A.Kd_Rek_2 = C.Kd_Rek_2 AND A.Kd_Rek_3 = C.Kd_Rek_3" +
                                                    " WHERE A.Kd_Rek_1 = 4 AND(B.Tahun = '"+tahun+"') " +
                                                    " AND(B.Tgl_Bukti <  '" + d1 + "')" +
                                                    " AND((CASE A.Kd_SKPD WHEN 1 THEN B.Posting ELSE B.Posting_SKPKD END) = 1) AND(A.Kd_Jurnal <> 8)" +
                                                    " UNION ALL " +
                                                    " SELECT substring(ltrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3)), 1, 3), " +
                                                    " A.Kd_Rek_1, A.Kd_Rek_2, A.Kd_Rek_3, A.Kd_Rek_4, A.Kd_Rek_5, 0 AS Anggaran, 0 AS Realisasi_Lalu," +
                                                    " CASE C.SaldoNorm WHEN 'D' THEN A.Debet - A.Kredit " +
                                                    " ELSE A.Kredit - A.Debet END AS Realisasi_Ini FROM DBO.Ta_JurnalSemua_Rinc A INNER JOIN" +
                                                    " DBO.Ta_JurnalSemua B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND A.No_Bukti = B.No_Bukti " +
                                                    " INNER JOIN DBO.Ref_Rek_3 C ON A.Kd_Rek_1 = C.Kd_Rek_1 AND A.Kd_Rek_2 = C.Kd_Rek_2 AND A.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE A.Kd_Rek_1 = 4 AND(B.Tahun = '"+tahun+"') " +
                                                    " AND(B.Tgl_Bukti BETWEEN '" + d1 + "'    AND   '" + d2 + "')" +
                                                    " AND((CASE A.Kd_SKPD WHEN 1 THEN B.Posting  ELSE B.Posting_SKPKD  END) = 1) AND (A.Kd_Jurnal <> 8)) A" +
                                                    " GROUP BY KD_REK_ALL, A.Kd_Rek_1, A.Kd_Rek_2, A.Kd_Rek_3, A.Kd_Rek_4, A.Kd_Rek_5 " +

                                                    " SELECT substring(ltrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3)+ rTRIM(A.Kd_rek_4)), 1, 6) kd_rek_all ," +
                                                    " CONVERT(varchar, A.Kd_Rek_1) +'.' + CONVERT(varchar, A.Kd_Rek_2) + '.' +" +
                                                    " CONVERT(varchar, A.Kd_Rek_3) + '.' + RIGHT('0' + CONVERT(varchar, A.Kd_Rek_4), 2) AS Kd_Rek," +
                                                    " B.Nm_Rek_4 AS Nm_Rek, A.Anggaran, A.Realisasi_Lalu, A.Realisasi_Ini,A.Realisasi_Total" +
                                                    " FROM(SELECT A.Kd_Rek_1, A.Kd_Rek_2, A.Kd_Rek_3, A.Kd_Rek_4, SUM(A.Anggaran) AS Anggaran, SUM(A.Realisasi_Lalu) AS Realisasi_Lalu, SUM(A.Realisasi_Ini) AS Realisasi_Ini,(sum(A.Realisasi_Lalu) + sum(A.Realisasi_Ini)) AS Realisasi_Total" +
                                                    " FROM @tmpTable A    " +
                                                    //" where kd_rek_all=" + jnsrek + "" +
                                                    " where A.Kd_Rek_1=4 and A.Kd_Rek_2=1 and A.Kd_Rek_3=1" +
                                                    " GROUP BY A.Kd_Rek_1, A.Kd_Rek_2, A.Kd_Rek_3, A.Kd_Rek_4 ) A INNER JOIN" +
                                                    " DBO.Ref_Rek_4 B ON A.Kd_Rek_1 = B.Kd_Rek_1 AND A.Kd_Rek_2 = B.Kd_Rek_2 AND A.Kd_Rek_3 = B.Kd_Rek_3 AND A.Kd_Rek_4 = B.Kd_Rek_4" +

                                                     "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new DataLRA_Gabungan()
                                 {
                                     Kd_rek_all = (dr["Kd_rek_all"]).ToString(),
                                     Kd_rek = (dr["Kd_rek"]).ToString(),
                                     Nm_rek = (dr["nm_rek"]).ToString().TrimEnd(),
                                     Anggaran = Convert.ToDecimal(dr["Anggaran"]),
                                     Realisasi_Ini = Convert.ToDecimal(dr["Realisasi_Ini"]),
                                     Realisasi_Lalu = Convert.ToDecimal(dr["Realisasi_Lalu"]),
                                     Realisasi_Total = Convert.ToDecimal(dr["Realisasi_Total"]),
                                     //Sisa = Convert.ToDecimal(dr["sisa"])
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
                return Ok("Data untuk Tahun "+tahun+" tidak ada");
            }
            return Ok(dataVM);
        }

        [HttpGet]
        [Route("lrapenobjbln/{tahun?}/{d1?}/{d2?}/{objrek?}")]
        public IActionResult GetLRAobj(string tahun, DateTime d1, DateTime d2, int? objrek)
        {
           
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<LraPenObj> dataVM = new List<LraPenObj>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE "+nmdb+"" +
                                                    " DECLARE @tmpTable TABLE(KDREKALL INT,BULAN INT,    Realisasi money)"+
                                                    " DECLARE @tmpHasil TABLE(KDREKALL INT,Bulan INT, Anggaran money, Realisasi money)" +
                                                    " DECLARE @tmpAngkas TABLE(KDREKALL int,BULAN INT,ANGGARAN MONEY) "+
                                                    " DECLARE @Kd_Perubahan tinyint, @TAHUN char (4),@TG1 datetime ,@TG2 datetime,@KDREKALL INT" +
                                                    " select @TAHUN='" + tahun + "'  , @TG1= '" + d1 + "'   , @TG2='" + d2 + "'  ,@KDREKALL='" + objrek + "' " +
                                                    " SELECT @Kd_Perubahan = MAX(Kd_Perubahan)FROM DBO.Ta_RASK_Arsip_Perubahan WHERE Tahun = @TAHUN AND Kd_Perubahan IN (4, 6, 8) AND Tgl_Perda <= @TG2"+
                                                    " INSERT INTO @tmpTable SELECT KDREKALL,BULAN, SUM(A.Realisasi) AS Realisasi" +
                                                    " FROM( " +
                                                    " SELECT (substring (ltrim(rTRIM(A.Kd_Rek_1)+rTRIM(A.kd_rek_2)+ rTRIM(A.Kd_rek_3)+ rTRIM(A.Kd_rek_4)),0,6)) KDREKALL,MONTH(Tgl_Bukti) BULAN,	CASE C.SaldoNorm WHEN 'D' THEN A.Debet - A.Kredit ELSE A.Kredit - A.Debet END AS Realisasi " +
                                                    " FROM DBO.Ta_JurnalSemua_Rinc A INNER JOIN DBO.Ta_JurnalSemua B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON A.Kd_Rek_1 = C.Kd_Rek_1 AND A.Kd_Rek_2 = C.Kd_Rek_2 AND A.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE A.Kd_Rek_1 = 4 AND A.KD_REK_2=1 AND A.KD_REK_3=1 AND (B.Tahun = @TAHUN )AND (B.Tgl_Bukti BETWEEN @TG1 AND  @TG2 ) AND ((CASE A.Kd_SKPD WHEN 1 THEN B.Posting ELSE B.Posting_SKPKD END) = 1)AND (A.Kd_Jurnal <> 8)) A GROUP BY KDREKALL,BULAN " +
                                                    " INSERT INTO @tmpAngkas SELECT A.KDREKALL , A.BULAN,A.ANGGARAN"+
                                                    " FROM ( " +
                                                    //" SELECT   RTRIM(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4)) KDREKALL, 1 AS BULAN, SUM(TOTAL) AS ANGGARAN FROM DBO.Ta_RASK_Arsip A WHERE TAHUN=@TAHUN AND  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " SELECT   RTRIM(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4)) KDREKALL, 1 AS BULAN, SUM(JAN) AS ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 2 AS BULAN, SUM(FEB)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 3 AS BULAN, SUM(MAR)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 4 AS BULAN, SUM(APR)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 5 AS BULAN, SUM(MEI)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 6 AS BULAN, SUM(JUN)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 7 AS BULAN, SUM(JUL)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 8 AS BULAN, SUM(AGT)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 9 AS BULAN, SUM(SEP)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 10 AS BULAN, SUM(OKT)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 11 AS BULAN, SUM(NOP)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " UNION ALL SELECT   Rtrim(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4))KDREKALL, 12 AS BULAN, SUM(DES)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 AND KD_REK_2=1 AND KD_REK_3=1 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " ) A GROUP BY A.KDREKALL, A.BULAN, A.ANGGARAN" +
                                                    //" insert into @tmpHasil " +
                                                    //" SELECT KDREKALL, 1 , ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 2 , ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 3 , ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 4 , ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 5 , ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 6 , ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 7 , ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 8 , ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 9 , ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 10, ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 11, ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" UNION ALL SELECT KDREKALL, 12, ANGGARAN, 0 FROM @tmpAngkas A" +
                                                    //" SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 1" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 2 " +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 3" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 4" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 5" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 6" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 7" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 8" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 9" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 10" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 11" +
                                                    //" UNION ALL SELECT KDREKALL, A.bulan, ANGGARAN, 0 FROM @tmpAngkas  A WHERE    A.BULAN = 12" +
                                                    //" UNION ALL SELECT KDREKALL, 1, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=1),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI) = 1 " +
                                                    //" UNION ALL SELECT KDREKALL, 2, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=2),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 2"+
                                                    //" UNION ALL SELECT KDREKALL, 3, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=3),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 3 "+
                                                    //" UNION ALL SELECT KDREKALL, 4, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=4),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 4"+
                                                    //" UNION ALL SELECT KDREKALL, 5, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=5),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 5 " +
                                                    //" UNION ALL SELECT KDREKALL, 6, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=6),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 6"+
                                                    //" UNION ALL SELECT KDREKALL, 7, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=7),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 7 "+
                                                    //" UNION ALL SELECT KDREKALL, 8, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=8),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 8"+
                                                    //" UNION ALL SELECT KDREKALL, 9, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=9),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 9 "+
                                                    //" UNION ALL SELECT KDREKALL, 10, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=10),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 10"+
                                                    //" UNION ALL SELECT KDREKALL, 11, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=11),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 11 "+
                                                    //" UNION ALL SELECT KDREKALL, 12, 0,(case 0 when isnull((select sum(Realisasi) from @tmpTable where MONTH(TGL_BUKTI)=12),0)then 0 * Realisasi else  Realisasi end )as realisasi FROM @tmpTable  A WHERE MONTH(TGL_BUKTI)<= 12"+
                                                    //" SELECT KDREKALL, BULAN,CASE   SUM(ANGGARAN)ANGGARAN, SUM(REALISASI)REALISASI FROM @tmpHasil"+
                                                    " SELECT A.KDREKALL, A.BULAN,   SUM(B.ANGGARAN)ANGGARAN, SUM(REALISASI)REALISASI FROM @TMPTABLE A INNER JOIN @TMPANGKAS B ON A.BULAN=B.BULAN" +
                                                    " WHERE A.KDREKALL = @KDREKALL"+
                                                    " GROUP BY A.KDREKALL, A.Bulan " +                                                   
                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new LraPenObj()
                                 {
                                     //Kd_rek_all = (dr["Kd_rek_all"]).ToString(),
                                     Bulan = Convert.ToInt32(dr["Bulan"]), 
                                     Anggaran = Convert.ToDecimal(dr["Anggaran"]),
                                     Realisasi = Convert.ToDecimal(dr["Realisasi"])
                                   


                                 }).OrderBy(c => c.Bulan).ToList();

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
        [Route("lragroup/{tahun?}/{d1?}/{d2?}")]
        public IActionResult GetLraGroup(string tahun, DateTime d1, DateTime d2, int? kdopd)
        {

            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<LraBelanjaGroup> dataVM = new List<LraBelanjaGroup>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " DECLARE @tmpTable TABLE(Kd_Urusan TINYINT,Kd_Bidang TINYINT,KD_UNIT TINYINT,Kd_Sub TINYINT,KDOPD INT,TGL_BUKTI DATETIME,ANGGARANBL MONEY,ANGGARANBTL MONEY,    RealisasiBL money, Realisasibtl money)" +
                                                    " DECLARE @Kd_Perubahan tinyint, @TAHUN char (4),@TG1 datetime ,@TG2 datetime,@KDOPD INT " +
                                                    " select @TAHUN='" + tahun + "'  , @TG1= '" + d1 + "'   , @TG2='" + d2 + "'  ,@KDOPD='" + kdopd + "' " +
                                                    " SELECT @Kd_Perubahan = MAX(Kd_Perubahan)FROM DBO.Ta_RASK_Arsip_Perubahan WHERE Tahun = @TAHUN AND Kd_Perubahan IN (4, 6, 8) AND Tgl_Perda <= @TG2" +
                                                    " INSERT INTO @tmpTable SELECT  A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub, KDOPD,TGL_BUKTI,SUM(A.ANGGARANBL),SUM(A.ANGGARANBTL)  ,SUM(A.Realisasibl) AS Realisasibl,SUM(A.Realisasibtl) AS Realisasibtl " +
                                                    " FROM( SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD," +
                                                    " '' TGL_BUKTI, A.Total AS AnggaranBL,0 AS AnggaranBTL,  0 AS Realisasibl,0 as realisasibtl FROM Ta_RASK_Arsip A WHERE (A.Kd_Perubahan = @Kd_Perubahan) AND (A.Tahun = @Tahun) AND Kd_Rek_1=5 AND Kd_Rek_2=2" +
                                                    " UNION ALL  SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD," +
                                                    " '' TGL_BUKTI, 0 AS AnggaranBL,A.Total AS AnggaranBTL,  0 AS Realisasibl,0 as realisasibtl FROM Ta_RASK_Arsip A WHERE (A.Kd_Perubahan = @Kd_Perubahan) AND (A.Tahun = @Tahun) AND Kd_Rek_1=5 AND Kd_Rek_2=1 " +
                                                    " UNION ALL SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD," +
                                                    " Tgl_Bukti,	0 AS ANGGARANBL,0 AS ANGGARANBTL,  CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS Realisasibl, 0 realisasibtl " +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc  B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND " +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE B.KD_REK_1 = 5 and b.Kd_Rek_2 = 2 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2) " +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8)" +
                                                    " UNION ALL SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD," +
                                                    " Tgl_Bukti,	0 AS ANGGARANBL,0 AS ANGGARANBTL, 0 realisasibl,  CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS Realisasibtl" +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND" +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE B.KD_REK_1 = 5 and b.Kd_Rek_2 = 1 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2)" +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8)" +
                                                    " ) A GROUP BY KDOPD, Tgl_Bukti, A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub" +
                                                    " SELECT X.KDGROUP,NMGROUP, " +
                                                    " SUM(ANGGARANBL) AS ANGGARANBL,SUM(ANGGARANBTL) AS ANGGARANBTL, SUM(REALISASIBL)REALISASIBL,SUM(REALISASIBTL)REALISASIBTL " +
                                                    " FROM SIMDABRIDGE.DBO.MAPGROUPOPD X INNER JOIN SIMDABRIDGE.DBO.GROUPOPD A ON X.KDGROUP=A.KDGROUP " +
                                                    " INNER JOIN @tmpTable B ON X.KdBidang = B.Kd_Bidang AND X.KdUrusan = B.Kd_Urusan AND X.KdUnit = B.KD_UNIT AND X.KdSub = B.Kd_Sub " +

                                                    //" where ISNULL ((@kdopd),'')='' or KDOPD=@KDOPD" +
                                                    " GROUP BY X.KDGROUP,NMGROUP " +
                                                    " ORDER BY X.KDGROUP,NMGROUP" +

                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new LraBelanjaGroup()
                                 {
                                     Kdgroup = Convert.ToInt32(dr["Kdlevel"]),
                                     Nmgroup = (dr["Nmjnslevel"]).ToString().TrimEnd(),
                                     AnggaranbL = Convert.ToDecimal(dr["AnggaranbL"]),
                                     Anggaranbtl = Convert.ToDecimal(dr["Anggaranbtl"]),
                                     Realisasibl = Convert.ToDecimal(dr["Realisasibl"]),
                                     Realisasibtl = Convert.ToDecimal(dr["Realisasibtl"])



                                 })//.OrderBy(c => c.Kdurusan & c.Kdbidang  & c.Kdunit & c.Kdsub)
                                 .ToList();

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
        [Route("lragroupopd/{tahun?}/{d1?}/{d2?}/{kdgroup?}")]
        public IActionResult GetLraGroupOpd(string tahun, DateTime d1, DateTime d2, int? kdgroup)
        {

            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<LraBelanjaGroupOpd> dataVM = new List<LraBelanjaGroupOpd>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " DECLARE @tmpTable TABLE(Kd_Urusan TINYINT,Kd_Bidang TINYINT,KD_UNIT TINYINT,Kd_Sub TINYINT,KDOPD INT,TGL_BUKTI DATETIME,ANGGARANBL MONEY,ANGGARANBTL MONEY,    RealisasiBL money, Realisasibtl money)" +
                                                    " DECLARE @Kd_Perubahan tinyint, @TAHUN char (4),@TG1 datetime ,@TG2 datetime,@KDGROUP INT " +
                                                    " select @TAHUN='" + tahun + "'  , @TG1= '" + d1 + "'   , @TG2='" + d2 + "'  ,@KDGROUP='" + kdgroup + "' " +
                                                    " SELECT @Kd_Perubahan = MAX(Kd_Perubahan)FROM DBO.Ta_RASK_Arsip_Perubahan WHERE Tahun = @TAHUN AND Kd_Perubahan IN (4, 6, 8) AND Tgl_Perda <= @TG2" +
                                                    " INSERT INTO @tmpTable SELECT  A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub, KDOPD,TGL_BUKTI,SUM(A.ANGGARANBL),SUM(A.ANGGARANBTL)  ,SUM(A.Realisasibl) AS Realisasibl,SUM(A.Realisasibtl) AS Realisasibtl " +
                                                    " FROM( SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD," +
                                                    " '' TGL_BUKTI, A.Total AS AnggaranBL,0 AS AnggaranBTL,  0 AS Realisasibl,0 as realisasibtl FROM Ta_RASK_Arsip A WHERE (A.Kd_Perubahan = @Kd_Perubahan) AND (A.Tahun = @Tahun) AND Kd_Rek_1=5 AND Kd_Rek_2=2" +
                                                    " UNION ALL  SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD," +
                                                    " '' TGL_BUKTI, 0 AS AnggaranBL,A.Total AS AnggaranBTL,  0 AS Realisasibl,0 as realisasibtl FROM Ta_RASK_Arsip A WHERE (A.Kd_Perubahan = @Kd_Perubahan) AND (A.Tahun = @Tahun) AND Kd_Rek_1=5 AND Kd_Rek_2=1 " +
                                                    " UNION ALL SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD," +
                                                    " Tgl_Bukti,	0 AS ANGGARANBL,0 AS ANGGARANBTL,  CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS Realisasibl, 0 realisasibtl " +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc  B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND " +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE B.KD_REK_1 = 5 and b.Kd_Rek_2 = 2 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2) " +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8)" +
                                                    " UNION ALL SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD," +
                                                    " Tgl_Bukti,	0 AS ANGGARANBL,0 AS ANGGARANBTL, 0 realisasibl,  CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS Realisasibtl" +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND" +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE B.KD_REK_1 = 5 and b.Kd_Rek_2 = 1 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2)" +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8)" +
                                                    " ) A GROUP BY KDOPD, Tgl_Bukti, A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub" +
                                                    " SELECT KDGROUP, KDOPD,NM_SUB_UNIT, " +
                                                    " SUM(ANGGARANBL) AS ANGGARANBL,SUM(ANGGARANBTL) AS ANGGARANBTL, SUM(REALISASIBL)REALISASIBL,SUM(REALISASIBTL)REALISASIBTL " +
                                                    " FROM SIMDABRIDGE.DBO.MAPGROUPOPD A inner JOIN @tmpTable B ON A.KdBidang = B.Kd_Bidang AND A.KdUrusan = B.Kd_Urusan AND A.KdUnit = B.KD_UNIT AND A.KdSub = B.Kd_Sub " +
                                                    " INNER JOIN REF_SUB_UNIT C   ON A.KdBidang = C.Kd_Bidang AND A.KdUrusan = C.Kd_Urusan AND A.KdUnit = C.KD_UNIT AND A.KdSub = C.Kd_Sub " +
                                                    " where   KDGROUP=@KDGROUP" +
                                                    " GROUP BY  KDGROUP,KDOPD,NM_SUB_UNIT " +
                                                    " ORDER BY KDGROUP,KDOPD" +

                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new LraBelanjaGroupOpd()
                                 {
                                     //Kd_rek_all = (dr["Kd_rek_all"]).ToString(),
                                     Kdgroup = Convert.ToInt32(dr["Kdgroup"]),
                                     Kdopd = Convert.ToInt32(dr["Kdopd"]),                                     
                                     Nmunit = (dr["NM_SUB_UNIT"]).ToString().TrimEnd(),
                                     AnggaranbL = Convert.ToDecimal(dr["AnggaranbL"]),
                                     Anggaranbtl = Convert.ToDecimal(dr["Anggaranbtl"]),
                                     Realisasibl = Convert.ToDecimal(dr["Realisasibl"]),
                                     Realisasibtl = Convert.ToDecimal(dr["Realisasibtl"])



                                 })//.OrderBy(c => c.Kdurusan & c.Kdbidang  & c.Kdunit & c.Kdsub)
                                 .ToList();

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
        [Route("lrablj/{tahun?}/{d1?}/{d2?}/{kdopd?}")]
        public IActionResult GetLrablj(string tahun, DateTime d1, DateTime d2, int? kdopd)
        {
            
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<LraBelanja> dataVM = new List<LraBelanja>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " DECLARE @tmpTable TABLE(Kd_Urusan TINYINT,Kd_Bidang TINYINT,KD_UNIT TINYINT,Kd_Sub TINYINT,KDOPD INT,TGL_BUKTI DATETIME,ANGGARANBL MONEY,ANGGARANBTL MONEY,    RealisasiBL money, Realisasibtl money)" +
                                                    " DECLARE @Kd_Perubahan tinyint, @TAHUN char (4),@TG1 datetime ,@TG2 datetime,@KDOPD INT " +
                                                    " select @TAHUN='" + tahun + "'  , @TG1= '" + d1 + "'   , @TG2='" + d2 + "'  ,@KDOPD='" + kdopd + "' " +
                                                    " SELECT @Kd_Perubahan = MAX(Kd_Perubahan)FROM DBO.Ta_RASK_Arsip_Perubahan WHERE Tahun = @TAHUN AND Kd_Perubahan IN (4, 6, 8) AND Tgl_Perda <= @TG2" +                                                   
                                                    " INSERT INTO @tmpTable SELECT  A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub, KDOPD,TGL_BUKTI,SUM(A.ANGGARANBL),SUM(A.ANGGARANBTL)  ,SUM(A.Realisasibl) AS Realisasibl,SUM(A.Realisasibtl) AS Realisasibtl " +
                                                    " FROM( SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD,"+
                                                    " '' TGL_BUKTI, A.Total AS AnggaranBL,0 AS AnggaranBTL,  0 AS Realisasibl,0 as realisasibtl FROM Ta_RASK_Arsip A WHERE (A.Kd_Perubahan = @Kd_Perubahan) AND (A.Tahun = @Tahun) AND Kd_Rek_1=5 AND Kd_Rek_2=2" +
                                                    " UNION ALL  SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD," +
                                                    " '' TGL_BUKTI, 0 AS AnggaranBL,A.Total AS AnggaranBTL,  0 AS Realisasibl,0 as realisasibtl FROM Ta_RASK_Arsip A WHERE (A.Kd_Perubahan = @Kd_Perubahan) AND (A.Tahun = @Tahun) AND Kd_Rek_1=5 AND Kd_Rek_2=1 " +
                                                    " UNION ALL SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring (ltrim(rTRIM(A.KD_URUSAN)+rTRIM(A.KD_BIDANG)+ rTRIM(A.KD_UNIT)+ rTRIM(A.Kd_Sub)),0,6)) KDOPD," +
                                                    " Tgl_Bukti,	0 AS ANGGARANBL,0 AS ANGGARANBTL,  CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS Realisasibl, 0 realisasibtl " +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc  B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND " +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE B.KD_REK_1 = 5 and b.Kd_Rek_2 = 2 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2) " +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8)" +
                                                    " UNION ALL SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,(substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD," +
                                                    " Tgl_Bukti,	0 AS ANGGARANBL,0 AS ANGGARANBTL, 0 realisasibl,  CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS Realisasibtl" +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND" +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE B.KD_REK_1 = 5 and b.Kd_Rek_2 = 1 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2)" +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8)" +
                                                    " ) A GROUP BY KDOPD, Tgl_Bukti, A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub" +
                                                    " SELECT A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,KDOPD,NM_SUB_UNIT, " +
                                                    " SUM(ANGGARANBL) AS ANGGARANBL,SUM(ANGGARANBTL) AS ANGGARANBTL, SUM(REALISASIBL)REALISASIBL,SUM(REALISASIBTL)REALISASIBTL " +
                                                    " FROM Ref_Sub_Unit A inner JOIN @tmpTable B ON A.Kd_Bidang = B.Kd_Bidang AND A.Kd_Urusan = B.Kd_Urusan AND A.Kd_Unit = B.KD_UNIT AND A.Kd_Sub = B.Kd_Sub " +
                                                    " where ISNULL ((@kdopd),'')='' or KDOPD=@KDOPD" +
                                                    " GROUP BY A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub,KDOPD,NM_SUB_UNIT " +
                                                    " ORDER BY KDOPD,A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub" +
 
                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new LraBelanja()
                                 {
                                     //Kd_rek_all = (dr["Kd_rek_all"]).ToString(),
                                     Kdurusan = Convert.ToInt32(dr["KD_URUSAN"]),
                                     Kdbidang = Convert.ToInt32(dr["KD_BIDANG"]),
                                     Kdunit = Convert.ToInt32(dr["KD_UNIT"]),
                                     Kdsub = Convert.ToInt32(dr["KD_SUB"]),
                                     Kdop = Convert.ToInt32(dr["KDOPD"]),
                                     Nmunit =  (dr["NM_SUB_UNIT"]).ToString().TrimEnd(),
                                     AnggaranbL = Convert.ToDecimal(dr["AnggaranbL"]),
                                     Anggaranbtl = Convert.ToDecimal(dr["Anggaranbtl"]),
                                     Realisasibl = Convert.ToDecimal(dr["Realisasibl"]),
                                     Realisasibtl = Convert.ToDecimal(dr["Realisasibtl"])



                                 })//.OrderBy(c => c.Kdurusan & c.Kdbidang  & c.Kdunit & c.Kdsub)
                                 .ToList();

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
        [Route("lrabljopdbulan/{tahun?}/{d1?}/{d2?}/{kdopd?}")]
        public IActionResult GetLrabljopd(string tahun, DateTime d1, DateTime d2, int? kdopd)
        {
         
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<LraBelanjaOpdBulan> dataVM = new List<LraBelanjaOpdBulan>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " DECLARE @tmpReal TABLE(Kd_Urusan TINYINT,Kd_Bidang TINYINT,KD_UNIT TINYINT,Kd_Sub TINYINT,KDOPD INT,KDBULAN INT,RealisasiBL money, Realisasibtl money) " +
                                                    " DECLARE @tmpAngkas TABLE(KDOPD INT, KDBULAN INT, ANGGARAN MONEY)" +
                                                    " DECLARE @Kd_Perubahan tinyint, @TAHUN char(4), @TG1 datetime, @TG2 datetime, @KDOPD INT  " +
                                                    " select @TAHUN='" + tahun + "'  , @TG1= '" + d1 + "'   , @TG2='" + d2 + "'  ,@KDOPD='" +kdopd + "' " +
                                                    " SELECT @Kd_Perubahan = MAX(Kd_Perubahan)FROM DBO.Ta_RASK_Arsip_Perubahan WHERE Tahun = @TAHUN AND Kd_Perubahan IN(4, 6, 8) AND Tgl_Perda <= @TG2" +
                                                    " INSERT INTO @tmpReal SELECT  A.Kd_Urusan, A.Kd_Bidang, A.KD_UNIT, A.Kd_Sub, KDOPD, KDBULAN,  SUM(A.Realisasibl) AS Realisasibl, SUM(A.Realisasibtl) AS Realisasibtl" +
                                                    " FROM( "+
                                                   
                                                    " SELECT A.Kd_Urusan, A.Kd_Bidang, A.KD_UNIT, A.Kd_Sub, (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD," +
                                                    " MONTH(Tgl_Bukti)KDBULAN, CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS Realisasibl, 0 realisasibtl" +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc  B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND" +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3" +
                                                    " WHERE B.KD_REK_1 = 5 and b.Kd_Rek_2 = 2 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2)" +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8)" +
                                                    " UNION ALL SELECT A.Kd_Urusan, A.Kd_Bidang, A.KD_UNIT, A.Kd_Sub, (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD," +
                                                    " MONTH(Tgl_Bukti)KDBULAN,  0 realisasibl, CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS Realisasibtl" +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc  B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND" +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3" +
                                                    " WHERE B.KD_REK_1 = 5 and b.Kd_Rek_2 = 1 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2)" +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8))" +                                                    
                                                    " A GROUP BY KDOPD, KDBULAN, A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub" +

                                                    //" INSERT INTO @tmpReal SELECT KDOPD, KDBULAN, SUM(ANGGARANBL)ANGGARANBL,SUM(ANGGARANBTL)ANGGARANBTL" +
                                                    //" FROM(" +
                                                    //" SELECT KDOPD, 1 KDBULAN, ANGGARANBL, ANGGARANBTL," +
                                                    // " (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=1),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=1),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 1 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 2 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=2),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=2),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 2 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 3 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=3),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=3),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 3 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 4 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=4),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=4),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 4 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 5 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=5),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=5),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 5 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 6 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=6),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=6),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 6 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 7 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=7),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=7),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 7 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 8 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=8),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=8),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 8 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 9 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=9),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=9),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 9 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 10 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=10),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=10),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 10 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 11 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=11),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=11),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 11 AND KDOPD = @KDOPD" +
                                                    //" UNION ALL SELECT KDOPD, 12 KDBULAN, ANGGARANBL, ANGGARANBTL, " +
                                                    //" (case 0 when isnull((select sum (REALISASIBL) FROM @tmpTable where month(tgl_bukti)=12),0) then 0 * REALISASIBL ELSE REALISASIBL  END) REALISASIBL," +
                                                    //" (case 0 when isnull((select sum (REALISASIBTL) FROM @tmpTable where month(tgl_bukti)=12),0) then 0 * REALISASIBTL ELSE REALISASIBTL  END) REALISASIBTL" +
                                                    //" FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 12 AND KDOPD = @KDOPD" +
                                                    " INSERT INTO @tmpAngkas SELECT A.KDOPD , A.KDBULAN,A.ANGGARAN" +
                                                    " FROM ( " +
                                                    //" SELECT   RTRIM(rTRIM(A.Kd_Rek_1) + rTRIM(A.kd_rek_2) + rTRIM(A.Kd_rek_3) + rTRIM(A.Kd_rek_4)) KDREKALL, 1 AS BULAN, SUM(TOTAL) AS ANGGARAN FROM DBO.Ta_RASK_Arsip A WHERE TAHUN=@TAHUN AND  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 GROUP BY  A.KD_REK_1, A.KD_REK_2, A.KD_REK_3, A.KD_REK_4" +
                                                    " SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 1 AS KDBULAN, SUM(JAN) AS ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 5 GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 2 AS KDBULAN, SUM(FEB)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4   GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 3 AS KDBULAN, SUM(MAR)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4  GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 4 AS KDBULAN, SUM(APR)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4  GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 5 AS KDBULAN, SUM(MEI)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4  GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 6 AS KDBULAN, SUM(JUN)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4  GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 7 AS KDBULAN, SUM(JUL)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4  GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 8 AS KDBULAN, SUM(AGT)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4  GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 9 AS KDBULAN, SUM(SEP)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4  GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 10 AS KDBULAN, SUM(OKT)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 11 AS KDBULAN, SUM(NOP)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " UNION ALL SELECT   (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, 12 AS KDBULAN, SUM(DES)ANGGARAN FROM DBO.Ta_Rencana_Arsip A WHERE  KD_PERUBAHAN = @KD_PERUBAHAN AND Kd_Rek_1 = 4 GROUP BY  A.KD_URUSAN, A.KD_BIDANG, A.KD_UNIT, A.KD_SUB" +
                                                    " ) A GROUP BY A.KDOPD, A.KDBULAN, A.ANGGARAN" +
                                                    " SELECT A.KDOPD,A.KDBULAN,ISNULL((B.ANGGARAN), 0)ANGGARAN, ISNULL(SUM(REALISASIBL), 0)REALISASIBL,ISNULL(SUM(REALISASIBTL), 0)REALISASIBTL" +
                                                    " FROM  @tmpReal A LEFT JOIN @tmpAngkas B ON A.KDBULAN = B.KDBULAN and A.KDOPD=B.KDOPD WHERE A.KDOPD=@KDOPD" +
                                                    " GROUP BY A.KDBULAN,A.KDOPD,ANGGARAN" +
                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new LraBelanjaOpdBulan()
                                 {
                                     Kdopd = Convert.ToInt32(dr["KDOPD"]),
                                     Kdbulan = Convert.ToInt32(dr["kdbulan"]),                                
                                     Anggaran = Convert.ToDecimal(dr["Anggaran"]),                                    
                                     Realisasibl = Convert.ToDecimal(dr["Realisasibl"]),
                                     Realisasibtl = Convert.ToDecimal(dr["Realisasibtl"])



                                 }).OrderBy(c => c.Kdbulan).ToList();

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
        [Route("lrabljdetail/{tahun?}/{d1?}/{d2?}/{kdopd?}")]
        public IActionResult GetLraProkegOpd(string tahun, DateTime d1, DateTime d2, int? kdopd)
        {
            
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<LraDetailBelanjaOpd> dataVM = new List<LraDetailBelanjaOpd>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " DECLARE @tmpTable TABLE(Kd_prog int,Id_prog int,Kd_keg int,kd_rek_1 int,kd_rek_2 int,kd_rek_3 int, kd_rek_4 int , kd_rek_5 int, Kd_Urusan INT,Kd_Bidang INT,KD_UNIT INT,Kd_Sub INT,KDOPD INT,TGL_BUKTI DATETIME,ANGGARAN MONEY,Realisasi money ) " +
                                                    " DECLARE @Kd_Perubahan tinyint, @TAHUN char(4), @TG1 datetime, @TG2 datetime ,@kdopd nvarchar(10) select @TAHUN = '" + tahun + "', @TG1 = '" + d1 + "', @TG2 = '" + d2 + "', @KDOPD = '" + kdopd + "' " + 
                                                    " SELECT @Kd_Perubahan = MAX(Kd_Perubahan)FROM DBO.Ta_RASK_Arsip_Perubahan WHERE Tahun = @TAHUN AND Kd_Perubahan IN(4, 6, 8) AND Tgl_Perda <= @TG2 " +
                                                    " INSERT INTO @tmpTable SELECT Kd_Prog, ID_Prog, Kd_Keg, Kd_Rek_1, Kd_Rek_2, Kd_Rek_3, Kd_Rek_4, Kd_Rek_5, A.Kd_Urusan, A.Kd_Bidang, A.KD_UNIT, A.Kd_Sub, KDOPD, TGL_BUKTI, SUM(A.ANGGARAN)ANGGARAN, SUM(A.Realisasi) AS Realisasi " +
                                                    " FROM(SELECT  A.Kd_Prog, A.ID_Prog, A.Kd_Keg, Kd_Rek_1, Kd_Rek_2, Kd_Rek_3, Kd_Rek_4, Kd_Rek_5, A.Kd_Urusan, A.Kd_Bidang, A.KD_UNIT, A.Kd_Sub, (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, " +
                                                    " '' TGL_BUKTI, A.Total AS Anggaran, 0 AS Realisasi FROM Ta_RASK_Arsip A  WHERE KD_REK_1 = 5  AND(A.Kd_Perubahan = @Kd_Perubahan) AND(A.Tahun = @Tahun) " +
                                                    " UNION ALL SELECT B.Kd_Prog, B.ID_Prog, B.Kd_Keg, b.Kd_Rek_1, b.Kd_Rek_2, b.Kd_Rek_3, b.Kd_Rek_4, b.Kd_Rek_5, A.Kd_Urusan, A.Kd_Bidang, A.KD_UNIT, A.Kd_Sub, (substring(ltrim(rTRIM(A.KD_URUSAN) + rTRIM(A.KD_BIDANG) + rTRIM(A.KD_UNIT) + rTRIM(A.Kd_Sub)), 0, 6)) KDOPD, " +
                                                    " Tgl_Bukti, 0 AS ANGGARAN, CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS Realisasi " +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc  B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3 WHERE B.KD_REK_1 = 5   AND(B.Tahun = @TAHUN)AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2) " +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8) ) A GROUP BY Kd_Keg, Kd_Prog, ID_Prog, Kd_Rek_1, Kd_Rek_2, Kd_Rek_3, Kd_Rek_4, Kd_Rek_5, KDOPD, Tgl_Bukti, A.Kd_Urusan,A.Kd_Bidang,A.KD_UNIT,A.Kd_Sub " +
                                                    " SELECT  CONVERT(varchar, B.Kd_Urusan) + '.' + RIGHT('0' + CONVERT(varchar, B.Kd_Bidang), 2) + ' . ' + CONVERT(varchar, B.Kd_Urusan) + '.' + RIGHT('0' + CONVERT(varchar, B.Kd_Bidang), 2) + '.' + RIGHT('0' + CONVERT(varchar, B.Kd_Unit), 2) + "+
                                                    " ' . ' + RIGHT('0' + CONVERT(varchar, B.Kd_Sub), 2) + ' . ' + CASE LEN(CONVERT(varchar, B.Kd_Prog)) WHEN 3 THEN CONVERT(varchar, B.Kd_Prog) ELSE RIGHT('0' + CONVERT(varchar, B.Kd_Prog), 2) END + ' . ' + CASE LEN(CONVERT(varchar, B.Kd_Prog)) WHEN 3 THEN CONVERT(varchar, B.Kd_KEG) ELSE RIGHT('0' + CONVERT(varchar, B.Kd_KEG), 2)END AS Kd_Gab_Prog, " +
                                                    " NM_SUB_UNIT,B.Kd_Prog,B.Id_prog,B.Kd_Keg,B.Kd_Rek_1,B.Kd_Rek_2,B.Kd_Rek_3,B.Kd_Rek_4,B.Kd_Rek_5, KET_PROGRAM,KET_KEGIATAN,Nm_Rek_5, " +
                                                    " LTRIM(B.KD_REK_1) + '.' + LTRIM(B.KD_REK_2) + '.' + LTRIM(B.KD_REK_3) + '.' + LTRIM(B.KD_REK_4) + '.' + LTRIM(B.KD_REK_5) AS KDPER, SUM(ANGGARAN) AS ANGGARAN, SUM(REALISASI)REALISASI ,CASE WHEN(SUM(ANGGARAN))<> 0 THEN((SUM(Realisasi) / SUM(ANGGARAN)) * 100)ELSE 0 END AS PRESENTASE " +
                                                    " FROM Ref_Sub_Unit A inner JOIN @tmpTable B ON A.Kd_Bidang = B.Kd_Bidang AND A.Kd_Urusan = B.Kd_Urusan AND A.Kd_Unit = B.KD_UNIT AND A.Kd_Sub = B.Kd_Sub " +
                                                    " INNER JOIN Ta_Program C ON B.Id_prog = C.Id_prog AND B.Kd_prog = C.Kd_Prog AND A.Kd_Urusan = C.Kd_Urusan AND A.Kd_Bidang = C.Kd_Bidang AND A.Kd_Unit = C.Kd_Unit AND A.Kd_Sub = C.Kd_Sub " +
                                                    " INNER JOIN Ta_Kegiatan D ON B.Kd_keg = D.Kd_Keg AND B.Id_prog = D.ID_Prog AND B.KD_PROG = D.KD_PROG AND A.Kd_Urusan = D.Kd_Urusan AND A.Kd_Bidang = D.Kd_Bidang AND A.Kd_Unit = D.Kd_Unit AND A.Kd_Sub = D.Kd_Sub " +
                                                    " INNER JOIN Ref_Rek_5 E ON B.Kd_Rek_1 = E.kd_rek_1 AND B.Kd_Rek_2 = E.kd_rek_2 AND B.kd_rek_3 = E.Kd_Rek_3 AND B.Kd_Rek_4 = E.kd_rek_4 AND B.kd_rek_5 = E.KD_REK_5 " +
                                                    " where KDOPD = @KDOPD GROUP BY B.KD_URUSAN,B.KD_BIDANG,B.KD_UNIT,B.KD_SUB,B.Kd_Prog,B.KD_KEG,B.Id_prog,B.Kd_Keg,B.Kd_Rek_1,B.Kd_Rek_2,B.Kd_Rek_3,B.Kd_Rek_4,B.Kd_Rek_5,NM_SUB_UNIT, KET_PROGRAM,KET_KEGIATAN,NM_REK_5  ORDER BY B.Kd_Prog, B.Kd_Keg " +
                                                    "" +
                                                    "", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new LraDetailBelanjaOpd()
                                 {
                                     Kdgabprog = (dr["Kd_Gab_Prog"]).ToString(),
                                     Nmopd = (dr["NM_SUB_UNIT"]).ToString(),
                                     Nmprog = (dr["ket_program"]).ToString(),
                                     Nmkeg = (dr["ket_kegiatan"]).ToString(),
                                     Nmrek = (dr["nm_rek_5"]).ToString(),
                                     Kdper = (dr["kdper"]).ToString(),
                                     Anggaran = Convert.ToDecimal(dr["Anggaran"]),
                                     Realisasi = Convert.ToDecimal(dr["realisasi"]),
                                     Prosen = Convert.ToDecimal(dr["presentase"])
                                   



                                 }).OrderBy(c => c.Kdgabprog).ToList();

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
        [Route("lrahibah/{tahun?}/{d1?}/{d2?}")]
        public IActionResult GetLraHibah(string tahun, DateTime d1, DateTime d2)
        {
            
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<LraHibahBulan> dataVM = new List<LraHibahBulan>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " DECLARE @tmpTable TABLE(TGL_BUKTI DATETIME,ANGGARAN MONEY,     REALISASI money   ) "+
                                                    " DECLARE @tmpReal TABLE(KDBULAN INT, ANGGARAN MONEY, Realisasi money) " +
                                                    " DECLARE @Kd_Perubahan tinyint, @TAHUN char(4), @TG1 datetime, @TG2 datetime  select @TAHUN = '"+tahun+"', @TG1 = '"+d1+"', @TG2 = ' "+d2+"' " +
                                                    " SELECT @Kd_Perubahan = MAX(Kd_Perubahan)FROM DBO.Ta_RASK_Arsip_Perubahan WHERE Tahun = @TAHUN AND Kd_Perubahan IN(4, 6, 8) AND Tgl_Perda <= @TG2 " +
                                                    " INSERT INTO @tmpTable SELECT  TGL_BUKTI, SUM(A.ANGGARAN)anggaran, SUM(A.Realisasi) AS Realisasi " +
                                                    " FROM(SELECT '' TGL_BUKTI, A.Total AS Anggaran, 0 AS Realisasi  FROM Ta_RASK_Arsip A " +
                                                    " WHERE KD_REK_1 = 5 AND KD_REK_2 = 1 and KD_REK_3 = 4 AND(A.Kd_Perubahan = @Kd_Perubahan) AND(A.Tahun = @Tahun) " +
                                                    " UNION ALL SELECT  Tgl_Bukti, 0 AS ANGGARAN, CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS REALISASI " +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc  B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND " +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE  B.KD_REK_1 = 5 AND B.KD_REK_2 = 1 and B.KD_REK_3 = 4 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2) " +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8) " +
                                                    " ) A GROUP BY Tgl_Bukti " +
                                                    " INSERT INTO @tmpReal SELECT KDBULAN,SUM(ANGGARAN)ANGGARAN,SUM(REALISASI) REALISASI " +
                                                    " FROM(SELECT 1 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 1 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 1 " +
                                                    " UNION ALL SELECT 2 KDBULAN, ANGGARAN, " +
                                                    " (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 2 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 2 " +
                                                    " UNION ALL SELECT 3 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 3 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 3 " +
                                                    " UNION ALL SELECT 4 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 4 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 4 " +
                                                    " UNION ALL SELECT 5 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 5 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 5 " +
                                                    " UNION ALL SELECT 6 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 6 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 6 " +
                                                    " UNION ALL SELECT 7 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 7 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 7 " +
                                                    " UNION ALL SELECT 8 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 8 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 8 " +
                                                    " UNION ALL SELECT 9 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 9 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 9 " +
                                                    " UNION ALL SELECT 10 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 10 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 10 " +
                                                    " UNION ALL SELECT 11 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 11 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 11 " +
                                                    " UNION ALL SELECT 12 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 12 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 12 " +
                                                    " )A GROUP BY KDBULAN " +
                                                    " SELECT A.KDBULAN,ISNULL((ANGGARAN), 0)ANGGARAN,ISNULL((REALISASI), 0)REALISASI " +
                                                    " FROM  SIMDABRIDGE.DBO.BULAN A LEFT JOIN @tmpReal B ON A.KDBULAN = B.KDBULAN " + 
                                                    " ", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new LraHibahBulan()
                                 {
                                     Bulan = Convert.ToInt32 (dr["KDBULAN"]),                                    
                                     Anggaran = Convert.ToDecimal(dr["Anggaran"]),
                                     Realisasi = Convert.ToDecimal(dr["realisasi"])
                            




                                 }).OrderBy(c => c.Bulan).ToList();

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
        [Route("dthibahdetail/{tahun?}/{d1?}/{d2?}/{filter?}")]
        public IActionResult GetHibahDetail(string tahun, DateTime d1, DateTime d2,string filter=null)
        {
           
            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<DataHibahPerBukti> dataVM = new List<DataHibahPerBukti>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    if (!String.IsNullOrEmpty(filter))
                    {
                        SqlConnection conn = konn.GetConn();
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                        " declare @tahun nvarchar(4) ,@tg1 datetime, @tg2 datetime,@filter nvarchar (100) " +
                                                        " select @tahun='" + tahun + "', @tg1='" + d1 + "',  @tg2='" + d2 + "' ,@filter='"+filter+"'"+
                                                        " select nm_sub_unit ,TGL_SPM,A.No_SPM,Tgl_SP2D,NO_SP2D,Keterangan,Nm_Penerima,Bank_Penerima,Rek_Penerima,NPWP, " +
                                                        " LTRIM(b.Kd_Rek_1)+'.'+LTRIM(b.Kd_Rek_2)+'.'+LTRIM(b.Kd_Rek_3)+'.'+LTRIM(b.Kd_Rek_4)+'.'+LTRIM(b.Kd_Rek_5)+'.' as kdrek,Nm_Rek_5 nmrek, nilai" +
                                                        " from ta_spm a inner join Ta_SPM_Rinc b on a.No_SPM=b.No_SPM inner join Ta_SP2D c on a.No_SPM=c.No_SPM AND B.NO_SPM=C.NO_SPM" +
                                                        " inner join Ref_Rek_5 e on b.Kd_Rek_1=e.Kd_Rek_1 and b.Kd_Rek_2=e.Kd_Rek_2 and b.Kd_Rek_3=e.Kd_Rek_3 and b.Kd_Rek_4=e.Kd_Rek_4 and b.Kd_Rek_5=e.Kd_Rek_5 " +
                                                        " INNER JOIN REF_SUB_unit F ON A.KD_URUSAN=F.KD_URUSAN AND A.KD_BIDANG=F.KD_BIDANG AND A.KD_UNIT=F.KD_UNIT AND A.KD_SUB=F.KD_SUB" +
                                                        " where " +
                                                        " (c.Tgl_sp2d BETWEEN @TG1 AND  @TG2 )and  b.tahun =@tahun and b.Kd_Rek_1 = 5 and b.Kd_Rek_2 = 1 and b.Kd_Rek_3 = 4 and No_SP2D in (select a.no_bukti from Ta_JurnalSemua a inner join " +
                                                        " Ta_JurnalSemua_Rinc b on a.No_Bukti = b.No_Bukti where b.tahun =@tahun and ((CASE Kd_SKPD WHEN 1 THEN Posting ELSE Posting_SKPKD END) = 1)) " +
                                                        " AND (((isnull(@filter, '') = '' or No_sp2d like '%' + @filter + '%')" +
                                                        " OR (isnull(@filter, '') = '' or Nm_Penerima like '%' + @filter + '%')" +
                                                        " OR (isnull(@filter, '') = '' or KETERANGAN like '%' + @filter + '%')))" +                                                        
                                                        
                                                        " ", conn);
                        cmd.ExecuteNonQuery();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        var table = (from DataRow dr in dt.Rows

                                     select new DataHibahPerBukti()
                                     {
                                         Nmunit = (dr["nm_sub_unit"]).ToString().TrimEnd(),
                                         Tglsp2d = Convert.ToDateTime(dr["Tgl_sp2d"]),
                                         Tglspm = Convert.ToDateTime(dr["Tgl_spm"]),
                                         Nospm =  (dr["No_spm"]).ToString().TrimEnd(),
                                         Nosp2d = (dr["No_sp2d"]).ToString().TrimEnd(),
                                         Keterangan = (dr["Keterangan"]).ToString().TrimEnd(),
                                         NamaPenerima = (dr["Nm_penerima"]).ToString().TrimEnd(),
                                         BankPenerima = (dr["Bank_penerima"]).ToString().TrimEnd(),
                                         RekPenerima = (dr["Rek_penerima"]).ToString().TrimEnd(),
                                         Kdrek = (dr["Kdrek"]).ToString().TrimEnd(),
                                         Nmrek = (dr["nmrek"]).ToString().TrimEnd(),
                                         Npwp = (dr["Npwp"]).ToString().TrimEnd(),
                                         Nilai = Convert.ToDecimal(dr["nilai"])





                                     }).OrderBy(c => c.Tglsp2d).ToList();

                        dataVM = table.ToList();
                        conn.Close();
                    }
                    else
                    {
                        SqlConnection conn = konn.GetConn();
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                " declare @tahun nvarchar(4) ,@tg1 datetime, @tg2 datetime,@filter nvarchar (100) " +
                                " select @tahun='" + tahun + "',  @tg1='" + d1 + "',  @tg2='" + d2 + "' ,@filter='" + filter + "'" +
                                " select nm_sub_unit,TGL_SPM,A.No_SPM,Tgl_SP2D,NO_SP2D,Keterangan,Nm_Penerima,Bank_Penerima,Rek_Penerima,NPWP, " +
                                " LTRIM(b.Kd_Rek_1)+'.'+LTRIM(b.Kd_Rek_2)+'.'+LTRIM(b.Kd_Rek_3)+'.'+LTRIM(b.Kd_Rek_4)+'.'+LTRIM(b.Kd_Rek_5)+'.' as kdrek,Nm_Rek_5 nmrek, nilai" +
                                " from ta_spm a inner join Ta_SPM_Rinc b on a.No_SPM=b.No_SPM inner join Ta_SP2D c on a.No_SPM=c.No_SPM " +
                                " inner join Ref_Rek_5 e on b.Kd_Rek_1=e.Kd_Rek_1 and b.Kd_Rek_2=e.Kd_Rek_2 and b.Kd_Rek_3=e.Kd_Rek_3 and b.Kd_Rek_4=e.Kd_Rek_4 and b.Kd_Rek_5=e.Kd_Rek_5 " +
                                " INNER JOIN REF_SUB_unit F ON A.KD_URUSAN=F.KD_URUSAN AND A.KD_BIDANG=F.KD_BIDANG AND A.KD_UNIT=F.KD_UNIT AND A.KD_SUB=F.KD_SUB" +
                                " where " +                                 
                                " (c.Tgl_sp2d BETWEEN @TG1 AND  @TG2 )and  b.tahun =@tahun and b.Kd_Rek_1 = 5 and b.Kd_Rek_2 = 1 and b.Kd_Rek_3 = 4 and No_SP2D in (select a.no_bukti from Ta_JurnalSemua a inner join " +
                                " Ta_JurnalSemua_Rinc b on a.No_Bukti = b.No_Bukti where b.tahun =@tahun and ((CASE Kd_SKPD WHEN 1 THEN Posting ELSE Posting_SKPKD END) = 1)) " +
                               
                                " ", conn);
                        cmd.ExecuteNonQuery();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        var table = (from DataRow dr in dt.Rows

                                     select new DataHibahPerBukti()
                                     {
                                         Nmunit = (dr["nm_sub_unit"]).ToString().TrimEnd(),
                                         Tglsp2d = Convert.ToDateTime(dr["Tgl_sp2d"]),
                                         Tglspm = Convert.ToDateTime(dr["Tgl_spm"]),
                                         Nospm = (dr["No_spm"]).ToString().TrimEnd(),
                                         Nosp2d = (dr["No_sp2d"]).ToString().TrimEnd(),
                                         Keterangan = (dr["Keterangan"]).ToString().TrimEnd(),
                                         NamaPenerima = (dr["Nm_penerima"]).ToString().TrimEnd(),
                                         BankPenerima = (dr["Bank_penerima"]).ToString().TrimEnd(),
                                         RekPenerima = (dr["Rek_penerima"]).ToString().TrimEnd(),
                                         Kdrek = (dr["Kdrek"]).ToString().TrimEnd(),
                                         Nmrek = (dr["nmrek"]).ToString().TrimEnd(),
                                         Npwp = (dr["Npwp"]).ToString().TrimEnd(),
                                         Nilai = Convert.ToDecimal(dr["nilai"])




                                     }).OrderBy(c => c.Tglsp2d).ToList();

                        dataVM = table.ToList();
                        conn.Close();

                    }


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
        [Route("lradndesa/{tahun?}/{d1?}/{d2?}")]
        public IActionResult GetLraDnDesa(string tahun, DateTime d1, DateTime d2)
        {

            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<LraDnDesaBulan> dataVM = new List<LraDnDesaBulan>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    SqlConnection conn = konn.GetConn();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                    " DECLARE @tmpTable TABLE(TGL_BUKTI DATETIME,ANGGARAN MONEY,     REALISASI money   ) " +
                                                    " DECLARE @tmpReal TABLE(KDBULAN INT, ANGGARAN MONEY, Realisasi money) " +
                                                    " DECLARE @Kd_Perubahan tinyint, @TAHUN char(4), @TG1 datetime, @TG2 datetime  select @TAHUN = '" + tahun + "', @TG1 = '" + d1 + "', @TG2 = ' " + d2 + "' " +
                                                    " SELECT @Kd_Perubahan = MAX(Kd_Perubahan)FROM DBO.Ta_RASK_Arsip_Perubahan WHERE Tahun = @TAHUN AND Kd_Perubahan IN(4, 6, 8) AND Tgl_Perda <= @TG2 " +
                                                    " INSERT INTO @tmpTable SELECT  TGL_BUKTI, SUM(A.ANGGARAN)anggaran, SUM(A.Realisasi) AS Realisasi " +
                                                    " FROM(SELECT '' TGL_BUKTI, A.Total AS Anggaran, 0 AS Realisasi  FROM Ta_RASK_Arsip A " +
                                                    " WHERE KD_REK_1 = 5 AND KD_REK_2 = 1 and KD_REK_3 = 7 AND(A.Kd_Perubahan = @Kd_Perubahan) AND(A.Tahun = @Tahun) " +
                                                    " UNION ALL SELECT  Tgl_Bukti, 0 AS ANGGARAN, CASE C.SaldoNorm WHEN 'D' THEN B.Debet - B.Kredit ELSE B.Kredit - B.Debet END AS REALISASI " +
                                                    " FROM DBO.Ta_JurnalSemua A INNER JOIN DBO.Ta_JurnalSemua_Rinc  B ON A.Tahun = B.Tahun AND A.Kd_Source = B.Kd_Source AND " +
                                                    " A.No_Bukti = B.No_Bukti INNER JOIN DBO.Ref_Rek_3 C ON B.Kd_Rek_1 = C.Kd_Rek_1 AND B.Kd_Rek_2 = C.Kd_Rek_2 AND B.Kd_Rek_3 = C.Kd_Rek_3 " +
                                                    " WHERE  B.KD_REK_1 = 5 AND B.KD_REK_2 = 1 and B.KD_REK_3 = 7 AND(B.Tahun = @TAHUN )AND(A.Tgl_Bukti BETWEEN @TG1 AND  @TG2) " +
                                                    " AND((CASE B.Kd_SKPD WHEN 1 THEN A.Posting ELSE A.Posting_SKPKD END) = 1)AND(B.Kd_Jurnal <> 8) " +
                                                    " ) A GROUP BY Tgl_Bukti " +
                                                    " INSERT INTO @tmpReal SELECT KDBULAN,SUM(ANGGARAN)ANGGARAN,SUM(REALISASI) REALISASI " +
                                                     " FROM(SELECT 1 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 1 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 1 " +
                                                    " UNION ALL SELECT 2 KDBULAN, ANGGARAN, " +
                                                    " (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 2 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 2 " +
                                                    " UNION ALL SELECT 3 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 3 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 3 " +
                                                    " UNION ALL SELECT 4 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 4 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 4 " +
                                                    " UNION ALL SELECT 5 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 5 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 5 " +
                                                    " UNION ALL SELECT 6 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 6 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 6 " +
                                                    " UNION ALL SELECT 7 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 7 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 7 " +
                                                    " UNION ALL SELECT 8 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 8 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 8 " +
                                                    " UNION ALL SELECT 9 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 9 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 9 " +
                                                    " UNION ALL SELECT 10 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 10 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 10 " +
                                                    " UNION ALL SELECT 11 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 11 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 11 " +
                                                    " UNION ALL SELECT 12 KDBULAN, ANGGARAN, (CASE 0 WHEN ISNULL((SELECT SUM (REALISASI) FROM @tmpTable WHERE MONTH(TGL_BUKTI) = 12 ),0) THEN 0* REALISASI ELSE REALISASI END) REALISASI  FROM @tmpTable WHERE MONTH(TGL_BUKTI) <= 12 " +
                                                    " )A GROUP BY KDBULAN " +
                                                    " SELECT A.KDBULAN,ISNULL((ANGGARAN), 0)ANGGARAN,ISNULL((REALISASI), 0)REALISASI " +
                                                    " FROM  SIMDABRIDGE.DBO.BULAN A LEFT JOIN @tmpReal B ON A.KDBULAN = B.KDBULAN " +
                                                    " ", conn);
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    var table = (from DataRow dr in dt.Rows

                                 select new LraDnDesaBulan()
                                 {
                                     Bulan = Convert.ToInt32(dr["KDBULAN"]),
                                     Anggaran = Convert.ToDecimal(dr["Anggaran"]),
                                     Realisasi = Convert.ToDecimal(dr["realisasi"])





                                 }).OrderBy(c => c.Bulan).ToList();

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
        [Route("dtdndesadetail/{tahun?}/{d1?}/{d2?}/{filter?}")]
        public IActionResult GetdtDnDesaDetail(string tahun, DateTime d1, DateTime d2, string filter = null)
        {

            getdb.GetDb(tahun);
            string nmdb = getdb.Nmdbsimda.TrimEnd();
            IEnumerable<DataDnDesaPerBukti> dataVM = new List<DataDnDesaPerBukti>();
            if (!String.IsNullOrEmpty(nmdb))
            {
                try
                {
                    if (!String.IsNullOrEmpty(filter))
                    {
                        SqlConnection conn = konn.GetConn();
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                                        " declare @tahun nvarchar(4) ,@tg1 datetime, @tg2 datetime,@filter nvarchar (100) " +
                                                        " select @tahun='" + tahun + "', @tg1='" + d1 + "',  @tg2='" + d2 + "' ,@filter='" + filter + "'" +
                                                        " select nm_sub_unit ,TGL_SPM,A.No_SPM,Tgl_SP2D,NO_SP2D,Keterangan,Nm_Penerima,Bank_Penerima,Rek_Penerima,NPWP, " +
                                                        " LTRIM(b.Kd_Rek_1)+'.'+LTRIM(b.Kd_Rek_2)+'.'+LTRIM(b.Kd_Rek_3)+'.'+LTRIM(b.Kd_Rek_4)+'.'+LTRIM(b.Kd_Rek_5)+'.' as kdrek,Nm_Rek_5 nmrek, nilai" +
                                                        " from ta_spm a inner join Ta_SPM_Rinc b on a.No_SPM=b.No_SPM inner join Ta_SP2D c on a.No_SPM=c.No_SPM AND B.NO_SPM=C.NO_SPM" +
                                                        " inner join Ref_Rek_5 e on b.Kd_Rek_1=e.Kd_Rek_1 and b.Kd_Rek_2=e.Kd_Rek_2 and b.Kd_Rek_3=e.Kd_Rek_3 and b.Kd_Rek_4=e.Kd_Rek_4 and b.Kd_Rek_5=e.Kd_Rek_5 " +
                                                        " INNER JOIN REF_SUB_unit F ON A.KD_URUSAN=F.KD_URUSAN AND A.KD_BIDANG=F.KD_BIDANG AND A.KD_UNIT=F.KD_UNIT AND A.KD_SUB=F.KD_SUB" +
                                                        " where " +
                                                        " (c.Tgl_sp2d BETWEEN @TG1 AND  @TG2 )and  b.tahun =@tahun and b.Kd_Rek_1 = 5 and b.Kd_Rek_2 = 1 and b.Kd_Rek_3 = 7 and No_SP2D in (select a.no_bukti from Ta_JurnalSemua a inner join " +
                                                        " Ta_JurnalSemua_Rinc b on a.No_Bukti = b.No_Bukti where b.tahun =@tahun and ((CASE Kd_SKPD WHEN 1 THEN Posting ELSE Posting_SKPKD END) = 1)) " +
                                                        " AND (((isnull(@filter, '') = '' or No_sp2d like '%' + @filter + '%')" +
                                                        " OR (isnull(@filter, '') = '' or Nm_Penerima like '%' + @filter + '%')" +
                                                        " OR (isnull(@filter, '') = '' or KETERANGAN like '%' + @filter + '%')))" +

                                                        " ", conn);
                        cmd.ExecuteNonQuery();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        var table = (from DataRow dr in dt.Rows

                                     select new DataDnDesaPerBukti()
                                     {
                                         Nmunit = (dr["nm_sub_unit"]).ToString().TrimEnd(),
                                         Tglsp2d = Convert.ToDateTime(dr["Tgl_sp2d"]),
                                         Tglspm = Convert.ToDateTime(dr["Tgl_spm"]),
                                         Nospm = (dr["No_spm"]).ToString().TrimEnd(),
                                         Nosp2d = (dr["No_sp2d"]).ToString().TrimEnd(),
                                         Keterangan = (dr["Keterangan"]).ToString().TrimEnd(),
                                         NamaPenerima = (dr["Nm_penerima"]).ToString().TrimEnd(),
                                         BankPenerima = (dr["Bank_penerima"]).ToString().TrimEnd(),
                                         RekPenerima = (dr["Rek_penerima"]).ToString().TrimEnd(),
                                         Kdrek = (dr["Kdrek"]).ToString().TrimEnd(),
                                         Nmrek = (dr["nmrek"]).ToString().TrimEnd(),
                                         Npwp = (dr["Npwp"]).ToString().TrimEnd(),
                                         Nilai = Convert.ToDecimal(dr["nilai"])





                                     }).OrderBy(c => c.Tglsp2d).ToList();

                        dataVM = table.ToList();
                        conn.Close();
                    }
                    else
                    {
                        SqlConnection conn = konn.GetConn();
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(" USE " + nmdb + "" +
                                " declare @tahun nvarchar(4) ,@tg1 datetime, @tg2 datetime,@filter nvarchar (100) " +
                                " select @tahun='" + tahun + "',  @tg1='" + d1 + "',  @tg2='" + d2 + "' ,@filter='" + filter + "'" +
                                " select nm_sub_unit,TGL_SPM,A.No_SPM,Tgl_SP2D,NO_SP2D,Keterangan,Nm_Penerima,Bank_Penerima,Rek_Penerima,NPWP, " +
                                " LTRIM(b.Kd_Rek_1)+'.'+LTRIM(b.Kd_Rek_2)+'.'+LTRIM(b.Kd_Rek_3)+'.'+LTRIM(b.Kd_Rek_4)+'.'+LTRIM(b.Kd_Rek_5)+'.' as kdrek,Nm_Rek_5 nmrek, nilai" +
                                " from ta_spm a inner join Ta_SPM_Rinc b on a.No_SPM=b.No_SPM inner join Ta_SP2D c on a.No_SPM=c.No_SPM " +
                                " inner join Ref_Rek_5 e on b.Kd_Rek_1=e.Kd_Rek_1 and b.Kd_Rek_2=e.Kd_Rek_2 and b.Kd_Rek_3=e.Kd_Rek_3 and b.Kd_Rek_4=e.Kd_Rek_4 and b.Kd_Rek_5=e.Kd_Rek_5 " +
                                " INNER JOIN REF_SUB_unit F ON A.KD_URUSAN=F.KD_URUSAN AND A.KD_BIDANG=F.KD_BIDANG AND A.KD_UNIT=F.KD_UNIT AND A.KD_SUB=F.KD_SUB" +
                                " where " +
                                " (c.Tgl_sp2d BETWEEN @TG1 AND  @TG2 )and  b.tahun =@tahun and b.Kd_Rek_1 = 5 and b.Kd_Rek_2 = 1 and b.Kd_Rek_3 = 7 and No_SP2D in (select a.no_bukti from Ta_JurnalSemua a inner join " +
                                " Ta_JurnalSemua_Rinc b on a.No_Bukti = b.No_Bukti where b.tahun =@tahun and ((CASE Kd_SKPD WHEN 1 THEN Posting ELSE Posting_SKPKD END) = 1)) " +

                                " ", conn);
                        cmd.ExecuteNonQuery();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        var table = (from DataRow dr in dt.Rows

                                     select new DataDnDesaPerBukti()
                                     {
                                         Nmunit = (dr["nm_sub_unit"]).ToString().TrimEnd(),
                                         Tglsp2d = Convert.ToDateTime(dr["Tgl_sp2d"]),
                                         Tglspm = Convert.ToDateTime(dr["Tgl_spm"]),
                                         Nospm = (dr["No_spm"]).ToString().TrimEnd(),
                                         Nosp2d = (dr["No_sp2d"]).ToString().TrimEnd(),
                                         Keterangan = (dr["Keterangan"]).ToString().TrimEnd(),
                                         NamaPenerima = (dr["Nm_penerima"]).ToString().TrimEnd(),
                                         BankPenerima = (dr["Bank_penerima"]).ToString().TrimEnd(),
                                         RekPenerima = (dr["Rek_penerima"]).ToString().TrimEnd(),
                                         Kdrek = (dr["Kdrek"]).ToString().TrimEnd(),
                                         Nmrek = (dr["nmrek"]).ToString().TrimEnd(),
                                         Npwp = (dr["Npwp"]).ToString().TrimEnd(),
                                         Nilai = Convert.ToDecimal(dr["nilai"])




                                     }).OrderBy(c => c.Tglsp2d).ToList();

                        dataVM = table.ToList();
                        conn.Close();

                    }


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

        //[HttpGet]
        //[Route("lrapendapatan/{tahun?}/{d1?}")]
        //public IActionResult GetData(string tahun, string d1)
        //{
        //    //var xDoc = XDocument.Parse((System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/StoreProcedure/test.xml")));

        //    // with Descendents
        //    //var customer = xDoc.Root.Element("Sp");
        //    //var optinStatus = customer.Element("query");
        //    //Console.WriteLine(status.Value);
        //    //var status = xDoc.Descendants("query2").Single();

        //    IEnumerable<DataLRAPPKD> dataVM = new List<DataLRAPPKD>();
        //    try
        //    {
        //        SqlConnection conn = konn.GetConn();
        //        conn.Open();
        //        SqlCommand cmd = new SqlCommand("RptLRAPPKD", conn);
        //        cmd.Parameters.AddWithValue("@tahun", tahun);
        //        cmd.Parameters.AddWithValue("@d1", Convert.ToDateTime(d1));
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.ExecuteNonQuery();
        //        SqlDataAdapter da = new SqlDataAdapter(cmd);
        //        DataTable dt = new DataTable();
        //        da.Fill(dt);
        //        var table = (from DataRow dr in dt.Rows

        //                     select new DataLRAPPKD()
        //                     {
        //                         Realisasi = Convert.ToDecimal(dr["realisasi"]),
        //                         Kdlra1 = Convert.ToInt16(dr["kd_rek_1"])

        //                     }).OrderBy(c => c.Kdlra1).ToList();
        //        //var peg = table.ToList();
        //        //var daftunitVm = Mapper.Map<IEnumerable<Daftunit>, IEnumerable<DaftunitViewModel>>(daftunit);
        //        dataVM = table.ToList();
        //        conn.Close();

        //    }
        //    catch (SqlException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    return Ok(dataVM);
        //}

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

        public string Nmdbsimda { get; set; }
        // GET api/values/5
        [HttpGet("datatahun/{tahun?}")]
        public ActionResult GetTahun(string tahun=null)
        {
            var articles = new object();
            //string th = "2018";
            try
            {
                //var file = (JObject)JsonConvert.DeserializeObject(System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/AppConfig/simdapropertis.json"));
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

                                }).Where(c=> c.Tahun ==tahun).ToList();// mengambil data dalam format array json
                    Nmdbsimda = data.Select(c => c.Nmdatabase).FirstOrDefault();// jika mengambil data spesifik 
                    return Ok(data);
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


            var xDoc = XDocument.Parse((System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/StoreProcedure/test.xml")));

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