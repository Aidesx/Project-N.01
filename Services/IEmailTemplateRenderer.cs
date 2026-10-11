namespace Project_385.Services
{
    public interface IEmailTemplateRenderer
    {
        Task<string> RenderAsync(string templateName, Dictionary<string, string> variables);
    }
}
