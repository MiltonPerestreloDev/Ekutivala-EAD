using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Ekutivala_EAD.Models
{
    public class VendaCursoModel
    {
        [Key]
        public int IdVenda { get; set; }

        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(100)]
        [Display(Name = "Nome do Cliente")]
        public string NomeCliente { get; set; }

        [Required(ErrorMessage = "O e-mail do cliente é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(100)]
        [Display(Name = "E-mail do Cliente")]
        public string EmailCliente { get; set; }

        [Required(ErrorMessage = "O telefone do cliente é obrigatório.")]
        [Phone(ErrorMessage = "Número de telefone inválido.")]
        [StringLength(20)]
        [Display(Name = "Telefone do Cliente")]
        public string TelefoneCliente { get; set; }

        [Required(ErrorMessage = "O tipo de pagamento é obrigatório.")]
        [StringLength(32)]
        [Display(Name = "Tipo de Pagamento")]
        public string TipoPagamento { get; set; }

        [Required(ErrorMessage = "É necessário associar um Curso.")]
        [Display(Name = "Curso")]
        public int IdCursoFK { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pendente";

        [Required]
        [StringLength(10)]
        [Display(Name = "Fatura")]
        public string Fatura { get; set; }

        [ValidateNever]
        public CursoModel Curso{ get; set; }

    }
}
