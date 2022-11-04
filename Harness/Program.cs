/******************************************************************************/
/* Copyright 2022 Keyfactor                                                   */
/* Licensed under the Apache License, Version 2.0 (the "License"); you may    */
/* not use this file except in compliance with the License.  You may obtain a */
/* copy of the License at http://www.apache.org/licenses/LICENSE-2.0.  Unless */
/* required by applicable law or agreed to in writing, software distributed   */
/* under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES   */
/* OR CONDITIONS OF ANY KIND, either express or implied. See the License for  */
/* the specific language governing permissions and limitations under the      */
/* License.                                                                   */
/******************************************************************************/

using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

using EJBCA;

namespace Harness
{
    class Program
    {
        static void Main(string[] args)
        {
            // Environment config
            string baseURL = "https://192.168.40.132/ejbca/ejbca-rest-api";
            string Ca_name = "testCA";
            string End_entity_profile_name = "JDK";
            string Subject_dn = "CN=jdk";
            string clientAuthCertPath = "C:\\certs\\ejbca-client-cert.pfx";
            string clientAuthCertPassword = "";

            // Auto-generated values for end entity to request certificate
            string username = $"CSharp-Client{new Random().NextDouble()}";
            string password = $"CSharp-Client{new Random().NextDouble()}";

            // Disable server certificate validation. Insecure - testing purposes only.
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            
            // Set up EJBCA HTTP client
            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new X509Certificate2(clientAuthCertPath, clientAuthCertPassword));
            HttpClient httpclient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseURL)
            };
            Client c = new Client(httpclient);

            // Add a new end entity for the certificate request
            AddEndEntityRestRequest addReq = new AddEndEntityRestRequest()
            {
                Username = username,
                Password = password,
                Ca_name = Ca_name,
                End_entity_profile_name = End_entity_profile_name,
                Certificate_profile_name = "ENDUSER",
                Subject_dn = Subject_dn,
                Token = AddEndEntityRestRequestToken.USERGENERATED
            };
            c.AddAsync(addReq).Wait();

            // Request new certificate
            CertificateRequestRestRequest req = new CertificateRequestRestRequest()
            {
                Certificate_authority_name = Ca_name,
                Include_chain = true,
                Username = username,
                Password = password,
                Certificate_request = "MII..."
            };
            CertificateRestResponse resp = c.CertificateRequestAsync(req).Result;
            Console.WriteLine(resp.Certificate);
        }
    }
}
