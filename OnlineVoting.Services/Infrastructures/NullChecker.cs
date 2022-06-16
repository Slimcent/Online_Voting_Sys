namespace OnlineVoting.Services.Infrastructures
{
    public static class NullChecker
    {
        public static Dictionary<string, string> CheckNullValues(this object target, params string[] excludeProps)
        {
            var errors = new Dictionary<string, string>();
            var properties = target.GetType().GetProperties()
                .Where(pi => pi.PropertyType == typeof(string) || pi.PropertyType == typeof(int)).ToList();


            foreach (var property in properties)
            {
                if (excludeProps == null || !excludeProps.Contains(property.Name))
                {
                    var parent = property.DeclaringType?.Name;


                    if (parent.EndsWith("Dto") || parent.EndsWith("dto"))
                    {
                        parent = parent.Substring(0, parent.Length - 3);
                    }

                    var key = $"{parent}.{property.Name}";
                    var value = $"The {property.Name} is required";

                    var propertyValue = property.GetValue(target);

                    if (propertyValue is int && (int)propertyValue == 0)
                    {
                        errors.AddError(key, value);
                    }

                    if (propertyValue is string && string.IsNullOrWhiteSpace((string)propertyValue))
                    {
                        errors.AddError(key, value);
                    }

                    if (propertyValue is null)
                    {
                        errors.AddError(key, value);
                    }
                }

            }

            return errors;
        }

        private static void AddError(this Dictionary<string, string> errors, string key, string value)
        {
            if (!errors.ContainsKey(key))
            {
                errors[key] = value;
            }
        }
    }
}
