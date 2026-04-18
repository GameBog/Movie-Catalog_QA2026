using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using Movie_CatalogQA2026.Models;

namespace Movie_CatalogQA2026
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string lastCreateadMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI0NjkyYzAzMS00OTQ3LTQyMTQtOGMzMC1mZWQ2Mjg1NDlhMDgiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjI0OjM3IiwiVXNlcklkIjoiZGJjM2U2MGMtMjlmYi00ZmM5LTYyNGYtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJxYTEyM3FhQGV4YW1wbGUuY29tIiwiVXNlck5hbWUiOiJxYTEyM3FhIiwiZXhwIjoxNzc2NTE1MDc3LCJpc3MiOiJNb3ZpZUNhdGFsb2dfQXBwX1NvZnRVbmkiLCJhdWQiOiJNb3ZpZUNhdGFsb2dfV2ViQVBJX1NvZnRVbmkifQ.xxa47bHd3Ij8bC2oEvBwyMFKJ0QlJj0rvMpn0Ff1juM";
        private const string LoginEmail = "qa123qa@example.com";
        private const string LoginPassword = "113355";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateMovie_WithRequiredFIelds_ShouldReturnSuccess()
        {
            var movieData = new MovieDTO
            {
                Title = "Test Movie",
                Description = "This is a test movie description",

            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(createResponse.Msg, Is.EqualTo("Movie created successfully!"));
            Assert.That(createResponse.Movie, Is.Not.Null, "Movie object should be returned.");
            Assert.That(createResponse, Is.Not.Null, "Response should not be null.");
            lastCreateadMovieId = createResponse.Movie.Id;

        }

        [Order(2)]
        [Test]

        public void EditExistingMovie_ShouldReturnSuccess()
        {
            var editRequestData = new MovieDTO
            {
                Title = "Edited Movie",
                Description = "This is a edited movie description",
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);

            request.AddQueryParameter("movieId", lastCreateadMovieId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseItems, Is.Not.Empty);
            Assert.That(responseItems, Is.Not.Null);

        }

        [Order(4)]
        [Test]

        public void DeleteMovie_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", lastCreateadMovieId);
            var response = this.client.Execute(request);

            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(deleteResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            var movieData = new MovieDTO
            {
                Title = "",
                Description = "This is a edited movie description",
            };
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }

        [Order(6)]
        [Test]

        public void EditNonExistingMovie_ShouldReturnNotFound()
        {
            var editRequestData = new MovieDTO
            {
                Title = "Edited Movie",
                Description = "This is a edited movie description",
            };
            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", lastCreateadMovieId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(editResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingMovie_ShouldReturnNotFound()
        {

            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", lastCreateadMovieId);
            var response = this.client.Execute(request);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(deleteResponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}