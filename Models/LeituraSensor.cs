using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceAgro.DotNetApi.Models
{
    [Table("TB_LEITURA_SENSOR")]
    public class LeituraSensor
    {
        [Key]
        [Column("ID_LEITURA")] // Em maiúsculo
        public int Id { get; set; }

        [Column("TEMPERATURA")]
        public double Temperatura { get; set; }

        [Column("UMIDADE_AR")]
        public double UmidadeAr { get; set; }

        [Column("UMIDADE_SOLO")]
        public double UmidadeSolo { get; set; }

        [Column("DATA_HORA")]
        public DateTime DataHora { get; set; }

        [Column("ID_DISPOSITIVO")]
        public int IdDispositivo { get; set; }
    }
}