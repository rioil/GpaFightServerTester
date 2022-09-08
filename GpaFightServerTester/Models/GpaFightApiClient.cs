using RestSharp;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GpaFightServerTester.Models
{
    internal class GpaFightApiClient
    {
        private const string BaseUrl = "https://api.gpa-fight.server-on.net/v1/";
        //private const string BaseUrl = "http://localhost:8080/";

        private readonly RestClient _restClient = new(BaseUrl);
        private readonly JsonSerializerOptions _serializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public string? Token { get; set; }

        public async Task<User?> CreateUser(User user)
        {
            var result = await CreateUserInternal(user);

            return result.IsSuccessful ? result.Data : null;
        }

        public async Task<string?> CreateUserRaw(User user)
        {
            var result = await CreateUserInternal(user);

            return result.Content;
        }

        public async Task<User?> GetUser(string id)
        {
            var result = await GetUserInternal(id);

            return result.IsSuccessful ? result.Data : null;
        }

        public async Task<string?> GetUserRaw(string id)
        {
            var result = await GetUserInternal(id);

            return result.Content;
        }

        public async Task<bool> UpdateUser(User user)
        {
            var result = await UpdateUserInternal(user);

            return result.IsSuccessful;
        }

        public async Task<string?> UpdateUserRaw(User user)
        {
            var result = await UpdateUserInternal(user);

            return result.Content;
        }

        public async Task<bool> Login(LoginCredential loginCredential)
        {
            var request = new RestRequest("login");
            var serializedBody = JsonSerializer.Serialize(loginCredential, _serializerOptions);
            request.AddJsonBody(serializedBody);

            var result = await _restClient.ExecutePostAsync(request);

            if (result.IsSuccessful)
            {
                Token = result.Headers?.FirstOrDefault(header => string.Equals(header.Name, "X-Auth-Token", StringComparison.OrdinalIgnoreCase))?.Value as string;
            }

            return result.IsSuccessful;
        }

        public async Task<bool> UpdateGrade(Grade grade)
        {
            if (Token is null) { throw new InvalidOperationException(); }

            var request = new RestRequest("grades");
            request.AddHeader("Authorization", $"Bearer {Token}");
            var serializedBody = JsonSerializer.Serialize(grade, _serializerOptions);
            request.AddJsonBody(serializedBody);

            var result = await _restClient.ExecutePostAsync(request);

            return result.IsSuccessful;
        }

        public async Task<Ranking?> GetWholeRanking()
        {
            var result = await GetWholeRankingInternal();
            
            return result.IsSuccessful ? result.Data : null;
        }

        public async Task<string?> GetWholeRankingRaw()
        {
            var result = await GetWholeRankingInternal();
            return result.Content;
        }

        public async Task<Ranking?> GetAffiliationRanking()
        {
            var result = await GetAffiliationRankingInternal();

            return result.IsSuccessful ? result.Data : null;
        }

        public async Task<string?> GetAffiliationRankingRaw()
        {
            var result = await GetAffiliationRankingInternal();
            return result.Content;
        }

        #region private function
        private async Task<RestResponse<User>> CreateUserInternal(User user)
        {
            var request = new RestRequest("users");
            var serializedBody = JsonSerializer.Serialize(user, _serializerOptions);
            request.AddJsonBody(serializedBody);

            var result = await _restClient.ExecutePostAsync<User>(request);

            return result;
        }

        private async Task<RestResponse<User>> GetUserInternal(string id)
        {
            if (Token is null) { throw new InvalidOperationException(); }

            var request = new RestRequest($"users/{id}");
            request.AddHeader("Authorization", $"Bearer {Token}");

            var result = await _restClient.ExecuteGetAsync<User>(request);

            return result;
        }

        private async Task<RestResponse> UpdateUserInternal(User user)
        {
            var request = new RestRequest($"users/{user.Username}");
            var serializedBody = JsonSerializer.Serialize(user, _serializerOptions);
            request.AddJsonBody(serializedBody);

            var result = await _restClient.ExecutePostAsync(request);

            return result;
        }

        private async Task<RestResponse<Ranking>> GetWholeRankingInternal()
        {
            if (Token is null) { throw new InvalidOperationException(); }

            var request = new RestRequest("rankings/gpa/whole");
            request.AddHeader("Authorization", $"Bearer {Token}");

            return await _restClient.ExecuteGetAsync<Ranking>(request);
        }

        private async Task<RestResponse<Ranking>> GetAffiliationRankingInternal()
        {
            if (Token is null) { throw new InvalidOperationException(); }

            var request = new RestRequest("rankings/gpa/affiliation");
            request.AddHeader("Authorization", $"Bearer {Token}");

            return await _restClient.ExecuteGetAsync<Ranking>(request);
        }
        #endregion
    }
}
