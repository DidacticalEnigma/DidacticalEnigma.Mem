using System.Linq;
using System.Text;

namespace DidacticalEnigma.Mem.Extensions
{
    public static class DapperExtensions
    {
        public static string SqlForFunction<T>(string procedureName, T parameters)
            where T : notnull
        {
            var paramNames = parameters.GetType().GetProperties().Select(prop => prop.Name);
            var sb = new StringBuilder();
            sb.Append("SELECT *");
            sb.Append(" FROM \"");
            sb.Append(procedureName);
            sb.Append("\" (");
            bool first = true;
            foreach (var paramName in paramNames)
            {
                if (!first)
                {
                    sb.Append(", ");                    
                }

                first = false;
                sb.Append('"');
                sb.Append(paramName);
                sb.Append('"');
                sb.Append(" := @");
                sb.Append(paramName);
            }
            sb.Append(");");

            return sb.ToString();
        }
        
        public static string SqlForProcedure<T>(string procedureName, T parameters)
            where T : notnull
        {
            var paramNames = parameters.GetType().GetProperties().Select(prop => prop.Name);
            var sb = new StringBuilder();
            sb.Append("CALL \"");
            sb.Append(procedureName);
            sb.Append("\" (");
            bool first = true;
            foreach (var paramName in paramNames)
            {
                if (!first)
                {
                    sb.Append(", ");                    
                }

                first = false;
                sb.Append('"');
                sb.Append(paramName);
                sb.Append('"');
                sb.Append(" := @");
                sb.Append(paramName);
            }
            sb.Append(");");

            return sb.ToString();
        }
    }
}