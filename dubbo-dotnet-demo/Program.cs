using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using dubbo.dotnet.common;

namespace dubbo.dotnet.demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Dubbo-Dotnet-Demo starting...");
           

             //curl -X POST -d {\"id\":\"-9072391164073734559\",\"jsonrpc\":\"2.0\",\"method\":\"getUser\",\"params\":[2]} 
            string url = "http://192.168.99.1:10010/com.ikurento.user.UserProvider3";
            string json = JsonrpcClient.invokeJson(url, "getUser", new object[] { 2 });

            Console.WriteLine(json.ToString());

            User user = Json.deserialize<ResponseMessage<User>>(json).result;
            Console.WriteLine(user.ToString());

            Console.ReadKey();
        }
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
            return String.Format("id={0},name={1},age={2},time={3},sex={4}",id,name,age,time,sex);
        }
    }
}
