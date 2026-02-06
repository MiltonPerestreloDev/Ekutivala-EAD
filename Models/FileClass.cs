using System;
namespace Ekutivala_EAD.Models;

 public class FileClass
{
    

    /// <summary>
    /// Identificador do arquivo.
    /// </summary>
    public int FileId { get; set; } = 0;

    /// <summary>
    /// Nome do arquivo.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Caminho do arquivo.
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// Lista de arquivos filhos.
    /// </summary>
    public List<FileClass> Files { get; set; } = new List<FileClass>();

}