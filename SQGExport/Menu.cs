using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SQGExport
{
    public class Menu
    {
        public string ArquivoSelecionado { get; set; }

        public string Itens(string path)
        {

            bool isValid = false;
            int numberTest;
            string menuSelecionado;

            Console.WriteLine("Escolha o arquivo digitando o número correspondente:\n");
            foreach (var file in ListarArquivos(path))
            {
                Console.WriteLine($"( {file.Id} ) => {file.Filename}");
            };
            var listamenu = ListarArquivos(path);
            do
            {
                Console.Write("Digite:");
                menuSelecionado = Console.ReadLine();
                isValid = Int32.TryParse(menuSelecionado, out numberTest);

                if (!isValid)
                    Console.WriteLine("Menu inválido, digite o número do item:\n");
                else
                {
                    if (!listamenu.Select(x => x.Id).Contains(numberTest))
                    {
                        Console.WriteLine("Menu inválido:\n");
                    }
                    else
                        break;
                }

            } while (isValid);

            return ListarArquivos(path).FirstOrDefault(x => x.Id == numberTest).Filename;
        }

        private static List<FileStringModel> ListarArquivos(string nameDir)
        {
            int counter = 1;
            var FileList = new List<FileStringModel>();

            DirectoryInfo Dir = new DirectoryInfo($@"{nameDir}");
            // Busca automaticamente todos os arquivos em todos os subdiretórios
            FileInfo[] Files = Dir.GetFiles("*", SearchOption.TopDirectoryOnly);
            foreach (FileInfo File in Files)
            {
                // Retira o diretório informado inicialmente
                string FileName = File.FullName.Replace(Dir.FullName, "");
                FileList.Add(new FileStringModel
                {
                    Id = counter,
                    Filename = FileName.Replace("\\","")
                });

                counter++;
            }

            return FileList.Where(x => x.Filename != "SQGExport.exe").ToList();
        }
    }
}
