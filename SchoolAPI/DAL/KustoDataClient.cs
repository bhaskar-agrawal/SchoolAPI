using Azure;
using Kusto.Cloud.Platform.Utils;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Exceptions;
using Kusto.Data.Ingestion;
using Kusto.Data.Net.Client;
using Kusto.Ingest;
using SchoolAPI.Models;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SchoolAPI.DAL
{
    public class KustoDataClient : IDataClient
    {
        private string databaseName;
        private ICslAdminProvider kustoAdminClient;
        private IKustoIngestClient kustoIngestClient;
        private ICslQueryProvider kustoQueryClient;

        public KustoDataClient(IConfiguration configuration, KustoAuthDetails kustoAppDetails)
        {
            string kustoUri = configuration["Kusto:ClusterUri"];
            string appId = kustoAppDetails.AppId;
            string appKey = kustoAppDetails.AppKey;
            string authority = configuration["Kusto:TenantId"];
            var kcsb = new KustoConnectionStringBuilder(kustoUri).WithAadApplicationKeyAuthentication(appId, appKey, authority);

            this.databaseName = configuration["Kusto:DatabaseName"];

            this.kustoAdminClient = KustoClientFactory.CreateCslAdminProvider(kcsb);
            this.kustoIngestClient = KustoIngestFactory.CreateStreamingIngestClient(kcsb);
            this.kustoQueryClient = KustoClientFactory.CreateCslQueryProvider(kcsb);
        }

        private MemoryStream ConvertObjectToStream(object obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
            MemoryStream stream = new MemoryStream(jsonUtf8Bytes);
            return stream;
        }

        public async Task<bool> IsUserRegisteredAsync(string userName)
        {
            string queryUserCount = $"Users | where UserName == '{userName}' | count";
            var clientRequestProperties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };
            var reader = await this.kustoQueryClient.ExecuteQueryAsync(this.databaseName, queryUserCount, clientRequestProperties);
            long count = 0;
            while (reader.Read())
            {
                count = reader.GetInt64(0);
            }
            return count > 0;
        }

        public async Task CreateUserAsync(string userName, string password, string role)
        {
            User user = new User
            {
                UserName = userName,
                Password = password,
                Role = role
            };
            MemoryStream userStream = ConvertObjectToStream(user);
            var ingestionprops = new KustoIngestionProperties(databaseName, "Users")
            {
                Format = DataSourceFormat.json,
                IngestionMapping = new IngestionMapping()
                {
                    IngestionMappingKind = IngestionMappingKind.Json,
                    IngestionMappingReference = "UserMappingSchoolAPI"
                }
            };
            try
            {
                await this.kustoIngestClient.IngestFromStreamAsync(userStream, ingestionprops);
            }
            catch (Exception ex)
            {
                throw new Exception($"500 {ex.Message}");
            } 
        }

        public async Task<string> ValidateUserSigninAsync(string userName, string password)
        {
            string queryForUserAuth = $"Users | where UserName == '{userName}' and Password == '{password}' | project Role";
            var clientRequestProperties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };
            IDataReader reader;
            reader = await this.kustoQueryClient.ExecuteQueryAsync(this.databaseName, queryForUserAuth, clientRequestProperties);
            string? role = null;
            while (reader.Read())
            {
                role = reader.GetString(0);
            }
            if(role == null)
            {
                throw new Exception("401 InvalidPassword");
            }
            return role;
            
        }

        public async Task CheckIfStudentSubjectPresentAsync(string studentName, string subjectName)
        {
            string query = $"Subjects | where StudentName == '{studentName}' and SubjectName == '{subjectName}' | count";
            var clientRequestProperties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };
            var reader = await this.kustoQueryClient.ExecuteQueryAsync(this.databaseName, query, clientRequestProperties);
            long count = 0;
            while (reader.Read())
            {
                count = reader.GetInt64(0);
            }
            if (count > 0)
            {
                throw new Exception("409 MarksAlreadyExists");
            }
        }

        public async Task UpdateMarksForStudentAsync(string studentName, string subjectName, int marks)
        {
            SubjectEntity subjectOfStudentWithMarks = new SubjectEntity
            {
                UserName = studentName,
                SubjectName = subjectName,
                Marks = marks
            };
            MemoryStream streamOfSubjectOfStudent =  ConvertObjectToStream(subjectOfStudentWithMarks);
            var ingestionProps = new KustoIngestionProperties(this.databaseName, "Subjects")
            {
                Format = DataSourceFormat.json,
                IngestionMapping = new IngestionMapping()
                {
                    IngestionMappingKind = IngestionMappingKind.Json,
                    IngestionMappingReference = "SubjectMappingSchoolAPI"
                }
            };
            await this.kustoIngestClient.IngestFromStreamAsync(streamOfSubjectOfStudent, ingestionProps);
        }

        public async Task<List<Subject>> GetMarksForStudentAsync(string studentName)
        {
            string queryToGetMarksForStudent = $"Subjects | where  StudentName == '{studentName}'";
            var clientRequestProperties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };
            var reader = await this.kustoQueryClient.ExecuteQueryAsync(this.databaseName, queryToGetMarksForStudent, clientRequestProperties);
            List<Subject> subjectsOfStudent = new List<Subject>();
            while (reader.Read())
            {
                subjectsOfStudent.Add(new Subject(reader.GetString(1), reader.GetInt32(2)));
            }
            return subjectsOfStudent;
        }
    }
}
