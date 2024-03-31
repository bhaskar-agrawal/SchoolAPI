using SchoolAPI.Models;

namespace SchoolAPI.DAL
{
    public interface IDataClient
    {

        public Task<bool> IsUserRegisteredAsync(string userName);

        public Task CreateUserAsync(string userName, string password, string role);

        public Task<string> ValidateUserSigninAsync(string userName, string password);

        public Task CheckIfStudentSubjectPresentAsync(string studentName, string subjectName);

        public Task UpdateMarksForStudentAsync(string studentName, string subjectName, int marks);

        public Task<List<Subject>> GetMarksForStudentAsync(string studentName);

    }
}
