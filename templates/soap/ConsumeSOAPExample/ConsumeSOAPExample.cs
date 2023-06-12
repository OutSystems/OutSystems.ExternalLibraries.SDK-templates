using System.Xml;
using System.ServiceModel;
using ServiceReference;
using OutSystems.ExternalLibraries.SDK;
using ConsumeSOAPExample.Structures;

namespace ConsumeSOAPExample{
    /// <summary>
    /// The ICalculator interface defines the methods (exposed as server actions)
    /// for basic mathematical operations. These server actions are essentially
    /// SOAP-based web service operations exposed in a .NET context.
    /// </summary>
    [OSInterface(Description = "Consume a Free SOAP web service in OutSystems Developer Cloud (ODC).", IconResourceName = "ConsumeSOAPExample.Resources.SOAP.jpg", Name = "SOAP")]
    public interface ICalculator
    {
        /// <summary>
        /// Takes two numbers as input parameters and returns their sum. This is done by
        /// sending a SOAP request to the web service, which then processes the addition 
        /// and returns the result in the SOAP response.
        /// This method is exposed as a server action to your ODC apps and libraries.
        /// </summary>
        [OSAction(Description = "The sum method takes two numbers as input parameters and returns their sum.", IconResourceName = "ConsumeSOAPExample.Resources.SOAP.jpg", ReturnName = "sum")]
        int Sum(
            [OSParameter(Description = "Number A & B as an integer")] Structures.Numbers numbers
        );
        /// <summary>
        /// Takes two numbers as input parameters and returns the difference between 
        /// the first number and the second number. This is done by sending a SOAP request
        /// to the web service, which then processes the subtraction and returns the result 
        /// in the SOAP response.
        /// This method is exposed as a server action to your ODC apps and libraries.
        /// </summary>
        [OSAction(Description = "The subtraction method takes two numbers as input parameters and returns the difference between the first number and the second number.", IconResourceName = "ConsumeSOAPExample.Resources.SOAP.jpg", ReturnName = "sub")]
        int Subtract(
            [OSParameter(Description = "Number A & B as an integer")] Structures.Numbers numbers
        );
    }
    public class Calculator : ICalculator
    {
        // URL for the SOAP web service 
        private static readonly string ENDPOINTADDRESS = "https://ecs.syr.edu/faculty/fawcett/Handouts/cse775/code/calcWebService/Calc.asmx";
        private static readonly int TIMEOUT = 1000;
        private readonly BasicHttpBinding _binding;
        private readonly EndpointAddress _address;

        public Calculator()
        {
            // Configure SOAP service settings. These settings determine how the .NET 
            // client communicates with the SOAP web service, including message sizes,
            // timeouts, and security settings.
            _binding = new BasicHttpBinding
            {
                SendTimeout = TimeSpan.FromSeconds(TIMEOUT),
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                AllowCookies = true,
                ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                Security = new BasicHttpSecurity 
                {
                    Mode = BasicHttpSecurityMode.Transport
                }
            };
            _address = new EndpointAddress(ENDPOINTADDRESS);
        }
        public int Sum(Numbers numbers)
        {
            // Asynchronously calls the SOAP service to perform the addition operation.
            // This is done by sending a SOAP request to the web service and waiting for 
            // the SOAP response.
           return AddAsync(numbers).Result;
        }
        private async Task<int> AddAsync(Numbers numbers) 
        {
            // Creates and uses a SOAP service client, often referred to as a "proxy", for the Add operation. 
            // The proxy abstracts the complexities of making a SOAP request, so you don't have to manually 
            // create the SOAP request, send it via HTTP, and parse the SOAP response.
            using (var proxy = new CalculatorWebServiceSoapClient(_binding, _address))
            {
                // Asynchronously performs the Add operation and returns the result. The proxy 
                // automatically transforms the method call into a SOAP request, sends the 
                // request to the web service, and converts the SOAP response back into a .NET object.
                var result = await proxy.AddAsync(numbers.NumberA, numbers.NumberB);
                return result;
            }
        }
        public int Subtract(Numbers numbers)
        {
            // Creates and uses a SOAP service client (proxy) for the Subtract operation. 
            // The proxy takes care of all the SOAP-specific tasks, so you can work with 
            // the web service operations as if they were regular .NET methods.
            return SubtractAsync(numbers).Result;
        }
        private async Task<int> SubtractAsync(Numbers numbers) 
        {
            // Creates and uses a SOAP service client for the Subtract operation. The client
            // sends a SOAP request to the web service and then parses the SOAP response
            // to extract the result of the subtraction.
            using (var proxy = new CalculatorWebServiceSoapClient(_binding, _address))
            {
                // Asynchronously performs the Subtract operation and returns the result.
                // The proxy sends the SOAP request to the web service and parses the SOAP 
                // response, transforming the response into a .NET object.
                var result = await proxy.SubtractAsync(numbers.NumberA, numbers.NumberB);
                return result;
            }
        }
    }
}