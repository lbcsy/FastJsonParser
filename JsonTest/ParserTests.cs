// On GitHub: https://github.com/ysharplanguage/FastJsonParser
//#define THIS_JSON_PARSER_ONLY // (If *not* defined, the speed tests will require a reference to (at least) Json.NET)
#define RUN_UNIT_TESTS
#define RUN_BASIC_JSONPATH_TESTS
#define RUN_ADVANCED_JSONPATH_TESTS
#define RUN_SERVICESTACK_TESTS              // (If defined, the speed tests may require a reference to ServiceStack)
//#define RUN_NETJSON_TESTS                   // (If defined, the speed tests may require a reference to NetJSON)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization; // For Microsoft's JavaScriptSerializer

#if !THIS_JSON_PARSER_ONLY
using Newtonsoft.Json; // Cf. https://www.nuget.org/packages/Newtonsoft.Json
#if RUN_SERVICESTACK_TESTS
using ServiceStack.Text; // Cf. https://www.nuget.org/packages/ServiceStack.Text
#endif
#if RUN_NETJSON_TESTS
using NetJSON; // Cf. https://www.nuget.org/packages/NetJSON
#endif
#endif

using System.Text.Json; // Our stuff; cf. https://www.nuget.org/packages/System.Text.Json

namespace Test
{
    using Sys.Text.Json.JsonPath;
#if RUN_UNIT_TESTS && (RUN_BASIC_JSONPATH_TESTS || RUN_ADVANCED_JSONPATH_TESTS)
    using System.Text.Json.JsonPath;
    using System.Text.Json.JsonPath.LambdaCompilation;
#endif

    public class E
    {
        public object zero { get; set; }
        public int one { get; set; }
        public int two { get; set; }
        public List<int> three { get; set; }
        public List<int> four { get; set; }
    }

    public class F
    {
        public object g { get; set; }
    }

    public class E2
    {
        public F f { get; set; }
    }

    public class D
    {
        public E2 e { get; set; }
    }

    public class C
    {
        public D d { get; set; }
    }

    public class B
    {
        public C c { get; set; }
    }

    public class A
    {
        public B b { get; set; }
    }

    public class H
    {
        public A a { get; set; }
    }

    public class HighlyNested
    {
        public string a { get; set; }
        public bool b { get; set; }
        public int c { get; set; }
        public List<object> d { get; set; }
        public E e { get; set; }
        public object f { get; set; }
        public H h { get; set; }
        public List<List<List<List<List<List<List<object>>>>>>> i { get; set; }
    }

    public class BoonSmall
    {
        public string debug { get; set; }
        public IList<int> nums { get; set; }
    }

    public enum Status
    {
        Single,
        Married,
        Divorced
    }

    public interface ISomething
    {
        // Notice how "Name" isn't introduced here yet, but only in the implementation class "Stuff"
        int Id { get; set; }
    }

    public class Stuff : ISomething
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid? UniqueId { get; set; } public ulong? LargeUInt { get; set; }
        public sbyte SmallInt1 { get; set; } public sbyte SmallInt2 { get; set; }
    }

    public class StuffHolder
    {
        public IList<ISomething> Items { get; set; }
    }

    public class Asset
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Owner : Person
    {
        public IList<Asset> Assets { get; set; }
    }

    public class Owners
    {
        public IDictionary<decimal, Owner> OwnerByWealth { get; set; }
        public IDictionary<Owner, decimal> WealthByOwner { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Both string and integral enum value representations can be parsed:
        public Status Status { get; set; }

        public string Address { get; set; }

        // Just to be sure we support that one, too:
        public IEnumerable<int> Scores { get; set; }

        public object Data { get; set; }

        // Generic dictionaries are also supported; e.g.:
        // '{
        //    "Name": "F. Bastiat", ...
        //    "History": [
        //       { "key": "1801-06-30", "value": "Birth date" }, ...
        //    ]
        //  }'
        public IDictionary<DateTime, string> History { get; set; }

        // 1-char-long strings in the JSON can be deserialized into System.Char:
        public char Abc { get; set; }
    }

    public enum SomeKey
    {
        Key0, Key1, Key2, Key3, Key4,
        Key5, Key6, Key7, Key8, Key9
    }

    public class DictionaryData
    {
        public IList<IDictionary<SomeKey, string>> Dictionaries { get; set; }
    }

    public class DictionaryDataAdaptJsonNetServiceStack
    {
        public IList<
            IList<KeyValuePair<SomeKey, string>>
        > Dictionaries { get; set; }
    }

    public class FathersData
    {
        public Father[] fathers { get; set; }
    }

    public class Someone
    {
        public string name { get; set; }
    }

    public class Father : Someone
    {
        public int id { get; set; }
        public bool married { get; set; }
        // Lists...
        public List<Son> sons { get; set; }
        // ... or arrays for collections, that's fine:
        public Daughter[] daughters { get; set; }
    }

    public class Child : Someone
    {
        public int age { get; set; }
    }

    public class Son : Child
    {
    }

    public class Daughter : Child
    {
        public string maidenName { get; set; }
    }

    public enum VendorID
    {
        Vendor0,
        Vendor1,
        Vendor2,
        Vendor3,
        Vendor4,
        Vendor5
    }

    public class SampleConfigItem
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }

    public class SampleConfigData<TKey>
    {
        public Dictionary<TKey, object> ConfigItems { get; set; }
    }

    #region POCO model for SO question "Json deserializing issue c#" ( http://stackoverflow.com/questions/26426594/json-deserializing-issue-c-sharp )
    public class From
    {
        public string id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
    }

    public class Post
    {
        public string id { get; set; }
        public From from { get; set; }
        public string message { get; set; }
        public string picture { get; set; }
        public Dictionary<string, Like[]> likes { get; set; }
    }

    public class Like
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    #endregion

    #region POCO model for JSONPath Tests (POCO)
    public class Data
    {
        public Store store { get; set; }
    }

    public class Store
    {
        public Book[] book { get; set; }
        public Bicycle bicycle { get; set; }
    }

    public class Book
    {
        public string category { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public string isbn { get; set; }
        public decimal price { get; set; }
    }

    public class Bicycle
    {
        public string color { get; set; }
        public decimal price { get; set; }
    }
    #endregion

    class ParserTests
    {
        private static readonly string THE_BURNING_MONK_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}yan-cui-10k-simple-objects.json.txt", Path.DirectorySeparatorChar);
        private static readonly string OJ_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}_oj-highly-nested.json.txt", Path.DirectorySeparatorChar);
        private static readonly string BOON_SMALL_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}boon-small.json.txt", Path.DirectorySeparatorChar);
        private static readonly string TINY_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}tiny.json.txt", Path.DirectorySeparatorChar);
        private static readonly string DICOS_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}dicos.json.txt", Path.DirectorySeparatorChar);
        private static readonly string SMALL_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}small.json.txt", Path.DirectorySeparatorChar);
        private static readonly string TWITTER_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}twitter.json.txt", Path.DirectorySeparatorChar);
        private static readonly string FATHERS_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}fathers.json.txt", Path.DirectorySeparatorChar);
        private static readonly string HUGE_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}huge.json.txt", Path.DirectorySeparatorChar);

