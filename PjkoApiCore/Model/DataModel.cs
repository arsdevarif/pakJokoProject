using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PjkoApiCore.Model
{
    public class DataModel
    {
    }
    public class DataLRAPPKD
    {
        public decimal Realisasi { get; set; }
        public int Kdlra1 { get; set; }
    }
    public class DataLRA_Gabungan
    {
        public decimal Anggaran { get; set; }
        public decimal Realisasi_Lalu { get; set; }
        public decimal Realisasi_Ini { get; set; }
        public decimal Realisasi_Total { get; set; }
        public decimal Sisa { get; set; }
        public string Kd_rek { get; set; }
        public string Kd_rek_all { get; set; }
        public string Nm_rek { get; set; }
    
    }
    public class DataTahun
    {
        public int Id { get; set; }
        public string Tahun { get; set; }
        public string Nmdatabase { get; set; }
         
    }
    public class ParamRekJenisPen
    {
        public string Kd_rek { get; set; }
        public string Nmrek { get; set; }
       
    }
    public class ParamRekObjekPen
    {
        public string Kd_rek { get; set; }
        public string Nmrek { get; set; }
    }
    public class ParamDataOpd
    {
        public string Kdopd { get; set; }
        public string Nmsubunit { get; set; }
    }
    public class ParamDataHibah
    {
        public string Kd_rek { get; set; }
        public string Nmrek { get; set; }
    }
    public class ParamDataDanaDesa
    {
        public string Kd_rek { get; set; }
        public string Nmrek { get; set; }
    }
    public class LraPenObj
    {
        public int Bulan { get; set; }
        public decimal Anggaran { get; set; }
        public decimal Realisasi { get; set; }
    }
    public class LraHibahBulan
    {
        public int Bulan { get; set; }
        public decimal Anggaran { get; set; }
        public decimal Realisasi { get; set; }

    }
    public class DataHibahPerBukti
    {
        public string Nmunit { get; set; }
        public DateTime Tglspm { get; set; }
        public DateTime Tglsp2d { get; set; }
        public string Nospm { get; set; }
        public string Nosp2d { get; set; }
        public string Keterangan { get; set; }
        public string NamaPenerima { get; set; }
        public string BankPenerima { get; set; }
        public string RekPenerima { get; set; }
        public string Npwp { get; set; }
        public string Kdrek { get; set; }
        public string Nmrek { get; set; }
        public decimal Nilai { get; set; }



    }
    public class LraDnDesaBulan
    {
        public int Bulan { get; set; }
        public decimal Anggaran { get; set; }
        public decimal Realisasi { get; set; }
    }
    public class DataDnDesaPerBukti
    {
        public string Nmunit { get; set; }
        public DateTime Tglspm { get; set; }
        public DateTime Tglsp2d { get; set; }
        public string Nospm { get; set; }
        public string Nosp2d { get; set; }
        public string Keterangan { get; set; }
        public string NamaPenerima { get; set; }
        public string BankPenerima { get; set; }
        public string RekPenerima { get; set; }
        public string Npwp { get; set; }
        public string Kdrek { get; set; }
        public string Nmrek { get; set; }
        public decimal Nilai { get; set; }



    }
    public class LraBelanjaGroup
    {
    
        public int Kdgroup { get; set; }
        public string Nmgroup { get; set; }
        public decimal AnggaranbL { get; set; }
        public decimal Anggaranbtl { get; set; }
        public decimal Realisasibl { get; set; }
        public decimal Realisasibtl { get; set; }
    }
    public class LraBelanjaGroupOpd
    {

        public int Kdgroup { get; set; }
        public int Kdopd { get; set; }
        public string Nmunit { get; set; }
        public decimal AnggaranbL { get; set; }
        public decimal Anggaranbtl { get; set; }
        public decimal Realisasibl { get; set; }
        public decimal Realisasibtl { get; set; }
    }
    public class LraBelanja
    {
        public int Kdurusan { get; set; }
        public int Kdbidang { get; set; }
        public int Kdunit { get; set; }
        public int Kdsub { get; set; }
        public int Kdop { get; set; }
        public string Nmunit { get; set; }
        public decimal AnggaranbL { get; set; }
        public decimal Anggaranbtl { get; set; }
        public decimal Realisasibl { get; set; }
        public decimal Realisasibtl { get; set; }
    }
    public class LraBelanjaOpdBulan
    {
        public int Kdopd { get; set; }
        public int Kdbulan { get; set; } 
        public decimal Anggaran { get; set; }   
        public decimal Realisasibl { get; set; }
        public decimal Realisasibtl { get; set; }
    }
    public class LraDetailBelanjaOpd
    {

        public string Kdgabprog { get; set; }
        public string Nmopd { get; set; }
        public string Nmprog { get; set; }
        public string Nmkeg { get; set; }
        public string Nmrek { get; set; }
        public string Kdper { get; set; }
        public decimal Anggaran{ get; set; }    
        public decimal Realisasi { get; set; }
        public decimal Prosen { get; set; }
    }
}
