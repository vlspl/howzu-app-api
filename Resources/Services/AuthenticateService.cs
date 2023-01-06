 using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VLS_API.Model;

namespace VLS_API.Services
{
    public class AuthenticateService :IAuthenticateService
    {
        DataAccessLayer DAL = new DataAccessLayer();
        private readonly AppSettings _appSettings;
        public AuthenticateService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }     
        public User Authenticate(string userName, string password)
        {
            User user = new User();
            DataTable dt = new DataTable();
            SqlParameter[] param = new SqlParameter[]
              {
                        new SqlParameter("@Username",userName),
                        new SqlParameter("@password",password)
              };
            dt = DAL.ExecuteStoredProcedureDataTable("Ws_sp_LoginMaster", param);

            // return null if user is not found
            if (dt.Rows.Count > 0)
            {
                // if User Found
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Key);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, dt.Rows[0]["UserId"].ToString()),
                        new Claim(ClaimTypes.Role, dt.Rows[0]["Role"].ToString()),
                        new Claim(ClaimTypes.Name, dt.Rows[0]["sFullName"].ToString()),
                        new Claim("UserId", dt.Rows[0]["UserId"].ToString()),
                      
                       new Claim("UserName", dt.Rows[0]["sFullName"].ToString()),
                          //new Claim("Org_Id", dt.Rows[0]["Org_Id"].ToString()),
                    //   new Claim(ClaimTypes.OrgId, dt.Rows[0]["Org_Id"].ToString()),
                       
                    }),
                    Expires = DateTime.UtcNow.AddDays(365),
                    Issuer = "http://localhost:49930/",
                    Audience = "http://localhost:49930/",
                    //Issuer = "http://endpoint.visionarylifesciences.in/",
                    //Audience = "http://endpoint.visionarylifesciences.in/",
                    //Issuer = " http://localhost:49930/",
                    //Audience = " http://localhost:49930/",
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
               
               var token = tokenHandler.CreateToken(tokenDescriptor);
                user.Token = tokenHandler.WriteToken(token);
                user.UserId = Convert.ToInt32(dt.Rows[0]["UserId"].ToString());
                user.Name = dt.Rows[0]["sFullName"].ToString();
                user.MobileVerified= Convert.ToBoolean(dt.Rows[0]["MobileVerified"].ToString());
                user.RegistrationStatus = Convert.ToBoolean(dt.Rows[0]["RegistrationStatus"].ToString());
                user.Role = dt.Rows[0]["Role"].ToString();
                user.EmailId = dt.Rows[0]["sEmailId"].ToString();
                user.OrgId= dt.Rows[0]["Org_Id"].ToString();

                return user;
            }
            else
            {
                return null;
            }
        }
    }
}
