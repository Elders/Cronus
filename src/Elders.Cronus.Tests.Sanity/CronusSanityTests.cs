//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.Serialization;
//using System.Xml.Linq;
//using Elders.Cronus.DomainModeling;

//namespace Elders.Cronus.Tests.Sanity
//{
//    [TestFixture]
//    public class CronusSanityTests<T>
//    {
//        string eventsSoFar = @"..\..\..\Resources\Events.xml";

//        public void SetXmlLocation(string location)
//        {
//            eventsSoFar = location;
//        }

//        static private List<Type> types = null;

//        public List<Type> GetAllEvents()
//        {
//            if (types != null)
//            {
//                return types;

//            }
//            else
//            {
//                var asm = typeof(T).Assembly;

//                types = new List<Type>();

//                var aggregateTypes = asm.GetTypes().Where(x => x.BaseType != null && x.BaseType.IsGenericType && x.BaseType.GetGenericTypeDefinition() == (typeof(AggregateRootState<>))).ToList();

//                foreach (var type in aggregateTypes)
//                {
//                    var handlers = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == "When");
//                    foreach (var handler in handlers)
//                    {
//                        types.Add(handler.GetParameters().Single().ParameterType);
//                    }
//                }
//                return types;
//            }
//        }

//        [Test]
//        public void CheckInterfaces()
//        {
//            var events = GetAllEvents();
//            List<Action> assertions = new List<Action>();

//            foreach (Type evnt in events)
//            {
//                Type workingEvnt = evnt;
//                assertions.Add(() => Assert.That(workingEvnt.GetInterfaces().Contains(typeof(IEvent)), String.Format("The event class '{0}' does not implement '{1}'", workingEvnt.FullName, typeof(IEvent).Name)));
//            }
//            AssertAll.Succeed(assertions.ToArray());
//        }

//        [Test]
//        public void CheckForProtobuffDataMembers()
//        {
//            var events = GetAllEvents();
//            List<Action> assertions = new List<Action>();
//            foreach (var evnt in events)
//            {
//                var properties = evnt.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                foreach (PropertyInfo property in properties)
//                {
//                    PropertyInfo workingProperty = property;
//                    Type workingEvnt = evnt;
//                    assertions.Add(() => Assert.AreEqual(true, workingProperty.GetCustomAttributes(false).Select(x => x.GetType()).Contains(typeof(DataMemberAttribute)), String.Format("Property '{0}' of '{1}' does not have '{2}'", workingProperty.Name, workingEvnt.FullName, typeof(DataMemberAttribute).Name)));
//                }
//            }
//            AssertAll.Succeed(assertions.ToArray());
//        }

//        [Test]
//        public void CheckForProtobuffDataContracts()
//        {
//            var events = GetAllEvents();
//            List<Action> assertions = new List<Action>();
//            foreach (Type evnt in events)
//            {
//                Type workingEvnt = evnt;
//                assertions.Add(() => Assert.AreEqual(true, workingEvnt.GetCustomAttributes(false).Select(x => x.GetType()).Contains(typeof(DataContractAttribute)), String.Format("'{0}' does not have '{1}'", workingEvnt.FullName, typeof(DataContractAttribute).Name)));
//            }
//            AssertAll.Succeed(assertions.ToArray());
//        }

//        [Test]
//        public void Test_Events_For_Having_Unique_DataContracts()
//        {
//            var events = GetAllEvents();
//            List<Action> assertions = new List<Action>();
//            Dictionary<int, Type> uniqueEventIds = new Dictionary<int, Type>();
//            foreach (Type evnt in events)
//            {

//                Type workingEvnt = evnt;
//                DataContractAttribute attribute = (DataContractAttribute)workingEvnt
//                                               .GetCustomAttributes(false)
//                                               .Where(attrib => attrib.GetType() == typeof(DataContractAttribute))
//                                               .SingleOrDefault();

//                bool isValidEvent = attribute != null && !String.IsNullOrWhiteSpace(attribute.Name);
//                assertions.Add(() => Assert.That(isValidEvent, String.Format("The event '{0}' does not have Name in the DataContract.(Try Name=\"{1}\")", workingEvnt.FullName, GetUniqueGuid())));
//                if (isValidEvent)
//                {
//                    int eventId = Math.Abs((Guid.Parse(attribute.Name).GetHashCode())) / 4;
//                    var isUniqueEvent = !uniqueEventIds.ContainsKey(eventId) && Math.Abs((Guid.Parse(attribute.Name).GetHashCode())) % 4 == 0;
//                    assertions.Add(() => Assert.That(isUniqueEvent, String.Format("The event '{0}'  DataContract Name is not unique.(Try Name=\"{1}\")", workingEvnt.FullName, GetUniqueGuid())));
//                    if (isUniqueEvent)
//                        uniqueEventIds.Add(eventId, workingEvnt);
//                }

//            }
//            AssertAll.Succeed(assertions.ToArray());
//        }

//        [Test]
//        public void Test_For_Deleted_Events()
//        {
//            XDocument xmlDoc;
//            bool hasUpdates = false;
//            using (var textStream = File.OpenRead(eventsSoFar))
//            {
//                List<Action> assertions = new List<Action>();
//                xmlDoc = XDocument.Load(textStream);

//                HashSet<Guid> eventIdsFromFile = new HashSet<Guid>(xmlDoc.Descendants("Event").Select(x => Guid.Parse(x.Attribute("Id").Value)));
//                Dictionary<Guid, Type> eventIds = null;
//                try
//                {
//                    eventIds = GetAllEvents().ToDictionary(
//                        x => x.GetCustomAttributes(false)
//                               .Where(attrib => attrib.GetType() == typeof(DataContractAttribute) && (attrib as DataContractAttribute).Name != null)
//                               .Select(y => Guid.Parse((y as DataContractAttribute).Name)).Single(),
//                        y => y);

