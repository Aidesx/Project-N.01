namespace Project_385.Repositery
{
    public interface IEmailTemplateRenderer
    {
        Task<string> RenderAsync(string templateName, Dictionary<string, string> variables);
    }
}
