﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FIRSTShares.Data;
using FIRSTShares.Entities;
using FIRSTShares.Models;
using GoogleTranslateFreeApi;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FIRSTShares.Controllers
{
    public class PostController : Controller
    {
        public readonly LazyContext BD;
        public Usuario Usuario;
        public Categoria Categoria;
        public Discussao Discussao;
        public Postagem Postagem;

        public PostController(LazyContext bd)
        {
            BD = bd;

            Categoria = new Categoria(BD);

            Usuario = new Usuario(BD);

            Discussao = new Discussao(BD);

            Postagem = new Postagem(BD);
        }

        public IActionResult Index(int id, string idioma)
        {
            MostrarFotoPerfil();

            var postagem = BD.Postagens.Single(p => p.ID == id && !p.Excluido);
            var categorias = BD.Categorias.Where(c => c.Excluido == false).ToList();

            if (!string.IsNullOrEmpty(idioma))
            {
                var traducao = TraduzirPostAsync(postagem, idioma);

                var modelTupleCategoriasPostagemTraucao =
                    new Tuple<List<Categoria>, Postagem>(categorias, traducao.Result);

                return View(modelTupleCategoriasPostagemTraucao);
            }

            var modelTupleCategoriasPostagem = new Tuple<List<Categoria>, Postagem>(categorias, postagem);

            return View(modelTupleCategoriasPostagem);
        }

        public async Task<Postagem> TraduzirPostAsync(Postagem postagem, string idioma)
        {
            var translator = new GoogleTranslator();

            var from = GoogleTranslator.GetLanguageByName("Portuguese");
            var to = GoogleTranslator.GetLanguageByName("Spanish");

            if (idioma == Idioma.PT.ToString())
            {
                from = GoogleTranslator.GetLanguageByName("Spanish");
                to = GoogleTranslator.GetLanguageByName("Portuguese");
            }

            var resultPost = await translator.TranslateLiteAsync(postagem.ConteudoHtml, from, to);
            var resultThread = await translator.TranslateLiteAsync(postagem.Discussao.Assunto, from, to);

            foreach (var comentario in postagem.Postagens)
            {
                var resultComent = (await translator.TranslateLiteAsync(comentario.ConteudoHtml, from, to)).MergedTranslation;
                comentario.ConteudoHtml = resultComent;
            }

            postagem.ConteudoHtml = resultPost.MergedTranslation;
            postagem.Discussao.Assunto = resultThread.MergedTranslation;

            return postagem;
        }

        [HttpPost]
        public void AdicionarComentario([FromBody] ComentarioModel comentario)
        {
            var postagemPai = BD.Postagens.Single(p => p.ID == comentario.IDPostagemPai);
            var usuarioLogado = RetornarUsuarioLogado();
            var comentarioPostagem = new Postagem
            {
                Categoria = postagemPai.Categoria,
                ConteudoHtml = comentario.Conteudo,
                DataCriacao = DateTime.Now,
                Discussao = postagemPai.Discussao,
                PostagemPai = postagemPai,
                Usuario = usuarioLogado
            };

            if (usuarioLogado.ID != postagemPai.Usuario.ID)
            {
                var notificacao = new Notificacao
                {
                    Acao = TipoAcao.Comentou,
                    UsuarioAcao = RetornarUsuarioLogado(),
                    UsuarioNotificado = postagemPai.Usuario,
                    Data = DateTime.Now
                };

                BD.Add(notificacao);
                BD.SaveChanges();
            }

            SalvarComentario(comentarioPostagem);
        }

        [HttpPost]
        public object Curtir([FromBody] CurtirModel model)
        {
            var postagem = BD.Postagens.FirstOrDefault(p => p.ID == model.PostID);
            var usuario = RetornarUsuarioLogado();
            var curtida = BD.Curtidas.FirstOrDefault(u => u.Usuario == usuario && u.Postagem.ID == model.PostID);

            if (model.Curtiu && curtida == null)
            {
                var novaCurtida = new Curtida
                {
                    Curtiu = model.Curtiu,
                    Postagem = BD.Postagens.FirstOrDefault(p => p.ID == model.PostID),
                    Usuario = usuario
                };

                if (usuario.ID != postagem.Usuario.ID)
                {
                    var notificacao = new Notificacao
                    {
                        Acao = TipoAcao.Curtiu,
                        UsuarioAcao = usuario,
                        UsuarioNotificado = postagem.Usuario,
                        Data = DateTime.Now
                    };

                    BD.Add(notificacao);
                    BD.SaveChanges();
                }

                SalvarCurtida(novaCurtida);
            }
            else if (model.Curtiu && curtida != null)
            {
                curtida.Excluido = false;

                SalvarCurtida(curtida, true);
            }
            else
            {
                curtida.Excluido = true;

                SalvarCurtida(curtida, true);
            }

            var objecto = new
            {
                model.Curtiu,
                NumeroCurtidas = postagem.Curtidas.Where(c => !c.Excluido).Count()
            };

            return objecto;
        }

        public IActionResult CurtirComentario(CurtirModel model)
        {

            return View(model);
        }

        public ActionResult Excluir(int id)
        {
            var postagem = BD.Postagens.FirstOrDefault(p => p.ID == id);
            postagem.Excluido = true;

            BD.Postagens.Update(postagem);

            BD.SaveChanges();

            if (postagem.PostagemPai != null)
            {
                MostrarFotoPerfil();

                var categorias = BD.Categorias.Where(c => c.Excluido == false).ToList();
                var modelTupleCategoriasPostagem = new Tuple<List<Categoria>, Postagem>(categorias, postagem);

                return View("Index", modelTupleCategoriasPostagem);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public void EditarPostagem([FromBody] PostagemModel conteudo)
        {
            var postagem = BD.Postagens.FirstOrDefault(post => post.ID == conteudo.ID);

            var dataAtual = DateTime.Now;

            postagem.Discussao = Discussao.Alterar(postagem.Discussao.ID, conteudo.Assunto, dataAtual);
            postagem.Categoria = Categoria.RetornarCategoriaPorId(conteudo.Categoria);
            postagem.ConteudoHtml = conteudo.ConteudoHtml;
            postagem.Conteudo = conteudo.Conteudo;
            postagem.DataCriacao = dataAtual;

            Postagem.Alterar(postagem);
        }

        public Usuario RetornarUsuarioLogado()
        {
            var claims = (ClaimsIdentity)User.Identity;

            return Usuario.RetornarUsuarioPorNomeUsuario(claims.Claims.Single(u => u.Type == "NomeUsuario").Value);
        }

        private string SalvarComentario(Postagem postagem)
        {
            BD.Postagens.Add(postagem);

            return BD.SaveChanges() > 0 ?
               "Sucesso ao adicionar comentário" : ViewBag.Mensagem = "Falha ao adicionar comentário";
        }

        private bool SalvarCurtida(Curtida curtida, bool alterar = false)
        {
            if (alterar)
                BD.Curtidas.Update(curtida);
            else
                BD.Curtidas.Add(curtida);

            return BD.SaveChanges() > 0 ? true : false;
        }

        private void MostrarFotoPerfil()
        {
            if (User.Identity.IsAuthenticated)
            {
                var claims = (ClaimsIdentity)User.Identity;
                var usuario = Usuario.RetornarUsuarioPorNomeUsuario(claims.Claims.Single(u => u.Type == "NomeUsuario").Value);

                var foto = usuario.Foto.FotoBase64;
                ViewData["foto"] = foto;
                ViewData["temNotificacao"] = usuario.NotificacoesRecebidas.Where(n => !n.Excluido).ToList().Count > 0;
            }
        }
    }

    public enum Idioma
    {
        ES,
        PT
    }
}