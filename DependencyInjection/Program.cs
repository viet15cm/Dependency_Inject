using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace DependencyInjection
{

    public class Singleton
    {
        private static Singleton _intance;

        private Singleton() { }

        public static  Singleton GetSigleton()
        {
            _intance = _intance ?? new Singleton();

            return _intance;
        }
        //public int inti { get; set; }
    }

    interface IClassB
    {
        public void ActionB();
    }
    interface IClassC
    {
        public void ActionC();
    }

     class ClassC : IClassC
    {
        public ClassC() => Console.WriteLine("ClassC is created");
        public void ActionC() => Console.WriteLine("Action in ClassC");
    }

    class ClassB : IClassB
    {
        IClassC c_dependency;
        public ClassB(IClassC classc)
        {
            c_dependency = classc;
            Console.WriteLine("ClassB is created");
        }
        public void ActionB()
        {
            Console.WriteLine("Action in ClassB");
            c_dependency.ActionC();
        }
    }

    class ClassB2 : IClassB
    {
        IClassC c_dependency;
        string message;
        public ClassB2(IClassC classc, string mgs)
        {
            c_dependency = classc;
            message = mgs;
            Console.WriteLine("ClassB2 is created");
        }
        public void ActionB()
        {
            Console.WriteLine(message);
            c_dependency.ActionC();
        }
    }
    class ClassA
    {
        IClassB b_dependency;
        public ClassA(IClassB classb)
        {
            b_dependency = classb;
            Console.WriteLine("ClassA is created");
        }
        public void ActionA()
        {
            Console.WriteLine("Action in ClassA");
            b_dependency.ActionB();
        }
    }

    class ClassC1 : IClassC
    {
        public ClassC1() => Console.WriteLine("ClassC1 is created");
        public void ActionC()
        {
            Console.WriteLine("Action in C1");
        }
    }

    class ClassB1 : IClassB
    {
        IClassC c_dependency;
        public ClassB1(IClassC classc)
        {
            c_dependency = classc;
            Console.WriteLine("ClassB1 is created");
        }
        public void ActionB()
        {
            Console.WriteLine("Action in B1");
            c_dependency.ActionC();
        }
    }
    public class Horn
    {
        int lever = 0;
        public Horn (int lever)
        {
            this.lever = lever;
        }
        public void Beep() => Console.WriteLine("Beep..Beep..");
    }
    public class Car
    {
        public Horn horn { get; set; }
        public Car(Horn horn)
        {
            this.horn = horn;
        }
        public void Beep()
        {
            
            horn.Beep();
        }
    }
    class Program
    {

        static void Demo()
        {
            
           var car = new Car(new Horn(1));
           car.Beep();

           IClassC IC = new ClassC();
           IClassB IB = new ClassB(IC);
           ClassA CA = new ClassA(IB);
           CA.ActionA();

            var sevices = new ServiceCollection();
            // dang ki cac dich vu...
            //IClassC , ClassC, ClassC1
            //Dang ki kieu Singleton
            sevices.AddScoped<IClassC, ClassC>();
            // dagn ki xong
            var providder = sevices.BuildServiceProvider();
            
            // lay ra cac doi tuong da dang ky
            for (int i = 0; i < 5; i++)
            {
                var a = providder.GetService<IClassC>();
             
                Console.WriteLine(a.GetHashCode());
                
            }
            using (var c = providder.CreateScope())
            {
                var providder1 = c.ServiceProvider;
                for (int i = 0; i < 5; i++)
                {
                    var s = providder1.GetService<IClassC>();

                    Console.WriteLine(s.GetHashCode());

                }
            }
            
        }

       // Degetegate Factory
        static void DelegteFactory()
        {
            var services = new ServiceCollection();

            services.AddScoped<ClassA, ClassA>();

            services.AddScoped<IClassB, ClassB2>((provider) => {

                var c = new ClassB2(
                    provider.GetService<IClassC>(),
                    "Da injection B2"
                    );
                return c;
            });
            services.AddScoped<IClassC, ClassC>();

            var provider = services.BuildServiceProvider();

            ClassA a = provider.GetService<ClassA>();
            a.ActionA();
        }
        public static IClassB CreatB2(IServiceProvider provider)
        {
            var b2 = new ClassB2(
//                new ClassC()
                   provider.GetService<IClassC>(),
                   "thuc hien trong class B2"
                   );

            return b2;

        }

        // OptionInject
        public class MyServices
        {

            public MyServices(IOptions<MysevicesOption> options)
            {
                var _options = options.Value;
                Data1 = _options.Data1;
                Data2 = _options.Data2;
            }
            public string Data1 { get; set; }
            public int Data2 { get; set; }
            public void printData() => Console.WriteLine($"{Data1}/{Data2}");
        }

        public class MysevicesOption
        {
            public string Data1 { get; set; }
            public int Data2 { get; set; }
        }
       

        public static void OptionInject()
        {
            var sevices = new ServiceCollection();
            // Đăng Ký Các Dịch vụ
            sevices.AddSingleton<MyServices>();
            //Đăng Ký Option.
            sevices.Configure<MysevicesOption>((options) => {

                options.Data1 = "Viet";
                options.Data2 = 23;

            });
            var provider = sevices.BuildServiceProvider();

            var myservices = provider.GetService<MyServices>();

            myservices.printData();
        }

        // Nạp Cấu hình File vào Ứng Dụng Dependency Inject

        // Thư Viện Hổ trợ Nạp File Cấu Hình
        /* 
          dotnet add package Microsoft.Extensions.Configuration
          dotnet add package Microsoft.Extensions.Options.ConfigurationExtensions
        */

        //Nạp file định dạng muốn dùng
        /*
         dotnet add package Microsoft.Extensions.Configuration.Json
         dotnet add package Microsoft.Extensions.Configuration.Ini
         dotnet add package Microsoft.Extensions.Configuration.Xml
        */
        public static void ReadFileDependencyInject()
        {
            //IConfigurationRoot configurationRoot;
            ConfigurationRoot configurationRoot;
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("CauHinh.json");
            configurationRoot = (ConfigurationRoot)configurationBuilder.Build();

            var mySeviceOptions = configurationRoot.GetSection("MyServiceOptions");



            var sevices = new ServiceCollection();
            // Đăng Ký Các Dịch vụ
            sevices.AddSingleton<MyServices>();
            //Đăng Ký Option.
            sevices.Configure<MysevicesOption>(mySeviceOptions);
            var provider = sevices.BuildServiceProvider();

            var myservices = provider.GetService<MyServices>();
            myservices.printData();
        }
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddScoped<ClassA, ClassA>();

            services.AddScoped<IClassB, ClassB2>((provider) => {

                var c = new ClassB2(
                    // Viet bang hai cách
                    //new ClassC(), 
                    provider.GetService<IClassC>(),
                    "Da injection B2"
                    );
                return c;
            });
            services.AddScoped<IClassC, ClassC>();

            var provider = services.BuildServiceProvider();

            ClassA a = provider.GetService<ClassA>();
            a.ActionA();
        }
    }
}
