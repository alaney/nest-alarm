
using Google.Cloud.PubSub.V1;

namespace nestalarm
{
  public class DeviceAccess
  {
    private string projectId;
    private string subscriptionId;
    private HttpClient client;

    public DeviceAccess(DeviceAccessOptions options)
    {
      this.projectId = options.ProjectId;
      this.subscriptionId = options.SubscriptionId;
      this.client = new HttpClient();
    }

    public async Task<bool> CheckForPersonEventAsync(bool acknowledge)
    {
      SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
      SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);
      // SubscriberClient runs your message handle function on multiple
      // threads to maximize throughput.
      int personEventCount = 0;
      Task startTask = subscriber.StartAsync((PubsubMessage message, CancellationToken cancel) =>
      {
        string text = System.Text.Encoding.UTF8.GetString(message.Data.ToArray());
        if (text.Contains("sdm.devices.events.CameraPerson.Person"))
        {
          Interlocked.Increment(ref personEventCount);
        }
        return Task.FromResult(acknowledge ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack);
      });
      // Run for 5 seconds.
      await Task.Delay(5000);
      await subscriber.StopAsync(CancellationToken.None);
      // Lets make sure that the start task finished successfully after the call to stop.
      await startTask;
      return personEventCount > 0;
    }
  }
}