using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Ekutivala_EAD.Models
{
    public class ListaLivrosViewModel
    {
        public List<LivroModel> Livros { get; set; }
        public string SearchTerm { get; set; }
        public string CategoriaSelecionada { get; set; }
        public string FormatoSelecionado { get; set; }
        public List<string> Categorias { get; set; }
        public List<string> Formatos { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
    }
}