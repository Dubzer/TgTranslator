namespace TgTranslator.Models;

public partial class User
{
    public long UserId { get; set; }
    public string Track { get; set; }
    public bool PmAllowed { get; set; }
}