﻿using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("INTEGRANTES_EVENTOS")]
    public class IntegrantesEventos
    {
        public int Id { get; set; }

        [ForeignKey("Id_Evento")]
        public Evento? Evento { get; set; }
        public int? Id_Evento { get; set; }

        [ForeignKey("Id_Usuario")]
        public Usuario? Usuario { get; set; }
        public int? Id_Usuario { get; set; }
        public bool Leido { get; set; }
        public bool Confirmado { get; set; }

    }
}
