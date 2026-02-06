using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Ekutivala_EAD.Models
{
   public class NotificacaoEstudanteModel
    {
        public int IdNotificacao { get; set; }

        [Required]
        public int IdEstudante { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Mensagem")]
        public string Mensagem { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; } 
        
    }

}