﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Ncqrs.Eventing.Storage.NoDB.Tests
{
    public class when_saving_a_new_event_source : NoDBEventStoreTestFixture
    {
        private string _filename;
        private string _foldername;

        [SetUp]
        public void SetUp()
        {
            _foldername = Source.EventSourceId.ToString().Substring(0, 2);
            _filename = Source.EventSourceId.ToString().Substring(2);
            EventStore.Save(Source);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_foldername, true);
        }

        [Test]
        public void it_should_create_a_new_event_history_file()
        {
            Assert.That(File.Exists(Path.Combine(_foldername, _filename)));
        }

        [Test]
        public void it_should_serialize_the_uncommitted_events_to_the_file()
        {
            using (var reader = new StreamReader(File.Open(Path.Combine(_foldername, _filename), FileMode.Open)))
            {
                foreach (string line in GetEventStrings(reader))
                {
                    Console.WriteLine(line);
                    StoredEvent<JObject> storedevent = line.ReadStoredEvent();
                    Assert.That(storedevent, Is.Not.Null);
                    Assert.That(Events.Count(e => e.EventIdentifier == storedevent.EventIdentifier) == 1);
                }
            }
        }

        private static IEnumerable<string> GetEventStrings(TextReader reader)
        {
            string line = reader.ReadLine();
            while (line != null)
            {
                yield return line;
                line = reader.ReadLine();
            }
        }
    }
}