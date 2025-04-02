using System.Collections.Generic;

namespace Application.Services;

public class SendGridSettings
{
    public string ApiKey { get; set; }
    public IList<Sender> Senders { get; set; } = new List<Sender>();
}

public class Sender
{
    public string Name { get; set; }
    public string Email { get; set; }
}