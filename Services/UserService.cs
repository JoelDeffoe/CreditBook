using CreditBook.Entities;
using CreditBook.Helpers;
using CreditBook.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace CreditBook.Services
{
    public interface IUserService
    {
        AutResponse Aut(AutRequest model);
        IEnumerable<User> GetAll();
        User GetById(int id);
    }

    public class UserService : IUserService
    {

        //public string ConnectionString { get; set; }

        /*private List<User> _users = new List<User>
        {

            new User { Id = 1, Username = "test", Email = "User", Password = "test" }
        };
        */

        private readonly List<User> _users;

        private readonly AppSettings _appSettings;

        private  List<User> FetchUsers()
        {
            List<User> _users = new List<User>();
            string queryString = "SELECT * FROM Users";
            using (SqlConnection con = new SqlConnection(_appSettings.Creadibookdb))
            {
                try
                {
                    con.Open();
                    SqlCommand com = new SqlCommand(queryString, con);
                    SqlDataReader reader = com.ExecuteReader();
                    while (reader.Read())
                    {
                        var User = new User();
                        User.Ids = reader[0].ToString();
                        User.Username = reader[1].ToString();
                        User.Email = reader[2].ToString();
                        User.Password = reader[3].ToString();

                        _users.Add(User);
                    }
                }
                catch(Exception e )
                {
                    Console.WriteLine(e.ToString());
                }
             
                return _users.ToList();
            }
        }
        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _users = FetchUsers();
        }

        public AutResponse Aut(AutRequest model)
        {
            var user = _users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);


            if (user == null)
            {
                return null;
            }
            
            var token = generateJwtToken(user);

            return new AutResponse(user, token);
        }

        public IEnumerable<User> GetAll()
        {
            return _users;
        }

        public User GetById(int id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }

        private string generateJwtToken(User user)
        {
            // generate token valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
