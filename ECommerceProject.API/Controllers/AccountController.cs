using ECommerceProject.API.DataAccess;
using ECommerceProject.API.Entities;
using ECommerceProject.Core.Controllers;
using ECommerceProject.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyServices;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ECommerceProject.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles ="Admin")]
    public class AccountController : ControllerBase
    {
       
        private DataBaseContext _db;
        private IConfiguration _cfg;
        public AccountController(DataBaseContext dataBaseContext ,
            IConfiguration configuration)
        {
            _db = dataBaseContext;
            _cfg = configuration;
        }

        [HttpPost("merchant/applyment")]
        [ProducesResponseType(200, Type = typeof(Resp<ApplymentAccountResponseModel>))]
        [ProducesResponseType(400, Type = typeof(Resp<ApplymentAccountResponseModel>))]
        public IActionResult Applyment([FromBody] ApplymentAccountRequestModel model)
        {

            //if (ModelState.IsValid)
            //{
            Resp<ApplymentAccountResponseModel> response = new Resp<ApplymentAccountResponseModel>();
            model.Username = model.Username?.Trim().ToLower();
            
            if (_db.Accounts.Any(x => x.Username.ToLower() == model.Username))
            {
                response.AddError(nameof(model.Username), "Bu kullanıcı adı kullanılıyor.");
                return BadRequest(response);
                
            }
            else
            {
                Account account = new Account
                {
                    Username = model.Username,
                    Password = model.Password,
                    CompanyName = model.CompanyName,
                    ContactEmail = model.ContactEmail,
                    ContactName = model.ContactName,
                    Type = AccountType.Merchant,
                    IsApplyment = true
                };
                _db.Accounts.Add(account);
                _db.SaveChanges();
                ApplymentAccountResponseModel applymentAccountResponseModel = new ApplymentAccountResponseModel
                {
                    Id = account.Id,
                    Username = account.Username,
                    ContactName = account.ContactName,
                    CompanyName = account.CompanyName,
                    ContactEmail = account.ContactEmail,
                };

                account.Password = null;
                response.Data = applymentAccountResponseModel;

                return Ok(response);
            }
        }
    //}
            //Buraya startup da davranışı değiştirip hatayı kendim karşılamak istersem ihtiyacım olur.
            //List<string> errors = ModelState.Values.SelectMany(x => x.Errors.Select(p=>p.ErrorMessage)).ToList();

            //return BadRequest(ModelState);

        [HttpPost("register")]
        [ProducesResponseType(200, Type = typeof(Resp<RegisterResponseModel>))]
        [ProducesResponseType(400, Type = typeof(Resp<RegisterResponseModel>))]
        public IActionResult Register([FromBody]RegisterRequestModel model)
        {
            model.Username=model.Username.Trim().ToLower();
            Resp<RegisterResponseModel> response = new Resp<RegisterResponseModel>();
            if (_db.Accounts.Any(x=>x.Username.ToLower()==model.Username))
            {
               response.AddError(nameof(model.Username),"Bu kullanıcı adı zaten kullanılıyor.");
                return BadRequest(response);
            }
            else
            {
                Account account = new Account
                {
                    Username = model.Username,
                    Password = model.Password,
                    Type = AccountType.Member
                };
                _db.Accounts.Add(account);
                _db.SaveChanges();
                RegisterResponseModel data = new RegisterResponseModel
                {
                    Id = account.Id,
                    Username = account.Username,
                };
                response.Data = data;
                return Ok(response);
            }
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(200, Type = typeof(Resp<AuthenticateResponseModel>))]
        [ProducesResponseType(400, Type = typeof(Resp<AuthenticateResponseModel>))]
        public IActionResult Authenticate([FromBody] AuthenticateRequestModel model)
        {
            Resp<AuthenticateResponseModel> response = new Resp<AuthenticateResponseModel>();
            model.Username= model.Username.Trim().ToLower();
            Account account = _db.Accounts.SingleOrDefault
                (x=>x.Username.ToLower()==model.Username&& x.Password==model.Password);

            if (account!=null)
            {
                if (account.IsApplyment)
                {
                    response.AddError("*", "Henüz satıcı başvurusu tamamlanmadı.");
                    return BadRequest(response);
                }
                else
                {
                    //Token oluşturma

                    string key = _cfg["JwtOptions:Key"];
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim("id", account.Id.ToString()),
                        new Claim(ClaimTypes.Name,account.Username),
                        new Claim("type",((int)account.Type).ToString()),
                        new Claim(ClaimTypes.Role, account.Type.ToString()),
                    };
                    string token = TokenServices.GenerateToken(key, claims , DateTime.Now.AddDays(30));
                    AuthenticateResponseModel data = new AuthenticateResponseModel { Token = token };
                    response.Data = data;
                    return Ok(response);
                }
            }
            else
            {
                response.AddError("*", "Kullanıcı adı ya da şifre eşleşmiyor");
                return BadRequest(response);
            }
        }

       
    }
}
