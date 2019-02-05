using System.Collections.Generic;

namespace CSharpModelsToJson
{
    public class CSharpModelsToJsonOptions
    {
        public List<string> Include { get; set; } = new List<string>();
        public List<string> Exclude { get; set; } = new List<string>();
        public PropertyNameSource PropertyNameSource { get; set; } = PropertyNameSource.Default;
    }

    public enum PropertyNameSource
    {
        Default,
        JsonProperty,
        DataMember
    }
}