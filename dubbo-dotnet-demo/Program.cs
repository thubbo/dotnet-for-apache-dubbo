using System;
using System.Collections;
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
using System.Web.Script.Serialization;


namespace com.ikurento.user
{

    public interface UserProvider
    {
        User getUser(int id);

        int Calc(int a, int b);

        Dictionary<string, User> queryAll();
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
            
            // =============================

            string baseUrl = "http://localhost:10010";
   
            UserProvider userProvider = JsonrpcProxy.proxyService<UserProvider>(baseUrl);
            
            
            User user = userProvider.getUser(2);
            Console.WriteLine("userProvider.getUser(2)= " + user.ToString());

            int rcalc = userProvider.Calc(3,4);
            Console.WriteLine("userProvider.Calc(3,4)= " + rcalc);
            

            Dictionary<string, User>  dict = userProvider.queryAll();
            Console.WriteLine("userProvider.queryAll()= " );
            dict.Keys.ToList().ForEach(e => Console.WriteLine("  "+ e + "=>" + dict[e].ToString()));

            Console.ReadKey();
        }

        public static void TestJsonrpc()
        {
            //curl -X POST -d {\"id\":\"-9072391164073734559\",\"jsonrpc\":\"2.0\",\"method\":\"getUser\",\"params\":[2]} 
            string url = "http://localhost:10010/com.ikurento.user.UserProvider";
            string json = JsonrpcClient.invokeJson(url, "getUser", new object[] { 2 });

            Console.WriteLine(json.ToString());
            // {"jsonrpc":"2.0","id":"1000","result":
            //      {"id":"2","name":"userCode get","age":48,"time":1534659569624,"sex":"MAN"}}

            User user = Json.deserializeResponseMessage<User>(json);
            Console.WriteLine(user.ToString());
        }

        public static void TestSerialize()
        {

            string j = "{\"jsonrpc\":\"2.0\",\"id\":\"1000\",\"result\":{\"001\":{\"id\":\"001\",\"name\":\"demo-zhangsan\",\"age\":18,\"time\":1534689290641,\"sex\":\"MAN\"},\"002\":{\"id\":\"002\",\"name\":\"demo-lisi\",\"age\":20,\"time\":1534689290642,\"sex\":\"MAN\"},\"003\":{\"id\":\"003\",\"name\":\"demo-lily\",\"age\":23,\"time\":1534689290642,\"sex\":\"MAN\"},\"004\":{\"id\":\"004\",\"name\":\"demo-lisa\",\"age\":32,\"time\":1534689290642,\"sex\":\"MAN\"}}}";

            Dictionary<string, User> d = Json.deserializeResponseMessage<Dictionary<string, User>>(j);
            d.Keys.ToList().ForEach(e => Console.WriteLine("  " + e + "=>" + d[e].ToString()));

            // ===========================
            string j1 = "{\"id\":\"001\",\"name\":\"demo-zhangsan\",\"age\":18,\"time\":1534689290641,\"sex\":\"MAN\"}";
            string j2 = "{\"id\":\"002\",\"name\":\"demo-lisi\",\"age\":20,\"time\":1534689290642,\"sex\":\"MAN\"}";
            User u1 = Json.deserialize<User>(j1);
            User u2 = Json.deserialize<User>(j2);
            Dictionary<string, User> d2 = new Dictionary<string, User> { { "001", u1 }, { "002", u2 } };
            string js2 = Json.serialize(d2);
            Console.WriteLine("DataContractJsonSerializer Dictionary<string, User>= " + js2);

            // "[{\"Key\":\"001\",\"Value\":{\"age\":18,\"id\":\"001\",\"name\":\"demo-zhangsan\",\"sex\":\"MAN\",\"time\":1534689290641}},{\"Key\":\"002\",\"Value\":{\"age\":20,\"id\":\"002\",\"name\":\"demo-lisi\",\"sex\":\"MAN\",\"time\":1534689290642}}]"

            //// ===========================
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string js3 = jss.Serialize(d2);
            Console.WriteLine("JavaScriptSerializer Dictionary<string, User>= " + js3);

            string json11 = "{\"001\":{\"id\":\"001\",\"name\":\"demo-zhangsan\",\"age\":18,\"time\":1534689290641,\"sex\":\"MAN\"},\"002\":{\"id\":\"002\",\"name\":\"demo-lisi\",\"age\":20,\"time\":1534689290642,\"sex\":\"MAN\"},\"003\":{\"id\":\"003\",\"name\":\"demo-lily\",\"age\":23,\"time\":1534689290642,\"sex\":\"MAN\"},\"004\":{\"id\":\"004\",\"name\":\"demo-lisa\",\"age\":32,\"time\":1534689290642,\"sex\":\"MAN\"}}";

            Dictionary<string, User> d11 = Json.deserialize<Dictionary<string, User>>(json11);

            d11.Keys.ToList().ForEach(e => Console.WriteLine("  " + e + "=>" + d11[e].ToString()));

        }
    }

   
   
}



