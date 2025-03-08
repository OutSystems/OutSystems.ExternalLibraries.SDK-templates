using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace mTLSclient {
    internal class ExampleClientBase {
        private static readonly string ClientCertificatePath;
        private static readonly string ClientCertificatePassword;

        private static bool ValidateServerCertificate(HttpRequestMessage request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors errors) {
            // Perform custom server certificate validation if required
            // Return true if the certificate is trusted, false otherwise
            return true;
        }

        protected Task<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken) {
            // Load the client certificate
            if(!File.Exists(ClientCertificatePath)) {
                throw new FileNotFoundException($"Could not find the file {ClientCertificatePath}");
            }
            var clientCertificate = new X509Certificate2(ClientCertificatePath, ClientCertificatePassword);

            // Create an HttpClient with MTLS authentication
            var httpClient = new HttpClient(new HttpClientHandler {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                SslProtocols = SslProtocols.Tls12,
                ServerCertificateCustomValidationCallback = ValidateServerCertificate,
                ClientCertificates = { clientCertificate }
            });
            return Task.FromResult(httpClient);
        }

    }
}
