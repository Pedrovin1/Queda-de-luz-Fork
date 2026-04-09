public class Message
{
    public int? Id;
    public string Text; 
    public string? ImageLink;
    public long? UTC_TimeSent;
    public int AccountId;
    public bool IsHidden = false;  

    //To recover from Database
    public Message(long Message_id, string Message_text, string Message_image_link, long UTC_datetime_sent, bool Is_hidden, long Base_Account_id)
    {
        this.Id = (int)Message_id;
        this.Text = Message_text; 
        this.ImageLink =  Message_image_link;
        this.UTC_TimeSent = UTC_datetime_sent;
        this.AccountId = (int)Base_Account_id;
        this.IsHidden = Is_hidden;  
    }

    //To insert into the Database
    public Message(string Message_text, long Base_Account_id)
    {
        this.Text = Message_text; 
        this.AccountId = (int)Base_Account_id;
    }
}