using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Ekutivala_EAD.Models;
using Ekutivala_EAD.Services;
using System.Text.Json;

namespace Ekutivala_EAD.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly ICursoService _cursoService;
    private List<CursoModel> _cursos => _cursoService.ObterTodosCursos();


    public HomeController(ILogger<HomeController> logger, ICursoService cursoService)
    {
        _logger = logger;
         _cursoService = cursoService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Cursos(string termoPesquisa = "", string tagFiltro = "", int pagina = 1)
    {
        try
        {
   
            var cursosFiltrados = _cursos.AsQueryable();

            // Filtro por termo de pesquisa
            if (!string.IsNullOrEmpty(termoPesquisa))
            {
                cursosFiltrados = cursosFiltrados.Where(c =>
                    c.Nome.Contains(termoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                    c.Descricao.Contains(termoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                    c.Tag.Contains(termoPesquisa, StringComparison.OrdinalIgnoreCase));
            }

            // Filtro por tag
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
                .Take(8)
                .ToList();

            // Paginação
            int cursosPorPagina = 8;
            int totalCursos = cursosFiltrados.Count();
            var totalPaginas = (int)Math.Ceiling(totalCursos / (double)cursosPorPagina);

            var cursosPaginados = cursosFiltrados
                .Skip((pagina - 1) * cursosPorPagina)
                .Take(cursosPorPagina)
                .ToList();

            var viewModel = new ListaCursosViewModel
            {
                Cursos = cursosPaginados,
                TagsPopulares = tagsPopulares,
                TermoPesquisa = termoPesquisa,
                TagSelecionada = tagFiltro,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas,
                TotalCursos = totalCursos
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            // Log do erro
            _logger.LogError(ex, "Erro ao carregar cursos");
            TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a lista de cursos.";
            return View(new ListaCursosViewModel());
        }
    }
    public IActionResult Prof01()
    {
        return View();
    }
    public IActionResult Prof02()
    {
        return View();
    }
    public IActionResult Prof03()
    {
        return View();
    }
    public IActionResult Prof04()
    {
        return View();
    }
    public IActionResult Prof05()
    {
        return View();
    }
    public IActionResult Prof06()
    {
        return View();
    }
    public IActionResult Prof07()
    {
        return View();
    }
    public IActionResult Prof09()
    {
        return View();
    }
    public IActionResult Prof10()
    {
        return View();
    }
    public IActionResult Prof11()
    {
        return View();
    }


    [HttpGet]
    public IActionResult Biblioteca(string searchTerm = "", string categoria = "", string formato = "", int page = 1)
    {
        // Verificação de sessão
        int? IdFunc = HttpContext.Session.GetInt32("idFunc");
        string NomeFunc = HttpContext.Session.GetString("nomeFunc");
        string EmailFunc = HttpContext.Session.GetString("emailFunc");
        string StatusFunc = HttpContext.Session.GetString("statusFunc");

        /*if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || StatusFunc.ToUpper() != "ATIVO")
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login_func", "Files1");
        }*/

        try
        {
            Repositorio repositorio = new Repositorio();

            // Obter todos os livros com filtros aplicados
            var livrosFiltrados = repositorio.ListarLivros()
                .Where(l =>
                    (string.IsNullOrEmpty(searchTerm) ||
                    l.Titulo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    l.Autor.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(categoria) || l.Categoria == categoria) &&
                    (string.IsNullOrEmpty(formato) || l.Formato == formato))
                .ToList();

            // Paginação
            int pageSize = 6;
            int totalItems = livrosFiltrados.Count;
            var pagedLivros = livrosFiltrados
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Obter categorias e formatos únicos para os filtros
            var categorias = repositorio.ListarLivros()
                .Select(l => l.Categoria)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var formatos = repositorio.ListarLivros()
                .Select(l => l.Formato)
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct()
                .OrderBy(f => f)
                .ToList();

            // Preencher o ViewModel
            var model = new ListaLivrosViewModel
            {
                Livros = pagedLivros,
                SearchTerm = searchTerm,
                CategoriaSelecionada = categoria,
                FormatoSelecionado = formato,
                Categorias = categorias,
                Formatos = formatos,
                PaginaAtual = page,
                TotalPaginas = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return View(model);
        }
        catch (Exception ex)
        {
            // Você pode logar o erro aqui se desejar
            TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a lista de livros.";
            return View(new ListaLivrosViewModel());
        }
    }

    public IActionResult BibliotecaFisica()
    {
        return View();
    }
    public IActionResult BibliotecaPDF()
    {
        return View();
    }

    [HttpGet]
    public IActionResult DetalhesLivro(int id)
    {

        try
        {
            Repositorio repositorio = new Repositorio();
            LivroModel livro = repositorio.ObterLivroPorId(id);

            if (livro == null)
            {
                TempData["ErrorMessage"] = "Livro não encontrado ou não está disponível.";
                return RedirectToAction("Biblioteca");
            }

            return View(livro);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar detalhes do livro: {ex.Message}");
            TempData["ErrorMessage"] = "Ocorreu um erro ao carregar os detalhes do livro.";
            return RedirectToAction("Biblioteca");
        }
    }

    [HttpGet]
    public IActionResult ComprarLivro(int idLivro)
    {
        // Verificação de sessão
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string telefone = HttpContext.Session.GetString("telefone");

        try
        {
            Repositorio repositorio = new Repositorio();
            var livro = repositorio.ObterLivroPorId(idLivro);

            if (livro == null || livro.Status?.ToUpper() != "ATIVO")
            {
                TempData["ErrorMessage"] = "Livro não disponível para compra.";
                return RedirectToAction("Biblioteca");
            }

            var model = new VendaLivroModel
            {
                IdLivroFK = idLivro,
                NomeCliente = nomeEstudante ?? "",
                EmailCliente = email ?? "",
                TelefoneCliente = telefone ?? "",
                Livro = livro
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a página de compra.";
            return RedirectToAction("Biblioteca");
        }

    }

    [HttpPost]
    public IActionResult ComprarLivro(VendaLivroModel venda)
    {
        Console.WriteLine("Iniciando processo de compra...");
        Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState inválido. Detalhes dos erros:");
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Erro em '{key}': {error.ErrorMessage}");
                }
            }
            Repositorio repositorio = new Repositorio();
            venda.Livro = repositorio.ObterLivroPorId(venda.IdLivroFK); return View(venda);

        }
        try
        {
            Console.WriteLine("ModelState válido. Dados recebidos:");
            Console.WriteLine($"IdLivroFK: {venda.IdLivroFK}");
            Console.WriteLine($"Nome: {venda.NomeCliente}");
            Console.WriteLine($"Email: {venda.EmailCliente}");
            Console.WriteLine($"Telefone: {venda.TelefoneCliente}");
            Console.WriteLine($"TipoPagamento: {venda.TipoPagamento}");
            Console.WriteLine($"Fatura: {venda.Fatura}");
            Repositorio repositorio = new Repositorio();
            int idVenda = repositorio.CadastrarVenda(venda);
            Console.WriteLine($"Resultado da inserção (idVenda): {idVenda}");
            if (idVenda > 0)
            {
                Console.WriteLine("Compra cadastrada com sucesso.");
                TempData["SuccessMessage"] = "Compra realizada com sucesso! Você receberá um e-mail com os detalhes.";
                return RedirectToAction("VendaLivroSucesso");
            }
            else
            {
                Console.WriteLine("Erro: CadastrarVenda retornou 0 ou valor negativo.");
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
            TempData["ErrorMessage"] = "Ocorreu um erro ao processar sua compra."; return View(venda);
        }
    }


    [HttpGet]
    public IActionResult PagarReferencia(string fatura, string nome, string email, string telefone, int livroId)
    {

        // Adicione logs para depuração
        Console.WriteLine($"Parâmetros recebidos: Fatura={fatura}, Nome={nome}, Email={email}, Telefone={telefone}, LivroId={livroId}");

        try
        {
            Repositorio repositorio = new Repositorio();
            var livro = repositorio.ObterLivroPorId(livroId);

            if (livro == null || livro.Status?.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Livro não disponível para compra.");
                return RedirectToAction("Biblioteca");
            }

            var model = new VendaLivroModel
            {
                Fatura = fatura,
                NomeCliente = nome,
                EmailCliente = email,
                TelefoneCliente = telefone,
                IdLivroFK = livroId,
                Livro = livro
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
    public IActionResult PagarExpress(string fatura, string nome, string email, string telefone, int livroId)
    {
        // Adicione logs para depuração
        Console.WriteLine($"Parâmetros recebidos: Fatura={fatura}, Nome={nome}, Email={email}, Telefone={telefone}, LivroId={livroId}");

        try
        {
            Repositorio repositorio = new Repositorio();
            var livro = repositorio.ObterLivroPorId(livroId);

            if (livro == null || livro.Status?.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Livro não disponível para compra.");
                return RedirectToAction("Biblioteca");
            }

            var model = new VendaLivroModel
            {
                Fatura = fatura,
                NomeCliente = nome,
                EmailCliente = email,
                TelefoneCliente = telefone,
                IdLivroFK = livroId,
                Livro = livro
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
    public IActionResult PagarTransferencia(string fatura, string nome, string email, string telefone, int livroId)
    {
        // Adicione logs para depuração
        Console.WriteLine($"Parâmetros recebidos: Fatura={fatura}, Nome={nome}, Email={email}, Telefone={telefone}, LivroId={livroId}");

        try
        {
            Repositorio repositorio = new Repositorio();
            var livro = repositorio.ObterLivroPorId(livroId);

            if (livro == null || livro.Status?.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Livro não disponível para compra.");
                return RedirectToAction("Biblioteca");
            }

            var model = new VendaLivroModel
            {
                Fatura = fatura,
                NomeCliente = nome,
                EmailCliente = email,
                TelefoneCliente = telefone,
                IdLivroFK = livroId,
                Livro = livro
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
    public IActionResult LivroGratuito(string fatura, string nome, string email, string telefone, int livroId)
    {
        // Adicione logs para depuração
        Console.WriteLine($"Parâmetros recebidos: Fatura={fatura}, Nome={nome}, Email={email}, Telefone={telefone}, LivroId={livroId}");

        try
        {
            Repositorio repositorio = new Repositorio();
            var livro = repositorio.ObterLivroPorId(livroId);

            if (livro == null || livro.Status?.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Livro não disponível para compra.");
                return RedirectToAction("Biblioteca");
            }

            var model = new VendaLivroModel
            {
                Fatura = fatura,
                NomeCliente = nome,
                EmailCliente = email,
                TelefoneCliente = telefone,
                IdLivroFK = livroId,
                Livro = livro
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
    public IActionResult VendaLivroSucesso()
    {

        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        int? idEstudante = HttpContext.Session.GetInt32("idEstudante");
        string nomeEstudante = HttpContext.Session.GetString("nome");
        string email = HttpContext.Session.GetString("email");
        string status = HttpContext.Session.GetString("status");

        Console.WriteLine($"Sessão Atual -> id: {idEstudante}, nome: {nomeEstudante}, email: {email}, status: {status}");
       
        // Verifica sessão e status
        if (idEstudante == null || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(status) || status.ToUpper() != "ATIVO")
        {
            Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
            HttpContext.Session.Clear();
            return View();
        }

        return RedirectToAction("Index", "Estudante");
    }

    [HttpPost]
    public IActionResult Login(EstudanteModel estudante)
    {
        Repositorio repositorio = new Repositorio();
        EstudanteModel estudanteLogado = repositorio.LoginEstudante(estudante); // Método que você deve implementar

        if (estudanteLogado != null)
        {
            // Armazenar dados essenciais na sessão
            HttpContext.Session.SetInt32("idEstudante", estudanteLogado.IdEstudante);
            HttpContext.Session.SetString("nome", estudanteLogado.Nome);
            HttpContext.Session.SetString("email", estudanteLogado.Email);
            HttpContext.Session.SetString("status", estudanteLogado.Status);

            // Apenas se já estiverem preenchidos
            HttpContext.Session.SetString("telefone", estudanteLogado.Telefone ?? "null");
            HttpContext.Session.SetString("cidade", estudanteLogado.Cidade ?? "null");
            HttpContext.Session.SetString("nacionalidade", estudanteLogado.Nacionalidade ?? "null");

            Console.WriteLine($"Login OK: {estudanteLogado.Email} / {estudanteLogado.Status}");

            return RedirectToAction("Index", "Estudante"); // Redireciona para o painel do estudante
        }
        else
        {
            ViewBag.NSucesso = "Login falhou! Email ou senha inválidos.";
            return View(estudante); // Retorna com os dados preenchidos
        }
    }


    [HttpGet]
    public IActionResult Cadastrar()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Cadastrar(EstudanteModel estudante)
    {
        Repositorio repositorio = new Repositorio();
        string resposta = repositorio.CadastrarEstudante(estudante);

        if (resposta == "Estudante cadastrado com sucesso!")
        {
            ViewBag.MensagemSucesso = resposta;
            return RedirectToAction("Login"); 
        }

        ViewBag.MensagemErro = resposta;
        return View(estudante); // Retorna a view preenchida com os dados e a mensagem de erro
    }

    public IActionResult Contactos()
    {
        return View();
    }
    public IActionResult Comissao()
    {
        return View();
    }
    public IActionResult Politicas()
    {
        return View();
    }
    public IActionResult Termo()
    {
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
