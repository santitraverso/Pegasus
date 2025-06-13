using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Reflection;

namespace PegasusV1.Security
{
    public static class SecureQueryParser
    {
        // Lista blanca de propiedades permitidas (case-insensitive)
        private static readonly HashSet<string> AllowedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // IDs principales
            "Id", "ID", "id",
            
            "ID_ALUMNO", "id_alumno", "Id_Alumno",
            "ID_MATERIA", "id_materia", "Id_Materia", "id_Materia",
            "ID_CURSO", "id_curso", "Id_Curso",
            "ID_USUARIO", "id_usuario", "Id_Usuario",
            "ID_DOCENTE", "id_docente", "Id_Docente",
            "ID_EVENTO", "id_evento", "Id_Evento",
            "ID_COMUNICADO", "id_comunicado", "Id_Comunicado",
            "ID_PADRE", "id_padre", "Id_Padre",
            "ID_PERFIL", "id_perfil", "Id_Perfil",
            "TIPO_CONTACTO", "tipo_contacto", "Tipo_Contacto",
            "ACTIVO", "activo", "Activo",
            
            "FECHA", "fecha", "Fecha",
            "PRESENTE", "presente", "Presente",
            "CALIFICACION", "calificacion", "Calificacion",
            "NOMBRE", "nombre", "Nombre",
            "APELLIDO", "apellido", "Apellido",
            "MAIL", "mail", "Mail",
            "TELEFONO", "telefono", "Telefono",
            "TITULO", "titulo", "Titulo",
            "DESCRIPCION", "descripcion", "Descripcion",
            "NOMBRE_CURSO", "nombre_curso", "Nombre_Curso",
            "GRADO", "grado", "Grado",
            "DIVISION", "division", "Division",
            "TURNO", "turno", "Turno",
            "LEIDO", "leido", "Leido",
            "CONFIRMADO", "confirmado", "Confirmado",
            "ID_MODULO", "id_modulo", "Id_Modulo",
            "MODULO", "modulo", "Modulo",
            "PARAMETRO", "parametro", "Parametro",
            "Page", "page",
            
            // Propiedades especiales
            "HasValue", "Value", "Date"
        };

