using SharePhotos.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace SharePhotos.API.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration configuration;

        public CuentasController(UserManager<IdentityUser> userManager,
                    SignInManager<IdentityUser> siginManager,
                    IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = siginManager;
            this.configuration = configuration;
        }

        
        [HttpPost("registro")]
        public async Task<ActionResult<UserToken>> RegistroUsuario([FromBody] UserInfo userInfo)
        {
            var user = new IdentityUser
            {
                UserName = userInfo.UserName,
                Email = userInfo.Email
            };

            var result = await userManager.CreateAsync(user, userInfo.Password);

            if (result.Succeeded)
            {
                return BuildToken(userInfo);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo model)
        {
            var result = await signInManager.PasswordSignInAsync(
                model.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return BuildToken(model);
            }
            else
            {
                return BadRequest("Login Fallido");
            }
        }

        private UserToken BuildToken(UserInfo userInfo)
        {
            //Buscamos primero las propiedades de nustro token
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userInfo.UserName), //Claim tiene nombre e identificador
                //new Claim(ClaimTypes.Email, userInfo.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtkey"])); //Llamamos la llave
            var credential = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(1); //Se puede cambiar a otros tiempos

            JwtSecurityToken token = new JwtSecurityToken(
                                   issuer: null,
                                   audience: null,
                                   claims: claims,
                                   expires: expiration,
                                   signingCredentials: credential);

            return new UserToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