#if RUN_UNIT_TESTS
        static object UnitTest<T>(string input, Func<string, T> parse, ref int count, ref int passed) { return UnitTest(input, parse, ref count, ref passed, false); }

        static object UnitTest<T>(string input, Func<string, T> parse, ref int count, ref int passed, bool errorCase)
        {
            object obj;
            Console.WriteLine();
            Console.WriteLine(errorCase ? "(Error case)" : "(Nominal case)");
            Console.WriteLine("\tTry parse: {0} ... as: {1} ...", input, typeof(T).FullName);
            try { obj = parse(input); }
            catch (Exception ex) { obj = ex; }
            Console.WriteLine("\t... result: {0}{1}", (obj != null) ? obj.GetType().FullName : "(null)", (obj is Exception) ? " (" + ((Exception)obj).Message + ")" : String.Empty);
            Console.WriteLine();
            Console.WriteLine("Press a key...");
            Console.WriteLine();
            Console.ReadKey();
            count++;
            if (!errorCase)
            {
                passed += !(obj is Exception) ? 1 : 0;
            }
            else
            {
                passed += (obj is Exception) ? 1 : 0;
            }
            return obj;
        }

        static void UnitTests()
        {
            object obj; int count = 0, passed = 0;
            Console.Clear();
            Console.WriteLine("Press ESC to skip the unit tests or any other key to start...");
            Console.WriteLine();
            if (Console.ReadKey().KeyChar == 27)
                return;
#if RUN_BASIC_JSONPATH_TESTS || RUN_ADVANCED_JSONPATH_TESTS
            #region JSONPath Tests ( http://goessner.net/articles/JsonPath/ )
            string input = @"
              { ""store"": {
                    ""book"": [ 
                      { ""category"": ""reference"",
                            ""author"": ""Nigel Rees"",
                            ""title"": ""Sayings of the Century"",
                            ""price"": 8.95
                      },
                      { ""category"": ""fiction"",
                            ""author"": ""Evelyn Waugh"",
                            ""title"": ""Sword of Honour"",
                            ""price"": 12.99
                      },
                      { ""category"": ""fiction"",
                            ""author"": ""Herman Melville"",
                            ""title"": ""Moby Dick"",
                            ""isbn"": ""0-553-21311-3"",
                            ""price"": 8.99,
                            ""status"": ""Married""
                      },
                      { ""category"": ""fiction"",
                            ""author"": ""J. R. R. Tolkien"",
                            ""title"": ""The Lord of the Rings"",
                            ""isbn"": ""0-395-19395-8"",
                            ""price"": 22.99
                      }
                    ],
                    ""bicycle"": {
                      ""color"": ""red"",
                      ""price"": 19.95
                    }
              }
            }
        ";
            JsonPathScriptEvaluator evaluator =
                (script, value, context) =>
                    (value is Type)
                    ? // This holds: (value as Type) == typeof(Func<string, T, IJsonPathContext, object>), with T inferred by JsonPathSelection::SelectNodes(...)
                    ExpressionParser.Parse((Type)value, script, true, typeof(Data).Namespace).Compile()
                    :
                    null;
            JsonPathSelection scope;
            JsonPathNode[] nodes;

#if RUN_BASIC_JSONPATH_TESTS
            var parser1 = new JsonParser();
            var untyped = parser1.Parse(input); // (object untyped = ...)

            scope = new JsonPathSelection(untyped); // Cache the JsonPathSelection.
            nodes = scope.SelectNodes("$.store.book[3].title"); // Normalized in bracket-notation: $['store']['book'][3]['title']
            Assert
            (
                nodes != null &&
                nodes.Length == 1 &&
                nodes[0].Value is string &&
                nodes[0].As<string>() == "The Lord of the Rings"
            );

            scope = new JsonPathSelection(untyped, evaluator); // Cache the JsonPathSelection and its lambdas compiled on-demand (at run-time) by the evaluator.
            nodes = scope.SelectNodes("$.store.book[?(@.ContainsKey(\"isbn\") && (string)@[\"isbn\"] == \"0-395-19395-8\")].title");
            Assert
            (
                nodes != null &&
                nodes.Length == 1 &&
                nodes[0].Value is string &&
                nodes[0].As<string>() == "The Lord of the Rings"
            );
#endif

#if RUN_ADVANCED_JSONPATH_TESTS
            var typed = new JsonParser().Parse<Data>(input); // (Data typed = ...)

            scope = new JsonPathSelection(typed, evaluator); // Cache the JsonPathSelection and its lambdas compiled on-demand (at run-time) by the evaluator.
            nodes = scope.SelectNodes("$.store.book[?(@.title == \"The Lord of the Rings\")].price");
            Assert
            (
                nodes != null &&
                nodes.Length == 1 &&
                nodes[0].Value is decimal &&
                nodes[0].As<decimal>() == 22.99m
            );

            // Yup. This works too.
            nodes = scope.SelectNodes("$.[((@ is Data) ? \"store\" : (string)null)]"); // Dynamic member (property) selection
            Assert
            (
                nodes != null &&
                nodes.Length == 1 &&
                nodes[0].Value is Store &&
                nodes[0].As<Store>() == scope.SelectNodes("$['store']")[0].As<Store>() && // Normalized in bracket-notation
                nodes[0].As<Store>() == scope.SelectNodes("$.store")[0].As<Store>() // Common dot-notation
            );

            // And this, as well. To compare with the above '... nodes = scope.SelectNodes("$.store.book[3].title")'
            nodes = scope.
                SelectNodes
                (   // JSONPath expression template...
                    "$.[{0}].[{1}][{2}].[{3}]",
                // ... interpolated with these compile-time lambdas:
                    (script, value, context) => "store", // Member selector (by name)
                    (script, value, context) => "book", // Member selector (by name)
                    (script, value, context) => 1, // Member selector (by index)
                    (script, value, context) => "title" // Member selector (by name)
                );
            Assert
            (
                nodes != null &&
                nodes.Length == 1 &&
                nodes[0].Value is string &&
                nodes[0].As<string>() == "Sword of Honour"
            );

            // Some JSONPath expressions from Stefan Gössner's JSONPath examples ( http://goessner.net/articles/JsonPath/#e3 )...

            // Authors of all books in the store
            Assert
            (
                (nodes = scope.SelectNodes("$.store.book[*].author")).Length == 4
            );

            // Price of everything in the store
            Assert
            (
                (nodes = scope.SelectNodes("$.store..price")).Length == 5
            );

            // Third book
            Assert
            (
                (nodes = scope.SelectNodes("$..book[2]"))[0].Value is Book && nodes[0].As<Book>().isbn == "0-553-21311-3"
            );

            // Last book in order
            Assert
            (
                (nodes = scope.SelectNodes("$..book[(@.Length - 1)]"))[0].Value == scope.SelectNodes("$..book[-1:]")[0].Value
            );

            // First two books
            Assert
            (
                (nodes = scope.SelectNodes("$..book[0,1]")).Length == scope.SelectNodes("$..book[:2]").Length && nodes.Length == 2
            );

            // All books with an ISBN
            Assert
            (
                (nodes = scope.SelectNodes("$..book[?(@.isbn)]")).Length == 2
            );

            // All books cheaper than 10
            Assert
            (
                (nodes = scope.SelectNodes("$..book[?(@.price < 10m)]")).Length == 2
            );

            // Speaks for itself
            var parser = new JsonParser();
            var parsed = parser.Parse<FathersData>(System.IO.File.ReadAllText(FATHERS_TEST_FILE_PATH));
            var jsonPath = new JsonPathSelection(parsed, evaluator);
            var st = Time.Start();
            var minorSonCount = jsonPath.SelectNodes("$.fathers[*].sons[?(@.age < 18)]").Length;
            var legalSonCount = jsonPath.SelectNodes("$.fathers[*].sons[?(@.age >= 18)]").Length;
            var totalSonCount = jsonPath.SelectNodes("$.fathers[*].sons[*]").Length;
            var tm = st.ElapsedMilliseconds;
            Assert(totalSonCount == minorSonCount + legalSonCount);
            Console.WriteLine();
            Console.WriteLine("\t\t\t$.fathers[*].sons[?(@.age < 18)]\t:\t{0}", minorSonCount);
            Console.WriteLine("\t\t\t$.fathers[*].sons[?(@.age >= 18)]\t:\t{0}", legalSonCount);
            Console.WriteLine("\t\t\t$.fathers[*].sons[*]\t\t\t:\t{0}", totalSonCount);
            Console.WriteLine();
            Console.WriteLine("\t\t\t... in {0} ms.", tm.ToString("0,0"));
            Console.WriteLine();
            Console.WriteLine("Press a key...");
            Console.WriteLine();
            Console.ReadKey();

            // Anonymous type instance prototype of the target object model,
            // used for static type inference by the C# compiler (see below)
            var OBJECT_MODEL = new
            {
                country = new // (Anonymous) country
                {
                    name = default(string),
                    people = new[] // (Array of...)
                    {
                        new // (Anonymous) person
                        {
                            initials = default(string),
                            DOB = default(DateTime),
                            citizen = default(bool),
                            status = default(Status) // (Marital "Status" enumeration type)
                        }
                    }
                }
            };

            var anonymous = new JsonParser().Parse
            (
                // Anonymous type instance prototype, passed in
                // solely for type inference by the C# compiler
                OBJECT_MODEL,

                // Input
                @"{
                    ""country"": {
                        ""name"": ""USA"",
                        ""people"": [
                            {
                                ""initials"": ""VV"",
                                ""citizen"": true,
                                ""DOB"": ""1970-03-28"",
                                ""status"": ""Married""
                            },
                            {
                                ""DOB"": ""1970-05-10"",
                                ""initials"": ""CJ""
                            },
                            {
                                ""initials"": ""RP"",
                                ""DOB"": ""1935-08-20"",
                                ""status"": ""Married"",
                                ""citizen"": true
                            }
                        ]
                    }
                }"
            );

            Assert(anonymous.country.people.Length == 3);

            foreach (var person in anonymous.country.people)
                Assert
                (
                    person.initials.Length == 2 &&
                    person.DOB > new DateTime(1901, 1, 1)
                );

            scope = new JsonPathSelection(anonymous, evaluator);
            Assert
            (
                (nodes = scope.SelectNodes(@"$..people[?(!@.citizen)]")).Length == 1 &&
                nodes.As(OBJECT_MODEL.country.people[0])[0].initials == "CJ" &&
                nodes.As(OBJECT_MODEL.country.people[0])[0].DOB == new DateTime(1970, 5, 10) &&
                nodes.As(OBJECT_MODEL.country.people[0])[0].status == Status.Single
            );
