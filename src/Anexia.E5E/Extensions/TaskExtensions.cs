namespace Anexia.E5E.Extensions;

internal static class TaskExtensions
{
	/// <summary>
	/// Waits for the <see cref="Task"/> to complete. If the provided <see cref="CancellationToken"/> is cancelled
	/// before the successful execution of the task, an <see cref="OperationCanceledException"/> is thrown.
	///
	/// This allows callers to "cancel" operations, even if they do not have cancellation support.
	///
	/// Licensed under a CC-BY-SA 4.0 by i3arnon: https://stackoverflow.com/a/26942757
	/// </summary>
	/// <remarks>The actual task still runs in the background. Be aware of memory leaks.</remarks>
	/// <param name="task">The task that does not support cancellation.</param>
	/// <param name="cancellationToken">The token to cancel the operation.</param>
	/// <typeparam name="T">The result type</typeparam>
	/// <returns>The same as the input task.</returns>
	/// <exception cref="OperationCanceledException">Thrown if the cancellation was requested ahead of the completion.</exception>
	public static Task<T> WithWaitCancellation<T>(
		this Task<T> task, CancellationToken cancellationToken) =>
		task.IsCompleted
			? task
			: task.ContinueWith(
				completedTask => completedTask.GetAwaiter().GetResult(),
				cancellationToken,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);
}
