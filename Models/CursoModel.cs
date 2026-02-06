using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Ekutivala_EAD.Models
{
    public class CursoModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string ImagemUrl { get; set; }
        public string PaginaUrl { get; set; }
        public string CursoUrl { get; set; }

        [Required]
        [Range(0, 100000000)]
        [Display(Name = "Preço")]
        public decimal Preco { get; set; }
        public int QuantidadeProfessores { get; set; }
 
        public DateTime DataPublicacao { get; set; }
        public int Curtidas { get; set; }
        public string Tag { get; set; }
    }
}