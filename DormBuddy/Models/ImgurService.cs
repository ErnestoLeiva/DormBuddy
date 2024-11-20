using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace DormBuddy.Models {

    public class ImgurService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;

        public ImgurService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _clientId = configuration["Imgur:ClientId"];
        }

        public async Task<string> UploadImageAsync(byte[] imageBytes)
        {
            var base64Image = Convert.ToBase64String(imageBytes);

            var requestContent = new MultipartFormDataContent
            {
                { new StringContent(base64Image), "image" }
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", _clientId);
            var response = await _httpClient.PostAsync("https://api.imgur.com/3/image", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to upload image to Imgur");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(jsonResponse)["data"];
            return data["link"].ToString();
        }
    }

}