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
              { 
""emptyObject"":{},
""store"": {
                    ""book"": [ 
                      { ""category"": ""reference"",
                            ""author"": ""Nigel\u2019 Rees"",
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
                            ""borrowHistroy"":[true, false]
                      }
                    ],
                    ""bicycle"": {
                      ""color"": ""red"",
                      ""price"": 19.95,
                      ""nullable"": null,
                      ""spares"":[""bell"",""brakeX2""]  ,
                      ""travelDistances"":[10,14,9],
                      ""emptyObject"":{}
                    }
              }
            }
        ";
            var parser1 = new JsonPather();
            var untyped = parser1.Parse(input); // (object untyped = ...)
            var dic = untyped;

            Assert.IsTrue(dic.Count > 1);

            var paths = dic.Keys.Where(k => k.Contains("store"));
            Assert.AreEqual( 32, paths.Count());

            var books = dic.Keys.Where(k => k.Contains("book"));

            dic.TryGetValue(books.ToArray()[1],out var bookAuthor );
            Assert.AreEqual("Nigel’ Rees", bookAuthor);           
            
            Assert.IsTrue(dic.TryGetValue("store.book[3].onshell", out var onshell));
            Assert.AreEqual("true", onshell);

            var bikes = dic.Keys.Where(k => k.Contains("bicycle"));
            Assert.AreEqual(8, bikes.Count());

            Assert.AreEqual("store.bicycle.nullable", bikes.ToArray()[2]);
            Assert.IsTrue(dic.TryGetValue("store.bicycle.nullable", out  onshell));
            Assert.AreEqual("null", onshell);
        }
    }
}