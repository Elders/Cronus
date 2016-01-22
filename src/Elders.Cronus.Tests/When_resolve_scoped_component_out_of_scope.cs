//using System;
//using System.IO;
//using Elders.Protoreg;
//using Machine.Specifications;

//namespace Elders.Cronus.Tests.DomainModeling
//{
//    [Subject("IocContainer")]
//    public class When_an_exception_is_serialized
//    {
//        static ILog log = LogProvider.GetLogger(typeof(When_an_exception_is_serialized));

//        Establish context = () =>
//            {
//                log4net.Config.XmlConfigurator.Configure();
//                log = LogProvider.GetLogger(typeof(When_an_exception_is_serialized));
//                ProtoRegistration reg = new ProtoRegistration();
//                reg.RegisterAssembly<When_an_exception_is_serialized>();
//                serializer = new ProtoregSerializer(reg);
//                serializer.Build();

//                try
//                {
//                    throw new ArgumentNullException("inner");
//                }
//                catch (Exception ex)
//                {
//                    inner = ex;
//                }
//                try
//                {
//                    throw new Exception("root", inner);
//                }
//                catch (Exception ex)
//                {
//                    root = ex;
//                    serializableException = new ProtoregSerializableException(ex);
//                }
//            };

//        Because of = () =>
//            {
//                using (var stream = new MemoryStream())
//                {
//                    serializer.Serialize(stream, serializableException);
//                    stream.Position = 0;
//                    serialized = stream.ToArray();
//                }
//            };

//        It should_properly_deserialize = () =>
//        {
//            using (var stream = new MemoryStream(serialized))
//            {
//                ProtoregSerializableException result = (ProtoregSerializableException)serializer.Deserialize(stream);
//                string asd = result.ToString();
//                log.Error("test message", result);
//                log.Error("test message", root);
//            }
//        };


//        static Exception root;
//        static Exception inner;
//        static ProtoregSerializableException serializableException;
//        static ProtoregSerializer serializer;
//        static byte[] serialized;
//    }
//}