#endif
            #endregion
#endif

            // A few nominal cases
            obj = UnitTest("null", s => new JsonParser().Parse(s), ref count, ref passed);
            Assert(obj == null);

            obj = UnitTest("true", s => new JsonParser().Parse(s), ref count, ref passed);
            Assert(obj is bool && (bool)obj);

            obj = UnitTest(@"""\z""", s => new JsonParser().Parse<char>(s), ref count, ref passed);
            Assert(obj is char && (char)obj == 'z');

            obj = UnitTest(@"""\t""", s => new JsonParser().Parse<char>(s), ref count, ref passed);
            Assert(obj is char && (char)obj == '\t');

            obj = UnitTest(@"""\u0021""", s => new JsonParser().Parse<char>(s), ref count, ref passed);
            Assert(obj is char && (char)obj == '!');

            obj = UnitTest(@"""\u007D""", s => new JsonParser().Parse<char>(s), ref count, ref passed);
            Assert(obj is char && (char)obj == '}');

            obj = UnitTest(@" ""\u007e"" ", s => new JsonParser().Parse<char>(s), ref count, ref passed);
            Assert(obj is char && (char)obj == '~');

            obj = UnitTest(@" ""\u202A"" ", s => new JsonParser().Parse<char>(s), ref count, ref passed);
            Assert(obj is char && (int)(char)obj == 0x202a);

            obj = UnitTest("123", s => new JsonParser().Parse<int>(s), ref count, ref passed);
            Assert(obj is int && (int)obj == 123);

            obj = UnitTest("\"\"", s => new JsonParser().Parse<string>(s), ref count, ref passed);
            Assert(obj is string && (string)obj == String.Empty);

            obj = UnitTest("\"Abc\\tdef\"", s => new JsonParser().Parse<string>(s), ref count, ref passed);
            Assert(obj is string && (string)obj == "Abc\tdef");

            obj = UnitTest("[null]", s => new JsonParser().Parse<object[]>(s), ref count, ref passed);
            Assert(obj is object[] && ((object[])obj).Length == 1 && ((object[])obj)[0] == null);

            obj = UnitTest("[true]", s => new JsonParser().Parse<IList<bool>>(s), ref count, ref passed);
            Assert(obj is IList<bool> && ((IList<bool>)obj).Count == 1 && ((IList<bool>)obj)[0]);

            obj = UnitTest("[1,2,3,4,5]", s => new JsonParser().Parse<int[]>(s), ref count, ref passed);
            Assert(obj is int[] && ((int[])obj).Length == 5 && ((int[])obj)[4] == 5);

            obj = UnitTest("123.456", s => new JsonParser().Parse<decimal>(s), ref count, ref passed);
            Assert(obj is decimal && (decimal)obj == 123.456m);

            obj = UnitTest("{\"a\":123,\"b\":true,\"o\":{\"p\":\"v\"}}", s => new JsonParser().Parse(s), ref count, ref passed);
            Assert(obj is IDictionary<string, object> && (((IDictionary<string, object>)obj)["a"] as string) == "123" && ((obj = ((IDictionary<string, object>)obj)["b"]) is bool) && (bool)obj);

            obj = UnitTest("1", s => new JsonParser().Parse<Status>(s), ref count, ref passed);
            Assert(obj is Status && (Status)obj == Status.Married);

            obj = UnitTest("\"Divorced\"", s => new JsonParser().Parse<Status>(s), ref count, ref passed);
            Assert(obj is Status && (Status)obj == Status.Divorced);

            obj = UnitTest("{\"Name\":\"Peter\",\"Status\":0}", s => new JsonParser().Parse<Person>(s), ref count, ref passed);
            Assert(obj is Person && ((Person)obj).Name == "Peter" && ((Person)obj).Status == Status.Single);

            obj = UnitTest("{\"Name\":\"Paul\",\"Status\":\"Married\"}", s => new JsonParser().Parse<Person>(s), ref count, ref passed);
            Assert(obj is Person && ((Person)obj).Name == "Paul" && ((Person)obj).Status == Status.Married);

            obj = UnitTest("{\"History\":[{\"key\":\"1801-06-30T00:00:00Z\",\"value\":\"Birth date\"}]}", s => new JsonParser().Parse<Person>(s), ref count, ref passed);
            Assert(obj is Person && ((Person)obj).History[new DateTime(1801, 6, 30, 0, 0, 0, DateTimeKind.Utc)] == "Birth date");

            obj = UnitTest(@"{""Items"":[
                {
                    ""__type"": ""Test.Stuff, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                    ""Id"": 123, ""Name"": ""Foo""
                },
                {
                    ""__type"": ""Test.Stuff, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                    ""Id"": 456, ""Name"": ""Bar"",
                    ""LargeUInt"": 18446744073709551615,
                    ""UniqueId"": ""aad737f7-0caa-4574-9ca5-f39964d50f41"",
                    ""SmallInt1"": 127,
                    ""SmallInt2"": -128
                }]}", s => new JsonParser().Parse<StuffHolder>(s), ref count, ref passed);
            Assert
            (
                obj is StuffHolder && ((StuffHolder)obj).Items.Count == 2 &&
                ((Stuff)((StuffHolder)obj).Items[1]).Name == "Bar" &&
                ((Stuff)((StuffHolder)obj).Items[1]).UniqueId.HasValue &&
                ((Stuff)((StuffHolder)obj).Items[1]).UniqueId.Value == new Guid("aad737f7-0caa-4574-9ca5-f39964d50f41") &&
                ((Stuff)((StuffHolder)obj).Items[1]).LargeUInt.HasValue &&
                ((Stuff)((StuffHolder)obj).Items[1]).LargeUInt.Value == ulong.MaxValue &&
                ((Stuff)((StuffHolder)obj).Items[1]).SmallInt1 == sbyte.MaxValue &&
                ((Stuff)((StuffHolder)obj).Items[1]).SmallInt2 == sbyte.MinValue
            );

            string configTestInputVendors = @"{
                ""ConfigItems"": {
                    ""Vendor1"": {
                        ""__type"": ""Test.SampleConfigItem, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                        ""Id"": 100,
                        ""Content"": ""config content for vendor 1""
                    },
                    ""Vendor3"": {
                        ""__type"": ""Test.SampleConfigItem, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                        ""Id"": 300,
                        ""Content"": ""config content for vendor 3""
                    }
                }
            }";

            string configTestInputIntegers = @"{
                ""ConfigItems"": {
                    ""123"": {
                        ""__type"": ""Test.SampleConfigItem, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                        ""Id"": 123000,
                        ""Content"": ""config content for key 123""
                    },
                    ""456"": {
                        ""__type"": ""Test.SampleConfigItem, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                        ""Id"": 456000,
                        ""Content"": ""config content for key 456""
                    }
                }
            }";

            obj = UnitTest(configTestInputVendors, s => new JsonParser().Parse<SampleConfigData<VendorID>>(s), ref count, ref passed);
            Assert
            (
                obj is SampleConfigData<VendorID> &&
                ((SampleConfigData<VendorID>)obj).ConfigItems.ContainsKey(VendorID.Vendor3) &&
                ((SampleConfigData<VendorID>)obj).ConfigItems[VendorID.Vendor3] is SampleConfigItem &&
                ((SampleConfigItem)((SampleConfigData<VendorID>)obj).ConfigItems[VendorID.Vendor3]).Id == 300
            );

            obj = UnitTest(configTestInputIntegers, s => new JsonParser().Parse<SampleConfigData<int>>(s), ref count, ref passed);
            Assert
            (
                obj is SampleConfigData<int> &&
                ((SampleConfigData<int>)obj).ConfigItems.ContainsKey(456) &&
                ((SampleConfigData<int>)obj).ConfigItems[456] is SampleConfigItem &&
                ((SampleConfigItem)((SampleConfigData<int>)obj).ConfigItems[456]).Id == 456000
            );

            obj = UnitTest(@"{
                ""OwnerByWealth"": {
                    ""15999.99"":
                        { ""Id"": 1,
                          ""Name"": ""Peter"",
                          ""Assets"": [
                            { ""Name"": ""Car"",
                              ""Price"": 15999.99 } ]
                        },
                    ""250000.05"":
                        { ""Id"": 2,
                          ""Name"": ""Paul"",
                          ""Assets"": [
                            { ""Name"": ""House"",
                              ""Price"": 250000.05 } ]
                        }
                },
                ""WealthByOwner"": [
                    { ""key"": { ""Id"": 1, ""Name"": ""Peter"" }, ""value"": 15999.99 },
                    { ""key"": { ""Id"": 2, ""Name"": ""Paul"" }, ""value"": 250000.05 }
                ]
            }", s => new JsonParser().Parse<Owners>(s), ref count, ref passed);
            Owner peter, owner;
            Assert
            (
                (obj is Owners) &&
                (peter = ((Owners)obj).WealthByOwner.Keys.
                    Where(person => person.Name == "Peter").FirstOrDefault()
                ) != null &&
                (owner = ((Owners)obj).OwnerByWealth[15999.99m]) != null &&
                (owner.Name == peter.Name) &&
                (owner.Assets.Count == 1) &&
                (owner.Assets[0].Name == "Car")
            );

            var nea = (Status?[])UnitTest(@"[1,""Married"",null,2]", s => new JsonParser().Parse<Status?[]>(s), ref count, ref passed);
            Assert(nea[0].Value == Status.Married && nea[1].Value == nea[0].Value && !nea[2].HasValue && nea[3].Value == Status.Divorced);

            var anon = new { Int1 = default(int?), Stat = default(Status?), Stat2 = default(Status?), Stat3 = default(Status?) };
            anon = new JsonParser().Parse(anon, @"{""Int1"":null,""Stat"":1,""Stat3"":null}");
            Assert(!anon.Int1.HasValue && anon.Stat == Status.Married && !anon.Stat2.HasValue && !anon.Stat3.HasValue);

            var someGuys = @"{
                ""Jean-Pierre"": ""Single"",
                ""Jean-Philippe"": 1,
                ""Jean-Paul"": ""Divorced"",
                ""Jean-Jacques"": null
            }";
            var someStatuses =
                new JsonParser().Parse<Dictionary<string, Status?>>(someGuys);
            var statusCheck =
                someStatuses["Jean-Pierre"].Value == Status.Single &&
                someStatuses["Jean-Philippe"].Value == Status.Married &&
                someStatuses["Jean-Paul"].Value == Status.Divorced &&
                !someStatuses["Jean-Jacques"].HasValue; // (we don't know about his...)
            Assert(statusCheck);

            var someGuys2 = @"{
                ""Single"": [ ""Jean-Pierre"" ],
                ""Married"": [ ""Jean-Philippe"" ],
                ""2"": [ ""Jean-Paul"" ]
            }";
            var someStatuses2 =
                new JsonParser().Parse<Dictionary<Status?, string[]>>(someGuys2);
            var statusCheck2 =
                someStatuses2[Status.Single][0] == "Jean-Pierre" &&
                someStatuses2[Status.Married][0] == "Jean-Philippe" &&
                someStatuses2[Status.Divorced][0] == "Jean-Paul";
                // (we still don't know about Jean-Jacques' status...)
            Assert(statusCheck2);

