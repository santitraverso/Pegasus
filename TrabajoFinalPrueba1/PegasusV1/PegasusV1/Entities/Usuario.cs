﻿using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    public class Usuario
    {
        public int? Id { get; set; }

        public string? Nombre { get; set; }

        public string? Apellido { get; set; }

        public int? Perfil { get; set; }
        [ForeignKey("Perfil")]
        public Roles Rol { get; set; }

        public string? Mail { get; set; }

        public bool? Activo { get; set; }
    }
}
