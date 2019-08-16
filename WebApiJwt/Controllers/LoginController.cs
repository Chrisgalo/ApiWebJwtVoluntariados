using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApiJwt.Models;
using System.Web.Http.Description;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNet.Identity;

namespace WebApiJwt.Controllers
{
    public class LoginController : ApiController
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Login
        [ActionName("GET")]
        public IQueryable<UsuarioLogin> GetFundacions()
        {
            return db.usuarioLogin;
        }

        // GET: api/Login/username
        [ResponseType(typeof(UsuarioLogin))]
        [ActionName("GET/id")]
        public async Task<IHttpActionResult> GetUsuarioLogin(string id)
        {
            UsuarioLogin usuarioLogin = await db.usuarioLogin.FindAsync(id);
            if (usuarioLogin == null)
            {
                return NotFound();
            }

            return Ok(usuarioLogin);
        }

        // PUT: api/Login/username
        [ResponseType(typeof(void))]
        [ActionName("PUT")]
        public async Task<IHttpActionResult> PutUsuarioLogin(string id, UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != usuarioLogin.Usuario)
            {
                return BadRequest();
            }

            db.Entry(usuarioLogin).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioLoginExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        // DELETE: api/Login/username
        [ResponseType(typeof(UsuarioLogin))]
        [ActionName("DELETE")]
        
        public async Task<IHttpActionResult> DeleteUsuarioLogin(string id)
        {
            UsuarioLogin usuarioLogin = await db.usuarioLogin.FindAsync(id);
            if (usuarioLogin == null)
            {
                return NotFound();
            }

            db.usuarioLogin.Remove(usuarioLogin);
            await db.SaveChangesAsync();

            return Ok(usuarioLogin);
        }

        // POST: api/UsuarioLogins
        [ResponseType(typeof(UsuarioLogin))]
        [ActionName("POST")]
        public async Task<IHttpActionResult> PostUsuarioLogin(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.usuarioLogin.Add(usuarioLogin);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UsuarioLoginExists(usuarioLogin.Usuario))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = usuarioLogin.Usuario }, usuarioLogin);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsuarioLoginExists(string id)
        {
            return db.usuarioLogin.Count(e => e.Usuario == id) > 0;
        }

        [HttpPost]
        [ActionName("Authenticate")]
        public IHttpActionResult Login(string username = "", string password = "")
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                string query = "SELECT * FROM UsuarioLogins ";
                //	Se envió el usuario y la clave
                if (!(username.Equals("") && password.Equals("")))
                {
                    query = query + "WHERE Usuario = '" + username + "' AND Password = '" + password + "'";
                    var result = context.usuarioLogin.SqlQuery(query).FirstOrDefault();

                    if (result != null)
                    {
                        //	Generar JWT
                        return Ok(new { token = GenerarTokenJWT(result) });
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
        }


        // GENERAMOS EL TOKEN CON LA INFORMACIÓN DEL USUARIO
        private string GenerarTokenJWT(UsuarioLogin usuarioLogin)
        {
            // RECUPERAMOS LAS VARIABLES DE CONFIGURACIÓN
            var _ClaveSecreta = ConfigurationManager.AppSettings["ClaveSecreta"];
            var _Issuer = ConfigurationManager.AppSettings["Issuer"];
            var _Audience = ConfigurationManager.AppSettings["Audience"];
            int _Expires;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["Expires"], out _Expires))
                _Expires = 24;


            // CREAMOS EL HEADER //
            var _symmetricSecurityKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_ClaveSecreta));
            var _signingCredentials = new SigningCredentials(
                    _symmetricSecurityKey, SecurityAlgorithms.HmacSha256
                );
            var _Header = new JwtHeader(_signingCredentials);

            // CREAMOS LOS CLAIMS //
            var _Claims = new[] {
                
                new Claim("Usuario", usuarioLogin.Usuario),
                new Claim("Password", usuarioLogin.Password)
            };

            // CREAMOS EL PAYLOAD //
            var _Payload = new JwtPayload(
                    issuer: _Issuer,
                    audience: _Audience,
                    claims: _Claims,
                    notBefore: DateTime.Now,
                    // Exipra a la 24 horas.
                    expires: DateTime.Now.AddHours(_Expires)
                );

            // GENERAMOS EL TOKEN //
            var _Token = new JwtSecurityToken(
                    _Header,
                    _Payload
                );

            return new JwtSecurityTokenHandler().WriteToken(_Token);
        }

    }

    internal class ValidarTokenHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                               CancellationToken cancellationToken)
        {
            HttpStatusCode statusCode;
            string token;

            if (!TryRetrieveToken(request, out token))
            {
                statusCode = HttpStatusCode.Unauthorized;
                return base.SendAsync(request, cancellationToken);
            }

            try
            {
                var claveSecreta = ConfigurationManager.AppSettings["ClaveSecreta"];
                var issuerToken = ConfigurationManager.AppSettings["Issuer"];
                var audienceToken = ConfigurationManager.AppSettings["Audience"];

                var securityKey = new SymmetricSecurityKey(
                    System.Text.Encoding.Default.GetBytes(claveSecreta));

                SecurityToken securityToken;
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                TokenValidationParameters validationParameters = new TokenValidationParameters()
                {
                    ValidAudience = audienceToken,
                    ValidIssuer = issuerToken,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    // DELEGADO PERSONALIZADO PERA COMPROBAR
                    // LA CADUCIDAD EL TOKEN.
                    LifetimeValidator = this.LifetimeValidator,
                    IssuerSigningKey = securityKey
                };

                // COMPRUEBA LA VALIDEZ DEL TOKEN
                Thread.CurrentPrincipal = tokenHandler.ValidateToken(token,
                                                                     validationParameters,
                                                                     out securityToken);
                HttpContext.Current.User = tokenHandler.ValidateToken(token,
                                                                      validationParameters,
                                                                      out securityToken);

                return base.SendAsync(request, cancellationToken);
            }
            catch (SecurityTokenValidationException ex)
            {
                statusCode = HttpStatusCode.Unauthorized;
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.InternalServerError;
            }

            return Task<HttpResponseMessage>.Factory.StartNew(() =>
                        new HttpResponseMessage(statusCode) { });
        }

        // RECUPERA EL TOKEN DE LA PETICIÓN
        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;
            IEnumerable<string> authzHeaders;
            if (!request.Headers.TryGetValues("Authorization", out authzHeaders) ||
                                              authzHeaders.Count() > 1)
            {
                return false;
            }
            var bearerToken = authzHeaders.ElementAt(0);
            token = bearerToken.StartsWith("Bearer ") ?
                    bearerToken.Substring(7) : bearerToken;
            return true;
        }

        // COMPRUEBA LA CADUCIDAD DEL TOKEN
        public bool LifetimeValidator(DateTime? notBefore,
                                      DateTime? expires,
                                      SecurityToken securityToken,
                                      TokenValidationParameters validationParameters)
        {
            var valid = false;

            if ((expires.HasValue && DateTime.UtcNow < expires)
                && (notBefore.HasValue && DateTime.UtcNow > notBefore))
            { valid = true; }

            return valid;
        }

    }
}