#if !THIS_JSON_PARSER_ONLY
            // Support for Json.NET's "$type" pseudo-key (in addition to ServiceStack's "__type"):
            Person jsonNetPerson = new Person { Id = 123, Abc = '#', Name = "Foo", Scores = new[] { 100, 200, 300 } };

            // (Expected serialized form shown in next comment)
            string jsonNetString = JsonConvert.SerializeObject(jsonNetPerson, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
            // => '{"$type":"Test.ParserTests+Person, Test","Id":123,"Name":"Foo","Status":0,"Address":null,"Scores":[100,200,300],"Data":null,"History":null,"Abc":"#"}'

            // (Note the Parse<object>(...))
            object restoredObject = UnitTest(jsonNetString, s => new JsonParser().Parse<object>(jsonNetString), ref count, ref passed);
            Assert
            (
                restoredObject is Person &&
                ((Person)restoredObject).Name == "Foo" &&
                ((Person)restoredObject).Abc == '#' &&
                ((IList<int>)((Person)restoredObject).Scores).Count == 3 &&
                ((IList<int>)((Person)restoredObject).Scores)[2] == 300
            );
#endif

            var SO_26426594_input = @"{ ""data"": [
    {
      ""id"": ""post 1"", 
      ""from"": {
        ""category"": ""Local business"", 
        ""name"": ""..."", 
        ""id"": ""...""
      }, 
      ""message"": ""..."", 
      ""picture"": ""..."", 
      ""likes"": {
        ""data"": [
          {
            ""id"": ""like 1"", 
            ""name"": ""text 1...""
          }, 
          {
            ""id"": ""like 2"", 
            ""name"": ""text 2...""
          }
        ]
      }
    }
] }";
            var posts =
                (
                    UnitTest(
                        SO_26426594_input,
                        FacebookPostDeserialization_SO_26426594, ref count, ref passed
                    ) as
                    Dictionary<string, Post[]>
                );
            Assert(posts != null && posts["data"][0].id == "post 1");
            Assert(posts != null && posts["data"][0].from.category == "Local business");
            Assert(posts != null && posts["data"][0].likes["data"][0].id == "like 1");
            Assert(posts != null && posts["data"][0].likes["data"][1].id == "like 2");

            // A few error cases
            obj = UnitTest("\"unfinished", s => new JsonParser().Parse<string>(s), ref count, ref passed, true);
            Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Bad string"));

            obj = UnitTest("[123]", s => new JsonParser().Parse<string[]>(s), ref count, ref passed, true);
            Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Bad string"));

            obj = UnitTest("[null]", s => new JsonParser().Parse<short[]>(s), ref count, ref passed, true);
            Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Bad number (short)"));

            obj = UnitTest("[123.456]", s => new JsonParser().Parse<int[]>(s), ref count, ref passed, true);
            Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Unexpected character at 4"));

            obj = UnitTest("\"Unknown\"", s => new JsonParser().Parse<Status>(s), ref count, ref passed, true);
            Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Bad enum value"));

            Console.Clear();
            Console.WriteLine("... Unit tests done: {0} passed, out of {1}.", passed, count);
            Console.WriteLine();
            Console.WriteLine("Press a key to start the speed tests...");
            Console.ReadKey();
        }
