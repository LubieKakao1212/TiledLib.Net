﻿using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TiledLib.Tests
{
    [TestClass]
    public class WriteTests
    {
        [DataTestMethod]
        [DataRow("Data/tileset_map_base64.tmx")]
        [DataRow("Data/External_tileset_map.tmx")]
        [DataRow("Data/External_tileset_map_base64.tmx")]
        public void TestWriting(string file)
        {
            using (var original = new MemoryStream())
            {
                using (var mapStream = File.OpenRead(file))
                {
                    mapStream.CopyTo(original);
                    original.Seek(0, SeekOrigin.Begin);
                }

                var map = Map.FromStream(original);
                original.Seek(0, SeekOrigin.Begin);
                using (var output = new MemoryStream())
                {
                    using (var writer = new StreamWriter(output, Encoding.UTF8, 1024, true))
                    {
                        new XmlSerializer(typeof(Map)).Serialize(writer, map);
                    }

                    var expected = Encoding.UTF8.GetString(original.ToArray());
                    var result = Encoding.UTF8.GetString(output.ToArray()).Substring(1); //Skip BOM

                    while (expected.Length > 0 && result.Length > 0 && char.ToLowerInvariant(expected[0]) == char.ToLowerInvariant(result[0]))
                    {
                        expected = expected.Substring(1).Trim();
                        //TODO: Implement support for property types.
                        foreach (var item in new[] { "type=\"bool\"", "type=\"int\"" })
                            while (expected.StartsWith(item))
                                expected = expected.Substring(item.Length).Trim();

                        result = result.Substring(1).Trim();
                    }
                    Assert.AreEqual(expected, result, ignoreCase: true);
                }
            }
        }
    }
}
