namespace OnlineVoting.Api.Documentation.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ApiDocumentationAttribute : Attribute
    {
        public ApiDocumentationAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}