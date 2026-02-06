using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Ekutivala_EAD.Models
{
    public class LivroModel
    {
        public int IdLivro { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Autor")]
        public string Autor { get; set; }

        [Required]
        [Range(0, 100000000)]
        [Display(Name = "Preço")]
        public decimal Preco { get; set; }

        [StringLength(100)]
        [Display(Name = "Formato")]
        public string Formato { get; set; }

        [StringLength(100)]
        [Display(Name = "Categoria")]
        public string Categoria { get; set; }

        [Display(Name = "Imagem")]
        public string ImagemCaminho { get; set; }

        [Display(Name = "Link")]
        public string LinkCaminho { get; set; }

        [Display(Name = "Estado")]
        public string Status { get; set; } = "ATIVO";

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}