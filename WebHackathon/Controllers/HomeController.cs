﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebHackathon.CrossCutting;
using WebHackathon.Helper;

namespace WebHackathon.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult InscreverAtividade()
        {            
            return View();
        }

        public ActionResult DetalheAtividade(bool? validado)
        {
            if (validado.HasValue)
                ViewBag.Validado = validado.Value.ToString().ToLower();
            else
                ViewBag.Validado = "false";

            return View();
        }

        public async Task<ActionResult> getAgendamentos(bool navio, bool atividade)
        {
            AgendamentoResponse response = new AgendamentoResponse();

            try
            {
                MyHttp myHttp = new MyHttp(@"https://hackathonbtpapi.azurewebsites.net/api/");                
                var result = await myHttp.Get(string.Concat("Agendamento/ObterPorModalidadeId/", navio.ToString().ToLower(), "&", atividade.ToString().ToLower()));
                MyFile.saveJson(result);
                response.jsonCalendar = result;                
                response.ResultCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.ResultCode = (int)HttpStatusCode.InternalServerError;
            }

            return Json(new { data = response.jsonCalendar }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getQRCode()
        {
            JsonResult result = new JsonResult();
            var request = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            var bitmap = GerarQRCode(200, 200, request + "/Home/setScore?pessoaId=2&score=1");
            byte[] imageByteData = ImageToByte2(bitmap);
            string imageBase64Data = Convert.ToBase64String(imageByteData);
            string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
            result.Data = imageDataURL;
            return Json(new { data = imageDataURL }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> setScore(string pessoaId, int score)
        {
            ScoreResponse response = new ScoreResponse();

            try
            {
                MyHttp myHttp = new MyHttp(@"https://hackathonbtpapi.azurewebsites.net/api/");
                var result = await myHttp.Post("Score/Pontuacao/", new { PessoaId = pessoaId, Pontuacao = score });
                response.ResultCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.ResultCode = (int)HttpStatusCode.InternalServerError;
            }
            
            return View("~/Views/Home/DetalheAtividade.cshtml?validado=true");
        }

        public async Task<ActionResult> getScore(string pessoaId)
        {
            JsonResult result = new JsonResult();
            List<ScoreResponse> response = new List<ScoreResponse>();

            try
            {
                MyHttp myHttp = new MyHttp(@"https://hackathonbtpapi.azurewebsites.net/api/");
                response = await myHttp.Get<ScoreResponse>(string.Concat("Score/ObterPorPessoaId/", pessoaId));
                return Json(new { data = response[0].Pontuacao }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = (int)HttpStatusCode.InternalServerError });
            }
        }

        public async Task<ActionResult> agendar(AgendamentoRequest request)
        {
            AgendamentoInsertResponse response = new AgendamentoInsertResponse();

            try
            {
                MyHttp myHttp = new MyHttp(@"https://hackathonbtpapi.azurewebsites.net/api/");
                var result = await myHttp.Post<AgendamentoInsertResponse>("Agendamento/Incluir/", new {
                    pessoaID = request.pessoaID,
                    modalidadeID = request.modalidadeID,
                    dataAgendamentoInicio = request.dataAgendamentoInicio,
                    dataAgendamentoFim = request.dataAgendamentoFim,
                    descricao = request.descricao,
                    detalhe = request.detalhe,
                    nomeNavio = "",
                    statusAgendamento = 0,
                });

                response.id = result.id;
                response.ResultCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                return Json(new { data = (int)HttpStatusCode.InternalServerError });
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private Bitmap GerarQRCode(int width, int height, string text)
        {
            try
            {
                var bw = new ZXing.BarcodeWriter();
                var encOptions = new ZXing.Common.EncodingOptions() { Width = width, Height = height, Margin = 0 };
                bw.Options = encOptions;
                bw.Format = ZXing.BarcodeFormat.QR_CODE;
                var resultado = new Bitmap(bw.Write(text));
                return resultado;
            }
            catch
            {
                throw;
            }
        }
    }
}