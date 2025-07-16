using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnvifCameraManager.Services
{
    public class CredentialManager
    {
        public string InsertCredentialsIntoRtspUrl(string originalUri, string username, string password)
        {
            var uriBuilder = new UriBuilder(originalUri)
            {
                UserName = Uri.EscapeDataString(username),
                Password = Uri.EscapeDataString(password)
            };
            return uriBuilder.ToString();
        }

    }
}
