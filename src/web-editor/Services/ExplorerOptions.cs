namespace CollabEdit.Services
{
    public class ExplorerOptions
    {
        public string EditorRoot { get; set; } = "./wwwwroot/editor-root";
        public string VirtualRoot { get; set; } = "home";
        public bool CreateIfNotExists { get; set; } = false;
    }
}