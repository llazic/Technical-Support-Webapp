using System;
using System.Collections.Generic;
using System.Linq;
using PayPalCheckoutSdk.Core;
using BraintreeHttp;

using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

namespace IEPProjekat.PayPal
{
    public class PayPalClient
    {
        /**
           Set up PayPal environment with sandbox credentials.
           In production, use LiveEnvironment.
        */
        public static PayPalEnvironment environment()
        {
            //ovo ispod su clientID i clientSecret
            return new SandboxEnvironment("ARyDkoB48MAmMCLxVkaKXFBuzMN0m2rLbTvNqKRzglHJ9i9wb9GnrPSh7uNVktayQOX8izWQPap-0phm", "EHNKV-Ks_wtIavIcn806MTxVibvO9muy1XO7EyakVz7WSJA7vrc2--1THtMlKP1jwt3hxzH1BVxNvNtU");
        }

        /**
            Returns PayPalHttpClient instance to invoke PayPal APIs.
         */
        public static HttpClient client()
        {
            return new PayPalHttpClient(environment());
        }

        public static HttpClient client(string refreshToken)
        {
            return new PayPalHttpClient(environment(), refreshToken);
        }

        /**
            Use this method to serialize Object to a JSON string.
        */
        public static String ObjectToJSONString(Object serializableObject)
        {
            MemoryStream memoryStream = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(
                        memoryStream, Encoding.UTF8, true, true, "  ");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(serializableObject.GetType(), new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
            ser.WriteObject(writer, serializableObject);
            memoryStream.Position = 0;
            StreamReader sr = new StreamReader(memoryStream);
            return sr.ReadToEnd();
        }
    }
}