#endif

        static void LoopTest<T>(string parserName, Func<string, T> parseFunc, string testFile, int count)
        {
            Console.Clear();
            Console.WriteLine("Parser: {0}", parserName);
            Console.WriteLine();
            Console.WriteLine("Loop Test File: {0}", testFile);
            Console.WriteLine();
            Console.WriteLine("Iterations: {0}", count.ToString("0,0"));
            Console.WriteLine();
            Console.WriteLine("Deserialization: {0}", (typeof(T) != typeof(object)) ? "POCO(s)" : "loosely-typed");
            Console.WriteLine();
            Console.WriteLine("Press ESC to skip this test or any other key to start...");
            Console.WriteLine();
            if (Console.ReadKey().KeyChar == 27)
                return;

            System.Threading.Thread.MemoryBarrier();
            var initialMemory = System.GC.GetTotalMemory(true);

            var json = System.IO.File.ReadAllText(testFile);
            var st = Time.Start();
            var l = new List<object>();
            for (var i = 0; i < count; i++)
                l.Add(parseFunc(json));
            var tm = st.ElapsedMilliseconds;

            System.Threading.Thread.MemoryBarrier();
            var finalMemory = System.GC.GetTotalMemory(true);
            var consumption = finalMemory - initialMemory;

            Assert(l.Count == count);

            Console.WriteLine();
            Console.WriteLine("... Done, in {0} ms. Throughput: {1} characters / second.", tm.ToString("0,0"), (1000 * (decimal)(count * json.Length) / (tm > 0 ? tm : 1)).ToString("0,0.00"));
            Console.WriteLine();
            Console.WriteLine("\tMemory used : {0}", ((decimal)finalMemory).ToString("0,0"));
            Console.WriteLine();

            GC.Collect();
            Console.WriteLine("Press a key...");
            Console.WriteLine();
            Console.ReadKey();
        }

        static void Test<T>(string parserName, Func<string, T> parseFunc, string testFile)
        {
            Test<T>(parserName, parseFunc, testFile, null, null, null);
        }

        static void Test<T>(string parserName, Func<string, T> parseFunc, string testFile, string jsonPathExpression, Func<object, object> jsonPathSelect, Func<object, bool> jsonPathAssert)
        {
            var jsonPathTest = !String.IsNullOrWhiteSpace(jsonPathExpression) && (jsonPathSelect != null) && (jsonPathAssert != null);
            Console.Clear();
            Console.WriteLine("{0}Parser: {1}", (jsonPathTest ? "(With JSONPath selection test) " : String.Empty), parserName);
            Console.WriteLine();
            Console.WriteLine("Test File: {0}", testFile);
            Console.WriteLine();
            Console.WriteLine("Deserialization: {0}", (typeof(T) != typeof(object)) ? "POCO(s)" : "loosely-typed");
            Console.WriteLine();
            if (jsonPathTest)
            {
                Console.WriteLine("JSONPath expression: {0}", jsonPathExpression);
                Console.WriteLine();
            }
            Console.WriteLine("Press ESC to skip this test or any other key to start...");
            Console.WriteLine();
            if (Console.ReadKey().KeyChar == 27)
                return;

            System.Threading.Thread.MemoryBarrier();
            var initialMemory = System.GC.GetTotalMemory(true);

            var json = System.IO.File.ReadAllText(testFile);
            var st = Time.Start();
            var o = parseFunc(json);
            if (jsonPathTest)
            {
                var selection = jsonPathSelect(o);
                var assertion = jsonPathAssert(selection);
                Assert(assertion);
            }
            var tm = st.ElapsedMilliseconds;

            System.Threading.Thread.MemoryBarrier();
            var finalMemory = System.GC.GetTotalMemory(true);
            var consumption = finalMemory - initialMemory;

            Console.WriteLine();
            Console.WriteLine("... Done, in {0} ms. Throughput: {1} characters / second.", tm.ToString("0,0"), (1000 * (decimal)json.Length / (tm > 0 ? tm : 1)).ToString("0,0.00"));
            Console.WriteLine();
            Console.WriteLine("\tMemory used : {0}", ((decimal)finalMemory).ToString("0,0"));
            Console.WriteLine();

            if (o is FathersData)
            {
                Console.WriteLine("Fathers : {0}", ((FathersData)(object)o).fathers.Length.ToString("0,0"));
                Console.WriteLine();
            }
            GC.Collect();
            Console.WriteLine("Press a key...");
            Console.WriteLine();
            Console.ReadKey();
        }

        public sealed class Time
        {
            private System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
            private Time() { }
            public static Time Start() { return new Time(); }
            public long Reset() { w.Stop(); var t = (long)w.ElapsedMilliseconds; w.Restart(); return t; }
            public long ElapsedMilliseconds { get { return Reset(); } }
        }

        static void SpeedTests()
        {
#if !THIS_JSON_PARSER_ONLY
            //LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<TheBurningMonk.RootObject>, THE_BURNING_MONK_TEST_FILE_PATH, 10);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<TheBurningMonk.RootObject>, THE_BURNING_MONK_TEST_FILE_PATH, 10);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<TheBurningMonk.RootObject>().DeserializeFromString, THE_BURNING_MONK_TEST_FILE_PATH, 10);
#endif
#if RUN_NETJSON_TESTS
            LoopTest(GetVersionString(typeof(NetJSON.NetJSON).Assembly.GetName()), NetJSON.NetJSON.Deserialize<TheBurningMonk.RootObject>, THE_BURNING_MONK_TEST_FILE_PATH, 10);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<TheBurningMonk.RootObject>, THE_BURNING_MONK_TEST_FILE_PATH, 10);

