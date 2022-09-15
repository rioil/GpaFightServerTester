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

        public User? CurrentUser { get; set; }

        public async Task<User?> CreateUser(User user)
        {
            var result = await CreateUserInternal(user);

            return result.IsSuccessful ? result.Data : null;
        }

        public async Task<RestResponse?> CreateUserRaw(User user)
        {
            var result = await CreateUserInternal(user);
            return result;
        }

        public async Task<User?> GetUser(string id)
        {
            var result = await GetUserInternal(id);

            return result.IsSuccessful ? result.Data : null;
        }

        public async Task<RestResponse?> GetUserRaw(string id)
        {
            var result = await GetUserInternal(id);

            return result;
        }

        public async Task<bool> UpdateUser(User user)
        {
            var result = await UpdateUserInternal(user);

            return result.IsSuccessful;
        }

        public async Task<RestResponse?> UpdateUserRaw(User user)
        {
            var result = await UpdateUserInternal(user);

            return result;
        }

        public async Task<bool> DeleteUser(User user)
        {
            var result = await DeleteUserInternal(user);

            return result.IsSuccessful;
        }

        public async Task<RestResponse?> DeleteUserRaw(User user)
        {
            return await DeleteUserInternal(user);
        }

        public async Task<bool> GetCurrentUser()
        {
            await GetCurrentUserInternal();

            return CurrentUser is not null;
        }

        public async Task<RestResponse<User>?> GetCurrentUserRaw()
        {
            return await GetCurrentUserInternal();
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

        public async Task<RankingItem[]?> GetWholeRanking()
        {
            var result = await GetWholeRankingInternal();

            return result.IsSuccessful ? result.Data : null;
        }

        public async Task<RestResponse<RankingItem[]>> GetWholeRankingRaw()
        {
            var result = await GetWholeRankingInternal();
            return result;
        }

        public async Task<RankingItem[]?> GetAffiliationRanking()
        {
            var result = await GetAffiliationRankingInternal();

            return result.IsSuccessful ? result.Data : null;
        }

        public async Task<RestResponse<RankingItem[]>> GetAffiliationRankingRaw()
        {
            var result = await GetAffiliationRankingInternal();
            return result;
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
            if (Token is null) { throw new InvalidOperationException(); }

            var request = new RestRequest($"users/{user.Id}");
            request.AddHeader("Authorization", $"Bearer {Token}");
            var serializedBody = JsonSerializer.Serialize(user, _serializerOptions);
            request.AddJsonBody(serializedBody);

            var result = await _restClient.ExecutePostAsync(request);

            return result;
        }

        private async Task<RestResponse> DeleteUserInternal(User user)
        {
            if (Token is null) { throw new InvalidOperationException(); }

            var request = new RestRequest($"users/{user.Id}", Method.Delete);
            request.AddHeader("Authorization", $"Bearer {Token}");
            var result = await _restClient.ExecuteAsync(request);

            return result;
        }

        private async Task<RestResponse<User>?> GetCurrentUserInternal()
        {
            if (Token is null) { throw new InvalidOperationException(); }

            var request = new RestRequest("users/current");
            request.AddHeader("Authorization", $"Bearer {Token}");
            var result = await _restClient.ExecuteGetAsync<User>(request);

            CurrentUser = result?.Data;

            return result;
        }

        private async Task<RestResponse<RankingItem[]>> GetWholeRankingInternal()
        {
            if (Token is null) { throw new InvalidOperationException(); }

            var request = new RestRequest("rankings/gpa/whole");
            request.AddHeader("Authorization", $"Bearer {Token}");

            return await _restClient.ExecuteGetAsync<RankingItem[]>(request);
        }

        private async Task<RestResponse<RankingItem[]>> GetAffiliationRankingInternal()
        {
            if (Token is null) { throw new InvalidOperationException(); }

            var request = new RestRequest("rankings/gpa/affiliation");
            request.AddHeader("Authorization", $"Bearer {Token}");

            return await _restClient.ExecuteGetAsync<RankingItem[]>(request);
        }
        #endregion
    }
}
