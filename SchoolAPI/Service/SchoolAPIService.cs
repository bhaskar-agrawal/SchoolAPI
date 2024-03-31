using Azure.Core;
using Microsoft.IdentityModel.Tokens;
using SchoolAPI.DAL;
using SchoolAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SchoolAPI.Service
{
    public class SchoolAPIService
    {
        private IDataClient dataClient;
        private readonly IConfiguration configuration;
        public SchoolAPIService(IDataClient dataClient, IConfiguration configuration)
        {
            this.dataClient = dataClient;
            this.configuration = configuration;
        }

        public async Task CreateUserAsync(string userName, string password, string role)
        {
            Task<bool> userPresenceValidationTask = this.dataClient.IsUserRegisteredAsync(userName);
            if (await userPresenceValidationTask)
            {
                throw new Exception("409 UserAlreadyExists");
            }
            await this.dataClient.CreateUserAsync(userName, password, role);
        }

        private string GenerateJSONWebToken(string userName, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                configuration["Authentication:SecretForKey"]));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claimsForToken = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            };
            var token = new JwtSecurityToken(
                               claims: claimsForToken,
                               expires: DateTime.Now.AddMinutes(60),
                               signingCredentials: signingCredentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }

        public async Task<string?> ValidateUserSigninAsync(string userName, string password)
        {
            bool userPresent = await dataClient.IsUserRegisteredAsync(userName);
            if (!userPresent)
            {
                throw new Exception("404 UserNotFound");
            }
            Task<string> roleTask = dataClient.ValidateUserSigninAsync(userName, password);
            string role = await roleTask;
            return GenerateJSONWebToken(userName, role);
        }

        public async Task UpdateMarksForStudentAsync(string studentName, string subjectName, int marks)
        {
            Task studentSubjectValidation = dataClient.CheckIfStudentSubjectPresentAsync(studentName, subjectName);
            await studentSubjectValidation;
            await dataClient.UpdateMarksForStudentAsync(studentName, subjectName, marks);  
        }

        public async Task<List<Subject>> GetMarksForStudentAsync(string studentName)
        {
            bool studentPresent = await dataClient.IsUserRegisteredAsync(studentName);
            if (!studentPresent)
            {
                throw new Exception("404 StudentNotFound");
            }
            List<Subject> subjectsOfStudent = await dataClient.GetMarksForStudentAsync(studentName);
            return subjectsOfStudent;
        }

    }
}
