using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ekutivala_EAD.Models;

namespace Ekutivala_EAD.Services
{
    public interface ICursoService
    {
        List<CursoModel> ObterTodosCursos();
    }
}