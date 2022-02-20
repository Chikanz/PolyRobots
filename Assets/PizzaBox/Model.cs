// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using PolyPizza;
//
//    var model = Model.FromJson(jsonString);

namespace PolyPizza
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SearchResults
    {
        [JsonProperty("results")]
        public Model[] Results { get; set; }
        
        [JsonProperty("total")]
        public long Total { get; set; }
    }
    
    public partial class Model
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("Attribution")]
        public string Attribution { get; set; }

        [JsonProperty("Thumbnail")]
        public Uri Thumbnail { get; set; }

        [JsonProperty("Download")]
        public Uri Download { get; set; }

        [JsonProperty("Tri Count")]
        public long TriCount { get; set; }

        [JsonProperty("Creator")]
        public Creator Creator { get; set; }

        [JsonProperty("Category")]
        public string Category { get; set; }

        [JsonProperty("Tags")]
        public string[] Tags { get; set; }

        [JsonProperty("Licence")]
        public string Licence { get; set; }

        [JsonProperty("Animated")]
        public bool Animated { get; set; }

        [JsonProperty("Orbit")]
        public Orbit Orbit { get; set; }
    }
    
    public partial class Orbit
    {
        [JsonProperty("phi")]
        public float phi { get; set; }

        [JsonProperty("theta")]
        public float theta { get; set; }
    }

    public partial class Creator
    {
        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("DPURL")]
        public Uri Dpurl { get; set; }
    }

    public partial class Model
    {
        public static Model FromJson(string json) => JsonConvert.DeserializeObject<Model>(json, PolyPizza.Converter.Settings);
    }
    
    public partial class SearchResults
    {
        public static SearchResults FromJson(string json) => JsonConvert.DeserializeObject<SearchResults>(json, PolyPizza.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Model self) => JsonConvert.SerializeObject(self, PolyPizza.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
