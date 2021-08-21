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

        public static Singleton GetSigleton()
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
        public Horn(int lever)
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

    public class MysevicesOptions
    {
        public string Data1 { get; set; }
        public int Data2 { get; set; }
    }
    public  class MyServices
    {

        public MyServices(IOptions<MysevicesOptions> options)
        {
            var _options = options.Value;
            Data1 = _options.Data1;
            Data2 = _options.Data2;
        }
        public string Data1 { get; set; }
        public int Data2 { get; set; }
        public void printData() => Console.WriteLine($"{Data1}/{Data2}");
    }
    class Program
    {

        public static void DependencyInject()
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
        public static void OptionInject()
        {
            var sevices = new ServiceCollection();
            // Đăng Ký Các Dịch vụ
            sevices.AddSingleton<MyServices>();
            //Đăng Ký Option.
            sevices.Configure<MysevicesOptions>((options) => {

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
            IConfiguration configurationRoot;
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("CauHinh2.json");
            configurationRoot = configurationBuilder.Build();

            var testOptions = configurationRoot.GetSection("TestOptions");



            var sevices = new ServiceCollection();
            //Đăng Ký Các Dịch vụ
           // sevices.AddSingleton<MyServices>();
            //Đăng Ký Option.
            sevices.Configure<TestOptions>(testOptions);
            var provider = sevices.BuildServiceProvider();

            var myservices = provider.GetService<IOptions<TestOptions>>().Value;
            Console.WriteLine(myservices.Key_2.K1);
        }

        // setting File starup demo
        public static void Starup()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<Starup, Starup>((provider) => {
                IConfiguration configurationRoot;
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                configurationBuilder.AddJsonFile("CauHinh2.json");
                configurationRoot = configurationBuilder.Build();

                var c = new Starup(configurationRoot);

                return c;
            });
            services.AddSingleton<IConfiguration, ConfigurationRoot>();

            IServiceProvider provider = services.BuildServiceProvider();

            Starup starup = provider.GetService<Starup>();

            starup.ConfigurationSevices(services);
            starup.Display(services);
        }

        public class ClassBB
        {
            public ClassBB() => Console.WriteLine("Da Khoi tao ClassBB");

            public void ActionBB()
            {
                Console.WriteLine("Action BB");
            }
        }

        public class ClassCC
        {
            public ClassCC() => Console.WriteLine("Da Khoi tao ClassCC");
            public void ActionCC()
            {
                Console.WriteLine("Action CC");
            }
        }

        public class ClassAA
        {
            public readonly ClassBB _classBB;

            public readonly ClassCC _classCC;

            public ClassAA(ClassBB classBB , ClassCC classCC)
            {
                _classBB = classBB;
                _classCC = classCC;
            }

            public void Action()
            {
                _classBB.ActionBB();
                _classCC.ActionCC();
                Console.WriteLine("Action AA");
            }
        }
        static void Main(string[] args)
        {
            IServiceCollection serviceDescriptors = new ServiceCollection();
            serviceDescriptors.AddSingleton<ClassAA, ClassAA>();
            serviceDescriptors.AddSingleton<ClassBB, ClassBB>();
            serviceDescriptors.AddSingleton<ClassCC, ClassCC>();
            IServiceProvider provider = serviceDescriptors.BuildServiceProvider();

            var c = provider.GetService<ClassAA>();
            c.Action();
        }
    }

    // khoi tao file starup
    class Starup
    {

        private readonly IConfiguration _configuration;
        public IServiceProvider _serviceProvider { set; get; }
        public Starup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigurationSevices(IServiceCollection services)
        {
            var options = _configuration.GetSection("TestOptions");
            services.Configure<TestOptions>(options);
            _serviceProvider = services.BuildServiceProvider();
        }

        public void Display(IServiceCollection services)
        {
            var pr = _serviceProvider.GetService<IOptions<TestOptions>>().Value;
            Console.WriteLine(pr.Key_2.K2);
        }

    }
    class TestOptions
    {
        public string Key_1 { get; set; }
        public SubTestOptions Key_2 { get; set; }

    }

    class SubTestOptions
    {
        public string K1 { get; set; }
        public string K2 { get; set; }
    }

    

    
}
