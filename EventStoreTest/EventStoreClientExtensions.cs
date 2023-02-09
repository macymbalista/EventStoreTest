using System.Runtime.CompilerServices;
using EventStore.Client;

namespace EventStoreTest;

public static class EventStoreClientExtensions
{
    public static async Task<ConditionalWriteResult> AppendToStream(
            this EventStoreClient eventStoreClient,
            string streamName,
            IEnumerable<EventData> eventData,
            StreamRevision? expectedVersion
             )
    {
        if( expectedVersion.HasValue )
        {
           var result =  await eventStoreClient.ConditionalAppendToStreamAsync(
                streamName,
                expectedVersion.Value,
                eventData
            );

            if (result.Status != ConditionalWriteStatus.Succeeded)
            {
                throw new ArgumentException("Sorry something went wrong ");
            }

            return result;
        }

        var result2 =  await eventStoreClient.ConditionalAppendToStreamAsync(
            streamName,
            StreamState.NoStream,
            eventData).ConfigureAwait(false);
		
        return result2;
			
    }
}