using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SQGExport
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory();
            EnviromentEnum env = EnviromentEnum.Development;
            if (env == EnviromentEnum.Development)
                path = "D:\\Wiz\\SQG";

            var menu = new Menu();
            var itemMenu = menu.Itens(path);

            Console.WriteLine($"Arquivo Selecionado: {itemMenu}");
            Console.WriteLine($"Aguarde até o final do processamento.");
            var result = Exportar(itemMenu);
            Console.WriteLine($"Arquivos OutputFileFiltered.txt e OutputFileUpdated.txt gerados com sucesso. Precione qualquer tecla para finalizar.");
            Console.WriteLine($"Total de Registros: {result.TotalLines}. Total de registros filtrados {result.TotalLinesFiltereds}\n");
            Console.ReadKey();
        }

        private static ResultModel Exportar(string filesource)
        {
            int totalFiltered = 0;
            int totalUpdated = 0;

            double valorLiberadoOut;
            int noParcelaOut;
            int counter = 1;
            string line;
            string path = Directory.GetCurrentDirectory();
            var ListReader = new List<SQGModel>();

            EnviromentEnum env = EnviromentEnum.Development;
            if (env == EnviromentEnum.Development)
                path = "D:\\Wiz\\SQG";

            var fileReader = $@"{path}\{filesource}";
            var outputFileFiltered = $@"{path}\OutputFileFiltered.txt";
            var outputFileUpdated = $@"{path}\OutputFileUpdated.txt";

            var SQG = new SQGFile();
            var filtro = SQG.ListarNomesFiltrados(fileReader);

            StreamWriter fileFiltered = new StreamWriter(outputFileFiltered, false, Encoding.GetEncoding(1252));
            StreamWriter fileUpdated = new StreamWriter(outputFileUpdated, false, Encoding.GetEncoding(1252));
            try
            {
                using (StreamReader reader = new StreamReader(fileReader, Encoding.GetEncoding(1252)))
                {

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (counter != 1)
                        {
                            var addRecord = new SQGModel
                            {
                                Linha = counter,
                                Grupo = line.Substring(0, 6),
                                Cota = line.Substring(6, 6),
                                NoParcela = int.TryParse(line.Substring(648, 3), out noParcelaOut) ? noParcelaOut : 0,
                                NomeConsorciado = line.Substring(12, 70),
                                CdProduto = line.Substring(518, 3),
                                Teste = (line.Substring(518, 2) == "AN") ? line.Substring(564, 6).Trim() : "",
                                ValorLiberado =
                                    (line.Substring(518, 3) == "ANG") ?
                                    (double.TryParse(line.Substring(564, 6).Trim().Replace("MIL", "000"), out valorLiberadoOut) ? valorLiberadoOut : 00.00) :
                                    (line.Substring(518, 3) == "0AN") ?
                                    (double.TryParse(line.Substring(551, 6), out valorLiberadoOut) ? valorLiberadoOut : 0.00) :
                                    (line.Substring(518, 3) == "0IM") ?
                                    (double.TryParse(line.Substring(562, 10).Trim(), out valorLiberadoOut) ? valorLiberadoOut : 0.00) :
                                    (line.Substring(518, 3) == "ING") ?
                                    (double.TryParse(line.Substring(562, 10).Trim(), out valorLiberadoOut) ? valorLiberadoOut : 0.00) : 00.00,
                            };
                            
                            if (filtro.Any(x => x.Contains(addRecord.NomeConsorciado.Trim())))
                            {
                                totalFiltered++;
                                fileFiltered.WriteLine(line);
                            }
                            else
                            {
                                totalUpdated++;
                                fileUpdated.WriteLine(line);
                            }
                            ListReader.Add(addRecord);
                        }
                        else
                        {
                            fileFiltered.WriteLine(line);
                            fileUpdated.WriteLine(line);
                        }
                        counter++;
                    }
                    reader.Close();
                    fileUpdated.Close();
                    fileFiltered.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Falha no processamento: {e.Message}");
            }

            return new ResultModel
            {
                TotalLines = counter,
                TotalLinesFiltereds = totalFiltered,
                TotalLinesUpdateds = totalUpdated
            };
        }
    }
}
