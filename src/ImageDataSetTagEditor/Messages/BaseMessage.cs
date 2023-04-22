namespace ImageDataSetTagEditor.Messages;

public abstract class BaseMessage
{
    protected BaseMessage(object sender)
    {
        Sender = sender;
    }

    public object Sender { get; }
}