#if !THIS_JSON_PARSER_ONLY
            //LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<TheBurningMonk.RootObject>, THE_BURNING_MONK_TEST_FILE_PATH, 100);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<TheBurningMonk.RootObject>, THE_BURNING_MONK_TEST_FILE_PATH, 100);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<TheBurningMonk.RootObject>().DeserializeFromString, THE_BURNING_MONK_TEST_FILE_PATH, 100);
#endif
#if RUN_NETJSON_TESTS
            LoopTest(GetVersionString(typeof(NetJSON.NetJSON).Assembly.GetName()), NetJSON.NetJSON.Deserialize<TheBurningMonk.RootObject>, THE_BURNING_MONK_TEST_FILE_PATH, 100);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<TheBurningMonk.RootObject>, THE_BURNING_MONK_TEST_FILE_PATH, 100);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, OJ_TEST_FILE_PATH, 10000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject, OJ_TEST_FILE_PATH, 10000);
#if RUN_SERVICESTACK_TESTS
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, OJ_TEST_FILE_PATH, 10000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse, OJ_TEST_FILE_PATH, 10000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<HighlyNested>, OJ_TEST_FILE_PATH, 10000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<HighlyNested>, OJ_TEST_FILE_PATH, 10000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<HighlyNested>().DeserializeFromString, OJ_TEST_FILE_PATH, 10000);
#endif
#if RUN_NETJSON_TESTS
            LoopTest(GetVersionString(typeof(NetJSON.NetJSON).Assembly.GetName()), NetJSON.NetJSON.Deserialize<HighlyNested>, OJ_TEST_FILE_PATH, 10000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<HighlyNested>, OJ_TEST_FILE_PATH, 10000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, OJ_TEST_FILE_PATH, 100000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject, OJ_TEST_FILE_PATH, 100000);
#if RUN_SERVICESTACK_TESTS
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, OJ_TEST_FILE_PATH, 100000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse, OJ_TEST_FILE_PATH, 100000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<HighlyNested>, OJ_TEST_FILE_PATH, 100000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<HighlyNested>, OJ_TEST_FILE_PATH, 100000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<HighlyNested>().DeserializeFromString, OJ_TEST_FILE_PATH, 100000);
#endif
#if RUN_NETJSON_TESTS
            LoopTest(GetVersionString(typeof(NetJSON.NetJSON).Assembly.GetName()), NetJSON.NetJSON.Deserialize<HighlyNested>, OJ_TEST_FILE_PATH, 100000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<HighlyNested>, OJ_TEST_FILE_PATH, 100000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 1000000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 1000000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<BoonSmall>().DeserializeFromString, BOON_SMALL_TEST_FILE_PATH, 1000000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 1000000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 10000000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 10000000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<BoonSmall>().DeserializeFromString, BOON_SMALL_TEST_FILE_PATH, 10000000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 10000000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<Person>, TINY_TEST_FILE_PATH, 10000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<Person>, TINY_TEST_FILE_PATH, 10000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<Person>().DeserializeFromString, TINY_TEST_FILE_PATH, 10000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<Person>, TINY_TEST_FILE_PATH, 10000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<Person>, TINY_TEST_FILE_PATH, 100000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<Person>, TINY_TEST_FILE_PATH, 100000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<Person>().DeserializeFromString, TINY_TEST_FILE_PATH, 100000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<Person>, TINY_TEST_FILE_PATH, 100000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<Person>, TINY_TEST_FILE_PATH, 1000000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<Person>, TINY_TEST_FILE_PATH, 1000000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<Person>().DeserializeFromString, TINY_TEST_FILE_PATH, 1000000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<Person>, TINY_TEST_FILE_PATH, 1000000);

#if !THIS_JSON_PARSER_ONLY
            //LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 10000);//(Can't deserialize properly)
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 10000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<DictionaryDataAdaptJsonNetServiceStack>().DeserializeFromString, DICOS_TEST_FILE_PATH, 10000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<DictionaryData>, DICOS_TEST_FILE_PATH, 10000);

#if !THIS_JSON_PARSER_ONLY
            //LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 100000);//(Can't deserialize properly)
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 100000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<DictionaryDataAdaptJsonNetServiceStack>().DeserializeFromString, DICOS_TEST_FILE_PATH, 100000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<DictionaryData>, DICOS_TEST_FILE_PATH, 100000);

#if !THIS_JSON_PARSER_ONLY
            //LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 1000000);//(Can't deserialize properly)
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 1000000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<DictionaryDataAdaptJsonNetServiceStack>().DeserializeFromString, DICOS_TEST_FILE_PATH, 1000000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<DictionaryData>, DICOS_TEST_FILE_PATH, 1000000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, SMALL_TEST_FILE_PATH, 10000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject, SMALL_TEST_FILE_PATH, 10000);
#if RUN_SERVICESTACK_TESTS
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, SMALL_TEST_FILE_PATH, 10000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse, SMALL_TEST_FILE_PATH, 10000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, SMALL_TEST_FILE_PATH, 100000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject, SMALL_TEST_FILE_PATH, 100000);//(Json.NET: OutOfMemoryException)
#if RUN_SERVICESTACK_TESTS
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, SMALL_TEST_FILE_PATH, 100000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse, SMALL_TEST_FILE_PATH, 100000);

#if !THIS_JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<TwitterSample.RootObject>, TWITTER_TEST_FILE_PATH, 100000);
            LoopTest(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<TwitterSample.RootObject>, TWITTER_TEST_FILE_PATH, 100000);
#if RUN_SERVICESTACK_TESTS
            LoopTest("ServiceStack", new JsonSerializer<TwitterSample.RootObject>().DeserializeFromString, TWITTER_TEST_FILE_PATH, 100000);
#endif
#if RUN_NETJSON_TESTS
            LoopTest(GetVersionString(typeof(NetJSON.NetJSON).Assembly.GetName()), NetJSON.NetJSON.Deserialize<TwitterSample.RootObject>, TWITTER_TEST_FILE_PATH, 100000);
#endif
#endif
            LoopTest(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<TwitterSample.RootObject>, TWITTER_TEST_FILE_PATH, 100000);

#if !THIS_JSON_PARSER_ONLY
            var msJss = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
            Test(typeof(JavaScriptSerializer).FullName, msJss.Deserialize<FathersData>, FATHERS_TEST_FILE_PATH);
            Test(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject<FathersData>, FATHERS_TEST_FILE_PATH);
#if RUN_SERVICESTACK_TESTS
            Test("ServiceStack", new JsonSerializer<FathersData>().DeserializeFromString, FATHERS_TEST_FILE_PATH);
#endif
#if RUN_NETJSON_TESTS // Has an issue with NetJSON 1.0.7...
            //Test(GetVersionString(typeof(NetJSON.NetJSON).Assembly.GetName()), NetJSON.NetJSON.Deserialize<FathersData>, FATHERS_TEST_FILE_PATH);
