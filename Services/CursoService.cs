using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using Ekutivala_EAD.Models;

namespace Ekutivala_EAD.Services
{
    public class CursoService : ICursoService
    {
        private readonly List<CursoModel> _cursos;

        public CursoService()
        {
            _cursos = CarregarCursosDoJson();
        }

        private List<CursoModel> CarregarCursosDoJson()
        {
            try
            {
                var caminhoArquivo = "Data/cursos.json";
                
                if (File.Exists(caminhoArquivo))
                {
                    var json = File.ReadAllText(caminhoArquivo);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var cursos = JsonSerializer.Deserialize<List<CursoModel>>(json, options);
                    
                    // Converter string de data para DateTime
                    foreach (var curso in cursos)
                    {
                        if (curso.DataPublicacao == default)
                        {
                            // Se não conseguir converter, mantém a data atual
                            curso.DataPublicacao = DateTime.Now;
                        }
                    }
                    
                    return cursos;
                }
                
                return new List<CursoModel>();
            }
            catch (Exception ex)
            {
                // Log do erro (você pode usar ILogger aqui)
                Console.WriteLine($"Erro ao carregar cursos: {ex.Message}");
                return new List<CursoModel>();
            }
        }

        public List<CursoModel> ObterTodosCursos()
        {
            return _cursos;
        }
    }
}