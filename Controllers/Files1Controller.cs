using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Diagnostics;
using Ekutivala_EAD.Models;
using Ekutivala_EAD.Services;

namespace Ekutivala_EAD.Controllers
{
    public class Files1Controller : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ICursoService _cursoService;
        private List<CursoModel> _cursos => _cursoService.ObterTodosCursos();

        public Files1Controller(IWebHostEnvironment hostingEnvironment, ICursoService cursoService)
        {
            _hostingEnvironment = hostingEnvironment;
            _cursoService = cursoService;
        }

        [HttpGet]
        public IActionResult Login_func()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login_func(FuncionarioModel funcionario)
        {
            Repositorio repositorio = new Repositorio();
            FuncionarioModel funcionarioLogado = repositorio.LoginFuncionario(funcionario);

            if (funcionarioLogado != null)
            {
                // Armazenar dados essenciais na sessão
                HttpContext.Session.SetInt32("idFunc", funcionarioLogado.IdFunc);
                HttpContext.Session.SetString("nomeFunc", funcionarioLogado.NomeFunc);
                HttpContext.Session.SetString("emailFunc", funcionarioLogado.EmailFunc);
                HttpContext.Session.SetString("statusFunc", funcionarioLogado.StatusFunc);
                HttpContext.Session.SetString("acessoFunc", funcionarioLogado.AcessoFunc ?? "null");

                Console.WriteLine($"Login OK: {funcionarioLogado.EmailFunc} / {funcionarioLogado.StatusFunc}");

                return RedirectToAction("Index", "Files1");

            }
            else
            {
                ViewBag.NSucesso = "Login falhou! Email ou senha inválidos.";
                return View(funcionario);
            }
        }

        [HttpGet]
        public IActionResult Index()
        {

            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
           

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}");
            
            // Verifica sessão e status
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || StatusFunc.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Index_Upload(string fileName = "")
        {
            FileClass fileObj = new FileClass();
            fileObj.Name = fileName;
            string path = Path.Combine(_hostingEnvironment.WebRootPath, "files");

            int nId = 1;

            foreach (string pdfPath in Directory.EnumerateFiles(path, "*.pdf"))
            {
                fileObj.Files.Add(new FileClass()
                {
                    FileId = nId++,
                    Name = Path.GetFileName(pdfPath),
                    Path = pdfPath
                });
            }

            return View(fileObj);
        }


