using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Reflection;  

using Castle.Core;  
using Castle.DynamicProxy;  

using dubbo.dotnet.common;

using com.ikurento.user;


namespace com.ikurento.user
{

    public interface UserProvider3
    {
        User getUser(int id);
    }

    public class User
    {
        public string id { get; set; }

        public string name { get; set; }

        public int age { get; set; }

        public long time { get; set; }

        public string sex { get; set; }

        public override string ToString()
        {
            return String.Format("id={0},name={1},age={2},time={3},sex={4}", id, name, age, time, sex);
        }
    }
}

namespace dubbo.dotnet.demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Dubbo-Dotnet-Demo starting...");

            string baseUrl = "http://localhost:10010";
   
            UserProvider3 userProvider = JsonrpcProxy.proxyService<UserProvider3>(baseUrl);

            User user3 = userProvider.getUser(2);
            Console.WriteLine("final return: "+user3.ToString());

            Console.ReadKey();
        }

        public static void TestJsonrpc()
        {
            //curl -X POST -d {\"id\":\"-9072391164073734559\",\"jsonrpc\":\"2.0\",\"method\":\"getUser\",\"params\":[2]} 
            string url = "http://localhost:10010/com.ikurento.user.UserProvider3";
            string json = JsonrpcClient.invokeJson(url, "getUser", new object[] { 2 });

            Console.WriteLine(json.ToString());
            // {"jsonrpc":"2.0","id":"1000","result":
            //      {"id":"2","name":"userCode get","age":48,"time":1534659569624,"sex":"MAN"}}

            User user = Json.deserializeResponseMessage<User>(json).result;
            Console.WriteLine(user.ToString());
        }
    }

   
   
}



