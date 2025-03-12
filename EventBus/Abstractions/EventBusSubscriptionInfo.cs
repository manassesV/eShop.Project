using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace eShop.EventBus.Abstractions
{
    public class EventBusSubscriptionInfo
    {
        public Dictionary<string, Type> EventTypes { get; } = [];

        public JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerOptions.Default);

        internal static readonly JsonSerializerOptions DefaultSerializeOptions = new()
        {
            TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault ? CreateDefaultTypeResolver()
             : JsonTypeInfoResolver.Combine()
        };

        private static IJsonTypeInfoResolver CreateDefaultTypeResolver()
            => new DefaultJsonTypeInfoResolver();
          

    }
}
