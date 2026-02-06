using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Ekutivala_EAD.Models
{
    public class ListaCursosViewModel
    {
        public List<CursoModel> Cursos { get; set; }
        public List<string> TagsPopulares { get; set; }
        public List<VendaCursoModel> VendasCursos { get; set; } = new List<VendaCursoModel>();
        public string TermoPesquisa { get; set; }
        public string TagSelecionada { get; set; } = "";
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalCursos { get; set; }
        public int CursosPorPagina { get; set; } = 8;
    }
}