#endif
#endif
            Test(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<FathersData>, FATHERS_TEST_FILE_PATH);

#if RUN_UNIT_TESTS && RUN_ADVANCED_JSONPATH_TESTS
            JsonPathScriptEvaluator evaluator =
                (script, value, context) =>
                    (value is Type)
                    ? // This holds: (value as Type) == typeof(Func<string, T, IJsonPathContext, object>), with T inferred by JsonPathSelection::SelectNodes(...)
                    ExpressionParser.Parse((Type)value, script, true, typeof(Data).Namespace).Compile()
                    :
                    null;
            var JSONPATH_SAMPLE_QUERY = "$.fathers[?(@.id == 28149)].daughters[?(@.age == 12)]";
#if !THIS_JSON_PARSER_ONLY
            Test // Note: requires Json.NET 6.0+
            (
                GetVersionString(typeof(JsonConvert).Assembly.GetName()), Newtonsoft.Json.Linq.JObject.Parse, FATHERS_TEST_FILE_PATH,
                JSONPATH_SAMPLE_QUERY,
                (parsed) => ((Newtonsoft.Json.Linq.JObject)parsed).SelectToken(JSONPATH_SAMPLE_QUERY),
                (selected) => ((Newtonsoft.Json.Linq.JToken)selected).Value<string>("name") == "Susan"
            );
#endif
            Test
            (
                GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse<FathersData>, FATHERS_TEST_FILE_PATH,
                JSONPATH_SAMPLE_QUERY,
                (parsed) => new JsonPathSelection(parsed, evaluator).SelectNodes(JSONPATH_SAMPLE_QUERY),
                (selected) =>
                    (selected as JsonPathNode[]).Length > 0 &&
                    (selected as JsonPathNode[])[0].Value is Daughter &&
                    (selected as JsonPathNode[])[0].As<Daughter>().name == "Susan"
            );
#endif

            if (File.Exists(HUGE_TEST_FILE_PATH))
            {
#if !THIS_JSON_PARSER_ONLY
                Test(typeof(JavaScriptSerializer).FullName, msJss.DeserializeObject, HUGE_TEST_FILE_PATH);
                Test(GetVersionString(typeof(JsonConvert).Assembly.GetName()), JsonConvert.DeserializeObject, HUGE_TEST_FILE_PATH);//(Json.NET: OutOfMemoryException)
#if RUN_SERVICESTACK_TESTS
                //Test("ServiceStack", new JsonSerializer<object>().DeserializeFromString, HUGE_TEST_FILE_PATH);
#endif
#endif
                Test(GetVersionString(typeof(JsonParser).Assembly.GetName()), new JsonParser().Parse, HUGE_TEST_FILE_PATH);
            }

            StreamTest(null);

            // Note this will be invoked thru the filters dictionary passed to this 2nd "StreamTest" below, in order to:
            // 1) for each "Father", skip the parsing of the properties to be ignored (i.e., all but "id" and "name")
            // 2) while populating the resulting "Father[]" array, skip the deserialization of the first 29,995 fathers
            Func<object, object> mapperCallback = obj =>
            {
                Father father = (obj as Father);
                // Output only the individual fathers that the filters decided to keep
                // (i.e., when obj.Type equals typeof(Father)),
                // but don't output (even once) the resulting array
                // (i.e., when obj.Type equals typeof(Father[])):
                if (father != null)
                {
                    Console.WriteLine("\t\tId : {0}\t\tName : {1}", father.id, father.name);
                }
                // Do not project the filtered data in any specific way otherwise, just return it deserialized as-is:
                return obj;
            };

            StreamTest
            (
                new Dictionary<Type, Func<Type, object, object, int, Func<object, object>>>
                {
                    // We don't care about anything but these 2 properties:
                    {
                        typeof(Father),
                        (type, obj, key, index) =>
                            ((key as string) == "id" || (key as string) == "name") ?
                            mapperCallback :
                            JsonParser.Skip
                    },
                    // We want to pick only the last 5 fathers from the source:
                    {
                        typeof(Father[]),
                        (type, obj, key, index) =>
                            (index >= 29995) ?
                            mapperCallback :
                            JsonParser.Skip
                    }
                }
            );

            FilteredFatherStreamTestDaughterMaidenNamesFixup();
        }

        static void StreamTest(IDictionary<Type, Func<Type, object, object, int, Func<object, object>>> filter)
        {
            Console.Clear();
            System.Threading.Thread.MemoryBarrier();
            var initialMemory = System.GC.GetTotalMemory(true);
            using (var reader = new System.IO.StreamReader(FATHERS_TEST_FILE_PATH))
            {
                Console.WriteLine("\"Fathers\" Test... streamed{0} (press a key)", (filter != null) ? " AND filtered" : String.Empty);
                Console.WriteLine();
                Console.ReadKey();
                var st = Time.Start();
                var o = new JsonParser().Parse<FathersData>(reader, filter);
                var tm = st.ElapsedMilliseconds;

                System.Threading.Thread.MemoryBarrier();
                var finalMemory = System.GC.GetTotalMemory(true);
                var consumption = finalMemory - initialMemory;

                Console.WriteLine();
                if (filter == null)
                {
                    Assert(o.fathers.Length == 30000);
                }
                Console.WriteLine();
                Console.WriteLine("... {0} ms", tm.ToString("0,0"));
                Console.WriteLine();
                Console.WriteLine("\tMemory used : {0}", ((decimal)finalMemory).ToString("0,0"));
                Console.WriteLine();
            }
            Console.ReadKey();
        }

        static Dictionary<string, Post[]> FacebookPostDeserialization_SO_26426594(string input)
        {
            return new JsonParser().Parse<Dictionary<string, Post[]>>(input);
        }

        // Existing test (above) simplified for SO question "Deserialize json array stream one item at a time":
        // ( http://stackoverflow.com/questions/20374083/deserialize-json-array-stream-one-item-at-a-time )
        static void FilteredFatherStreamTestSimplified()
        {
            // Get our parser:
            var parser = new JsonParser();

            // (Note this will be invoked thanks to the "filters" dictionary below)
            Func<object, object> filteredFatherStreamCallback = obj =>
            {
                Father father = (obj as Father);
                // Output only the individual fathers that the filters decided to keep (i.e., when obj.Type equals typeof(Father)),
                // but don't output (even once) the resulting array (i.e., when obj.Type equals typeof(Father[])):
                if (father != null)
                {
                    Console.WriteLine("\t\tId : {0}\t\tName : {1}", father.id, father.name);
                }
                // Do not project the filtered data in any specific way otherwise,
                // just return it deserialized as-is:
                return obj;
            };

            // Prepare our filter, and thus:
            // 1) we want only the last five (5) fathers (array index in the resulting "Father[]" >= 29,995),
            // (assuming we somehow have prior knowledge that the total count is 30,000)
            // and for each of them,
            // 2) we're interested in deserializing them with only their "id" and "name" properties
            var filters =
                new Dictionary<Type, Func<Type, object, object, int, Func<object, object>>>
                {
                    // We don't care about anything but these 2 properties:
                    {
                        typeof(Father), // Note the type
                        (type, obj, key, index) =>
                            ((key as string) == "id" || (key as string) == "name") ?
                            filteredFatherStreamCallback :
                            JsonParser.Skip
                    },
                    // We want to pick only the last 5 fathers from the source:
                    {
                        typeof(Father[]), // Note the type
                        (type, obj, key, index) =>
                            (index >= 29995) ?
                            filteredFatherStreamCallback :
                            JsonParser.Skip
                    }
                };

            // Read, parse, and deserialize fathers.json.txt in a streamed fashion,
            // and using the above filters, along with the callback we've set up:
            using (var reader = new System.IO.StreamReader(FATHERS_TEST_FILE_PATH))
            {
                FathersData data = parser.Parse<FathersData>(reader, filters);

                Assert
                (
                    (data != null) &&
                    (data.fathers != null) &&
                    (data.fathers.Length == 5)
                );
                foreach (var i in Enumerable.Range(29995, 5))
                    Assert
                    (
                        (data.fathers[i - 29995].id == i) &&
                        !String.IsNullOrEmpty(data.fathers[i - 29995].name)
                    );
            }
            Console.ReadKey();
        }

        // This test deserializes the last ten (10) fathers found in fathers.json.txt,
        // and performs a fixup of the maiden names (all absent from fathers.json.txt)
        // of their daughters (if any):
        static void FilteredFatherStreamTestDaughterMaidenNamesFixup()
        {
            Console.Clear();
            Console.WriteLine("\"Fathers\" Test... streamed AND filtered");
            Console.WriteLine();
            Console.WriteLine("(static void FilteredFatherStreamTestDaughterMaidenNamesFixup())");
            Console.WriteLine();
            Console.WriteLine("(press a key)");
            Console.WriteLine();
            Console.ReadKey();

            // Get our parser:
            var parser = new JsonParser();

            // (Note this will be invoked thanks to the "filters" dictionary below)
            Func<object, object> filteredFatherStreamCallback = obj =>
            {
                Father father = (obj as Father);
                // Processes only the individual fathers that the filters decided to keep
                // (i.e., iff obj.Type equals typeof(Father))
                if (father != null)
                {
                    if ((father.daughters != null) && (father.daughters.Length > 0))
                        // The fixup of the maiden names is done in-place, on
                        // by-then freshly deserialized father's daughters:
                        foreach (var daughter in father.daughters)
                            daughter.maidenName = father.name.Substring(father.name.IndexOf(' ') + 1);
                }
                // Do not project the filtered data in any specific way otherwise,
                // just return it deserialized as-is:
                return obj;
            };

            // Prepare our filters, i.e., we want only the last ten (10) fathers
            // (array index in the resulting "Father[]" >= 29990)
            var filters =
                new Dictionary<Type, Func<Type, object, object, int, Func<object, object>>>
                {
                    // Necessary to perform post-processing on the daughters (if any)
                    // of each father we kept in "Father[]" via the 2nd filter below:
                    {
                        typeof(Father), // Note the type
                        (type, obj, key, index) => filteredFatherStreamCallback
                    },
                    // We want to pick only the last 10 fathers from the source:
                    {
                        typeof(Father[]), // Note the type
                        (type, obj, key, index) =>
                            (index >= 29990) ?
                            filteredFatherStreamCallback :
                            JsonParser.Skip
                    }
                };

            // Read, parse, and deserialize fathers.json.txt in a streamed fashion,
            // and using the above filters, along with the callback we've set up:
            using (var reader = new System.IO.StreamReader(FATHERS_TEST_FILE_PATH))
            {
                FathersData data = parser.Parse<FathersData>(reader, filters);

                Assert
                (
                    (data != null) &&
                    (data.fathers != null) &&
                    (data.fathers.Length == 10)
                );
                foreach (var father in data.fathers)
                {
                    Console.WriteLine();
                    Console.WriteLine("\t\t{0}'s daughters:", father.name);
                    if ((father.daughters != null) && (father.daughters.Length > 0))
                        foreach (var daughter in father.daughters)
                        {
                            Assert
                            (
                                !String.IsNullOrEmpty(daughter.maidenName)
                            );
                            Console.WriteLine("\t\t\t\t{0} {1}", daughter.name, daughter.maidenName);
                        }
                    else
                        Console.WriteLine("\t\t\t\t(None)");
                }
            }
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }

        static string GetVersionString(System.Reflection.AssemblyName assemblyName)
        {
            return String.Format("{0} {1}.{2}.{3}.{4}", assemblyName.Name, assemblyName.Version.Major, assemblyName.Version.MajorRevision, assemblyName.Version.Minor, assemblyName.Version.MinorRevision);
        }

        public static void Run()
        {
#if RUN_UNIT_TESTS
            UnitTests();
#endif
            SpeedTests();
        }

        public static void Assert(bool condition)
        {
            //System.Diagnostics.Debug.Assert(condition);
            if (!condition)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("\t\t\t\t^^^ TEST FAILED!");
            }
        }
    }
}

