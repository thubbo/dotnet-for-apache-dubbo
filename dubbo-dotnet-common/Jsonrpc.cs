using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Net.Http;

namespace dubbo.dotnet.common
{
    [DataContract]
    public class RequestMessage
    {
        [DataMember(Name="id")]
        public string Id { get; set; }

        [DataMember(Name = "jsonrpc")]
        public string Version { get; set; }

        [DataMember(Name = "method")]
        public string Method { get; set; }

        [DataMember(Name = "params")]
        public object[] Parameters { get; set; }
    }

    [DataContract]
    public class ResponseMessage<T>
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "jsonrpc")]
        public string Version { get; set; }

        [DataMember(Name = "result")]
        public T result { get; set; }
    }

    public class Json
    {
        public static string serialize(object o)
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(o.GetType());
            MemoryStream msObj = new MemoryStream();

            js.WriteObject(msObj, o);
            msObj.Position = 0;

            StreamReader sr = new StreamReader(msObj, Encoding.UTF8);
            string json = sr.ReadToEnd();
            sr.Close();
            msObj.Close();

            return json;
        }

        public static T deserialize<T>(string json) where T : class
        {
            var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(json));
            var deseralizer = new DataContractJsonSerializer(typeof(T));
            var model = deseralizer.ReadObject(memoryStream);
            memoryStream.Close();
            return (T)model ;
        }
    }

    public class HttpClient
    {
        public static string post(string url, string body)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.Method = "POST";
            
            using (var reqStream =  request.GetRequestStream())
            {
                var data = Encoding.UTF8.GetBytes(body);
                reqStream.Write(data, 0, data.Length);
            }

            using (var resStream = request.GetResponse().GetResponseStream())
            {
                return new StreamReader(resStream, Encoding.UTF8).ReadToEnd();
            }

        }
    }

    public class JsonrpcClient
    {
        private static long _id = 1000;
        public static string invokeJson(string url,string method, object[] parameters)
        {
            RequestMessage message = new RequestMessage()
            {
                Id = _id++.ToString(),
                Version = "2.0",
                Method = method,
                Parameters = parameters
            };

            string req = Json.serialize(message);
            return HttpClient.post(url, req);
        }

        public static T invoke<T>(string url, string method, object[] parameters) where T : class
        {
            string json = invokeJson(url, method, parameters);
            return Json.deserialize<T>(json);
        } 

    }
}
