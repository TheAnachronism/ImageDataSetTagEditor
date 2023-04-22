namespace ImageDataSetTagEditor.Messages;

public class FocusTextBoxMessage : BaseMessage
{
    public FocusTextBoxMessage(TextBoxType type, object sender) : base(sender)
    {
        Type = type;
    }

    public enum TextBoxType
    {
        ImageSearch,
        TagSearch
    }

    public TextBoxType Type { get; }
}