using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace EFCoreDatabaseFunctions
{
    public static class Json
    {
        public static string Value(
            string expression,
            string path)
            => throw new InvalidOperationException($"{nameof(Value)}cannot be called client side");
    }

    public static class AnswersToTheUniverse
    {
        public static int What() 
            => throw new InvalidOperationException($"{nameof(What)}cannot be called client side");
    }
    
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder UseCustomDbFunctions(this ModelBuilder builder)
        {
            var jsonvalueMethodInfo = typeof(Json)
                .GetRuntimeMethod(
                    nameof(Json.Value),
                    new[] {typeof(string), typeof(string)}
                );
            builder
            .HasDbFunction(jsonvalueMethodInfo)
            .HasTranslation(args => 
                SqlFunctionExpression.Create("JSON_VALUE",
                    args,
                    typeof(string),
                null /* guess */
                )
            );
            
            var methodInfo =
                typeof(AnswersToTheUniverse)
                    .GetRuntimeMethod(nameof(AnswersToTheUniverse.What), new Type[0]);
            
            builder.HasDbFunction(methodInfo)
                .HasTranslation(args =>
                    SqlFunctionExpression.Create(
                        builder.Model.GetDefaultSchema(), 
                        "FortyTwo",
                        args,
                        typeof(int),
                        null /* guess */
                    ));
            
            return builder;
        }    
    }
}