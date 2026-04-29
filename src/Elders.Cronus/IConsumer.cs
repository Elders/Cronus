using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus;

/// <summary>
/// Represents a consumer that processes messages of the specified handler type.
/// </summary>
/// <typeparam name="T">The type of message handler the consumer dispatches to.</typeparam>
public interface IConsumer<out T> where T : IMessageHandler
{
    /// <summary>
    /// Starts the consumer so it can begin dispatching messages.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the consumer and releases any resources associated with the running consumer instance.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task StopAsync(CancellationToken cancellationToken = default);
}
