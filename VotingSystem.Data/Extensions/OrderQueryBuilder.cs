using System.Reflection;
using System.Text;

namespace VotingSystem.Data.Extensions
{
    public static class OrderQueryBuilder
    {
        public static string CreateOrderQuery<T>(string orderByQueryString)
        {
            string[] orderParams = orderByQueryString.Trim().Split(',');
            PropertyInfo[] propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            StringBuilder orderQueryBuilder = new();

            foreach (string param in orderParams)
            {
                if (string.IsNullOrWhiteSpace(param))
                    continue;

                string trimmedParam = param.Trim();
                string propertyFromQueryName = trimmedParam.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

                PropertyInfo? objectProperty = propertyInfos.FirstOrDefault(property =>
                    property.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

                if (objectProperty == null)
                    continue;

                string direction = trimmedParam.EndsWith(" desc", StringComparison.InvariantCultureIgnoreCase)
                    ? "descending"
                    : "ascending";

                orderQueryBuilder.Append($"{objectProperty.Name} {direction}, ");
            }

            string orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');

            return orderQuery;
        }
    }
}