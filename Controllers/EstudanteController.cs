using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Ekutivala_EAD.Models;
using Ekutivala_EAD.Services;
//using Ekutivala_EAD.ViewModels;

namespace Ekutivala_EAD.Controllers;

public class EstudanteController : Controller
{
    private readonly ILogger<EstudanteController> _logger;
    private readonly ICursoService _cursoService;
    private List<CursoModel> _cursos => _cursoService.ObterTodosCursos(); 

    public EstudanteController(ILogger<EstudanteController> logger, ICursoService cursoService)
    {
        _logger = logger;
        _cursoService = cursoService;
    }
   
    [HttpGet]
    public IActionResult Index(string termoPesquisa = null, int pagina = 1, string tagFiltro = null)
    {
        Console.WriteLine("Verificando sessão do estudante...");

        // Verificação da sessão (mantida do seu código original)
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string status = HttpContext.Session.GetString("status");

        Console.WriteLine($"Sessão Atual -> idEstudante: {idEstudante}, nome: {nomeEstudante}, email: {email}, status: {status}");

        if (idEstudante == null || string.IsNullOrEmpty(email))
        {
            Console.WriteLine("Redirecionando para Login -> Sessão inválida ou inexistente.");
            return RedirectToAction("Login", "Home");
        }

        if (string.IsNullOrEmpty(status) || status.ToUpper() != "ATIVO")
        {
            Console.WriteLine("Redirecionando para Login -> Estudante não está ATIVO.");
            return RedirectToAction("Login", "Home");
        }

        Console.WriteLine("Sessão válida. Acesso autorizado ao painel do estudante.");

         var cursosFiltrados = _cursos.AsQueryable();

        // Filtro por termo de pesquisa (mantido)
        if (!string.IsNullOrEmpty(termoPesquisa))
        {
            cursosFiltrados = cursosFiltrados.Where(c =>
                c.Nome.Contains(termoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                c.Descricao.Contains(termoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                c.Tag.Contains(termoPesquisa, StringComparison.OrdinalIgnoreCase));
        }

        // Novo filtro por tag
        if (!string.IsNullOrEmpty(tagFiltro))
        {
            cursosFiltrados = cursosFiltrados.Where(c => 
                c.Tag.Equals(tagFiltro, StringComparison.OrdinalIgnoreCase));
        }

        // Extrair tags únicas e ordenar por popularidade
        var tagsPopulares = _cursos
            .GroupBy(c => c.Tag)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(8) // Limita a 8 tags mais populares
            .ToList();

        var totalCursos = cursosFiltrados.Count();
        var cursosPorPagina = 8;
        var totalPaginas = (int)Math.Ceiling(totalCursos / (double)cursosPorPagina);

        

        var cursosPaginados = cursosFiltrados
            .Skip((pagina - 1) * cursosPorPagina)
            .Take(cursosPorPagina)
            .ToList();

        var viewModel = new ListaCursosViewModel
        {
            Cursos = cursosPaginados,
            TagsPopulares = tagsPopulares, // Adiciona as tags ao ViewModel
            TermoPesquisa = termoPesquisa,
            PaginaAtual = pagina,
            TotalPaginas = totalPaginas,
            TotalCursos = totalCursos
        };

        return View(viewModel);
    }

    [HttpGet("perfil")]
    public IActionResult Perfil()
    {
        Console.WriteLine("Verificando sessão do estudante...");

        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string status = HttpContext.Session.GetString("status");

        ViewBag.NomeEstudante = nomeEstudante;

        Console.WriteLine($"Sessão Atual -> idEstudante: {idEstudante}, nome: {nomeEstudante}, email: {email}, status: {status}");

        // Verificação de sessão válida
        if (idEstudante == null || string.IsNullOrEmpty(email))
        {
            Console.WriteLine("Redirecionando para Login -> Sessão inválida ou inexistente.");
            return RedirectToAction("Login", "Home");
        }

        // Verificação de status ativo
        if (string.IsNullOrEmpty(status) || status.ToUpper() != "ATIVO")
        {
            Console.WriteLine("Redirecionando para Login -> Estudante não está ATIVO.");
            return RedirectToAction("Login", "Home");
        }

        // Buscar dados do estudante pelo ID
        Repositorio repositorio = new Repositorio();
        EstudanteModel estudante = repositorio.ObterEstudantePorId(idEstudante.Value);

        if (estudante == null)
        {
            Console.WriteLine("Estudante não encontrado no banco de dados.");
            ViewBag.MensagemErro = "Estudante não encontrado.";
            return RedirectToAction("Index", "Home");
        }

        Console.WriteLine("Sessão válida. Carregando dados do estudante para exibir no perfil.");
        return View(estudante);
    }




    [HttpPost("perfil")]
    public IActionResult Perfil(EstudanteModel estudante)
    {
        Repositorio repositorio = new Repositorio();
        string resultado = repositorio.AtualizarEstudante(estudante);

        if (resultado == "Dados atualizados com sucesso")
        {
            ViewBag.MensagemSucesso = resultado;
        }
        else
        {
            ViewBag.MensagemErro = resultado;
        }

        return View(estudante); // retorna os dados atualizados
    }

    [HttpPost]
    public IActionResult EliminarEstudante(int idEstudante)
    {
        try
        {
            Repositorio repositorio = new Repositorio();
            
            // Primeiro atualiza o status para INATIVO
            string resultado = repositorio.EliminarEstudante(idEstudante);

            if (resultado.Contains("sucesso")) // ou outra condição de sucesso
            {
                // Limpa a sessão e faz logout
                HttpContext.Session.Clear();
                
                // Redireciona para a página de login
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ViewBag.MensagemErro = resultado;
                return RedirectToAction("Perfil"); // ou outra ação apropriada
            }
        }
        catch (Exception ex)
        {
            ViewBag.MensagemErro = "Ocorreu um erro ao desativar a conta: " + ex.Message;
            return RedirectToAction("Perfil");
        }
    }

    /* PAGINAS DOS CURSOS */

    [HttpGet]
    public IActionResult DetalhesCursos()
    {
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string status = HttpContext.Session.GetString("status");
        string telefone = HttpContext.Session.GetString("telefone");
        string cidade = HttpContext.Session.GetString("cidade");
        string nacionalidade = HttpContext.Session.GetString("nacionalidade");

        Console.WriteLine($"Sessão Atual -> id: {idEstudante}, nome: {nomeEstudante}, email: {email}, status: {status}");
        Console.WriteLine($"Verificando dados completos: Telefone = {telefone}, cidade = {cidade}, Nacionalidade = {nacionalidade}");

        // Verifica sessão e status
        if (idEstudante == null || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(status) || status.ToUpper() != "ATIVO")
        {
            Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        // Verifica se os dados opcionais estão preenchidos
        if (string.IsNullOrEmpty(telefone) || telefone == "null"
            || string.IsNullOrEmpty(cidade) || cidade == "null"
            || string.IsNullOrEmpty(nacionalidade) || nacionalidade == "null")
        {
            Console.WriteLine("Redirecionando: Perfil incompleto.");
            return RedirectToAction("Perfil", "Estudante");
        }

        ViewBag.NomeEstudante = nomeEstudante;

        return View();
    }

    [HttpGet]
    public IActionResult DetalhesCursos_InglesM01()
    {
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string status = HttpContext.Session.GetString("status");
        string telefone = HttpContext.Session.GetString("telefone");
        string cidade = HttpContext.Session.GetString("cidade");
        string nacionalidade = HttpContext.Session.GetString("nacionalidade");

        Console.WriteLine($"Sessão Atual -> id: {idEstudante}, nome: {nomeEstudante}, email: {email}, status: {status}");
        Console.WriteLine($"Verificando dados completos: Telefone = {telefone}, cidade = {cidade}, Nacionalidade = {nacionalidade}");

        // Verifica sessão e status
        if (idEstudante == null || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(status) || status.ToUpper() != "ATIVO")
        {
            Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        // Verifica se os dados opcionais estão preenchidos
        if (string.IsNullOrEmpty(telefone) || telefone == "null"
            || string.IsNullOrEmpty(cidade) || cidade == "null"
            || string.IsNullOrEmpty(nacionalidade) || nacionalidade == "null")
        {
            Console.WriteLine("Redirecionando: Perfil incompleto.");
            return RedirectToAction("Perfil", "Estudante");
        }

        // ID do curso de Inglês (ajuste conforme necessário)
        int idCursoIngles = 1;
        
        // Consulta se o estudante já comprou este curso
        Repositorio repositorio = new Repositorio();
        var vendaCurso = repositorio.ObterVendaPorEmailECurso(email, idCursoIngles);

        // Passa o status para a View
        ViewBag.NomeEstudante = nomeEstudante;
        ViewBag.StatusVenda = vendaCurso?.Status; // Pode ser "Pendente", "Aprovada", "Cancelada" ou null
        ViewBag.IdCurso = idCursoIngles;

        return View();
    }

    [HttpGet]
    public IActionResult DetalhesCursos_Cinematografia()
    {
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string status = HttpContext.Session.GetString("status");
        string telefone = HttpContext.Session.GetString("telefone");
        string cidade = HttpContext.Session.GetString("cidade");
        string nacionalidade = HttpContext.Session.GetString("nacionalidade");

        Console.WriteLine($"Sessão Atual -> id: {idEstudante}, nome: {nomeEstudante}, email: {email}, status: {status}");
        Console.WriteLine($"Verificando dados completos: Telefone = {telefone}, cidade = {cidade}, Nacionalidade = {nacionalidade}");

        // Verifica sessão e status
        if (idEstudante == null || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(status) || status.ToUpper() != "ATIVO")
        {
            Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        // Verifica se os dados opcionais estão preenchidos
        if (string.IsNullOrEmpty(telefone) || telefone == "null"
            || string.IsNullOrEmpty(cidade) || cidade == "null"
            || string.IsNullOrEmpty(nacionalidade) || nacionalidade == "null")
        {
            Console.WriteLine("Redirecionando: Perfil incompleto.");
            return RedirectToAction("Perfil", "Estudante");
        }

        // ID do curso de Inglês (ajuste conforme necessário)
        int idCurso = 13;
        
        // Consulta se o estudante já comprou este curso
        Repositorio repositorio = new Repositorio();
        var vendaCurso = repositorio.ObterVendaPorEmailECurso(email, idCurso);

        // Passa o status para a View
        ViewBag.NomeEstudante = nomeEstudante;
        ViewBag.StatusVenda = vendaCurso?.Status; 
        ViewBag.IdCurso = idCurso;

        return View();
    }
    
   [HttpGet]
    public IActionResult ComprarCurso(int idCurso)
    {
        // Verificação de sessão
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string telefone = HttpContext.Session.GetString("telefone");

        try
        {
           // Buscar o curso (do JSON ou onde você armazena)
            var curso = _cursos.FirstOrDefault(c => c.Id == idCurso);

            if (curso == null)
            {
                TempData["ErrorMessage"] = "Curso não encontrado.";
                return RedirectToAction("Index");
            }

            var model = new VendaCursoModel
            {
                IdCursoFK = idCurso,
                NomeCliente = nomeEstudante ?? "",
                EmailCliente = email ?? "",
                TelefoneCliente = telefone ?? "",
                Status = "PENDENTE",
                Curso = curso
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a página de compra.";
            return RedirectToAction("Cursos");
        }
    }


    [HttpPost]
    public IActionResult ComprarCurso(VendaCursoModel venda)
    {
        Console.WriteLine("Iniciando processo de compra de curso...");
        Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState inválido. Detalhes:");
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Erro em '{key}': {error.ErrorMessage}");
                }
            }

            Repositorio repositorio = new Repositorio();
            venda.Curso = _cursos.FirstOrDefault(c => c.Id == venda.IdCursoFK);
            return View(venda);
        }

        try
        {
            Console.WriteLine("ModelState válido. Dados recebidos:");
            Console.WriteLine($"IdCursoFK: {venda.IdCursoFK}");
            Console.WriteLine($"Nome: {venda.NomeCliente}");
            Console.WriteLine($"Email: {venda.EmailCliente}");
            Console.WriteLine($"Telefone: {venda.TelefoneCliente}");
            Console.WriteLine($"TipoPagamento: {venda.TipoPagamento}");
            Console.WriteLine($"Fatura: {venda.Fatura}");

            Repositorio repositorio = new Repositorio();
            int idVenda = repositorio.CadastrarVendaCurso(venda);

            Console.WriteLine($"Resultado da inserção (idVenda): {idVenda}");
            if (idVenda > 0)
            {
                Console.WriteLine("Compra de curso cadastrada com sucesso.");
                TempData["SuccessMessage"] = "Compra realizada com sucesso! Você receberá um e-mail com os detalhes.";
                return RedirectToAction("VendaCursoSucesso");
            }
            else
            {
                Console.WriteLine("Erro: CadastrarVendaCurso retornou 0 ou valor negativo.");
                TempData["ErrorMessage"] = "Não foi possível processar sua compra. Por favor, tente novamente.";
                return View(venda);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar compra: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Erro interno: {ex.InnerException.Message}");
            }

            TempData["ErrorMessage"] = "Ocorreu um erro ao processar sua compra.";
            return View(venda);
        }
    }





    [HttpGet]
    public IActionResult PagarExpress(string fatura, string nome, string email, string telefone, int cursoId)
    {
        // Adicione logs para depuração
        Console.WriteLine($"Parâmetros recebidos: Fatura={fatura}, Nome={nome}, Email={email}, Telefone={telefone}, cursoId={cursoId}");

        try
        {
            // Buscar o curso (do JSON ou onde você armazena)
            var curso = _cursos.FirstOrDefault(c => c.Id == cursoId);

            if (curso == null)
            {
                TempData["ErrorMessage"] = "Curso não encontrado.";
                return RedirectToAction("Index");
            }

            var model = new VendaCursoModel
            {
                Fatura = fatura,
                NomeCliente = nome,
                EmailCliente = email,
                TelefoneCliente = telefone,
                IdCursoFK = cursoId,
                Curso = curso
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a página de pagamento.";
            return RedirectToAction("Biblioteca");
        }
    }
    
    [HttpGet]
    public IActionResult PagarReferencia(string fatura, string nome, string email, string telefone, int cursoId)
    {
        // Adicione logs para depuração
        Console.WriteLine($"Parâmetros recebidos: Fatura={fatura}, Nome={nome}, Email={email}, Telefone={telefone}, cursoId={cursoId}");

        try
        {
            // Buscar o curso (do JSON ou onde você armazena)
            var curso = _cursos.FirstOrDefault(c => c.Id == cursoId);

            if (curso == null)
            {
                TempData["ErrorMessage"] = "Curso não encontrado.";
                return RedirectToAction("Index");
            }

            var model = new VendaCursoModel
            {
                Fatura = fatura,
                NomeCliente = nome,
                EmailCliente = email,
                TelefoneCliente = telefone,
                IdCursoFK = cursoId,
                Curso = curso
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a página de pagamento.";
            return RedirectToAction("Biblioteca");
        }
    }

    [HttpGet]
    public IActionResult PagarTransferencia(string fatura, string nome, string email, string telefone, int cursoId)
    {
        // Adicione logs para depuração
        Console.WriteLine($"Parâmetros recebidos: Fatura={fatura}, Nome={nome}, Email={email}, Telefone={telefone}, cursoId={cursoId}");

        try
        {
            // Buscar o curso (do JSON ou onde você armazena)
            var curso = _cursos.FirstOrDefault(c => c.Id == cursoId);

            if (curso == null)
            {
                TempData["ErrorMessage"] = "Curso não encontrado.";
                return RedirectToAction("Index");
            }

            var model = new VendaCursoModel
            {
                Fatura = fatura,
                NomeCliente = nome,
                EmailCliente = email,
                TelefoneCliente = telefone,
                IdCursoFK = cursoId,
                Curso = curso
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a página de pagamento.";
            return RedirectToAction("Biblioteca");
        }
    }
    [HttpGet]
    public IActionResult CursoGratuito(string fatura, string nome, string email, string telefone, int cursoId)
    {
        // Adicione logs para depuração
        Console.WriteLine($"Parâmetros recebidos: Fatura={fatura}, Nome={nome}, Email={email}, Telefone={telefone}, cursoId={cursoId}");

        try
        {
            // Buscar o curso (do JSON ou onde você armazena)
            var curso = _cursos.FirstOrDefault(c => c.Id == cursoId);

            if (curso == null)
            {
                TempData["ErrorMessage"] = "Curso não encontrado.";
                return RedirectToAction("Index");
            }

            var model = new VendaCursoModel
            {
                Fatura = fatura,
                NomeCliente = nome,
                EmailCliente = email,
                TelefoneCliente = telefone,
                IdCursoFK = cursoId,
                Curso = curso
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a página de pagamento.";
            return RedirectToAction("Biblioteca");
        }
    }

    [HttpGet]
    public IActionResult VendaCursoSucesso()
    {

        return View();
    }
    
    [HttpGet]
    public IActionResult ListaMeusCursos()
    {
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        if (idEstudante == null)
            return RedirectToAction("Login", "Home");

        Repositorio repositorio = new Repositorio();
        var vendas = repositorio.ObterCursosDoEstudante(idEstudante.Value);

        // buscar os cursos REAIS no JSON
        var cursos = vendas?
            .Select(v => _cursos.FirstOrDefault(c => c.Id == v.IdCursoFK))
            .Where(c => c != null)
            .ToList() ?? new List<CursoModel>();

        var model = new ListaCursosViewModel
        {
            Cursos = cursos,
            VendasCursos = vendas, // AGORA estamos passando as vendas também!
            TermoPesquisa = "",
            TagSelecionada = "",
            PaginaAtual = 1,
            TotalPaginas = 1,
            TotalCursos = cursos.Count,
            TagsPopulares = cursos
                .Where(c => !string.IsNullOrEmpty(c.Tag))
                .GroupBy(c => c.Tag)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(8)
                .ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult ListaBaseAulas()
    {
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string status = HttpContext.Session.GetString("status");
        string telefone = HttpContext.Session.GetString("telefone");
        string cidade = HttpContext.Session.GetString("cidade");
        string nacionalidade = HttpContext.Session.GetString("nacionalidade");

        Console.WriteLine($"Sessão Atual -> id: {idEstudante}, nome: {nomeEstudante}, email: {email}, status: {status}");
        Console.WriteLine($"Verificando dados completos: Telefone = {telefone}, cidade = {cidade}, Nacionalidade = {nacionalidade}");

        // Verifica sessão e status
        if (idEstudante == null || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(status) || status.ToUpper() != "ATIVO")
        {
            Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        // Verifica se os dados opcionais estão preenchidos
        if (string.IsNullOrEmpty(telefone) || telefone == "null"
            || string.IsNullOrEmpty(cidade) || cidade == "null"
            || string.IsNullOrEmpty(nacionalidade) || nacionalidade == "null")
        {
            Console.WriteLine("Redirecionando: Perfil incompleto.");
            return RedirectToAction("Perfil", "Estudante");
        }

        ViewBag.NomeEstudante = nomeEstudante;

        return View();
    }

    [HttpGet]
    public IActionResult ListaNotificacao()
    {
        Console.WriteLine("Verificando sessão do estudante...");

        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string status = HttpContext.Session.GetString("status");

        Console.WriteLine($"Sessão Atual -> idEstudante: {idEstudante}, nome: {nomeEstudante}, email: {email}, status: {status}");

        if (idEstudante == null || string.IsNullOrEmpty(email))
        {
            Console.WriteLine("Redirecionando para Login -> Sessão inválida ou inexistente.");
            return RedirectToAction("Login", "Home");
        }

        if (string.IsNullOrEmpty(status) || status.ToUpper() != "ATIVO")
        {
            Console.WriteLine("Redirecionando para Login -> Estudante não está ATIVO.");
            return RedirectToAction("Login", "Home");
        }

        Console.WriteLine("Sessão válida. Acesso autorizado ao painel do estudante.");

        Repositorio repositorio = new Repositorio();
        var notificacoes = repositorio.ListarNotificacoesNaoLidas(idEstudante.Value);

        return View(notificacoes);
    }


    [HttpPost]
    public IActionResult MarcarComoLida(int id)
    {
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");

        if (idEstudante == null)
        {
            return RedirectToAction("Login", "Home");
        }

        Repositorio repositorio = new Repositorio();
        bool sucesso = repositorio.MarcarNotificacaoComoLida(id, idEstudante.Value);

        if (sucesso)
        {
            ViewBag.MensagemSucesso = "Notificação marcada como lida!";
        }
        else
        {
            ViewBag.MensagemErro = "Erro ao marcar notificação.";
        }

        return RedirectToAction("ListaNotificacao");
    }






    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction("Login", "Home");
    }

    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
