using Microsoft.Data.SqlClient;

namespace WebAPI.Utils;

public static class SqlExceptionHandler
{
    public static (int statusCode, string message) HandleSqlException(SqlException sqlException)
    {
        return sqlException.Number switch
        {
            // Primary key violation
            2627 => (409, ExtractDuplicateKeyMessage(sqlException.Message, "primary key")),
            
            // Unique constraint violation
            2601 => (409, ExtractDuplicateKeyMessage(sqlException.Message, "unique constraint")),
            
            // Foreign key constraint violation
            547 when sqlException.Message.Contains("DELETE") => (400, ExtractForeignKeyDeleteMessage(sqlException.Message)),
            547 when sqlException.Message.Contains("CHECK") => (400, ExtractCheckConstraintMessage(sqlException.Message)),
            547 => (400, ExtractForeignKeyMessage(sqlException.Message)),
            
            // Timeout
            -2 => (408, "The database operation timed out. Please try again."),
            
            // Cannot connect to server
            2 => (503, "Database service is currently unavailable."),
            
            // Default case for other SQL errors
            _ => (500, "A database error occurred. Please try again later.")
        };
    }

    private static string ExtractDuplicateKeyMessage(string sqlMessage, string constraintType)
    {
        try
        {
            var tableName = ExtractTableName(sqlMessage);
            var constraintName = ExtractConstraintName(sqlMessage);
            
            if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(constraintName))
            {
                return $"Duplicate entry found in table '{tableName}'. The {constraintType} constraint '{constraintName}' prevents duplicate values.";
            }
            else if (!string.IsNullOrEmpty(tableName))
            {
                return $"Duplicate entry found in table '{tableName}'. This field must be unique.";
            }
            
            return $"This record already exists and cannot be duplicated due to {constraintType} violation.";
        }
        catch
        {
            return $"This record already exists and cannot be duplicated due to {constraintType} violation.";
        }
    }

    private static string ExtractForeignKeyMessage(string sqlMessage)
    {
        try
        {
            var tableName = ExtractTableName(sqlMessage);
            var constraintName = ExtractConstraintName(sqlMessage);
            
            if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(constraintName))
            {
                return $"Foreign key constraint '{constraintName}' failed on table '{tableName}'. Please ensure the referenced record exists.";
            }
            else if (!string.IsNullOrEmpty(tableName))
            {
                return $"Foreign key constraint failed on table '{tableName}'. Please ensure all referenced data exists.";
            }
            
            return "The operation failed due to a reference constraint. Please ensure all referenced data exists.";
        }
        catch
        {
            return "The operation failed due to a reference constraint. Please ensure all referenced data exists.";
        }
    }

    private static string ExtractForeignKeyDeleteMessage(string sqlMessage)
    {
        try
        {
            var tableName = ExtractTableName(sqlMessage);
            var constraintName = ExtractConstraintName(sqlMessage);
            
            if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(constraintName))
            {
                return $"Cannot delete record from table '{tableName}' due to foreign key constraint '{constraintName}'. This record is referenced by other data.";
            }
            else if (!string.IsNullOrEmpty(tableName))
            {
                return $"Cannot delete record from table '{tableName}'. This record is referenced by other data.";
            }
            
            return "This record cannot be deleted because it is referenced by other data.";
        }
        catch
        {
            return "This record cannot be deleted because it is referenced by other data.";
        }
    }

    private static string ExtractCheckConstraintMessage(string sqlMessage)
    {
        try
        {
            var tableName = ExtractTableName(sqlMessage);
            var constraintName = ExtractConstraintName(sqlMessage);
            
            if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(constraintName))
            {
                return $"Check constraint '{constraintName}' failed on table '{tableName}'. The data provided does not meet the required validation rules.";
            }
            else if (!string.IsNullOrEmpty(tableName))
            {
                return $"Check constraint failed on table '{tableName}'. The data provided does not meet the required validation rules.";
            }
            
            return "The data provided does not meet the required validation rules.";
        }
        catch
        {
            return "The data provided does not meet the required validation rules.";
        }
    }

    private static string ExtractTableName(string sqlMessage)
    {
        try
        {
            // Look for patterns like "object 'dbo.TableName'" or "table 'TableName'"
            var patterns = new[]
            {
                @"object\s+'(?:dbo\.)?([^']+)'",
                @"table\s+'([^']+)'",
                @"'([^']+)'\s+table",
                @"in\s+object\s+'(?:dbo\.)?([^']+)'"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(sqlMessage, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractConstraintName(string sqlMessage)
    {
        try
        {
            // Look for patterns like "constraint 'ConstraintName'" or "'ConstraintName' constraint"
            var patterns = new[]
            {
                @"constraint\s+'([^']+)'",
                @"'([^']+)'\s+constraint",
                @"index\s+'([^']+)'",
                @"'([^']+)'\s+index"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(sqlMessage, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
