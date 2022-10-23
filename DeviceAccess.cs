
using System.Net.Http.Headers;
using System.Text.Json;
using Newtonsoft.Json;

namespace nestalarm
{
    public class DeviceAccess
    {
        private string projectId;
        private string accessToken;
        private string refreshToken;
        private string oauthClientSecret;
        private string oauthClientId; 

        public DeviceAccess(string accessToken, string refreshToken, string projectId, string oauthClientSecret, string oauthClientId)
        {
            this.accessToken = accessToken;
            this.refreshToken = refreshToken;
            this.projectId = projectId;
            this.oauthClientId = oauthClientId;
            this.oauthClientSecret = oauthClientSecret;
        }

        public async Task Authenticate()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var devicesUri = $"https://smartdevicemanagement.googleapis.com/v1/enterprises/{projectId}/devices";
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(devicesUri))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        await RefreshAccessToken();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
            }
        }

        public async Task RefreshAccessToken()
        {
            var refreshUri = $"https://www.googleapis.com/oauth2/v4/token?client_id={oauthClientId}&client_secret={oauthClientSecret}&refresh_token={refreshToken}&grant_type=refresh_token";
            HttpClient client = new HttpClient();
            try
            {
                StringContent content = new StringContent("");
                using (HttpResponseMessage response = await client.PostAsync(refreshUri, content))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic resp = JsonConvert.DeserializeObject(responseBody);

                    if (resp?.access_token != null) 
                    {
                        this.accessToken = resp.access_token;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
            }
        }
    }
}