        public static Expression<Func<T, bool>>? ParseSafeQuery<T>(string? query) where T : class
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            try
            {
                // Validación básica de seguridad
                if (!IsQuerySafe(query))
                {
                    throw new SecurityException($"Consulta no permitida por razones de seguridad");
                }

                // Parsear la consulta
                return ParseQuery<T>(query);
            }
            catch (Exception ex)
            {
                throw new SecurityException($"Error al parsear consulta: {ex.Message}");
            }
        }

        private static bool IsQuerySafe(string query)
        {
            // Verificar palabras clave peligrosas
            var dangerousKeywords = new[]
            {
                "System.", "File.", "Directory.", "Process.", "Assembly.",
                "Delete", "Drop", "Truncate", "Exec", "Execute", "Script",
                "Eval", "Function", "Method", "Reflection."
            };

            foreach (var keyword in dangerousKeywords)
            {
                if (query.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private static Expression<Func<T, bool>> ParseQuery<T>(string query) where T : class
        {
            var parameter = Expression.Parameter(typeof(T), "x");

            // Limpiar la query (remover lambda si existe)
            var cleanQuery = CleanQuery(query);

            // Parsear la expresión
            var body = ParseExpression(cleanQuery, parameter);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private static string CleanQuery(string query)
        {
            // Remover "x=>" del inicio
            var match = Regex.Match(query, @"^[a-zA-Z_][a-zA-Z0-9_]*\s*=>\s*(.+)$");
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            return query.Trim();
        }

        private static Expression ParseExpression(string expression, ParameterExpression parameter)
        {
            // Manejar OR (||) - tiene menor precedencia
            if (expression.Contains("||"))
            {
                var orParts = SplitByOperator(expression, "||");
                if (orParts.Count > 1)
                {
                    var expressions = orParts.Select(part => ParseExpression(part.Trim(), parameter));
                    return expressions.Aggregate(Expression.OrElse);
                }
            }

            // Manejar AND (&&) - tiene mayor precedencia
            if (expression.Contains("&&"))
            {
                var andParts = SplitByOperator(expression, "&&");
                if (andParts.Count > 1)
                {
                    var expressions = andParts.Select(part => ParseExpression(part.Trim(), parameter));
                    return expressions.Aggregate(Expression.AndAlso);
                }
            }

            // Parsear comparación simple
            return ParseComparison(expression, parameter);
        }

        private static List<string> SplitByOperator(string expression, string operatorStr)
        {
            var parts = new List<string>();
            var currentPart = "";
            var i = 0;

            while (i < expression.Length)
            {
                if (i <= expression.Length - operatorStr.Length &&
                    expression.Substring(i, operatorStr.Length) == operatorStr)
                {
                    parts.Add(currentPart.Trim());
                    currentPart = "";
                    i += operatorStr.Length;
                }
                else
                {
                    currentPart += expression[i];
                    i++;
                }
            }

            if (!string.IsNullOrEmpty(currentPart.Trim()))
            {
                parts.Add(currentPart.Trim());
            }

            return parts.Count > 1 ? parts : new List<string> { expression };
        }

        private static Expression ParseComparison(string expression, ParameterExpression parameter)
        {
            // Patrones para tus casos reales (sin variables)
            var patterns = new[]
            {
                // Números: x.id_materia==1, x.id_perfil == 2
                @"^([a-zA-Z_][a-zA-Z0-9_]*(?:\.[a-zA-Z_][a-zA-Z0-9_]*)*)\s*(==|!=|>|<|>=|<=)\s*([0-9]+)$",
                // Booleanos: x.activo == true, x.presente==false
                @"^([a-zA-Z_][a-zA-Z0-9_]*(?:\.[a-zA-Z_][a-zA-Z0-9_]*)*)\s*(==|!=)\s*(true|false)$",
                // Decimales: x.promedio>=7.5
                @"^([a-zA-Z_][a-zA-Z0-9_]*(?:\.[a-zA-Z_][a-zA-Z0-9_]*)*)\s*(==|!=|>|<|>=|<=)\s*([0-9]+\.[0-9]+)$"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(expression, pattern);
                if (match.Success)
                {
                    var propertyPath = match.Groups[1].Value;
                    var operatorStr = match.Groups[2].Value;
                    var valueStr = match.Groups[3].Value;

                    // Crear expresión de propiedad
                    var propertyExpression = CreatePropertyExpression(propertyPath, parameter);

                    // Crear expresión de valor
                    var valueExpression = CreateValueExpression(valueStr, propertyExpression.Type);

                    // Crear comparación
                    return CreateComparisonExpression(propertyExpression, operatorStr, valueExpression);
                }
            }

            throw new SecurityException($"Expresión no válida: {expression}");
        }

        private static Expression CreatePropertyExpression(string propertyPath, ParameterExpression parameter)
        {
            // Limpiar el path (remover "x." si existe)
            var cleanPath = propertyPath;
            if (propertyPath.StartsWith("x.", StringComparison.OrdinalIgnoreCase))
            {
                cleanPath = propertyPath.Substring(2);
            }

            // Validar que la propiedad esté permitida
            if (!IsPropertyAllowed(cleanPath))
            {
                throw new SecurityException($"Propiedad no permitida: {cleanPath}");
            }

            // Crear expresión de propiedad
            Expression expression = parameter;
            var parts = cleanPath.Split('.');

            foreach (var part in parts)
            {
                var property = expression.Type.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    throw new SecurityException($"Propiedad no encontrada: {part}");
                }
                expression = Expression.Property(expression, property);
            }

            return expression;
        }

        private static Expression CreateValueExpression(string valueStr, Type targetType)
        {
            // Manejar tipos nullable
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            var actualType = underlyingType ?? targetType;

            object value;

            try
            {
                // Convertir según el tipo
                if (actualType == typeof(int))
                {
                    value = int.Parse(valueStr);
                }
                else if (actualType == typeof(bool))
                {
                    value = bool.Parse(valueStr);
                }
                else if (actualType == typeof(decimal))
                {
                    value = decimal.Parse(valueStr);
                }
                else if (actualType == typeof(double))
                {
                    value = double.Parse(valueStr);
                }
                else if (actualType == typeof(float))
                {
                    value = float.Parse(valueStr);
                }
                else if (actualType == typeof(byte))
                {
                    value = byte.Parse(valueStr);
                }
                else if (actualType == typeof(short))
                {
                    value = short.Parse(valueStr);
                }
                else if (actualType == typeof(long))
                {
                    value = long.Parse(valueStr);
                }
                else if (actualType == typeof(DateTime))
                {
                    value = DateTime.Parse(valueStr);
                }
                else
                {
                    value = valueStr; // String por defecto
                }

                // Crear constante con el tipo correcto
                return Expression.Constant(value, targetType);
            }
            catch (Exception ex)
            {
                throw new SecurityException($"Error al convertir valor '{valueStr}' a tipo {targetType.Name}: {ex.Message}");
            }
        }

        private static Expression CreateComparisonExpression(Expression left, string operatorStr, Expression right)
        {
            return operatorStr switch
            {
                "==" => Expression.Equal(left, right),
                "!=" => Expression.NotEqual(left, right),
                ">" => Expression.GreaterThan(left, right),
                "<" => Expression.LessThan(left, right),
                ">=" => Expression.GreaterThanOrEqual(left, right),
                "<=" => Expression.LessThanOrEqual(left, right),
                _ => throw new SecurityException($"Operador no permitido: {operatorStr}")
            };
        }

        private static bool IsPropertyAllowed(string property)
        {
            return AllowedProperties.Contains(property) ||
                   property.StartsWith("ID_", StringComparison.OrdinalIgnoreCase) ||
                   property.StartsWith("id_", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
        public SecurityException(string message, Exception innerException) : base(message, innerException) { }
    }
}
