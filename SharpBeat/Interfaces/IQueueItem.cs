
public interface IQueueItem<T>
{
    void AddToQueue(Queue<T> queue);
}