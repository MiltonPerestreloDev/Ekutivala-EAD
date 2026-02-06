using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Http;
using Ekutivala_EAD.Models;


namespace Ekutivala_EAD.Models
{
    public class Repositorio
    {
        /*ENDEREÇO DE CONEXÃO*/
        private const string enderecoConexao = "Database = dbekutivala; Datasource = localhost; Port = 3306; Username = root; Password =";
        //private const  string enderecoConexao = "Server=MYSQL5049.site4now.net; Database=db_abd9a5_dbekuti; Uid=abd9a5_dbekuti; Pwd=Semana99##";
        // UserBaseSenhas: teste123
        /****************************************ESTUDANTE*******************************************/
        /*CADASTRARO DE ESTUDANTE*/
        public string CadastrarEstudante(EstudanteModel estudante)
        {
            string mensagem = string.Empty;

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    // Verificar se o email já está cadastrado
                    string consultaEmail = "SELECT COUNT(*) FROM estudante WHERE email = @Email";
                    using (MySqlCommand cmdVerifica = new MySqlCommand(consultaEmail, conexao))
                    {
                        cmdVerifica.Parameters.AddWithValue("@Email", estudante.Email);
                        int emailExiste = Convert.ToInt32(cmdVerifica.ExecuteScalar());

                        if (emailExiste > 0)
                        {
                            return "Erro: Este e-mail já está cadastrado!";
                        }
                    }

                    // Inserir o novo estudante
                    string query = @"
                        INSERT INTO estudante 
                        (nome, email, senha, status, telefone, cidade, dataNascimento, nacionalidade, 
                        pais, provincia, nivelAcademico, instituicao, dataCadastro)
                        VALUES 
                        (@Nome, @Email, MD5(MD5(@Senha)), @Status, @Telefone, @Cidade, @DataNascimento, @Nacionalidade,
                        @Pais, @Provincia, @NivelAcademico, @Instituicao, NOW());
                    ";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        // Campos obrigatórios
                        cmd.Parameters.AddWithValue("@Nome", estudante.Nome);
                        cmd.Parameters.AddWithValue("@Email", estudante.Email);
                        cmd.Parameters.AddWithValue("@Senha", estudante.Senha);
                        cmd.Parameters.AddWithValue("@Status", estudante.Status);
                        cmd.Parameters.AddWithValue("@DataNascimento", estudante.DataNascimento);

                        // Campos opcionais
                        cmd.Parameters.AddWithValue("@Telefone", string.IsNullOrEmpty(estudante.Telefone) ? DBNull.Value : (object)estudante.Telefone);
                        cmd.Parameters.AddWithValue("@Cidade", string.IsNullOrEmpty(estudante.Cidade) ? DBNull.Value : (object)estudante.Cidade);
                        cmd.Parameters.AddWithValue("@Nacionalidade", string.IsNullOrEmpty(estudante.Nacionalidade) ? DBNull.Value : (object)estudante.Nacionalidade);
                        cmd.Parameters.AddWithValue("@Pais", string.IsNullOrEmpty(estudante.Pais) ? DBNull.Value : (object)estudante.Pais);
                        cmd.Parameters.AddWithValue("@Provincia", string.IsNullOrEmpty(estudante.Provincia) ? DBNull.Value : (object)estudante.Provincia);
                        cmd.Parameters.AddWithValue("@NivelAcademico", string.IsNullOrEmpty(estudante.NivelAcademico) ? DBNull.Value : (object)estudante.NivelAcademico);
                        cmd.Parameters.AddWithValue("@Instituicao", string.IsNullOrEmpty(estudante.Instituicao) ? DBNull.Value : (object)estudante.Instituicao);

                        int linhasAfetadas = cmd.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            // Obter o ID do estudante recém-cadastrado
                            long idEstudante = cmd.LastInsertedId;

                            // Criar notificação para perfil incompleto
                            string queryNotificacao = @"
                                INSERT INTO notificacaoestudante 
                                (idEstudante, titulo, mensagem, estado)
                                VALUES 
                                (@idEstudante, 'Ekutivala - Perfil Incompleto', 
                                'Preencha seu perfil para acessar os cursos e aproveitar melhor a plataforma.', 'NAO_LIDA')";

                            using (MySqlCommand cmdNotificacao = new MySqlCommand(queryNotificacao, conexao))
                            {
                                cmdNotificacao.Parameters.AddWithValue("@idEstudante", idEstudante);
                                cmdNotificacao.ExecuteNonQuery();
                            }

                            mensagem = "Estudante cadastrado com sucesso!";
                        }
                        else
                        {
                            mensagem = "Erro ao cadastrar o estudante.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    mensagem = "Erro no sistema: " + ex.Message;
                }
            }

