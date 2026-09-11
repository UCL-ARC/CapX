using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PPMTool.Data.Context;

namespace PPMTool.Services
{
    /// <summary>
    /// A service that generates a prompt describing the database schema for use with AI agents.
    /// </summary>
    public sealed class DatabaseSchemaPromptService
    {
        private readonly IDbContextFactory<PPMToolContext> contextFactory;

        /// <summary>
        /// Cached prompt string to avoid rebuilding the prompt on every request.
        /// </summary>
        private string cachedPrompt;
        private readonly SemaphoreSlim lockFlag = new(1, 1);

        public DatabaseSchemaPromptService(
            IDbContextFactory<PPMToolContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        /// <summary>
        /// Gets the prompt describing the database schema.
        /// If the prompt has already been generated, it returns the cached version.
        /// Otherwise, it builds the prompt from the EF Core model.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> GetPromptAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(cachedPrompt))
            {
                return cachedPrompt;
            }

            // Join the queue to build the prompt
            await lockFlag.WaitAsync(cancellationToken);

            try
            {
                // Check again if the prompt has been built while waiting for the lock
                if (!string.IsNullOrWhiteSpace(cachedPrompt))
                {
                    return cachedPrompt;
                }

                using var context = contextFactory.CreateDbContext();

                cachedPrompt = BuildPrompt(context.Model);

                return cachedPrompt;
            }
            finally
            {
                lockFlag.Release();
            }
        }

        /// <summary>
        /// Builds a prompt string that describes the database schema based on the provided EF Core model.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static string BuildPrompt(IModel model)
        {
            var sb = new StringBuilder();

            sb.AppendLine("""
                You are the CapX Data Agent.

                The application uses Entity Framework Core.

                Only use tables, columns and relationships that are documented below.

                DATABASE SCHEMA
                ================
                """
            );

            // List all tables, columns, and relationships in the database schema
            foreach (var entityType in model.GetEntityTypes()
                         .Where(e => !e.IsOwned())
                         .OrderBy(e => e.GetTableName()))
            {
                var tableName = entityType.GetTableName();

                if (string.IsNullOrWhiteSpace(tableName))
                {
                    continue;
                }

                // Skip the EF Core migrations history table, as it is not relevant to the schema description.
                if (tableName == "__EFMigrationsHistory")
                {
                    continue;
                }

                // Add a blank line before each table for better readability
                sb.AppendLine();

                // Add the table name to the prompt
                sb.AppendLine($"TABLE: {tableName}");

                // Add the table's schema if it exists
                var primaryKey = entityType.FindPrimaryKey();

                foreach (var property in entityType.GetProperties())
                {
                    var line = new StringBuilder();

                    line.Append("  ");
                    line.Append(property.Name);
                    line.Append(" : ");
                    line.Append(GetTypeDescription(property));

                    if (primaryKey?.Properties.Contains(property) == true)
                    {
                        line.Append(" [PRIMARY KEY]");
                    }

                    if (!property.IsNullable)
                    {
                        line.Append(" [REQUIRED]");
                    }

                    sb.AppendLine(line.ToString());
                }

                var foreignKeys = entityType.GetForeignKeys().ToList();

                if (foreignKeys.Count != 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("  RELATIONSHIPS:");

                    foreach (var fk in foreignKeys)
                    {
                        sb.AppendLine(
                            $"    {string.Join(", ", fk.Properties.Select(p => p.Name))}" +
                            $" -> {fk.PrincipalEntityType.GetTableName()}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine();

            // List all enumerations used in the database schema
            sb.AppendLine("ENUMERATIONS");
            sb.AppendLine("============");

            // Get all distinct enum types used in the entity properties
            var enums = model.GetEntityTypes()
                .SelectMany(e => e.GetProperties())
                .Select(p => Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType)
                .Where(t => t.IsEnum)
                .Distinct()
                .OrderBy(t => t.Name);

            foreach (var enumType in enums)
            {
                sb.AppendLine();
                sb.AppendLine(enumType.Name);

                // List all values of the enum type with their corresponding integer values
                foreach (var value in Enum.GetValues(enumType))
                {
                    sb.AppendLine($"  {(int)value} = {value}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a string description of the property type for use in the prompt.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static string GetTypeDescription(IProperty property)
        {
            var clrType =
                Nullable.GetUnderlyingType(property.ClrType)
                ?? property.ClrType;

            if (clrType.IsEnum)
            {
                return $"ENUM({clrType.Name})";
            }

            if (clrType == typeof(string))
            {
                return "STRING";
            }

            if (clrType == typeof(int))
            {
                return "INT";
            }

            if (clrType == typeof(long))
            {
                return "LONG";
            }

            if (clrType == typeof(decimal))
            {
                return "DECIMAL";
            }

            if (clrType == typeof(double))
            {
                return "DOUBLE";
            }

            if (clrType == typeof(float))
            {
                return "FLOAT";
            }

            if (clrType == typeof(bool))
            {
                return "BOOLEAN";
            }

            if (clrType == typeof(DateTime))
            {
                return "DATETIME";
            }

            if (clrType == typeof(Guid))
            {
                return "GUID";
            }

            return clrType.Name.ToUpperInvariant();
        }
    }
}
