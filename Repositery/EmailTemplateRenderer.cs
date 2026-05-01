namespace Project_385.Repositery
{
    public class EmailTemplateRenderer : IEmailTemplateRenderer
    {
        private readonly IWebHostEnvironment _env;

        public EmailTemplateRenderer(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> RenderAsync(string templateName, Dictionary<string, string> variables)
        {
            var path = Path.Combine(_env.ContentRootPath, "Templates", "Email", $"{templateName}.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Không tìm thấy template: {path}");

            var content = await File.ReadAllTextAsync(path);

            foreach (var variable in variables)
                content = content.Replace($"{{{{{variable.Key}}}}}", variable.Value);

            return content;

        }
    }
}