using NUnit.Framework;
using Sys.Text.Json;
using Sys.Text.Json.JsonPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.JsonPath;
using System.Text.Json.JsonPath.LambdaCompilation;

namespace PatherTests
{
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

    public class PatherTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestPatherString()
        {
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
                            ""price"": 12.99,
                            ""onshell"": null
                      },
                      { ""category"": ""fiction"",
                            ""author"": ""Herman Melville"",
                            ""title"": ""Moby Dick"",
                            ""isbn"": ""0-553-21311-3"",
                            ""price"": 8.99,
                            ""status"": ""Married"",
                            ""onshell"": false
                      },
                      { ""category"": ""fiction"",
                            ""author"": ""J. R. R. Tolkien"",
                            ""title"": ""The Lord of the Rings"",
                            ""isbn"": ""0-395-19395-8"",
                            ""price"": 22.99,
                            ""onshell"": true,
                            ""borrowHistroy"":[]
                      }
                    ],
                    ""bicycle"": {
                      ""color"": ""red"",
                      ""price"": 19.95,
                      ""nullable"": null,
                      ""spares"":[""bell"",""brakeX2""]  ,
                      ""travelDistances"":[10,14,9]
                    }
              }
            }
        ";
            var parser1 = new JsonPather();
            var untyped = parser1.Parse(input); // (object untyped = ...)
            var dic = (Dictionary<string, object>)untyped;

            Assert.IsTrue(dic.Count == 1);
            dic = (Dictionary<string,object>)dic["store"];
            Assert.IsTrue(dic.Count== 2);

            var books=  dic["book"];

            var book = ((IList<object>)books).ToArray()[2];
            var bookProps = (Dictionary<string, object>)book;
            Assert.IsTrue(bookProps["onshell"].ToString()=="false");
            dic = (Dictionary<string, object>)dic["bicycle"];
            Assert.IsTrue(dic.Count== 5);

            Assert.IsNull(dic["nullable"]);


            JsonPathScriptEvaluator evaluator =
                (script, value, context) =>
                    (value is Type)
                    ? // This holds: (value as Type) == typeof(Func<string, T, IJsonPathContext, object>), with T inferred by JsonPathSelection::SelectNodes(...)
                    ExpressionParser.Parse((Type)value, script, true, typeof(Data).Namespace).Compile()
                    :
                    null;
            JsonPathSelection scope;
            JsonPathNode[] nodes;

            scope = new JsonPathSelection(untyped); // Cache the JsonPathSelection.
            nodes = scope.SelectNodes("$.store.book[3].title"); // Normalized in bracket-notation: $['store']['book'][3]['title']
            Assert.IsTrue(
                nodes != null&&
                nodes.Length == 1 &&
                nodes[0].Value is string &&
                nodes[0].As<string>() == "The Lord of the Rings" 
    
            );

            scope = new JsonPathSelection(untyped, evaluator); // Cache the JsonPathSelection and its lambdas compiled on-demand (at run-time) by the evaluator.
            nodes = scope.SelectNodes("$.store.book[?(@.ContainsKey(\"isbn\") && (string)@[\"isbn\"] == \"0-395-19395-8\")].title");
            Assert.IsTrue
            (
                nodes != null &&
                nodes.Length == 1 &&
                nodes[0].Value is string &&
                nodes[0].As<string>() == "The Lord of the Rings"
            );           

        }
    }
}