        [HttpPost]
        public IActionResult Index_Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "files");
                // Gera um nome de arquivo único para evitar colisões
                string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }

                // Armazena o caminho e os Nome do arquivo na TempData
                TempData["FilePath"] = filePath;
                TempData["FileName"] = fileName;
            }

            return RedirectToAction("Arquivo", "Home");

        }

        //************************ ARQUIVO ***********************************

        [HttpGet]
        public IActionResult UploadCapa()
        {
            // Limpa os dados temporários do upload anterior
            TempData.Remove("CapaPath");
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");

            // Verificação de sessão (mantida igual)
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
        
            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}");
            
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || StatusFunc.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCapa(IFormFile file)
        {
            // Limpa mensagens anteriores
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");

            if (file != null && file.Length > 0)
            {
                // Verifica se é uma imagem
                var supportedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!supportedTypes.Contains(file.ContentType))
                {
                    TempData["ErrorMessage"] = "Tipo de arquivo não suportado. Por favor, envie apenas imagens (JPEG, PNG, GIF).";
                    return View();
                }

                // Limita o tamanho da imagem (5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "A imagem é muito grande. Tamanho máximo permitido: 5MB.";
                    return View();
                }

                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "covers");
                
                // Cria o diretório se não existir
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Gera um nome de arquivo único
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                string relativePath = Path.Combine("uploads", "covers", uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Armazena o caminho na TempData para usar no formulário de cadastro
                TempData["CapaPath"] = relativePath;
                TempData["SuccessMessage"] = "Capa enviada com sucesso!";

                return RedirectToAction("Cadastrar_Arquivo");
            }

            TempData["ErrorMessage"] = "Por favor, selecione uma imagem para upload.";
            return View();
        }


        [HttpGet]
        public IActionResult Cadastrar_Arquivo()
        {
             int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
           

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}");
            
            // Verifica sessão e status
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || StatusFunc.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            // Verifica se já tem uma capa enviada
            if (TempData["CapaPath"] != null)
            {
                ViewBag.CapaPath = TempData["CapaPath"];
                // Mantém o valor para o POST também
                TempData.Keep("CapaPath");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar_Arquivo(LivroModel livro)
        {
            if (ModelState.IsValid)
            {
                if (TempData["CapaPath"] != null)
                {
                    livro.ImagemCaminho = TempData["CapaPath"].ToString();
                }

                Repositorio repositorio = new Repositorio();
                string resultado = repositorio.CadastrarLivro(livro);

                if (resultado.Contains("sucesso"))
                {
                    TempData["SuccessMessage"] = resultado;
                    return RedirectToAction("Listar_Arquivos");
                }

                ModelState.AddModelError("", resultado);
            }

            // Se houver erro, manter imagem
            if (TempData["CapaPath"] != null)
            {
                ViewBag.CapaPath = TempData["CapaPath"];
                TempData.Keep("CapaPath");
            }

            return View(livro);
        }


        [HttpGet]
        public IActionResult Listar_Arquivos()
        {
            // Verificação de sessão (mantida igual)
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
        
            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}");
            
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || StatusFunc.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                List<LivroModel> livros = repositorio.ListarTodosLivros();
                return View(livros);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao listar arquivos: {ex.Message}");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a lista de livros.";
                return View(new List<LivroModel>());
            }
        }

        [HttpGet]
        public IActionResult Editar_Arquivo(int id)
        {
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}");

            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || StatusFunc.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                LivroModel livro = repositorio.ObterLivroPorId(id);

                if (livro == null)
                {
                    TempData["ErrorMessage"] = "Livro não encontrado.";
                    return RedirectToAction("Listar_Arquivos", "Files1");
                }

                return View(livro);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar os dados do livro.";
                return RedirectToAction("Listar_Arquivos", "Files1");
            }
        }


        [HttpPost]
        public IActionResult Editar_Arquivo(LivroModel livro)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Repositorio repositorio = new Repositorio();
                    repositorio.AtualizarLivro(livro);

                    TempData["SuccessMessage"] = "Livro atualizado com sucesso!";
                    return RedirectToAction("Listar_Arquivos");
                }

                TempData["ErrorMessage"] = "Dados inválidos. Verifique os campos e tente novamente.";
                return View(livro);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao atualizar o livro.";
                return View(livro);
            }
        }

        [HttpGet]
        public IActionResult Excluir_Arquivo(int id)
        {
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");

            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || StatusFunc.ToUpper() != "ATIVO")
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

           try
            {
                Repositorio repositorio = new Repositorio();
                bool resultado = repositorio.InativarLivro(id);

                if (resultado)
                    TempData["SuccessMessage"] = "Livro inativado com sucesso.";
                else
                    TempData["ErrorMessage"] = "Não foi possível inativar o livro. Verifique o ID ou o estado atual.";

                return RedirectToAction("Listar_Arquivos");
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Erro ao tentar inativar o livro.";
                return RedirectToAction("Listar_Arquivos");
            }

        }

        //************************ FUNCIONARIO ***********************************

        [HttpGet]
        public IActionResult Cadastrar_Funcionario()
        {
             int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
           

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}");
            
            // Verifica sessão e status
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || StatusFunc.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar_Funcionario(FuncionarioModel funcionario)
        {
            if (!ModelState.IsValid)
            {
                return View(funcionario);
            }

            Repositorio repositorio = new Repositorio();
            string resposta = repositorio.CadastrarFuncionario(funcionario);

            if (resposta == "Funcionário cadastrado com sucesso!")
            {
                TempData["MensagemSucesso"] = resposta;
                return RedirectToAction("Listar_Funcionarios"); // Redireciona para a lista de funcionários
            }

            ViewBag.MensagemErro = resposta;
            return View(funcionario); // Retorna a view preenchida com os dados e a mensagem de erro
        }

        [HttpGet]
        public IActionResult Listar_Funcionarios()
        {
            // Verificação de sessão
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}");
            
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || StatusFunc.ToUpper() != "ATIVO")
            {
                Console.WriteLine("Redirecionando: Sessão inválida ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                List<FuncionarioModel> funcionarios = repositorio.ListarTodosFuncionarios();
                return View(funcionarios);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao listar funcionários: {ex.Message}");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a lista de funcionários.";
                return View(new List<FuncionarioModel>());
            }
        }

        [HttpGet]
        public IActionResult Editar_Funcionario(int id)
        {
            // Verificação de sessão (mantido igual)
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
            string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}, acesso: {AcessoFunc}");

            // Verificar se usuário tem acesso de administrador
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || 
                StatusFunc.ToUpper() != "ATIVO" || AcessoFunc != "A")
            {
                Console.WriteLine("Redirecionando: Acesso negado ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                FuncionarioModel funcionario = repositorio.ObterFuncionarioPorId(id);

                if (funcionario == null)
                {
                    TempData["ErrorMessage"] = "Funcionário não encontrado.";
                    return RedirectToAction("Listar_Funcionarios", "Files1");
                }

                return View(funcionario);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar funcionário: {ex.Message}");
                TempData["ErrorMessage"] = "Erro ao carregar os dados do funcionário.";
                return RedirectToAction("Listar_Funcionarios", "Files1");
            }
        }

        [HttpPost]
        public IActionResult Editar_Funcionario(FuncionarioModel funcionario)
        {
            try
            {
                Console.WriteLine("Iniciando edição de funcionário...");

                // Verificar sessão novamente no POST
                int? IdFunc = HttpContext.Session.GetInt32("idFunc");
                string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

                Console.WriteLine($"Sessão -> IdFunc: {IdFunc}, AcessoFunc: {AcessoFunc}");

                if (IdFunc == null || AcessoFunc != "A")
                {
                    TempData["ErrorMessage"] = "Acesso negado.";
                    return RedirectToAction("Login_func", "Files1");
                }

                // 🔥 REMOVER ERROS DE VALIDAÇÃO DOS CAMPOS HIDDEN
                ModelState.Remove("SenhaFunc");
                ModelState.Remove("DataCadastroFunc");
                
                // 🔥 VALIDAR APENAS OS CAMPOS EDITÁVEIS
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState inválido. Erros:");
                    foreach (var error in ModelState)
                    {
                        if (error.Value.Errors.Count > 0)
                        {
                            Console.WriteLine($"Campo: {error.Key}, Erro: {error.Value.Errors[0].ErrorMessage}");
                        }
                    }
                    
                    TempData["ErrorMessage"] = "Dados inválidos. Verifique os campos.";
                    
                    // Recarregar os dados do funcionário para mostrar a view corretamente
                    Repositorio repositorioTemp = new Repositorio();
                    var funcionarioCompleto = repositorioTemp.ObterFuncionarioPorId(funcionario.IdFunc);
                    return View(funcionarioCompleto);
                }

                Repositorio repositorio = new Repositorio();
                var funcionarioExistente = repositorio.ObterFuncionarioPorId(funcionario.IdFunc);

                if (funcionarioExistente == null)
                {
                    TempData["ErrorMessage"] = "Funcionário não encontrado.";
                    return RedirectToAction("Listar_Funcionarios", "Files1");
                }

                // 🔥 VERIFICAR SE O USUÁRIO ESTÁ EDITANDO A SI MESMO
                bool isEditingSelf = IdFunc == funcionario.IdFunc;
                string statusOriginal = funcionarioExistente.StatusFunc;
                string statusNovo = funcionario.StatusFunc;

                // 🔥 ATUALIZAR APENAS OS CAMPOS PERMITIDOS
                funcionarioExistente.NomeFunc = funcionario.NomeFunc;
                funcionarioExistente.EmailFunc = funcionario.EmailFunc;
                funcionarioExistente.StatusFunc = funcionario.StatusFunc;
                funcionarioExistente.AcessoFunc = funcionario.AcessoFunc;

                // Salvar no banco
                string resultado = repositorio.AtualizarFuncionario(funcionarioExistente);

                if (resultado.Contains("sucesso"))
                {
                    // 🔥 VERIFICAR SE O USUÁRIO EDITOU A SI MESMO E ALTEROU O STATUS DE "ATIVO"
                    if (isEditingSelf && statusOriginal == "ATIVO" && statusNovo != "ATIVO")
                    {
                        Console.WriteLine($"Usuário {IdFunc} alterou seu próprio status de ATIVO para {statusNovo}. Fazendo logout...");
                        
                        TempData["SuccessMessage"] = "Funcionário atualizado com sucesso! Seu status foi alterado, faça login novamente.";
                        HttpContext.Session.Clear();
                        return RedirectToAction("Login_func", "Files1");
                    }

                    TempData["SuccessMessage"] = "Funcionário atualizado com sucesso!";
                    return RedirectToAction("Listar_Funcionarios", "Files1");
                }
                else
                {
                    TempData["ErrorMessage"] = resultado;
                    
                    // Recarregar os dados para mostrar a view
                    var funcionarioRecarregado = repositorio.ObterFuncionarioPorId(funcionario.IdFunc);
                    return View(funcionarioRecarregado);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar funcionário: {ex}");
                TempData["ErrorMessage"] = "Erro ao atualizar o funcionário.";
                
                // Recarregar os dados para mostrar a view
                Repositorio repositorio = new Repositorio();
                var funcionarioRecarregado = repositorio.ObterFuncionarioPorId(funcionario.IdFunc);
                return View(funcionarioRecarregado);
            }
        }

        [HttpGet]
        public IActionResult Listar_Estudantes()
        {
            // Verificação de sessão
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
            string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}, acesso: {AcessoFunc}");

            // Verificar se usuário tem acesso
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || 
                StatusFunc.ToUpper() != "ATIVO" || AcessoFunc != "A")
            {
                Console.WriteLine("Redirecionando: Acesso negado ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                List<EstudanteModel> estudantes = repositorio.ListarEstudantes();

                return View(estudantes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao listar estudantes: {ex.Message}");
                TempData["ErrorMessage"] = "Erro ao carregar a lista de estudantes.";
                return View(new List<EstudanteModel>());
            }
        }

        [HttpGet]
        public IActionResult Editar_Estudante(int id)
        {
            // Verificação de sessão
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
            string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}, acesso: {AcessoFunc}");

            // Verificar se usuário tem acesso de administrador
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || 
                StatusFunc.ToUpper() != "ATIVO" || AcessoFunc != "A")
            {
                Console.WriteLine("Redirecionando: Acesso negado ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                EstudanteModel estudante = repositorio.ObterEstudantePorId(id);

                if (estudante == null)
                {
                    TempData["ErrorMessage"] = "Estudante não encontrado.";
                    return RedirectToAction("Listar_Estudantes", "Files1");
                }

                // Criar ViewModel apenas com campos editáveis
                var estudanteEditViewModel = new EstudanteModel
                {
                    IdEstudante = estudante.IdEstudante,
                    Nome = estudante.Nome,
                    Email = estudante.Email,
                    Status = estudante.Status,
                    // Outros campos apenas para exibição
                    Telefone = estudante.Telefone,
                    Cidade = estudante.Cidade,
                    DataNascimento = estudante.DataNascimento,
                    Nacionalidade = estudante.Nacionalidade,
                    DataCadastro = estudante.DataCadastro
                };

                return View(estudanteEditViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar estudante: {ex.Message}");
                TempData["ErrorMessage"] = "Erro ao carregar os dados do estudante.";
                return RedirectToAction("Listar_Estudantes", "Files1");
            }
        }

        [HttpPost]
        public IActionResult Editar_Estudante(EstudanteModel estudante)
        {
            try
            {
                Console.WriteLine("Iniciando edição de estudante...");

                // Verificar sessão novamente no POST
                int? IdFunc = HttpContext.Session.GetInt32("idFunc");
                string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

                Console.WriteLine($"Sessão -> IdFunc: {IdFunc}, AcessoFunc: {AcessoFunc}");

                if (IdFunc == null || AcessoFunc != "A")
                {
                    TempData["ErrorMessage"] = "Acesso negado.";
                    return RedirectToAction("Login_func", "Files1");
                }

                // 🔥 REMOVER ERROS DE VALIDAÇÃO DE TODOS OS CAMPOS EXCETO STATUS
                // Como os campos estão como hidden, precisamos limpar o ModelState
                ModelState.Clear(); // Limpa todos os erros
                
                // 🔥 ADICIONAR VALIDAÇÃO APENAS PARA O CAMPO STATUS
                if (string.IsNullOrEmpty(estudante.Status))
                {
                    ModelState.AddModelError("Status", "Status é obrigatório");
                }

                // 🔥 VALIDAR APENAS O CAMPO STATUS
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState inválido para o campo Status.");
                    
                    // Recarregar os dados completos do estudante
                    Repositorio repositorioTemp = new Repositorio();
                    var estudanteCompleto = repositorioTemp.ObterEstudantePorId(estudante.IdEstudante);
                    
                    TempData["ErrorMessage"] = "Selecione um status válido.";
                    return View(estudanteCompleto);
                }

                Repositorio repositorio = new Repositorio();
                var estudanteExistente = repositorio.ObterEstudantePorId(estudante.IdEstudante);

                if (estudanteExistente == null)
                {
                    TempData["ErrorMessage"] = "Estudante não encontrado.";
                    return RedirectToAction("Listar_Estudantes", "Files1");
                }

                // 🔥 ATUALIZAR APENAS O STATUS
                estudanteExistente.Status = estudante.Status;

                // Salvar no banco
                string resultado = repositorio.AtualizarEstudante(estudanteExistente);

                if (resultado.Contains("sucesso"))
                {
                    TempData["SuccessMessage"] = "Status do estudante atualizado com sucesso!";
                    return RedirectToAction("Listar_Estudantes", "Files1");
                }
                else
                {
                    TempData["ErrorMessage"] = resultado;
                    
                    // Recarregar os dados completos
                    var estudanteRecarregado = repositorio.ObterEstudantePorId(estudante.IdEstudante);
                    return View(estudanteRecarregado);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar estudante: {ex}");
                TempData["ErrorMessage"] = "Erro ao atualizar o estudante.";
                
                // Recarregar os dados completos
                Repositorio repositorio = new Repositorio();
                var estudanteRecarregado = repositorio.ObterEstudantePorId(estudante.IdEstudante);
                return View(estudanteRecarregado);
            }
        }

        [HttpGet]
        public IActionResult Listar_VendasCursos()
        {
            // Verificação de sessão
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
            string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}, acesso: {AcessoFunc}");

            // Verificar se usuário tem acesso
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || 
                StatusFunc.ToUpper() != "ATIVO" || AcessoFunc != "A")
            {
                Console.WriteLine("Redirecionando: Acesso negado ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                List<VendaCursoModel> vendas = repositorio.ListarVendasCursos();

                // Verificar se a lista de vendas não é nula
                if (vendas != null)
                {
                    // Buscar os cursos REAIS no JSON e associar às vendas
                    foreach (var venda in vendas)
                    {
                        if (venda != null)
                        {
                            venda.Curso = _cursos?.FirstOrDefault(c => c != null && c.Id == venda.IdCursoFK);
                        }
                    }
                }
                else
                {
                    vendas = new List<VendaCursoModel>();
                }

                return View(vendas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao listar vendas de cursos: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Erro ao carregar a lista de vendas.";
                return View(new List<VendaCursoModel>());
            }
        }

        [HttpGet]
        public IActionResult Editar_VendaCurso(int id)
        {
            // Verificação de sessão
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

            if (IdFunc == null || AcessoFunc != "A")
            {
                TempData["ErrorMessage"] = "Acesso negado.";
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                VendaCursoModel venda = repositorio.ObterVendaCursoPorId(id);

                if (venda == null)
                {
                    TempData["ErrorMessage"] = "Venda não encontrada.";
                    return RedirectToAction("Listar_VendasCursos", "Files1");
                }

                // Buscar o curso REAL no JSON
                venda.Curso = _cursoService?.ObterTodosCursos()?.FirstOrDefault(c => c != null && c.Id == venda.IdCursoFK);

                return View(venda);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar venda para edição: {ex.Message}");
                TempData["ErrorMessage"] = "Erro ao carregar os dados da venda.";
                return RedirectToAction("Listar_VendasCursos", "Files1");
            }
        }

        [HttpPost]
        public IActionResult Editar_VendaCurso(VendaCursoModel venda)
        {
            try
            {
                Console.WriteLine("Iniciando edição de venda de curso...");

                // Verificar sessão novamente no POST
                int? IdFunc = HttpContext.Session.GetInt32("idFunc");
                string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

                Console.WriteLine($"Sessão -> IdFunc: {IdFunc}, AcessoFunc: {AcessoFunc}");

                if (IdFunc == null || AcessoFunc != "A")
                {
                    TempData["ErrorMessage"] = "Acesso negado.";
                    return RedirectToAction("Login_func", "Files1");
                }

                // 🔥 REMOVER ERROS DE VALIDAÇÃO DE TODOS OS CAMPOS EXCETO STATUS
                ModelState.Clear(); // Limpa todos os erros
                
                // 🔥 ADICIONAR VALIDAÇÃO APENAS PARA O CAMPO STATUS
                if (string.IsNullOrEmpty(venda.Status))
                {
                    ModelState.AddModelError("Status", "Status é obrigatório");
                }

                // 🔥 VALIDAR APENAS O CAMPO STATUS
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState inválido para o campo Status.");
                    
                    // Recarregar os dados completos da venda
                    Repositorio repositorioTemp = new Repositorio();
                    var vendaCompleta = repositorioTemp.ObterVendaCursoPorId(venda.IdVenda);
                    
                    // Buscar o curso
                    vendaCompleta.Curso = _cursoService?.ObterTodosCursos()?.FirstOrDefault(c => c != null && c.Id == vendaCompleta.IdCursoFK);
                    
                    TempData["ErrorMessage"] = "Selecione um status válido.";
                    return View(vendaCompleta);
                }

                Repositorio repositorio = new Repositorio();
                var vendaExistente = repositorio.ObterVendaCursoPorId(venda.IdVenda);

                if (vendaExistente == null)
                {
                    TempData["ErrorMessage"] = "Venda não encontrada.";
                    return RedirectToAction("Listar_VendasCursos", "Files1");
                }

                // 🔥 ATUALIZAR APENAS O STATUS
                vendaExistente.Status = venda.Status;

                // Salvar no banco
                string resultado = repositorio.AtualizarVendaCurso(vendaExistente);

                if (resultado.Contains("sucesso"))
                {
                    TempData["SuccessMessage"] = "Status da venda atualizado com sucesso!";
                    return RedirectToAction("Listar_VendasCursos", "Files1");
                }
                else
                {
                    TempData["ErrorMessage"] = resultado;
                    
                    // Recarregar os dados completos
                    var vendaRecarregada = repositorio.ObterVendaCursoPorId(venda.IdVenda);
                    vendaRecarregada.Curso = _cursoService?.ObterTodosCursos()?.FirstOrDefault(c => c != null && c.Id == vendaRecarregada.IdCursoFK);
                    return View(vendaRecarregada);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar venda: {ex}");
                TempData["ErrorMessage"] = "Erro ao atualizar a venda.";
                
                // Recarregar os dados completos
                Repositorio repositorio = new Repositorio();
                var vendaRecarregada = repositorio.ObterVendaCursoPorId(venda.IdVenda);
                vendaRecarregada.Curso = _cursoService?.ObterTodosCursos()?.FirstOrDefault(c => c != null && c.Id == vendaRecarregada.IdCursoFK);
                return View(vendaRecarregada);
            }
        }

        [HttpGet]
        public IActionResult Listar_VendasLivros()
        {
            // Verificação de sessão
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string NomeFunc = HttpContext.Session.GetString("nomeFunc");
            string EmailFunc = HttpContext.Session.GetString("emailFunc");
            string StatusFunc = HttpContext.Session.GetString("statusFunc");
            string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

            Console.WriteLine($"Sessão Atual -> id: {IdFunc}, nome: {NomeFunc}, email: {EmailFunc}, status: {StatusFunc}, acesso: {AcessoFunc}");

            // Verificar se usuário tem acesso
            if (IdFunc == null || string.IsNullOrEmpty(EmailFunc) || string.IsNullOrEmpty(StatusFunc) || 
                StatusFunc.ToUpper() != "ATIVO" || AcessoFunc != "A")
            {
                Console.WriteLine("Redirecionando: Acesso negado ou usuário inativo.");
                HttpContext.Session.Clear();
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                List<VendaLivroModel> vendas = repositorio.ListarVendasLivros();

                return View(vendas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao listar vendas de livros: {ex.Message}");
                TempData["ErrorMessage"] = "Erro ao carregar a lista de vendas de livros.";
                return View(new List<VendaLivroModel>());
            }
        }

        [HttpGet]
        public IActionResult Editar_VendaLivro(int id)
        {
            // Verificação de sessão
            int? IdFunc = HttpContext.Session.GetInt32("idFunc");
            string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

            if (IdFunc == null || AcessoFunc != "A")
            {
                TempData["ErrorMessage"] = "Acesso negado.";
                return RedirectToAction("Login_func", "Files1");
            }

            try
            {
                Repositorio repositorio = new Repositorio();
                VendaLivroModel venda = repositorio.ObterVendaLivroPorId(id);

                if (venda == null)
                {
                    TempData["ErrorMessage"] = "Venda não encontrada.";
                    return RedirectToAction("Listar_VendasLivros", "Files1");
                }

                return View(venda);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar venda para edição: {ex.Message}");
                TempData["ErrorMessage"] = "Erro ao carregar os dados da venda.";
                return RedirectToAction("Listar_VendasLivros", "Files1");
            }
        }

        [HttpPost]
        public IActionResult Editar_VendaLivro(VendaLivroModel venda)
        {
            try
            {
                Console.WriteLine("Iniciando edição de venda de livro...");

                // Verificar sessão novamente no POST
                int? IdFunc = HttpContext.Session.GetInt32("idFunc");
                string AcessoFunc = HttpContext.Session.GetString("acessoFunc");

                Console.WriteLine($"Sessão -> IdFunc: {IdFunc}, AcessoFunc: {AcessoFunc}");

                if (IdFunc == null || AcessoFunc != "A")
                {
                    TempData["ErrorMessage"] = "Acesso negado.";
                    return RedirectToAction("Login_func", "Files1");
                }

                // 🔥 REMOVER ERROS DE VALIDAÇÃO DE TODOS OS CAMPOS EXCETO STATUS
                ModelState.Clear(); // Limpa todos os erros
                
                // 🔥 ADICIONAR VALIDAÇÃO APENAS PARA O CAMPO STATUS
                if (string.IsNullOrEmpty(venda.Status))
                {
                    ModelState.AddModelError("Status", "Status é obrigatório");
                }

                // 🔥 VALIDAR APENAS O CAMPO STATUS
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState inválido para o campo Status.");
                    
                    // Recarregar os dados completos da venda
                    Repositorio repositorioTemp = new Repositorio();
                    var vendaCompleta = repositorioTemp.ObterVendaLivroPorId(venda.IdVenda);
                    
                    TempData["ErrorMessage"] = "Selecione um status válido.";
                    return View(vendaCompleta);
                }

                Repositorio repositorio = new Repositorio();
                var vendaExistente = repositorio.ObterVendaLivroPorId(venda.IdVenda);

                if (vendaExistente == null)
                {
                    TempData["ErrorMessage"] = "Venda não encontrada.";
                    return RedirectToAction("Listar_VendasLivros", "Files1");
                }

                // 🔥 ATUALIZAR APENAS O STATUS
                vendaExistente.Status = venda.Status;

                // Salvar no banco
                string resultado = repositorio.AtualizarVendaLivro(vendaExistente);

                if (resultado.Contains("sucesso"))
                {
                    TempData["SuccessMessage"] = "Status da venda atualizado com sucesso!";
                    return RedirectToAction("Listar_VendasLivros", "Files1");
                }
                else
                {
                    TempData["ErrorMessage"] = resultado;
                    
                    // Recarregar os dados completos
                    var vendaRecarregada = repositorio.ObterVendaLivroPorId(venda.IdVenda);
                    return View(vendaRecarregada);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar venda: {ex}");
                TempData["ErrorMessage"] = "Erro ao atualizar a venda.";
                
                // Recarregar os dados completos
                Repositorio repositorio = new Repositorio();
                var vendaRecarregada = repositorio.ObterVendaLivroPorId(venda.IdVenda);
                return View(vendaRecarregada);
            }
        }
       
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login_func", "Files1");
        }




        
    }
}
