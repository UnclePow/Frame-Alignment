using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Newtonsoft.Json;
using ProtoBuf;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            List<char> str = new List<char>();
            for (int i = 0;  i < 500; i++) {
                str.Add('1');
            }
            char[] chars = str.ToArray();
            string resstr = new string(chars);
            //Console.WriteLine(resstr);
            Login login_pb2 = new Login()
            {
                Username = resstr,
                Password = resstr
            };
            Pbv3.Login login_pb3 = new Pbv3.Login
            {
                Username = resstr,
            };

            PBNET.Login login_net = new PBNET.Login
            {
                username = resstr,
                password = resstr
            };

            MyLogin myLogin = new MyLogin
            {
                Username = resstr,
                Password = resstr
            };



            using (var output = File.Create("login.dat"))
            {
                login_pb2.WriteTo(output);
            }

            Pbv3.Login loginres;
            using (var output = File.OpenRead("login.dat")) {
                loginres = Pbv3.Login.Parser.ParseFrom(output);
            }
            //Console.WriteLine(loginres.ToString());


            //protobuf-net
            DateTime beforDT = System.DateTime.Now;
            //耗时巨大的代码
            for (var i = 0; i < 999; i++)
            {             
                byte[] res = PBHelper.Serialize<PBNET.Login>(login_net);
                PBNET.Login login = PBHelper.Deserialize<PBNET.Login>(res);
                //Console.WriteLine("protobuf-net: " + res.Length);
            }
            DateTime afterDT = System.DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforDT);
            Console.WriteLine("DateTime总共花费{0}ms.", ts.TotalMilliseconds);
            

            

            //newtonsoftjson
            beforDT = System.DateTime.Now;
            //耗时巨大的代码
            for (var i = 0; i < 999; i++)
            {
                byte[] res = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(login_net));
                MyLogin myLogin1 = JsonConvert.DeserializeObject<MyLogin>(Encoding.UTF8.GetString(res));
                //Console.WriteLine("newtonsoftjson: " + res.Length);
            }
            afterDT = System.DateTime.Now;
            ts = afterDT.Subtract(beforDT);
            Console.WriteLine("DateTime总共花费{0}ms.", ts.TotalMilliseconds);

            //google-protobuf2
            beforDT = System.DateTime.Now;
            //耗时巨大的代码
            for (var i = 0; i < 999; i++)
            {
                byte[] res = login_pb2.ToByteArray();
                Login login = Login.Parser.ParseFrom(res);
                //Console.WriteLine("google-protobuf2: " + res.Length);
            }
            afterDT = System.DateTime.Now;
            ts = afterDT.Subtract(beforDT);
            Console.WriteLine("DateTime总共花费{0}ms.", ts.TotalMilliseconds);

            //google-protobuf3
            beforDT = System.DateTime.Now;
            //耗时巨大的代码
            for (var i = 0; i < 1; i++)
            {
                //byte[] res = login_pb3.ToByteArray();
                //Login login = Login.Parser.ParseFrom(res);
                byte[] res = PBHelper.Serialize_PB3<Pbv3.Login>(login_pb3);
                Pbv3.Login login = PBHelper.Deserialize_PB3<Pbv3.Login>(res);
                Console.WriteLine(login);
                //Console.WriteLine("google-protobuf3: " + res.Length);
            }
            afterDT = System.DateTime.Now;
            ts = afterDT.Subtract(beforDT);
            Console.WriteLine("DateTime总共花费{0}ms.", ts.TotalMilliseconds);

            Console.ReadLine();
        }

        static void Main1(string[] args) {
            TestAction((str)=> { Console.WriteLine(str); });
            string res = TestFunc((str1, str2) => { return str1+str2; });
            Console.WriteLine(res);
            Console.ReadKey();
        }

        private static string TestFunc(Func<string, string, string> func)
        {
            return func("str1", "str2");
        }

        private static void TestAction(Action<string> action) {
            action("test action");
        }

        private static void Test(string str) {
            Console.WriteLine(str);
        }
    }

    class MyLogin { 
        public virtual string Username { set; get; }
        public virtual string Password { set; get; }
    }

    [ProtoContract]
    class LoginTest
    {
        [ProtoMember(1)]
        public string Username;
        [ProtoMember(2)]
        public string Password;
    }
}
