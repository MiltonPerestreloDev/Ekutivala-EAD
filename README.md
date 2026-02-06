# 📚 Ekutivala EAD - Plataforma de Ensino à Distância

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue.svg)](https://docs.microsoft.com/aspnet/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)]()

Uma plataforma completa de ensino à distância desenvolvida em ASP.NET Core 8.0, oferecendo gestão de cursos, biblioteca digital e sistema de vendas integrado.

## 🚀 Sobre o Projeto

O Ekutivala EAD é uma solução moderna e escalável para educação online, desenvolvida com as melhores práticas do mercado. A plataforma permite que estudantes acessem cursos, comprem livros digitais e participem de uma comunidade de aprendizado dinâmica.

### 🎯 Funcionalidades Principais

#### 📖 Gestão de Cursos
- **Catálogo de Cursos**: Browse through diverse course offerings including idiomas, cinematografia, e mais
- **Cursos Gratuitos e Pagos**: Flexible pricing model with free and premium content
- **Sistema de Aulas**: Organized lesson structure with progress tracking
- **Certificados**: Automatic certificate generation upon course completion

#### 📚 Biblioteca Digital
- **Livros Digitais**: Extensive collection of e-books in PDF format
- **Biblioteca Física**: Management system for physical book inventory
- **Upload Automatizado**: Easy book and cover upload system
- **Busca Avançada**: Powerful search and filtering capabilities

#### 💳 Sistema de Vendas
- **Múltiplos Pagamentos**: Express, reference, and bank transfer options
- **Gestão de Vendas**: Complete sales tracking and reporting
- **Comissões**: Built-in commission management system
- **Relatórios Financeiros**: Detailed sales and revenue analytics

#### 👥 Gestão de Usuários
- **Perfil de Estudante**: Comprehensive student profiles with progress tracking
- **Painel Administrativo**: Full admin dashboard for system management
- **Sistema de Notificações**: Real-time notification system for students
- **Controle de Acesso**: Role-based access control

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 8.0**: Latest .NET framework for high performance
- **ASP.NET Core MVC**: Modern web framework for building APIs and web apps
- **MySQL**: Relational database for data persistence

### Frontend
- **HTML5/CSS3/JavaScript**: Modern web standards
- **Bootstrap 5**: Responsive UI framework
- **jQuery**: Fast and feature-rich JavaScript library
- **DataTables**: Advanced table interactions
- **Font Awesome**: Icon library for UI enhancement
- **Owl Carousel**: Touch-enabled carousel plugin

### Ferramentas de Desenvolvimento
- **Visual Studio 2022**: Primary development environment
- **Git**: Version control system
- **GitHub**: Code hosting and collaboration

## 📋 Pré-requisitos

- **.NET 8.0 SDK** - [Download aqui](https://dotnet.microsoft.com/download/dotnet/8.0)
- **MySQL Server** 8.0 ou superior
- **Visual Studio 2022** ou **VS Code**
- **Git** para controle de versão

## 🚀 Instalação e Configuração

### 1. Clone o Repositório
```bash
git clone https://github.com/MiltonPerestreloDev/Ekutivala-EAD.git
cd Ekutivala-EAD
```

### 2. Configuração do Banco de Dados
```sql
-- Crie o banco de dados MySQL
CREATE DATABASE dbekutivala;
```


### 3. Restaurar Dependências
```bash
dotnet restore
```

### 4. Compilar e Executar
```bash
dotnet build
dotnet run
```

## 📁 Estrutura do Projeto

```
Ekutivala_EAD/
├── 📂 Controllers/          # Lógica de controle MVC
│   ├── EstudanteController.cs
│   ├── Files1Controller.cs
│   └── HomeController.cs
├── 📂 Models/              # Modelos de dados e entidades
│   ├── CursoModel.cs
│   ├── EstudanteModel.cs
│   └── VendaCursoModel.cs
├── 📂 Views/               # Interfaces Razor
│   ├── Estudante/
│   ├── Files1/
│   ├── Home/
│   └── Shared/
├── 📂 Services/            # Lógica de negócio
│   ├── ICursoService.cs
│   └── CursoService.cs
├── 📂 ViewModels/          # Models para Views
├── 📂 Data/               # Arquivos de dados
├── 📂 wwwroot/            # Recursos estáticos
│   ├── css/
│   ├── js/
│   └── uploads/
└── 📄 Program.cs          # Ponto de entrada da aplicação
```

## 🎯 Como Usar

### Para Estudantes
1. **Acessar a Plataforma**: Navegue até a página principal
2. **Criar Conta**: Registre-se como estudante
3. **Explorar Cursos**: Navegue pelo catálogo de cursos disponíveis
4. **Comprar Cursos**: Escolha o método de pagamento preferido
5. **Acessar Conteúdo**: Acesse seus cursos na área do estudante

### Para Administradores
1. **Login de Admin**: Acesse `/Files1/Login_func`
2. **Gerenciar Cursos**: Adicione, edite ou remova cursos
3. **Gerenciar Estudantes**: Visualize e gerencie usuários
4. **Relatórios**: Acesse relatórios de vendas e progresso



## 📝 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 👨‍💻 Autor

**Milton Perestrelo**
- GitHub: [@MiltonPerestreloDev](https://github.com/MiltonPerestreloDev)
- Email: [perestrelo1000ton@gmail.com](mailto:perestrelo1000ton@gmail.com)

## 🙏 Agradecimentos

- **Microsoft** - pela excelente plataforma .NET
- **MySQL Community** - pelo banco de dados robusto
- **Bootstrap Contributors** - pelo framework UI responsivo
- **Font Awesome** - pelos ícones incríveis

## 📞 Suporte

Para suporte, envie um email para ekutivalasuporte@gmail.comm ou abra uma [issue](https://github.com/MiltonPerestreloDev/Ekutivala-EAD/issues) no GitHub.

---

⭐ **Se este projeto foi útil para você, por favor considere dar uma estrela!**

**Made with ❤️ by Milton Perestrelo**