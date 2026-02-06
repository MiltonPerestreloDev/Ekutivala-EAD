using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Ekutivala_EAD.Models
{
    public class EstudanteModel
    {
        [Key]
        public int IdEstudante { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(255)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [StringLength(128)]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        [StringLength(255)]
        public string Senha { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "ATIVO"; // ATIVO, INATIVO, BLOQUEADO

        [Phone]
        [StringLength(20)]
        public string Telefone { get; set; }

        [StringLength(255)]
        public string Cidade { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        [StringLength(50)]
        public string Nacionalidade { get; set; }

        [StringLength(32)]
        public string Pais { get; set; }

        [StringLength(64)]
        public string Provincia { get; set; }

        [StringLength(128)]
        public string NivelAcademico { get; set; }

        [StringLength(255)]
        public string Instituicao { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}