//                }
//                catch (Exception)
//                {
//                    throw new Exception(@"The events do not pass the ""Test_Events_For_Having_Unique_DataContracts()"" test");
//                }

//                foreach (var filedEvent in eventIdsFromFile)
//                {
//                    Guid workingWtf = filedEvent;
//                    assertions.Add(() => Assert.That(eventIds.ContainsKey(workingWtf), String.Format("The event with id '{0}' is not contained in the current contracts assembly!The event was deleted or its Id was altered", workingWtf)));

//                }
//                AssertAll.Succeed(assertions.ToArray());
//                UpdateEventsXML(xmlDoc, eventIds, eventIdsFromFile, out hasUpdates);

//            }
//            if (hasUpdates)
//            {
//                using (var stream = File.OpenWrite(eventsSoFar))
//                {
//                    xmlDoc.Save(stream);
//                }
//            }
//        }

//        [Test]
//        public void CheckForParameterlessConstructor()
//        {
//            var events = GetAllEvents();
//            List<Action> assertions = new List<Action>();
//            foreach (Type evnt in events)
//            {
//                Type workingEvnt = evnt;
//                var ctors = workingEvnt.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
//                bool hasParameterlessConsructor = false;
//                foreach (ConstructorInfo ctor in ctors)
//                {
//                    hasParameterlessConsructor = ctor.GetParameters().Count() == 0;
//                    if (hasParameterlessConsructor)
//                        break;
//                }
//                assertions.Add(() => Assert.That(hasParameterlessConsructor, String.Format("The event '{0}' does not contains parameterless private constructor ", workingEvnt.FullName)));
//            }
//            AssertAll.Succeed(assertions.ToArray());
//        }

//        private void UpdateEventsXML(XDocument xmlDoc, Dictionary<Guid, Type> eventIdsInContracts, HashSet<Guid> eventIdsFromFile, out bool hasChanges)
//        {
//            var newEvents = eventIdsInContracts.Where(x => !eventIdsFromFile.Contains(x.Key));
//            hasChanges = newEvents.Count() > 0;


//            foreach (var evnt in newEvents)
//            {
//                XElement[] DataMembers = evnt.Value.GetProperties().Where(x => x.GetCustomAttributes(false).Where(y => y is DataMemberAttribute).Count() > 0)
//                    .Select(z => new XElement("DataMember",
//                        new XAttribute("Order", (z.GetCustomAttributes(false).Where(y => y is DataMemberAttribute).SingleOrDefault() as DataMemberAttribute).Order),
//                        DataMember.GetTypeName(z.PropertyType)
//                        )).ToArray();

//                xmlDoc.Element("Events").Add(new XElement("Event",
//                    new XAttribute("Id", evnt.Key),
//                    DataMembers
//                    ));
//            }

//        }

//        private List<DataMember> GetDataMembers(XElement elem)
//        {
//            return elem.Descendants("DataMember").Select(z => new DataMember(z.Attribute("Order").Value, z.Value)).ToList();
//        }

//        private static Guid GetUniqueGuid()
//        {
//            Guid theGuid;

//            while (true)
//            {
//                theGuid = Guid.NewGuid();
//                if (Math.Abs(theGuid.GetHashCode()) % 4 == 0)
//                    break;
//            }

//            return theGuid;
//        }

//        public static class AssertAll
//        {
//            public static void Succeed(params Action[] assertions)
//            {
//                var errors = new List<Exception>();

//                foreach (var assertion in assertions)
//                    try
//                    {
//                        assertion();
//                    }
//                    catch (Exception ex)
//                    {
//                        errors.Add(ex);
//                    }

//                if (errors.Any())
//                {
//                    var ex = new AssertionException(String.Concat(
//                        string.Join(Environment.NewLine, errors.Select(e => e.Message)), errors.First(),
//                        string.Join(Environment.NewLine, "\n\r---------------------------\n\r"),
//                        string.Join(Environment.NewLine, String.Format("{0} out of {1} failed.", errors.Count, assertions.Count())),
//                        string.Join(Environment.NewLine, "\n\r---------------------------\n\r")));
//                    ReplaceStackTrace(ex, errors.First().StackTrace);
//                    throw ex;
//                }
//            }

//            static void ReplaceStackTrace(Exception exception, string stackTrace)
//            {
//                var remoteStackTraceString = typeof(Exception)
//                    .GetField("_remoteStackTraceString",
//                        BindingFlags.Instance | BindingFlags.NonPublic);

//                remoteStackTraceString.SetValue(exception, stackTrace);
//            }
//        }

//        private class DataMember : ValueObject<DataMember>
//        {
//            public DataMember(string order, string typeName)
//            {
//                Order = Int32.Parse(order);
//                TypeName = typeName;
//            }
//            public int Order { get; set; }
//            public string TypeName { get; set; }

//            public static string GetTypeName(Type t)
//            {
//                if (IsProtoType(t))
//                    return (t.GetCustomAttributes(false).Where(attrib => attrib.GetType() == typeof(DataContractAttribute) && (attrib as DataContractAttribute).Name != null).SingleOrDefault() as DataContractAttribute).Name;

//                else
//                    return t.FullName;
//            }

//            public static bool IsProtoType(Type t)
//            {
//                return t.GetCustomAttributes(false).Where(attrib => attrib.GetType() == typeof(DataContractAttribute) && (attrib as DataContractAttribute).Name != null).Count() > 0;
//            }
//        }
//    }
//}
