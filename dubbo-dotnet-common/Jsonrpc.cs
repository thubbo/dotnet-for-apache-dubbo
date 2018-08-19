using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Web.Script.Serialization;  
using System.Net;
using System.Net.Http;
using System.Reflection;

using Castle.Core;  
using Castle.DynamicProxy;


namespace dubbo.dotnet.common
{
    /*
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
     */

    public class RequestMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("jsonrpc")]
        public string Version { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public object[] Parameters { get; set; }
    }

    public class ResponseMessage<T>
    {
        public string id { get; set; }

        public string jsonrpc { get; set; }

        public T result { get; set; }
    }

    public class Json
    {
        static JavaScriptSerializer jss = null;
        public static JavaScriptSerializer getJavaScriptSerializer()
        {
            if (jss == null)
            {
                jss = new JavaScriptSerializer();
                jss.RegisterConverters(new[] { new JsonConverter() });
            }
            return jss;
        }

        public static string serialize(object o)
        {
            return getJavaScriptSerializer().Serialize(o);
        }

        public static T deserializeResponseMessage<T>(string json) 
        {
            ResponseMessage<T> response = deserialize(json, typeof(ResponseMessage<T>)) as ResponseMessage<T>;
            return response.result;
        }


        public static T deserialize<T>(string json) where T : class
        {
            return getJavaScriptSerializer().Deserialize<T>(json);
        }

        public static object deserialize(string json, Type type)
        {
            return getJavaScriptSerializer().Deserialize(json, type);
        }
    }

    public class HttpClient
    {
        public static string post(string url, string body)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
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

            string request = Json.serialize(message);
            Console.WriteLine("request==> " + request);
            return HttpClient.post(url, request);
        }
    }

    public class JsonrpcInterceptor : IInterceptor
    {
        string baseUrl = "http://localhost/";

        public JsonrpcInterceptor() { }
        public JsonrpcInterceptor(string url)
        {
            this.baseUrl = url;
            if (!this.baseUrl.EndsWith("/")) this.baseUrl = this.baseUrl + "/";
        }

        public void Intercept(IInvocation invocation)
        {
            MethodInfo method = invocation.GetConcreteMethod();
            Type type = method.DeclaringType;

            if (method.DeclaringType.IsInterface)
            {
                string serviceUrl = this.baseUrl + type.FullName;
                string methodName = method.Name;
                object[] parameters = invocation.Arguments;

                // invoke JSONRPC via http
                string json = JsonrpcClient.invokeJson(serviceUrl, methodName, parameters);

                Console.WriteLine(json.ToString());
                Type retType = method.ReturnType;

                // deserializeResponseMessage and make generic
                MethodInfo dmethod = typeof(Json).GetMethod("deserializeResponseMessage");
                MethodInfo generic = dmethod.MakeGenericMethod(retType);

                // Json.deserializeResponseMessage(json)
                object respose = generic.Invoke(null, new object[] { json });

                // ResponseMessage.result
                // object ret = respose.GetType().GetProperty("result").GetValue(respose);

                invocation.ReturnValue = respose;
            }
        }

    }

    public class JsonrpcProxy
    {

        static ProxyGenerator generator = new ProxyGenerator();

        public static T proxyService<T>(string baseUrl) where T : class
        {
            JsonrpcInterceptor interceptor = new JsonrpcInterceptor(baseUrl);
            return generator.CreateInterfaceProxyWithoutTarget<T>(interceptor);
        }
    }

}