namespace TheBurningMonk
{
    public class RootObject
    {
        public SimpleObject[] All { get; set; }
    }

    public class SimpleObject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public int[] Scores { get; set; }
    }
}

namespace TwitterSample
{
    public class Url
    {
        public object expanded_url { get; set; }
        public string url { get; set; }
        public List<int> indices { get; set; }
    }

    public class UserMention
    {
        public string name { get; set; }
        public string id_str { get; set; }
        public int id { get; set; }
        public List<int> indices { get; set; }
        public string screen_name { get; set; }
    }

    public class Entities
    {
        public List<Url> urls { get; set; }
        public List<object> hashtags { get; set; }
        public List<UserMention> user_mentions { get; set; }
    }

    public class User
    {
        public string profile_sidebar_border_color { get; set; }
        public string name { get; set; }
        public string profile_sidebar_fill_color { get; set; }
        public bool profile_background_tile { get; set; }
        public string profile_image_url { get; set; }
        public string location { get; set; }
        public string created_at { get; set; }
        public string id_str { get; set; }
        public bool follow_request_sent { get; set; }
        public string profile_link_color { get; set; }
        public int favourites_count { get; set; }
        public string url { get; set; }
        public bool contributors_enabled { get; set; }
        public int utc_offset { get; set; }
        public int id { get; set; }
        public bool profile_use_background_image { get; set; }
        public int listed_count { get; set; }
        public bool @protected { get; set; }
        public string lang { get; set; }
        public string profile_text_color { get; set; }
        public int followers_count { get; set; }
        public string time_zone { get; set; }
        public bool verified { get; set; }
        public bool geo_enabled { get; set; }
        public string profile_background_color { get; set; }
        public bool notifications { get; set; }
        public string description { get; set; }
        public int friends_count { get; set; }
        public string profile_background_image_url { get; set; }
        public int statuses_count { get; set; }
        public string screen_name { get; set; }
        public bool following { get; set; }
        public bool show_all_inline_media { get; set; }
    }

    public class RootObject
    {
        public object coordinates { get; set; }
        public string created_at { get; set; }
        public bool favorited { get; set; }
        public bool truncated { get; set; }
        public string id_str { get; set; }
        public Entities entities { get; set; }
        public object in_reply_to_user_id_str { get; set; }
        public string text { get; set; }
        public object contributors { get; set; }
        public long id { get; set; }
        public object retweet_count { get; set; }
        public object in_reply_to_status_id_str { get; set; }
        public object geo { get; set; }
        public bool retweeted { get; set; }
        public object in_reply_to_user_id { get; set; }
        public User user { get; set; }
        public object in_reply_to_screen_name { get; set; }
        public string source { get; set; }
        public object place { get; set; }
        public object in_reply_to_status_id { get; set; }
    }
}
