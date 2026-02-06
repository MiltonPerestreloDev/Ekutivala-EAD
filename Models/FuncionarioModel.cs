using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Ekutivala_EAD.Models
{
    public class FuncionarioModel
    {
        [Key]
        public int IdFunc { get; set; }
        public string NomeFunc { get; set; }
        public string EmailFunc { get; set; }
        public string SenhaFunc { get; set; }
        public string StatusFunc { get; set; } = "ATIVO"; // ATIVO, INATIVO, BLOQUEADO
        public string AcessoFunc { get; set; } // A, B e C
        public DateTime DataCadastroFunc { get; set; } = DateTime.Now;
    }
    
}