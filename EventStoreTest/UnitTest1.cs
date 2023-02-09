using System.Diagnostics;
using System.Text;
using EventStore.Client;
using Newtonsoft.Json;

namespace EventStoreTest;

public class Tests
{
    private EventStoreClient _client;
    
    [SetUp]
    public void Setup()
    {
        var clientSettings = EventStoreClientSettings.Create( $"esdb://localhost:2113?Tls=false" );
        _client = new EventStoreClient( clientSettings );
        
    }

    [Test]
    public async Task ThrowArgumentExceptionWhenExpectedVersionIsWrong()
    {
       var id = $"myaggregatemock-{Guid.NewGuid()}";

       var t1 = new
       {
           aggregateIdentity = id,
           journalVersion = 1,
           aggregateEvent = new {},
           metadata= new {
               type_name = "MyMockCreated",
               type_version = "0",
               event_id = Guid.NewGuid() 
           }
       };
       
       var t2 = new
       {
           aggregateIdentity = id,
           journalVersion = 2,
           aggregateEvent = new { MyText = "Hello" },
           metadata= new {
               type_name = "MyMockTextChanged",
               type_version = "0",
               event_id = Guid.NewGuid() 
           }
       };
       

       var t1Ser = JsonConvert.SerializeObject(t1);
       var t2Ser = JsonConvert.SerializeObject(t2);
       
       var eventData1 = new EventData(
           Uuid.NewUuid(),
           "AggregateTransaction",
           Encoding.UTF8.GetBytes(t1Ser),
           contentType: "application/octet-stream"
       );
       
       var eventData2 = new EventData(
           Uuid.NewUuid(),
           "AggregateTransaction",
           Encoding.UTF8.GetBytes(t2Ser),
           contentType: "application/octet-stream"
       );
       
       
        var result = await _client.AppendToStream(
            id,
            new []
            {
                eventData1,
                eventData2
            },
            null
            );
        
        Assert.AreEqual(result.Status, ConditionalWriteStatus.Succeeded);
        
        Assert.ThrowsAsync<ArgumentException>( async () => await _client.AppendToStream(
                id,
                new EventData[]
                {
                    eventData2
                },
                StreamRevision.FromStreamPosition(2)
            ));
    }
}