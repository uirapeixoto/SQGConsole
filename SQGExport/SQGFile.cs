﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQGExport
{
    public class SQGFile
    {
        public string CodCobrador { get; set; }
        public string NroSequencia { get; set; }

        private static List<SQGModel> Dados(StreamReader reader)
        {
            int counter = 1;
            string line = null;
            int noParcelaOut;
            double valorLiberadoOut;
            var listData = new List<SQGModel>();

            while ((line = reader.ReadLine()) != null)
            {
                var addData = new SQGModel
                {
                    Linha = counter,
                    Grupo = line.Substring(0, 6),
                    Cota = line.Substring(6, 6),
                    NoParcela = Int32.TryParse(line.Substring(648, 3), out noParcelaOut) ? noParcelaOut : 0,
                    NomeConsorciado = line.Substring(12, 70),
                    CdProduto = line.Substring(518, 3),
                    Teste = (line.Substring(518, 3) == "ING") ? line.Substring(562, 10).Trim() : "",
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
                counter++;
                listData.Add(addData);
            }
            return listData;
        }

        public List<SqgDadosFiltradosModel> AgrupadorGrupoCotaConsorciadoProduto(List<SQGModel> dados)
        {
            return dados
                .GroupBy(g => new { g.NomeConsorciado, g.CdProduto, g.Grupo, g.Cota })
                .Select(x => new SqgDadosFiltradosModel
                {
                    Grupo = x.FirstOrDefault().Grupo,
                    Cota = x.FirstOrDefault().Cota,
                    NomeConsorciado = x.FirstOrDefault().NomeConsorciado,
                    CdProduto = x.FirstOrDefault().CdProduto,
                    Parcelas = x.Count(),
                    Total = x.Sum(s => s.ValorLiberado)
                }).Where(x => x.Total > 500000).ToList();
        }

        public List<SqgDadosFiltradosModel> AgrupadorConsorciadoProduto(List<SQGModel> dados)
        {
            return dados
                .GroupBy(g => new { g.NomeConsorciado, g.CdProduto })
                .Select(x => new SqgDadosFiltradosModel
                {
                    Grupo = x.FirstOrDefault().Grupo,
                    Cota = x.FirstOrDefault().Cota,
                    NomeConsorciado = x.FirstOrDefault().NomeConsorciado,
                    CdProduto = x.FirstOrDefault().CdProduto,
                    Parcelas = x.Count(),
                    Total = x.Sum(s => s.ValorLiberado)
                }).Where(x => x.Total > 500000).ToList();
        }

        public void GerarTxt(List<SqgDadosFiltradosModel> dados)
        {
            string path = Directory.GetCurrentDirectory(); ;
            var writer = new StreamWriter($@"{path}\ExportData.txt", true, Encoding.GetEncoding(1252));
            writer.WriteLine($"Grupo;Cota;Nome Consorciado;Produto;Parcelas;Total");
            dados.ForEach(x => {
                writer.WriteLine($"{x.Grupo};{x.Cota};{x.NomeConsorciado.Trim()};{x.CdProduto};{x.Parcelas};{x.Total}");
            });
        }

        public List<string> ListarNomesFiltrados(string fileReader)
        {
            string line;
            double valorLiberadoOut;
            var ListReader = new List<SQGModel>();
            StreamReader reader = new StreamReader(fileReader, Encoding.GetEncoding(1252));

            while ((line = reader.ReadLine()) != null)
            {
                var addData = new SQGModel
                {
                    NomeConsorciado = line.Substring(12, 70),
                    CdProduto = line.Substring(518, 3),
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

                ListReader.Add(addData);
            };
            reader.Close();
            return ListReader
                .GroupBy(g => new { g.NomeConsorciado, g.CdProduto })
                .Select(x => new SqgDadosFiltradosModel
                {
                    Grupo = x.FirstOrDefault().Grupo,
                    Cota = x.FirstOrDefault().Cota,
                    NomeConsorciado = x.FirstOrDefault().NomeConsorciado,
                    CdProduto = x.FirstOrDefault().CdProduto,
                    Parcelas = x.Count(),
                    Total = x.Sum(s => s.ValorLiberado)
                }).Where(x => x.Total > 500000).Select(x => x.NomeConsorciado).ToList();
        }
    }
}