            return mensagem;
        }

        public EstudanteModel LoginEstudante(EstudanteModel estudante)
        {
            EstudanteModel estudanteLogado = null;

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sqlLogin = @"
                    SELECT idEstudante, nome, email, telefone, cidade, 
                        dataNascimento, nacionalidade, pais, provincia,
                        nivelAcademico, instituicao, dataCadastro, status
                    FROM estudante
                    WHERE email = @Email
                    AND senha = MD5(MD5(@Senha))
                    AND status = 'ATIVO'";

                using (MySqlCommand comando = new MySqlCommand(sqlLogin, conexao))
                {
                    comando.Parameters.AddWithValue("@Email", estudante.Email);
                    comando.Parameters.AddWithValue("@Senha", estudante.Senha);

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            estudanteLogado = new EstudanteModel
                            {
                                IdEstudante = reader.GetInt32("idEstudante"),
                                Nome = reader.GetString("nome"),
                                Email = reader.GetString("email"),
                                Telefone = reader.IsDBNull(reader.GetOrdinal("telefone")) ? null : reader.GetString("telefone"),
                                Cidade = reader.IsDBNull(reader.GetOrdinal("cidade")) ? null : reader.GetString("cidade"),
                                DataNascimento = reader.IsDBNull(reader.GetOrdinal("dataNascimento")) ? (DateTime?)null : reader.GetDateTime("dataNascimento"),
                                Nacionalidade = reader.IsDBNull(reader.GetOrdinal("nacionalidade")) ? null : reader.GetString("nacionalidade"),
                                Pais = reader.IsDBNull(reader.GetOrdinal("pais")) ? null : reader.GetString("pais"),
                                Provincia = reader.IsDBNull(reader.GetOrdinal("provincia")) ? null : reader.GetString("provincia"),
                                NivelAcademico = reader.IsDBNull(reader.GetOrdinal("nivelAcademico")) ? null : reader.GetString("nivelAcademico"),
                                Instituicao = reader.IsDBNull(reader.GetOrdinal("instituicao")) ? null : reader.GetString("instituicao"),
                                DataCadastro = reader.GetDateTime("dataCadastro"),
                                Status = reader.GetString("status")
                            };

                            Console.WriteLine($"Estudante autenticado: {estudanteLogado.Nome} - {estudanteLogado.Email}");
                        }
                        else
                        {
                            Console.WriteLine("Login falhou: Nenhum estudante encontrado com essas credenciais.");
                        }
                    }
                }
            }

            return estudanteLogado;
        }

        public List<EstudanteModel> ListarEstudantes()
        {
            List<EstudanteModel> estudantes = new List<EstudanteModel>();

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sql = @"
                            SELECT idEstudante, nome, email, telefone, status,
                                dataCadastro, cidade, nacionalidade, dataNascimento,
                                pais, provincia, nivelAcademico, instituicao
                            FROM estudante
                            ORDER BY dataCadastro DESC";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    using (MySqlDataReader leitor = comando.ExecuteReader())
                    {
                        while (leitor.Read())
                        {
                            EstudanteModel estudante = new EstudanteModel
                            {
                                IdEstudante = leitor.GetInt32("idEstudante"),
                                Nome = leitor.GetString("nome"),
                                Email = leitor.GetString("email"),
                                Telefone = leitor.IsDBNull(leitor.GetOrdinal("telefone")) ? null : leitor.GetString("telefone"),
                                Status = leitor.GetString("status"),
                                Cidade = leitor.IsDBNull(leitor.GetOrdinal("cidade")) ? null : leitor.GetString("cidade"),
                                Nacionalidade = leitor.IsDBNull(leitor.GetOrdinal("nacionalidade")) ? null : leitor.GetString("nacionalidade"),
                                Pais = leitor.IsDBNull(leitor.GetOrdinal("pais")) ? null : leitor.GetString("pais"),
                                Provincia = leitor.IsDBNull(leitor.GetOrdinal("provincia")) ? null : leitor.GetString("provincia"),
                                NivelAcademico = leitor.IsDBNull(leitor.GetOrdinal("nivelAcademico")) ? null : leitor.GetString("nivelAcademico"),
                                Instituicao = leitor.IsDBNull(leitor.GetOrdinal("instituicao")) ? null : leitor.GetString("instituicao"),
                                DataCadastro = leitor.GetDateTime("dataCadastro")
                            };

                            // Tratamento especial para data de nascimento (pode ser nulo)
                            if (!leitor.IsDBNull(leitor.GetOrdinal("dataNascimento")))
                            {
                                estudante.DataNascimento = leitor.GetDateTime("dataNascimento");
                            }

                            estudantes.Add(estudante);
                        }
                    }
                }
            }

            return estudantes;
        }

        public EstudanteModel ObterEstudantePorId(int idEstudante)
        {
            EstudanteModel estudante = null;

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sql = @"
                    SELECT idEstudante, nome, email, telefone, status,
                        dataCadastro, cidade, nacionalidade, dataNascimento,
                        pais, provincia, nivelAcademico, instituicao
                    FROM estudante
                    WHERE idEstudante = @idEstudante";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@idEstudante", idEstudante);

                    using (MySqlDataReader leitor = comando.ExecuteReader())
                    {
                        if (leitor.Read())
                        {
                            estudante = new EstudanteModel
                            {
                                IdEstudante = leitor.GetInt32("idEstudante"),
                                Nome = leitor["nome"] as string,
                                Email = leitor["email"] as string,
                                Telefone = leitor["telefone"] as string,
                                Status = leitor["status"] as string,
                                DataCadastro = leitor.GetDateTime("dataCadastro"),
                                Cidade = leitor["cidade"] as string,
                                Nacionalidade = leitor["nacionalidade"] as string,
                                DataNascimento = leitor["dataNascimento"] as DateTime?,
                                Pais = leitor["pais"] as string,
                                Provincia = leitor["provincia"] as string,
                                NivelAcademico = leitor["nivelAcademico"] as string,
                                Instituicao = leitor["instituicao"] as string
                            };
                        }
                    }
                }
            }

            return estudante;
        }

        public string AtualizarEstudante(EstudanteModel estudante)
        {
            string mensagem = "";
            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    string query = @"
                        UPDATE estudante 
                        SET nome = @Nome,
                            telefone = @Telefone,
                            email = @Email,
                            nacionalidade = @Nacionalidade,
                            cidade = @Cidade,
                            pais = @Pais,
                            provincia = @Provincia,
                            nivelAcademico = @NivelAcademico,
                            instituicao = @Instituicao,
                            status = @Status
                        WHERE idEstudante = @IdEstudante";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        // Parâmetros obrigatórios
                        cmd.Parameters.AddWithValue("@IdEstudante", estudante.IdEstudante);
                        cmd.Parameters.AddWithValue("@Nome", estudante.Nome ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Email", estudante.Email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", estudante.Status ?? "ATIVO");

                        // Parâmetros opcionais
                        cmd.Parameters.AddWithValue("@Telefone", estudante.Telefone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Nacionalidade", estudante.Nacionalidade ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cidade", estudante.Cidade ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Pais", estudante.Pais ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Provincia", estudante.Provincia ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NivelAcademico", estudante.NivelAcademico ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Instituicao", estudante.Instituicao ?? (object)DBNull.Value);

                        int linhas = cmd.ExecuteNonQuery();
                        mensagem = linhas > 0 ? "Dados atualizados com sucesso" : "Nenhuma alteração feita.";
                    }
                }
                catch (Exception ex)
                {
                    mensagem = "Erro ao atualizar: " + ex.Message;
                }
            }
            return mensagem;
        }

        public string EliminarEstudante(int idEstudante)
        {
            string mensagem = "";
            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    string query = @"
                        UPDATE estudante 
                        SET status = 'INATIVO'
                        WHERE idEstudante = @IdEstudante";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@IdEstudante", idEstudante);

                        int linhas = cmd.ExecuteNonQuery();
                        mensagem = linhas > 0
                            ? "Conta desativada com sucesso"
                            : "Nenhuma conta encontrada para desativar";
                    }
                }
                catch (Exception ex)
                {
                    mensagem = "Erro ao desativar conta: " + ex.Message;
                }
            }
            return mensagem;
        }

        public List<NotificacaoEstudanteModel> ListarNotificacoesNaoLidas(int idEstudante)
        {
            List<NotificacaoEstudanteModel> notificacoes = new List<NotificacaoEstudanteModel>();

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                string query = @"
                    SELECT idNotificacao, titulo, mensagem, dataCadastro 
                    FROM notificacaoestudante 
                    WHERE idEstudante = @IdEstudante 
                    AND estado = 'NAO_LIDA'
                    ORDER BY dataCadastro DESC";

                conexao.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                {
                    cmd.Parameters.AddWithValue("@IdEstudante", idEstudante);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notificacoes.Add(new NotificacaoEstudanteModel
                            {
                                IdNotificacao = reader.GetInt32("idNotificacao"),
                                Titulo = reader.GetString("titulo"),
                                Mensagem = reader.GetString("mensagem"),
                                DataCadastro = reader.GetDateTime("dataCadastro")
                            });
                        }
                    }
                }
            }

            return notificacoes;
        }

        public bool MarcarNotificacaoComoLida(int idNotificacao, int idEstudante)
        {
            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    string query = @"
                        UPDATE notificacaoestudante 
                        SET estado = 'LIDA' 
                        WHERE idNotificacao = @IdNotificacao
                        AND idEstudante = @IdEstudante";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@IdNotificacao", idNotificacao);
                        cmd.Parameters.AddWithValue("@IdEstudante", idEstudante);

                        int linhasAfetadas = cmd.ExecuteNonQuery();
                        return linhasAfetadas > 0;
                    }
                }
                catch (Exception ex)
                {
                    // Logar o erro (ex) se necessário
                    Console.WriteLine($"Erro ao marcar notificação como lida: {ex.Message}");
                    return false;
                }
            }
        }

        /****************************************FUNCIONARIO*******************************************/
        public FuncionarioModel LoginFuncionario(FuncionarioModel funcionario)
        {
            FuncionarioModel funcionarioLogado = null;

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sqlLogin = @"
                    SELECT id_func, nome_func, email_func, status_func, 
                        acesso_func, dataCadastro_func
                    FROM funcionario
                    WHERE email_func = @Email
                    AND senha_func = MD5(MD5(@Senha))
                    AND status_func = 'ATIVO'";

                using (MySqlCommand comando = new MySqlCommand(sqlLogin, conexao))
                {
                    comando.Parameters.AddWithValue("@Email", funcionario.EmailFunc);
                    comando.Parameters.AddWithValue("@Senha", funcionario.SenhaFunc);

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            funcionarioLogado = new FuncionarioModel
                            {
                                IdFunc = reader.GetInt32("id_func"),
                                NomeFunc = reader.GetString("nome_func"),
                                EmailFunc = reader.GetString("email_func"),
                                StatusFunc = reader.GetString("status_func"),
                                AcessoFunc = reader.IsDBNull(reader.GetOrdinal("acesso_func")) ? null : reader.GetString("acesso_func"),
                                DataCadastroFunc = reader.GetDateTime("dataCadastro_func")
                            };

                            Console.WriteLine($"Funcionário autenticado: {funcionarioLogado.NomeFunc} - {funcionarioLogado.EmailFunc}");
                        }
                        else
                        {
                            Console.WriteLine("Login falhou: Nenhum funcionário encontrado com essas credenciais.");
                        }
                    }
                }
            }

            return funcionarioLogado;
        }

        public string CadastrarFuncionario(FuncionarioModel funcionario)
        {
            string mensagem = string.Empty;

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    // Verificar se o email já está cadastrado
                    string consultaEmail = "SELECT COUNT(*) FROM funcionario WHERE email_func = @email_func";
                    using (MySqlCommand cmdVerifica = new MySqlCommand(consultaEmail, conexao))
                    {
                        cmdVerifica.Parameters.AddWithValue("@email_func", funcionario.EmailFunc);
                        int emailExiste = Convert.ToInt32(cmdVerifica.ExecuteScalar());

                        if (emailExiste > 0)
                        {
                            return "Erro: Este e-mail já está cadastrado!";
                        }
                    }

                    // Inserir o novo funcionário
                    string query = @"
                        INSERT INTO funcionario 
                        (nome_func, email_func, senha_func, status_func, acesso_func, dataCadastro_func)
                        VALUES 
                        (@nome_func, @email_func, MD5(MD5(@senha_func)), @status_func, @acesso_func, NOW())";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        // Campos obrigatórios
                        cmd.Parameters.AddWithValue("@nome_func", funcionario.NomeFunc);
                        cmd.Parameters.AddWithValue("@email_func", funcionario.EmailFunc);
                        cmd.Parameters.AddWithValue("@senha_func", funcionario.SenhaFunc);
                        cmd.Parameters.AddWithValue("@status_func", funcionario.StatusFunc);
                        cmd.Parameters.AddWithValue("@acesso_func", funcionario.AcessoFunc);

                        int linhasAfetadas = cmd.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            mensagem = "Funcionário cadastrado com sucesso!";
                        }
                        else
                        {
                            mensagem = "Erro ao cadastrar o funcionário.";
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    // Tratamento específico para erros MySQL
                    switch (ex.Number)
                    {
                        case 1062: // Duplicate entry
                            mensagem = "Erro: Este e-mail já está cadastrado no sistema!";
                            break;
                        case 1048: // Column cannot be null
                            mensagem = "Erro: Todos os campos obrigatórios devem ser preenchidos!";
                            break;
                        default:
                            mensagem = "Erro no banco de dados: " + ex.Message;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    mensagem = "Erro no sistema: " + ex.Message;
                }
            }

            return mensagem;
        }

        public List<FuncionarioModel> ListarTodosFuncionarios()
        {
            var funcionarios = new List<FuncionarioModel>();

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sql = @"
                    SELECT 
                        id_func AS IdFunc,
                        nome_func AS NomeFunc,
                        email_func AS EmailFunc,
                        status_func AS StatusFunc,
                        acesso_func AS AcessoFunc,
                        dataCadastro_func AS DataCadastroFunc
                    FROM funcionario 
                    ORDER BY dataCadastro_func DESC";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            funcionarios.Add(new FuncionarioModel
                            {
                                IdFunc = reader.GetInt32("IdFunc"),
                                NomeFunc = reader.GetString("NomeFunc"),
                                EmailFunc = reader.GetString("EmailFunc"),
                                StatusFunc = reader.GetString("StatusFunc"),
                                AcessoFunc = reader.IsDBNull(reader.GetOrdinal("AcessoFunc")) ? null : reader.GetString("AcessoFunc"),
                                DataCadastroFunc = reader.GetDateTime("DataCadastroFunc")
                            });
                        }
                    }
                }
            }

            return funcionarios;
        }

        public FuncionarioModel ObterFuncionarioPorId(int idFunc)
        {
            FuncionarioModel funcionario = null;

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sql = @"
                    SELECT id_func, nome_func, email_func, status_func, 
                        acesso_func, dataCadastro_func
                    FROM funcionario
                    WHERE id_func = @id_func";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@id_func", idFunc);

                    using (MySqlDataReader leitor = comando.ExecuteReader())
                    {
                        if (leitor.Read())
                        {
                            funcionario = new FuncionarioModel
                            {
                                IdFunc = leitor.GetInt32("id_func"),
                                NomeFunc = leitor.GetString("nome_func"),
                                EmailFunc = leitor.GetString("email_func"),
                                StatusFunc = leitor.GetString("status_func"),
                                AcessoFunc = leitor.IsDBNull(leitor.GetOrdinal("acesso_func")) ? null : leitor.GetString("acesso_func"),
                                DataCadastroFunc = leitor.GetDateTime("dataCadastro_func")
                            };
                        }
                    }
                }
            }

            return funcionario;
        }

        public string AtualizarFuncionario(FuncionarioModel funcionario)
        {
            string mensagem = "";

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    string query = @"
                        UPDATE funcionario 
                        SET nome_func = @NomeFunc,
                            email_func = @EmailFunc,
                            status_func = @StatusFunc,
                            acesso_func = @AcessoFunc
                        WHERE id_func = @IdFunc";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        // Parâmetros obrigatórios
                        cmd.Parameters.AddWithValue("@IdFunc", funcionario.IdFunc);
                        cmd.Parameters.AddWithValue("@NomeFunc", funcionario.NomeFunc);
                        cmd.Parameters.AddWithValue("@EmailFunc", funcionario.EmailFunc);
                        cmd.Parameters.AddWithValue("@StatusFunc", funcionario.StatusFunc);

                        // Parâmetro opcional (pode ser nulo)
                        if (string.IsNullOrEmpty(funcionario.AcessoFunc))
                        {
                            cmd.Parameters.AddWithValue("@AcessoFunc", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@AcessoFunc", funcionario.AcessoFunc);
                        }

                        int linhas = cmd.ExecuteNonQuery();
                        mensagem = linhas > 0 ? "Funcionário atualizado com sucesso" : "Nenhuma alteração feita.";
                    }
                }
                catch (MySqlException ex)
                {
                    if (ex.Number == 1062) // Código de erro para duplicação
                    {
                        mensagem = "Erro: Já existe um funcionário com este e-mail.";
                    }
                    else
                    {
                        mensagem = "Erro ao atualizar funcionário: " + ex.Message;
                    }
                }
                catch (Exception ex)
                {
                    mensagem = "Erro ao atualizar: " + ex.Message;
                }
            }

            return mensagem;
        }

        /****************************************LIVRO*******************************************/
        public string CadastrarLivro(LivroModel livro)
        {
            string mensagem = string.Empty;

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    // Verificar se o livro com o mesmo título e autor já existe (opcional)
                    string consultaLivro = "SELECT COUNT(*) FROM livro WHERE titulo = @Titulo AND autor = @Autor";
                    using (MySqlCommand cmdVerifica = new MySqlCommand(consultaLivro, conexao))
                    {
                        cmdVerifica.Parameters.AddWithValue("@Titulo", livro.Titulo);
                        cmdVerifica.Parameters.AddWithValue("@Autor", livro.Autor);
                        int livroExiste = Convert.ToInt32(cmdVerifica.ExecuteScalar());

                        if (livroExiste > 0)
                        {
                            return "Erro: Este livro já está cadastrado!";
                        }
                    }

                    string query = @"
                        INSERT INTO livro 
                        (titulo, autor, preco, formato, categoria, imagemCaminho, linkCaminho, status, dataCadastro)
                        VALUES 
                        (@Titulo, @Autor, @Preco, @Formato, @Categoria, @ImagemCaminho, @LinkCaminho, @Status, NOW());
                    ";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@Titulo", livro.Titulo);
                        cmd.Parameters.AddWithValue("@Autor", livro.Autor);
                        cmd.Parameters.AddWithValue("@Preco", livro.Preco);
                        cmd.Parameters.AddWithValue("@Formato", string.IsNullOrEmpty(livro.Formato) ? DBNull.Value : (object)livro.Formato);
                        cmd.Parameters.AddWithValue("@Categoria", string.IsNullOrEmpty(livro.Categoria) ? DBNull.Value : (object)livro.Categoria);
                        cmd.Parameters.AddWithValue("@ImagemCaminho", string.IsNullOrEmpty(livro.ImagemCaminho) ? DBNull.Value : (object)livro.ImagemCaminho);
                        cmd.Parameters.AddWithValue("@LinkCaminho", string.IsNullOrEmpty(livro.LinkCaminho) ? DBNull.Value : (object)livro.LinkCaminho);
                        cmd.Parameters.AddWithValue("@Status", livro.Status ?? "ATIVO");

                        int linhasAfetadas = cmd.ExecuteNonQuery();

                        mensagem = linhasAfetadas > 0 ? "Livro cadastrado com sucesso!" : "Erro ao cadastrar o livro.";
                    }
                }
                catch (Exception ex)
                {
                    mensagem = "Erro no sistema: " + ex.Message;
                }
            }

            return mensagem;
        }
        public List<LivroModel> ListarLivros()
        {
            var livros = new List<LivroModel>();

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sql = @"
                    SELECT idLivro, titulo, autor, preco, formato, 
                        categoria, imagemCaminho, linkCaminho, 
                        status, dataCadastro
                    FROM livro
                    WHERE  status = 'ATIVO'
                    ORDER BY dataCadastro DESC";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            livros.Add(new LivroModel
                            {
                                IdLivro = reader.GetInt32("idLivro"),
                                Titulo = reader.GetString("titulo"),
                                Autor = reader.GetString("autor"),
                                Preco = reader.GetDecimal("preco"),
                                Formato = reader.IsDBNull(reader.GetOrdinal("formato")) ? null : reader.GetString("formato"),
                                Categoria = reader.IsDBNull(reader.GetOrdinal("categoria")) ? null : reader.GetString("categoria"),
                                ImagemCaminho = reader.IsDBNull(reader.GetOrdinal("imagemCaminho")) ? null : reader.GetString("imagemCaminho"),
                                LinkCaminho = reader.IsDBNull(reader.GetOrdinal("linkCaminho")) ? null : reader.GetString("linkCaminho"),
                                Status = reader.GetString("status"),
                                DataCadastro = reader.GetDateTime("dataCadastro")
                            });
                        }
                    }
                }
            }

            return livros;
        }
        public List<LivroModel> ListarTodosLivros()
        {
            var livros = new List<LivroModel>();

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sql = @"
                    SELECT idLivro, titulo, autor, preco, formato, 
                        categoria, imagemCaminho, linkCaminho, 
                        status, dataCadastro
                    FROM livro 
                    ORDER BY dataCadastro DESC";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            livros.Add(new LivroModel
                            {
                                IdLivro = reader.GetInt32("idLivro"),
                                Titulo = reader.GetString("titulo"),
                                Autor = reader.GetString("autor"),
                                Preco = reader.GetDecimal("preco"),
                                Formato = reader.IsDBNull(reader.GetOrdinal("formato")) ? null : reader.GetString("formato"),
                                Categoria = reader.IsDBNull(reader.GetOrdinal("categoria")) ? null : reader.GetString("categoria"),
                                ImagemCaminho = reader.IsDBNull(reader.GetOrdinal("imagemCaminho")) ? null : reader.GetString("imagemCaminho"),
                                LinkCaminho = reader.IsDBNull(reader.GetOrdinal("linkCaminho")) ? null : reader.GetString("linkCaminho"),
                                Status = reader.GetString("status"),
                                DataCadastro = reader.GetDateTime("dataCadastro")
                            });
                        }
                    }
                }
            }

            return livros;
        }

        public LivroModel ObterLivroPorId(int id)
        {
            LivroModel livro = null;

            using (MySqlConnection con = new MySqlConnection(enderecoConexao))
            {
                con.Open();
                string query = "SELECT * FROM livro WHERE IdLivro = @Id";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            livro = new LivroModel
                            {
                                IdLivro = Convert.ToInt32(reader["IdLivro"]),
                                Titulo = reader["Titulo"].ToString(),
                                Autor = reader["Autor"].ToString(),
                                Preco = Convert.ToDecimal(reader["Preco"]),
                                Formato = reader["Formato"].ToString(),
                                Categoria = reader["Categoria"].ToString(),
                                LinkCaminho = reader["LinkCaminho"].ToString(),
                                ImagemCaminho = reader["ImagemCaminho"].ToString()
                            };
                        }
                    }
                }
            }

            return livro;
        }

        public int CadastrarVenda(VendaLivroModel venda)
        {
            int idVenda = 0;

            using (MySqlConnection con = new MySqlConnection(enderecoConexao))
            {
                con.Open();
                string query = @"
                    INSERT INTO venda_livro 
                    (nome_cliente, email_cliente, telefone_cliente, tipo_pagamento, id_livro_fk, fatura, status, data_cadastro) 
                    VALUES 
                    (@NomeCliente, @EmailCliente, @TelefoneCliente, @TipoPagamento, @IdLivroFK, @Fatura, @Status, @DataCadastro);
                    SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@NomeCliente", venda.NomeCliente);
                    cmd.Parameters.AddWithValue("@EmailCliente", venda.EmailCliente);
                    cmd.Parameters.AddWithValue("@TelefoneCliente", venda.TelefoneCliente);
                    cmd.Parameters.AddWithValue("@TipoPagamento", venda.TipoPagamento);
                    cmd.Parameters.AddWithValue("@IdLivroFK", venda.IdLivroFK);
                    cmd.Parameters.AddWithValue("@Fatura", venda.Fatura);
                    cmd.Parameters.AddWithValue("@Status", "Pendente");
                    cmd.Parameters.AddWithValue("@DataCadastro", DateTime.Now);

                    idVenda = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return idVenda;
        }


        public void AtualizarLivro(LivroModel livro)
        {
            using (MySqlConnection con = new MySqlConnection(enderecoConexao))
            {
                con.Open();

                string query = @"UPDATE livro SET 
                                    Titulo = @Titulo,
                                    Autor = @Autor,
                                    Preco = @Preco,
                                    Formato = @Formato,
                                    Categoria = @Categoria,
                                    LinkCaminho = @LinkCaminho
                                WHERE IdLivro = @IdLivro";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Titulo", livro.Titulo);
                    cmd.Parameters.AddWithValue("@Autor", livro.Autor);
                    cmd.Parameters.AddWithValue("@Preco", livro.Preco);
                    cmd.Parameters.AddWithValue("@Formato", livro.Formato);
                    cmd.Parameters.AddWithValue("@Categoria", livro.Categoria);
                    cmd.Parameters.AddWithValue("@LinkCaminho", livro.LinkCaminho);
                    cmd.Parameters.AddWithValue("@IdLivro", livro.IdLivro);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool InativarLivro(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(enderecoConexao))
                {
                    con.Open();

                    string query = "UPDATE livro SET status = 'INATIVO' WHERE IdLivro = @Id";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        /****************************************CURSOS*******************************************/
        public int CadastrarVendaCurso(VendaCursoModel venda)
        {
            int idVenda = 0;

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    string query = @"
                    INSERT INTO venda_curso
                    (nome_cliente, email_cliente, telefone_cliente, tipo_pagamento, id_curso_fk, fatura, data_cadastro, status)
                    VALUES
                    (@NomeCliente, @EmailCliente, @TelefoneCliente, @TipoPagamento, @IdCursoFK, @Fatura, NOW(), @Status);
                    SELECT LAST_INSERT_ID();";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@NomeCliente", venda.NomeCliente);
                        cmd.Parameters.AddWithValue("@EmailCliente", venda.EmailCliente);
                        cmd.Parameters.AddWithValue("@TelefoneCliente", venda.TelefoneCliente);
                        cmd.Parameters.AddWithValue("@TipoPagamento", venda.TipoPagamento);
                        cmd.Parameters.AddWithValue("@IdCursoFK", venda.IdCursoFK);
                        cmd.Parameters.AddWithValue("@Fatura", venda.Fatura);
                        cmd.Parameters.AddWithValue("@Status", venda.Status ?? "PENDENTE");

                        idVenda = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao cadastrar venda curso: " + ex.Message);
                }
            }

            return idVenda;
        }

        public List<VendaCursoModel> ObterCursosDoEstudante(int idEstudante)
        {
            List<VendaCursoModel> listaCursos = new List<VendaCursoModel>();

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sql = @"
                SELECT vc.id_venda, vc.nome_cliente, vc.email_cliente, vc.telefone_cliente,
                    vc.tipo_pagamento, vc.id_curso_fk, vc.fatura, vc.data_cadastro, vc.status
                FROM venda_curso vc
                INNER JOIN estudante e ON e.email = vc.email_cliente
                WHERE e.idEstudante = @idEstudante
                ORDER BY vc.data_cadastro DESC;";

                using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@idEstudante", idEstudante);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            VendaCursoModel venda = new VendaCursoModel
                            {
                                IdVenda = reader.GetInt32("id_venda"),
                                NomeCliente = reader.GetString("nome_cliente"),
                                EmailCliente = reader.GetString("email_cliente"),
                                TelefoneCliente = reader.GetString("telefone_cliente"),
                                TipoPagamento = reader.GetString("tipo_pagamento"),
                                IdCursoFK = reader.GetInt32("id_curso_fk"),
                                Fatura = reader.GetString("fatura"),
                                DataCadastro = reader.GetDateTime("data_cadastro"),
                                Status = reader.GetString("status")
                            };
                            listaCursos.Add(venda);
                        }
                    }
                }
            }

            return listaCursos;
        }

        public VendaCursoModel ObterVendaPorEmailECurso(string emailCliente, int idCurso)
        {
            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                conexao.Open();

                string sql = @"
                    SELECT 
                        id_venda, 
                        nome_cliente, 
                        email_cliente, 
                        telefone_cliente,
                        tipo_pagamento, 
                        id_curso_fk, 
                        fatura, 
                        data_cadastro, 
                        status
                    FROM venda_curso 
                    WHERE email_cliente = @emailCliente 
                    AND id_curso_fk = @idCurso
                    ORDER BY data_cadastro DESC 
                    LIMIT 1";

                using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@emailCliente", emailCliente);
                    cmd.Parameters.AddWithValue("@idCurso", idCurso);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new VendaCursoModel
                            {
                                IdVenda = reader.GetInt32("id_venda"),
                                NomeCliente = reader.GetString("nome_cliente"),
                                EmailCliente = reader.GetString("email_cliente"),
                                TelefoneCliente = reader.GetString("telefone_cliente"),
                                TipoPagamento = reader.GetString("tipo_pagamento"),
                                IdCursoFK = reader.GetInt32("id_curso_fk"),
                                Fatura = reader.GetString("fatura"),
                                DataCadastro = reader.GetDateTime("data_cadastro"),
                                Status = reader.GetString("status")
                            };
                        }
                    }
                }
            }

            return null;
        }

        public List<VendaCursoModel> ListarVendasCursos()
        {
            List<VendaCursoModel> vendas = new List<VendaCursoModel>();

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
                {
                    conexao.Open();

                    string sql = @"
                        SELECT id_venda, nome_cliente, email_cliente, telefone_cliente,
                            tipo_pagamento, id_curso_fk, fatura, data_cadastro, status
                        FROM venda_curso
                        ORDER BY data_cadastro DESC";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                // Verificar se as colunas não são nulas antes de acessar
                                if (!leitor.IsDBNull(leitor.GetOrdinal("id_venda")))
                                {
                                    VendaCursoModel venda = new VendaCursoModel
                                    {
                                        IdVenda = leitor.GetInt32("id_venda"),
                                        NomeCliente = leitor.IsDBNull(leitor.GetOrdinal("nome_cliente")) ? string.Empty : leitor.GetString("nome_cliente"),
                                        EmailCliente = leitor.IsDBNull(leitor.GetOrdinal("email_cliente")) ? string.Empty : leitor.GetString("email_cliente"),
                                        TelefoneCliente = leitor.IsDBNull(leitor.GetOrdinal("telefone_cliente")) ? string.Empty : leitor.GetString("telefone_cliente"),
                                        TipoPagamento = leitor.IsDBNull(leitor.GetOrdinal("tipo_pagamento")) ? string.Empty : leitor.GetString("tipo_pagamento"),
                                        IdCursoFK = leitor.IsDBNull(leitor.GetOrdinal("id_curso_fk")) ? 0 : leitor.GetInt32("id_curso_fk"),
                                        Fatura = leitor.IsDBNull(leitor.GetOrdinal("fatura")) ? string.Empty : leitor.GetString("fatura"),
                                        DataCadastro = leitor.IsDBNull(leitor.GetOrdinal("data_cadastro")) ? DateTime.MinValue : leitor.GetDateTime("data_cadastro"),
                                        Status = leitor.IsDBNull(leitor.GetOrdinal("status")) ? "Pendente" : leitor.GetString("status")
                                    };

                                    vendas.Add(venda);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no ListarVendasCursos: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                // Retorna lista vazia em caso de erro
            }

            return vendas;
        }

        public VendaCursoModel ObterVendaCursoPorId(int idVenda)
        {
            VendaCursoModel venda = null;

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
                {
                    conexao.Open();

                    string sql = @"
                        SELECT id_venda, nome_cliente, email_cliente, telefone_cliente,
                            tipo_pagamento, id_curso_fk, fatura, data_cadastro, status
                        FROM venda_curso
                        WHERE id_venda = @id_venda";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@id_venda", idVenda);

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            if (leitor.Read())
                            {
                                // Verificar se as colunas não são nulas antes de acessar
                                if (!leitor.IsDBNull(leitor.GetOrdinal("id_venda")))
                                {
                                    venda = new VendaCursoModel
                                    {
                                        IdVenda = leitor.GetInt32("id_venda"),
                                        NomeCliente = leitor.IsDBNull(leitor.GetOrdinal("nome_cliente")) ? string.Empty : leitor.GetString("nome_cliente"),
                                        EmailCliente = leitor.IsDBNull(leitor.GetOrdinal("email_cliente")) ? string.Empty : leitor.GetString("email_cliente"),
                                        TelefoneCliente = leitor.IsDBNull(leitor.GetOrdinal("telefone_cliente")) ? string.Empty : leitor.GetString("telefone_cliente"),
                                        TipoPagamento = leitor.IsDBNull(leitor.GetOrdinal("tipo_pagamento")) ? string.Empty : leitor.GetString("tipo_pagamento"),
                                        IdCursoFK = leitor.IsDBNull(leitor.GetOrdinal("id_curso_fk")) ? 0 : leitor.GetInt32("id_curso_fk"),
                                        Fatura = leitor.IsDBNull(leitor.GetOrdinal("fatura")) ? string.Empty : leitor.GetString("fatura"),
                                        DataCadastro = leitor.IsDBNull(leitor.GetOrdinal("data_cadastro")) ? DateTime.MinValue : leitor.GetDateTime("data_cadastro"),
                                        Status = leitor.IsDBNull(leitor.GetOrdinal("status")) ? "Pendente" : leitor.GetString("status")
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no ObterVendaCursoPorId: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            return venda;
        }

        public string AtualizarVendaCurso(VendaCursoModel venda)
        {
            string mensagem = "";

            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    string query = @"
                        UPDATE venda_curso 
                        SET status = @Status
                        WHERE id_venda = @IdVenda";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@IdVenda", venda.IdVenda);
                        cmd.Parameters.AddWithValue("@Status", venda.Status);

                        int linhas = cmd.ExecuteNonQuery();
                        mensagem = linhas > 0 ? "Venda atualizada com sucesso" : "Nenhuma alteração feita.";
                    }
                }
                catch (Exception ex)
                {
                    mensagem = "Erro ao atualizar venda: " + ex.Message;
                }
            }

            return mensagem;
        }

        public List<VendaLivroModel> ListarVendasLivros()
        {
            List<VendaLivroModel> vendas = new List<VendaLivroModel>();

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
                {
                    conexao.Open();

                    string sql = @"
                        SELECT vl.id_venda, vl.nome_cliente, vl.email_cliente, vl.telefone_cliente,
                            vl.tipo_pagamento, vl.id_livro_fk, vl.fatura, vl.data_cadastro, vl.status,
                            l.titulo, l.autor, l.preco, l.formato, l.categoria, l.imagemCaminho
                        FROM venda_livro vl
                        INNER JOIN livro l ON vl.id_livro_fk = l.idLivro
                        ORDER BY vl.data_cadastro DESC";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                if (!leitor.IsDBNull(leitor.GetOrdinal("id_venda")))
                                {
                                    VendaLivroModel venda = new VendaLivroModel
                                    {
                                        IdVenda = leitor.GetInt32("id_venda"),
                                        NomeCliente = leitor.IsDBNull(leitor.GetOrdinal("nome_cliente")) ? string.Empty : leitor.GetString("nome_cliente"),
                                        EmailCliente = leitor.IsDBNull(leitor.GetOrdinal("email_cliente")) ? string.Empty : leitor.GetString("email_cliente"),
                                        TelefoneCliente = leitor.IsDBNull(leitor.GetOrdinal("telefone_cliente")) ? string.Empty : leitor.GetString("telefone_cliente"),
                                        TipoPagamento = leitor.IsDBNull(leitor.GetOrdinal("tipo_pagamento")) ? string.Empty : leitor.GetString("tipo_pagamento"),
                                        IdLivroFK = leitor.IsDBNull(leitor.GetOrdinal("id_livro_fk")) ? 0 : leitor.GetInt32("id_livro_fk"),
                                        Fatura = leitor.IsDBNull(leitor.GetOrdinal("fatura")) ? string.Empty : leitor.GetString("fatura"),
                                        DataCadastro = leitor.IsDBNull(leitor.GetOrdinal("data_cadastro")) ? DateTime.MinValue : leitor.GetDateTime("data_cadastro"),
                                        Status = leitor.IsDBNull(leitor.GetOrdinal("status")) ? "Pendente" : leitor.GetString("status"),
                                        Livro = new LivroModel
                                        {
                                            IdLivro = leitor.GetInt32("id_livro_fk"),
                                            Titulo = leitor.IsDBNull(leitor.GetOrdinal("titulo")) ? string.Empty : leitor.GetString("titulo"),
                                            Autor = leitor.IsDBNull(leitor.GetOrdinal("autor")) ? string.Empty : leitor.GetString("autor"),
                                            Preco = leitor.IsDBNull(leitor.GetOrdinal("preco")) ? 0 : leitor.GetDecimal("preco"),
                                            Formato = leitor.IsDBNull(leitor.GetOrdinal("formato")) ? null : leitor.GetString("formato"),
                                            Categoria = leitor.IsDBNull(leitor.GetOrdinal("categoria")) ? null : leitor.GetString("categoria"),
                                            ImagemCaminho = leitor.IsDBNull(leitor.GetOrdinal("imagemCaminho")) ? null : leitor.GetString("imagemCaminho")
                                        }
                                    };

                                    vendas.Add(venda);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no ListarVendasLivros: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            return vendas;
        }

        public VendaLivroModel ObterVendaLivroPorId(int idVenda)
        {
            VendaLivroModel venda = null;

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
                {
                    conexao.Open();

                    string sql = @"
                        SELECT vl.id_venda, vl.nome_cliente, vl.email_cliente, vl.telefone_cliente,
                            vl.tipo_pagamento, vl.id_livro_fk, vl.fatura, vl.data_cadastro, vl.status,
                            l.titulo, l.autor, l.preco, l.formato, l.categoria, l.imagemCaminho, l.linkCaminho
                        FROM venda_livro vl
                        INNER JOIN livro l ON vl.id_livro_fk = l.idLivro
                        WHERE vl.id_venda = @id_venda";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@id_venda", idVenda);

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            if (leitor.Read())
                            {
                                if (!leitor.IsDBNull(leitor.GetOrdinal("id_venda")))
                                {
                                    venda = new VendaLivroModel
                                    {
                                        IdVenda = leitor.GetInt32("id_venda"),
                                        NomeCliente = leitor.IsDBNull(leitor.GetOrdinal("nome_cliente")) ? string.Empty : leitor.GetString("nome_cliente"),
                                        EmailCliente = leitor.IsDBNull(leitor.GetOrdinal("email_cliente")) ? string.Empty : leitor.GetString("email_cliente"),
                                        TelefoneCliente = leitor.IsDBNull(leitor.GetOrdinal("telefone_cliente")) ? string.Empty : leitor.GetString("telefone_cliente"),
                                        TipoPagamento = leitor.IsDBNull(leitor.GetOrdinal("tipo_pagamento")) ? string.Empty : leitor.GetString("tipo_pagamento"),
                                        IdLivroFK = leitor.IsDBNull(leitor.GetOrdinal("id_livro_fk")) ? 0 : leitor.GetInt32("id_livro_fk"),
                                        Fatura = leitor.IsDBNull(leitor.GetOrdinal("fatura")) ? string.Empty : leitor.GetString("fatura"),
                                        DataCadastro = leitor.IsDBNull(leitor.GetOrdinal("data_cadastro")) ? DateTime.MinValue : leitor.GetDateTime("data_cadastro"),
                                        Status = leitor.IsDBNull(leitor.GetOrdinal("status")) ? "Pendente" : leitor.GetString("status"),
                                        Livro = new LivroModel
                                        {
                                            IdLivro = leitor.GetInt32("id_livro_fk"),
                                            Titulo = leitor.IsDBNull(leitor.GetOrdinal("titulo")) ? string.Empty : leitor.GetString("titulo"),
                                            Autor = leitor.IsDBNull(leitor.GetOrdinal("autor")) ? string.Empty : leitor.GetString("autor"),
                                            Preco = leitor.IsDBNull(leitor.GetOrdinal("preco")) ? 0 : leitor.GetDecimal("preco"),
                                            Formato = leitor.IsDBNull(leitor.GetOrdinal("formato")) ? null : leitor.GetString("formato"),
                                            Categoria = leitor.IsDBNull(leitor.GetOrdinal("categoria")) ? null : leitor.GetString("categoria"),
                                            ImagemCaminho = leitor.IsDBNull(leitor.GetOrdinal("imagemCaminho")) ? null : leitor.GetString("imagemCaminho"),
                                            LinkCaminho = leitor.IsDBNull(leitor.GetOrdinal("linkCaminho")) ? null : leitor.GetString("linkCaminho")
                                        }
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no ObterVendaLivroPorId: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            return venda;
        }

        public string AtualizarVendaLivro(VendaLivroModel venda)
        {
            string mensagem = "";
            
            using (MySqlConnection conexao = new MySqlConnection(enderecoConexao))
            {
                try
                {
                    conexao.Open();

                    string query = @"
                        UPDATE venda_livro 
                        SET status = @Status
                        WHERE id_venda = @IdVenda";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@IdVenda", venda.IdVenda);
                        cmd.Parameters.AddWithValue("@Status", venda.Status);

                        int linhas = cmd.ExecuteNonQuery();
                        mensagem = linhas > 0 ? "Venda atualizada com sucesso" : "Nenhuma alteração feita.";
                    }
                }
                catch (Exception ex)
                {
                    mensagem = "Erro ao atualizar venda: " + ex.Message;
                }
            }
            
            return mensagem;
        }




    }
}