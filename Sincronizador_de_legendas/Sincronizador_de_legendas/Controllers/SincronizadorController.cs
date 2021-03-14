﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sincronizador_de_legendas.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sincronizador_de_legendas.Controllers
{
    public class SincronizadorController : Controller
    {
        private readonly string _pastaResources;
        //criei esse construtor para conseguir pegar o caminho onde a aplicação está rodando 
        //e posteriormente salvar os arquivos na pasta resources
        public SincronizadorController(IWebHostEnvironment env)
        {
            _pastaResources = $"{env.ContentRootPath}\\Resources";
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        //método para enviar os arquivos usando a interface IFormFile
        [HttpPost]
        public IActionResult ProcessarArquivosDeLegenda(IEnumerable<IFormFile> arquivos, double offset)
        {
            foreach (var arquivo in arquivos)
            {
                try
                {
                    var arquivoProcessado = ProcessarLegenda(arquivo, offset);

                    //armazena o arquivo formatado na pasta resources com "offseted-" na frente do nome
                    System.IO.File.WriteAllLines($"{_pastaResources}\\offseted-{arquivo.FileName}", arquivoProcessado);
                }
                catch (Exception e)
                {
                    //se houver arquivo(s) invalido(s) será retornado um erro
                    ViewData["Erro"] = e.Message;
                    return View(ViewData);
                }
            }

            ViewData["Resultado"] = "Legenda(s) formatada(s) com sucesso!";
            return View(ViewData);
        }

        private static IEnumerable<string> ProcessarLegenda(IFormFile arquivo, double offset)
        {
            if(!arquivo.FileName.Contains(".srt"))
            {
                throw new Exception("Arquivo(s) selecionado(s) não pertence ao formato 'srt'");
            }

            //armazena cada linha do arquivo em um indice do array
            var arquivoArray = new StreamReader(arquivo.OpenReadStream())
            .ReadToEnd()
            .Replace("\r", string.Empty)
            .Split("\n");
            for (int i = 0; i < arquivoArray.Length; i++)
            {
                //verifica se a linha é de tempo ou não
                if (arquivoArray[i].Contains(" --> "))
                {
                    //separa os tempos da linha e armazena-os
                    string[] tempos = arquivoArray[i].Split(" --> ");
                    string linhaFormatada = default;

                    //ajuste do primeiro tempo da linha
                    string tempoProcessado = DateTime.ParseExact(tempos[0], "HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture).AddMilliseconds(-offset).ToString("HH:mm:ss,fff");
                    linhaFormatada += $"{tempoProcessado} --> ";

                    //ajuste do segundo tempo da linha
                    tempoProcessado = DateTime.ParseExact(tempos[1], "HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture).AddMilliseconds(-offset).ToString("HH:mm:ss,fff");
                    linhaFormatada += tempoProcessado;

                    //coloca a linha formatada no lugar da linha original
                    arquivoArray[i] = linhaFormatada;
                }
            }

            return arquivoArray;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
