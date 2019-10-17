﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FIRSTShares.Data;
using FIRSTShares.Entities;
using FIRSTShares.Models;
using FIRSTShares.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FIRSTShares.Controllers
{
    public class AccountController : Controller
    {

        private readonly LazyContext BD;
        private Usuario Usuario;

        public AccountController(LazyContext bd)
        {
            BD = bd;
            Usuario = new Usuario(BD);
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarUsuario(Usuario usuario, string confirmaSenha,
            string numero, string sobrenome, IFormFile foto)
        {
            if (usuario.Senha == confirmaSenha)
                usuario.Senha = Criptografia.Codifica(usuario.Senha);
            else
            {
                ViewBag.Mensagem = "As senhas devem ser iguais.";

                return View("Register");
            }

            if (Usuario.ChecarSeEmailOuUsuarioEstaCadastrado(usuario))
            {
                ViewBag.Mensagem = "Este e-mail ou usuário já está cadastrado.";

                return View("Register");
            }

            var fotoBytes = AdicionarUsuarioFoto(foto);
            // fotoBytes = ResizeImage(byteArrayToImage(fotoBytes), 400, 400);

            var usuarioDb = new Usuario
            {
                Nome = string.Format("{0} {1}", usuario.Nome, sobrenome),
                Email = usuario.Email,
                Senha = usuario.Senha,
                Time = RetornarTime(numero),
                CargoTime = usuario.CargoTime,
                DataCriacao = DateTime.Now,
                Cargo = BD.Cargos.ToList().Find(cargo => cargo.Tipo == CargoTipo.Usuario),
                NomeUsuario = usuario.NomeUsuario,
                Foto = new Foto { FotoBase64 = (fotoBytes == null) ? (BD.Fotos.SingleOrDefault(ft => ft.ID == 2)).FotoBase64 : fotoBytes }
            };

            SalvarUsuario(usuarioDb);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim("NomeUsuario", usuario.NomeUsuario),
                    new Claim("CargoUsuario", CargoTipo.Usuario.ToString())
                };

            var userIdentity = new ClaimsIdentity(claims, "login");
            var principal = new ClaimsPrincipal(userIdentity);

            Thread.CurrentPrincipal = principal;

            await HttpContext.SignInAsync(principal);

            return RedirectToAction("Index", "Home");
        }

        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            var ms = new MemoryStream(byteArrayIn);
            var returnImage = Image.FromStream(ms);

            return returnImage;
        }

        public byte[] AdicionarUsuarioFoto(IFormFile foto)
        {
            if(foto == null)
                return null;

            using (var ms = new MemoryStream())
            {
                foto.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public byte[] ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            using(var destImage = new Bitmap(width, height)){

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                var ms = new MemoryStream();
                destImage.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);

                return ms.ToArray();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AcessarConta(UsuarioViewModel usuarioModel)
        {
            var usuario = RetornarUsuarioPorEmailOuUsuario(usuarioModel.UsuarioEmail);
            if (usuario != null)
            {
                if (LoginUsuario(usuario, usuarioModel.Senha))
                {
                    var notificacao = usuario.NotificacoesRecebidas.Where(n => !n.Excluido).Count() > 0 ? "Tem" : "NaoTem";

                    var nomeUsuario = usuario.NomeUsuario;
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuario.Nome),
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim("NomeUsuario", nomeUsuario),
                        new Claim("CargoUsuario", usuario.Cargo.Tipo.ToString()),
                        new Claim("TemNotificacao", notificacao)
                    };


                    var userIdentity = new ClaimsIdentity(claims, "login");
                    var principal = new ClaimsPrincipal(userIdentity);

                    Thread.CurrentPrincipal = principal;

                    await HttpContext.SignInAsync(principal);

                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Mensagem = "Senha e/ou e-mail incorretos!";

            return View("Login");
        }

        public bool LoginUsuario(Usuario usuario, string senha)
        {
            return ((usuario == null) || (!Criptografia.Compara(usuario.Senha, senha))) ? false : true;
        }

        private Usuario RetornarUsuarioPorEmailOuUsuario(string emailUsuario)
        {
            return BD.Usuarios
                .SingleOrDefault(usuario =>
                ((usuario.Email == emailUsuario) || (usuario.NomeUsuario == emailUsuario)) && (usuario.Excluido == false)
                );
        }

        private string SalvarUsuario(Usuario usuario)
        {
            BD.Usuarios.Add(usuario);
            if (BD.SaveChanges() > 0)
                return ViewBag.Mensagem = "Usuário cadastrado com sucesso!";

            return ViewBag.Mensagem = "Falha ao cadastrar usuário.";
        }

        private Time RetornarTime(string numero)
        {
            if (numero != null)
            {
                var time = new Time();
                var retornarTime = time.RetornarTimePorNumero(numero, BD.Times.ToList());
                var timeTheBlueAlliance = time.RetornarTimeTheBlueAlliance(numero);
                var timeExiste = time.ChecarSeTimeExiste(timeTheBlueAlliance);

                if (retornarTime == null && timeExiste)
                {

                    time.Nome = timeTheBlueAlliance.Nome;
                    time.Numero = numero;
                    time.Pais = timeTheBlueAlliance.Pais;
                    time.CodPais = time.RetornarCodPais(time.Pais);

                    return time;
                }
                else if (!timeExiste)
                    return null;

                return retornarTime;
            }

            return null;
        }

        private string RetornarFoto(string nomeUsuario)
        {
            var folderName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/usuarios");
            var imgPadrao = "user.png";

            foreach (var files in Directory.GetFiles(folderName))
            {
                var info = new FileInfo(files);
                var fileName = Path.GetFileNameWithoutExtension(info.Name);

                if (fileName == nomeUsuario)
                    return Path.GetFileName(info.FullName);
            }

            return imgPadrao;
        }

        private string SalvarFoto(IFormFile foto, string nomeUsuario)
        {
            if (foto != null)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/usuarios", nomeUsuario + Path.GetExtension(foto.FileName));

                if (foto.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        foto.CopyTo(stream);
                        stream.Flush();
                    }
                }

                return nomeUsuario + Path.GetExtension(foto.FileName);
            }

            return "user.png";
        }
    }
}