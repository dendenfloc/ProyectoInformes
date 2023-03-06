using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Dropbox.Api;

namespace ProyectoSonia.Models
{
    public static class GetAuthorizationCodeAsync
    {
        public static async Task<string> GetAuthorizationCode()
        {
            // Construir la URL de autorización
            var authorizeUrl = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, "rnwla4y023r4da0", "https://localhost:7035");

            // Enviar solicitud GET a la URL de autorización
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(authorizeUrl);

            // Analizar la respuesta para extraer el código de autorización
            var responseContent = await response.Content.ReadAsStringAsync();
            var authorizationCode = responseContent.Split('=')[1];

            return authorizationCode;
        }
    }
}